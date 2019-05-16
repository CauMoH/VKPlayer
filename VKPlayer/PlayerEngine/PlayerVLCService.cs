using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using VKPlayer.Enums;
using VKPlayer.Interfaces;
using VKPlayer.Logging;
using Vlc.DotNet.Core;

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

            try
            {
                if (currentAssembly != null)
                {
                    var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
                    if (currentDirectory == null)
                        return;

                    var vlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @"lib\x86"));

                    _vlcMediaPlayer = new VlcMediaPlayer(vlcLibDirectory);
                }
                else
                {
                    throw new FileNotFoundException(Localization.strings.PlayerEngineNotFound);
                }
            }
            catch (Exception e)
            {
                LoggerFacade.WriteError(Localization.strings.PlayerNotLoaded, e, isShow: true);
            }
            
            _vlcMediaPlayer.EndReached += VlcMediaPlayer_OnEndReached;
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

        private void VlcMediaPlayer_OnEndReached(object sender, VlcMediaPlayerEndReachedEventArgs e)
        {
            EndReached?.Invoke(this, new EndReachedEventArgs());
        }

        #endregion

        public void Play(Uri playPathUri)
        {
            try
            {
                _vlcMediaPlayer.Play(playPathUri.AbsoluteUri);
            }
            catch (Exception e)
            {
                LoggerFacade.WriteError(e);
            }
        }

        public void Stop()
        {
            try
            {
                _vlcMediaPlayer.Stop();
            }
            catch (Exception e)
            {
                LoggerFacade.WriteError(e);
            }
        }

        public void Pause(bool doPause)
        {
            try
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
            catch (Exception e)
            {
                LoggerFacade.WriteError(e);
            }
        }

        public void SetPosition(float position)
        {
            try
            {
                _vlcMediaPlayer.Position = position;
            }
            catch (Exception e)
            {
                LoggerFacade.WriteError(e);
            }
        }

        #region Events

        public event EventHandler<IPlayerStateChangedEventArgs> PlayerStateChanged;
        public event EventHandler<IPositionChangedEventArgs> PositionChanged;
        public event EventHandler<ILengthChangedEventArgs> LengthChanged;
        public event EventHandler<IEndReachedEventArgs> EndReached;

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

        private sealed class EndReachedEventArgs : IEndReachedEventArgs
        {
            
        }

    }
}
