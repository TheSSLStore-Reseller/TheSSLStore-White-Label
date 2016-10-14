using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WBSSLStore.Domain
{
    [Serializable]
    public class Pages : IEntity
    {
        [Key]
        public int ID { get; set; }
        [Display(Name = "ParentID_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        [Required]
        public int ParentID { get; set; }
        [Display(Name = "PageCaption_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                    ErrorMessageResourceName = "PageNameRequired", AllowEmptyStrings = false)]
        [RegularExpression(RegularExpressionConstants.SPECIALCHARS, ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                  ErrorMessageResourceName = "ValidCaption")]
        public string Caption { get; set; }
        [Display(Name = "Slug_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        [StringLength(256)]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                    ErrorMessageResourceName = "SlugRequired", AllowEmptyStrings = false)]
        public string slug { get; set; }
        public int PageStatusID { get; set; }        
        public int BrandID { get; set; }
        public int SiteID { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [Display(Name = "StartDate_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public DateTime? StartDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [Display(Name = "EndDate_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public DateTime? EndDate { get; set; }
        [Display(Name = "DisplayOrder_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public decimal DisplayOrder { get; set; }
        [Display(Name = "URLTarget_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public int URLTargetID { get; set; }
        [NotMapped]
        public URLTarget URLTarget
        {
            get { return (URLTarget)URLTargetID; }
            set { URLTargetID = (int)value; }
        }
        [NotMapped]
        public PageStatus PageStatus
        {
            get { return (PageStatus)PageStatusID; }
            set { PageStatusID = (int)value; }
        }
        //Navigational Properties
        [ForeignKey("SiteID")]
        public virtual Site Site { get; set; }
    }
}
