using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Script.Serialization;

namespace WBSSLStore.Web.Helpers.PopupForm
{
    public static class PopupHelpers
    {
        public static MvcHtmlString PopupActionLink<T>(this HtmlHelper html, Expression<Action<T>> e, string text = null, object htmlAttributes = null, string prefix = null, string area = null)
            where T : Controller
        {
            return
                MvcHtmlString.Create(
                    string.Format("<a href='javascript:{0}' {2} >{1}</a>",
                                  PopupAction(new UrlHelper(html.ViewContext.RequestContext), e, prefix, area),
                                  text ?? e.Name(), htmlAttributes.GetDictionary().ToHtml()));
        }



        public static MvcHtmlString PopupActionLink(this HtmlHelper html, PopupActionLinkParams o)
        {
            return html.PopupActionLink(o.Action, o.Text, o.Parameters, o.HtmlAttributes, o.Controller, o.Prefix, o.Area);
        }

        public static MvcHtmlString PopupActionLink(this HtmlHelper html, string action, string text = null, IEnumerable<object> parameters = null, object htmlAttributes = null, string controller = null, string prefix = null, string area = null)
        {
            return
                MvcHtmlString.Create(
                    string.Format("<a href='javascript:{0}' {2} >{1}</a>", PopupAction(new UrlHelper(html.ViewContext.RequestContext), action.ToLower(), parameters, controller, prefix, area),
                                  text ?? action, htmlAttributes.GetDictionary().ToHtml()));
        }

        public static MvcHtmlString PopupAction<T>(this UrlHelper url, Expression<Action<T>> e, string prefix = null, string area = null) where T : Controller
        {
            return PopupAction(url, e.Name(), e.GetValues(), typeof(T).Controller(), prefix, area);
        }

        public static MvcHtmlString PopupAction(this UrlHelper url, PopupActionParams o)
        {
            return url.PopupAction(o.Action, o.Parameters, o.Controller, o.Prefix, o.Area);
        }

        public static MvcHtmlString PopupAction(this UrlHelper url, string action, IEnumerable<object> parameters = null, string controller = null, string prefix = null, string area = null)
        {
            var funcParams = parameters != null ?
                parameters.Aggregate("", (current, o) => current + new JavaScriptSerializer().Serialize(o) + ",").RemoveSuffix(",")
                : string.Empty;

            return MvcHtmlString.Create("call" + MakeId(url, action, controller, area, prefix) + "(" + funcParams + ")");
        }

        public static MvcHtmlString GetPopupId(this UrlHelper url, string action, string controller = null, string prefix = null, string area = null)
        {
            return MvcHtmlString.Create(MakeId(url, action, controller, area, prefix));
        }

        private static string MakeId(UrlHelper url, string action, string controller, string area, string prefix)
        {
            return "p" + prefix + (action + (controller ?? url.Controller()) + (area ?? url.Area())).ToLower();
        }

        public static MvcHtmlString MakePopup<T>(
            this HtmlHelper html,
            Expression<Action<T>> e,
            string title = null,
            IDictionary<string, string> buttons = null,
            int? height = null,
            int? width = null,
            bool? modal = null,
            bool? resizable = null,
            string prefix = null,
            string position = null,
            bool? fullScreen = null,
            string area = null
            ) where T : Controller
        {
            return MakePopup(html, e.Name(), e.Params(), typeof(T).Controller(), title, buttons, height, width, modal, resizable, prefix, position, fullScreen, area);
        }

        public static MvcHtmlString MakePopup(this HtmlHelper html, string action, string[] parameters = null, string controller = null, string title = null, IDictionary<string, string> buttons = null, int? height = null, int? width = null, bool? modal = null, bool? resizable = null, string prefix = null, string position = null, bool? fullScreen = null, string content = null, string area = null,bool? refreshonclose=null)
        {
            return html.MakePopup(new MakePopupParams { Action = action, Buttons = buttons, Content = content, Controller = controller, FullScreen = fullScreen, Height = height, Modal = modal, Parameters = parameters, Position = position, Prefix = prefix, Resizable = resizable, Title = title, Width = width, Area = area, RefreshOnClose = refreshonclose });
        }

        public static MvcHtmlString MakePopup(this HtmlHelper html, MakePopupParams o)
        {
            o.ReadSettings(typeof(Settings.Popup));
            var info = new PopupInfo();
            info.ReadParams(o);
            info.Controller = info.Controller ?? html.Controller();
            info.Position = info.Position.JQPos();
            info.Parameters = info.Parameters ?? new string[] { };
            info.Buttons = info.Buttons ?? new Dictionary<string, string>();
            info.Area = info.Area ?? html.Area();

            return html.Partial(Settings.AwesomeFolder + "Popup.ascx", info);
        }
    }
}