using System.ComponentModel.DataAnnotations;

namespace WBSSLStore.Domain
{
    public class CertificateContact : IEntity
    {
        [Key]
        public int ID { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "FirstNameRequired", AllowEmptyStrings = true)]
        [Display(Name = "Fname_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string FirstName { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "LastNameRequired", AllowEmptyStrings = true)]
        [Display(Name = "Lname_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string LastName { get; set; }
        [Display(Name = "Title_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Title { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                  ErrorMessageResourceName = "EmailRequired", AllowEmptyStrings = true)]
        [RegularExpression(RegularExpressionConstants.EMAIL, ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                  ErrorMessageResourceName = "ValidEmail")]
        [Display(Name = "Email_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string EmailAddress { get; set; }
        [StringLength(64)]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "PhoneRequired", AllowEmptyStrings = true)]
        [Display(Name = "Phone_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string PhoneNumber { get; set; }
    }
}