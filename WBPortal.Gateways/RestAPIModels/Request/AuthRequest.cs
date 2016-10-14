
namespace WBSSLStore.Gateways.RestAPIModels.Request
{
    public class AuthRequest
    {
        public string PartnerCode { get; set; }        
        public string AuthToken { get; set; }
        public string ReplayToken { get; set; }
        public string UserAgent { get; set; }
    }
}