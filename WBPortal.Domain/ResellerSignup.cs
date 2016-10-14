using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{

    public class ResellerSignup
    {
        public int UserID { get; set; }

        [Display(Name = "Company_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),ErrorMessageResourceName = "CompanyNameRequired", AllowEmptyStrings = false)]
        public string CompanyName { get; set; }

        [Display(Name = "Fname_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),ErrorMessageResourceName = "FirstNameRequired", AllowEmptyStrings = false)]
        public string FirstName { get; set; }

        [Display(Name = "Lname_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),ErrorMessageResourceName = "LastNameRequired", AllowEmptyStrings = false)]
        public string LastName { get; set; }
        
        [StringLength(256)]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message), ErrorMessageResourceName = "EmailRequired", AllowEmptyStrings = false)]
        [RegularExpression(RegularExpressionConstants.EMAIL, ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message), ErrorMessageResourceName = "ValidEmail")]
        [Display(Name = "Email_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Email { get; set; }

        [StringLength(150)]
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Password { get; set; }

        [StringLength(150)]        
        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessageResourceName = "ComparPassword", ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message))]
        public string ConfirmPassword { get; set; }

        [StringLength(256)]
        [RegularExpression(RegularExpressionConstants.EMAIL, ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),ErrorMessageResourceName = "ValidEmail")]
        [Display(Name = "AlternativeEmail_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string AlternateEmail { get; set; }              

        [StringLength(128)]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "StreetRequired", AllowEmptyStrings = false)]
        [Display(Name = "Street_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Street { get; set; }

        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),ErrorMessageResourceName = "CountryNameRequired", AllowEmptyStrings = false)]
        [Display(Name = "Country_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string CountryName { get; set; }

        [StringLength(64)]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),ErrorMessageResourceName = "StateRequired", AllowEmptyStrings = false)]
        [Display(Name = "State_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string State { get; set; }

        [StringLength(64)]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "CityRequired", AllowEmptyStrings = false)]
        [Display(Name = "City_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string City { get; set; }

        [StringLength(30)]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "ZipRequired", AllowEmptyStrings = false)]
        [Display(Name = "Zip_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Zip { get; set; }

        [StringLength(20)]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),ErrorMessageResourceName = "PhoneRequired", AllowEmptyStrings = false)]
        [Display(Name = "Phone_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Phone { get; set; }

        [Display(Name = "Fax_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Fax { get; set; }

        [StringLength(30)]
        [Display(Name = "Mobile_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Mobile { get; set; }


        public bool isReseller { get; set; }
        public bool isActive { get; set; }
        public bool isDeleted { get; set; }
        public bool isEnterprise { get; set; }
        public bool IsUnsubscribed { get; set; }
        public bool SecurityAlerts { get; set; }
        public decimal InitialFund { get; set; }
        public string HearedBy { get; set; }
        public string AllowBrands { get; set; }
        public decimal AllowCredit { get; set; }
        public AuthRequestReseller AuthRequest { get; set; }
        public AuthResponseReseller AuthResponse { get; set; }

        public ResellerSignup()
        {
            AuthRequest = new AuthRequestReseller();
            AuthResponse = new AuthResponseReseller();
        }
   
    }

    public class AuthRequestReseller
    {
        [Required]
        public string SecurityCode { get; set; }
        [Required]
        public string AuthToken { get; set; }
        public string ReplayToken { get; set; }
        public string UserAgent { get; set; }
        public string IPAddress { get; set; }

    }

    public class AuthResponseReseller
    {
        public bool isError;
        public string ErrorMessage;
        public string Timestamp { get; set; }
        public string ReplayToken { get; set; }
        public string InvokingPartnerCode { get; set; }
    }


}
