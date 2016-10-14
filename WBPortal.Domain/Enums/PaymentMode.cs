using System.ComponentModel;
namespace WBSSLStore.Domain
{
    public enum CardTypes
    {
        Visa = 1,
        MasterCard = 2,
        AMEX = 3,
        DisCover = 4,
        AmericanExpress = 5
    }
    public enum PGInstances
    {
        PayPalIPN = 1,
        AuthorizeNet = 2,
        Moneybookers = 3
        //ACHDirect=3,
        //PayPalPro=4,
        //NoblePay=5
    }
    public enum PaymentMode
    {
        [Description("Credit Card")]
        CC = 0,
        [Description("Paypal")]
        PAYPAL = 1,
        [Description("Google Checkout")]
        GOOGLECHECKOUT = 2,
        [Description("Offline")]
        OFFLINE = 3,
        [Description("Moneybookers")]
        MONEYBOOKERS = 4
    }
}