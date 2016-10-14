using WBSSLStore.Gateways.RestAPIModels.Request;
namespace WBSSLStore.Gateways.RestAPIModels.Response
{
 
    public class OrderResponse
    {
        public AuthResponse AuthResponse { get; set; }
        public string PartnerOrderID { get; set; }
        public string CustomOrderID { get; set; }
        public string TheSSLStoreOrderID { get; set; } //Required for eg, for support chat etc from our end.
        public string VendorOrderID { get; set; }
        public string RefundRequestID { get; set; }
        public bool isRefundApproved { get; set; }
        public string TinyOrderLink { get; set; }
        public ApiOrderStatus OrderStatus { get; set; }
        public string OrderAmount { get; set; }
        public string CertificateStartDate { get; set; }
        public string CertificateEndDate { get; set; }
        public string CommonName { get; set; }
        public string DNSNames {get;set;}
        public string State{get;set;}
        public string Country { get; set; }
        public string Locality { get; set; }
        public string Organization { get; set; }
        public string OrganizationalUnit { get; set; }
        public string WebServerType { get; set; }
        public string ApproverEmail { get; set; }
        public string ProductName { get; set; }
        public string TokenCode { get; set; }
        public string TokenID { get; set; }
        public string AuthFileContent { get; set; }
        public string AuthFileName { get; set; }
        public string PollStatus { get; set; } 
        public string PollDate { get; set; } 
        public Contact AdminContact { get; set; }
        public Contact TechnicalContact { get; set; }
        //0 returned in case of "sub-users". Only returned in case of "newOrder/inviteorder"
        public string ReissueSuccessCode { get; set; }

        public OrderResponse()
        {
            AuthResponse = new AuthResponse();
            OrderStatus = new ApiOrderStatus();
            AdminContact = new Contact();
            TechnicalContact = new Contact(); 
        }
    }

    public class ApiOrderStatus
    {
        public bool isTinyOrder { get; set; }
        public bool isTinyOrderClaimed { get; set; }
        public string MajorStatus { get; set; }
        public string MinorStatus { get; set; }
    }
}