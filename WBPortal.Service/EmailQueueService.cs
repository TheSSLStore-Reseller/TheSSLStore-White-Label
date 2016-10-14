using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WBSSLStore.Domain;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Service.ViewModels;
using System.Web.Mvc;
using System.Web;

namespace WBSSLStore.Service
{
    public interface IEmailQueueService
    {
        bool PrepareEmailQueue(int SiteID, int LangID, EmailType eEmailType, int SMTPId, string ToAddress, dynamic d);
        bool PrepareEmailQueue(int SiteID, int SMTPId, string From, string ToAddress, string CC, string BCC, string strEmailContent, string strSubject);
    }

    public class EmailQueueService : IEmailQueueService
    {
        private readonly IRepository<EmailTemplates> _EmailTemplates;
        private readonly IRepository<EmailQueue> _EmailQueue;
        private readonly IUnitOfWork _unitOfWork;

        public EmailQueueService(IRepository<EmailTemplates> pEmailTemplates, IRepository<EmailQueue> pEmailQueue, IUnitOfWork unitOfWork)
        {
            _EmailTemplates = pEmailTemplates;
            _EmailQueue = pEmailQueue;
            _unitOfWork = unitOfWork;
        }


        public bool PrepareEmailQueue(int SiteID, int SMTPId, string From, string ToAddress, string CC, string BCC, string strEmailContent, string strSubject)
        {
            EmailQueue objQueue = new EmailQueue();
            objQueue.BCC = BCC;
            objQueue.CC = CC;
            objQueue.EmailContent = strEmailContent;
            objQueue.From = From;
            objQueue.QueuedOn = DateTimeWithZone.Now;
            objQueue.SiteID = SiteID;
            objQueue.SiteSMTPID = SMTPId;
            objQueue.Subject = strSubject;
            objQueue.TO = ToAddress;

            _EmailQueue.Add(objQueue);
            return true;
        }
        public bool PrepareEmailQueue(int SiteID, int LangID, EmailType eEmailType, int SMTPId, string ToAddress, dynamic d)
        {
            string SiteSupportEmail = string.Empty;
            if (SiteID > 0)
            {
                var siteservice = DependencyResolver.Current.GetService<ISiteService>();
                Site site = siteservice.GetSite(SiteID);


                if (site == null)
                    throw new HttpException("No default site either");


                using (CurrentSiteSettings currentSiteSettings = new Domain.CurrentSiteSettings(site))
                {
                    siteservice = null;

                    string Host = (string.IsNullOrEmpty(site.Alias) ? site.CName : site.Alias);

                    if (Host.IndexOf('.') > 0 && !(Host.Split('.').Length > 2))
                    {
                        if (currentSiteSettings.IsRunWithWWW && !Host.Contains("www."))
                        {
                            Host = Host.Replace(Host, "www." + Host);
                        }
                        else if (!currentSiteSettings.IsRunWithWWW && Host.Contains("www."))
                            Host = Host.Replace("www.", "");
                    }


                    Host = (currentSiteSettings.USESSL ? "https" : "http") + "://" + Host + "/";

                    ReplaceMailMerge.SiteAlias = Host;
                    SiteSupportEmail = currentSiteSettings.SiteSupportEmail;
                }

            }

            string strSubject = string.Empty, strEmailContent = string.Empty;
            EmailTemplates objTemplate = _EmailTemplates.Find(et => et.EmailTypeId == (int)eEmailType && et.SiteID == SiteID && et.LangID == LangID).FirstOrDefault();
            if (objTemplate != null)
            {
                strSubject = objTemplate.EmailSubject;
                switch (eEmailType)
                {
                    case EmailType.ALL_FORGOTPASSWORD:
                        strEmailContent = ReplaceMailMerge.ForgotPassword(objTemplate.EmailContent, d as User);
                        break;
                    case EmailType.RESELLER_WELCOME_EMAIL:
                    case EmailType.RESELLER_ACCOUNT_ACTIVATION_EMAIL:
                    case EmailType.ADMIN_NEW_RESELLER:
                        strEmailContent = ReplaceMailMerge.ResellerWelComeEmail(objTemplate.EmailContent, d as User);
                        break;
                    case EmailType.CUSTOMER_WELCOME_EMAIL:
                    case EmailType.ADMIN_NEW_CUSTOMER:
                        strEmailContent = ReplaceMailMerge.CustomerWelComeEmail(objTemplate.EmailContent, d as User);
                        break;
                    case EmailType.ADMIN_ADD_FUND_NOTIFICATION:
                        strEmailContent = ReplaceMailMerge.AdminAddFund(objTemplate.EmailContent, d as UserTransaction);
                        break;
                    case EmailType.RESELLER_REFUND_NOTIFICATION:
                        strEmailContent = ReplaceMailMerge.ResellerRefundNotification(objTemplate.EmailContent, SiteSupportEmail, d as UserTransaction);
                        break;
                    case EmailType.CUSTOMER_REFUND_NOTIFICATION:
                        strEmailContent = ReplaceMailMerge.CustomerRefundNotification(objTemplate.EmailContent, SiteSupportEmail, d as UserTransaction);
                        break;
                    case EmailType.ADMIN_REFUND_NOTIFICATION:
                        strEmailContent = ReplaceMailMerge.AdminRefundNotification(objTemplate.EmailContent, d as SupportDetail);
                        break;
                    case EmailType.CUSTOMER_SUPPORT_NOTIFICATION:
                    case EmailType.RESELLER_SUPPORT_NOTIFICATION:
                    case EmailType.ADMIN_SUPPORT_NOTIFICATION:
                        strSubject = ReplaceMailMerge.SupportNotification(strSubject, d as SupportDetail);
                        strEmailContent = ReplaceMailMerge.SupportNotification(objTemplate.EmailContent, d as SupportDetail);
                        break;
                    case EmailType.RESELLER_CHANGE_PASSWORD_EMAIL:
                    case EmailType.CUSTOMER_CHANGE_PASSWORD:
                        strEmailContent = ReplaceMailMerge.ChangePassword(objTemplate.EmailContent, d as User, SiteSupportEmail);
                        break;
                    case EmailType.SUPPORT_FRAUDORDER_NOTIFICATION:
                        strEmailContent = ReplaceMailMerge.FraudNotification(objTemplate.EmailContent, d as string);
                        break;
                    case EmailType.RESELLER_NEW_ORDER_EMAIL:
                    case EmailType.CUSTOMER_NEW_ORDER:
                    case EmailType.ADMIN_NEW_ORDER:
                        strSubject = ReplaceMailMerge.OrderNotification(strSubject, d as CheckOutViewModel);
                        strEmailContent = ReplaceMailMerge.OrderNotification(objTemplate.EmailContent, d as CheckOutViewModel);
                        break;
                    case EmailType.ADMIN_NEWUSER_WELCOME_EMAIL:
                        strEmailContent = ReplaceMailMerge.AdminNewUserWelcome(objTemplate.EmailContent, d as User);
                        break;

                    case EmailType.CONTACTUS_EMAIL:
                        string[] strVal = d as string[];
                        strSubject = strVal[5];
                        strEmailContent = ReplaceMailMerge.ContactUsEmail(objTemplate.EmailContent, d as string[]);
                        break;
                    case EmailType.NeedAdvice_EMAIL:
                        string[] strValAd = d as string[];
                        strSubject = strValAd[4];
                        strEmailContent = ReplaceMailMerge.RFQEmails(objTemplate.EmailContent, 0, d as string[]);
                        break;
                    case EmailType.RequestForQuote_EMAIL:
                        string[] strValreq = d as string[];
                        strSubject = strValreq[4];
                        strEmailContent = ReplaceMailMerge.RFQEmails(objTemplate.EmailContent, 1, d as string[]);
                        break;
                }

                EmailQueue objQueue = new EmailQueue();
                objQueue.BCC = objTemplate.BCC;
                objQueue.CC = objTemplate.CC;
                objQueue.EmailContent = strEmailContent;
                objQueue.From = objTemplate.From;
                objQueue.QueuedOn = DateTimeWithZone.Now;
                objQueue.SiteID = objTemplate.SiteID;
                objQueue.SiteSMTPID = SMTPId;
                objQueue.Subject = strSubject;
                objQueue.TO = ToAddress;

                _EmailQueue.Add(objQueue);
                return true;
            }
            return false;
        }
    }
}
