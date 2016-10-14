namespace WBSSLStore.Scheduler
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SchedulerProjectInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.WBSchedulerInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // SchedulerProjectInstaller
            // 
            this.SchedulerProjectInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            this.SchedulerProjectInstaller.Password = null;
            this.SchedulerProjectInstaller.Username = null;
            // 
            // WBSchedulerInstaller
            // 
            this.WBSchedulerInstaller.Description = "WBScheduler for Email Messaging and Order Updation";
            this.WBSchedulerInstaller.DisplayName = "WBScheduler";
            this.WBSchedulerInstaller.ServiceName = "SchedulerService";
            this.WBSchedulerInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.SchedulerProjectInstaller,
            this.WBSchedulerInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller SchedulerProjectInstaller;
        private System.ServiceProcess.ServiceInstaller WBSchedulerInstaller;
    }
}