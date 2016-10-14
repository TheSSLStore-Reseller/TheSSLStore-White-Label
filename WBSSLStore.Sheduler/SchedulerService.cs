using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.Configuration;

namespace WBSSLStore.Scheduler
{
    partial class SchedulerService : ServiceBase
    {
        Timer EmailMessaging = null;
        Timer OrderUpdation = null;
        Timer SendReminderMail = null;
        public SchedulerService()
        {
            InitializeComponent();
        } 
        protected override void OnStart(string[] args)
        {
            EmailMessaging = new Timer(300000);//(Convert.ToDouble(ConfigurationManager.AppSettings["RemiderFireDuration"]));            
            EmailMessaging.Enabled = true;
            EmailMessaging.AutoReset = true;
            EmailMessaging.Elapsed += new System.Timers.ElapsedEventHandler(EmailMessaging_Elapsed);

            OrderUpdation = new Timer(300000 * 12 * 24);//(Convert.ToDouble(ConfigurationManager.AppSettings["RemiderFireDuration"]));
            OrderUpdation.Enabled = true;
            OrderUpdation.AutoReset = true;
            OrderUpdation.Elapsed += new System.Timers.ElapsedEventHandler(OrderUpdation_Elapsed);

            SendReminderMail = new Timer(300000 * 12 * 24);
            SendReminderMail.Enabled = true;
            SendReminderMail.AutoReset = true;
            SendReminderMail.Elapsed += new ElapsedEventHandler(SendReminderMail_Elapsed);
        }

        protected void SendReminderMail_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var objData = new RenewalReminder.RenewalReminder(new Data.Infrastructure.DatabaseFactory());
                objData.SendMailToCustomer();
                objData.SendMailToReseller();
                objData.SendMailToRsellerCustomer();
                objData = null;
            }
            catch
            { }
        }
        protected void EmailMessaging_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {


            try
            {

                EmailMessaging.EmailMessagingChannel obj = new EmailMessaging.EmailMessagingChannel();
                obj.SendMessage();
                obj = null;

            }
            catch { }


        }
        protected void OrderUpdation_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                var obj = new WBSSLStore.Scheduler.OrderUpdation.OrderUpdation(new Data.Infrastructure.DatabaseFactory());
                obj.FetchApiOrders();
                obj = null;
            }
            catch
            {

            }

        }
        protected override void OnStop()
        {
            EmailMessaging.Enabled = false; // TODO: Add code here to perform any tear-down necessary to stop your service.
        }
    }
}
