using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WBSSLStore.Domain;


namespace WBSSLStore.Service.ViewModels
{
    public class StaticPageViewModel
    {
        public int ProductID { get; set; }
        public List<ProductPricing> Items { get; set; }
        public int qty { get; set; }
        public string ReadMoreUrl { get; set; }
        public CMSPage CMSPage { get; set; }
        public CMSPageContent CMSPageContent { get; set; }
        public string CurrentUserName { get; set; } 
    }


  
   
}
