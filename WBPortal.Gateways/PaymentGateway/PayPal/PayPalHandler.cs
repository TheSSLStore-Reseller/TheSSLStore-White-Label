using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Net;
using System.IO;
using System.Web;

namespace WBSSLStore.Gateway.PayPal
{
    public class PayPalHandler
    {
        //Singleton Pattern
        public PayPalHandler()
        {
        }

        //General Redirect Params
        public static void SetRedirect(PayPalOrderData order, bool isTestMode)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            System.Web.HttpContext.Current.Response.RedirectPermanent(PaypalURL(order, isTestMode));
        }

        public static string GetPaypalUrl(PayPalOrderData ord, bool isTestMode)
        {
            return PaypalURL(ord, isTestMode);
        }

        private static string PaypalURL(PayPalOrderData ord, bool isTestMode)
        {
            System.Text.StringBuilder sbr = new System.Text.StringBuilder();
            if (isTestMode)
                sbr.AppendFormat("{0}business={1}", ord.PaypalSandoxUrl , HTTPPOSTEncode(ord.PayPalRecpientID));
            else
                sbr.AppendFormat("{0}business={1}",ord.PayPalLiveUrl , HTTPPOSTEncode(ord.PayPalRecpientID));

            if (ord.IsShoppingCart && null != ord.OrderedData && ord.OrderedData.Rows.Count > 0)
            {
                sbr.Append("&upload=1");
                int intI = 1;
                foreach (DataRow objRow in ord.OrderedData.Rows)
                {
                    sbr.Append(string.Format("&item_name_{0}={1}", intI, HTTPPOSTEncode(objRow["ProductName"].ToString())));
                    sbr.Append(string.Format("&amount_{0}={1}", intI,((decimal)objRow["Price"] * (decimal)objRow["Qty"])));
                    intI++;
                }
            }
            else
            {
                sbr.Append("&item_name=" + HTTPPOSTEncode(ord.Name));
                sbr.Append("&item_number=" + HTTPPOSTEncode(ord.ItemNumber));
            }


             sbr.Append("&cmd=" + HTTPPOSTEncode("_xclick"));

            sbr.Append("&custom=" + HTTPPOSTEncode(ord.Custom));
            sbr.Append("&invoice=" + HTTPPOSTEncode(ord.Invoice));
            string amount = Convert.ToString(ord.Amount, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            sbr.Append("&amount=" + HTTPPOSTEncode(amount));
            sbr.Append("&currency_code=" + HTTPPOSTEncode(ord.CurrencyCode ));
            sbr.Append("&return=" + HTTPPOSTEncode(ord.ReturnUrl));
            sbr.Append("&cancel_return=" + HTTPPOSTEncode(ord.CancelUrl));
            sbr.Append("&notify_url=" + HTTPPOSTEncode(ord.NotifyUrl));
            sbr.Append("&undefined_quantity=&no_note=1&no_shipping=1");

            return sbr.ToString();
        }

        public static PayPalResultData Results(bool isTestMode)
        {
            PayPalResultData result = new PayPalResultData();

            bool blnCancel = false;
            bool blnValid = true;
            string strTransactionType;
            string strTransactionID = "";
            string strPost = "cmd=_notify-validate";
            //string ItemDesc;
            string email;

            foreach (string strName in System.Web.HttpContext.Current.Request.Form)
            {
                string strValue = System.Web.HttpContext.Current.Request.Form[strName];
                switch (strName)
                {
                    case "txn_type":
                        strTransactionType = strValue;
                        result.TransactionType = strValue;
                        if (strTransactionType == "subscr_cancel")
                            blnCancel = true;
                        break;
                    //case "payment_status":
                    //    if (strValue != "Completed")
                    //        blnValid = false;
                    //    break;

                    case "txn_id":
                        strTransactionID = strValue;
                        result.TransactionID = strValue;
                        break;

                    case "receiver_email":
                        //Verify receiver id
                        result.ReceiverEmail = strValue;
                        break;

                    case "mc_gross":
                        decimal amount = decimal.Parse(strValue);
                        result.Amount = amount;
                        break;

                    case "item_number":
                        result.ItemNumber = strValue;
                        break;

                    case "item_name":
                        result.Name = strValue;
                        break;

                    case "custom":
                        result.Custom = strValue;
                        break;

                    case "email":
                        email = strValue;
                        result.email = strValue;
                        break;
                }
                //Reconstruct For PostBack Validation
                strPost += String.Format("&{0}={1}", strName, HTTPPOSTEncode(strValue));
            }

            //PostBack to Verify Source
            if (blnValid)
            {
                string strPaypalVerifyURL = string.Empty;

                if (isTestMode)
                    strPaypalVerifyURL = "https://www.sandbox.paypal.com/cgi-bin/webscr";
                else
                    strPaypalVerifyURL = "https://www.paypal.com/cgi-bin/webscr";

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(strPaypalVerifyURL);

                objRequest.Method = "POST";
                objRequest.ContentType = "application/x-www-form-urlencoded";
                byte[] param = HttpContext.Current.Request.BinaryRead(HttpContext.Current.Request.ContentLength);
                string strRequest = Encoding.ASCII.GetString(param);
                strRequest += "&cmd=_notify-validate";
                objRequest.ContentLength = strRequest.Length;
             
                StreamWriter objStream = new StreamWriter(objRequest.GetRequestStream(), System.Text.Encoding.ASCII);
                objStream.Write(strRequest);
                objStream.Close();
                StreamReader streamIn = new StreamReader(objRequest.GetResponse().GetResponseStream());
                string strResponse = streamIn.ReadToEnd();
                streamIn.Close();

           
                if (strResponse == "VERIFIED")
                {
                    blnValid = true;
                }
                else
                {
                    blnValid = false;
                }

            }

            result.Valid = blnValid;
            result.Cancelled = blnCancel;

            return result;
        }

        public static string HTTPPOSTEncode(string strPost)
        {
          
            return System.Web.HttpUtility.UrlEncode(strPost);
        }

    }


    public class PayPalOrderData
    {
        public string Name = "";
        public string ItemNumber = "";
        public string Invoice = "";
        public int Qty = 0;
        public string Custom = "";
        public decimal Amount = 0;
        public string ReturnUrl = "";
        public string CancelUrl = "";
        public string NotifyUrl = "";
        public string PayPalLiveUrl = "";
        public string PaypalSandoxUrl = "";
        public string PayPalRecpientID = "";
        public string CurrencyCode = "USD";
        public bool IsShoppingCart = false;
        public DataTable OrderedData = null;
    }


    public class PayPalResultData
    {
        public bool Cancelled;
        public bool Valid;
        public string Custom;
        public string email;
        public string ReceiverEmail;
        public string TransactionType;
        public string TransactionID;
        public Decimal Amount;
        public string ItemNumber;
        public string Name;



    }

}
