using System;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Configuration;
using System.Collections;


namespace WBSSLStore.Gateways.RestAPIModels.Helpers
{
    public class ApiHelper
    {
        private static Hashtable haWBProductCode = null;
        private static Hashtable hsProdcode = null;
        public static string GetResponseFromAPI(string strRequest, string Url, string ContentType = "application/json", string method = "POST")
        {
            string strData = string.Empty;
            string strError = string.Empty;
            try
            {
                ASCIIEncoding enc = new ASCIIEncoding();
                byte[] data = enc.GetBytes(strRequest);

                Url = ConfigurationManager.AppSettings["RestAPIURL"] + Url;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
                req.Method = method;

                if (!Url.Contains("/health/status/"))
                {
                    req.ContentLength = data.Length;
                    req.ContentType = ContentType;
                    req.Timeout = Int32.MaxValue;
                    Stream reqStream = req.GetRequestStream();
                    reqStream.Write(data, 0, data.Length);
                    reqStream.Close();
                }

                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                if (req.HaveResponse)
                {
                    if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Accepted)
                    {
                        using (StreamReader streamIn = new StreamReader(resp.GetResponseStream()))
                        {
                            strData = Url.Contains("ssltools") ? streamIn.ReadToEnd() : HttpUtility.UrlDecode(streamIn.ReadToEnd());
                        }
                    }
                    else
                        throw new Exception("Request failed: " + resp.StatusDescription);
                }
                resp.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return strData;
        }
        public static string GetInternalProductCode(string key)
        {
            if (haWBProductCode == null)
            {
                haWBProductCode = new Hashtable();
                haWBProductCode.Add("securesiteproev", "securesiteproev");
                haWBProductCode.Add("securesiteev", "securesiteev");
                haWBProductCode.Add("securesitepro", "securesitepro");
                haWBProductCode.Add("securesite", "securesite");
                haWBProductCode.Add("verisigncsc", "verisigncsc");
                haWBProductCode.Add("trustsealorg", "trustsealorg");
                haWBProductCode.Add("quicksslpremium", "quicksslpremium");
                haWBProductCode.Add("truebusinessidev", "truebusinessidev");
                haWBProductCode.Add("truebizid", "truebizid");
                haWBProductCode.Add("truebizidmd", "truebizidmd");
                haWBProductCode.Add("truebusinessidwildcard", "truebusinessidwildcard");
                haWBProductCode.Add("truebusinessidevmd", "truebusinessidevmd");
                haWBProductCode.Add("malwarescan", "malwarescan");
                haWBProductCode.Add("truebusinessidev_cu", "truebusinessidev_cu");
                haWBProductCode.Add("sslwebserverev", "sslwebserverev");
                haWBProductCode.Add("ssl123", "ssl123");
                haWBProductCode.Add("sslwebserver", "sslwebserver");
                haWBProductCode.Add("sgcsupercerts", "sgcsupercerts");
                haWBProductCode.Add("sslwebserverwildcard", "sslwebserverwildcard");
                haWBProductCode.Add("thawtecsc", "thawtecsc");
                haWBProductCode.Add("rapidssl", "rapidssl");
                haWBProductCode.Add("rapidsslwildcard", "rapidsslwildcard");
                haWBProductCode.Add("rapidssl_cu", "rapidssl_cu");
                haWBProductCode.Add("freessl", "freessl");
                haWBProductCode.Add("essentialssl", "301");
                haWBProductCode.Add("comodoevssl", "337");
                haWBProductCode.Add("comodomdc", "335");
                haWBProductCode.Add("essentialwildcard", "343");
                haWBProductCode.Add("instantssl", "24");
                haWBProductCode.Add("instantsslpro", "34");
                haWBProductCode.Add("comodosgc", "317");
                haWBProductCode.Add("comodosgcwildcard", "323");
                haWBProductCode.Add("comodossl", "488");
                haWBProductCode.Add("hgpcicontrolscan", "346");
                haWBProductCode.Add("comodopremiumssl", "7");
                haWBProductCode.Add("comodoevmdc", "410");
                haWBProductCode.Add("comodoucc", "361");
                haWBProductCode.Add("positivessl", "287");
                haWBProductCode.Add("positivesslwildcard", "289");
                haWBProductCode.Add("comodowildcard", "489");
                haWBProductCode.Add("hackerprooftm", "357");
                haWBProductCode.Add("comodointranetssl", "44");
                haWBProductCode.Add("comodopremiumwildcard", "35");
                haWBProductCode.Add("twpremiumssl", "TW1");
                haWBProductCode.Add("comodocsc", "8");
                haWBProductCode.Add("comodoevsgc", "338");
                haWBProductCode.Add("twenterprise", "TW4");
                haWBProductCode.Add("twpremiumwlidcard", "TW7");
                haWBProductCode.Add("twpremiumev", "TW10");
                haWBProductCode.Add("twcsc", "TW12");
                haWBProductCode.Add("sslplus", "TW18");
                haWBProductCode.Add("plusevssl", "TW21");
                haWBProductCode.Add("twdv", "TW30");
                haWBProductCode.Add("securesitewildcard", "securesitewildcard");
            }

            return haWBProductCode[key] != null ? haWBProductCode[key].ToString() : key;
        }
        public static string GetApiProductCode(string s)
        {
            if (hsProdcode == null)
            {
                 hsProdcode = new Hashtable();

                 hsProdcode.Add("24", "instantssl");
                 hsProdcode.Add("279", "positivemdcssl");
                 hsProdcode.Add("287", "positivessl");
                 hsProdcode.Add("289", "positivesslwildcard");
                 hsProdcode.Add("301", "essentialssl");
                 hsProdcode.Add("317", "comodosgc");
                 hsProdcode.Add("323", "comodosgcwildcard");
                 hsProdcode.Add("335", "comodomdc");
                 hsProdcode.Add("337", "comodoevssl");
                 hsProdcode.Add("338", "comodoevsgc");
                 hsProdcode.Add("34", "instantsslpro");
                 hsProdcode.Add("343", "essentialwildcard");
                 hsProdcode.Add("346", "hgpcicontrolscan");
                 hsProdcode.Add("35", "comodopremiumwildcard");
                 hsProdcode.Add("357", "hackerprooftm");
                 hsProdcode.Add("361", "comodoucc");
                 hsProdcode.Add("410", "comodoevmdc");
                 hsProdcode.Add("44", "comodointranetssl");
                 hsProdcode.Add("488", "comodossl");
                 hsProdcode.Add("489", "comodowildcard");
                 hsProdcode.Add("492", "comododvucc");
                 hsProdcode.Add("62", "elitessl");
                 hsProdcode.Add("7", "comodopremiumssl");
                 hsProdcode.Add("8", "comodocsc");
                 hsProdcode.Add("freessl", "freessl");
                 hsProdcode.Add("malwarescan", "malwarescan");
                 hsProdcode.Add("quicksslpremium", "quicksslpremium");
                 hsProdcode.Add("quicksslpremiummd", "quicksslpremiummd");
                 hsProdcode.Add("rapidssl", "rapidssl");
                 hsProdcode.Add("rapidssl_cu", "rapidssl_cu");
                 hsProdcode.Add("rapidsslwildcard", "rapidsslwildcard");
                 hsProdcode.Add("securesite", "securesite");
                 hsProdcode.Add("securesiteev", "securesiteev");
                 hsProdcode.Add("securesitepro", "securesitepro");
                 hsProdcode.Add("securesiteproev", "securesiteproev");
                 hsProdcode.Add("securesitewildcard", "securesitewildcard");
                 hsProdcode.Add("sgcsupercerts", "sgcsupercerts");
                 hsProdcode.Add("ssl123", "ssl123");
                 hsProdcode.Add("sslwebserver", "sslwebserver");
                 hsProdcode.Add("sslwebserverev", "sslwebserverev");
                 hsProdcode.Add("sslwebserverwildcard", "sslwebserverwildcard");
                 hsProdcode.Add("thawtecsc", "thawtecsc");
                 hsProdcode.Add("tkpcidss", "tkpcidss");
                 hsProdcode.Add("truebizid", "truebizid");
                 hsProdcode.Add("truebizidmd", "truebizidmd");
                 hsProdcode.Add("truebusinessidev", "truebusinessidev");
                 hsProdcode.Add("truebusinessidev_cu", "truebusinessidev_cu");
                 hsProdcode.Add("truebusinessidevmd", "truebusinessidevmd");
                 hsProdcode.Add("truebusinessidwildcard", "truebusinessidwildcard");
                 hsProdcode.Add("trustsealorg", "trustsealorg");
                 hsProdcode.Add("TW1", "twpremiumssl");
                 hsProdcode.Add("TW10", "twpremiumev");
                 hsProdcode.Add("TW12", "twcsc");                 
                 hsProdcode.Add("TW14", "twemailid");
                 hsProdcode.Add("TW18", "sslplus");                 
                 hsProdcode.Add("TW21", "plusevssl");                 
                 hsProdcode.Add("TW30", "twdv");                 
                 hsProdcode.Add("TW4", "twenterprise");
                 hsProdcode.Add("TW7", "twpremiumwlidcard");                 
                 hsProdcode.Add("verisigncsc", "verisigncsc");
            }

             
            return hsProdcode[s] != null ? hsProdcode[s].ToString () : s ;
        }
        private static string ReadFromFile(string virtualPath)
        {
            TextReader tr = new StreamReader(HostingEnvironment.MapPath("~/content/json/" + virtualPath));
            StringBuilder sbRequest = new StringBuilder(tr.ReadToEnd());
            tr.Close();
            return sbRequest.ToString();
        }
        private static string ReadFromFileScheduler(string virtualPath)
        {
            string dir = System.IO.Directory.GetCurrentDirectory();
            TextReader tr = new StreamReader(System.IO.Directory.GetCurrentDirectory() + "/content/json/" + virtualPath); 
            StringBuilder sbRequest = new StringBuilder(tr.ReadToEnd());
            tr.Close();
            return sbRequest.ToString();
        }

        public static StringBuilder GetRequestRefundRequst()
        {
            return new StringBuilder(ReadFromFile("orderrequest.txt"));
        }
        public static StringBuilder InviteOrderRequst()
        {
            return new StringBuilder(ReadFromFile("inviteorder.txt"));
        }
        public static StringBuilder ProductRequst()
        {
            return new StringBuilder(ReadFromFile("productservices.txt"));
        }
        public static StringBuilder CheckCSRRequst()
        {
            return new StringBuilder(ReadFromFile("checkcsr.txt"));
        }
        public static StringBuilder QueryOrderRequst()
        {
            return new StringBuilder(ReadFromFileScheduler("queryorder.txt"));
        }
        public static StringBuilder ReissuedOrder()
        {
            return new StringBuilder(ReadFromFile("reissueorder.txt"));
        }
        
    }
}
