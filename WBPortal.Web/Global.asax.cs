using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Ninject;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Data.Repository;
using WBSSLStore.Service;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Web.Helpers.Caching;
using WBSSLStore.Web.Helpers.IoC;
using WBSSLStore.Logger;
using WBSSLStore.Domain;
using System.Web.Optimization;

namespace WBSSLStore.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
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
                var eConfigset = WBSSLStore.Web.Helpers.WBHelper.GetSiteConfiguration();
                if ((int)WBSSLStore.Domain.ConfigurationStage.GeneralSetup != (int)(WBSSLStore.Domain.ConfigurationStage)eConfigset)
                {
                    if (!string.IsNullOrEmpty(Request.Url.LocalPath) && Request.Url.LocalPath.Equals("/") && WhiteBrandShrink.ConfigurationHelper.IsConfigurationFileExist())
                        Response.Redirect("~/runsetup/install/installindex");
                }
            }
        }

        private void Redirect301(string slug, CurrentSiteSettings CurrentSiteSettings)
        {

            if (slug.Contains(@"/admin"))
                return;

            string Host = Request.Url.DnsSafeHost.ToLower();

            //if (Host.IndexOf('.') > 0 && !(Host.Split('.').Length > 2))
            //{
            //    if (CurrentSiteSettings.IsRunWithWWW && !slug.Contains("www."))
            //    {
            //        Response.RedirectPermanent(slug.Replace(Host, "www." + Host));
            //    }
            //    else if (!CurrentSiteSettings.IsRunWithWWW && slug.Contains("www."))
            //        Response.RedirectPermanent(slug.Replace("www.", ""), true);
            //}
            //else if (!CurrentSiteSettings.IsRunWithWWW && Host.Contains("www."))
            //{
            //    Response.RedirectPermanent(slug.Replace("www.", ""), true);
            //}
        }

        private void SetCulture(CurrentSiteSettings CurrentSiteSettings)
        {
            if (CurrentSiteSettings.CurrentSite != null)
            {
                WBSSLStore.Web.Helpers.Localization.CultureSwitch.SwitchCulture(CurrentSiteSettings.CurrentSite, CurrentSiteSettings.CurrentLangCode, CurrentSiteSettings.CurrentCultureKey);
            }
        }


    }
}