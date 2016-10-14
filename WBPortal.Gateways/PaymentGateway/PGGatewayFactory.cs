using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WBSSLStore.Gateway.AuthorizeNET;
using WBSSLStore.Gateway.PayPal;
using WBSSLStore.Gateways.PaymentGateway.MoneyBookers;

namespace WBSSLStore.Gateway
{
    public enum PaymentMode
    {
        CC = 0,
        PAYPAL = 1,
        GOOGLECHECKOUT = 2,
        OFFLINE = 3,
        MONEYBOOKERS = 4
    }
    public class PGGatewayFactory
    {
        public static PGGatewayHandler CreateInstance(int Gateway)
        {
            PGGatewayHandler oPGGatewayHandler = null;
            switch (Gateway)
            {
                case 1:
                    oPGGatewayHandler = new PaypalGatewayHandler(); 
                    break;
                case 2:
                    oPGGatewayHandler = new AuthorizeNETHandler();
                    break;
                case 4:
                    oPGGatewayHandler = new MBGatewayHandler();
                    break;
                default:
                    return null;
            }

            return oPGGatewayHandler;
        }
    }
}
