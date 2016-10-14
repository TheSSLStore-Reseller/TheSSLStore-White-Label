using System.ComponentModel;
namespace WBSSLStore.Domain
{
    public enum PageStatus
    { 
        [Description("Hide in site")]
        Hide = 0,
        [Description("Show in site")]
        Show = 1,
        [Description("Hide in navigation")]
        HideInNavigation = 2
    }
}