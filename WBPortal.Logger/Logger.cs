using System.Web;
using Elmah;
using System;
namespace WBSSLStore.Logger
{
    public class Logger : ILogger
    {

        public void Log(string Message, LogType logType)
        {
            //TODO:Implement ELMAH
            try
            {
                var context = HttpContext.Current;
                Error err = new Error();
                err.ApplicationName = context != null ? context.Request.UserHostAddress : "Administrator";
                err.HostName = context != null ? context.Request.Url.DnsSafeHost : string.Empty;
                err.Detail = Message;
                err.Message = Message;
                err.StatusCode = (int)logType;
                err.Type = logType.ToString();
                err.Time = DateTime.Now;
                err.User = (context != null && context.User != null && context.User.Identity.IsAuthenticated) ? System.Web.Security.Membership.GetUser().Email : string.Empty;
                ErrorLog.GetDefault(context).Log(err);
            }
            catch
            { }
        }

        public void LogException(Exception e)
        {
            try
            {
                var context = HttpContext.Current;
                Error err = new Error(e, context);
                err.ApplicationName = context != null ? context.Request.UserHostAddress : "Administrator";
                err.HostName = context != null ? context.Request.Url.DnsSafeHost : string.Empty;
                ErrorLog.GetDefault(context).Log(err);
            }
            catch
            { }
        }
    }
}