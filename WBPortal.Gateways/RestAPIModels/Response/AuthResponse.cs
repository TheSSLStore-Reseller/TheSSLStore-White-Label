namespace WBSSLStore.Gateways.RestAPIModels.Response
{   
    public class AuthResponse
    {
        public bool isError;
        public string[] Message;
        public string Timestamp { get; set; }
        public string ReplayToken { get; set; }
        public string InvokingPartnerCode { get; set; }
    }
}