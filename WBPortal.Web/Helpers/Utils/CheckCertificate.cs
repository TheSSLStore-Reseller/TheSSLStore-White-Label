using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.IO;

namespace WBSSLStore.Web.Helpers
{
    class AllAcceptCertificationPolicy : ICertificatePolicy
    {
        public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
        {
            return true;
        }
    }
    //Implement the ICertificatePolicy interface.
    class CertPolicy : ICertificatePolicy
    {
        public bool CheckValidationResult(ServicePoint srvPoint,
    X509Certificate certificate, WebRequest request, int certificateProblem)
        {
            // You can do your own certificate checking.
            // You can obtain the error values from WinError.h.

            // Return true so that any certificate will work with this sample.
            return true;
        }
    }

    public class CheckCertificate
    {
        public static X509Certificate GetCertDetails(string DomainName)
        {
            try
            {
                DomainName = DomainName.StartsWith("*.") ? DomainName.Replace("*.", "www.") : DomainName;
#pragma warning disable CS0618 // 'ServicePointManager.CertificatePolicy' is obsolete: 'CertificatePolicy is obsoleted for this type, please use ServerCertificateValidationCallback instead. http://go.microsoft.com/fwlink/?linkid=14202'
                System.Net.ServicePointManager.CertificatePolicy = new AllAcceptCertificationPolicy();
#pragma warning restore CS0618 // 'ServicePointManager.CertificatePolicy' is obsolete: 'CertificatePolicy is obsoleted for this type, please use ServerCertificateValidationCallback instead. http://go.microsoft.com/fwlink/?linkid=14202'

                

                var objReq =
                    (System.Net.HttpWebRequest)
                    System.Net.WebRequest.Create("https://" + DomainName + "/");

                objReq.Method = "GET"; 
                objReq.AllowAutoRedirect = false;


                var objRes = (System.Net.HttpWebResponse)objReq.GetResponse();
              
                return objReq.ServicePoint.Certificate;
            }
            catch
            {
                return null;
            }

        }

     
    }
}
