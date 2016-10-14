using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WBSSLStore.Gateways.RestAPIModels.Request
{
    public class ProductRequest
    {
        public AuthRequest AuthRequest { get; set; }
        public string ProductCode { get; set; }
        public ProductType ProductType { get; set; }
    }

    public enum ProductType
    {
        ALL = 0,
        DV = 1,
        EV = 2,
        OV = 3,
        WILDCARD = 4,
        SCAN = 5,
        PCI = 6,
        SAN_ENABLED = 7,
        CODESIGN = 8,
        CU = 9
    }
  
}
