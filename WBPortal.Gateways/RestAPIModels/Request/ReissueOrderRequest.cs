using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WBSSLStore.Gateways.RestAPIModels.Request
{
    public class ReissueOrderRequest
    {
        public AuthRequest AuthRequest { get; set; }
        public string TheSSLStoreOrderID { get; set; }
        public string CSR { get; set; }
        public string WebServerType { get; set; }
        public string[] DNSNames { get; set; }
        public bool isRenewalOrder { get; set; }
        public string SpecialInstructions { get; set; }
        public Pair[] EditSAN { get; set; }
        public Pair[] DeleteSAN { get; set; }
        public Pair[] AddSAN { get; set; }
        public bool isWildCard { get; set; }
        public string ReissueEmail { get; set; }
    }

    public class Pair
    {
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
