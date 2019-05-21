using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using VKPlayer.AppCommon;
using VKPlayer.AppStartup;
using VKPlayer.ExceptionHandling;
using VKPlayer.UiHelpers;
using VKPlayer.ViewModels;
using VKPlayer.Views;

namespace VKPlayer
{
    public partial class App : ISingleInstanceApp
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (!SingleInstance<App>.InitializeAsFirstInstance(AppInfo.AppMutexName))
            {
                return;
            }

            try
            {
                if (args.Any(a =>
                    a.Trim().ToLower() == CommandLineArgs.Exit ||
                    a.Trim().ToLower() == CommandLineArgs.SignOut))
                {
                    // Если приложение не запущено и вызывается с флагом завершения, то и не запускаем его.
                    return;
                }

                var application = new App();

                application.InitializeComponent();
                application.Run();

                Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline),
                    new FrameworkPropertyMetadata { DefaultValue = 30 });

            }
            finally
            {
                SingleInstance<App>.Cleanup();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Logging.LoggerFacade.WriteInformation("App Startup...");

            var splashScreen = new SplashScreen("../Views/Images/splash.png");
            splashScreen.Show(true);

            InitializeMainWindow();
        }

        private MainWindow _mainWindow;
        private static MainViewModel _mainVm;
        private static App _application;

        private void InitializeMainWindow()
        {
            InitUnhandledExceptions();

            _application = this;
            _mainVm = new MainViewModel();
            _mainWindow = new MainWindow { DataContext = _mainVm };

            Current.MainWindow = _mainWindow;
            Current.MainWindow?.Show();
            _mainWindow.Open();
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            if (args == null || args.Count == 0)
            {
                return true;
            }

            return true;
        }

        private void InitUnhandledExceptions()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Dispatcher.UnhandledException += DispatcherOnUnhandledException;
            Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        }

        #region Exception Handled

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            ExceptionHandler.Handle(e.Exception, isSilent: true);
        }

        private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            ExceptionHandler.Handle(e.Exception, forceShutdown: true);
        }

        private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            ExceptionHandler.Handle(e.Exception, forceShutdown: true);
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ExceptionHandler.Handle((Exception)e.ExceptionObject, forceShutdown: true);
        }

        #endregion

        #region Exiting

        public static bool IsExiting { get; private set; }

        public new static void Exit()
        {
            if (IsExiting)
            {
                return;
            }

            IsExiting = true;

            _mainVm?.OnUnload();

            Task.Run(() =>
            {
                UiInvoker.Invoke(() => _application.Shutdown());
            });
        }

        #endregion
    }
}
