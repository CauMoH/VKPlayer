using System.Windows.Media.Imaging;
using Prism.Mvvm;
using VkNet.Model;
using VKPlayer.Helpers;
using VKPlayer.UiHelpers;

namespace VKPlayer.ViewModels
{
    public class FriendViewModel : BindableBase
    {
        #region Members

        private readonly User _user;

        private BitmapImage _photo;

        #endregion

        #region Props

        /// <summary>
        /// Id пользователя
        /// </summary>
        public long Id => _user.Id;

        /// <summary>
        /// Полное имя пользователя
        /// </summary>
        public string FullName => string.Join(" ", _user.FirstName, _user.LastName);

        /// <summary>
        /// Фото
        /// </summary>
        public BitmapImage Photo
        {
            get => _photo;
            set => SetProperty(ref _photo, value);
        }

        #endregion

        public FriendViewModel(User user)
        {
            _user = user;
            
            LoadPhoto();
        }

        private async void LoadPhoto()
        {
            if(_user.Photo50 == null)
                return;

            var url = _user.Photo50.AbsoluteUri;
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
