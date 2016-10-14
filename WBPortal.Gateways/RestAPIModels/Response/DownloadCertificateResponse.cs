using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WBSSLStore.Gateways.RestAPIModels.Response
{
    public class DownloadCertificateResponse
    {
        public AuthResponse AuthResponse { get; set; }
        public string PartnerOrderID { get; set; }
        public string CertificateStartDate { get; set; }
        public string CertificateEndDate { get; set; }

        public string CertificateStatus { get; set; }
        public string ValidationStatus { get; set; }
        public List<Certificate> Certificates { get; set; }

        public DownloadCertificateResponse()
        {
            Certificates = new List<Certificate>();
            AuthResponse = new AuthResponse();
        }
       
    }

    public class DownloadCertificateZipResponse
    {
        public AuthResponse AuthResponse { get; set; }
        public string PartnerOrderID { get; set; }
        public string CertificateStartDate { get; set; }
        public string CertificateEndDate { get; set; }

        public string CertificateStatus { get; set; }
        public string ValidationStatus { get; set; }
        //public List<Certificate> Certificates { get; set; }
        public string Zip { get; set; }
        public DownloadCertificateZipResponse()
        {
           // Certificates = new List<Certificate>();
            AuthResponse = new AuthResponse();
        }

    }


    public class Certificate
    {
        public string FileName { get; set; }
        public string FileContent { get; set; }
    }
}
