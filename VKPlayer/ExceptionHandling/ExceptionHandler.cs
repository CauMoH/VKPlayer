using System;
using System.Linq;
using System.Windows;
using VKPlayer.AppCommon;
using VKPlayer.Logging;
using VKPlayer.UiHelpers;

namespace VKPlayer.ExceptionHandling
{
    public static class ExceptionHandler
    {
        private static readonly ThreadSafeMarker IsShuttingDown = new ThreadSafeMarker();

        public static void Handle(Exception ex, bool forceShutdown = false, bool isSilent = false)
        {
            // TODO: localize

            LoggerFacade.WriteError(GetLogMessage(ex));

            var shutdown = forceShutdown;

            // возможна ситуация когда происходит подряд сразу несколько критических исключений,
            // в этом случае отображаем только первое и завершаем работу, остальные только логируем.
            if (shutdown && !IsShuttingDown.TrySet())
            {
                return;
            }

            if (!isSilent || shutdown)
            {
                var sourceEx = ExtractException(ex);
                var message = sourceEx.Message + "Unknown error occurred.";

                if (shutdown)
                {
                    message += Environment.NewLine + "Application will be closed. Please contact developers for support." + Environment.NewLine +
                                                     "Restart Application ?";
                }

                // при вызове из App_OnDispatcherUnhandledException, стандартный MessageBox работает надёжно.
                UiInvoker.Invoke(() =>
                {
                    MessageBoxResult result;
                    if (Application.Current.MainWindow != null)
                    {
                        result = MessageBox.Show(Application.Current.MainWindow, message, AppInfo.AppName,
                            MessageBoxButton.YesNo, MessageBoxImage.Error);
                    }
                    else
                    {
                        // произошло ещё до инициализации главного окна
                        result = MessageBox.Show(message, AppInfo.AppName, MessageBoxButton.YesNo, MessageBoxImage.Error);
                    }

                    if (result == MessageBoxResult.Yes)
                        System.Windows.Forms.Application.Restart();
                });
            }

            if (shutdown)
            {
                Environment.Exit(0);
            }
        }

        private static string GetLogMessage(Exception ex)
        {
            return ex is AggregateException aex ? string.Join(Environment.NewLine, aex.InnerExceptions.Select(e => e.ToString())) : ex.ToString();
        }

        private static Exception ExtractException(Exception ex)
        {
            return ex is AggregateException aex ? aex.InnerException : ex;
        }
    }
}
