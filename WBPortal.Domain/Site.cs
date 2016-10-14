using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace WBSSLStore.Domain
{
    [Serializable] 
    public class Site : IEntity
    {
        [Key]
        public int ID { get; set; }
        [StringLength(256)]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "AliasRequired", AllowEmptyStrings = false)]
        public string CName { get; set; }
        [StringLength(256)]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "AliasRequired", AllowEmptyStrings = true)]
        public string Alias { get; set; }
        public bool isActive { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "APIUsernameRequired", AllowEmptyStrings = false)]
        [Display(Name = "Username_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string APIUsername { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "APIPasswordRequired", AllowEmptyStrings = false)]
        public string APIPassword { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "APIPartnerCodeRequired", AllowEmptyStrings = false)]
        public string APIPartnerCode { get; set; }
        [StringLength(1024)]
        [Display(Name = "APIAuthToken_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string APIAuthToken { get; set; }
        public bool APIisInTest { get; set; }
        //Navigational Properties
        public ICollection<SiteSettings> Settings { get; set; }
        public ICollection<Language> SupportedLanguages { get; set; }
        public ICollection<Pages> Pages { get; set; }
    
    }
}
