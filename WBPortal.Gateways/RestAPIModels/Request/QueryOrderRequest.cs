using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WBSSLStore.Gateways.RestAPIModels.Request
{
    public class OrderQueryRequest
    {
        public AuthRequest AuthRequest { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SubUserID { get; set; }
        public string ProductCode { get; set; }
    }
}
