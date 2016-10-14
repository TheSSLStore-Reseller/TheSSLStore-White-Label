namespace WBSSLStore.Gateways.RestAPIModels.Request
{
    public class OrderRequest
    {
      
        public AuthRequest AuthRequest { get; set; }
        public string CustomOrderID { get; set; }
        public string TheSSLStoreOrderID { get; set; }
        public string ResendEmailType { get; set; } //Only used when resend is needed
        public string ResendEmail { get; set; }//Only used when changeapproveremail is needed
        public string RefundReason { get; set; }
        public string RefundRequestID { get; set; }
    }

    public class Contact
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public string OrganizationName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

    }
    public class TinyOrderRequest
    {
        public AuthRequest AuthRequest { get; set; }
        public bool PreferVendorLink { get; set; }
        public string ProductCode { get; set; }
        public string ExtraProductCode { get; set; }
        public int ServerCount { get; set; }
        public string RequestorEmail { get; set; }
        public int ExtraSAN { get; set; }
        public string CustomOrderID { get; set; }
        public int ValidityPeriod { get; set; }
        public bool AddInstallationSupport { get; set; }
        public string EmailLanguageCode { get; set; }

    }

    public class NewOrderRequest
    {
        public AuthRequest AuthRequest { get; set; }
        public OrganizationInfo OrganisationInfo { get; set; }        
        public Contact AdminContact { get; set; }
        public Contact TechnicalContact { get; set; }

        public string ProductCode { get; set; }
        public string ExtraProductCodes { get; set; }
        public string CustomOrderID { get; set; }
        public int ValidityPeriod { get; set; }
        public int ServerCount { get; set; }
        public string CSR { get; set; }
        public string DomainName { get; set; }
        public string WebServerType { get; set; }
        public string DNSNames { get; set; }
        public string AddDomainNames { get; set; }
        public int AdditionalDomains { get; set; }
        public bool isCUOrder { get; set; }
        public bool isRenewalOrder { get; set; }
        public string SpecialInstructions { get; set; }
        public string RelatedTheSSLStoreOrderID { get; set; }
        public bool isTrialOrder { get; set; }
       
        public string ApproverEmail { get; set; }
        public int ReserveSANCount { get; set; }
        public bool AddInstallationSupport { get; set; }
        public string EmailLanguageCode { get; set; }
        public string PartnerOrderID { get; set; }
        public bool ReissueInsurance { get; set; }
        public string RequestorEmail { get; set; }
        public bool IsEnrollmentLink { get; set; }

    }

    public class OrganizationInfo
    {
        
        public string OrganizationName { get; set; }
        public string DUNS { get; set; }
        public string Division { get; set; }
        public string IncorporatingAgency { get; set; }
        public string RegistrationNumber { get; set; }
        public string JurisdictionCity { get; set; }
        public string JurisdictionRegion { get; set; }
        public string JurisdictionCountry { get; set; }
        public OrganizationAddress OrganizationAddress { get; set; }
    }

    public class OrganizationAddress
    {        
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }        
        public string City { get; set; }        
        public string Region { get; set; }       
        public string PostalCode { get; set; }        
        public string Country { get; set; }        
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string LocalityName { get; set; }
    }

    
}