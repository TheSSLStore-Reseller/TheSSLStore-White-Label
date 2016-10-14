using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WBSSLStore.Gateways.RestAPIModels.Response
{
   
    public class ServerCertInfo
    {
        public string CommonName { get; set; }
        public string SANs { get; set; }
        public string OrganizationName { get; set; }
        public string CertificateStartDate { get; set; }
        public string CertificateEndDate { get; set; }
        public int RemainingDays { get; set; }
        public string SignatureAlgorithm { get; set; }
        public string Issuer { get; set; }
    }

    public class ChainCertInfo
    {
        public string CommonName { get; set; }
        public string OrganizationName { get; set; }
        public string Location { get; set; }
        public string CertificateStartDate { get; set; }
        public string CertificateEndDate { get; set; }
        public string SignatureAlgorithm { get; set; }
        public string Issuer { get; set; }
    }

 
    public class SSLCheckerResponse
    {
        public bool isCertificateFound { get; set; }
        public string ServerIP { get; set; }
        public string ServerType { get; set; }
        public ServerCertInfo ServerCertInfo { get; set; }
        public List<ChainCertInfo> ChainCertInfo { get; set; }
        public AuthResponse AuthResponse { get; set; }
    }

    public class CSRDecodeResponse
    {
        public object CommonName { get; set; }
        public object OrganizationName { get; set; }
        public object OrganizationUnit { get; set; }
        public object Locality { get; set; }
        public object State { get; set; }
        public object Country { get; set; }
        public object Email { get; set; }
        public int? KeySize { get; set; }
        public AuthResponse AuthResponse { get; set; }
    }

    public class CertificateDecodeResponse
    {
        public string CommonName { get; set; }
        public string AlternativeNames { get; set; }
        public object OrganizationName { get; set; }
        public object OrganizationUnit { get; set; }
        public object Locality { get; set; }
        public object State { get; set; }
        public object Country { get; set; }
        public string CertificateStartDate { get; set; }
        public string CertificateEndDate { get; set; }
        public string Issuer { get; set; }
        public string SerialNumber { get; set; }
        public AuthResponse AuthResponse { get; set; }
    }

    public class KeyMatcherresponse
    {
        public bool IsCSRMatched { get; set; }
        public bool IsPrivateKeyMatched { get; set; }
        public AuthResponse AuthResponse { get; set; }
    }

    public class SSLConvertorResponse
    {
        public string Certificate { get; set; }
        public string PrivateKey { get; set; }
        public List<string> ChainCertificates { get; set; }
        public AuthResponse AuthResponse { get; set; }
    }

    public class CSRGenerateResponse
    {
        public string CSR { get; set; }
        public string PrivateKey { get; set; }
        public AuthResponse AuthResponse { get; set; }
    }


    public class InsecureLink
    {
        public int Line { get; set; }
        public string Link { get; set; }
    }

   

    public class WhyNoPadLockResponse
    {
        public bool isCertificateFound { get; set; }
        public string URLTested { get; set; }
        public string Domain { get; set; }
        public string ServerIP { get; set; }
        public string CommonName { get; set; }
        public string Issuer { get; set; }
        public string CertificateStartDate { get; set; }
        public string CertificateEndDate { get; set; }
        public List<InsecureLink> InsecureLinks { get; set; }
        public AuthResponse AuthResponse { get; set; }
    }

}
