using System;
using Logging;

namespace VKPlayer.Logging
{
    internal sealed class Logger : ILogger
    {
        public void WriteError(Exception ex, bool isShow = false)
        {
            LoggerFacade.WriteError(ex, isShow);
        }

        public void WriteError(string error, Exception ex, bool isShow = false)
        {
            LoggerFacade.WriteError(error, ex, isShow);
        }

        public void WriteError(string error, bool isShow = false)
        {
            LoggerFacade.WriteError(error, isShow);
        }

        public void WriteInformation(string text)
        {
            LoggerFacade.WriteInformation(text);
        }
    }
}
