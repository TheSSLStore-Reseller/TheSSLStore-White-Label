using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WBSSLStore.Gateways.RestAPIModels.Request
{
    public class CSRRequest
    {
        public AuthRequest AuthRequest { get; set; }
        public string ProductCode { get; set; }
        public string CSR { get; set; }
    }
}
