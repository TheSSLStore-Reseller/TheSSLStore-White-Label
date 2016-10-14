using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WBSSLStore.Domain
{
    [NotMapped]
    public static class SSLCategories
    {
        public static string StandardSSL
        {
            get
            {
                return "rapidssl,ssl123,quicksslpremium,287,301";
            }
        }
        public static string HighAssurance
        {
            get
            {
                return "securesite,securesitepro,sslwebserver,truebizid,truebusinessidev,24,34,7";
            }
        }
        public static string WildCardSSL
        {
            get
            {
                return "truebusinessidwildcard,sslwebserverwildcard,rapidsslwildcard,343,289,35,TW7";
            }
        }
        public static string SAN
        {
            get
            {
                return "truebizidmd,truebusinessidevmd,335,410,361";
            }
        }
        public static string SGC
        {
            get
            {
                return "securesitepro,sgcsupercerts,323,317,338";
            }
        }
        public static string EV
        {
            get
            {
                return "securesiteproev,securesiteev,truebusinessidev,sslwebserverev,410,337,TW10";
            }
        }
        public static string CodeSigning
        {
            get
            {
                return "verisigncsc,thawtecsc,8";
            }
        }
        public static string TrustSeal
        {
            get
            {
                return "trustsealorg";
            }
        }
        public static string Other
        {
            get
            {
                return "tkpcidss,tw14,tw18,tw21";
            }
        }
        public static string Malware
        {
            get
            {
                return "malwarescan";
            }
        }
    }
}
