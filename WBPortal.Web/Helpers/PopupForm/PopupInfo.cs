using System.Collections.Generic;

namespace WBSSLStore.Web.Helpers.PopupForm
{
    public class PopupInfo : PopupInfoBase
    {
        public IDictionary<string, string> Buttons { get; set; }
#pragma warning disable CS0108 // 'PopupInfo.Prefix' hides inherited member 'PopupInfoBase.Prefix'. Use the new keyword if hiding was intended.
        public string Prefix { get; set; }
#pragma warning restore CS0108 // 'PopupInfo.Prefix' hides inherited member 'PopupInfoBase.Prefix'. Use the new keyword if hiding was intended.
        public string Content { get; set; }
        public bool RefreshOnClose { get; set; }
    }
}