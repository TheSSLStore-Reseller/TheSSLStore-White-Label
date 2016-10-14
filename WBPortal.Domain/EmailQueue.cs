using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class EmailQueue : IEntity
    {
        [Key]
        public int ID { get; set; }
        public int SiteSMTPID { get; set; }
        public int SiteID { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "SubjectRequired", AllowEmptyStrings = false)]
        [Display(Name = "Subject_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Subject { get; set; }
       [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "EmailFromRequired", AllowEmptyStrings = false)]
       [RegularExpression(RegularExpressionConstants.EMAIL, ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                 ErrorMessageResourceName = "ValidEmail")]
       [Display(Name = "From_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string From { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "EmailTORequired", AllowEmptyStrings = false)]
        [RegularExpression(RegularExpressionConstants.EMAIL, ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                  ErrorMessageResourceName = "ValidEmail")]
        [Display(Name = "TO_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string TO { get; set; }
        [RegularExpression(RegularExpressionConstants.EMAIL, ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                  ErrorMessageResourceName = "ValidEmail")]
        [Display(Name = "ReplyTO_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string ReplyTO { get; set; }
        [RegularExpression(RegularExpressionConstants.EMAIL, ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                  ErrorMessageResourceName = "ValidEmail")]
        [Display(Name = "CC_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string CC { get; set; }
        [RegularExpression(RegularExpressionConstants.EMAIL, ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                  ErrorMessageResourceName = "ValidEmail")]
        [Display(Name = "BCC_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string BCC { get; set; }

        [MaxLength]
        [Column(TypeName="ntext")]
        [System.Web.Mvc.AllowHtml]
        [Display(Name = "EmailContent_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string EmailContent { get; set; }

        public int NumberOfTries { get; set; }
        public DateTime QueuedOn { get; set; }
        public DateTime? LastAttempt { get; set; }
        public string LastErrorMessage { get; set; }
        
        //Navigation
        [ForeignKey("SiteID")]
        public Site Site { get; set; }
        [ForeignKey("SiteSMTPID")]
        public SiteSMTP SiteSMTP { get; set; }
    }
}