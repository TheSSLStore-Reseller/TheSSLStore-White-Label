using System.Web.Mvc;

namespace WBSSLStore.Web.Areas.Client
{
    public class ClientAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "client";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Client_default",
                "client/{controller}/{action}/{id}",
                new { action = "index", id = UrlParameter.Optional }
            );
        }
    }
}
