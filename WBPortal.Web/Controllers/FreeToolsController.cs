using System;
using System.Web.Mvc;
using WBSSLStore.Web.Helpers.Caching;
using WBSSLStore.Gateways.RestAPIModels.Response;
using WBSSLStore.Gateways.RestAPIModels.Services;
using WBSSLStore.Gateways.RestAPIModels.Request;

namespace WBSSLStore.Web.Controllers
{
    [HandleError]
    public class FreeToolsController : Controller
    {

        [Route("freetools/csrgenerator", Name = "csrGen_us")]
        public ActionResult csrgenerator()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("freetools/csrgenerator", Name = "csrGenpost_us")]
        public ActionResult csrgenerator(FormCollection collection)
        {

            CSRGenerateRequest req = new CSRGenerateRequest();
            req.AuthRequest = GetAuthrequest();
            req.CommonName = Request.Form["generate_csr_cn"].Trim();
            req.Country = Request.Form["generate_csr_c"].Trim();
            req.Email = Request.Form["generate_csr_email"].Trim();
            req.KeySize = Request.Form["generate_csr_key"].Trim();
            req.Locality = Request.Form["generate_csr_l"].Trim();
            req.OrganizationName = Request.Form["generate_csr_o"].Trim();
            req.OrganizationUnit = Request.Form["generate_csr_ou"].Trim();
            req.SignatureAlgorithm = Request.Form["generate_csr_algorithm"].Trim();
            req.State = Request.Form["generate_csr_st"].Trim();

            CSRGenerateResponse objRestResponse = FreeSSLToolsService.CsrGenerator(req);

            return View(objRestResponse);
        }

        [Route("freetools/certkeymatcher", Name = "keymatcher_us")]
        public ActionResult certkeymatcher()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("freetools/certkeymatcher", Name = "keymatcherpost_us")]
        public ActionResult certkeymatcher(FormCollection collection)
        {
            KeyMatcherRequest req = new KeyMatcherRequest();
            KeyMatcherresponse objRestResponse = null;
            bool iscsr = Convert.ToString(Request.Form["hdnType"]).Equals("CSR", StringComparison.OrdinalIgnoreCase);

            if (iscsr && (string.IsNullOrEmpty(Request.Form["txtsslcert"]) || string.IsNullOrEmpty(Request.Form["txtprivatekey"])))
            {
                objRestResponse = new KeyMatcherresponse();
                objRestResponse.AuthResponse = new AuthResponse();
                objRestResponse.AuthResponse.isError = true;
                objRestResponse.AuthResponse.Message = new string[1] { iscsr ? "Please insert SSL certificate and CSR!" : "Please insert SSL certificate and Private key!" };
                return View(objRestResponse);
            }

            req.AuthRequest = GetAuthrequest();
            req.Certificate = Request.Form["txtsslcert"].Trim();
            req.CSR = iscsr ? Request.Form["txtprivatekey"].Trim() : "";
            req.PrivateKey = !iscsr ? Convert.ToString(Request.Form["txtprivatekey"]).Trim() : "";
            objRestResponse = FreeSSLToolsService.KeyMatcher(req);



            return View(objRestResponse);
        }

        [Route("freetools/sslconvertor", Name = "sslconvertor_us")]
        public ActionResult sslconvertor()
        {
            return View();
        }
        [HttpPost]
        [Route("freetools/sslconvertor", Name = "sslconvertorpost_us")]
        public ActionResult sslconvertor(FormCollection collection)
        {
            SSLConvertorRequest req = new SSLConvertorRequest();
            req.AuthRequest = GetAuthrequest();
            req.Certificate = Request.Form["CommonName"].Trim();
            req.ConvertFrom = Request.Form["Country"].Trim();
            req.ConvertTo = Request.Form["Email"].Trim();
            req.IntermediatesCA = Request.Form["IntermediatesCA"].Trim();
            req.KeyPassword = Request.Form["KeyPassword"].Trim();
            req.PrivateKey = Request.Form["PrivateKey"].Trim();
            req.RootCA = Request.Form["RootCA"].Trim();


            SSLConvertorResponse objRestResponse = FreeSSLToolsService.SSLConvertor(req);

            return View(objRestResponse);

        }

        [Route("freetools/whynopadlock", Name = "whynopadlock_us")]
        public ActionResult whynopadlock()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("freetools/whynopadlock", Name = "whynopadlockpost_us")]
        public ActionResult whynopadlock(FormCollection collection)
        {
            WhyNoPadLockResponse objRestResponse = null;
            if (!string.IsNullOrEmpty(Request.Form["txtUrl"]))
            {
                WhyNoPadLockRequest req = new WhyNoPadLockRequest();
                req.AuthRequest = GetAuthrequest();
                req.URL = Request.Form["txtUrl"].Trim();

                objRestResponse = FreeSSLToolsService.WhyNoPadLock(req);
            }
            else
            {
                objRestResponse = new WhyNoPadLockResponse();
                objRestResponse.AuthResponse = new AuthResponse();
                objRestResponse.AuthResponse.isError = true;
                objRestResponse.AuthResponse.Message = new string[1] { "Please insert Secure URL!" };
            }
            return View(objRestResponse);
        }

        [HttpGet]
        [Route("freetools/CertificateDecoder", Name = "CertificateDecoder_us")]
        public ActionResult CertificateDecoder()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("freetools/CertificateDecoder", Name = "CertificateDecoderpost_us")]
        public ActionResult CertificateDecoder(FormCollection collection)
        {
            CertificateDecodeResponse objRestResponse = null;
            if (!string.IsNullOrEmpty(Request.Form["txtCertificate"]))
            {
                CertificateDecodeRequest req = new CertificateDecodeRequest();
                req.AuthRequest = GetAuthrequest();
                req.Certificate = Request.Form["txtCertificate"].Trim();
                objRestResponse = FreeSSLToolsService.DecodeCertificate(req);
            }
            else
            {
                objRestResponse = new CertificateDecodeResponse();
                objRestResponse.AuthResponse = new AuthResponse();
                objRestResponse.AuthResponse.isError = true;
                objRestResponse.AuthResponse.Message = new string[1] { "Please insert Certificate!" };

            }
            return View(objRestResponse);
        }

        [HttpGet]
        [Route("freetools/CheckCSR", Name = "CheckCSR_us")]
        public ActionResult CheckCSR()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("freetools/CheckCSR", Name = "CheckCSRpost_us")]
        public ActionResult CheckCSR(FormCollection collection)
        {
            string CSR = Request.Form["txtCSR"];
            CSRDecodeResponse objRestResponse = null;
            if (!string.IsNullOrEmpty(CSR))
            {

                CSRDecodeRequest request = new CSRDecodeRequest();
                request.AuthRequest = GetAuthrequest();
                request.CSR = CSR.Trim().Replace("\r\n", "\n");
                objRestResponse = FreeSSLToolsService.DecodeCSRDetail(request);

            }
            else
            {
                objRestResponse = new CSRDecodeResponse();
                objRestResponse.AuthResponse = new AuthResponse();
                objRestResponse.AuthResponse.isError = true;
                objRestResponse.AuthResponse.Message = new string[1] { "CSR can not be blank!" };
            }
            return View(objRestResponse);
        }

        [HttpGet]
        [Route("freetools/CheckSSLCertificate", Name = "CheckSSLCertificate_us")]
        public ActionResult CheckSSLCertificate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("freetools/CheckSSLCertificate", Name = "CheckSSLCertificatepost_us")]
        public ActionResult CheckSSLCertificate(FormCollection collection)
        {
            SSLCheckerResponse objRestResponse = null;
            if (!string.IsNullOrEmpty(Request.Form["txtDomain"]))
            {
                SSLCheckerRequest req = new SSLCheckerRequest();
                req.AuthRequest = GetAuthrequest();
                req.HostName = Request.Form["txtDomain"].Trim();

                objRestResponse = FreeSSLToolsService.CheckSSLCertificate(req);

            }
            else
            {
                objRestResponse = new SSLCheckerResponse();
                objRestResponse.AuthResponse = new AuthResponse();
                objRestResponse.AuthResponse.isError = true;
                objRestResponse.AuthResponse.Message = new string[1] { "Server Hostname can not be blank!" };
            }
            return View(objRestResponse);
        }


        private AuthRequest GetAuthrequest()
        {
            Domain.Site s = SiteCacher.CurrentSite;

            AuthRequest AuthRequest = new AuthRequest();
            AuthRequest.PartnerCode = s.APIPartnerCode;
            AuthRequest.AuthToken = s.APIAuthToken;
            AuthRequest.UserAgent = s.Alias;
            AuthRequest.ReplayToken = "SSL Store WhiteBrand Sites : " + s.Alias;

            return AuthRequest;
        }
    }
}
