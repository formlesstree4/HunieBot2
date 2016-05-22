using Discord;
using Discord.Audio;
using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExtractor;

namespace HunieBot.MusicStream
{

    [HunieBot("Music Streaming Service")]
    public sealed class MusicStream
    {
        private readonly Random _shuffle;
        private readonly AudioService _audioSevice;
        private readonly ConcurrentDictionary<ulong, MusicChannelWrapper> _voiceChannels;
        private List<string> _playlistSource;
        private ConcurrentQueue<string> _playlist;


        public MusicStream(DiscordClient discordClient)
        {
            discordClient.UsingAudio(x =>
            {
                x.Mode = AudioMode.Outgoing;
            });
            _audioSevice = discordClient.GetService<AudioService>();
            _voiceChannels = new ConcurrentDictionary<ulong, MusicChannelWrapper>();
            _playlistSource = new List<string>();
            _playlist = new ConcurrentQueue<string>();
            _shuffle = new SecureRandom();
        }


        [HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, UserPermissions.Moderator, false, "jvc")]
        public async Task JoinVoiceChannelOnServer(IHunieCommand command)
        {
            if (command.User.VoiceChannel == null) return;
            MusicChannelWrapper mcw;
            if(_voiceChannels.TryGetValue(command.Server.Id, out mcw))
            {
                await mcw.Channel.Join(command.User.VoiceChannel);
            }
            else
            {
                mcw = new MusicChannelWrapper(await _audioSevice.Join(command.User.VoiceChannel), _audioSevice.Config.Channels);
                mcw.AutoPlay = true;
                mcw.Crashed += (sender, args) =>
                {
                    // Time to bail out of this.
                    MusicChannelWrapper lcl;
                    _voiceChannels.TryRemove(mcw.Channel.Server.Id, out lcl);
                    lcl.Dispose();
                };
                mcw.Finished += (sender, args) =>
                {
                    var wrapper = sender as MusicChannelWrapper;
                    if (!wrapper.AutoPlay) return;
                    wrapper.Play(GetNextSong());
                };
                _voiceChannels.TryAdd(command.Server.Id, mcw);
            }
        }

        [HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, UserPermissions.Moderator, false, "qvc")]
        public async Task LeaveVoiceChannelOnServer(IHunieCommand command)
        {
            MusicChannelWrapper wrapper;
            if (_voiceChannels.TryRemove(command.Server.Id, out wrapper))
            {
                wrapper.Dispose();
            }
        }

        [HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, UserPermissions.Moderator, false, "yt")]
        public async Task PlayYouTubeVideo(IHunieCommand command)
        {
            // the first parameter should be the yt video to download.
            if (command.Parameters.Length == 0) return;
            string link;

            if (!DownloadUrlResolver.TryNormalizeYoutubeUrl(command.Parameters[0], out link)) return;
            var videoInfo = DownloadUrlResolver.GetDownloadUrls(link);
            var bestAudio = videoInfo
                .Where(info => info.CanExtractAudio)
                .OrderByDescending(info => info.AudioBitrate)
                .FirstOrDefault();
            if (bestAudio == null) return;
            if (bestAudio.RequiresDecryption) DownloadUrlResolver.DecryptDownloadUrl(bestAudio);
            using (var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset))
            {
                var downloader = new AudioDownloader(bestAudio, "");



            }
            

        }


        //[HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, UserPermissions.Moderator, false, "sps")]
        //public async Task SetPlaylistSourceDirectory(IHunieCommand command)
        //{
        //    _playlistSource.Clear();
        //    await command.User.SendMessage($"Populating the internal queue with all MP3 files in directory {command.Parameters[0]}. Depending on the size of the directory, this could take some time...");
        //    _playlistSource.AddRange(Directory.EnumerateFiles(string.Join(" ", command.Parameters), "*.mp3", SearchOption.AllDirectories));
        //    await command.User.SendMessage($"Finished! There are {_playlistSource.Count} items in the queue.");
        //    ResetQueue();
        //}

        //[HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, UserPermissions.Moderator, false, "reloadqueue")]
        //public async Task ReloadPlaylist(IHunieCommand command)
        //{
        //    ResetQueue();
        //}

        //[HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, UserPermissions.Moderator, false, "playing")]
        //public async Task NowPlayingOnServer(IHunieCommand command)
        //{
        //    MusicChannelWrapper wrapper;
        //    if (_voiceChannels.TryRemove(command.Server.Id, out wrapper))
        //    {
        //        await command.User.SendMessage($"Current Song: {wrapper.CurrentlyPlaying}");
        //    }
        //}

        //[HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, UserPermissions.Moderator, false, "start")]
        //public async Task StartMusic(IHunieCommand command)
        //{
        //    MusicChannelWrapper wrapper;
        //    if (!_voiceChannels.TryGetValue(command.Server.Id, out wrapper)) return;
        //    if (wrapper.IsPlaying) return;
        //    wrapper.Play(GetNextSong());
        //}

        //[HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, UserPermissions.Moderator, false, "stop")]
        //public async Task StopMusic(IHunieCommand command)
        //{
        //    MusicChannelWrapper wrapper;
        //    if (!_voiceChannels.TryGetValue(command.Server.Id, out wrapper)) return;
        //    if (!wrapper.IsPlaying) return;
        //    wrapper.Stop();
        //}

        //[HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, UserPermissions.Moderator, false, "next")]
        //public async Task NextSong(IHunieCommand command)
        //{
        //    MusicChannelWrapper wrapper;
        //    if (!_voiceChannels.TryGetValue(command.Server.Id, out wrapper)) return;
        //    if (!wrapper.IsPlaying) return;
        //    var autoplay = wrapper.AutoPlay;
        //    wrapper.AutoPlay = false;
        //    wrapper.Stop();
        //    wrapper.Play(GetNextSong());
        //    wrapper.AutoPlay = autoplay;
        //}


        private void ResetQueue()
        {
            _playlistSource.Shuffle(_shuffle);
            _playlist = new ConcurrentQueue<string>(_playlistSource);
        }
        private string GetNextSong()
        {
            string next;
            lock (_playlist)
            {
                if(_playlist.Count == 0)
                {
                    ResetQueue();
                }
                while (!_playlist.TryDequeue(out next)) ;
            }
            return next;
        }
        private string PeekNextInQueue()
        {
            string next;
            lock (_playlist)
            {
                if (_playlist.Count == 0)
                {
                    ResetQueue();
                }
                while (!_playlist.TryPeek(out next)) ;
            }
            return next;
        }

    }

    internal static class Extensions
    {
        public static void Shuffle<T>(this IList<T> list, Random rand)
        {
            // Random will 99% of the time be thread-safe. So fuckin' deal with it.
            // Note: I'm not saying the TYPE is thread-safe, but, the instance I am
            // passing in is a thread-safe subclass of Random so *I* know for sure
            // I don't need to take a lock out on the random object.
            int n = list.Count;
            for (int i = 0; i < n; i++)
            {
                // NextDouble returns a random number between 0 and 1.
                int r = i + (int)(rand.NextDouble() * (n - i));
                T t = list[r];
                list[r] = list[i];
                list[i] = t;
            }
        }
    }
}