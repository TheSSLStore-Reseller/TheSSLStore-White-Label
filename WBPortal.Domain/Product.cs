using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class Product : IEntity
    {
        [Key]
        public int ID { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "BrandNameRequired", AllowEmptyStrings = false)]
        public int BrandID { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "ProductNameRequired", AllowEmptyStrings = false)]
        [Display(Name = "Product_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string ProductName { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "ProductDescriptionRequired", AllowEmptyStrings = false)]
    [Display(Name = "Description_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string ProductDescription { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "ProductCodeRequired", AllowEmptyStrings = false)]
        [Display(Name = "InternalProductCode_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string InternalProductCode { get; set; }
        [Display(Name = "CanbeReissued_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public bool CanbeReissued { get; set; }

        public int ReissueDays { get; set; }
        [Display(Name = "RefundDays_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public int RefundDays { get; set; }
        public string ReissueType { get; set; }
        [Display(Name = "isWildcard_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public bool isWildcard { get; set; }
        [Display(Name = "isSANEnabled_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public bool isSANEnabled { get; set; }
        [Display(Name = "isCompetitiveUpgradeAllowed_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public bool isCompetitiveUpgradeAllowed { get; set; }
        [Display(Name = "isNoOfServerFree_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public bool isNoOfServerFree { get; set; }
        [Display(Name = "SanMin_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public int? SanMin { get; set; }
        [Display(Name = "SanMax_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public int? SanMax { get; set; }

        [Display(Name = "ProductTypeID_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public int ProductTypeID { get; set; }

        [Display(Name = "Detail Page URL")]
        
        public string DetailPageslug { get; set; }

        [NotMapped]
        public ProductType ProductType
        {
            get { return (ProductType)ProductTypeID; }
            set { ProductTypeID = (int)value; }
        }

        public int RecordStatusID { get; set; }
        [NotMapped]
        public RecordStatus RecordStatus
        {
            get { return (RecordStatus)RecordStatusID; }
            set { RecordStatusID = (int)value; }
        }

        //Navigational Properties
        [ForeignKey("BrandID")]
        public Brand Brand { get; set; }
    }
}
