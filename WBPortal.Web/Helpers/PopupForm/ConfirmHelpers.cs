using System.Web.Mvc;
using System.Web.Mvc.Html;
using WBSSLStore.Web.Helpers.PopupForm;

namespace WBSSLStore.Web.Helpers.PopupForm
{
    public static class ConfirmHelpers
    {
        public static MvcHtmlString Confirm(this HtmlHelper html, string message, string cssClass = null, string title = null, int? height = null, int? width = null, string yesText = null, string noText = null)
        {
            return html.Confirm(new ConfirmParams { CssClass = cssClass, Height = height, Message = message, NoText = noText, Title = title, Width = width, YesText = yesText });
        }

        public static MvcHtmlString Confirm(this HtmlHelper html, ConfirmParams o)
        {
            if (Settings.GetText != null)
            {
                if (o.YesText == null) o.YesText = Settings.GetText("Confirm", "Yes");
                if (o.NoText == null) o.NoText = Settings.GetText("Confirm", "No");
                if (o.Title == null) o.Title = Settings.GetText("Confirm", "Title");
            }
            return html.Partial(Settings.AwesomeFolder + "Confirm.ascx", new ConfirmInfo().ReadParams(o.ReadSettings(typeof(Settings.Confirm))));
        }
    }
}