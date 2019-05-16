using System;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;
using Prism.Mvvm;
using VkNet.Model;
using VKPlayer.Helpers;
using VKPlayer.Logging;
using VKPlayer.UiHelpers;

namespace VKPlayer.ViewModels
{
    public class GroupViewModel : BindableBase
    {
        #region Members

        private readonly Group _group;

        private BitmapImage _photo;

        #endregion

        #region Props

        /// <summary>
        /// Id группы
        /// </summary>
        public long Id => _group.Id;

        /// <summary>
        /// Полное имя пользователя
        /// </summary>
        public string Name => _group.Name;

        /// <summary>
        /// Фото
        /// </summary>
        public BitmapImage Photo
        {
            get => _photo;
            set => SetProperty(ref _photo, value);
        }

        #endregion

        public GroupViewModel(Group group)
        {
            _group = group;

            LoadPhoto();
        }

        private async void LoadPhoto()
        {
            if(_group.Photo50 == null)
                return;

            var url = _group.Photo50.AbsoluteUri;
            if (url.Contains("?ava=1"))
            {
                url = url.Replace("?ava=1", "");
            }

            var photo = await ImageHelper.GetBitmapFromUrl(url);

            UiInvoker.Invoke(() =>
            {
                Photo = photo;
            });
        }
    }
}
