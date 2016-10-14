using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Script.Serialization;

namespace WBSSLStore.Web.Helpers.PopupForm
{
    public static class PopupFormHelpers
    {
        public static MvcHtmlString GetPopupFormId(this UrlHelper url, string action, string controller = null, string area = null, string prefix = null)
        {
            return MvcHtmlString.Create(MakeId(url, action, controller, area, prefix));
        }

        private static string MakeId(UrlHelper url, string action, string controller, string area, string prefix)
        {
            return "pf" + prefix + (action + (controller ?? url.Controller()) + (area ?? url.Area())).ToLower();
        }

        public static MvcHtmlString PopupFormActionLink<T>(this HtmlHelper html, Expression<Action<T>> e, string text = null, object htmlAttributes = null, string area = null, string prefix = null)
            where T : Controller
        {
            return
                MvcHtmlString.Create(
                    string.Format("<a href='javascript:{0}' {2} >{1}</a>",
                    PopupFormAction(new UrlHelper(html.ViewContext.RequestContext), e, area, prefix),
                                  text ?? e.Name(), htmlAttributes.GetDictionary().ToHtml()));
        }

        public static MvcHtmlString PopupFormAction<T>(this UrlHelper url, Expression<Action<T>> e, string area = null, string prefix = null) where T : Controller
        {
            return PopupFormAction(url, e.Name(), e.GetValues(), typeof(T).Controller(), area, prefix);
        }

        public static MvcHtmlString PopupFormActionLink(this HtmlHelper html, PopupActionLinkParams o)
        {
            return html.PopupFormActionLink(o.Action, o.Text, o.Parameters, o.HtmlAttributes, o.Controller, o.Area, o.Prefix);
        }

        public static MvcHtmlString PopupFormActionLink(this HtmlHelper html, string action, string text = null, IEnumerable<object> parameters = null, object htmlAttributes = null, string controller = null, string area = null, string prefix = null)
        {
            return
                MvcHtmlString.Create(
                    string.Format("<a href='javascript:{0}' {2} >{1}</a>", PopupFormAction(new UrlHelper(html.ViewContext.RequestContext), action, parameters, controller, area, prefix),
                                  text ?? action, htmlAttributes.GetDictionary().ToHtml()));
        }

        public static MvcHtmlString PopupFormAction(this UrlHelper url, PopupActionParams o)
        {
            return url.PopupFormAction(o.Action, o.Parameters, o.Controller, o.Area, o.Prefix);
        }

        public static MvcHtmlString PopupFormAction(this UrlHelper url, string action, IEnumerable<object> parameters = null, string controller = null, string area = null, string prefix = null)
        {
            var funcParams = parameters != null ? parameters
                .Aggregate("", (current, o) => current + new JavaScriptSerializer().Serialize(o) + ",")
                .RemoveSuffix(",") : string.Empty;

            return MvcHtmlString.Create("call" + MakeId(url, action, controller, area, prefix) + "(" + funcParams + ")");
        }

        public static MvcHtmlString MakePopupForm(this HtmlHelper html,
            string action,
            string[] parameters = null,
            string controller = null,
            string title = null,
            int? height = null,
            int? width = null,
            bool? refreshOnSuccess = null,
            string okText = null,
            string cancelText = null,
            bool? clientSideValidation = null,
            string successFunction = null,
            bool? modal = null,
            bool? resizable = null,
            string position = null,
            bool? fullScreen = null,
            string area = null,
            string prefix = null)
        {
            return html.MakePopupForm(new MakePopupFormParams
            {
                Action = action,
                Parameters = parameters,
                Controller = controller,
                Title = title,
                Height = height,
                Width = width,
                RefreshOnSuccess = refreshOnSuccess,
                OkText = okText,               
                CancelText = @WBSSLStore.Resources.GeneralMessage.Message.Cancel_btn, 
                ClientSideValidation = clientSideValidation,
                SuccessFunction = successFunction,
                Modal = modal,
                Resizable = resizable,
                Position = position,
                FullScreen = fullScreen,
                Area = area,
                Prefix = prefix
            });
        }

        public static MvcHtmlString MakePopupForm(this HtmlHelper html, MakePopupFormParams o)
        {
            if (Settings.GetText != null)
            {
                if (o.OkText == null) o.OkText = Settings.GetText("PopupForm", "Ok");
                if (o.CancelText == null) o.CancelText = Settings.GetText("PopupForm", @WBSSLStore.Resources.GeneralMessage.Message.Cancel_btn);
                if (o.Title == null) o.Title = Settings.GetText("PopupForm", "Title");
            }
            o.ReadSettings(typeof(Settings.PopupForm));
            var info = new PopupFormInfo();
            info.ReadParams(o);
            info.Controller = info.Controller ?? html.Controller();
            info.Parameters = info.Parameters ?? new string[] { };
            info.Position = info.Position.JQPos();
            info.Area = info.Area ?? html.Area();
            return html.Partial(Settings.AwesomeFolder + "PopupForm.ascx", info);
        }

        public static MvcHtmlString MakePopupForm<T>(
            this HtmlHelper html,
            Expression<Action<T>> e,
            string title = null,
            int? height = null,
            int? width = null,
            bool? refreshOnSuccess = null,
            string okText = null,
            string cancelText = null,
            bool? clientSideValidation = null,
            string successFunction = null,
            bool? modal = null,
            bool? resizable = null,
            string position = null,
            bool? fullScreen = null,
            string area = null
            ) where T : Controller
        {
            return MakePopupForm(html, e.Name(), e.Params(), typeof(T).Controller(), title, height, width, refreshOnSuccess, okText,
                                 cancelText, clientSideValidation, successFunction, modal, resizable, position, fullScreen, area);
        }
    }
}