using System;
using System.Web.Mvc;
using WBSSLStore.Web.Helpers.Caching;
using WBSSLStore.Gateways.RestAPIModels.Response;
using WBSSLStore.Gateways.RestAPIModels.Services;
using WBSSLStore.Gateways.RestAPIModels.Request;
using Ionic.Zip;
using System.IO;

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

            if(objRestResponse != null && !objRestResponse.AuthResponse.isError && !string.IsNullOrEmpty (objRestResponse.CSR))
            {
                objRestResponse.CSR = System.Web.HttpUtility.HtmlDecode(objRestResponse.CSR);
            }
            
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
        private string ReadPostedFile(System.Web.HttpPostedFileBase file)
        {
            if (file.ContentLength == 0)
                return "";

            BinaryReader b = new BinaryReader(file.InputStream);
            byte[] binData = b.ReadBytes(file.ContentLength);
            b.Dispose();
            return System.Text.Encoding.UTF8.GetString(binData);
        }

        private  string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
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
            req.Certificate = ReadPostedFile(Request.Files["inCertValid"]);
            req.ConvertFrom = Request.Form["group_name"].ToString();
            req.ConvertTo = Request.Form["chn_group_name"];
            req.IntermediatesCA = ReadPostedFile(Request.Files["chain1file"]) + "\n" + ReadPostedFile(Request.Files["chain2file"]);
            req.KeyPassword = Request.Form["pfxkeypass"];
            req.PrivateKey = ReadPostedFile(Request.Files["inprvKeyValid"]);
            req.RootCA = Request.Form["RootCA"];
            SSLConvertorResponse objRestResponse = FreeSSLToolsService.SSLConvertor(req);
            try
            {
                if (!objRestResponse.AuthResponse.isError)
                {
                    string filename = "Certificate." + req.ConvertTo.ToLower();

                    byte[] bytContents = Convert.FromBase64String("");
                    if ((req.ConvertFrom.ToLower().Equals("pem") && req.ConvertTo.ToLower().Equals("der")) || (req.ConvertFrom.ToLower().Equals("pem") && req.ConvertTo.ToLower().Equals("pfx")) || (req.ConvertFrom.ToLower().Equals("p7b") && req.ConvertTo.ToLower().Equals("pfx")))
                    {
                        //fromb64

                        bytContents = Convert.FromBase64String(objRestResponse.Certificate);

                    }
                    else if ((req.ConvertFrom.ToLower().Equals("pem") && req.ConvertTo.ToLower().Equals("p7b")) || (req.ConvertFrom.ToLower().Equals("der") && req.ConvertTo.ToLower().Equals("pem")))
                    {
                        bytContents = Convert.FromBase64String(objRestResponse.Certificate);
                        //noneed from64
                    }
                    else if ((req.ConvertFrom.ToLower().Equals("pfx") && req.ConvertTo.ToLower().Equals("pem")) || (req.ConvertFrom.ToLower().Equals("p7b") && req.ConvertTo.ToLower().Equals("pem")))
                    {
                        //objRestResponse.ChainCertificates .cer
                        //objRestResponse.Certificate .cer
                        //objRestResponse.PrivateKey .key


                        System.IO.MemoryStream ms = new System.IO.MemoryStream();
                        using (ZipFile zip = new ZipFile())
                        {
                            if (!string.IsNullOrEmpty(objRestResponse.Certificate))
                            {
                                zip.AddEntry("Certificate.cer", objRestResponse.Certificate);
                            }
                            if (objRestResponse != null && objRestResponse.ChainCertificates.Count > 0)
                            {
                                int Count = 1;
                                foreach (string CA in objRestResponse.ChainCertificates)
                                {
                                    zip.AddEntry("CACerficate-" + Count.ToString() + ".cer", CA.ToString());
                                    Count++;
                                }
                            }
                            if (!(req.ConvertFrom.ToLower().Equals("p7b") && req.ConvertTo.ToLower().Equals("pem")))
                            {
                                if (!string.IsNullOrEmpty(objRestResponse.PrivateKey))
                                {
                                    zip.AddEntry("PrivateKey.key", objRestResponse.PrivateKey);
                                }
                            }
                            zip.Save(ms);

                            bytContents = ms.ToArray();
                        }

                        return File(bytContents, "application/zip", "Certificate.zip");
                    }

                    return File(bytContents, "application/" + req.ConvertTo.ToLower(), filename);

                }
            }
            catch (Exception ex)
            {
                return View(objRestResponse);
            }

            return View();
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
