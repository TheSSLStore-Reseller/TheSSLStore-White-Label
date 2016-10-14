using System.Web;
using System.Web.Optimization;

namespace WBSSLStore
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {

            #region SiteInstaller
            bundles.Add(new ScriptBundle("~/bundales/jquery11.3js").Include("~/content/js/jquery-1.11.3.js"));
            bundles.Add(new ScriptBundle("~/bundales/unobtrusive-ajaxjs").Include("~/content/js/jquery.unobtrusive-ajax.js"));
            bundles.Add(new ScriptBundle("~/bundales/validate-unobtrusive").Include("~/content/js/jquery.validate.js", "~/content/js/jquery.validate.unobtrusive.js"));
            bundles.Add(new StyleBundle("~/bundales/sitestylecss").Include("~/content/css/wizard.css"));
            bundles.Add(new StyleBundle("~/bundales/fluploadcss").Include("~/content/vscript/vupload.css"));
            bundles.Add(new ScriptBundle("~/bundales/fluploadjs").Include("~/content/vscript/jquery.form.js", "~/content/vscript/vuploadfile.js"));
            #endregion

            #region  Default Site JS and CSS
            bundles.Add(new ScriptBundle("~/bundales/jquery.11.3.js").Include("~/content/js/jquery-1.11.3.js"));
            bundles.Add(new ScriptBundle("~/bundales/homepagejs").Include("~/content/js/jquery-1.11.3.js", "~/content/js/smk-accordion.js", "~/content/js/easyResponsiveTabs.js","~/content/scripts/WB/Custom.js"));
            bundles.Add(new ScriptBundle("~/bundales/wb/scripts").Include("~/content/scripts/jquery.ui/jquery-ui.js", "~/content/scripts/wb/wbscript.js", "~/content/scripts/wb/jquery.form.js", "~/content/scripts/wb/popup.js", "~/content/scripts/wb/cart.js"));
            bundles.Add(new ScriptBundle("~/bundales/wb/validationjs").Include("~/content/scripts/jquery.validate.js","~/content/scripts/jquery.validate.unobtrusive.js"));
            
            bundles.Add(new ScriptBundle("~/bundales/wb/jquery.unobtrusive.validate-js").Include("~/content/scripts/jquery.unobtrusive-ajax.js", "~/content/scripts/jquery.validate.js", "~/content/scripts/jquery.validate.unobtrusive.js"));
            bundles.Add(new ScriptBundle("~/bundales/wb/jquery.validate").Include("~/content/scripts/jquery.validate.js"));
            bundles.Add(new ScriptBundle("~/bundales/checkoutjs").Include("~/content/scripts/jquery.unobtrusive-ajax.js", "~/content/scripts/jquery.validate.js", "~/content/scripts/payment/jsvat.js", "~/content/scripts/payment/payment.js", "~/content/scripts/jquery.glob.js"));
            bundles.Add(new ScriptBundle("~/bundales/jquery-glob-all").Include("~/content/scripts/jQuery.glob.all.js"));
            bundles.Add(new ScriptBundle("~/bundales/jquery-glob").Include("~/content/scripts/jQuery.glob.js"));
            bundles.Add(new ScriptBundle("~/bundales/jquery-ui-1.10.custom").Include("~/Content/Scripts/jquery-ui-1.10.custom.js"));
            bundles.Add(new ScriptBundle("~/bundales/jquery-datepickerjs").Include("~/content/Scripts/i18n/jquery-ui-i18n.js"));

            bundles.Add(new ScriptBundle("~/bundales/jquery-1.5.1").Include("~/Content/Scripts/jquery-1.5.1.js"));
            bundles.Add(new ScriptBundle("~/bundales/jquery-ui-1.8.11").Include("~/Content/Scripts/jquery-ui-1.8.11.js"));

            bundles.Add(new ScriptBundle("~/bundales/jquery-ui").Include("~/Content/Scripts/Jquery.UI/jquery-ui.js"));
            bundles.Add(new ScriptBundle("~/bundales/admincommonjs").Include("~/Content/admin/js/common.js"));
            
            ///////----------Start CSS----------////////
            bundles.Add(new StyleBundle("~/bundales/homepagecss").Include("~/content/css/style.css", "~/content/css/header.css", "~/content/css/leftpanel.css", "~/content/css/rightpanel.css", "~/content/css/contentpanel.css", "~/content/css/controlpanel.css", "~/content/css/footer.css", "~/content/css/responsive.css"));
            bundles.Add(new StyleBundle("~/bundales/tabcss").Include("~/content/css/easy-responsive-tabs.css"));
            bundles.Add(new StyleBundle("~/bundales/checkoutcss").Include("~/content/css/checkout.css"));
            

            bundles.Add(new StyleBundle("~/bundales/jquery-ui-css").Include("~/content/themes/base/jquery-ui.css"));
            bundles.Add(new StyleBundle("~/bundales/jquery-ui-1.10.customcss").Include("~/content/Themes/base/jquery-ui-1.10.custom.css"));

            bundles.Add(new StyleBundle("~/bundales/mastercss").Include("~/content/Admin/css/master.css"));

            bundles.Add(new StyleBundle("~/bundales/adminmastercss").Include("~/content/admin/css/masterlayout.css"));
            bundles.Add(new StyleBundle("~/bundales/addfundcss").Include("~/content/css/addfund.css"));
            bundles.Add(new StyleBundle("~/bundales/jquery.fancyboxcss").Include("~/content/css/jquery.fancybox.css"));

            //BundleTable.EnableOptimizations = false;
            #endregion
        }
    }
}