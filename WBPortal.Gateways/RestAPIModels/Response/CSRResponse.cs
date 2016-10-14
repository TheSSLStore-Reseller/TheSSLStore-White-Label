using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WBSSLStore.Gateways.RestAPIModels.Response
{
    public class CSRResponse
    {
        public AuthResponse AuthResponse { get; set; }
        public string DominName { get; set; }

        public string[] DNSNames { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string Locality { get; set; }
        public string Organization { get; set; }
        public string OrganisationUnit { get; set; }
        public string OrganizationUnit { get; set; }
        public string State { get; set; }
        public bool hasBadExtensions { get; set; }
        public bool isValidDomainName { get; set; }
        public bool isWildcardCSR { get; set; }
        public string MD5Hash { get; set; }
        public string SHA1Hash { get; set; }

        public string DomainName { get; set; }
        public string SignatureAlgorithm { get; set; }
        public string RegionSpecificOrderIndicator { get; set; }
        public CSRResponse()
        {
            AuthResponse = new AuthResponse();
        }
    }
}
