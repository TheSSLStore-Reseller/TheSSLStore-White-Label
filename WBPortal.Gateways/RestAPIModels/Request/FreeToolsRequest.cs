using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WBSSLStore.Gateways.RestAPIModels.Request
{
    public class SSLCheckerRequest
    {
        public string HostName { get; set; }
        public AuthRequest AuthRequest { get; set; }
    }

    public class CSRDecodeRequest
    {
        public string CSR { get; set; }
        public AuthRequest AuthRequest { get; set; }
    }

    public class CertificateDecodeRequest
    {
        public string Certificate { get; set; }
        public AuthRequest AuthRequest { get; set; }
    }

    public class KeyMatcherRequest
    {
        public string Certificate { get; set; }
        public string CSR { get; set; }
        public string PrivateKey { get; set; } 
        public AuthRequest AuthRequest { get; set; }
    }

    public class WhyNoPadLockRequest
    {
        public string URL { get; set; } 
        public AuthRequest AuthRequest { get; set; }
    }

    public class SSLConvertorRequest
    {
        public string Certificate { get; set; }
        public string PrivateKey { get; set; }
        public string RootCA { get; set; }
        public string IntermediatesCA { get; set; }
        public string KeyPassword { get; set; }
        public string ConvertFrom { get; set; }
        public string ConvertTo { get; set; }
        public AuthRequest AuthRequest { get; set; }
    }

    public class CSRGenerateRequest
    {
        public string CommonName { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationUnit { get; set; }
        public string Locality { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string KeySize { get; set; }
        public string SignatureAlgorithm { get; set; }
        public AuthRequest AuthRequest { get; set; }
    }
}
