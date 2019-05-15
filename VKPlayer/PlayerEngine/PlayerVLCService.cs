using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using VKPlayer.Enums;
using VKPlayer.Interfaces;
using Vlc.DotNet.Core;
using Vlc.DotNet.Core.Interops;

namespace VKPlayer.PlayerEngine
{
    public class PlayerVlcService : IPlayer
    {
        #region Members

        private readonly VlcMediaPlayer _vlcMediaPlayer;

        #endregion

        #region Props
        
        public long Length { get; private set; }
        public float Position { get; private set; }
        public PlayerState PlayerState { get; private set; } = PlayerState.Stopped;

        #endregion

        public PlayerVlcService()
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            if (currentDirectory == null)
                return;

            var vlcLibDirectory = IntPtr.Size == 4 ? new DirectoryInfo(Path.Combine(currentDirectory, @"lib\x86")) : new DirectoryInfo(Path.Combine(currentDirectory, @"lib\x64"));

            _vlcMediaPlayer = new VlcMediaPlayer(vlcLibDirectory);
            _vlcMediaPlayer.PositionChanged += VlcMediaPlayer_OnPositionChanged;
            _vlcMediaPlayer.LengthChanged += VlcMediaPlayer_OnLengthChanged;
            _vlcMediaPlayer.Stopped += VlcMediaPlayer_OnStopped;
            _vlcMediaPlayer.Paused += VlcMediaPlayer_OnPaused;
            _vlcMediaPlayer.Playing += VlcMediaPlayer_OnPlaying;
        }

        #region Event Handlers

        private void VlcMediaPlayer_OnLengthChanged(object sender, VlcMediaPlayerLengthChangedEventArgs e)
        {
            Length = e.NewLength;
            LengthChanged?.Invoke(this, new LengthChangedEventArgs
            {
                Length = Length
            });
        }

        private void VlcMediaPlayer_OnPositionChanged(object sender, VlcMediaPlayerPositionChangedEventArgs e)
        {
            Position = e.NewPosition;
            PositionChanged?.Invoke(this, new PositionChangedEventArgs
            {
                Position = Position
            });
        }

        private void VlcMediaPlayer_OnPlaying(object sender, VlcMediaPlayerPlayingEventArgs e)
        {
            PlayerState = PlayerState.Plays;
            PlayerStateChanged?.Invoke(this, new PlayerStateChangedEventArgs
            {
                PlayerState = PlayerState
            });
        }

        private void VlcMediaPlayer_OnPaused(object sender, VlcMediaPlayerPausedEventArgs e)
        {
            PlayerState = PlayerState.Paused;
            PlayerStateChanged?.Invoke(this, new PlayerStateChangedEventArgs
            {
                PlayerState = PlayerState
            });
        }

        private void VlcMediaPlayer_OnStopped(object sender, VlcMediaPlayerStoppedEventArgs e)
        {
            PlayerState = PlayerState.Stopped;
            PlayerStateChanged?.Invoke(this, new PlayerStateChangedEventArgs
            {
                PlayerState = PlayerState
            });
        }

        #endregion

        public void Play(Uri playPathUri)
        {
            Task.Run(() =>
            {
                _vlcMediaPlayer.Play(playPathUri.AbsoluteUri);
            });
        }

        public void Stop()
        {
            _vlcMediaPlayer.Stop();
        }

        public void Pause(bool doPause)
        {
            if (doPause)
            {
                _vlcMediaPlayer.Pause();
            }
            else
            {
                _vlcMediaPlayer.Play();
            }
        }

        public void SetPosition(float position)
        {
            _vlcMediaPlayer.Position = position;
        }

        #region Events

        public event EventHandler<IPlayerStateChangedEventArgs> PlayerStateChanged;
        public event EventHandler<IPositionChangedEventArgs> PositionChanged;
        public event EventHandler<ILengthChangedEventArgs> LengthChanged;

        #endregion

        private sealed class PlayerStateChangedEventArgs : IPlayerStateChangedEventArgs
        {
            public PlayerState PlayerState { get; set; }
        }

        private sealed class PositionChangedEventArgs : IPositionChangedEventArgs
        {
            public float Position { get; set; }
        }

        private sealed class LengthChangedEventArgs : ILengthChangedEventArgs
        {
            public long Length { get; set; }
        }
    }
}
