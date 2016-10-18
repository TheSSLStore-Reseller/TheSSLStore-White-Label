using Newtonsoft.Json;
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

            var json = JsonConvert.SerializeObject(request);
            string strResponse = ApiHelper.GetResponseFromAPI(json, "ssltools/certdecoder/");
            Response = JsonConvert.DeserializeObject<CertificateDecodeResponse>(strResponse);

            return Response;
        }
        public static CSRDecodeResponse DecodeCSRDetail(CSRDecodeRequest csrdetails)
        {
            CSRDecodeResponse Response = new CSRDecodeResponse();

            var json = JsonConvert.SerializeObject(csrdetails);
            string strResponse = ApiHelper.GetResponseFromAPI(json, "ssltools/csrdecoder/");


            Response = JsonConvert.DeserializeObject<CSRDecodeResponse>(strResponse);

            return Response;
        }
        public static CSRGenerateResponse CsrGenerator(CSRGenerateRequest csrdetails)
        {
            CSRGenerateResponse Response = new CSRGenerateResponse();

            var json = JsonConvert.SerializeObject(csrdetails);
            string strResponse = ApiHelper.GetResponseFromAPI(json, "ssltools/csrgenerator/");
            Response = JsonConvert.DeserializeObject<CSRGenerateResponse>(strResponse);

            return Response;
        }

        public static SSLConvertorResponse SSLConvertor(SSLConvertorRequest req)
        {
            SSLConvertorResponse Response = new SSLConvertorResponse();

            var json = JsonConvert.SerializeObject(req);
            string strResponse = ApiHelper.GetResponseFromAPI(json, "ssltools/sslconverter/");

            Response = JsonConvert.DeserializeObject<SSLConvertorResponse>(strResponse);

            return Response;
        }
        public static SSLCheckerResponse CheckSSLCertificate(SSLCheckerRequest Req)
        {
            SSLCheckerResponse Response = new SSLCheckerResponse();

            Req.HostName = Req.HostName.Trim();
            var json = JsonConvert.SerializeObject(Req);
            string strResponse = ApiHelper.GetResponseFromAPI(json, "ssltools/sslchecker/");

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore

            };
            Response = JsonConvert.DeserializeObject<SSLCheckerResponse>(strResponse, settings);

            return Response;
        }

        public static WhyNoPadLockResponse WhyNoPadLock(WhyNoPadLockRequest csrdetails)
        {
            WhyNoPadLockResponse Response = new WhyNoPadLockResponse();

            var json = JsonConvert.SerializeObject(csrdetails);
            string strResponse = ApiHelper.GetResponseFromAPI(json, "ssltools/whynopadlock/");

            Response = JsonConvert.DeserializeObject<WhyNoPadLockResponse>(strResponse);

            return Response;
        }
        public static KeyMatcherresponse KeyMatcher(KeyMatcherRequest csrdetails)
        {
            KeyMatcherresponse Response = new KeyMatcherresponse();

            var json = JsonConvert.SerializeObject(csrdetails);
            string strResponse = ApiHelper.GetResponseFromAPI(json, "ssltools/certkeymatcher/");

            Response = JsonConvert.DeserializeObject<KeyMatcherresponse>(strResponse);

            return Response;
        }


    }
}
