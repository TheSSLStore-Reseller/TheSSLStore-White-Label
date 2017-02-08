using System;

namespace WBSSLStore.Logger
{
    public interface ILogger
    {
        void Log(string Message,LogType logType);
        void LogException(Exception e);
    }
}