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
            Channel c;
            MusicChannelWrapper wrapper;

            // Let's find the channel.
            c = command.Server.FindChannels(command.Parameters[0]).FirstOrDefault();
            if (c == null) return;

            if(_voiceChannels.TryGetValue(command.Server.Id, out wrapper))
            {
                await command.User.SendMessage($"Moving to Voice Channel {c.Name}");
                await wrapper.Channel.Join(c);
                await command.User.SendMessage($"Moved to Voice Channel {c.Name}");
            }
            else
            {
                await command.User.SendMessage($"Joining Voice Channel {c.Name}");
                wrapper = new MusicChannelWrapper(await _audioSevice.Join(c), _audioSevice.Config.Channels);
                wrapper.AutoPlay = true;
                wrapper.Finished += (sender, args) =>
                {
                    if (!wrapper.AutoPlay) return;
                    wrapper.Play(GetNextSong());
                };
                await command.User.SendMessage($"Joining Voice Channel {c.Name}");
                _voiceChannels.TryAdd(command.Server.Id, wrapper);
            }
        }

        [HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, UserPermissions.Moderator, false, "qvc")]
        public async Task QuitVoiceChannelOnServer(IHunieCommand command)
        {
            MusicChannelWrapper wrapper;
            if (_voiceChannels.TryRemove(command.Server.Id, out wrapper))
            {
                await command.User.SendMessage($"Leaving Voice Channel {wrapper.Channel.Channel.Name}");
                wrapper.Dispose();
            }
        }

        [HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, UserPermissions.Moderator, false, "sps")]
        public async Task SetPlaylistSourceDirectory(IHunieCommand command)
        {
            _playlistSource.Clear();
            await command.User.SendMessage($"Populating the internal queue with all MP3 files in directory {command.Parameters[0]}. Depending on the size of the directory, this could take some time...");
            _playlistSource.AddRange(Directory.EnumerateFiles(string.Join(" ", command.Parameters), "*.mp3", SearchOption.AllDirectories));
            await command.User.SendMessage($"Finished! There are {_playlistSource.Count} items in the queue.");
            ResetQueue();
        }

        [HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, UserPermissions.Moderator, false, "reloadqueue")]
        public async Task ReloadPlaylist(IHunieCommand command)
        {
            ResetQueue();
        }

        [HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, UserPermissions.Moderator, false, "playing")]
        public async Task NowPlayingOnServer(IHunieCommand command)
        {
            MusicChannelWrapper wrapper;
            if (_voiceChannels.TryRemove(command.Server.Id, out wrapper))
            {
                await command.User.SendMessage($"Current Song: {wrapper.CurrentlyPlaying}");
            }
        }

        [HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, UserPermissions.Moderator, false, "start")]
        public async Task StartMusic(IHunieCommand command)
        {
            MusicChannelWrapper wrapper;
            if (!_voiceChannels.TryGetValue(command.Server.Id, out wrapper)) return;
            if (wrapper.IsPlaying) return;
            wrapper.Play(GetNextSong());
        }

        [HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, UserPermissions.Moderator, false, "stop")]
        public async Task StopMusic(IHunieCommand command)
        {
            MusicChannelWrapper wrapper;
            if (!_voiceChannels.TryGetValue(command.Server.Id, out wrapper)) return;
            if (!wrapper.IsPlaying) return;
            wrapper.Stop();
        }

        [HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, UserPermissions.Moderator, false, "next")]
        public async Task NextSong(IHunieCommand command)
        {
            MusicChannelWrapper wrapper;
            if (!_voiceChannels.TryGetValue(command.Server.Id, out wrapper)) return;
            if (!wrapper.IsPlaying) return;
            var autoplay = wrapper.AutoPlay;
            wrapper.AutoPlay = false;
            wrapper.Stop();
            wrapper.Play(GetNextSong());
            wrapper.AutoPlay = autoplay;
        }


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