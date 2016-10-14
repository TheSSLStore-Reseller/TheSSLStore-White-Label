using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using Elmah;

namespace WBSSLStore.Logger
{
    public interface ILogger
    {
        void Log(string Message,LogType logType);
        void LogException(Exception e);
    }
}