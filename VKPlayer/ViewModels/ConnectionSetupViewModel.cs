using System.Security;
using System.Windows;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using VKPlayer.Configuration;
using VKPlayer.Extension;
using VKPlayer.Views;

namespace VKPlayer.ViewModels
{
    public class ConnectionSetupViewModel : BindableBase
    {
        #region Members

        private readonly UserSettings _userSettings;

        private string _username;
        private SecureString _password;
        private string _accessToken;

        private bool _isOpen;

        private ConnectionSetupView _setupView;

        #endregion

        #region Props

        public PasswordBoxExtension PbExt { get; internal set; }

        public string UserName
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public SecureString Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string AccessToken
        {
            get => _accessToken;
            set => SetProperty(ref _accessToken, value);
        }

        /// <summary>
        /// Флаг открытых настроек
        /// </summary>
        public bool IsOpen
        {
            get => _isOpen;
            set => SetProperty(ref _isOpen, value);
        }

        #endregion

        public ConnectionSetupViewModel(UserSettings userSettings)
        {
            _userSettings = userSettings;
            InitCommands();
        }

        /// <summary>
        /// Открыть настройки
        /// </summary>
        public void OpenConnectionSettings()
        {
            InitSettings();
            IsOpen = true;

            _setupView = new ConnectionSetupView
            {
                DataContext = this,
                Owner = Application.Current.MainWindow
            };

            _setupView.ShowDialog();
        }

        /// <summary>
        /// Проинициализировать настройки
        /// </summary>
        private void InitSettings()
        {
            UserName = _userSettings.UserName;
            Password = _userSettings.Password;
            AccessToken = _userSettings.AccessToken;
        }

        #region Commands

        /// <summary>
        /// Инициализация команд
        /// </summary>
        private void InitCommands()
        {
            OkCommand = new DelegateCommand(OkExecute);
            CancelCommand = new DelegateCommand(CancelExecute);
        }

        /// <summary>
        /// Конманда применить настройки
        /// </summary>
        public ICommand OkCommand { get; private set; }

        /// <summary>
        /// Команда отмены настроек
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        private void OkExecute()
        {
            _setupView.MoveFocus(new TraversalRequest(FocusNavigationDirection.Last));

            _userSettings.UserName = UserName;
            _userSettings.Password = PbExt.Password;
            _userSettings.AccessToken = AccessToken;

            _userSettings.Save(isSilent: false);

            IsOpen = false;
        }

        private void CancelExecute()
        {
            IsOpen = false;
        }

        #endregion
    }
}
