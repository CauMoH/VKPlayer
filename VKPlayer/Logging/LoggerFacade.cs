using System;
using Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace VKPlayer.Logging
{
    internal static class LoggerFacade
    {
        private static readonly LogWriter LogWriter = new LogWriterFactory().Create();

        private const string CategoryError = "Error";
        private const string CategoryInformation = "Information";

        public static void WriteError(Exception ex, bool isShow = false)
        {
            WriteError(ex.ToString(), isShow);
        }

        public static void WriteError(string error, Exception ex, bool isShow = false)
        {
            WriteError($"{error}{Environment.NewLine}{ex}", isShow);
        }

        public static void WriteError(string error, bool isShow = false)
        {
            LogWriter.Write(error, CategoryError, 0, 0, System.Diagnostics.TraceEventType.Error);
            RaiseLogMessageAdded(error, isShow);
        }

        public static void WriteInformation(string text)
        {
            LogWriter.Write(text, CategoryInformation, 0, 0, System.Diagnostics.TraceEventType.Information);
            RaiseLogMessageAdded(text, isShow: false);
        }

        private static void RaiseLogMessageAdded(string message, bool isShow)
        {
            LogMessageAdded?.Invoke(null, new LogMessageAddedEventArgs(message, isShow));
        }

        public static event EventHandler<LogMessageAddedEventArgs> LogMessageAdded;
    }
}
