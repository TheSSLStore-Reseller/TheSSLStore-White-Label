using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WBSSLStore.Gateways.RestAPIModels.Request;

namespace WBSSLStore.Gateways.RestAPIModels.Response
{
    public class ProductResponse
    {
        public AuthResponse AuthResponse { get; set; }
        public string ProductCode;
        public string ProductName;
        public bool CanbeReissued;
        public int ReissueDays;
        public List<ProductPricing> PricingInfo;
        public ProductType ProductType { get; set; }
        public bool isWlidcard;
        public bool IsSanEnable;
        public bool IsCompetitiveUpgradeSupported;
        public bool isNoOfServerFree;
        public string VendorName;
    }


    public class ProductPricing
    {
        public int NumberOfMonths;
        public int NumberOfServer;
        public decimal Price;
        public decimal PricePerAdditionalSAN { get; set; }
        public decimal PricePerAdditionalServer { get; set; }
        public decimal SRP;

    }

    public class ALLProduct
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public bool CanbeReissued { get; set; }
        public int ReissueDays { get; set; }
        public int RefundDays { get; set; }
        public bool IsWildCardProduct { get; set; }
        public int ProductType { get; set; }
        public bool IsSanEnable { get; set; }
        public int SanMin { get; set; }
        public int SanMax { get; set; }
        public string Brand { get; set; }
        public bool IsCompetitiveUpgradeSupported { get; set; }
        public bool isNoOfServerFree { get; set; }
        public List<APIPricing> Pricings;
        public ALLProduct()
        {
            Pricings = new List<APIPricing>();
        }
    }
    public class APIPricing
    {
        public int NumberOfMonth;
        public decimal Price;
        public decimal SRP;
        public decimal AdditionalSanPrice;
    }

    public class GetAllPricingResponse
    {
        public string ErrorMessage = string.Empty;
        public string ErrorCode = string.Empty;
        public List<ALLProduct> Product;
        public GetAllPricingResponse()
        {
            Product = new List<ALLProduct>();
        }
    }

    public class AllowedBrandResponse
    {
        public string AllowedBrand = string.Empty;
        public string ErrorCode = string.Empty;
        public string ErrorMessage = string.Empty;
    }
}
