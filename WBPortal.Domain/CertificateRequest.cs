using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    public class CertificateRequest : IEntity
    {
        public int ID { get; set; }
        [StringLength(160)]
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "ProductNameRequired", AllowEmptyStrings = true)]
        [Display(Name = "Product_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string ProductName { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "CSRRequired", AllowEmptyStrings = true)]
        [Display(Name = "CSR_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string CSR { get; set; }
        [Display(Name = "NumberOfMonths_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public int NumberOfMonths { get; set; }
        [Display(Name = "isNewOrder_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public bool isNewOrder { get; set; }
        [Display(Name = "NumberOfServers_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public int NumberOfServers { get; set; }
        
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "DomainNameRequired", AllowEmptyStrings = true)]
        [Display(Name = "Domain_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string DomainName { get; set; }
        [Required(ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                   ErrorMessageResourceName = "ApproverEmailRequired", AllowEmptyStrings = true)]
        [RegularExpression(RegularExpressionConstants.EMAIL, ErrorMessageResourceType = typeof(WBSSLStore.Resources.ErrorMessage.Message),
                  ErrorMessageResourceName = "ValidEmail")]
        [Display(Name = "ApproverEmail_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string CertificateApproverEmail { get; set; }
        [Display(Name = "Organisation_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Organisation { get; set; }
        [Display(Name = "OrganisationUnit_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string OrganisationUnit { get; set; }
        [Display(Name = "Locality_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string Locality { get; set; }
        [Display(Name = "State_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string State { get; set; }
        [Display(Name = "Country_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string CountryText { get; set; }
        [Display(Name = "SpecialInstructions_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string SpecialInstructions { get; set; }
        [Display(Name = "AdditionalDomains_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public int AdditionalDomains { get; set; }
        [Display(Name = "WebServerID_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public int WebServerID { get; set; }
        [Display(Name = "AddDomainNames_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public string AddDomainNames { get; set; }
        [Display(Name = "IsCompetitiveUpgrade_Caption", ResourceType = typeof(WBSSLStore.Resources.GeneralMessage.Message))]
        public bool IsCompetitiveUpgrade { get; set; }

        public int AdminContactID { get; set; }
        public int TechnicalContactID { get; set; }
        public int BillingContactID { get; set; }

        //Navigation
        [ForeignKey("WebServerID")]
        public WebServerType WebServerType { get; set; }
        [ForeignKey("AdminContactID")]
        public CertificateContact AdminContact { get; set; }
        [ForeignKey("TechnicalContactID")]
        public CertificateContact TechnicalContact { get; set; }
        [ForeignKey("BillingContactID")]
        public CertificateContact BillingContact { get; set; }
    }
}
