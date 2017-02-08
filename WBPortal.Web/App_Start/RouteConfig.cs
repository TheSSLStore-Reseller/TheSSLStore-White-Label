using System.Web.Mvc;
using System.Web.Routing;
using WhiteBrandShrink.Migrations;

namespace WBSSLStore
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("content/{*pathInfo}");
            routes.IgnoreRoute("elmah.axd");
            routes.IgnoreRoute("*.png");
            routes.IgnoreRoute("*.jpg");
            routes.IgnoreRoute("*.gif");
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });
            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }, new string[] { "WBSSLStore.Web.Controllers" } // Parameter defaults
                );
            //routes.MapRoute("SiteDefault", "{controller}/{action}/{id}", new { controller = "StaticPage", action = "index", id = UrlParameter.Optional }, new string[] { "WBSSLStore.Web.Controllers" });


            using (ConfigurationHelper help = new ConfigurationHelper())
            {

                var allsettings = help.GetAllSettings();
                DataBaseSettings dbsettings = allsettings != null && allsettings.DataBaseSetting != null ? allsettings.DataBaseSetting : null;
                if (dbsettings != null)
                {
                    var eConfigset = WBSSLStore.Web.Helpers.WBHelper.GetSiteConfiguration(dbsettings.ConnectionString);
                    if ((int)WBSSLStore.Domain.ConfigurationStage.GeneralSetup != (int)(WBSSLStore.Domain.ConfigurationStage)eConfigset)
                    {
                        routes.MapRoute("Installation", "siteinstallation/{controller}/{action}/{id}", new { controller = "Install", action = "paymentsettings", id = UrlParameter.Optional }, new string[] { "WhiteBrandShrink.Controllers" });
                    }
                    else
                        LoadCustomRoutes(routes);
                }
                else
                {
                    LoadCustomRoutes(routes);
                }
            }



        }

        private static void LoadCustomRoutes(RouteCollection routes)
        {

            routes.MapRoute("SiteDefault", "{controller}/{action}/{id}", new { controller = "StaticPage", action = "index", id = UrlParameter.Optional }, new string[] { "WBSSLStore.Web.Controllers" });
            //routes.MapRoute("StaticPages", "{*slug}", new { controller = "StaticPage", action = "StaticRender", slug = "Index" });
            routes.MapRoute(
               "StaticPages",
               "{*slug}",
               new { controller = "StaticPage", action = "StaticRender", slug = UrlParameter.Optional }
               );
        }
    }
}