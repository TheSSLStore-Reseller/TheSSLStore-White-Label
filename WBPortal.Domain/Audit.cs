using System;
using System.ComponentModel.DataAnnotations;

namespace WBSSLStore.Domain
{
    [Serializable]
    public class Audit : IEntity
    {
        [Key]
        public int ID { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        
        [Display(Name = "Date_Created_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "DateCreatedRequired", AllowEmptyStrings = false)]
        [System.Xml.Serialization.XmlIgnore()]
        public DateTime DateCreated { get; set; }
         [System.Xml.Serialization.XmlIgnore()]
         [Display(Name = "Date_Modified_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public DateTime? DateModified { get; set; }
        [Display(Name = "IP_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string IP { get; set; }
        [StringLength(2000)]
        public string HttpHeaderDump { get; set; }
        [System.Xml.Serialization.XmlIgnore()]
        public int? ByUserID { get; set; }
    }
}