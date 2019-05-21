using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Logging;
using Microsoft.Extensions.DependencyInjection;
using Prism.Commands;
using Prism.Mvvm;
using VkNet;
using VkNet.AudioBypassService.Extensions;
using VkNet.Enums;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VKPlayer.AppCommon;
using VKPlayer.Configuration;
using VKPlayer.Enums;
using VKPlayer.EventArgs;
using VKPlayer.Extension;
using VKPlayer.Helpers;
using VKPlayer.Interfaces;
using VKPlayer.Logging;
using VKPlayer.PlayerEngine;
using VKPlayer.UiHelpers;
using Audio = VkNet.Model.Attachments.Audio;
using Settings = VKPlayer.Properties.Settings;
using Timer = System.Timers.Timer;

namespace VKPlayer.ViewModels
{
    public class MainViewModel : BindableBase
    {
        #region Members

        private uint _userId;

        private VkApi _api;

        private readonly IPlayer _player = new PlayerVlcService();

        private TrackViewModel _selectedTrack;

        private PlayerState _playerState = PlayerState.Stopped;

        private string _query = string.Empty;

        private double _totalProgress = 100;

        private double _processingProgress;

        private uint _offset;

        private readonly Timer _updateIsAuthorizedTimer = new Timer(1000);

        private FriendViewModel _selectedFriend;

        private GroupViewModel _selectedGroup;

        private PlaylistType _playlistType = PlaylistType.None;

        private string _duration;

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
        public TrackViewModel SelectedTrack
        {
            get => _selectedTrack;
            set => SetProperty(ref _selectedTrack, value);
        }

        /// <summary>
        /// Вью модель настроек
        /// </summary>
        public ConnectionSetupViewModel ConnectionSetupViewModel { get; set; }

        /// <summary>
        /// Вью модель ввода кода двухфакторной авторизации
        /// </summary>
        public TwoFactorAuthorizationViewModel TwoFactorAuthorizationViewModel { get; set; }

        /// <summary>
        /// Вью модель ввода каптчи
        /// </summary>
        public CaptchaViewModel CaptchaViewModel { get; set; }

        /// <summary>
        /// Текущий плейлист
        /// </summary>
        public ObservableCollection<TrackViewModel> Tracks { get; } = new ObservableCollection<TrackViewModel>();

        /// <summary>
        /// Состояние плеера
        /// </summary>
        public PlayerState PlayerState
        {
            get => _playerState;
            set => SetProperty(ref _playerState, value);
        }

        /// <summary>
        /// Тип плэйлиста
        /// </summary>
        public PlaylistType PlaylistType
        {
            get => _playlistType;
            set
            {
                if (value != _playlistType)
                {
                    _playlistType = value;
                    RaisePropertyChanged(nameof(PlaylistType));

                    if (_playlistType != PlaylistType.Search)
                    {
                        Query = string.Empty;
                    }

                    if (_playlistType != PlaylistType.UserOrGroup)
                    {
                        SelectedFriend = null;
                        SelectedGroup = null;
                    }
                }
            }
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

        /// <summary>
        /// Список друзей пользователя
        /// </summary>
        public ObservableCollection<FriendViewModel> Friends { get; } = new ObservableCollection<FriendViewModel>();

        /// <summary>
        /// Список групп пользователя
        /// </summary>
        public ObservableCollection<GroupViewModel> Groups { get; } = new ObservableCollection<GroupViewModel>();

        /// <summary>
        /// Выбранный плейлист друга
        /// </summary>
        public FriendViewModel SelectedFriend
        {
            get => _selectedFriend;
            set
            {
                if (value != _selectedFriend)
                {
                    _selectedFriend = value;
                    RaisePropertyChanged(nameof(SelectedFriend));

                    if (_selectedFriend != null)
                    {
                        GetUserOrGroupMusic(_selectedFriend.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Выбранный плейлист группы
        /// </summary>
        public GroupViewModel SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (value != _selectedGroup)
                {
                    _selectedGroup = value;
                    RaisePropertyChanged(nameof(SelectedGroup));

                    if (_selectedGroup != null)
                    {
                        GetUserOrGroupMusic(-_selectedGroup.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Время трека
        /// </summary>
        public string Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
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

            _updateIsAuthorizedTimer.Elapsed += UpdateIsAuthorizedTimer_OnElapsed;
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
            _player.EndReached += Player_OnEndReached;
        }
        
        /// <summary>
        /// Инициализация вью моделей
        /// </summary>
        private void InitViewModels()
        {
            ConnectionSetupViewModel = new ConnectionSetupViewModel(UserSettings);
            TwoFactorAuthorizationViewModel = new TwoFactorAuthorizationViewModel();
            CaptchaViewModel = new CaptchaViewModel();
            CaptchaViewModel.CaptchaEntered += CaptchaViewModel_OnCaptchaEntered;
        }
        
        #endregion

        #region Authorize

        /// <summary>
        /// Авторизация по токену
        /// </summary>
        public void AuthorizeFromAccessToken(long? captchaSid = null, string captchaKey = null)
        {
            if (string.IsNullOrWhiteSpace(UserSettings.AccessToken))
                return;

            try
            {
                _api.Authorize(new ApiAuthParams
                {
                    ApplicationId = AppInfo.VkAppId,
                    AccessToken = UserSettings.AccessToken,
                    CaptchaSid = captchaSid,
                    CaptchaKey = captchaKey,
                    Settings = VkNet.Enums.Filters.Settings.All
                });
            }
            catch (CaptchaNeededException e)
            {
                CaptchaViewModel.Open(e.Sid, e.Img);
                return;
            }
            catch (Exception e)
            {
                LoggerFacade.WriteError(Localization.strings.AuthorizeError, e, isShow: true);
                ConnectionSettingsCommand.Execute(null);
            }
          
            RaisePropertyChanged(nameof(AuthorizationStatus));

            if (AuthorizationStatus)
            {
                _updateIsAuthorizedTimer.Start();
                OnLoad();
            }
            else
            {
                _updateIsAuthorizedTimer.Stop();
            }
        }

        /// <summary>
        /// Авторизация по логину и паролю
        /// </summary>
        public void AuthorizeFromLogPass(long? captchaSid = null, string captchaKey = null)
        {
            if (string.IsNullOrWhiteSpace(UserSettings.UserName) || UserSettings.Password.Length == 0)
                return;

            TwoFactorAuthorizationViewModel.Open();

            Task.Run(() =>
                {
                    try
                    {
                        _api.Authorize(new ApiAuthParams
                        {
                            ApplicationId = AppInfo.VkAppId,
                            Login = UserSettings.UserName,
                            Password = new System.Net.NetworkCredential(string.Empty, UserSettings.Password).Password,
                            TwoFactorAuthorization = () =>
                            {
                                var code = TwoFactorAuthorizationViewModel.GetCode();
                                return code;
                            },
                            CaptchaSid = captchaSid,
                            CaptchaKey = captchaKey,
                            Settings = VkNet.Enums.Filters.Settings.All
                        });

                        if (_api.IsAuthorized)
                        {
                            UserSettings.AccessToken = _api.Token;
                            if (_api.UserId != null) UserSettings.UserId = (uint)_api.UserId;
                            UserSettings.Save();
                        }
                    }
                    catch (CaptchaNeededException e)
                    {
                        UiInvoker.Invoke(() =>
                        {
                            CaptchaViewModel.Open(e.Sid, e.Img);
                        });
                    }
                    catch (VkApiAuthorizationException e)
                    {
                        LoggerFacade.WriteError(Localization.strings.AuthorizeError + Environment.NewLine + e.Message, isShow: true);

                        UiInvoker.Invoke(() =>
                        {

                            ConnectionSettingsCommand.Execute(null);
                        });
                    }
                    finally
                    {
                        UiInvoker.Invoke(() =>
                        {
                            TwoFactorAuthorizationViewModel.IsOpen = false;

                            RaisePropertyChanged(nameof(AuthorizationStatus));

                            if (AuthorizationStatus)
                            {
                                _updateIsAuthorizedTimer.Start();
                                OnLoad();
                            }
                            else
                            {
                                _updateIsAuthorizedTimer.Stop();
                            }
                        });
                    }
                });
        }

        #endregion

        /// <summary>
        /// Очистка параметров
        /// </summary>
        private void Clear()
        {
            StopCommand.Execute(null);

            Tracks.Clear();
            Friends.Clear();
            Groups.Clear();
            SelectedFriend = null;
            SelectedGroup = null;
            
            _offset = 0;
        }

        /// <summary>
        /// Играть первый трек в списке
        /// </summary>
        private void PlayFirst()
        {
            SelectedTrack = Tracks.FirstOrDefault();
            Play();
        }

        /// <summary>
        /// Играть выбранный трек
        /// </summary>
        private void Play()
        {
            _player.Play(SelectedTrack?.Uri);
        }
        
        /// <summary>
        /// Установить позицию в треке
        /// </summary>
        /// <param name="positionInMs"></param>
        public void SetPosition(float positionInMs)
        {
            _player.SetPosition((float)(positionInMs / TotalProgress));
        }

        /// <summary>
        /// Очистить список треков
        /// </summary>
        private void ClearTracks()
        {
            TracksCleared?.Invoke(this, System.EventArgs.Empty);

            Tracks.Clear();
            _offset = 0;
        }

        /// <summary>
        /// Расчет строки времени трека
        /// </summary>
        private void CalculateDuration()
        {
            var totalTimeSpan = TimeSpan.FromMilliseconds(TotalProgress);
            var progressTimeSpan = TimeSpan.FromMilliseconds(ProcessingProgress);

            var progressString = progressTimeSpan.ToString(@"mm\:ss");
            var totalString = totalTimeSpan.ToString(@"mm\:ss");

            Duration = progressString + @"\" + totalString;
        }

        /// <summary>
        /// Загружает данные
        /// </summary>
        private async Task<bool> Download(string cmdParams)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var process = new Process();
                    var startInfo = new ProcessStartInfo
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = "cmd.exe",
                        Arguments = "/c " + cmdParams,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                    process.StartInfo = startInfo;
                    process.OutputDataReceived += OutputHandler;
                    process.ErrorDataReceived += OutputHandler;
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                    return true;
                }
                catch (Exception e)
                {
                    LoggerFacade.WriteError(e);
                    return false;
                }
            });
        }

        /// <summary>
        /// Создает параметры командной строки из данных трека
        /// </summary>
        private bool CheckFileExistAndCreateCmdArgs(TrackViewModel track, out string args)
        {
            var trackFileName = (track.Title + "_" + track.Artist + ".mp3").Replace(" ", "_");
            trackFileName = StringHelper.GetSafeFilename(trackFileName);
            var trackPath = Path.Combine(UserSettings.DownloadFolder, trackFileName);
            
            args = '"' + Directory.GetCurrentDirectory() + "\\lib\\ffmpeg" + '"' + " -i " + track.Uri?.AbsoluteUri + " -c copy " + trackPath;
            
            return track.Uri == null || File.Exists(trackPath);
        }
        
        #region Get Music

        /// <summary>
        /// Получить списко аудиозаписей пользователя или сообщества
        /// </summary>
        private void GetUserOrGroupMusic(long? id, bool isNextLoad = false)
        {
            if (!isNextLoad)
                _offset = 0; 

            if (id == _userId)
            {
                SelectedFriend = null;
                SelectedGroup = null;
            }
            else
            {
                if (SelectedFriend != null && SelectedFriend.Id == id)
                {
                    SelectedGroup = null;
                }
                else
                {
                    if (SelectedGroup != null && SelectedGroup.Id == id)
                    {
                        SelectedFriend = null;
                    }
                }
            }
            
            PlaylistType = PlaylistType.UserOrGroup;

            Task.Run(() =>
            {
                try
                {
                    var audios = _api.Audio.Get(new AudioGetParams
                    {
                        OwnerId = id,
                        Count = Settings.Default.Count,
                        Offset = _offset
                    });

                    LoadTracks(audios, isNextLoad);
                }
                catch (Exception e)
                {
                    LoggerFacade.WriteError(e.Message, isShow:true);
                }
            });
        }

        /// <summary>
        /// Получить популярные аудиозаписи
        /// </summary>
        private void GetPopular(bool isNextLoad = false)
        {
            if (!isNextLoad)
                _offset = 0;

            PlaylistType = PlaylistType.Popular;

            Task.Run(() =>
            {
                AudioGenre? audioGenre = null;

                if (UserSettings.PopularAudioGenre != AudioGenreExt.All)
                {
                    audioGenre = (AudioGenre?) UserSettings.PopularAudioGenre;
                }

                var audios = _api.Audio.GetPopular(UserSettings.PopularOnlyEng, audioGenre, Settings.Default.Count, _offset);

                LoadTracks(audios, isNextLoad);
            });
        }

        /// <summary>
        /// Получить рекомендованные аудиозаписи
        /// </summary>
        private void GetRecommendations(bool isNextLoad = false)
        {
            if (!isNextLoad)
                _offset = 0;

            PlaylistType = PlaylistType.Recommendations;

            Task.Run(() =>
            {
                var audios = _api.Audio.GetRecommendations(null, _userId, Settings.Default.Count, _offset, UserSettings.RecommendationsIsShuffle);

                LoadTracks(audios, isNextLoad);
            });
        }

        /// <summary>
        /// Получить аудиозаписи из посика
        /// </summary>
        private void GetSearch(bool isNextLoad = false)
        {
            if (!isNextLoad)
                _offset = 0;

            PlaylistType = PlaylistType.Search;

            Task.Run(() =>
            {
                var audios = _api.Audio.Search(new AudioSearchParams()
                {
                    Count = Settings.Default.Count,
                    Offset = _offset,
                    Autocomplete = true,
                    Query = Query
                });

                LoadTracks(audios, isNextLoad);
            });
        }

        /// <summary>
        /// Загрузить список треков
        /// </summary>
        private void LoadTracks(IEnumerable<Audio> audios, bool isNextLoad = false)
        {
            UiInvoker.Invoke(() =>
            {
                if(!isNextLoad)
                    ClearTracks();

                foreach (var audio in audios)
                {
                    Tracks.Add(new TrackViewModel(audio));
                }
            });
        }

        #endregion
        
        #region Others

        /// <summary>
        /// Загрузка
        /// </summary>
        public void OnLoad()
        {
            _userId = UserSettings.UserId;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var friends =_api.Friends.Get(new FriendsGetParams
                    {
                        Fields = ProfileFields.Photo50,
                        Order = FriendsOrder.Hints
                    });

                    var groups = _api.Groups.Get(new GroupsGetParams
                    {
                        Extended = true
                    });

                    UiInvoker.Invoke(() =>
                    {
                        Friends.Clear();
                        Groups.Clear();

                        foreach (var friend in friends)
                        {
                            Friends.Add(new FriendViewModel(friend));
                        }

                        foreach (var group in groups)
                        {
                            Groups.Add(new GroupViewModel(group));
                        }
                    });
                }
                catch (Exception e)
                {
                    LoggerFacade.WriteError(e);
                }
            });
        }

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
            if (e.IsChanged(nameof(UserSettings.UserName)) || e.IsChanged(nameof(UserSettings.Password)) || string.IsNullOrWhiteSpace(UserSettings.AccessToken))
            {
                AuthorizeFromLogPass();
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
            CalculateDuration();
        }

        private void Player_OnPlayerStateChanged(object sender, IPlayerStateChangedEventArgs e)
        {
            PlayerState = e.PlayerState;

            if (PlayerState == PlayerState.Stopped)
            {
                Duration = string.Empty;
            }
        }

        private void Player_OnLengthChanged(object sender, ILengthChangedEventArgs e)
        {
            TotalProgress = e.Length;
            CalculateDuration();
        }

        private void Player_OnEndReached(object sender, IEndReachedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(a =>
            {
                NextTrackCommand.Execute(null);
            });
        }

        #endregion

        #region Captcha

        private void CaptchaViewModel_OnCaptchaEntered(object sender, CaptchaEnteredEventArgs e)
        {
            AuthorizeFromLogPass(e.CaptchaSid, e.CaptchaKey);
        }

        #endregion

        #region Timers

        private void UpdateIsAuthorizedTimer_OnElapsed(object sender, ElapsedEventArgs e)
        {
            if (!_api.IsAuthorized)
            {
                _updateIsAuthorizedTimer.Stop();
                Clear();
            }
        }

        #endregion

        #region Downloader

        private void OutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data) && e.Data.Contains("video:0kB audio:"))
            {
                UiInvoker.Invoke(ExtensionMethods.Flash);
            }
        }

        #endregion

        #endregion

        #region Commands

        private void InitCommands()
        {
            ConnectionSettingsCommand = new DelegateCommand(ConnectionSettingsExecute);
            NextContentCommand = new DelegateCommand(NextContentExecute);
            SelectedCommand = new DelegateCommand<TrackViewModel>(SelectedExecute);
            PreviousTrackCommand = new DelegateCommand(PreviousTrackExecute);
            NextTrackCommand = new DelegateCommand(NextTrackExecute);
            PlayPauseCommand = new DelegateCommand(PlayPauseExecute);
            StopCommand = new DelegateCommand(StopExecute);
            DownloadCommand = new DelegateCommand(DownloadExecute);
            DownloadTrackCommand = new DelegateCommand<TrackViewModel>(DownloadTrackExecute);
            SearchCommand = new DelegateCommand(SearchExecute);
            GetPopularCommand = new DelegateCommand(GetPopularExecute);
            GetMyMusicCommand = new DelegateCommand(GetMyMusicExecute);
            GetRecommendationsCommand = new DelegateCommand(GetRecommendationsExecute);
        }

        #region Command Props

        public ICommand ConnectionSettingsCommand { get; private set; }

        public ICommand SelectedCommand { get; private set; }

        public ICommand PreviousTrackCommand { get; private set; }

        public ICommand NextTrackCommand { get; private set; }

        public ICommand PlayPauseCommand { get; private set; }

        public ICommand StopCommand { get; private set; }

        public ICommand NextContentCommand { get; private set; }

        public ICommand DownloadCommand { get; private set; }

        public ICommand DownloadTrackCommand { get; private set; }

        public ICommand SearchCommand { get; private set; }

        public ICommand GetPopularCommand { get; private set; }

        public ICommand GetMyMusicCommand { get; private set; }

        public ICommand GetRecommendationsCommand { get; private set; }
        
        #endregion

        private void ConnectionSettingsExecute()
        {
            ConnectionSetupViewModel.OpenConnectionSettings();
        }

        private void NextContentExecute()
        {
            _offset = _offset + Settings.Default.Count;

            switch (PlaylistType)
            {
                case PlaylistType.Popular:
                    GetPopular(isNextLoad:true);
                    break;

                case PlaylistType.UserOrGroup:
                    if (SelectedFriend != null)
                    {
                        GetUserOrGroupMusic(SelectedFriend.Id, isNextLoad:true);
                    }
                    else if (SelectedGroup != null)
                    {
                        GetUserOrGroupMusic(SelectedGroup.Id, isNextLoad: true);
                    }
                    else
                    {
                        GetUserOrGroupMusic(_userId, isNextLoad: true);
                    }
                    break;

                case PlaylistType.Recommendations:
                    GetRecommendations(isNextLoad: true);
                    break;

                case PlaylistType.Search:
                    GetSearch(isNextLoad:true);
                    break;

                case PlaylistType.None:
                    break;
            }
        }

        private void SelectedExecute(TrackViewModel track)
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
            if (string.IsNullOrWhiteSpace(UserSettings.DownloadFolder) ||
                !Directory.Exists(UserSettings.DownloadFolder))
            {
                MessageBoxExt.Show(Localization.strings.Error, Localization.strings.DownloadPathError);
                return;
            }

            for (var index = 0; index < Tracks.Count; index++)
            {
                var track = Tracks[index];
                var isExist = CheckFileExistAndCreateCmdArgs(track, out var args);
                if(isExist)
                    continue;

                await Download(args);
            }
        }

        private async void DownloadTrackExecute(TrackViewModel track)
        {
            if (string.IsNullOrWhiteSpace(UserSettings.DownloadFolder) ||
                !Directory.Exists(UserSettings.DownloadFolder))
            {
                MessageBoxExt.Show(Localization.strings.Error, Localization.strings.DownloadPathError);
                return;
            }

            var isExist = CheckFileExistAndCreateCmdArgs(track, out var args);
            if (isExist)
            {
                return;
            }

            await Download(args);
        }

        private void SearchExecute()
        {
            GetSearch();
        }

        private void GetPopularExecute()
        {
            GetPopular();
        }

        private void GetMyMusicExecute()
        {
            GetUserOrGroupMusic(_userId);
        }

        private void GetRecommendationsExecute()
        {
            GetRecommendations();
        }

        #endregion

        #region Events

        public event EventHandler<System.EventArgs> TracksCleared;

        #endregion
    }
    
}
