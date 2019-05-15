using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Prism.Commands;
using Prism.Mvvm;
using VKPlayer.EventArgs;
using VKPlayer.Logging;
using VKPlayer.Views;

namespace VKPlayer.ViewModels
{
    public class CaptchaViewModel : BindableBase
    {
        #region Members

        private bool _isOpen;
        private CaptchaView _view;
        private string _captcha;

        #endregion

        #region Props

        /// <summary>
        /// Флаг открытия окна
        /// </summary>
        public bool IsOpen
        {
            get => _isOpen;
            set => SetProperty(ref _isOpen, value);
        }

        /// <summary>
        /// Текста каптчи
        /// </summary>
        public string Captcha
        {
            get => _captcha;
            set => SetProperty(ref _captcha, value);
        }

        /// <summary>
        /// Ключ каптчи
        /// </summary>
        public long CaptchaSid { get; set; }

        /// <summary>
        /// Uri каптчи
        /// </summary>
        public Uri CaptchaUri { get; set; }

        /// <summary>
        /// Картинка каптчи
        /// </summary>
        public BitmapImage CaptchaImage
        {
            get
            {
                try
                {
                    var image = new BitmapImage();
                    int BytesToRead = 100;


                    WebRequest request = WebRequest.Create(CaptchaUri);
                    request.Timeout = -1;
                    WebResponse response = request.GetResponse();
                    Stream responseStream = response.GetResponseStream();
                    BinaryReader reader = new BinaryReader(responseStream);
                    MemoryStream memoryStream = new MemoryStream();

                    byte[] bytebuffer = new byte[BytesToRead];
                    int bytesRead = reader.Read(bytebuffer, 0, BytesToRead);

                    while (bytesRead > 0)
                    {
                        memoryStream.Write(bytebuffer, 0, bytesRead);
                        bytesRead = reader.Read(bytebuffer, 0, BytesToRead);
                    }

                    image.BeginInit();
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    image.StreamSource = memoryStream;
                    image.EndInit();

                    return image;
                }
                catch (Exception e)
                {
                    LoggerFacade.WriteError("load captcha error", e, isShow:true);
                }

                return null;
            }
        }

        #endregion

        public CaptchaViewModel()
        {
            InitCommands();
        }

        /// <summary>
        /// Открыть окно ввода captcha
        /// </summary>
        public void Open(long captchaSid, Uri captchaUrl)
        {
            Captcha = string.Empty;
            CaptchaSid = captchaSid;
            CaptchaUri = captchaUrl;

            IsOpen = true;

            _view = new CaptchaView
            {
                DataContext = this,
                Owner = Application.Current.MainWindow
            };

            _view.ShowDialog();
        }

        /// <summary>
        /// Инициализация команд
        /// </summary>
        private void InitCommands()
        {
            OkCommand = new DelegateCommand(OkExecute);
        }

        /// <summary>
        /// Конманда применить
        /// </summary>
        public ICommand OkCommand { get; private set; }

        private void OkExecute()
        {
            IsOpen = false;

            CaptchaEntered?.Invoke(this, new CaptchaEnteredEventArgs(CaptchaSid, Captcha));
        }

        #region Events

        public event EventHandler<CaptchaEnteredEventArgs> CaptchaEntered;

        #endregion
    }
}
