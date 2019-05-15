using System;
using System.ComponentModel;
using System.Windows;
using VKPlayer.Extension;
using VKPlayer.Properties;
using VKPlayer.ViewModels;

namespace VKPlayer.Views
{
    public partial class MainWindow
    {
        #region Members and Properties      

        private readonly object _lockedObj = new object();

        /// <summary>
        /// VM главного окна программы
        /// </summary>
        public MainViewModel ViewModel => (MainViewModel)DataContext;

        private bool _isReallyLoaded;

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Констурктор главного окна приложения
        /// </summary>
        public MainWindow()
        {
            FocusVisualStyleRemover.Init();

            InitializeComponent();
        }

        private void LoadDimensions()
        {
            if (ViewModel.UserSettings.MainWindowSettings.IsMaximized)
            {
                WindowState = WindowState.Maximized;
            }

            Width = Math.Max(ViewModel.UserSettings.MainWindowSettings.Width, Settings.Default.WindowMinWidth);
            Height = Math.Max(ViewModel.UserSettings.MainWindowSettings.Height, Settings.Default.WindowMinHeight);

            var left = ViewModel.UserSettings.MainWindowSettings.Left;
            if (left < SystemParameters.VirtualScreenWidth && left >= 0)
            {
                Left = left;
            }

            var top = ViewModel.UserSettings.MainWindowSettings.Top;
            if (top < SystemParameters.VirtualScreenHeight && top >= 0)
            {
                Top = top;
            }
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                lock (this)
                {
                    if (WindowState != WindowState.Maximized && _isReallyLoaded)
                    {
                        ViewModel.UserSettings.MainWindowSettings.Top = Top;
                        ViewModel.UserSettings.MainWindowSettings.Left = Left;
                        ViewModel.UserSettings.MainWindowSettings.Width = ActualWidth;
                        ViewModel.UserSettings.MainWindowSettings.Height = ActualHeight;
                    }
                }
            }
            catch
            {
                //ignore    
            }
        }

        private void MainWindow_OnLocationChanged(object sender, System.EventArgs e)
        {
            if (WindowState != WindowState.Maximized && IsLoaded)
            {
                var winDim = ViewModel.UserSettings.MainWindowSettings;
                winDim.Left = Left;
                winDim.Top = Top;
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadDimensions();
            lock (_lockedObj)
            {
                _isReallyLoaded = true;
            }

            SizeChanged += MainWindow_OnSizeChanged;
            LocationChanged += MainWindow_OnLocationChanged;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (App.IsExiting)
            {
                return;
            }

            ViewModel.UserSettings.MainWindowSettings.IsMaximized = WindowState == WindowState.Maximized;

            e.Cancel = true;

            ViewModel.ExitFromApp();
        }

        public void Open()
        {
            WindowState = ViewModel.UserSettings.MainWindowSettings.IsMaximized ? WindowState.Maximized : WindowState.Normal;
            Activate();
        }
    }
}
