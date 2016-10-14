using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WBSSLStore.Domain
{
    [Serializable]
    public class Address : IEntity
    {
        [Key]
        public int ID { get; set; }
        [StringLength(128)]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "StreetRequired", AllowEmptyStrings = false)]
        [Display(Name = "Street_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Street { get; set; }
        [StringLength(64)]
        [Display(Name = "Company_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string CompanyName { get; set; }
        [StringLength(64)]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "CityRequired", AllowEmptyStrings = false)]
        [Display(Name = "City_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string City { get; set; }
        [StringLength(64)]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "StateRequired", AllowEmptyStrings = false)]
        [Display(Name = "State_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        //[UIHint("State")]
        public string State { get; set; }
        [StringLength(30)]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "ZipRequired", AllowEmptyStrings = false)]
        [Display(Name = "Zip_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Zip { get; set; }
        [StringLength(20)]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "PhoneRequired", AllowEmptyStrings = false)]
        [Display(Name = "Phone_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Phone { get; set; }
        [StringLength(30)]
        [Display(Name = "Fax_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Fax { get; set; }
        [StringLength(30)]
        [Display(Name = "Mobile_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Mobile { get; set; }
        [StringLength(128)]
        [RegularExpression(RegularExpressionConstants.EMAIL,
            ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
            ErrorMessageResourceName = "ValidEmail")]
        [Display(Name = "Email_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Email { get; set; }
        public int CountryID { get; set; }


        //Navigational Properites
        [ForeignKey("CountryID")]
        [Display(Name = "Country_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public virtual Country Country { get; set; }
    }
}