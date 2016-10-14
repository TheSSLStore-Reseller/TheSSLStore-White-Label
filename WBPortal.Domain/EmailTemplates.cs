using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WBSSLStore.Domain
{
    public class EmailTemplates : IEntity
    {
        [Key]
        public int ID { get; set; }
        public int SiteID { get; set; }
        public int LangID { get; set; }
        [Display(Name = "EmailSubject_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                  ErrorMessageResourceName = "SubjectRequired", AllowEmptyStrings = false)]
        public string EmailSubject { get; set; }

        [MaxLength]
        [System.Web.Mvc.AllowHtml]
        [Column(TypeName = "ntext")]
        [Display(Name = "EmailContent_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string EmailContent { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                      ErrorMessageResourceName = "EmailFromRequired", AllowEmptyStrings = false)]
        [RegularExpression(RegularExpressionConstants.EMAIL, ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                  ErrorMessageResourceName = "ValidEmail")]
        [Display(Name = "From_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string From { get; set; }
        [RegularExpression(RegularExpressionConstants.EMAIL, ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                 ErrorMessageResourceName = "ValidEmail")]
        [Display(Name = "CC_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string CC { get; set; }
        [RegularExpression(RegularExpressionConstants.EMAIL, ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                 ErrorMessageResourceName = "ValidEmail")]
        [Display(Name = "BCC_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string BCC { get; set; }
        [RegularExpression(RegularExpressionConstants.EMAIL, ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                 ErrorMessageResourceName = "ValidEmail")]
        [Display(Name = "ReplyTO_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string ReplyTo { get; set; }

        [MaxLength]
        public string MailMerge { get; set; }
        public bool isActive { get; set; }

        public int EmailTypeId { get; set; }
        [NotMapped]
        public EmailType EmailType
        {
            get { return (EmailType)EmailTypeId; }
            set { EmailTypeId = (int)value; }
        }

        //Navigation
        [ForeignKey("LangID")]
        public virtual Language Language { get; set; }
        [ForeignKey("SiteID")]
        public virtual Site Site { get; set; }
    }
}