using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WBSSLStore.Gateway.PayPal
{
    public class PaypalGatewayHandler:PGGatewayHandler 
    {
        PayPalHandler objPaypal = null;
        PaymentGatewayInteraction objTranResult = null;
        public PaypalGatewayHandler()
        {
            //objPaypal = new PaypalHandler();
            //objTranResult = new TransactionResult();
            //objPaypal.SetRedirect(objOrderData);
        }

        ~PaypalGatewayHandler()
        {
            objPaypal = new PayPalHandler();
            objTranResult = new PaymentGatewayInteraction();
        }


        public override PaymentGatewayInteraction ProcessAuth(GatewayInParameter transaction)
        {
            throw new NotImplementedException();
        }

        public override PaymentGatewayInteraction ProcessAuthAndCapture(GatewayInParameter transaction)
        {
            objPaypal = new PayPalHandler();
         
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
