using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class PromoCode : IEntity
    {
        [Key]
        public int ID { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "CampaignNameRequired", AllowEmptyStrings = false)]
        
        [Display(Name = "CampaignName_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string CampaignName { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "PromoCodeRequired", AllowEmptyStrings = false)]
        [Display(Name = "CodeName_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Code { get; set; }
        [Display(Name = "StartDate_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public DateTime? StartDate { get; set; }
        [Display(Name = "EndDate_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public DateTime? EndDate { get; set; }
        [Display(Name = "Description_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string SmallDescription { get; set; }
        [Display(Name = "MaxOrders_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public int? MaxOrders { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "DiscountRequired", AllowEmptyStrings = false)]
        public decimal Discount { get; set; }
        public bool isForReseller { get; set; }
        public bool isForClient { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "ProductIDRequired", AllowEmptyStrings = false)]
        [Display(Name = "Product_Caption", ResourceType= typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public int ProductID { get; set; }
        public int NoOfMonths { get; set; }
        public int SiteID { get; set; }
        public int AuditID { get; set; }
        public int DiscountModeID { get; set; }
        [NotMapped]
        public DiscountMode DiscountMode
        {
            get { return (DiscountMode)DiscountModeID; }
            set { DiscountModeID = (int)value; }
        }

        //Navigation
        [ForeignKey("ProductID")]
        public Product Product { get; set; }
        [ForeignKey("SiteID")]
        public Site Site { get; set; }
        [ForeignKey("AuditID")]
        public Audit AuditDetails { get; set; }
    }

    public enum DiscountMode
    {
        FLAT = 0,
        PERCENTAGE = 1
    }
}
