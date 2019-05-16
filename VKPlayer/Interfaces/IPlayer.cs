using System;
using VKPlayer.Enums;

namespace VKPlayer.Interfaces
{
    /// <summary>
    /// Интерфейс движка плеера
    /// </summary>
    public interface IPlayer
    {
        /// <summary>
        /// Длина трека
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Позиция в треке
        /// </summary>
        float Position { get; }

        /// <summary>
        /// Состояние плеера
        /// </summary>
        PlayerState PlayerState { get; }

        /// <summary>
        /// Играть
        /// </summary>
        /// <param name="uri">Адрес стрима</param>
        void Play(Uri uri);

        /// <summary>
        /// Остановка
        /// </summary>
        void Stop();

        /// <summary>
        /// Пауза
        /// </summary>
        /// <param name="doPause">Вкл/Выкл паузу</param>
        void Pause(bool doPause);

        /// <summary>
        /// Установить позицию в треке
        /// </summary>
        /// <param name="position">Позиция</param>
        void SetPosition(float position);

        /// <summary>
        /// Событие - изменилось состояние плеера
        /// </summary>
        event EventHandler<IPlayerStateChangedEventArgs> PlayerStateChanged;

        /// <summary>
        /// Событие - изменилось позиция в треке
        /// </summary>
        event EventHandler<IPositionChangedEventArgs> PositionChanged;

        /// <summary>
        /// Событие - изменилась длина трека
        /// </summary>
        event EventHandler<ILengthChangedEventArgs> LengthChanged;

        /// <summary>
        /// Событие - конец достигнут
        /// </summary>
        event EventHandler<IEndReachedEventArgs> EndReached;
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

    public interface IEndReachedEventArgs
    {

    }
}
