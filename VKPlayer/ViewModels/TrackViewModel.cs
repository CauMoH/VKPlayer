using System;
using Prism.Mvvm;
using VkNet.Model.Attachments;

namespace VKPlayer.ViewModels
{
    public class TrackViewModel : BindableBase
    {
        #region Members

        private readonly Audio _audio;

        #endregion

        #region Props

        /// <summary>
        /// Название трека
        /// </summary>
        public string Title => _audio.Title;

        /// <summary>
        /// Исполнитель трека
        /// </summary>
        public string Artist => _audio.Artist;

        /// <summary>
        /// Длина трека
        /// </summary>
        public TimeSpan Duration => TimeSpan.FromSeconds(_audio.Duration);

        /// <summary>
        /// Ссылка на трека
        /// </summary>
        public Uri Uri => _audio.Url;

        #endregion

        public TrackViewModel(Audio audio)
        {
            _audio = audio;
        }
    }
}
