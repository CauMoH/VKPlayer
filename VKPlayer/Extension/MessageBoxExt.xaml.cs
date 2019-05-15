using System.Windows;

namespace VKPlayer.Extension
{
    public partial class MessageBoxExt
    {
        public static MessageBoxResult Result = MessageBoxResult.Cancel;
        public string Message { get; set; }
        public string MsgTitle { get; set; }

        private MessageBoxExt(string title, string message)
        {
            InitializeComponent();

            Message = message;
            MsgTitle = title;
            DataContext = this;
        }

        /// <summary>
        /// Показать MessageBox
        /// </summary>
        /// <param name="title">Заголовок</param>
        /// <param name="message">Сообщение</param>
        /// <param name="buttons">Типы кнопок для взаимодействия с пользователем</param>
        /// <returns></returns>
        public static MessageBoxResult Show(string title, string message, MessageBoxButton buttons = MessageBoxButton.OK)
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var msgBox = new MessageBoxExt(title, message)
                    {
                        Owner = Application.Current.MainWindow
                    };

                    if (buttons == MessageBoxButton.OK)
                    {
                        msgBox.OkButton.Visibility = Visibility.Visible;
                    }
                    else if (buttons == MessageBoxButton.OKCancel)
                    {
                        msgBox.OkButton.Visibility = Visibility.Visible;
                        msgBox.CancelButton.Visibility = Visibility.Visible;
                    }
                    else if (buttons == MessageBoxButton.YesNo)
                    {
                        msgBox.YesButton.Visibility = Visibility.Visible;
                        msgBox.NoButton.Visibility = Visibility.Visible;
                    }
                    else if (buttons == MessageBoxButton.YesNoCancel)
                    {
                        msgBox.YesButton.Visibility = Visibility.Visible;
                        msgBox.NoButton.Visibility = Visibility.Visible;
                        msgBox.CancelButton.Visibility = Visibility.Visible;
                    }

                    msgBox.ShowDialog();
                }
                catch
                {
                    // ignored
                }

                return Result;
            });
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            Close();
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }
    }
}
