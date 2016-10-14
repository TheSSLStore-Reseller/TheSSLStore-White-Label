using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations.Schema;


namespace WBSSLStore.Domain
{
    [Serializable]
    public class User : IEntity
    {
        [Key]
        public int ID { get; set; }
        [StringLength(256)]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "EmailRequired", AllowEmptyStrings = false)]
        [RegularExpression(RegularExpressionConstants.EMAIL, ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                  ErrorMessageResourceName = "ValidEmail")]
        [Display(Name = "Email_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Email { get; set; }
        [StringLength(64)]
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
      
        public string PasswordHash { get; set; }
        //start
        [StringLength(64)]
        [NotMapped]
        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        [System.ComponentModel.DataAnnotations.Compare("PasswordHash", ErrorMessageResourceName = "ComparPassword", ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message))]
        public string ConfirmPassword { get; set; }
        //End

        [StringLength(64)]
        public string PasswordSalt { get; set; }
        public int SiteID { get; set; }
        
        [Display(Name = "Company_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "CompanyNameRequired", AllowEmptyStrings = false)]
        public string CompanyName { get; set; }
        [Display(Name = "Fname_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "FirstNameRequired", AllowEmptyStrings = false)]
        public string FirstName { get; set; }
        [Display(Name = "Lname_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                    ErrorMessageResourceName = "LastNameRequired", AllowEmptyStrings = false)]
        public string LastName { get; set; }
        [StringLength(256)]
        [RegularExpression(RegularExpressionConstants.EMAIL, ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
        ErrorMessageResourceName = "ValidEmail")]
        [Display(Name = "AlternativeEmail_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        
        public string AlternativeEmail { get; set; }
        [StringLength(100)]
        [Display(Name = "HeardBy_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string HeardBy { get; set; }
        public int AuditID { get; set; }
        public int AddressID { get; set; }


        #region Enum
        public int UserTypeID { get; set; }
        [NotMapped]
        public UserType UserType
        {
            get { return (UserType)UserTypeID; }
            set { UserTypeID = (int)value; }
        }
        [Display(Name = "Status_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public int RecordStatusID { get; set; }
        [NotMapped]
        public RecordStatus RecordStatus
        {
            get { return (RecordStatus)RecordStatusID; }
            set { RecordStatusID = (int)value; }
        }
        #endregion


        //Navigational Properties
        [ForeignKey("SiteID")]
        public virtual Site Site { get; set; }
        [ForeignKey("AuditID")]
        public virtual Audit AuditDetails { get; set; }
        [ForeignKey("AddressID")]
        public virtual Address Address { get; set; }

        
    }
}
