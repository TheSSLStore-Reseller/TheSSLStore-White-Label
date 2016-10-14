using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WBSSLStore.Gateway;

namespace WBSSLStore.Gateways.PaymentGateway.MoneyBookers
{
    public class MBGatewayHandler : PGGatewayHandler
    {
        MBHandler objMB = null;
        PaymentGatewayInteraction objTranResult = null;
        public MBGatewayHandler()
        { }

        public override PaymentGatewayInteraction ProcessAuth(GatewayInParameter transaction)
        {
            throw new NotImplementedException();
        }

        public override PaymentGatewayInteraction ProcessAuthAndCapture(GatewayInParameter transaction)
        {
            objMB = new MBHandler();

            return objTranResult;
        }

        public override PaymentGatewayInteraction ProcessVoidTransaction(GatewayInParameter transaction)
        {
            throw new NotImplementedException();
        }

        public override PaymentGatewayInteraction ProcessRecurring(GatewayInParameter transaction)
        {
            throw new NotImplementedException();
        }

        public override PaymentGatewayInteraction ProcessECheck(GatewayInParameter transaction)
        {
            throw new NotImplementedException();
        }

        public override PaymentGatewayInteraction ProcessRefund(GatewayInParameter transaction)
        {
            throw new NotImplementedException();
        }
    }
}
