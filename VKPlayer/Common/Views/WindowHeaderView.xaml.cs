using System.Windows;
using System.Windows.Input;
using VKPlayer.Enums;

namespace VKPlayer.Common.Views
{
    public partial class WindowHeaderView
    {
        public static DependencyProperty MenuTypeProperty = DependencyProperty.Register(nameof(MenuType), typeof(HeaderMenuType), typeof(WindowHeaderView));
        public static DependencyProperty ParentWindowProperty = DependencyProperty.Register(nameof(ParentWindow), typeof(Window), typeof(WindowHeaderView));
        public static DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(WindowHeaderView));

        public WindowHeaderView()
        {
            InitializeComponent();
        }

        public HeaderMenuType MenuType
        {
            get => (HeaderMenuType)GetValue(MenuTypeProperty);
            set => SetValue(MenuTypeProperty, value);
        }

        public Window ParentWindow
        {
            get => (Window)GetValue(ParentWindowProperty);
            set => SetValue(ParentWindowProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        private void Title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ParentWindow.DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.WindowState = WindowState.Minimized;
        }

        private void ExpandButton_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.WindowState = ParentWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.Close();
        }
    }
}
