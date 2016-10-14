using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace WBSSLStore.Gateway
{
    [Serializable()]
    public class PaymentGatewayInteraction
    {
        GatewayOutParameter _wrapper;

        public GatewayOutParameter TransactionWrapper
        {
            get
            {
                return _wrapper;
            }
            set
            {
                _wrapper = value;
            }
        }
        public GatewayInParameter InParams
        {
            get;
            set;
        }
        public int StatusCode
        {
            get;
            set;
        }


        public PaymentGatewayInteraction()
        {
            _wrapper = new GatewayOutParameter();

        }
        public void Save()
        {

        }
    }
    
    public class GatewayOutParameter
    {
        public string GatewayRequest { get; set; }
        public string GatewayResponse { get; set; }
        public bool isSuccess { get; set; }
        public bool isPaymentTransaction { get; set; }
        public string GatewayAuthCode { get; set; }
        public string GatewayErrorCode { get; set; }
        public int PaymentModeID { get; set; }
        public decimal TransactionAmount { get; set; }
        public string statusCode { get; set; }

        
    }
    public class GatewayInParameter
    {
        //Out params


        //Input Parms
        public string APILoginID
        { get; set; }
        public string APITransactionKey
        { get; set; }
        public string APIPassword
        { get; set; }
        public string APIMerchantid
        { get; set; }
        public string APIKeyFilePath
        { get; set; }
        public string APIPartnerID
        { get; set; }
        public bool IsTestMode
        { get; set; }
        public string APILiveUrl
        {get;set;}
        public string APITestUrl
        { get; set; }
        public string InvoiceNumber
        { get; set; }

        public string  CardType
        {
            get;
            set;
        }
        public string CardFirstName
        {
            get;
            set;
        }
        public string CardLastName
        {
            get;
            set;
        }
        public string CardFullName
        {
            get;
            set;
        }
        public string CCNumber
        {
            get;
            set;
        }
        public int CCExpMonth
        {
            get;
            set;
        }
        public int CCExpYear
        {
            get;
            set;
        }
        public string CVV
        {
            get;
            set;
        }
        public string BillingAddress1
        {
            get;
            set;
        }
        public string BillingAddress2
        {
            get;
            set;
        }
        public string City
        {
            get;
            set;
        }
        public string State
        {
            get;
            set;
        }
        public string ZipCode
        {
            get;
            set;
        }
        public Decimal TransactionAmount
        {
            get;
            set;
        }
        public string CountryName
        {
            get;
            set;
        }
        public string RefTransactionId
        {
            get;
            set;
        }
        public string BillingEmail
        {
            get;
            set;
        }
        public string BillingPhone
        {
            get;
            set;
        }
        public string BillingCompanyName
        {
            get;
            set;
        }
    }
}
