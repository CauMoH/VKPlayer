using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Logging;
using Microsoft.Extensions.DependencyInjection;
using Prism.Commands;
using Prism.Mvvm;
using VkNet;
using VkNet.AudioBypassService.Extensions;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VKPlayer.AppCommon;
using VKPlayer.Configuration;
using VKPlayer.Enums;
using VKPlayer.EventArgs;
using VKPlayer.Extension;
using VKPlayer.Interfaces;
using VKPlayer.Logging;
using VKPlayer.PlayerEngine;
using VKPlayer.Properties;
using VKPlayer.UiHelpers;
using Audio = VkNet.Model.Attachments.Audio;

namespace VKPlayer.ViewModels
{
    public class MainViewModel : BindableBase
    {
        #region Members

        private VkApi _api;

        private readonly IPlayer _player = new PlayerVlcService();

        private Audio _selectedTrack;

        private PlayerState _playerState;

        private string _query;

        private double _totalProgress = 100;

        private double _processingProgress;

        #endregion

        #region Props

        /// <summary>
        /// Пользовательские настройки приложения
        /// </summary>
        public UserSettings UserSettings { get; }

        /// <summary>
        /// Минимальная ширина
        /// </summary>
        public int WindowMinWidth { get; }

        /// <summary>
        /// Минимальная высота
        /// </summary>
        public int WindowMinHeight { get; }

        /// <summary>
        /// Список полученных ошибок
        /// </summary>
        public ObservableCollection<string> ErrorCollection { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Выбранный трек
        /// </summary>
        public Audio SelectedTrack
        {
            get => _selectedTrack;
            set => SetProperty(ref _selectedTrack, value);
        }

        /// <summary>
        /// Вью модель настроек
        /// </summary>
        public ConnectionSetupViewModel ConnectionSetupViewModel { get; set; }

        /// <summary>
        /// Текущий плейлист
        /// </summary>
        public ObservableCollection<Audio> Tracks { get; } = new ObservableCollection<Audio>();

        /// <summary>
        /// Состояние плеера
        /// </summary>
        public PlayerState PlayerState
        {
            get => _playerState;
            set => SetProperty(ref _playerState, value);
        }

        /// <summary>
        /// Статус авторизации
        /// </summary>
        public bool AuthorizationStatus => _api.IsAuthorized;

        /// <summary>
        /// Запрос на поиск
        /// </summary>
        public string Query
        {
            get => _query;
            set => SetProperty(ref _query, value);
        }

        /// <summary>
        /// Общая длина трека
        /// </summary>
        public double TotalProgress
        {
            get => _totalProgress;
            set => SetProperty(ref _totalProgress, value);
        }

        /// <summary>
        /// Текущее положение
        /// </summary>
        public double ProcessingProgress
        {
            get => _processingProgress;
            set => SetProperty(ref _processingProgress, value);
        }

        #endregion

        public MainViewModel()
        {
            LoggerFacade.LogMessageAdded += LoggerFacade_OnLogMessageAdded;
            
            UserSettings = new UserSettings();
            UserSettings.Load();

            WindowMinHeight = Settings.Default.WindowMinHeight;
            WindowMinWidth = Settings.Default.WindowMinWidth;

            UserSettings.Saved += Settings_OnSaved;

            InitApi();
            InitPlayer();
            InitViewModels();

            InitCommands();

            Authorize();
        }

        #region Initialize

        /// <summary>
        /// Инициализация апи
        /// </summary>
        private void InitApi()
        {
            var services = new ServiceCollection();
            services.AddAudioBypass();

            _api = new VkApi(services);
        }

        /// <summary>
        /// Инициализация плеера
        /// </summary>
        private void InitPlayer()
        {
            _player.LengthChanged += Player_OnLengthChanged;
            _player.PlayerStateChanged += Player_OnPlayerStateChanged;
            _player.PositionChanged += Player_OnPositionChanged;
        }

        /// <summary>
        /// Инициализация вью моделей
        /// </summary>
        private void InitViewModels()
        {
            ConnectionSetupViewModel = new ConnectionSetupViewModel(UserSettings);
        }

        #endregion
        
        private void Authorize()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(UserSettings.AccessToken))
                {
                    _api.Authorize(new ApiAuthParams
                    {
                        ApplicationId = AppInfo.VkAppId,
                        AccessToken = UserSettings.AccessToken
                    });
                }
                else
                {
                    _api.Authorize(new ApiAuthParams
                    {
                        ApplicationId = AppInfo.VkAppId,
                        Login = UserSettings.UserName,
                        Password = new System.Net.NetworkCredential(string.Empty, UserSettings.Password).Password
                    });
                }
                
                if (_api.IsAuthorized)
                {
                    UserSettings.AccessToken = _api.Token;
                    UserSettings.Save();

                    //TODO
                    GetMyTracks(100);
                }
            }
            catch (Exception e)
            {

            }

            RaisePropertyChanged(nameof(AuthorizationStatus));
        }

        private void GetMyTracks(int count)
        {
            Tracks.Clear();

            var audios = _api.Audio.Get(new AudioGetParams()
            {
                Count = count
            });

            foreach (var audio in audios)
            {
                Tracks.Add(audio);
            }

            PlayFirst();
        }

        private void PlayFirst()
        {
            SelectedTrack = Tracks.FirstOrDefault();
            Play();
        }

        private void Play()
        {
            _player.Play(SelectedTrack?.Url);
        }
        
        public void SetPosition(float positionInMs)
        {
            _player.SetPosition((float)(positionInMs / TotalProgress));
        }

        #region Others

        public void OnUnload()
        {
            UserSettings.Save();
        }

        public void ExitFromApp()
        {
            App.Exit();
        }

        #endregion

        #region Event Handlers

        #region Settings

        private void Settings_OnSaved(object sender, SettingsSavedEventArgs e)
        {
            if (e.IsChanged(nameof(UserSettings.UserName)) || e.IsChanged(nameof(UserSettings.Password)))
            {
                Authorize();
            }
        }

        #endregion

        #region Logger

        private void LoggerFacade_OnLogMessageAdded(object sender, LogMessageAddedEventArgs e)
        {
            UiInvoker.BeginInvoke(() =>
            {
                try
                {
                    ErrorCollection.Add(e.Message);

                    if (e.IsShow)
                    {
                        MessageBoxExt.Show(Localization.strings.MessageBoxError, e.Message);
                    }
                }
                catch
                {
                    //Ignore
                }

            });
        }

        #endregion

        #region Player

        private void Player_OnPositionChanged(object sender, IPositionChangedEventArgs e)
        {
            ProcessingProgress = e.Position * TotalProgress;
        }

        private void Player_OnPlayerStateChanged(object sender, IPlayerStateChangedEventArgs e)
        {
            PlayerState = e.PlayerState;
        }

        private void Player_OnLengthChanged(object sender, ILengthChangedEventArgs e)
        {
            TotalProgress = e.Length;
        }

        #endregion

        #endregion

        #region Commands

        private void InitCommands()
        {
            ConnectionSettingsCommand = new DelegateCommand(ConnectionSettingsExecute);
            SearchCommand = new DelegateCommand(SearchExecute);
            GetFeedCommand = new DelegateCommand(GetFeedExecute);
            SelectedCommand = new DelegateCommand<Audio>(SelectedExecute);
            PreviousTrackCommand = new DelegateCommand(PreviousTrackExecute);
            NextTrackCommand = new DelegateCommand(NextTrackExecute);
            PlayPauseCommand = new DelegateCommand(PlayPauseExecute);
            StopCommand = new DelegateCommand(StopExecute);
            DownloadCommand = new DelegateCommand(DownloadExecute);
        }

        #region Command Props

        public ICommand ConnectionSettingsCommand { get; private set; }

        public ICommand SearchCommand { get; private set; }

        public ICommand GetFeedCommand { get; private set; }

        public ICommand SelectedCommand { get; private set; }

        public ICommand PreviousTrackCommand { get; private set; }

        public ICommand NextTrackCommand { get; private set; }

        public ICommand PlayPauseCommand { get; private set; }

        public ICommand StopCommand { get; private set; }

        public ICommand DownloadCommand { get; private set; }

        #endregion

        private void ConnectionSettingsExecute()
        {
            ConnectionSetupViewModel.OpenConnectionSettings();
        }

        private void SearchExecute()
        {
            //_apiEngine.SearchTracks(Query, UserSettings.SearchType, UserSettings.SearchPageCount, UserSettings.SearchNococrrect);
        }

        private void GetFeedExecute()
        {
            //_apiEngine.GetFeed();
        }

        private void SelectedExecute(Audio track)
        {
            SelectedTrack = track;
            Play();
        }

        private void PreviousTrackExecute()
        {
            if (SelectedTrack == null)
            {
                PlayFirst();
            }
            else
            {
                if (!Tracks.Contains(SelectedTrack))
                {
                    PlayFirst();
                }
                else
                {
                    if (Tracks.Count == 0)
                    {
                        return;
                    }

                    var indexOf = Tracks.IndexOf(SelectedTrack);
                    var item = indexOf == 0 ? Tracks.Last() : Tracks[indexOf - 1];
                    SelectedTrack = item;
                    Play();
                }
            }
        }

        private void NextTrackExecute()
        {
            if (SelectedTrack == null)
            {
                PlayFirst();
            }
            else
            {
                if (!Tracks.Contains(SelectedTrack))
                {
                    PlayFirst();
                }
                else
                {
                    if (Tracks.Count == 0)
                    {
                        return;
                    }

                    var indexOf = Tracks.IndexOf(SelectedTrack);
                    var item = indexOf + 1 == Tracks.Count ? Tracks.First() : Tracks[indexOf + 1];
                    SelectedTrack = item;
                    Play();
                }
            }
        }

        private void PlayPauseExecute()
        {
            if (PlayerState == PlayerState.Plays)
            {
                _player.Pause(true);
            }
            else if (PlayerState == PlayerState.Paused)
            {
                _player.Pause(false);
            }
            else
            {
                if (SelectedTrack == null)
                {
                    PlayFirst();
                }
                else
                {
                    Play();
                }
            }
        }

        private void StopExecute()
        {
            _player.Stop();
        }

        private async void DownloadExecute()
        {
            
        }

        #endregion
    }
    
}
