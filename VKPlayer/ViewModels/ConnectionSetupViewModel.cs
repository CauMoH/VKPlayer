using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Windows.Forms;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using VKPlayer.Configuration;
using VKPlayer.Data;
using VKPlayer.Enums;
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
        private Genre _selectedAudioGenre;
        private bool _popularOnlyEng;
        private bool _recommendationsIsShuffle;

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

        public Genre SelectedAudioGenre
        {
            get => _selectedAudioGenre;
            set => SetProperty(ref _selectedAudioGenre, value);
        }

        public bool PopularOnlyEng
        {
            get => _popularOnlyEng;
            set => SetProperty(ref _popularOnlyEng, value);
        }

        public bool RecommendationsIsShuffle
        {
            get => _recommendationsIsShuffle;
            set => SetProperty(ref _recommendationsIsShuffle, value);
        }

        public ObservableCollection<Genre> Genres { get; } = new ObservableCollection<Genre>
        {
            new Genre(AudioGenreExt.All, Localization.genre.All),
            new Genre(AudioGenreExt.Rock, Localization.genre.Rock),
            new Genre(AudioGenreExt.Pop, Localization.genre.Pop),
            new Genre(AudioGenreExt.RapAndHipHop, Localization.genre.RapAndHipHop),
            new Genre(AudioGenreExt.EasyListening, Localization.genre.EasyListening),
            new Genre(AudioGenreExt.DanceAndHouse, Localization.genre.DanceAndHouse),
            new Genre(AudioGenreExt.Instrumental, Localization.genre.Instrumental),
            new Genre(AudioGenreExt.Metal, Localization.genre.Metal),
            new Genre(AudioGenreExt.Dubstep, Localization.genre.Dubstep),
            new Genre(AudioGenreExt.DrumAndBass, Localization.genre.DrumAndBass),
            new Genre(AudioGenreExt.Trance, Localization.genre.Trance),
            new Genre(AudioGenreExt.Chanson, Localization.genre.Chanson),
            new Genre(AudioGenreExt.Ethnic, Localization.genre.Ethnic),
            new Genre(AudioGenreExt.AcousticAndVocal, Localization.genre.AcousticAndVocal),
            new Genre(AudioGenreExt.Reggae, Localization.genre.Reggae),
            new Genre(AudioGenreExt.Classical, Localization.genre.Classical),
            new Genre(AudioGenreExt.IndiePop, Localization.genre.IndiePop),
            new Genre(AudioGenreExt.Other, Localization.genre.Other),
            new Genre(AudioGenreExt.Speech, Localization.genre.Speech),
            new Genre(AudioGenreExt.Alternative, Localization.genre.Alternative),
            new Genre(AudioGenreExt.ElectropopAndDisco, Localization.genre.ElectropopAndDisco),
            new Genre(AudioGenreExt.JazzAndBlues, Localization.genre.JazzAndBlues)
        };

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
            SelectedAudioGenre = Genres.FirstOrDefault(genre => genre.AudioGenreExt == _userSettings.PopularAudioGenre);
            PopularOnlyEng = _userSettings.PopularOnlyEng;
            RecommendationsIsShuffle = _userSettings.RecommendationsIsShuffle;
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
            _userSettings.PopularAudioGenre = SelectedAudioGenre.AudioGenreExt;
            _userSettings.PopularOnlyEng = PopularOnlyEng;
            _userSettings.RecommendationsIsShuffle = RecommendationsIsShuffle;
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
