using System.Web.Mvc;

namespace WBSSLStore.Web.Areas.Checkout
{
    public class CheckoutAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Checkout";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Checkout_default",
                "Checkout/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
