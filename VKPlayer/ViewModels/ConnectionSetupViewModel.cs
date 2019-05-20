using System.Security;
using System.Windows.Forms;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using VKPlayer.Configuration;
using VKPlayer.Extension;
using VKPlayer.Views;
using Application = System.Windows.Application;

namespace VKPlayer.ViewModels
{
    public class ConnectionSetupViewModel : BindableBase
    {
        #region Members

        private ConnectionSetupView _setupView;

        private readonly UserSettings _userSettings;

        private string _username;
        private SecureString _password;

        private bool _isOpen;

        private string _downloadFolder;

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

        /// <summary>
        /// Флаг открытых настроек
        /// </summary>
        public bool IsOpen
        {
            get => _isOpen;
            set => SetProperty(ref _isOpen, value);
        }

        /// <summary>
        /// Папка загрузки
        /// </summary>
        public string DownloadFolder
        {
            get => _downloadFolder;
            set => SetProperty(ref _downloadFolder, value);
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

            _setupView.Show();
        }

        /// <summary>
        /// Проинициализировать настройки
        /// </summary>
        private void InitSettings()
        {
            UserName = _userSettings.UserName;
            Password = _userSettings.Password;
            DownloadFolder = _userSettings.DownloadFolder;
        }

        #region Commands

        /// <summary>
        /// Инициализация команд
        /// </summary>
        private void InitCommands()
        {
            OkCommand = new DelegateCommand(OkExecute);
            CancelCommand = new DelegateCommand(CancelExecute);
            LoginCommand = new DelegateCommand(LoginExecute);
            DownloadFolderSelectCommand = new DelegateCommand(DownloadFolderSelectExecute);
        }

        #region Command Props

        /// <summary>
        /// Конманда применить настройки
        /// </summary>
        public ICommand OkCommand { get; private set; }

        /// <summary>
        /// Команда отмены настроек
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        /// <summary>
        /// Войти в учетную запись
        /// </summary>
        public ICommand LoginCommand { get; private set; }

        /// <summary>
        /// Выбрать папку загрузки
        /// </summary>
        public ICommand DownloadFolderSelectCommand { get; private set; }

        #endregion

        #region Command Executes

        #endregion

        private void OkExecute()
        {
            IsOpen = false;

            _userSettings.UserName = UserName;
            _userSettings.Password = PbExt.Password;
            _userSettings.DownloadFolder = DownloadFolder;
            _userSettings.Save(isSilent: false);
        }

        private void CancelExecute()
        {
            IsOpen = false;
        }

        private void LoginExecute()
        {
            _userSettings.AccessToken = string.Empty;

            OkCommand.Execute(null);
        }

        private void DownloadFolderSelectExecute()
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                DownloadFolder = dialog.SelectedPath;
            }
        }

        #endregion
    }
}
