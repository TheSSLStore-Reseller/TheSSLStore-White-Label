using System.Web.Script.Serialization;
using WBSSLStore.Gateways.RestAPIModels.Helpers;
using WBSSLStore.Gateways.RestAPIModels.Request;
using WBSSLStore.Gateways.RestAPIModels.Response;


namespace WBSSLStore.Gateways.RestAPIModels.Services
{
    public class FreeSSLToolsService
    {
        public static CertificateDecodeResponse DecodeCertificate(CertificateDecodeRequest request)
        {
            CertificateDecodeResponse Response = new CertificateDecodeResponse();

            var json = new JavaScriptSerializer().Serialize(request);
            string strResponse = ApiHelper.GetResponseFromAPI(json, "ssltools/certdecoder/");

            Response = new JavaScriptSerializer().Deserialize<CertificateDecodeResponse>(strResponse);

            return Response;
        }
        public static CSRDecodeResponse DecodeCSRDetail(CSRDecodeRequest csrdetails)
        {
            CSRDecodeResponse Response = new CSRDecodeResponse();

            var json = new JavaScriptSerializer().Serialize(csrdetails);
            string strResponse = ApiHelper.GetResponseFromAPI(json, "ssltools/csrdecoder/");
            
            
            Response = new JavaScriptSerializer().Deserialize<CSRDecodeResponse>(strResponse);
            
            return Response;
        }
        public static CSRGenerateResponse CsrGenerator(CSRGenerateRequest csrdetails)
        {
            CSRGenerateResponse Response = new CSRGenerateResponse();

            var json = new JavaScriptSerializer().Serialize(csrdetails);
            string strResponse = ApiHelper.GetResponseFromAPI(json, "ssltools/csrgenerator/");

            Response = new JavaScriptSerializer().Deserialize<CSRGenerateResponse>(strResponse);
            
            //JavaScriptSerializer jsse = new JavaScriptSerializer();            
            //DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(CSRGenerateResponse));
            //Response = JsonConvert.DeserializeObject<CSRGenerateResponse>(strResponse);

            return Response;
        }

        public static SSLConvertorResponse SSLConvertor(SSLConvertorRequest req)
        {
            SSLConvertorResponse Response = new SSLConvertorResponse();

            var json = new JavaScriptSerializer().Serialize(req);
            string strResponse = ApiHelper.GetResponseFromAPI(json, "ssltools/sslconverter/");

            Response = new JavaScriptSerializer().Deserialize<SSLConvertorResponse>(strResponse);

            return Response;
        }
        public static SSLCheckerResponse CheckSSLCertificate(SSLCheckerRequest Req)
        {
            SSLCheckerResponse Response = new SSLCheckerResponse();

            Req.HostName = Req.HostName.Trim();
            var json = new JavaScriptSerializer().Serialize(Req);
            string strResponse = ApiHelper.GetResponseFromAPI(json, "ssltools/sslchecker/");
           // strResponse = strResponse.Replace("null", "\"\"");
            Response = new JavaScriptSerializer().Deserialize<SSLCheckerResponse>(strResponse);

            return Response;
        }

        public static WhyNoPadLockResponse WhyNoPadLock(WhyNoPadLockRequest csrdetails)
        {
            WhyNoPadLockResponse Response = new WhyNoPadLockResponse();

            var json = new JavaScriptSerializer().Serialize(csrdetails);
            string strResponse = ApiHelper.GetResponseFromAPI(json, "ssltools/whynopadlock/");

            Response = new JavaScriptSerializer().Deserialize<WhyNoPadLockResponse>(strResponse);

            return Response;
        }
        public static KeyMatcherresponse KeyMatcher(KeyMatcherRequest csrdetails)
        {
            KeyMatcherresponse Response = new KeyMatcherresponse();

            var json = new JavaScriptSerializer().Serialize(csrdetails);
            string strResponse = ApiHelper.GetResponseFromAPI(json, "ssltools/certkeymatcher/");

            Response = new JavaScriptSerializer().Deserialize<KeyMatcherresponse>(strResponse);

            return Response;
        }

        //public static CSRDecodeResponse DecodeCSRDetail(CSRDecodeRequest csrdetails)
        //{
        //    CSRDecodeResponse Response = new CSRDecodeResponse();

        //    var json = new JavaScriptSerializer().Serialize(csrdetails);
        //    string strResponse = ApiHelper.GetResponseFromAPI(json, "ssltools/csrdecoder/");

        //    Response = new JavaScriptSerializer().Deserialize<CSRDecodeResponse>(strResponse);

        //    return Response;
        //}
    }
}
