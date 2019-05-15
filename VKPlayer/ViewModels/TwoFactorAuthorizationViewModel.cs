using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using VKPlayer.UiHelpers;
using VKPlayer.Views;

namespace VKPlayer.ViewModels
{
    public class TwoFactorAuthorizationViewModel : BindableBase
    {
        #region Members

        private bool _isOpen;
        private string _twoFactCode;

        private TwoFactorAuthorizationView _view;

        #endregion

        #region Props

        /// <summary>
        /// Код двухфакторной авторизации
        /// </summary>
        public string TwoFactCode
        {
            get => _twoFactCode;
            set => SetProperty(ref _twoFactCode, value);
        }

        /// <summary>
        /// Флаг открытия окна
        /// </summary>
        public bool IsOpen
        {
            get => _isOpen;
            set => SetProperty(ref _isOpen, value);
        }

        #endregion

        /// <summary>
        /// Конструктор
        /// </summary>
        public TwoFactorAuthorizationViewModel()
        {
            InitCommands();
        }

        /// <summary>
        /// Открыть окно ввода кода
        /// </summary>
        public void Open()
        {
            TwoFactCode = string.Empty;

            IsOpen = true;
            _view = new TwoFactorAuthorizationView
            {
                DataContext = this,
                Owner = Application.Current.MainWindow
            };

            _view.Show();
        }

        /// <summary>
        /// Получить код двухфакторной авторизации
        /// </summary>
        /// <returns></returns>
        public string GetCode()
        {
            while (IsOpen)
            {
                Thread.Sleep(100);
            }

            return TwoFactCode;
        }

        /// <summary>
        /// Инициализация команд
        /// </summary>
        private void InitCommands()
        {
            OkCommand = new DelegateCommand(OkExecute);
        }

        /// <summary>
        /// Конманда применить настройки
        /// </summary>
        public ICommand OkCommand { get; private set; }

        private void OkExecute()
        {
            IsOpen = false;
        }
    }
}
