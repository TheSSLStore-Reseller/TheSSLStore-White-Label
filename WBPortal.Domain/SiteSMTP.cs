using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class SiteSMTP : IEntity
    {
        [Key]
        public int ID { get; set; }
        public int SiteID { get; set; }
        public int AuditID { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "SMTPHostRequired", AllowEmptyStrings = false)]
        [Display(Name = "SMTPHost_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string SMTPHost { get; set; }
        [Display(Name = "SMTPUser_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string SMTPUser { get; set; }
        [Display(Name = "SMTPPassword_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string SMTPPassword { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "SMTPPortRequired", AllowEmptyStrings = false)]
        [Display(Name = "SMTPPort_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public int SMTPPort { get; set; }
         [Display(Name = "UseSSL_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public bool UseSSL { get; set; }
        public int TimeOut { get; set; }
    
        //Navigation
        [ForeignKey("SiteID")]
        public Site Site { get; set; }
        [ForeignKey("AuditID")]
        public Audit AuditDetails { get; set; }
    }
}