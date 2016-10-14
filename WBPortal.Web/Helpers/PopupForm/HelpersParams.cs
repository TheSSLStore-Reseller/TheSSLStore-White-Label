using System.Collections.Generic;

namespace WBSSLStore.Web.Helpers.PopupForm
{
    public abstract class EditorParams
    {
        public string Area { get; set; }
        public string Prop { get; set; }
        public object Value { get; set; }
        public string Controller { get; set; }
        public object HtmlAttributes { get; set; }
        public string Prefix { get; set; }
        public IDictionary<string, string> Data { get; set; }
    }

    public class AjaxDropdownParams : EditorParams
    {
        public string ParentId { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
    }

    public class AutocompleteParams : EditorParams
    {
        public string ParentId { get; set; }

        public string PropId { get; set; }
        public object PropIdValue { get; set; }
        public int? MaxResults { get; set; }
        public int? MinLength { get; set; }
        public int? Delay { get; set; }
        public bool GeneratePropId { get; set; }
    }
    public class AjaxRadioListParams : EditorParams
    {
        public string ParentId { get; set; }
    }

    public class AjaxCheckBoxListParams : EditorParams
    {
        public string ParentId { get; set; }
    }

    public class LookupParams : EditorParams
    {
        public IDictionary<string, object> Parameters { get; set; }
        public string Title { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }
        public string ChooseText { get; set; }
        public string CancelText { get; set; }
        public bool? ClearButton { get; set; }
        public bool? Multiselect { get; set; }
        public bool? Fullscreen { get; set; }
        public bool? Paging { get; set; }
    }

    public class PopupActionParams
    {
        public string Area { get; set; }
        public string Action { get; set; }
        public IEnumerable<object> Parameters { get; set; }
        public string Prefix { get; set; }
        public string Controller { get; set; }
    }

    public class PopupActionLinkParams : PopupActionParams
    {
        public string Text { get; set; }
        public object HtmlAttributes { get; set; }
    }

    public class MakePopupParams
    {
        public string Area { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }
        public IDictionary<string, string> Buttons { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }
        public bool? Modal { get; set; }
        public bool? Resizable { get; set; }
        public string Prefix { get; set; }
        public string Position { get; set; }
        public bool? FullScreen { get; set; }
        public bool? RefreshOnClose { get; set; }
        public string[] Parameters { get; set; }
    }

    public class MakePopupFormParams
    {
        public string Area { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public string[] Parameters { get; set; }
        public string Title { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }
        public bool? Modal { get; set; }
        public bool? Resizable { get; set; }
        public string Prefix { get; set; }
        public string Position { get; set; }
        public bool? FullScreen { get; set; }
        public bool? RefreshOnSuccess { get; set; }
        public string OkText { get; set; }
        public string CancelText { get; set; }
        public bool? ClientSideValidation { get; set; }
        public string SuccessFunction { get; set; }
    }

    public class ConfirmParams
    {
        public string Message { get; set; }
        public string CssClass { get; set; }
        public string Title { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }
        public string YesText { get; set; }
        public string NoText { get; set; }
    }
}