using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WBSSLStore.Domain
{
    [Serializable]
    public class Testimonials : IEntity
    {
        [Key]
        public int ID
        { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message), ErrorMessageResourceName = "TestimonialName", AllowEmptyStrings = false)]
        [Display(Name = "TestimonialName_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Signature
        { get; set; }
        [StringLength(800)]
        [System.Web.Mvc.AllowHtml]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message), ErrorMessageResourceName = "TestimonialDescription", AllowEmptyStrings = false)]
        [Display(Name = "TestimonialDescription_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Description
        { get; set; }

        [Display(Name = "Status_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public int RecordStatusID { get; set; }
        public int SiteID { get; set; }
        [NotMapped]
        public TestimonialStatus RecordStatus
        {
            get { return (TestimonialStatus)RecordStatusID; }
            set { RecordStatusID = (int)value; }
        }
        [ForeignKey("SiteID")]
        public virtual Site Site { get; set; }
    }
}
