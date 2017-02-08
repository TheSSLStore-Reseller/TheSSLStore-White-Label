using System.ServiceProcess;

namespace WBSSLStore.Scheduler
{
    public static class Program
    {
        //static void Main(string[] args)
        //{
        //    ServiceBase[] ServicesToRun;
        //    ServicesToRun = new ServiceBase[]
        //    {
        //        new SchedulerService()
        //    };


        //    ServiceBase.Run(ServicesToRun);
        //}

        public static void Main(string[] args)
        {
            EmailMessaging.EmailMessagingChannel obj1 = new EmailMessaging.EmailMessagingChannel();
            obj1.SendMessage();
            obj1 = null;
            //var obj = new OrderUpdation.OrderUpdation(new Data.Infrastructure.DatabaseFactory());
            //obj.FetchApiOrders();
            //obj = null;

        }
    }
}
