using System.ComponentModel;
namespace WBSSLStore.Domain
{
    public enum RefundStatus
    {
        [Description("No Action")]
        NOACTION=0,
        [Description("Refund  my Credit Card (3-5 business days)")]
        CANCEL_ORDER_AND_REFUND=1,
        [Description("Give me store credit (Immediate)")]
        CANCEL_ORDER_AND_STORE_CREDIT=2
    }
}