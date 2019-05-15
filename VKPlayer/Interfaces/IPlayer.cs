using System;
using VKPlayer.Enums;

namespace VKPlayer.Interfaces
{
    public interface IPlayer
    {
        long Length { get; }
        float Position { get; }
        PlayerState PlayerState { get; }

        void Play(Uri uri);
        void Stop();
        void Pause(bool doPause);
        void SetPosition(float position);

        event EventHandler<IPlayerStateChangedEventArgs> PlayerStateChanged;
        event EventHandler<IPositionChangedEventArgs> PositionChanged;
        event EventHandler<ILengthChangedEventArgs> LengthChanged;
    }

    public interface IPlayerStateChangedEventArgs
    {
        PlayerState PlayerState { get; }
    }

    public interface IPositionChangedEventArgs
    {
        float Position { get; }
    }

    public interface ILengthChangedEventArgs
    {
        long Length { get; }
    }
}
