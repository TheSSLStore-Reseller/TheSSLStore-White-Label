using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace WBSSLStore.Gateway.AuthorizeNET
{
    public enum TransactionType
    {
        AUTH_CAPTURE = 1,
        CREDIT,
        VOID,
        AUTH
    }
    public class AuthoriseNetTransaction
    {
        #region Properties

        //Added By Kaushak And Atul for Frude dections
        public int StatusCode;

        public string CreditCardNumber;
        public string CCV;
        public string ExpireMonthYear;
        public decimal Amount;
        public string sFirstName;
        public string sLastName;
        public string sBillName;
        public string sBillAddress;
        public string sBillCity;
        public string sBillState;
        public string sBillZIP;
        public string sBillCountry;
        public string sBillPhone;
        public string sBillEmail;
        public string sBillCompany;
        public string OrderID;
        public string OrderOption;
        public string CustomerIP;
        public string RefTransactionId;
        public string RefAuthorizationCode;
        public string ErrorMessage;
        public System.Collections.Specialized.NameValueCollection objCCInf;
        public string[] objCCRetVals;
        public System.Collections.Specialized.NameValueCollection objVoidInf;
        public string[] objVoidRetVals;

        // for E-Check.
        public string RoutingNumber;
        public string BankAccountNumber;
        public string AccountType;
        public string BankName;
        public string BankAccountName;

        public bool isTestMode;
        private string AuthNetVersion = "3.1"; // Contains CCV support
        public string AuthNetLoginID;
        public string AuthNetPassword;
        public string AuthNetTransKey;
        public string AuthNetURL;
        public string AuthNeTestURL;
        public string InvoiceNumber;
        public string TransactionReference;
        public string EcheckType;

        #endregion

        #region Methods

        public AuthoriseNetTransaction()
        {

        }

        /// <summary>
        ///		Process online payment transaction using provided data.
        /// </summary>
        /// <returns>Returns True if transaction successfull, otherwise returns False.</returns>

        public bool Process(TransactionType Trans)
        {
            WebClient objRequest = new WebClient();

            byte[] objRetBytes;

            try
            {
                // Set attributes & their values
                objCCInf = SetAttributes(Trans);
                objRequest.BaseAddress = AuthNetURL;

                objRetBytes = objRequest.UploadValues(objRequest.BaseAddress, "POST", objCCInf);
                objCCRetVals = System.Text.Encoding.ASCII.GetString(objRetBytes).Split(",".ToCharArray());
               
                if (objCCRetVals != null && objCCRetVals.Length > 0)
                {
                    StatusCode = Convert.ToInt32(objCCRetVals[0].Trim(char.Parse("|")));
                    RefTransactionId = objCCRetVals[6].Trim(char.Parse("|"));
                }
                else
                    StatusCode = 0;

                if (objCCRetVals[0].Trim(char.Parse("|")) == "1")
                {
                    // Returned Authorisation Code
                    RefAuthorizationCode = objCCRetVals[4].Trim(char.Parse("|"));
                    // Returned Transaction ID
                    RefTransactionId = objCCRetVals[6].Trim(char.Parse("|"));

                    return true;
                }
                else
                {
                    // Error!
                    ErrorMessage = objCCRetVals[3].Trim(char.Parse("|")) + " (" +
                        objCCRetVals[2].Trim(char.Parse("|")) + ")";


                    if (objCCRetVals[2].Trim(char.Parse("|")) == "44")
                    {
                        // CCV transaction decline
                        ErrorMessage += "Our Card Code Verification (CCV) returned " +
                            "the following error: ";

                        switch (objCCRetVals[38].Trim(char.Parse("|")))
                        {
                            case "N":
                                ErrorMessage += "Card Code does not match.";
                                break;
                            case "P":
                                ErrorMessage += "Card Code was not processed.";
                                break;
                            case "S":
                                ErrorMessage += "Card Code should be on card but was not indicated.";
                                break;
                            case "U":
                                ErrorMessage += "Issuer was not certified for Card Code.";
                                break;
                        }
                    }

                    if (objCCRetVals[2].Trim(char.Parse("|")) == "45")
                    {
                        if (ErrorMessage.Length > 1)
                            ErrorMessage += "<br />n";

                        // AVS transaction decline
                        ErrorMessage += "Our Address Verification System (AVS) " +
                            "returned the following error: ";

                        switch (objCCRetVals[5].Trim(char.Parse("|")))
                        {
                            case "A":
                                ErrorMessage += " the zip code entered does not match " +
                                    "the billing address.";
                                break;
                            case "B":
                                ErrorMessage += " no information was provided for the AVS check.";
                                break;
                            case "E":
                                ErrorMessage += " a general error occurred in the AVS system.";
                                break;
                            case "G":
                                ErrorMessage += " the credit card was issued by a non-US bank.";
                                break;
                            case "N":
                                ErrorMessage += " neither the entered street address nor zip " +
                                    "code matches the billing address.";
                                break;
                            case "P":
                                ErrorMessage += " AVS is not applicable for this transaction.";
                                break;
                            case "R":
                                ErrorMessage += " please retry the transaction; the AVS system " +
                                    "was unavailable or timed out.";
                                break;
                            case "S":
                                ErrorMessage += " the AVS service is not supported by your " +
                                    "credit card issuer.";
                                break;
                            case "U":
                                ErrorMessage += " address information is unavailable for the " +
                                    "credit card.";
                                break;
                            case "W":
                                ErrorMessage += " the 9 digit zip code matches, but the " +
                                    "street address does not.";
                                break;
                            case "Z":
                                ErrorMessage += " the zip code matches, but the address does not.";
                                break;
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "System error occured while transaction with Authorize.Net - " + ex.ToString();
                return false;
            }
        }

        /// <summary>
        ///		Return NameValueCollection with Attributes and their values.
        /// </summary>
        /// <returns>Return NameValueCollection contains Attributes and their values.</returns>

        private System.Collections.Specialized.NameValueCollection SetAttributes(TransactionType Trans)
        {
            System.Collections.Specialized.NameValueCollection objInf =
                new System.Collections.Specialized.NameValueCollection(30);

            switch (Trans)
            {
                case TransactionType.AUTH_CAPTURE:
                    objInf.Add("x_type", "AUTH_CAPTURE");
                    break;
                case TransactionType.VOID:
                    objInf.Add("x_trans_id", TransactionReference);
                    objInf.Add("x_type", "VOID");
                    break;
                case TransactionType.CREDIT:
                    objInf.Add("x_trans_id", TransactionReference);
                    objInf.Add("x_type", "CREDIT");
                    break;
                case TransactionType.AUTH:
                    objInf.Add("x_type", "AUTH_ONLY");
                    break;

            }
            // [ Merchant Account Information ] //
            objInf.Add("x_login", AuthNetLoginID);
            objInf.Add("x_password", AuthNetPassword);
            objInf.Add("x_tran_key", AuthNetTransKey);
            objInf.Add("x_version", AuthNetVersion);
            objInf.Add("x_test_request", isTestMode ? "TRUE" : "FALSE");

            objInf.Add("x_delim_data", "TRUE");
            objInf.Add("x_relay_response", "FALSE");
            objInf.Add("x_delim_char", ",");
            objInf.Add("x_encap_char", "|");


            // [ Card Details ] //
            objInf.Add("x_card_num", CreditCardNumber);
            objInf.Add("x_exp_date", ExpireMonthYear);
            objInf.Add("x_card_code", CCV);
            objInf.Add("x_method", "CC");
                      
            objInf.Add("x_invoice_num", InvoiceNumber);
            objInf.Add("x_amount", Amount.ToString("0.00"));
            
            // Currency setting. Check the guide for other supported currencies
            objInf.Add("x_currency_code", "USD");


            // [ Customer Name and Billing Address ] //
            objInf.Add("x_first_name", sFirstName);
            objInf.Add("x_last_name", string.Empty);
            // Max : 60 Chars
            objInf.Add("x_address", sBillAddress.Length > 60 ? sBillAddress.Substring(0, 59) : sBillAddress);
            objInf.Add("x_city", sBillCity);
            // If passed, the value will be verified.(Any valid two digit state code or full state name)
            objInf.Add("x_state", sBillState);
            objInf.Add("x_zip", sBillZIP);
            objInf.Add("x_country", sBillCountry);
            // Recommended format is (123)123-1234
            objInf.Add("x_phone", sBillPhone);
            objInf.Add("x_Customer_IP", CustomerIP);
            objInf.Add("x_Company", sBillCompany);
            // [ Email Settings ] //
            objInf.Add("x_email", sBillEmail); // Customer email
            objInf.Add("x_email_customer", "FALSE"); // Indicates whether a confirmation email should be sent to the customer.
            return objInf;
        }
        #endregion
    }

}
