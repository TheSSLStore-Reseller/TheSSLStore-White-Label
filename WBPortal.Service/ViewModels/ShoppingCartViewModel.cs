using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WBSSLStore.Domain;

namespace WBSSLStore.Service.ViewModels
{

    public class ProductPricingModel
    {
        public ProductPricing Month_12 {get;set;}
        public ProductPricing Month_24 { get; set; }
        public ProductPricing Month_36 { get; set; }
        public ProductPricing Month_48 { get; set; }
        public ProductPricing Month_60 { get; set; }
        public Product product { get; set; }
        public ProductAvailablity productAvailablity { get; set; }
        public string Message { get; set; }
    }

   public class ShoppingCartViewModel
    {

       public ShoppingCart Cart { get; set; }
       public List<ShoppingCartDetail> CartDetails { get; set; }
    }

   public class ShoppingCartJSonMessageModel
   {
       public string Message { get; set; }
       public string CartTotal { get; set; }
       public string promodiscount { get; set; }
       public string CartCount { get; set; }
       public string ItemCount { get; set; }
       public string Id { get; set; }
   }

   
}
