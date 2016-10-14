using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
namespace WBSSLStore.Gateway
{
    public abstract class PGGatewayHandler
    {
        //Abstract Must Implement Methods for this Class.
        public abstract PaymentGatewayInteraction ProcessAuth(GatewayInParameter transaction);
        public abstract PaymentGatewayInteraction ProcessAuthAndCapture(GatewayInParameter transaction);
        public abstract PaymentGatewayInteraction ProcessVoidTransaction(GatewayInParameter transaction);
        public abstract PaymentGatewayInteraction ProcessRefund(GatewayInParameter transaction);
        public abstract PaymentGatewayInteraction ProcessRecurring(GatewayInParameter transaction);
        public abstract PaymentGatewayInteraction ProcessECheck(GatewayInParameter transaction);

       

       
        
    }
}
