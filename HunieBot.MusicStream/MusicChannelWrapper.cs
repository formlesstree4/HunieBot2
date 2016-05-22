using Discord.Audio;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HunieBot.MusicStream
{

    /// <summary>
    ///     A class that wraps around a <see cref="IAudioClient"/> instance to provide streaming services.
    /// </summary>
    public sealed class MusicChannelWrapper : IDisposable
    {
        private bool _errorOccurred = false;
        private bool _isPlaying = false;
        private bool _canPlay = true;
        private bool _disposedValue = false; // To detect redundant calls
        private readonly int _channels;
        private readonly IAudioClient _voiceChannel;



        /// <summary>
        ///     Raised when <see cref="MusicChannelWrapper"/> has finished playing a song.
        /// </summary>
        public EventHandler Finished;

        /// <summary>
        ///     Raised when <see cref="MusicChannelWrapper"/> crashes and is unable to continue playing.
        /// </summary>
        public EventHandler Crashed;



        /// <summary>
        ///     Gets the <see cref="IAudioClient"/> that either is having music streamed to it or will be streaming.
        /// </summary>
        public IAudioClient Channel => _voiceChannel;

        /// <summary>
        ///     Indicates whether or not <see cref="MusicChannelWrapper"/> is currently playing.
        /// </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        ///     Gets or sets whether or not this <see cref="MusicChannelWrapper"/> supports automatically playing songs.
        /// </summary>
        /// <remarks>
        ///     this is a specific interation between <see cref="MusicChannelWrapper"/> and <see cref="MusicStream"/>. If true, <see cref="MusicStream"/> will pop off another, random song to play.
        /// </remarks>
        public bool AutoPlay { get; set; }

        /// <summary>
        ///     Gets the name of the file that is currently playing.
        /// </summary>
        public string CurrentlyPlaying { get; private set; }



        /// <summary>
        ///     Creates a new instance of the <see cref="MusicChannelWrapper"/>.
        /// </summary>
        /// <param name="client"><see cref="IAudioClient"/></param>
        /// <param name="audioChannels">The supported number of channels</param>
        public MusicChannelWrapper(IAudioClient client, int audioChannels)
        {
            _voiceChannel = client;
            _channels = audioChannels;
        }

        /// <summary>
        ///     Plays a file from the file system.
        /// </summary>
        /// <param name="file">The file to play</param>
        /// <remarks>This operation will execute on a different thread than the calling thread. </remarks>
        /// <exception cref="InvalidOperationException" />
        public void Play(string file)
        {
            if (_errorOccurred) throw new InvalidOperationException();
            _canPlay = true;
            CurrentlyPlaying = Path.GetFileNameWithoutExtension(file);
            Task.Run(() =>
            {
                var OutFormat = new WaveFormat(48000, 16, _channels);
                using (var fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var MP3Reader = new Mp3FileReader(fileStream))
                using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat))
                {
                    resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                    int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                    byte[] buffer = new byte[blockSize];
                    int byteCount;
                    _isPlaying = true;
                    while (((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) & _canPlay) // Read audio into our buffer, and keep a loop open while data is present
                    {
                        if (byteCount < blockSize)
                        {
                            // Incomplete Frame
                            for (int i = byteCount; i < blockSize; i++)
                                buffer[i] = 0;
                        }
                        try
                        {
                            _voiceChannel.Send(buffer, 0, blockSize);
                        }
                        catch (OperationCanceledException)
                        {
                            _canPlay = false;
                            _errorOccurred = true;
                            Crashed?.Invoke(this, EventArgs.Empty);
                        }
                    }
                    if(_canPlay) Finished?.Invoke(this, EventArgs.Empty);
                    _isPlaying = false;
                }
            });
        }

        /// <summary>
        ///     Stops the streaming.
        /// </summary>
        public void Stop()
        {
            _canPlay = false;
        }

        #region IDisposable Support

        void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    try
                    {
                        _voiceChannel.Channel.LeaveAudio();
                    }
                    catch
                    {
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MusicChannelWrapper() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}