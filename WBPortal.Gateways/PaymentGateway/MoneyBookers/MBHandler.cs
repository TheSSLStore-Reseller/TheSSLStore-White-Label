using System;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Security.Cryptography;

namespace WBSSLStore.Gateways.PaymentGateway.MoneyBookers
{
    public class MBHandler
    {
        public static string strMoneyBookersURL = "https://www.moneybookers.com/app/payment.pl";

        public MBHandler()
        { }

        public static string GetMBUrl(MBOrderData objRequest)
        {
            string strParam = PrepareMBRequest(objRequest);
            string strResponse = PostDataOnMB(strParam);
            if (!strResponse.StartsWith("Error HTML :"))
                return strMoneyBookersURL + "?sid=" + strResponse;
            else
            {
                //log error
                
                Logger.Logger log = new Logger.Logger();
                log.Log(strResponse, Logger.LogType.ERROR);
                return string.Empty;
            }
        }

        public static void RedirectToMB(MBOrderData objRequest)
        {
            HttpContext.Current.Response.RedirectPermanent(GetMBUrl(objRequest));
        }

        private static string PrepareMBRequest(MBOrderData objRequest)
        {
            StringBuilder sbRequest = new StringBuilder();
            if (objRequest.Amount > 0)
                sbRequest.Append("&amount=" + objRequest.Amount);
            if (!string.IsNullOrEmpty(objRequest.Currency))
                sbRequest.Append("&currency=" + HTTPPOSTEncode(objRequest.Currency));
            if (!string.IsNullOrEmpty(objRequest.Description))
                sbRequest.Append("&detail1_description=" + HTTPPOSTEncode(objRequest.Description));
            if (!string.IsNullOrEmpty(objRequest.DetailText))
                sbRequest.Append("&detail1_text=" + HTTPPOSTEncode(objRequest.DetailText));
            sbRequest.Append("&prepare_only=" + HTTPPOSTEncode(strMoneyBookersURL));

            if (!string.IsNullOrEmpty(objRequest.PayToEmail))
                sbRequest.Append("&pay_to_email=" + HTTPPOSTEncode(objRequest.PayToEmail));
            if (!string.IsNullOrEmpty(objRequest.ReturnURL))
                sbRequest.Append("&return_url=" + HTTPPOSTEncode(objRequest.ReturnURL));
            if (!string.IsNullOrEmpty(objRequest.ReturnURLText))
                sbRequest.Append("&return_url_text=" + objRequest.ReturnURLText);
            else
                sbRequest.Append("&return_url_text=" + HTTPPOSTEncode("Return to " + HttpContext.Current.Request.Url.DnsSafeHost));
            sbRequest.Append("&return_url_target=" + (int)TargetType._Self);
            if (!string.IsNullOrEmpty(objRequest.CancelURL))
                sbRequest.Append("&cancel_url=" + HTTPPOSTEncode(objRequest.CancelURL));
            sbRequest.Append("&cancel_url_target=" + (int)TargetType._Self);

            if (!string.IsNullOrEmpty(objRequest.StatusURL))
                sbRequest.Append("&status_url=" + HTTPPOSTEncode(objRequest.StatusURL));
            if (!string.IsNullOrEmpty(objRequest.NameInSatetment))
                sbRequest.Append("&dynamic_descriptor=" + HTTPPOSTEncode(objRequest.NameInSatetment));
            sbRequest.Append("&language=" + objRequest.Language.ToString());
            if (!string.IsNullOrEmpty(objRequest.ConfirmationNote))
                sbRequest.Append("&confirmation_note=" + HTTPPOSTEncode(objRequest.ConfirmationNote));
            if (!string.IsNullOrEmpty(objRequest.LogoURL))
                sbRequest.Append("&logo_url=" + HTTPPOSTEncode(objRequest.LogoURL));
            if (!string.IsNullOrEmpty(objRequest.PayFromEmail))
                sbRequest.Append("&pay_from_email=" + HTTPPOSTEncode(objRequest.PayFromEmail));

            //Customer details
            if (!string.IsNullOrEmpty(objRequest.Title))
                sbRequest.Append("&title=" + HTTPPOSTEncode(objRequest.Title));
            if (!string.IsNullOrEmpty(objRequest.FirstName))
                sbRequest.Append("&firstname=" + HTTPPOSTEncode(objRequest.FirstName));
            if (!string.IsNullOrEmpty(objRequest.LastName))
                sbRequest.Append("&lastname=" + HTTPPOSTEncode(objRequest.LastName));
            if (!string.IsNullOrEmpty(objRequest.Address))
                sbRequest.Append("&address=" + HTTPPOSTEncode(objRequest.Address));
            if (!string.IsNullOrEmpty(objRequest.Address2))
                sbRequest.Append("&address2=" + HTTPPOSTEncode(objRequest.Address2));
            if (!string.IsNullOrEmpty(objRequest.PhoneNumber))
                sbRequest.Append("&phone_number=" + HTTPPOSTEncode(objRequest.PhoneNumber));
            if (!string.IsNullOrEmpty(objRequest.PostalCode))
                sbRequest.Append("&postal_code=" + HTTPPOSTEncode(objRequest.PostalCode));
            if (!string.IsNullOrEmpty(objRequest.City))
                sbRequest.Append("&city=" + HTTPPOSTEncode(objRequest.City));
            if (!string.IsNullOrEmpty(objRequest.State))
                sbRequest.Append("&state=" + HTTPPOSTEncode(objRequest.State));
            if (!string.IsNullOrEmpty(objRequest.Country))
                sbRequest.Append("&country=" + HTTPPOSTEncode(objRequest.Country));

            return sbRequest.ToString();
        }

        private static string PostDataOnMB(string strParam)
        {
            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(strMoneyBookersURL);
            WebReq.Method = "POST";

            byte[] buffer = Encoding.UTF8.GetBytes(strParam);

            WebReq.ContentLength = buffer.Length;
            WebReq.ContentType = "application/x-www-form-urlencoded";

            Stream PostData = WebReq.GetRequestStream();
            PostData.Write(buffer, 0, buffer.Length);
            PostData.Close();

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            PostData = WebResp.GetResponseStream();
            StreamReader reader = new StreamReader(PostData);
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            PostData.Close();
            WebResp.Close();

            string strHeaderSessionID = string.Empty;
            string[] strCookieHeader = WebResp.Headers.Get("Set-Cookie").Split(';');
            if (strCookieHeader != null && strCookieHeader.Length > 0)
                strHeaderSessionID = strCookieHeader[0].Replace("SESSION_ID=", string.Empty);

            if (responseFromServer == strHeaderSessionID)
                return strHeaderSessionID;
            else
                return "Error HTML : " + responseFromServer;
        }

        private static string HTTPPOSTEncode(string strPost)
        {
            strPost = strPost.Replace(@"\", "");
            strPost = System.Web.HttpUtility.UrlEncode(strPost);
            strPost = strPost.Replace("%2f", "/");
            return strPost;
        }

        public static MBResultData Results(string strSecretWord)
        {
            MBResultData result = new MBResultData();
            Logger.Logger logger = new Logger.Logger();
            
            logger.Log("Moneybookers IPN called.", Logger.LogType.INFO);

            logger.Log("Moneybookers response is : " + System.Web.HttpContext.Current.Request.RawUrl, Logger.LogType.INFO);
            logger.Log("HTTP_REFERER is : " + System.Web.HttpContext.Current.Request.Headers["HTTP_REFERER"], Logger.LogType.INFO);

            foreach (string strName in System.Web.HttpContext.Current.Request.Form)
            {
                string strValue = System.Web.HttpContext.Current.Request.Form[strName];
                switch (strName)
                {
                    case "pay_to_email":
                        result.PayToEmail = strValue;
                        break;
                    case "pay_from_email":
                        result.PayFromEmail = strValue;
                        break;
                    case "merchant_id":
                        result.MerchantID = strValue;
                        break;
                    case "customer_id":
                        result.CustomerID = strValue;
                        break;
                    case "transaction_id":
                        result.TransactionID = strValue;
                        break;
                    case "mb_transaction_id":
                        result.MBTransactionID = strValue;
                        break;
                    case "mb_amount":
                        result.MBAmount = Convert.ToDecimal(strValue);
                        break;
                    case "mb_currency":
                        result.MBCurrency = strValue;
                        break;
                    case "status":
                        result.Status = (MBResponseStatus)Convert.ToInt16(strValue);
                        break;
                    case "failed_reason_code":
                        result.FailedReasonCode = strValue;
                        break;
                    case "md5sig":
                        result.MD5sig = strValue;
                        break;
                    case "sha2sig":
                        result.SHA2sig = strValue;
                        break;
                    case "amount":
                        result.Amount = Convert.ToDecimal(strValue);
                        break;
                    case "currency":
                        result.Currency = strValue;
                        break;
                    case "payment_type":
                        result.PaymentType = strValue;
                        break;
                }
            }

            string strMD5Concating = string.Empty;
            if (!string.IsNullOrEmpty(strSecretWord))
            {
                strSecretWord = getMd5Hash(strSecretWord).ToUpper();
                strMD5Concating = getMd5Hash(result.MerchantID + result.TransactionID + strSecretWord + result.MBAmount + result.MBCurrency + ((int)result.Status)).ToUpper();
                logger.Log("Our MD5 is : " + strMD5Concating + " and Moneybookers MD5 is : " + result.MD5sig, Logger.LogType.INFO);
            }

            if (strMD5Concating != result.MD5sig && !string.IsNullOrEmpty(strSecretWord))
            {
                logger.Log("Moneybookers IPN request is tempered by anybody. So transaction return as fail.", Logger.LogType.CRITICAL);
                result.Status = MBResponseStatus.Failed;
            }
            return result;
        }

        private static string getMd5Hash(string input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }

    public class MBOrderData
    {
        /// <summary>
        /// Email address of the Merchant’s moneybookers.com account.
        /// </summary>
        public string PayToEmail
        { get; set; }
        /// <summary>
        /// A description of the Merchant, which will be shown on the gateway. If no value is submitted, the pay_to_email value will 
        /// be shown as the recipient of the payment. (Max 30 characters)
        /// </summary>
        public string RecipientDescription
        { get; set; }
        /// <summary>
        /// Reference or identification number provided by the Merchant. MUST be unique for each payment (Max 32 characters)
        /// </summary>
        public string TransactionID
        { get; set; }
        /// <summary>
        /// URL to which the customer will be returned when the payment is made. If this field is not filled, the gateway window will 
        /// simply close automatically at the end of the transaction, so that the customer will be returned to the last page on the 
        /// Merchant's website where he has been before. A secure return_url functionality is available.
        /// </summary>
        public string ReturnURL
        { get; set; }
        /// <summary>
        /// The text on the button when the user finishes his payment.
        /// </summary>
        public string ReturnURLText
        { get; set; }
        /// <summary>
        /// Specifies a target in which the return_url value will be called upon successful payment from customer. 
        /// Default value is 1.
        /// </summary>
        //private int ReturnURLTarget
        //{
        //    get
        //    {
        //        return (int)TargetType._Self;
        //    }
        //}
        /// <summary>
        /// URL to which the customer will be returned if the payment process is cancelled. If this field is not filled, 
        /// the gateway window will simply close automatically upon clicking the cancellation button, so the customer will be 
        /// returned to the last page on the Merchant's website where the customer has been before.
        /// </summary>
        public string CancelURL
        { get; set; }
        ///// <summary>
        ///// Specifies a target in which the cancel_url value will be called upon cancellation of payment from customer. Default value is 1.
        ///// </summary>
        //public string CancelURLTarget
        //{ get; set; }
        /// <summary>
        /// URL to which the transaction details will be posted after the payment process is complete. Alternatively,
        /// you may specify an email address to which you would like to receive the results.
        /// If the status_url is omitted, no transaction details will be sent to the Merchant.
        /// </summary>
        public string StatusURL
        { get; set; }
        /// <summary>
        /// Second URL to which the transaction details will be posted after the payment process is complete. 
        /// Alternatively you may specify an email address to which you would like to receive the results.
        /// </summary>
        public string StatusURL2
        { get; set; }
        /// <summary>
        /// 2-letter code of the language used for Skrill (Moneybookers) pages. 
        /// Can be any of EN, DE, ES, FR, IT, PL, GR RO, RU, TR, CN, CZ, NL, DA, SV or FI.
        /// </summary>
        public LanguageTye Language
        { get; set; }
        /// <summary>
        /// Merchant may show to the customer on the confirmation screen - the end step of the process - a note,confirmation number, PIN or any other message. 
        /// Line breaks BR may be used for longer messages.        
        /// </summary>
        public string ConfirmationNote
        { get; set; }
        /// <summary>
        /// The URL of the logo which you would like to appear at the top of the gateway. 
        /// The logo must be accessible via HTTPS otherwise it will not be shown. 
        /// For best integration results we recommend that Merchants use logos with dimensions up to 200px in width and 50px in height.
        /// </summary>
        public string LogoURL
        { get; set; }
        /// <summary>
        /// Forces only SID to be returned without actual page. Useful when using alternative ways to redirect the customer to the gateway. 
        /// See 2.3.2 for a more detailed explanation. Accepted values are 1 and 0.
        /// </summary>
        public string PrepareOnly
        { get; set; }
        /// <summary>
        /// Email address of the customer who is making the payment. If left empty, the customer has to enter his email address himself.
        /// </summary>
        public string PayFromEmail
        { get; set; }
        /// <summary>
        /// Customer’s title. Accepted values: Mr, Mrs or Ms
        /// </summary>
        public string Title
        { get; set; }
        /// <summary>
        /// Customer’s first name
        /// </summary>
        public string FirstName
        { get; set; }
        /// <summary>
        /// Customer’s last name
        /// </summary>
        public string LastName
        { get; set; }
        /// <summary>
        /// Date of birth of the customer. The format is ddmmyyyy. Only numeric values are accepted
        /// </summary>
        public string DateOfBirth
        { get; set; }
        /// <summary>
        /// Customer’s address (e.g. street)
        /// </summary>
        public string Address
        { get; set; }
        /// <summary>
        /// Customer’s address (e.g. town)
        /// </summary>
        public string Address2
        { get; set; }
        /// <summary>
        /// Customer’s phone number. Only numeric values are accepted
        /// </summary>
        public string PhoneNumber
        { get; set; }
        /// <summary>
        /// Customer’s postal code/ZIP Code. Only alphanumeric values are accepted (no punctuation marks etc.)
        /// </summary>
        public string PostalCode
        { get; set; }
        /// <summary>
        /// Customer’s City
        /// </summary>
        public string City
        { get; set; }
        /// <summary>
        /// state
        /// </summary>
        public string State
        { get; set; }
        /// <summary>
        /// Customer’s country in the 3-digit ISO Code (see Annex II for a list of allowed codes).
        /// </summary>
        public string Country
        { get; set; }
        /// <summary>
        /// The total amount payable. Please note that you should skip the trailing zeroes in case the amount is a natural number
        /// </summary>
        public decimal Amount
        { get; set; }
        /// <summary>
        /// 3-letter code of the currency of the amount according to ISO 4217 (see Annex I for accepted currencies)
        /// </summary>
        public string Currency
        { get; set; }
        /// <summary>
        /// Merchant may show up to 5 details about the product or transfer in the ’More information’ section in the header of the gateway.
        /// </summary>
        public string Description
        { get; set; }
        /// <summary>
        /// The detailX_text is shown next to the detailX_description. 
        /// The detail1_text is also shown to the client in his history at Skrill (Moneybookers)’ website.
        /// </summary>
        public string DetailText
        { get; set; }

        /// <summary>
        /// Merchant must have to activate this facility in their moneybookers account.
        /// Merchant name to be shown on the customer’s bank account statement. 
        /// The value can contain only alphanumeric characters. Maximum length is 100 characters.
        /// </summary>
        public string NameInSatetment
        { get; set; }

        public bool IsTestMode
        { get; set; }
    }

    public class MBResultData
    {
        /// <summary>
        /// Merchants email address.
        /// </summary>
        public string PayToEmail
        { get; set; }
        /// <summary>
        /// Email address of the customer who is making the payment, i.e. sending the money.
        /// </summary>
        public string PayFromEmail
        { get; set; }
        /// <summary>
        /// Unique ID for the Merchant’s moneybookers.com account. ONLY needed for the calculation of the MD5 signature (see Annex III)
        /// </summary>
        public string MerchantID
        { get; set; }
        /// <summary>
        /// Unique ID for the customer’s moneybookers.com account.
        /// </summary>
        public string CustomerID
        { get; set; }
        /// <summary>
        /// Reference or identification number provided by the Merchant.
        /// </summary>
        public string TransactionID
        { get; set; }
        /// <summary>
        /// Moneybookers' unique transaction ID for the transfer.
        /// </summary>
        public string MBTransactionID
        { get; set; }
        /// <summary>
        /// The total amount of the payment in Merchant's currency.
        /// </summary>
        public decimal MBAmount
        { get; set; }
        /// <summary>
        /// Currency of mb_amount. Will always be the same as the currency of the beneficiary's account at Skrill (Moneybookers).
        /// </summary>
        public string MBCurrency
        { get; set; }
        /// <summary>
        /// Status of the transaction: -2 failed / 2 processed / 0 pending / -1 cancelled (see detailed explanation below)
        /// </summary>
        public MBResponseStatus Status
        { get; set; }
        /// <summary>
        /// If the transaction is with status -2 (failed), this field will contain a code detailing the reason for the failure.
        /// </summary>
        public string FailedReasonCode
        { get; set; }
        /// <summary>
        /// MD5 signature (see Annex III)
        /// </summary>
        public string MD5sig
        { get; set; }
        /// <summary>
        /// To get this param from Moneybookers merchant has to contact Moneybookers via email.
        /// SHA2 signature (See Annex IV)
        /// </summary>
        public string SHA2sig
        { get; set; }
        /// <summary>
        /// Amount of the payment as posted by the Merchant on the entry form.
        /// </summary>
        public decimal Amount
        { get; set; }
        /// <summary>
        /// Currency of the payment as posted by the Merchant on the entry form
        /// </summary>
        public string Currency
        { get; set; }
        /// <summary>
        /// To get this param from Moneybookers merchant has to contact Moneybookers via email.
        /// The payment instrument used by the customer on the Gateway. The Merchant can choose to receive:
        /// - Consolidated values (only the type of the instrument, e.g. MBD - MB Direct, WLT - e-wallet or PBT -pending bank transfer)
        /// - Detailed values (the specific instrument used, e.g. VSA - Visa card, GIR – Giropay, etc.
        /// </summary>
        public string PaymentType
        { get; set; }
    }

    public enum TargetType
    {
        _Top = 1,
        _Parent = 2,
        _Self = 3,
        _Blank = 4
    }

    public enum MBResponseStatus
    {
        /// <summary>
        /// This status is sent when the customers pays via the pending bank transfer option. Such transactions will auto-process 
        /// IF the bank transfer is received by Skrill (Moneybookers). We strongly recommend that you do NOT process the order/transaction 
        /// in your system upon receipt of a pending status from Skrill (Moneybookers).
        /// </summary>
        Pending = 0,
        /// <summary>
        /// Pending transactions can either be cancelled manually by the sender in their online account history 
        /// or they will auto-cancel after 14 days if still pending.
        /// </summary>
        Cancelled = -1,
        /// <summary>
        /// This status is sent when the customer tries to pay via Credit Card or Direct Debit but our provider declines the transaction. 
        /// If you do not accept Credit Card or Direct Debit payments via Skrill (Moneybookers) (see page 16) then you will never receive the failed status.
        /// </summary>
        Failed = -2,
        /// <summary>
        /// This status could be received only if your account is configured to receive chargebacks. 
        /// If this is the case, whenever a chargeback is received by Skrill (Moneybookers), 
        /// a -3 status will be posted on the status_url for the reversed transaction.
        /// </summary>
        Chargeback = -3,
        /// <summary>
        /// This status is sent when the transaction is processed and the funds have been received on your Skrill (Moneybookers) account.
        /// </summary>
        Processed = 2

    }

    public enum LanguageTye
    {
        EN, DE, ES, FR, IT, PL, GR, RO, RU, TR, CN, CZ, NL, DA, SV, FI
    }
}
