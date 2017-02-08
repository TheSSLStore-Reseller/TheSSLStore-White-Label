using System.ComponentModel;
namespace WBSSLStore.Domain
{
    public enum ProductType
    {
        DV = 0,
        OV = 1,
        CODE_SIGN = 2,
        PCISCAN = 3,
        IDTYPE = 4,
        EMAILCERT = 5,
        DOCUMENT = 6,
        EV = 7
    }
    public enum ProductBrands
    {
        All = 0,
        RapidSSL = 1,
        GeoTrust = 2,
        Symantec = 3, 
        Thawte = 4,
        Comodo = 5,
        Certum = 6
      


    }

    public enum CertificateType
    {
        [Description("EV Certificates")]
        EVCertificates = 1,
        [Description("Standard SSL Certificates")]
        StandardSSLCertificates,
        [Description("SGC Certificates")]
        SGCCertificates,
        [Description("Code Signing Certificates")]
        CodeSigningCertificates,
        [Description("High Assurance Certificates")]
        HighAssuranceCertificates,
        [Description("Wildcard SSL Certificates")]
        WildcardSSLCertificates,
        [Description("SAN Certificates")]
        SANCertificates,
        [Description("Anti-Malware Scan")]
        AntiMalwareScan
    }
}