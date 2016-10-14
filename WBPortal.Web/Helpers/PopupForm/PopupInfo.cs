using System.Collections.Generic;

namespace WBSSLStore.Web.Helpers.PopupForm
{
    public class PopupInfo : PopupInfoBase
    {
        public IDictionary<string, string> Buttons { get; set; }
        public string Prefix { get; set; }
        public string Content { get; set; }
        public bool RefreshOnClose { get; set; }
    }
}