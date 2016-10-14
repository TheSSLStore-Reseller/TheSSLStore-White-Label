using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class ProductPricing : IEntity
    {
        public int ID { get; set; }
        public int ProductID { get; set; }
        public int SiteID { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "NumberOfMonthsRequired", AllowEmptyStrings = false)]
        public int NumberOfMonths { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "SalesPriceRequired", AllowEmptyStrings = false)]
        public decimal SalesPrice { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "RetailPriceRequired", AllowEmptyStrings = false)]
        public decimal RetailPrice { get; set; }
        public decimal AdditionalSanPrice { get; set; }
        public bool isRecommended { get; set; }
        public int ContractID { get; set; }

        public int RecordStatusID { get; set; }
        [NotMapped]
        public RecordStatus RecordStatus
        {
            get { return (RecordStatus) RecordStatusID; }
            set { RecordStatusID = (int) value; }
        }

        //Navigational Properties
        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }
        [ForeignKey("SiteID")]
        public virtual Site Site { get; set; }
        [ForeignKey("ContractID")]
        public virtual Contract Contract { get; set; }
    }

    [NotMapped]
    public class RecentlyViewProdctList:IEntity
    {
        public int ID { get; set; }
        public string ProductCode { get; set; }
        public string CertName { get; set; }
        public decimal Salesprice { get; set; }
        public string DetailPageslug { get; set; }
        public string BrandName { get; set; }
        public string ValidationType { get; set; }
        public string StarRating { get; set; }

    }
}
