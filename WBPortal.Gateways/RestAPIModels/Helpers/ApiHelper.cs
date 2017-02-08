using System;
using System.Collections.Generic;
using System.Linq;
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
      
        public static string GetResponseFromAPI(string strRequest, string Url, string ContentType = "application/json", string method = "POST")
        {
            string strData = string.Empty;
            string strError = string.Empty;
            try
            {
                ASCIIEncoding enc = new ASCIIEncoding();
                byte[] data = enc.GetBytes(strRequest);

                Url = ConfigurationManager.AppSettings["RestAPIURL"] + Url;

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

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
                            strData = Url.Contains("ssltools")? streamIn.ReadToEnd() : HttpUtility.UrlDecode(streamIn.ReadToEnd());
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
