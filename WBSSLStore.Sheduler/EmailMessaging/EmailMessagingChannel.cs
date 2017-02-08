using System;
using System.Net.Mail;
using System.Net;
using WBSSLStore.Domain;
using WBSSLStore.Data.Infrastructure;
using System.Net.Mime;
using System.Data;
using System.Linq;
using WBSSLStore.Data;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Configuration;
using System.Data.SqlClient;

namespace WBSSLStore.Scheduler.EmailMessaging
{

    public class EmailMessagingChannel : Domain.Disposable
    {
        private  WBSSLStoreDb _db = null;
        private  MessageContext context = null;
        private  Logger.Logger logger = null;
        //private static StringBuilder sb = null;

        private  WBSSLStoreDb objDbContext
        {
            get { return _db ?? (_db = new WBSSLStoreDb()); }
            set { _db = value; }
        }

        //public  void Main(string[] args)
        //{
        //    logger = new Logger.Logger();
        //    SendMessage();
        //}



        protected override void Disposer()
        {
            if (_db != null)
                _db.Dispose();
            logger = null;
        }
        public  void SendMessage()
        {
            try
            {
                if (logger == null)
                    logger = new Logger.Logger();

                //logger.Log("-------Start Sending Mails:" + DateTime.Now + "-------", Logger.LogType.EMAILQUEUE);

                int iCount = 0, iMaxCount = Convert.ToInt16(ConfigurationManager.AppSettings["MaxNoOfTries"]);
                var emailqueues = objDbContext.EmailQueues.Where(e => e.Site.isActive == true && e.NumberOfTries <= iMaxCount).EagerLoad(x => x.SiteSMTP).OrderBy(e => e.NumberOfTries).ToList();
                if (objDbContext != null)
                {
                    if (_db != null)
                    {
                        _db.Dispose();
                        _db = null;
                    }
                    objDbContext.Dispose();
                    objDbContext = null;
                }
                foreach (EmailQueue email in emailqueues)
                {
                    try
                    {
                        context = prepareMessage(email);
                        SendMail(context, iCount);
                    }
                    catch (SmtpException smtpex)
                    {
                        email.LastAttempt = DateTime.Now;
                        email.NumberOfTries = context.domain.NumberOfTries + 1;
                        email.LastErrorMessage = (smtpex.Message.Length > 126 ? smtpex.Message.Substring(0, 125) : smtpex.Message);

                        //objDbContext.EmailQueues.Attach(email);
                        //objDbContext.Entry<EmailQueue>(email).State = System.Data.Entity.EntityState.Modified;

                        SaveinDB(iCount, email, false);
                    }
                    catch (Exception ex)
                    {
                        email.LastAttempt = DateTime.Now;
                        email.NumberOfTries = context.domain.NumberOfTries + 1;
                        email.LastErrorMessage = (ex.Message.Length > 126 ? ex.Message.Substring(0, 125) : ex.Message);

                        //objDbContext.EmailQueues.Attach(context.domain);
                        //objDbContext.Entry<EmailQueue>(context.domain).State = System.Data.Entity.EntityState.Modified;


                        logger.LogException(ex);

                        SaveinDB(iCount, email, false);
                    }
                }


            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }
            finally
            {
                if (objDbContext != null)
                {
                    if (_db != null)
                    {
                        _db.Dispose();
                        _db = null;
                    }
                    objDbContext.Dispose();
                    objDbContext = null;
                }
                // logger.Log("-------End  Sending Mails:" + DateTime.Now + "-------", Logger.LogType.EMAILQUEUE);
            }
        }

        private  int SaveinDB(int iCount, EmailQueue email, bool isRemove)
        {
            try
            {
                using (AdoHelper db = new AdoHelper())
                {
                    string Query = isRemove ? string.Format("DELETE FROM dbo.EmailQueues  WHERE ID={0} AND SiteID={1}",email.ID,email.SiteID) : string.Format("UPDATE EmailQueues SET NumberOfTries={0} , LastAttempt=GETDATE(),LastErrorMessage='{1}' WHERE ID={2} AND SiteID={3}",email.NumberOfTries  , email.LastErrorMessage, email.ID,email.SiteID);
                    SqlCommand cmd = db.CreateCommand(Query, false);
                    iCount= db.ExecNonQuery(cmd,false);

                }

               // iCount += objDbContext.SaveChanges();
               // logger.Log("No of EmailQueue rows affected are : " + iCount, Logger.LogType.EMAILQUEUE);

            }
            catch (SqlException dbEx)
            {
                logger.LogException(dbEx);
            }

            return iCount;
        }

      
        private  MessageContext prepareMessage(EmailQueue email)
        {
            context = new MessageContext();
            context.SMTPSetting = email.SiteSMTP;
            context.domain = email;

            context.MailMessage.To.Add(GetValidEmailID(email.TO));
            context.MailMessage.From = new MailAddress(email.From, string.Empty, System.Text.Encoding.UTF8);

            if (!String.IsNullOrEmpty(email.CC))
                context.MailMessage.CC.Add(GetValidEmailID(email.CC));
            if (!String.IsNullOrEmpty(email.BCC))
                context.MailMessage.Bcc.Add(GetValidEmailID(email.BCC));
            if (!String.IsNullOrEmpty(email.ReplyTO))
                context.MailMessage.ReplyToList.Add(new MailAddress(email.ReplyTO));

            context.MailMessage.Subject = email.Subject;
            ContentType c = new ContentType(MediaTypeNames.Text.Html);
            AlternateView oAlternateView = AlternateView.CreateAlternateViewFromString(email.EmailContent, c);
            context.MailMessage.AlternateViews.Add(oAlternateView);

            context.MailMessage.Body = email.EmailContent;
            context.MailMessage.Priority = MailPriority.High;

            return context;
        }
        public  string GetValidEmailID(string strEmailID)
        {
            strEmailID = strEmailID.Trim();
            strEmailID = strEmailID.EndsWith(";") ? strEmailID.Substring(0, strEmailID.Length - 1) : strEmailID;
            return strEmailID.Replace(";", ",").Replace(" ", "");
        }
        public  void SendMail(MessageContext context, int iCount)
        {
            var smtpSettings = context.SMTPSetting;
            // can't process emails if the Smtp settings have not yet been set
            if (smtpSettings == null)
            {
                return;
            }

            using (SmtpClient smtpClient = new SmtpClient())
            {
                try
                {
                    smtpClient.UseDefaultCredentials = (!string.IsNullOrEmpty(smtpSettings.SMTPUser) && !string.IsNullOrEmpty(smtpSettings.SMTPPassword));
                    if (smtpClient.UseDefaultCredentials && !String.IsNullOrWhiteSpace(smtpSettings.SMTPUser))
                    {
                        smtpClient.Credentials = new NetworkCredential(smtpSettings.SMTPUser, smtpSettings.SMTPPassword);
                    }

                    if (context.MailMessage.To.Count == 0)
                    {
                        //Logger.Error("Recipient is missing an email address");
                        // sb.Append(string.Format("Recipient is missing an email address: {0} : SiteID is {1} and EmailQue ID : {2}{3}", context.MailMessage.To[0].Address, DateTimeWithZone.Now, context.domain.SiteID, context.domain.ID, Environment.NewLine));
                        return;
                    }

                    if (smtpSettings.SMTPHost != null)
                        smtpClient.Host = smtpSettings.SMTPHost;

                    smtpClient.Port = smtpSettings.SMTPPort;
                    smtpClient.EnableSsl = smtpSettings.UseSSL;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                    context.MailMessage.IsBodyHtml = context.MailMessage.Body != null && context.MailMessage.Body.Contains("<") && context.MailMessage.Body.Contains(">");
                    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                    smtpClient.Send(context.MailMessage);

                    //objDbContext.EmailQueues.Remove(context.domain);
                    SaveinDB(iCount, context.domain, true);
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }


    }


    //Model
    public class MessageContext
    {
        public MailMessage MailMessage { get; private set; }
        public EmailQueue domain { get; set; }
        public SiteSMTP SMTPSetting { get; set; }
        public bool MessagePrepared { get; set; }

        public MessageContext()
        {

            MailMessage = new MailMessage();
        }
    }
}
