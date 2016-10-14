using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WBSSLStore.Domain;
using WBSSLStore.Data.Infrastructure;
using System.Web.Mvc;
using System.Web;
using WBSSLStore.Service.ViewModels;
using System.Web.Caching;

namespace WBSSLStore.Service
{
    public class ReplaceMailMerge
    {

        private static System.Text.StringBuilder sBody = null;


        public static string SiteAlias
        {
            get;
            set;
        }

        private static List<Country> CountryList
        {
            get
            {
                List<Country> country = HttpContext.Current.Cache["Countries"] as List<Country>;

                if (country == null || country.Count() == 0)
                {
                    var repository = DependencyResolver.Current.GetService<WBSSLStore.Data.Repository.ISiteRepository>();

                    country = repository.GetCountryList();
                    HttpContext.Current.Cache.Insert("Countries", country, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 30, 0));
                }
                return country;
            }
        }
        public static string ForgotPassword(string content, User rowUser, string action = "")
        {

            //[SITEALIAS];[RESETLINK];[FULLNAME]
            sBody = new StringBuilder(content);

            string strPath = SiteAlias + (!string.IsNullOrEmpty(action) ? action : "passwordreset?token=");
            strPath += HttpUtility.HtmlEncode(WBSSLStore.CryptorEngine.Encrypt(rowUser.ID.ToString() + SettingConstants.Seprate + rowUser.PasswordHash, true));

            sBody.Replace("[RESETLINK]", strPath);
            sBody.Replace("[FULLNAME]", rowUser.FirstName + " " + rowUser.LastName);
            sBody.Replace("[SITEALIAS]", SiteAlias);
            return sBody.ToString();
        }
        public static string ResellerWelComeEmail(string content, User rowUser)
        {

            //[SITEALIAS];[FULLNAME];[CLIENTID];[EMAILADDRESS];[PASSWORD]
            //[COMPANYNAME];[ADDRESS];[CITYNAME];[STATENAME];[ZIPCODE];[COUNTRY];[EMAIL];[PHONE];[HEARABOUTUS]
            sBody = new StringBuilder(content);
            sBody.Replace("[FULLNAME]", rowUser.FirstName + " " + rowUser.LastName);
            sBody.Replace("[CLIENTID]", rowUser.ID.ToString());
            sBody.Replace("[EMAILADDRESS]", rowUser.Email);
            sBody.Replace("[EMAIL]", rowUser.Email);
            sBody.Replace("[HEARABOUTUS]", rowUser.HeardBy);
            sBody.Replace("[COMPANYNAME]", rowUser.CompanyName);
            if (rowUser.Address != null)
            {
                sBody.Replace("[ADDRESS]", rowUser.Address.Street);
                sBody.Replace("[CITYNAME]", rowUser.Address.City);
                sBody.Replace("[STATENAME]", rowUser.Address.State);
                sBody.Replace("[ZIPCODE]", rowUser.Address.Zip);
                if (rowUser.Address.Country != null)
                    sBody.Replace("[COUNTRY]", rowUser.Address.Country.CountryName);
                else if (rowUser.Address.CountryID > 0)
                    sBody.Replace("[COUNTRY]", CountryList.Where(c => c.ID == rowUser.Address.CountryID).FirstOrDefault().CountryName);

                sBody.Replace("[PHONE]", rowUser.Address.Phone);
            }
            else
            {
                sBody.Replace("[ADDRESS]", string.Empty);
                sBody.Replace("[CITYNAME]", string.Empty);
                sBody.Replace("[STATENAME]", string.Empty);
                sBody.Replace("[ZIPCODE]", string.Empty);
                sBody.Replace("[PHONE]", string.Empty);
            }
            sBody.Replace("[LOGINLINK]", SiteAlias + "logon");
            sBody.Replace("[SITEALIAS]", SiteAlias);
            return sBody.ToString();
        }
        public static string CustomerWelComeEmail(string content, dynamic s)
        {
            //[FULLNAME];[COMPANYNAME];[ADDRESS];[CITYNAME];[STATENAME];[ZIPCODE];[COUNTRY];[EMAIL];[PHONE]
            User rowUser = s as User;


            sBody = new StringBuilder(content);
            sBody.Replace("[FULLNAME]", rowUser.FirstName + " " + rowUser.LastName);
            sBody.Replace("[CLIENTID]", rowUser.ID.ToString());

            string strPath = SiteAlias + "passwordreset?token=";
            strPath += HttpUtility.HtmlEncode(WBSSLStore.CryptorEngine.Encrypt(rowUser.ID.ToString() + SettingConstants.Seprate + rowUser.PasswordHash, true));

            sBody.Replace("[LOGINLINK]", strPath);
            sBody.Replace("[SITEALIAS]", SiteAlias);
            sBody.Replace("[COMPANYNAME]", rowUser.CompanyName);
            sBody.Replace("[EMAIL]", rowUser.Email);

            if (rowUser.Address != null)
            {
                sBody.Replace("[ADDRESS]", rowUser.Address.Street);
                sBody.Replace("[CITYNAME]", rowUser.Address.City);
                sBody.Replace("[ZIPCODE]", rowUser.Address.Zip);
                sBody.Replace("[STATENAME]", rowUser.Address.State);
                if (rowUser.Address.Country != null)
                    sBody.Replace("[COUNTRY]", rowUser.Address.Country.CountryName);
                else if (rowUser.Address.CountryID > 0)
                    sBody.Replace("[COUNTRY]", CountryList.Where(c => c.ID == rowUser.Address.CountryID).FirstOrDefault().CountryName);
                sBody.Replace("[PHONE]", rowUser.Address.Phone);
            }
            return sBody.ToString();
        }
        public static string AdminAddFund(string content, UserTransaction usertransaction)
        {

            //[SITEALIAS];[RECIEPTDETAIL];[FULLNAME];[ACCOUNTTYPE];[AMOUNT]

            sBody = new StringBuilder(content);
            if (usertransaction.User != null)
            {
                sBody.Replace("[FULLNAME]", usertransaction.User.FirstName + " " + usertransaction.User.LastName);
                sBody.Replace("[ACCOUNTTYPE]", usertransaction.User.UserType.ToString());
            }
            else
            {
                sBody.Replace("[FULLNAME]", string.Empty);
                sBody.Replace("[ACCOUNTTYPE]", string.Empty);
            }
            sBody.Replace("[RECIEPTDETAIL]", usertransaction.ReceipientInstrumentDetails);
            sBody.Replace("[AMOUNT]", string.Format("{0:C}", usertransaction.TransactionAmount));
            sBody.Replace("[SITEALIAS]", SiteAlias);

            return sBody.ToString();
        }

        public static string ResellerRefundNotification(string content, string SiteSupportEmail, UserTransaction usertransaction)
        {

            //[SITEALIAS];[ACTION];[ORDERNUMBER];[FULLNAME];[SUPPORTMAIL]
            sBody = new StringBuilder(content);
            if (usertransaction.User != null)
                sBody.Replace("[FULLNAME]", usertransaction.User.FirstName + " " + usertransaction.User.LastName);
            else
                sBody.Replace("[FULLNAME]", string.Empty);
            if (usertransaction.OrderDetail != null)
                sBody.Replace("[ORDERNUMBER]", string.IsNullOrEmpty(usertransaction.OrderDetail.ExternalOrderID) ? "Invoice# " + usertransaction.OrderDetail.InvoiceNumber : "Order# " + usertransaction.OrderDetail.ExternalOrderID);
            else
                sBody.Replace("[ORDERNUMBER]", string.Empty);
            sBody.Replace("[ACTION]", RefundStatus.CANCEL_ORDER_AND_STORE_CREDIT.GetEnumDescription<RefundStatus>());
            sBody.Replace("[SITEALIAS]", SiteAlias);
            sBody.Replace("[SUPPORTMAIL]", SiteSupportEmail);
            return sBody.ToString();
        }

        public static string CustomerRefundNotification(string content, string SiteSupportEmail, UserTransaction usertransaction)
        {


            //[SITEALIAS];[ACTION];[ORDERNUMBER];[FULLNAME];[SUPPORTMAIL]
            sBody = new StringBuilder(content);
            if (usertransaction.User != null)
                sBody.Replace("[FULLNAME]", usertransaction.User.FirstName + " " + usertransaction.User.LastName);
            else
                sBody.Replace("[FULLNAME]", string.Empty);
            if (usertransaction.OrderDetail != null)
                sBody.Replace("[ORDERNUMBER]", string.IsNullOrEmpty(usertransaction.OrderDetail.ExternalOrderID) ? "Invoice# " + usertransaction.OrderDetail.InvoiceNumber : "Order# " + usertransaction.OrderDetail.ExternalOrderID);
            else
                sBody.Replace("[ORDERNUMBER]", string.Empty);
            sBody.Replace("[ACTION]", RefundStatus.CANCEL_ORDER_AND_STORE_CREDIT.GetEnumDescription<RefundStatus>());
            sBody.Replace("[SITEALIAS]", SiteAlias);
            sBody.Replace("[SUPPORTMAIL]", SiteSupportEmail);
            return sBody.ToString();
        }

        public static string AdminRefundNotification(string content, SupportDetail supportdetail)
        {


            //[SITEALIAS];[ACTION];[ORDERNUMBER];[FULLNAME]
            sBody = new StringBuilder(content);
            sBody.Replace("[FULLNAME]", supportdetail.SupportRequest.User.FirstName + " " + supportdetail.SupportRequest.User.LastName);
            sBody.Replace("[ORDERNUMBER]", string.IsNullOrEmpty(supportdetail.SupportRequest.OrderDetail.ExternalOrderID) ? "Invoice# " + supportdetail.SupportRequest.OrderDetail.InvoiceNumber : "Order# " + supportdetail.SupportRequest.OrderDetail.ExternalOrderID);
            sBody.Replace("[INCIDENTID]", "SSL-" + supportdetail.SupportRequest.ID);
            sBody.Replace("[SITEALIAS]", SiteAlias);
            return sBody.ToString();
        }
        public static string SupportNotification(string content, SupportDetail supportdetail)
        {

            //[SITEALIAS];[FULLNAME];[INCIDENTID];[SUPPORTINCIDENTSUBJECT];[STATUS]
            sBody = new StringBuilder(content);
            sBody.Replace("[FULLNAME]", supportdetail.SupportRequest.User.FirstName + " " + supportdetail.SupportRequest.User.LastName);
            sBody.Replace("[ACCOUNTTYPE]", supportdetail.SupportRequest.User.UserType.ToString());
            sBody.Replace("[ORDERNUMBER]", string.IsNullOrEmpty(supportdetail.SupportRequest.OrderDetail.ExternalOrderID) ? "Invoice# " + supportdetail.SupportRequest.OrderDetail.InvoiceNumber : "Order# " + supportdetail.SupportRequest.OrderDetail.ExternalOrderID);
            sBody.Replace("[INCIDENTID]", "SSL-" + supportdetail.SupportRequest.ID);
            sBody.Replace("[SUPPORTINCIDENTSUBJECT]", supportdetail.SupportRequest.Subject);
            sBody.Replace("[STATUS]", supportdetail.SupportRequest.isOpen ? "Open" : "Closed");
            sBody.Replace("[SITEALIAS]", SiteAlias);
            sBody.Replace("[SITEALIAS]", SiteAlias);
            return sBody.ToString();
        }
        public static string ChangePassword(string content, User rowUser, string SiteSupportEmail)
        {

            sBody = new StringBuilder(content);
            sBody.Replace("[FULLNAME]", rowUser.FirstName + " " + rowUser.LastName);
            sBody.Replace("[SITEALIAS]", SiteAlias);
            sBody.Replace("[SUPPORTMAIL]", SiteSupportEmail);
            sBody.Replace("[DATE]", Convert.ToString(DateTimeWithZone.CurrentTimeZone));


            return sBody.ToString();
        }
        public static string FraudNotification(string content, string AuthCode)
        {
            //[SITEALIAS];[TRANSACTIONID]
            sBody = new StringBuilder(content);
            sBody.Replace("[TRANSACTIONID]", AuthCode);
            sBody.Replace("[SITEALIAS]", SiteAlias);
            return sBody.ToString();
        }
        public static string OrderNotification(string content, CheckOutViewModel model)
        {

            //[PAYMENTMODE];[SITEALIAS];[DOWNLOADINVOICE];[PRODUCTINFO];[FULLNAME];[EMAILADDRESS];[COMPANYNAME];[ADDRESS];[CITYNAME];[CUSTOMER STATENAME];[CUSTOMER ZIPCODE]
            //;[CUSTOMER COUNTRY];[CUSTOMER PHONE];[CUSTOMER PHONE2];[CUSTOMER MOBILE];[CUSTOMER FAX];[DOMAINNAME];[PAYMENT DETAIL];[IPADDRESS];[TIME];[SITEALIAS]
            sBody = new StringBuilder(content);
            string strPaydata = string.Empty;

            sBody.Replace("[SITEALIAS]", SiteAlias);
            sBody.Replace("[FULLNAME]", model.user.FirstName + " " + model.user.LastName);
            sBody.Replace("[EMAILADDRESS]", model.user.Email);
            sBody.Replace("[COMPANYNAME]", model.user.Address.CompanyName);
            sBody.Replace("[ADDRESS]", model.user.Address.Street);
            sBody.Replace("[CITYNAME]", model.user.Address.City);
            sBody.Replace("[CUSTOMER STATENAME]", model.user.Address.State);
            sBody.Replace("[CUSTOMER ZIPCODE]", model.user.Address.Zip);
            if (model.user.Address.Country != null)
            {
                sBody.Replace("[CUSTOMER COUNTRY]", model.user.Address.Country.CountryName);
                sBody.Replace("[COUNTRY]", model.user.Address.Country.CountryName);
            }
            else
            {
                Country ctr = CountryList.Where(c => c.ID == model.user.Address.CountryID).FirstOrDefault();
                if (ctr != null)
                {
                    sBody.Replace("[CUSTOMER COUNTRY]", ctr.CountryName);
                    sBody.Replace("[COUNTRY]", ctr.CountryName);
                }
                else
                {
                    sBody.Replace("[CUSTOMER COUNTRY]", string.Empty);
                    sBody.Replace("[COUNTRY]", string.Empty);
                }
            }

            sBody.Replace("[CUSTOMER PHONE]", model.user.Address.Phone);
            sBody.Replace("[CUSTOMER MOBILE]", model.user.Address.Mobile);
            sBody.Replace("[CUSTOMER FAX]", model.user.Address.Fax);
            sBody.Replace("[STATENAME]", model.user.Address.State);
            sBody.Replace("[ZIPCODE]", model.user.Address.Zip);
            sBody.Replace("[INVOICEID]", model.InvoiceNumber);

            sBody.Replace("[PHONE]", model.user.Address.Phone);
            sBody.Replace("[MOBILE]", model.user.Address.Mobile);
            sBody.Replace("[FAX]", model.user.Address.Fax);

            sBody.Replace("[IPADDRESS]", Convert.ToString(System.Web.HttpContext.Current.Request.UserHostAddress));
            sBody.Replace("[DOMAINNAME]", string.Empty);
            sBody.Replace("[CUSTOMERID]", Convert.ToString(model.user.ID));
            sBody.Replace("[TIME]", Convert.ToString(DateTimeWithZone.Now.ToShortTimeString()));
            if (model.PayableAmount > 0 && model.paymentmode == Domain.PaymentMode.CC)
            {
                sBody.Replace("[PAYMENTMODE]", "Credit Card");
                strPaydata = "<p>Total Amount: " + String.Format("{0:c}", (model.PayableAmount)) + "</p>";
                strPaydata += "<p>Payment Mode: Credit Card</p><p>Credit Card Information:<br />";
                strPaydata += "<br />Card Number:&nbsp;" + model.CCNumber.Substring(model.CCNumber.Length - 4) + "<br/>";
                strPaydata += "";
            }
            else if (model.PayableAmount > 0 && model.paymentmode == Domain.PaymentMode.PAYPAL)
            {
                sBody.Replace("[PAYMENTMODE]", "PayPal");
                strPaydata = "<p>Total Amount: " + String.Format("{0:c}", model.PayableAmount) + "</p>";
                strPaydata += "<p>Payment Mode: PayPal</p><p>PayPal Information:<br />";
                strPaydata += "PaypalID:&nbsp;" + Convert.ToString(model.PayPalID) + "<br />";
            }
            else if (model.PayableAmount > 0 && model.paymentmode == Domain.PaymentMode.MONEYBOOKERS)
            {
                sBody.Replace("[PAYMENTMODE]", "Moneybookers");
                strPaydata = "<p>Total Amount: " + String.Format("{0:c}", model.PayableAmount) + "</p>";
                strPaydata += "<p>Payment Mode: Moneybookers</p>";
                if (!string.IsNullOrEmpty(model.MoneybookersID))
                    strPaydata += "<p>Moneybookers Information:<br />MoneybookersID:&nbsp;" + Convert.ToString(model.MoneybookersID) + "<br />";
            }
            else if (model.PayableAmount <= 0)
            {
                sBody.Replace("[PAYMENTMODE]", "Credit Used");
                strPaydata = "<p>Total Amount: " + String.Format("{0:c}", (model.OrderAmount + model.Tax) - model.PromoDiscount) + "</p>";
            }
            sBody.Replace("[PAYMENT DETAIL]", strPaydata);
            sBody.Replace("[PRODUCTINFO]", model.AllProductInfo);
            sBody.Replace("[DOWNLOADINVOICE]", "<a href='" + SiteAlias + "client/Orders/invoice/" + model.OrderID + "'>Click Here To Download Invoice</a>");
            return sBody.ToString();
        }
        public static string AdminNewUserWelcome(string content, dynamic s)
        {
            //[FULLNAME];[COMPANYNAME];[ADDRESS];[CITYNAME];[STATENAME];[ZIPCODE];[COUNTRY];[EMAIL];[PHONE]
            User rowUser = s as User;

            sBody = new StringBuilder(content);
            sBody.Replace("[FULLNAME]", rowUser.FirstName + " " + rowUser.LastName);
            sBody.Replace("[CLIENTID]", rowUser.ID.ToString());

            string strPath = SiteAlias + "passwordreset?token=";
            strPath += HttpUtility.HtmlEncode(WBSSLStore.CryptorEngine.Encrypt(rowUser.ID.ToString() + SettingConstants.Seprate + rowUser.PasswordHash, true));

            sBody.Replace("[LOGINLINK]", strPath);
            sBody.Replace("[SITEALIAS]", SiteAlias);
            sBody.Replace("[COMPANYNAME]", rowUser.CompanyName);
            sBody.Replace("[EMAIL]", rowUser.Email);

            if (rowUser.Address != null)
            {
                sBody.Replace("[ADDRESS]", rowUser.Address.Street);
                sBody.Replace("[CITYNAME]", rowUser.Address.City);
                sBody.Replace("[ZIPCODE]", rowUser.Address.Zip);
                sBody.Replace("[STATENAME]", rowUser.Address.State);
                if (rowUser.Address.Country != null)
                    sBody.Replace("[COUNTRY]", rowUser.Address.Country.CountryName);
                else if (rowUser.Address.CountryID > 0)
                    sBody.Replace("[COUNTRY]", CountryList.Where(c => c.ID == rowUser.Address.CountryID).FirstOrDefault().CountryName);
                sBody.Replace("[PHONE]", rowUser.Address.Phone);
            }
            return sBody.ToString();
        }

        public static string ContactUsEmail(string content, dynamic d)
        {
            string[] strVal = d as string[];
            sBody = new StringBuilder(content);
            sBody.Replace("[COMPANYNAME]", Convert.ToString(strVal[0]));
            sBody.Replace("[FULLNAME]", Convert.ToString(strVal[1]));
            sBody.Replace("[PHONE]", Convert.ToString(strVal[2]));
            sBody.Replace("[EMAIL]", Convert.ToString(strVal[3]));
            sBody.Replace("[COMMENTS]", Convert.ToString(strVal[4]));
            return sBody.ToString();
        }

        public static string RFQEmails(string content,int type ,dynamic d)
        {
            string[] strVal = d as string[];
            sBody = new StringBuilder(content);
            sBody.Replace("[FULLNAME]", Convert.ToString(strVal[0]));
            sBody.Replace("[PHONE]", Convert.ToString(strVal[1]));
            sBody.Replace("[EMAIL]", Convert.ToString(strVal[2]));
            
            if (type != 0)
                sBody.Replace("[COMMENTS]", Convert.ToString(strVal[3]));

            return sBody.ToString();
        }
    }
}
