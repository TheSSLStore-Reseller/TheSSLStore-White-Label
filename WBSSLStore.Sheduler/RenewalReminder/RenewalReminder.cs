using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Domain;
//using System.Data.Objects;
using System.Web.Mvc;
using System.Threading;
using System.Globalization;

namespace WBSSLStore.Scheduler.RenewalReminder
{
    class RenewalReminder : EFRepository<EmailQueue>
    {
        private Logger.Logger logger = null;
        private IDatabaseFactory _dbfactory;
        private string DomaiNames;

        public RenewalReminder(IDatabaseFactory dbfactory)
            : base(dbfactory)
        {
            _dbfactory = dbfactory;
            logger = new Logger.Logger();
        }

        //public static void Main(string[] args)
        //{
        //    var objData = new WBSSLStore.Scheduler.RenewalReminder.RenewalReminder(new Data.Infrastructure.DatabaseFactory());
        //    //objData.SendMailToCustomer();
        //    //objData.SendMailToReseller();
        //    objData.SendMailToRsellerCustomer();
        //}

        public void SendMailToCustomer()
        {
            try
            {
                logger.Log("---------Start customer renewal notification---------", Logger.LogType.REMINDER);

                DateTime dtbefore3Days = DateTime.Now.AddDays(3);
                DateTime dtbefore7Days = DateTime.Now.AddDays(7);
                DateTime dtbefore15Days = DateTime.Now.AddDays(15);
                DateTime dtbefore30Days = DateTime.Now.AddDays(30);

                IQueryable<RenewalReminderData> query = (from od in DbContext.OrderDetails
                                                         from o in DbContext.Orders
                                                                     .Where(ord => ord.ID == od.OrderID)
                                                         from cr in DbContext.CertificateRequests
                                                                     .Where(creq => creq.ID == od.CertificateRequestID)
                                                         from u in DbContext.Users
                                                                     .Where(usr => usr.ID == o.UserID)
                                                         from a in DbContext.Addresses
                                                                     .Where(add => add.ID == u.AddressID)
                                                         from c in DbContext.Countries
                                                                    .Where(ct => ct.ID == a.CountryID)
                                                         from s in DbContext.Sites
                                                                     .Where(st => st.ID == o.SiteID)
                                                         from sm in DbContext.SiteSmtps
                                                                     .Where(smtp => smtp.SiteID == s.ID)
                                                         where ((System.Data.Entity.DbFunctions.DiffDays(od.CertificateExpiresOn, dtbefore3Days) == 0 || System.Data.Entity.DbFunctions.DiffDays(od.CertificateExpiresOn, dtbefore7Days) == 0 || System.Data.Entity.DbFunctions.DiffDays(od.CertificateExpiresOn, dtbefore15Days) == 0 || System.Data.Entity.DbFunctions.DiffDays(od.CertificateExpiresOn, dtbefore30Days) == 0)
                                                                && od.OrderStatusID == (int)OrderStatus.ACTIVE && u.UserTypeID == (int)UserType.CUSTOMER)
                                                         orderby s.ID, od.CertificateExpiresOn
                                                         select new RenewalReminderData
                                                         {
                                                             FirstName = u.FirstName,
                                                             LastName = u.LastName,
                                                             DomaiName = cr.DomainName,
                                                             CertificateExpireOnDate = od.CertificateExpiresOn,
                                                             ProductName = od.ProductName,
                                                             SiteAlias = s.Alias,
                                                             UserTypeID = u.UserTypeID,
                                                             ProductID = od.ProductID,
                                                             UserID = u.ID,
                                                             SiteID = s.ID,
                                                             Email = u.Email,
                                                             AlternatEmail = u.AlternativeEmail,
                                                             SiteSMTP = sm.ID,
                                                             RenewalPrice = od.Price,
                                                             Phone = a.Phone,
                                                             Company = a.CompanyName,
                                                             Street = a.Street,
                                                             City = a.City,
                                                             State = a.State,
                                                             Zip = a.Zip,
                                                             Country = c.CountryName,
                                                             NumberOfMonths = od.NumberOfMonths
                                                         });
                var renewalReminderData = query.ToList();

                logger.Log("---------Start total number of records fetched for customer renewal notification are : " + renewalReminderData.Count + "---------", Logger.LogType.REMINDER);

                EmailQueue objEmailQueue = null;
                EmailTemplates objEmailTemplates = null;
                foreach (RenewalReminderData row in renewalReminderData)
                {
                    try
                    {
                        //Get site-wise email template for customer or reseller
                        int iBeforeNumberOfDays = Convert.ToDateTime(row.CertificateExpireOnDate.Value.ToShortDateString()).Subtract(DateTime.Today).Days;

                        row.LanguageCode = GetSiteSettingValue(row.SiteID, SettingConstants.CURRENT_SITELANGUAGE_KEY);
                        if (string.IsNullOrEmpty(row.LanguageCode))
                            row.LanguageCode = "en";
                        int LangID = (from l in DbContext.Languages
                                      where l.LangCode == row.LanguageCode
                                      select l).ToList().FirstOrDefault().ID;

                        switch (iBeforeNumberOfDays)
                        {
                            case 3:
                                objEmailTemplates = GetUserEmailTemplate(EmailType.CUSTOMER_RENEWAL_3, row.SiteID, LangID);
                                break;
                            case 7:
                                objEmailTemplates = GetUserEmailTemplate(EmailType.CUSTOMER_RENEWAL_7, row.SiteID, LangID);
                                break;
                            case 15:
                                objEmailTemplates = GetUserEmailTemplate(EmailType.CUSTOMER_RENEWAL_15, row.SiteID, LangID);
                                break;
                            case 30:
                                objEmailTemplates = GetUserEmailTemplate(EmailType.CUSTOMER_RENEWAL_30, row.SiteID, LangID);
                                break;
                        }
                        if (objEmailTemplates != null)
                        {
                            objEmailTemplates.EmailContent = ReplaceCustomerMailMerge(objEmailTemplates.EmailContent, row);


                            string[] strMailmerge = objEmailTemplates.MailMerge.Split(';');
                            var objResult = Array.FindAll(strMailmerge, s => objEmailTemplates.EmailSubject.Contains(s));
                            if (objResult != null && objResult.Length > 0)
                                objEmailTemplates.EmailSubject = ReplaceCustomerMailMerge(objEmailTemplates.EmailSubject, row);
                        }

                        
                        if (objEmailTemplates != null)
                        {
                            objEmailQueue = new EmailQueue();
                            objEmailQueue.BCC = objEmailTemplates.BCC;
                            objEmailQueue.CC = objEmailTemplates.CC;
                            objEmailQueue.EmailContent = objEmailTemplates.EmailContent;
                            objEmailQueue.From = objEmailTemplates.From;
                            objEmailQueue.QueuedOn = DateTime.Now;
                            objEmailQueue.SiteID = row.SiteID;
                            objEmailQueue.SiteSMTPID = row.SiteSMTP;
                            objEmailQueue.Subject = objEmailTemplates.EmailSubject;
                            objEmailQueue.TO = row.Email + (string.IsNullOrEmpty(row.AlternatEmail) ? string.Empty : ";" + row.AlternatEmail);

                            DbContext.EmailQueues.Add(objEmailQueue);
                            DbContext.Entry<EmailTemplates>(objEmailTemplates).State = System.Data.Entity.EntityState.Unchanged;
                            DbContext.Commit();
                            logger.Log("Reminder mail queued successfully for domain name : " + row.DomaiName, Logger.LogType.REMINDER);
                        }
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                    {
                        StringBuilder sbError = new StringBuilder();
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                            foreach (var validationError in validationErrors.ValidationErrors)
                                sbError.AppendLine("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                        logger.Log("************Start error for domain name : " + row.DomaiName + " : " + sbError.ToString() + "************", Logger.LogType.REMINDER);
                        logger.LogException(dbEx);
                        logger.Log("************End error************", Logger.LogType.REMINDER);
                    }
                    catch (Exception ex)
                    {
                        logger.Log("************Start error for domain name : " + row.DomaiName + " : " + ex.Message.ToString() + "************", Logger.LogType.REMINDER);
                        logger.LogException(ex);
                        logger.Log("************End error************", Logger.LogType.REMINDER);
                    }
                    finally
                    {
                        objEmailQueue = null;
                    }
                }
                logger.Log("---------End of records fetched for customer renewal notification---------", Logger.LogType.REMINDER);
            }
            catch (Exception ex)
            {
                logger.Log("************Start error in customer  renewal notification************", Logger.LogType.REMINDER);
                logger.LogException(ex);
                logger.Log("************End error in customer  renewal notification************", Logger.LogType.REMINDER);
            }
        }

        public void SendMailToReseller()
        {
            logger.Log("---------Start reseller renewal notification---------", Logger.LogType.REMINDER);
            ResellerReminder(3);
            ResellerReminder(7);
            ResellerReminder(15);
            ResellerReminder(30);
            logger.Log("---------End reseller renewal notification---------", Logger.LogType.REMINDER);
        }

        public void ResellerReminder(int iBeforeNumberOfDays)
        {
            try
            {
                
                DateTime dtbeforeDays = DateTime.Now.AddDays(iBeforeNumberOfDays);

                List<int> resellers = (from u in DbContext.Users
                                       from o in DbContext.Orders
                                                 .Where(ord => ord.UserID == u.ID)
                                       from od in DbContext.OrderDetails
                                                 .Where(dtl => dtl.OrderID == o.ID)
                                       from uo in DbContext.UserOptions.Where(uoption => uoption.UserID == u.ID).DefaultIfEmpty()
                                       //&& System.Data.Entity.DbFunctions.DiffDays(od.CertificateExpiresOn, dtbeforeDays) == 0   && u.UserTypeID == (int)UserType.RESELLER 
                                       //!(uo.StopResellerEmail ?? false) &&  where ((od.OrderStatusID != (int)OrderStatus.ACTIVE) && !(uo.StopResellerEmail ?? false)) 
                                       where (!(uo.StopResellerEmail ?? false) && System.Data.Entity.DbFunctions.DiffDays(od.CertificateExpiresOn, dtbeforeDays) == 0 && u.UserTypeID == (int)UserType.RESELLER && od.OrderStatusID == (int)OrderStatus.ACTIVE)
                                       select u.ID
                                              ).Distinct().ToList();
                logger.Log("---------Start Reseller renewal notification before " + iBeforeNumberOfDays + " days. Total records are :" + resellers.Count + "---------", Logger.LogType.REMINDER);

                EmailQueue objEmailQueue = null;
                EmailTemplates objEmailTemplates = null;
                foreach (int UserID in resellers)
                {
                    try
                    {

                        IQueryable<RenewalReminderData> query = (from od in DbContext.OrderDetails
                                                                 from o in DbContext.Orders
                                                                             .Where(ord => ord.ID == od.OrderID)
                                                                 from cr in DbContext.CertificateRequests
                                                                             .Where(creq => creq.ID == od.CertificateRequestID)
                                                                 from u in DbContext.Users
                                                                             .Where(usr => usr.ID == o.UserID)
                                                                 from a in DbContext.Addresses
                                                                             .Where(add => add.ID == u.AddressID)
                                                                 from c in DbContext.Countries
                                                                            .Where(ct => ct.ID == a.CountryID)
                                                                 from s in DbContext.Sites
                                                                             .Where(st => st.ID == o.SiteID)
                                                                 from sm in DbContext.SiteSmtps
                                                                             .Where(smtp => smtp.SiteID == s.ID)
                                                                 from uo in DbContext.UserOptions.Where(useop => useop.UserID == u.ID).DefaultIfEmpty()
                                                                 where (u.ID == UserID && System.Data.Entity.DbFunctions.DiffDays(od.CertificateExpiresOn, dtbeforeDays) == 0 && !(uo.StopResellerEmail ?? false))
                                                                 orderby s.ID, od.CertificateExpiresOn
                                                                 select new RenewalReminderData
                                                                 {
                                                                     FirstName = u.FirstName,
                                                                     LastName = u.LastName,
                                                                     DomaiName = cr.DomainName,
                                                                     CertificateExpireOnDate = od.CertificateExpiresOn,
                                                                     ProductName = od.ProductName,
                                                                     SiteAlias = s.Alias,
                                                                     UserTypeID = u.UserTypeID,
                                                                     ProductID = od.ProductID,
                                                                     UserID = u.ID,
                                                                     SiteID = s.ID,
                                                                     Email = u.Email,
                                                                     AlternatEmail = u.AlternativeEmail,
                                                                     SiteSMTP = sm.ID,
                                                                     RenewalPrice = od.Price,
                                                                     Phone = a.Phone,
                                                                     Company = a.CompanyName,
                                                                     Street = a.Street,
                                                                     City = a.City,
                                                                     State = a.State,
                                                                     Zip = a.Zip,
                                                                     Country = c.CountryName,
                                                                     NumberOfMonths = od.NumberOfMonths
                                                                 });


                        var resellerRenewalData = query.ToList();

                        if (resellerRenewalData != null && resellerRenewalData.Count > 0)
                        {
                            DomaiNames = string.Empty;

                            int SiteID = resellerRenewalData[0].SiteID;
                            string LanguageCode = string.Empty;

                            LanguageCode = GetSiteSettingValue(SiteID, SettingConstants.CURRENT_SITELANGUAGE_KEY);
                            if (string.IsNullOrEmpty(LanguageCode))
                                LanguageCode = "en";
                            int LangID = (from l in DbContext.Languages
                                          where l.LangCode == LanguageCode
                                          select l).ToList().FirstOrDefault().ID;

                            switch (iBeforeNumberOfDays)
                            {
                                case 3:
                                    objEmailTemplates = GetUserEmailTemplate(EmailType.RESELLER_RENEWAL_3, SiteID, LangID);
                                    break;
                                case 7:
                                    objEmailTemplates = GetUserEmailTemplate(EmailType.RESELLER_RENEWAL_7, SiteID, LangID);
                                    break;
                                case 15:
                                    objEmailTemplates = GetUserEmailTemplate(EmailType.RESELLER_RENEWAL_15, SiteID, LangID);
                                    break;
                                case 30:
                                    objEmailTemplates = GetUserEmailTemplate(EmailType.RESELLER_RENEWAL_30, SiteID, LangID);
                                    break;
                            }
                            if (objEmailTemplates != null)
                            {
                                objEmailTemplates.EmailContent = ReplaceResellerMailMerge(objEmailTemplates.EmailContent, resellerRenewalData);

                                string[] strMailmerge = objEmailTemplates.MailMerge.Split(';');
                                var objResult = Array.FindAll(strMailmerge, s => objEmailTemplates.EmailSubject.Contains(s));
                                if (objResult != null && objResult.Length > 0)
                                    objEmailTemplates.EmailSubject = ReplaceResellerMailMerge(objEmailTemplates.EmailSubject, resellerRenewalData);
                            }

                            if (objEmailTemplates != null)
                            {
                                objEmailQueue = new EmailQueue();
                                objEmailQueue.BCC = objEmailTemplates.BCC;
                                objEmailQueue.CC = objEmailTemplates.CC;
                                objEmailQueue.EmailContent = objEmailTemplates.EmailContent;
                                objEmailQueue.From = objEmailTemplates.From;
                                objEmailQueue.QueuedOn = DateTime.Now;
                                objEmailQueue.SiteID = SiteID;
                                objEmailQueue.SiteSMTPID = resellerRenewalData[0].SiteSMTP;
                                objEmailQueue.Subject = objEmailTemplates.EmailSubject;
                                objEmailQueue.TO = resellerRenewalData[0].Email + (string.IsNullOrEmpty(resellerRenewalData[0].AlternatEmail) ? string.Empty : ";" + resellerRenewalData[0].AlternatEmail);

                                DbContext.EmailQueues.Add(objEmailQueue);
                                DbContext.Entry<EmailTemplates>(objEmailTemplates).State = System.Data.Entity.EntityState.Unchanged;
                                DbContext.Commit();
                                logger.Log("Reminder mail queued successfully for domain name(s) : " + DomaiNames, Logger.LogType.REMINDER);
                            }
                        }
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                    {
                        StringBuilder sbError = new StringBuilder();
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                            foreach (var validationError in validationErrors.ValidationErrors)
                                sbError.AppendLine("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                        logger.Log("************Start error in Reseller renewal notification before " + iBeforeNumberOfDays + " days for UserID : " + UserID + " : " + sbError.ToString() + "************", Logger.LogType.REMINDER);
                        logger.LogException(dbEx);
                        logger.Log("************End error************", Logger.LogType.REMINDER);
                    }
                    catch (Exception ex)
                    {
                        logger.Log("************Start error in Reseller renewal notification before " + iBeforeNumberOfDays + " days for UserID : " + UserID + "************", Logger.LogType.REMINDER);
                        logger.LogException(ex);
                        logger.Log("************End error************", Logger.LogType.REMINDER);
                    }
                    finally
                    {
                        objEmailQueue = null;
                    }
                }

                logger.Log("---------End Reseller renewal notification before " + iBeforeNumberOfDays + " days---------", Logger.LogType.REMINDER);
            }
            catch (Exception ex)
            {
                logger.Log("************Start error in Reseller renewal notification before " + iBeforeNumberOfDays + " days************", Logger.LogType.REMINDER);
                logger.LogException(ex);
                logger.Log("************End error in Reseller renewal notification before " + iBeforeNumberOfDays + " days************", Logger.LogType.REMINDER);
            }
        }

        public void SendMailToRsellerCustomer()
        {
            try
            {
                logger.Log("---------Start reseller's customer renewal notification---------", Logger.LogType.REMINDER);

                DateTime dtbefore3Days = DateTime.Now.AddDays(3);
                DateTime dtbefore7Days = DateTime.Now.AddDays(7);
                DateTime dtbefore15Days = DateTime.Now.AddDays(15);
                DateTime dtbefore30Days = DateTime.Now.AddDays(30);

                IQueryable<RenewalReminderData> query = (from od in DbContext.OrderDetails
                                                         from o in DbContext.Orders
                                                                     .Where(ord => ord.ID == od.OrderID)
                                                         from cr in DbContext.CertificateRequests
                                                                     .Where(creq => creq.ID == od.CertificateRequestID)
                                                         from cc in DbContext.CertificateContacts
                                                                     .Where(contact => contact.ID == cr.AdminContactID)
                                                         from u in DbContext.Users
                                                                     .Where(usr => usr.ID == o.UserID)
                                                         from a in DbContext.Addresses
                                                                     .Where(add => add.ID == u.AddressID)
                                                         from c in DbContext.Countries
                                                                    .Where(ct => ct.ID == a.CountryID)
                                                         from s in DbContext.Sites
                                                                     .Where(st => st.ID == o.SiteID)
                                                         from sm in DbContext.SiteSmtps
                                                                     .Where(smtp => smtp.SiteID == s.ID)
                                                         from uo in DbContext.UserOptions
                                                                    .Where(uop => uop.UserID == u.ID).DefaultIfEmpty()
                                                         where (u.UserTypeID == (int)UserType.RESELLER && !(uo.StopResellerCustomerEmail ?? false) &&
                                                                (System.Data.Entity.DbFunctions.DiffDays(od.CertificateExpiresOn, dtbefore3Days) == 0 || System.Data.Entity.DbFunctions.DiffDays(od.CertificateExpiresOn, dtbefore7Days) == 0 || System.Data.Entity.DbFunctions.DiffDays(od.CertificateExpiresOn, dtbefore15Days) == 0 || System.Data.Entity.DbFunctions.DiffDays(od.CertificateExpiresOn, dtbefore30Days) == 0)
                                                               && (u.Email.Trim() != cc.EmailAddress.Trim())
                                                                )
                                                         orderby s.ID, od.CertificateExpiresOn
                                                         select new RenewalReminderData
                                                         {
                                                             FirstName = u.FirstName,
                                                             LastName = u.LastName,
                                                             DomaiName = cr.DomainName,
                                                             CertificateExpireOnDate = od.CertificateExpiresOn,
                                                             ProductName = od.ProductName,
                                                             SiteAlias = s.Alias,
                                                             UserTypeID = u.UserTypeID,
                                                             ProductID = od.ProductID,
                                                             UserID = u.ID,
                                                             SiteID = s.ID,
                                                             Email = u.Email,
                                                             AlternatEmail = u.AlternativeEmail,
                                                             SiteSMTP = sm.ID,
                                                             RenewalPrice = od.Price,
                                                             Phone = a.Phone,
                                                             Company = a.CompanyName,
                                                             Street = a.Street,
                                                             City = a.City,
                                                             State = a.State,
                                                             Zip = a.Zip,
                                                             Country = c.CountryName,
                                                             NumberOfMonths = od.NumberOfMonths,
                                                             ACFirstName = cc.FirstName,
                                                             ACLastName = cc.LastName,
                                                             ACEmail = cc.EmailAddress,
                                                             ACPhone = cc.PhoneNumber                                                             
                                                         });
                
                var renewalReminderData = query.ToList();

                logger.Log("---------Start total number of records fetched for reseller's customer renewal notification are : " + renewalReminderData.Count + "---------", Logger.LogType.REMINDER);

                EmailQueue objEmailQueue = null;
                EmailTemplates objEmailTemplates = null;
                string CustomEmailForRenewal = string.Empty;
                foreach (RenewalReminderData row in renewalReminderData)
                {
                    try
                    {
                        //Get site-wise email template for customer or reseller
                        int iBeforeNumberOfDays = Convert.ToDateTime(row.CertificateExpireOnDate.Value.ToShortDateString()).Subtract(DateTime.Today).Days;
                        CustomEmailForRenewal = string.Empty;
                        row.LanguageCode = GetSiteSettingValue(row.SiteID, SettingConstants.CURRENT_SITELANGUAGE_KEY);
                        if (string.IsNullOrEmpty(row.LanguageCode))
                            row.LanguageCode = "en";
                        int LangID = (from l in DbContext.Languages
                                      where l.LangCode == row.LanguageCode
                                      select l).ToList().FirstOrDefault().ID;

                        switch (iBeforeNumberOfDays)
                        {
                            case 3:
                                objEmailTemplates = GetUserEmailTemplate(EmailType.RESELLER_CUSTOMER_RENEWAL_3, row.SiteID, LangID);
                                break;
                            case 7:
                                objEmailTemplates = GetUserEmailTemplate(EmailType.RESELLER_CUSTOMER_RENEWAL_7, row.SiteID, LangID);
                                break;
                            case 15:
                                objEmailTemplates = GetUserEmailTemplate(EmailType.RESELLER_CUSTOMER_RENEWAL_15, row.SiteID, LangID);
                                break;
                            case 30:
                                objEmailTemplates = GetUserEmailTemplate(EmailType.RESELLER_CUSTOMER_RENEWAL_30, row.SiteID, LangID);
                                break;
                        }
                        if (objEmailTemplates != null)
                        {
                            objEmailTemplates.EmailContent = ReplaceResellerCustomerMailMerge(objEmailTemplates.EmailContent, row);
                            string[] strMailmerge = objEmailTemplates.MailMerge.Split(';');
                            var objResult = Array.FindAll(strMailmerge, s => objEmailTemplates.EmailSubject.Contains(s));
                            if (objResult != null && objResult.Length > 0)
                                objEmailTemplates.EmailSubject = ReplaceResellerCustomerMailMerge(objEmailTemplates.EmailSubject, row);
                        }

                        //As per Gino request that From email address of reseller's customer's renewal reminder email should be respective reseller's registered email 
                        CustomEmailForRenewal = GetSiteSettingValue(row.SiteID, SettingConstants.CUSTOM_EMAILID_RESELLER_CUSTOMERS_RENEWALS_NOTIFICATION);

                        if (objEmailTemplates != null)
                        {
                            objEmailQueue = new EmailQueue();
                            objEmailQueue.BCC = objEmailTemplates.BCC;
                            objEmailQueue.CC = objEmailTemplates.CC + (string.IsNullOrEmpty(row.Email) ? string.Empty : ";" + row.Email);
                            objEmailQueue.EmailContent = objEmailTemplates.EmailContent;
                            objEmailQueue.From = !string.IsNullOrEmpty(CustomEmailForRenewal) && CustomEmailForRenewal.ToLower().Equals("true") ? row.Email : objEmailTemplates.From;
                            objEmailQueue.QueuedOn = DateTime.Now;
                            objEmailQueue.SiteID = row.SiteID;
                            objEmailQueue.SiteSMTPID = row.SiteSMTP;
                            objEmailQueue.Subject = objEmailTemplates.EmailSubject;
                            objEmailQueue.TO =  row.ACEmail;

                            DbContext.EmailQueues.Add(objEmailQueue);
                            DbContext.Entry<EmailTemplates>(objEmailTemplates).State = System.Data.Entity.EntityState.Unchanged;
                            DbContext.Commit();
                            logger.Log("Reminder mail queued successfully for domain name : " + row.DomaiName, Logger.LogType.REMINDER);
                        }
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                    {
                        StringBuilder sbError = new StringBuilder();
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                            foreach (var validationError in validationErrors.ValidationErrors)
                                sbError.AppendLine("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                        logger.Log("************Start error for domain name : " + row.DomaiName + " : " + sbError.ToString() + "************", Logger.LogType.REMINDER);
                        logger.LogException(dbEx);
                        logger.Log("************End error************", Logger.LogType.REMINDER);
                    }
                    catch (Exception ex)
                    {
                        logger.Log("************Start error for domain name : " + row.DomaiName + " : " + ex.Message.ToString() + "************", Logger.LogType.REMINDER);
                        logger.LogException(ex);
                        logger.Log("************End error************", Logger.LogType.REMINDER);
                    }
                    finally
                    {
                        objEmailQueue = null;
                    }
                }
                logger.Log("---------End of records fetched for reseller's customer renewal notification---------", Logger.LogType.REMINDER);
            }
            catch (Exception ex)
            {
                logger.Log("************Start error in reseller's customer  renewal notification************", Logger.LogType.REMINDER);
                logger.LogException(ex);
                logger.Log("************End error in reseller's customer  renewal notification************", Logger.LogType.REMINDER);
            }
        }

        public string ReplaceCustomerMailMerge(string strMailContent, RenewalReminderData oRenewalReminderData)
        {
            StringBuilder sbMailContent = new StringBuilder(strMailContent);

            string strPhone = GetSiteSettingValue(oRenewalReminderData.SiteID, SettingConstants.CURRENT_SITEPNONE_KEY);

            bool bUseSSL = (GetSiteSettingValue(oRenewalReminderData.SiteID, SettingConstants.CURRENT_USESSL_KEY).ToLower() == Boolean.TrueString.ToLower());
            bool bRunWithWWW = (GetSiteSettingValue(oRenewalReminderData.SiteID, SettingConstants.CURRENT_IS_SITE_RUN_WITH_WWW).ToLower() == Boolean.TrueString.ToLower());
            string strCurrency = GetSiteSettingValue(oRenewalReminderData.SiteID, SettingConstants.CURRENT_SITECURRANCY_KEY);
            string strCulture = GetSiteSettingValue(oRenewalReminderData.SiteID, SettingConstants.CURRENT_SITECULTURE_KEY);
            string strURL = "http://" + (bRunWithWWW ? "www." : string.Empty) + oRenewalReminderData.SiteAlias;

            SwitchCulture(oRenewalReminderData.LanguageCode, string.IsNullOrEmpty(strCulture) ? oRenewalReminderData.LanguageCode : strCulture);

            sbMailContent.Replace("[SITEPHONENUMBER]", !string.IsNullOrEmpty(strPhone) ? strPhone : string.Empty);
            sbMailContent.Replace("[CONTECTNUMBER]", !string.IsNullOrEmpty(strPhone) ? strPhone : string.Empty);
            sbMailContent.Replace("[LOGINURL]", (bUseSSL ? strURL.Replace("http:", "https:") : strURL) + "/logon");
            sbMailContent.Replace("[SITEURL]", strURL);
            sbMailContent.Replace("[FULLNAME]", oRenewalReminderData.FirstName + " " + oRenewalReminderData.LastName);
            sbMailContent.Replace("[DOMAINNAME]", oRenewalReminderData.DomaiName);
            sbMailContent.Replace("[DATEOFEXPIRE]", Convert.ToDateTime(oRenewalReminderData.CertificateExpireOnDate).ToShortDateString());
            sbMailContent.Replace("[PRODUCTNAME]", oRenewalReminderData.ProductName);
            sbMailContent.Replace("[SITEALIAS]", oRenewalReminderData.SiteAlias);
            sbMailContent.Replace("[DAYS]", Convert.ToDateTime(oRenewalReminderData.CertificateExpireOnDate.Value.ToShortDateString()).Subtract(DateTime.Today).Days.ToString()); ;

            sbMailContent.Replace("[FIRSTNAME]", oRenewalReminderData.FirstName);
            sbMailContent.Replace("[LASTNAME]", oRenewalReminderData.LastName);

            sbMailContent.Replace("[EMAIL]", oRenewalReminderData.Email);
            sbMailContent.Replace("[PHONE]", oRenewalReminderData.Phone);
            sbMailContent.Replace("[COMPANYNAME]", oRenewalReminderData.Company);
            sbMailContent.Replace("[ADDRESS1]", oRenewalReminderData.Street);
            sbMailContent.Replace("[ADDRESS2]", string.Empty);
            sbMailContent.Replace("[CITY]", oRenewalReminderData.City);
            sbMailContent.Replace("[STATE]", oRenewalReminderData.State);
            sbMailContent.Replace("[ZIP]", oRenewalReminderData.Zip);
            sbMailContent.Replace("[COUNTRY]", oRenewalReminderData.Country);

            sbMailContent.Replace("[PROMOCODE]", string.Empty);

            ProductPricing pricing = (from c in DbContext.Contracts
                                      from pp in DbContext.ProductPricings
                                           .Where(p => p.ContractID == c.ID)
                                      where c.RecordStatusID == (int)RecordStatus.ACTIVE && c.isForReseller == false && c.SiteID == oRenewalReminderData.SiteID
                                        && pp.SiteID == oRenewalReminderData.SiteID && pp.RecordStatusID == (int)RecordStatus.ACTIVE
                                        && pp.ProductID == oRenewalReminderData.ProductID && pp.NumberOfMonths == oRenewalReminderData.NumberOfMonths
                                      select pp).ToList().FirstOrDefault();

            sbMailContent.Replace("[ADDTOCARTURL]", strURL + "/d/shoppingcart/addtocart?ppid=" + pricing.ID + "&qty=1");
            sbMailContent.Replace("[RENEWALPRICE]", string.Format("{0:C}", pricing.SalesPrice));

            return sbMailContent.ToString();
        }

        public string ReplaceResellerMailMerge(string strMailContent, List<RenewalReminderData> oRenewalReminderData)
        {
            StringBuilder sbMailContent = new StringBuilder(strMailContent);
            sbMailContent.Replace("[FULLNAME]", oRenewalReminderData[0].FirstName + " " + oRenewalReminderData[0].LastName);

            bool bUseSSL = (GetSiteSettingValue(oRenewalReminderData[0].SiteID, SettingConstants.CURRENT_USESSL_KEY).ToLower() == Boolean.TrueString.ToLower());
            bool bRunWithWWW = (GetSiteSettingValue(oRenewalReminderData[0].SiteID, SettingConstants.CURRENT_IS_SITE_RUN_WITH_WWW).ToLower() == Boolean.TrueString.ToLower());
            string strURL = "http://" + (bRunWithWWW ? "www." : string.Empty) + oRenewalReminderData[0].SiteAlias;
            string strSupportScript = GetSiteSettingValue(oRenewalReminderData[0].SiteID, SettingConstants.CURRENT_SITELIVECHATE_KEY);

            sbMailContent.Replace("[SUPPORTURL]", strSupportScript);
            sbMailContent.Replace("[SITEURL]", strURL);
            sbMailContent.Replace("[LOGINURL]", (bUseSSL ? strURL.Replace("http:", "https:") : strURL) + "/logon");
            sbMailContent.Replace("[SITEALIAS]", oRenewalReminderData[0].SiteAlias);

            //Replace Product list
            StringBuilder sbProductList = new StringBuilder();
            sbProductList.Append("<style type='text/css'>");
            sbProductList.Append(".InvoiceDetailHeader{font-family: Arial, Helvetica, sans-serif;color: #FFFFFF;font-size: 14px;font-weight: bold;padding: 8px;background-color: #6ba946;}");
            sbProductList.Append(".InvoiceDetailTd{font-family: Arial, Helvetica, sans-serif;color: #333333;font-size: 12px;font-weight: bold;padding: 8px;border-right: #6ba946 dashed 1px;border-bottom: #6ba946 dashed 1px;border-left: #6ba946 dashed 1px;}");
            sbProductList.Append("</style>");

            sbProductList.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0'>");
            sbProductList.Append("<tr>");
            sbProductList.Append("<th align='left' class='InvoiceDetailHeader'>");
            sbProductList.Append("Domain Name");
            sbProductList.Append("</th>");
            sbProductList.Append("<th align='left' class='InvoiceDetailHeader'>");
            sbProductList.Append("Product Name");
            sbProductList.Append("</th>");
            sbProductList.Append("<th align='left' class='InvoiceDetailHeader'>");
            sbProductList.Append("Date Of Expire");
            sbProductList.Append("</th>");
            sbProductList.Append("<th align='left' class='InvoiceDetailHeader'>");
            sbProductList.Append("Buy Cert");
            sbProductList.Append("</th>");

            sbProductList.Append("</tr>");
            foreach (RenewalReminderData cert in oRenewalReminderData)
            {
                DomaiNames += cert.DomaiName + ",";
                sbProductList.Append("<tr>");
                sbProductList.Append("<td align='left' class='InvoiceDetailTd'>");
                sbProductList.Append(cert.DomaiName);
                sbProductList.Append("</td>");
                sbProductList.Append("<td align='left' class='InvoiceDetailTd'>");
                sbProductList.Append(cert.ProductName);
                sbProductList.Append("</td>");
                sbProductList.Append("<td align='left' class='InvoiceDetailTd'>");
                sbProductList.Append(cert.CertificateExpireOnDate.Value.ToShortDateString());
                sbProductList.Append("</td>");

                ProductPricing pricing = (from rc in DbContext.ResellerContracts
                                          from pp in DbContext.ProductPricings
                                               .Where(p => p.ContractID == rc.ContractID)
                                          where rc.UserID == cert.UserID && pp.SiteID == cert.SiteID && pp.RecordStatusID == (int)RecordStatus.ACTIVE
                                                    && pp.ProductID == cert.ProductID && pp.NumberOfMonths == cert.NumberOfMonths
                                                    && rc.SiteID == cert.SiteID
                                          select pp).ToList().FirstOrDefault();


                sbProductList.Append("<td align='left' class='InvoiceDetailTd'>");
                sbProductList.Append("<a href='" + strURL + "/d/shoppingcart/addtocart?ppid=" + pricing.ID + "&qty=1'>Buy Now</a>");
                sbProductList.Append("</td>");
            }
            sbProductList.Append("</table>");

            sbMailContent.Replace("[EXPIRELIST]", sbProductList.ToString());

            sbMailContent.Replace("[COMPANYNAME]", oRenewalReminderData[0].Company);
            sbMailContent.Replace("[FIRSTNAME]", oRenewalReminderData[0].FirstName);
            sbMailContent.Replace("[LASTNAME]", oRenewalReminderData[0].LastName);
            sbMailContent.Replace("[EMAIL]", oRenewalReminderData[0].Email);
            sbMailContent.Replace("[ADDRESS1]", oRenewalReminderData[0].Street);
            sbMailContent.Replace("[ADDRESS2]", string.Empty);
            sbMailContent.Replace("[CITY]", oRenewalReminderData[0].City);
            sbMailContent.Replace("[STATE]", oRenewalReminderData[0].State);
            sbMailContent.Replace("[ZIP]", oRenewalReminderData[0].Zip);
            sbMailContent.Replace("[COUNTRY]", oRenewalReminderData[0].Country);
            sbMailContent.Replace("[PHONE]", oRenewalReminderData[0].Phone);
            sbMailContent.Replace("[DAYS]", oRenewalReminderData[0].CertificateExpireOnDate.Value.Subtract(DateTime.Now).Days.ToString());

            return sbMailContent.ToString();
        }

        public string ReplaceResellerCustomerMailMerge(string strMailContent, RenewalReminderData oRenewalReminderData)
        {
            StringBuilder sbMailContent = new StringBuilder(strMailContent);

            string strPhone = GetSiteSettingValue(oRenewalReminderData.SiteID, SettingConstants.CURRENT_SITEPNONE_KEY);

            bool bUseSSL = (GetSiteSettingValue(oRenewalReminderData.SiteID, SettingConstants.CURRENT_USESSL_KEY).ToLower() == Boolean.TrueString.ToLower());
            bool bRunWithWWW = (GetSiteSettingValue(oRenewalReminderData.SiteID, SettingConstants.CURRENT_IS_SITE_RUN_WITH_WWW).ToLower() == Boolean.TrueString.ToLower());
            string strCurrency = GetSiteSettingValue(oRenewalReminderData.SiteID, SettingConstants.CURRENT_SITECURRANCY_KEY);
            string strCulture = GetSiteSettingValue(oRenewalReminderData.SiteID, SettingConstants.CURRENT_SITECULTURE_KEY);
            string strURL = "http://" + (bRunWithWWW ? "www." : string.Empty) + oRenewalReminderData.SiteAlias;
            string strSupportScript = GetSiteSettingValue(oRenewalReminderData.SiteID, SettingConstants.CURRENT_SITELIVECHATE_KEY);

            SwitchCulture(oRenewalReminderData.LanguageCode, string.IsNullOrEmpty(strCulture) ? oRenewalReminderData.LanguageCode : strCulture);

            sbMailContent.Replace("[SUPPORTURL]", strSupportScript);
            sbMailContent.Replace("[SITEPHONENUMBER]", !string.IsNullOrEmpty(strPhone) ? strPhone : string.Empty);
            sbMailContent.Replace("[CONTECTNUMBER]", !string.IsNullOrEmpty(strPhone) ? strPhone : string.Empty);
            sbMailContent.Replace("[LOGINURL]", (bUseSSL ? strURL.Replace("http:", "https:") : strURL) + "/logon");
            sbMailContent.Replace("[SITEURL]", strURL);
            sbMailContent.Replace("[DOMAINNAME]", oRenewalReminderData.DomaiName);
            sbMailContent.Replace("[DATEOFEXPIRE]", Convert.ToDateTime(oRenewalReminderData.CertificateExpireOnDate).ToShortDateString());
            sbMailContent.Replace("[PRODUCTNAME]", oRenewalReminderData.ProductName);
            sbMailContent.Replace("[SITEALIAS]", oRenewalReminderData.SiteAlias);
            sbMailContent.Replace("[DAYS]", Convert.ToDateTime(oRenewalReminderData.CertificateExpireOnDate.Value.ToShortDateString()).Subtract(DateTime.Today).Days.ToString());

            sbMailContent.Replace("[RESELLERFIRSTNAME]", oRenewalReminderData.FirstName);
            sbMailContent.Replace("[RESELLERLASTNAME]", oRenewalReminderData.LastName);

            sbMailContent.Replace("[RESELLEREMAIL]", oRenewalReminderData.Email);
            sbMailContent.Replace("[RESELLERPHONE]", oRenewalReminderData.Phone);
            sbMailContent.Replace("[RESELLERCOMPANYNAME]", oRenewalReminderData.Company);
            sbMailContent.Replace("[RESELLERADDRESS1]", oRenewalReminderData.Street);
            sbMailContent.Replace("[RESELLERADDRESS2]", string.Empty);
            sbMailContent.Replace("[RESELLERCITY]", oRenewalReminderData.City);
            sbMailContent.Replace("[RESELLERSTATE]", oRenewalReminderData.State);
            sbMailContent.Replace("[RESELLERZIP]", oRenewalReminderData.Zip);
            sbMailContent.Replace("[RESELLERCOUNTRY]", oRenewalReminderData.Country);

            sbMailContent.Replace("[CUSTOMERFULLNAME]", oRenewalReminderData.ACFirstName + " " + oRenewalReminderData.ACFirstName);
            sbMailContent.Replace("[CUSTOMERFIRSTNAME]", oRenewalReminderData.ACFirstName);
            sbMailContent.Replace("[CUSTOMERLASTNAME]", oRenewalReminderData.ACLastName);
            sbMailContent.Replace("[CUSTOMEREMAIL]", oRenewalReminderData.ACEmail);
            sbMailContent.Replace("[CUSTOMERPHONE]", oRenewalReminderData.ACPhone);

            sbMailContent.Replace("[PROMOCODE]", string.Empty);

            ProductPricing pricing = (from rc in DbContext.ResellerContracts
                                      from pp in DbContext.ProductPricings
                                           .Where(p => p.ContractID == rc.ContractID)
                                      where rc.UserID == oRenewalReminderData.UserID && pp.SiteID == oRenewalReminderData.SiteID && pp.RecordStatusID == (int)RecordStatus.ACTIVE
                                                && pp.ProductID == oRenewalReminderData.ProductID && pp.NumberOfMonths == oRenewalReminderData.NumberOfMonths
                                                && rc.SiteID == oRenewalReminderData.SiteID
                                      select pp).ToList().FirstOrDefault();

            sbMailContent.Replace("[ADDTOCARTURL]", strURL + "/d/shoppingcart/addtocart?ppid=" + pricing.ID + "&qty=1");
            sbMailContent.Replace("[RENEWALPRICE]", string.Format("{0:C}", pricing.SalesPrice));

            return sbMailContent.ToString();
        }

        public string GetSiteSettingValue(int SiteID, string SettingName)
        {
            string strValue = string.Empty;

            List<SiteSettings> sitesettings = (from ss in DbContext.Settings
                                               where ss.SiteID == SiteID && ss.Key == SettingName
                                               select ss).ToList();
            if (sitesettings != null && sitesettings.Count > 0)
                return sitesettings.ToList().FirstOrDefault().Value;
            else
                return string.Empty;
        }

        public EmailTemplates GetUserEmailTemplate(EmailType eEmailType, int SiteID, int LangID)
        {
            EmailTemplates eEmailTemplates = (from et in DbContext.EmailTemplateses
                                              where et.SiteID == SiteID && et.isActive == true && et.EmailTypeId == (int)eEmailType
                                                      && et.LangID == LangID
                                              select et).ToList().FirstOrDefault();

            if (eEmailTemplates != null)
                logger.Log("EmailTemplate fetched successfully for " + eEmailType.ToString() + " and SiteID : " + SiteID, Logger.LogType.REMINDER);
            else
                logger.Log("EmailTemplate not found for " + eEmailType.ToString() + " and SiteID : " + SiteID, Logger.LogType.REMINDER);

            return eEmailTemplates;
        }

        public void SwitchCulture(string langCode, string culturekey)
        {
            if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToLower().Equals(langCode, StringComparison.OrdinalIgnoreCase))
                return;

            System.Globalization.CultureInfo ci = CultureInfo.CreateSpecificCulture(langCode); ;
            Thread.CurrentThread.CurrentUICulture = ci;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culturekey);
        }
    }

    public class RenewalReminderData
    {
        //LoginURL and AddToCartURL
        public string FirstName
        { get; set; }
        public string LastName
        { get; set; }
        public string DomaiName
        { get; set; }
        public DateTime? CertificateExpireOnDate
        { get; set; }
        public string ProductName
        { get; set; }
        public decimal RenewalPrice
        { get; set; }
        public string SiteAlias
        { get; set; }
        public string SitePhoneNumber
        { get; set; }
        public int UserID
        { get; set; }
        public int ProductID
        { get; set; }
        public int UserTypeID
        { get; set; }
        public int SiteID
        { get; set; }
        public string Email
        { get; set; }
        public string AlternatEmail
        { get; set; }
        public int SiteSMTP
        { get; set; }
        public string Phone
        { get; set; }
        public string Company
        { get; set; }
        public string Street
        { get; set; }
        public string City
        { get; set; }
        public string State
        { get; set; }
        public string Zip
        { get; set; }
        public string Country
        { get; set; }
        public int NumberOfMonths
        { get; set; }
        public string LanguageCode
        { get; set; }
        /// <summary>
        /// Only use in case of reseller's customer, in other case this properties value is blank
        /// </summary>
        public string ACFirstName
        { get; set; }
        /// <summary>
        /// Only use in case of reseller's customer, in other case this properties value is blank
        /// </summary>
        public string ACLastName
        { get; set; }
        /// <summary>
        /// Only use in case of reseller's customer, in other case this properties value is blank
        /// </summary>
        public string ACEmail
        { get; set; }
        /// <summary>
        /// Only use in case of reseller's customer, in other case this properties value is blank
        /// </summary>
        public string ACPhone
        { get; set; }
    }
}
