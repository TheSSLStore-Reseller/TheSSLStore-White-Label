using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WBSSLStore.Web.Helpers.Caching;
using WBSSLStore.Web.Helpers.IoC;
using WBSSLStore.Domain;
using System.Web.Optimization;
using WhiteBrandShrink.Migrations;

namespace WBSSLStore.Web
{
 
    public class MvcApplication : HttpApplication
    {

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            DependencyRegister.RegisterDepenencyResolver();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exc = Server.GetLastError();
#if(!DEBUG)
            throw exc;
#endif

            if (exc != null)
            {
                if (exc.Message.Contains("NoCatch") || exc.Message.Contains("maxUrlLength"))
                    return;
                HttpException httpEx = exc.GetBaseException() as HttpException;

                if (httpEx != null && (httpEx.GetHttpCode() == 403 || httpEx.GetHttpCode() == 404))
                    Response.RedirectToRoute("pagenotfound");
                else
                    Response.RedirectToRoute("pageerror");
            }
        }


        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            string slug = Request.Url.AbsoluteUri.ToString().ToLower();

            if (slug.Contains(".css") || slug.Contains(".js") || slug.Contains(".png") || slug.Contains(".jpg") || slug.Contains(".gif") || slug.Contains(".jpeg") || slug.Contains(".swf") || slug.Contains(".ico"))
                return;

            if (SiteCacher.isSiteNotCreate())
            {
                var eConfigset =Helpers.WBHelper.GetSiteConfiguration(string.Empty);
                if ((int)ConfigurationStage.GeneralSetup != (int)eConfigset)
                {
                    if (!string.IsNullOrEmpty(Request.Url.LocalPath) && Request.Url.LocalPath.Equals("/") && ConfigurationHelper.IsConfigurationFileExist())
                        Response.Redirect("~/runsetup/install/installindex");
                }
            }
        }


    }
}