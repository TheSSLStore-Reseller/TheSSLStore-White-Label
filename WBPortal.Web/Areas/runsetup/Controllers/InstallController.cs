using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using WhiteBrandShrink.Models;
using System.Security.Principal;
using System.Threading;
using System.Data;
using WBSSLStore.Domain;
using WBSSLStore.Data;
using System.Linq;
using System.Web.Security;
using System.Text;
using System.Configuration;
using WBSSLStore.Web.Helpers;
using System.Collections.Generic;
using WBSSLStore.Service.ViewModels;
using WhiteBrandShrink.Migrations;
using System.Web.Hosting;
using WhiteBrandShrink.Helper;
using WBSSLStore.Logger;

namespace WhiteBrandSite.Areas.runsetup.Controllers
{
    [RouteArea("runsetup")]
    public class InstallController : Controller
    {
        #region Properties
        public const string connectionStringName = "WhiteLabelConnection";
        private WBSSLStoreDb _db = null;
        public WBSSLStoreDb db
        {
            get
            {
                if (_db == null)
                {

                    _db = !string.IsNullOrEmpty(SetupSettings.DataBaseSetting.ConnectionString.ToString()) ? new WBSSLStoreDb(SetupSettings.DataBaseSetting.ConnectionString.ToString()) : new WBSSLStoreDb();
                }
                return _db;
            }
        }

        private SetupConfig _setupsettings = null;
        public SetupConfig SetupSettings
        {
            get
            {
                if (_setupsettings == null)
                {
                    using (HelpConfig)
                    {
                        _setupsettings = HelpConfig.GetAllSettings();
                    }
                }
                return _setupsettings;
            }
        }

        private ConfigurationHelper _helpconf = null;
        public ConfigurationHelper HelpConfig
        {
            get
            {
                if (_helpconf == null)
                {
                    _helpconf = new ConfigurationHelper();
                }
                return _helpconf;
            }
        }

        protected bool UseMars
        {
            get { return false; }
        }
        #endregion

        #region Step 1

        [HttpGet]
        public ActionResult installindex()
        {

            using (HelpConfig)
            {
                var re = GotoNextStep(null);
                if (re != null)
                    return re;
            }

            Server.ScriptTimeout = 600;
            var model = new InstallModel
            {
                DatabaseConnectionString = "",
                DataProvider = "sqlserver",
                //fast installation service does not support SQL compact
                DisableSqlCompact = false,// _config.UseFastInstallationService,
                SqlAuthenticationType = SQLAuthentication.SQL_SERVER_AUTHENTICATION.ToString(),
                SqlConnectionInfo = SQLServerInfo.SQLCONNECTIONVALUES.ToString(),
                SqlServerCreateDatabase = true,

                Collation = "SQL_Latin1_General_CP1_CI_AS",
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult indexpost(InstallModel model)
        {
            string errormsg = string.Empty;

            if (!ModelState.IsValid)
            {
                model.Errormessage = "Model is invalid";
                return View(model);
            }

            model.ConnectionTimeout = model.UseConnectionTimeout ? model.ConnectionTimeout : "300";
            Server.ScriptTimeout = Convert.ToInt32(model.ConnectionTimeout);
            model.DatabaseConnectionString = (model.DatabaseConnectionString != null) ? model.DatabaseConnectionString.Trim() : model.DatabaseConnectionString;
            model.DisableSqlCompact = false;


            if (model.DataProvider.Equals("sqlserver", StringComparison.InvariantCultureIgnoreCase))
            {
                if (model.SqlConnectionInfo.Equals(SQLServerInfo.SQLCONNECTIONSTRING.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    //raw connection string
                    if (string.IsNullOrEmpty(model.DatabaseConnectionString))
                        errormsg = "ConnectionString is Required";

                    try
                    {
                        //try to create connection string
                        new SqlConnectionStringBuilder(model.DatabaseConnectionString);
                    }
                    catch
                    {
                        errormsg = "Wrong ConnectionString Format";
                    }
                }
                else
                {
                    //values
                    if (string.IsNullOrEmpty(model.SqlServerName))
                        errormsg = "SQL Server Name Required";
                    if (string.IsNullOrEmpty(model.SqlDatabaseName))
                        errormsg = "Database Name Required";

                    //authentication type
                    if (model.SqlAuthenticationType.Equals(SQLAuthentication.SQL_SERVER_AUTHENTICATION.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        //SQL authentication
                        if (string.IsNullOrEmpty(model.SqlServerUsername))
                            errormsg = "SQL Server Username Required";
                        if (string.IsNullOrEmpty(model.SqlServerPassword))
                            errormsg = "SQL Server Password Required";
                    }
                }
                if (!string.IsNullOrEmpty(errormsg))
                    return Json(new { isSuccess = false, msg = errormsg }, JsonRequestBehavior.AllowGet);
            }

            var dirsToCheck = Helper.GetDirectoriesWrite();
            foreach (string dir in dirsToCheck)
                if (!Helper.CheckPermissions(dir, false, true, true, false))
                    errormsg = string.Format("The '{0}' account is not granted with Modify permission on folder '{1}'. Please configure these permissions", WindowsIdentity.GetCurrent().Name, dir);

            var filesToCheck = Helper.GetFilesWrite();
            foreach (string file in filesToCheck)
                if (!Helper.CheckPermissions(file, false, true, true, true))
                    errormsg = string.Format("The '{0}' account is not granted with Modify permission on file '{1}'. Please configure these permissions.", WindowsIdentity.GetCurrent().Name, file);

            if (!string.IsNullOrEmpty(errormsg))
                return Json(new { isSuccess = false, msg = errormsg }, JsonRequestBehavior.AllowGet);


            if (ModelState.IsValid)
            {
                try
                {
                    string connectionString = string.Empty;
                    if (model.DataProvider.Equals("sqlserver", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //SQL Server

                        if (model.SqlConnectionInfo.Equals(SQLServerInfo.SQLCONNECTIONSTRING.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            //raw connection string

                            //we know that MARS option is required when using Entity Framework
                            //let's ensure that it's specified
                            var sqlCsb = new SqlConnectionStringBuilder(model.DatabaseConnectionString);
                            if (UseMars)
                            {
                                sqlCsb.MultipleActiveResultSets = true;
                            }
                            connectionString = sqlCsb.ToString();
                        }
                        else
                        {
                            //values
                            connectionString = CreateConnectionString(model.SqlAuthenticationType.Equals(SQLAuthentication.INTEGRATED_WINDOWS_AUTHENTICATION.ToString(), StringComparison.OrdinalIgnoreCase),
                                (model.SqlServerName + (model.UseTCPPort ? "," + model.TCPPORT : string.Empty)), model.SqlDatabaseName,
                                model.SqlServerUsername, model.SqlServerPassword);
                            model.DatabaseConnectionString = connectionString;
                        }

                        //create connection string Start.
                        var config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
                        var connectionsec = (ConnectionStringsSection)config.GetSection("connectionStrings");
                        if (connectionsec.ConnectionStrings[connectionStringName] == null)
                            connectionsec.ConnectionStrings.Add(new ConnectionStringSettings(connectionStringName, connectionString));
                        else
                            connectionsec.ConnectionStrings[connectionStringName].ConnectionString = connectionString;
                        connectionsec.ConnectionStrings[connectionStringName].ProviderName = "System.Data.SqlClient";
                        config.Save(ConfigurationSaveMode.Modified);


                        ConfigurationManager.RefreshSection(config.ConnectionStrings.SectionInformation.SectionName);
                        var settings = ConfigurationManager.ConnectionStrings[connectionStringName];
                        var fi = typeof(ConfigurationElement).GetField("_bReadOnly", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        if (settings == null)
                        {
                            ConnectionStringSettings obj1 = new ConnectionStringSettings();
                            obj1.ConnectionString = connectionString;
                            obj1.ProviderName = "System.Data.SqlClient";
                            obj1.Name = connectionStringName;
                            settings = obj1;
                        }
                        fi.SetValue(settings, false);
                        settings.ConnectionString = connectionString;


                        if (model.SqlServerCreateDatabase)
                        {
                            if (!SqlServerDatabaseExists(connectionString))
                            {
                                //create database
                                var errorCreatingDatabase = CreateDatabase(connectionString, "SQL_Latin1_General_CP1_CI_AS");
                                if (!string.IsNullOrEmpty(errorCreatingDatabase))
                                    throw new Exception(errorCreatingDatabase);

                                //Database cannot be created sometimes. Weird! Seems to be Entity Framework issue
                                //that's just wait 6 seconds (3 seconds is not enough for some reasons)
                                Thread.Sleep(6000);
                            }
                        }
                        else
                        {
                            //check whether database exists
                            if (!SqlServerDatabaseExists(connectionString))
                                throw new Exception("Database does not exist or you don't have permissions to connect to it");
                        }
                    }
                    else
                    {
                        //SQL CE
                        string databaseFileName = "WBPortal.sdf";
                        string databasePath = @"|DataDirectory|\" + databaseFileName;
                        connectionString = "Data Source=" + databasePath + ";Persist Security Info=False";

                        //drop database if exists
                        string databaseFullPath = HostingEnvironment.MapPath("~/App_Data/") + databaseFileName;
                        if (System.IO.File.Exists(databaseFullPath))
                        {
                            System.IO.File.Delete(databaseFullPath);
                        }
                    }
                    //save settings
                    var dataProvider = model.DataProvider;
                    if (CreateAllDBsettings(model.DatabaseConnectionString))
                    {
                        SaveDBSettings(model, true);
                        Thread.Sleep(3000);
                        UpdateConfigStage(db, ConfigurationStage.CreateDB);
                        CreateDefaultStoreProcedure(model.DatabaseConnectionString, "");
                    }
                    else
                        return Json(new { isSuccess = false, msg = string.Format("Opps! General system error in create database. Please try again!") }, JsonRequestBehavior.AllowGet);

                    return Json(new { isSuccess = true, msg = string.Format("Data Base create successfully!"), url = Url.Action("paymentsettings", "install", new { area = "runsetup" }) }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception exception)
                {
                    Logger.Log_Exception(exception);
                    //clear provider settings if something got wrong
                    SaveDBSettings(model, false);
                    return Json(new { isSuccess = false, msg = string.Format("Setup failed: {0}", exception.Message) }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { isSuccess = false, msg = string.Format("Setup failed: {0}", "Try again!") }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Step 2
        [HttpGet]
        public ActionResult paymentsettings()
        {
         
            var re = GotoNextStep((int)ConfigurationStage.PaymentSetting);
            if (re != null)
                return re;

           

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult paymentsave(Paymentviewmodel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (db)
                    {
                        Site obsite = GetCurrentSite(db);

                        User objUser = GetDefaultAdminUser(db);
                        if (objUser == null)
                        {
                            objUser = CreateDefaultUser(db, obsite.ID);
                        }

                        PaymentGateways pg = new PaymentGateways();
                        try
                        {
                            var pgAuth = db.PaymentGateways.Where(x => x.SiteID.Equals(obsite.ID) && x.InstancesID.Equals((int)PGInstances.PayPalIPN)).FirstOrDefault();

                            pg = pgAuth != null ? pgAuth : pg;

                            pg.InstancesID = (int)PGInstances.PayPalIPN;
                            pg.IsTestMode = model.paymentmodel.EnableTestMode;
                            pg.KeyFilePath = string.Empty;
                            pg.LiveURL = "https://www.paypal.com/cgi-bin/webscr/";
                            pg.TestURL = "https://www.sandbox.paypal.com/cgi-bin/webscr/";
                            pg.SiteID = obsite.ID;
                            pg.Name = PGInstances.PayPalIPN.ToString();
                            pg.LoginID = model.paymentmodel.PaypalID;
                            pg.AuditDetails = new Audit();
                            pg.AuditID = pg.AuditDetails.ID;
                            pg.AuditDetails.ByUserID = objUser.ID;
                            pg.AuditDetails.DateCreated = DateTime.Now;
                            pg.AuditDetails.DateModified = DateTime.Now;
                            pg.AuditDetails.HttpHeaderDump = HttpContext.Request.Headers.ToString();
                            pg.AuditDetails.IP = HttpContext.Request.UserHostAddress;
                            if (pgAuth == null)
                                db.PaymentGateways.Add(pg);



                            pg = new PaymentGateways();
                            pgAuth = db.PaymentGateways.Where(x => x.SiteID.Equals(obsite.ID) && x.InstancesID.Equals((int)PGInstances.AuthorizeNet)).FirstOrDefault();
                            pg = pgAuth != null ? pgAuth : pg;
                            pg.InstancesID = (int)PGInstances.AuthorizeNet;
                            pg.IsTestMode = model.paymentmodel.EnableTestMode;
                            pg.KeyFilePath = string.Empty;
                            pg.LiveURL = "https://secure.authorize.net/gateway/transact.dll";
                            pg.TestURL = "https://test.authorize.net/gateway/transact.dll";
                            pg.SiteID = 1;
                            pg.Name = PGInstances.AuthorizeNet.ToString();
                            pg.LoginID = model.paymentmodel.AuthorizeNetloginID;
                            pg.TransactionKey = model.paymentmodel.AuthorizeNetTranKey;
                            pg.AuditDetails = new Audit();
                            pg.AuditID = pg.AuditDetails.ID;
                            pg.AuditDetails.ByUserID = objUser.ID;
                            pg.AuditDetails.DateCreated = DateTime.Now;
                            pg.AuditDetails.DateModified = DateTime.Now;
                            pg.AuditDetails.HttpHeaderDump = HttpContext.Request.Headers.ToString();
                            pg.AuditDetails.IP = HttpContext.Request.UserHostAddress;
                            if (pgAuth == null)
                                db.PaymentGateways.Add(pg);

                            db.SaveChanges();
                        }

                        catch (Exception ex)

                        {
                            return Json(new { isSuccess = false, msg = "Opps! Error in Payment settings saved. Please try again!" }, JsonRequestBehavior.AllowGet);
                        }

                        SiteSMTP objSmtp = new SiteSMTP();
                        try
                        {
                            var smtpset = db.SiteSmtps.Where(x => x.SiteID.Equals(obsite.ID)).FirstOrDefault();
                            objSmtp = smtpset != null ? smtpset : objSmtp;

                            objSmtp.SiteID = obsite.ID;
                            objSmtp.SMTPHost = model.SMTPmodel.SMTPHOST;
                            objSmtp.SMTPPort = Convert.ToInt32(model.SMTPmodel.SMTPPORT);
                            objSmtp.SMTPUser = model.SMTPmodel.SMTPUser;
                            objSmtp.SMTPPassword = model.SMTPmodel.SMTPPassword;
                            objSmtp.UseSSL = model.SMTPmodel.UserSSL;

                            objSmtp.AuditDetails = new Audit();
                            objSmtp.AuditID = objSmtp.AuditDetails.ID;
                            objSmtp.AuditDetails.ByUserID = objUser.ID;
                            objSmtp.AuditDetails.DateCreated = DateTime.Now;
                            objSmtp.AuditDetails.DateModified = DateTime.Now;
                            objSmtp.AuditDetails.HttpHeaderDump = HttpContext.Request.Headers.ToString();
                            objSmtp.AuditDetails.IP = HttpContext.Request.UserHostAddress;
                            if (smtpset == null)
                                db.SiteSmtps.Add(objSmtp);

                            db.SaveChanges();
                            UpdateConfigStage(db, ConfigurationStage.PaymentSetting);
                        }

                        catch (Exception ex)

                        {
                            return Json(new { isSuccess = false, msg = "Opps! Error in SMTP settings saved. Please try again!" }, JsonRequestBehavior.AllowGet);
                        }

                        using (HelpConfig)
                        {
                            var _AllSettings = HelpConfig.AllSettings;
                            _AllSettings.DataBaseSetting.IsFinishPaymentSetup = (pg != null && pg.ID > 0);
                            _AllSettings.DataBaseSetting.IsFinishSMTPSetup = (objSmtp != null && objSmtp.ID > 0);
                            HelpConfig.SaveDBSettingsFile(_AllSettings);

                            if (pg == null && objSmtp == null)
                                return Json(new { isSuccess = false, msg = "Payment and SMTP settings not saved!" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                return Json(new { isSuccess = true, msg = "", url = Url.Action("adminusersetup", "install", new { area = "runsetup" }) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, msg = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Step 3
        [HttpGet]
        public ActionResult adminusersetup()
        {
            var re = GotoNextStep((int)ConfigurationStage.AdminSetup);
            if (re != null)
                return re;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult adminusersave(AdminUserModel model)
        {


            if (ModelState.IsValid)
            {
                User objUser = new User();
                try
                {
                    using (db)
                    {
                        Site objSite = GetCurrentSite(db);

                        if (!db.Users.Any(x => x.Email.Equals(model.Email) && x.RecordStatusID.Equals((int)RecordStatus.ACTIVE) && x.SiteID.Equals(objSite.ID)))
                        {
                            objUser.FirstName = model.FullName.Split(' ')[0];
                            objUser.LastName = !string.IsNullOrEmpty(model.FullName.Replace(model.FullName.Split(' ')[0], string.Empty).Trim()) ? model.FullName.Replace(model.FullName.Split(' ')[0], string.Empty).Trim() : model.FullName.Split(' ')[0];
                            objUser.CompanyName = model.CompanyName;
                            objUser.AlternativeEmail = string.Empty;
                            objUser.Email = model.Email;
                            objUser.PasswordSalt = WBHelper.CreateSalt();
                            objUser.PasswordHash = WBHelper.CreatePasswordHash(model.Password, objUser.PasswordSalt);
                            objUser.ConfirmPassword = WBHelper.CreatePasswordHash(model.ConfirmPassword, objUser.PasswordSalt);
                            objUser.RecordStatus = RecordStatus.ACTIVE;
                            objUser.RecordStatusID = (int)RecordStatus.ACTIVE;
                            objUser.UserTypeID = (int)UserType.ADMIN;
                            objUser.UserType = UserType.ADMIN;
                            objUser.AuditDetails = new Audit();
                            objUser.AuditID = objUser.AuditDetails.ID;
                            objUser.AuditDetails.ByUserID = 0;
                            objUser.AuditDetails.DateCreated = DateTime.Now;
                            objUser.AuditDetails.DateModified = DateTime.Now;
                            objUser.AuditDetails.HttpHeaderDump = "SYSTEM";
                            objUser.AuditDetails.IP = Request.UserHostAddress;
                            CreateDefaultUser(db, objSite.ID, objUser, model.Phone);
                            //db.Users.Add(objUser);
                            var defalutuser = db.Users.Where(x => x.Email.Equals("admin@admin.com")).FirstOrDefault();
                            if (defalutuser != null)
                            {
                                defalutuser.ConfirmPassword = defalutuser.PasswordHash;
                                defalutuser.RecordStatus = RecordStatus.DELETED;
                                defalutuser.RecordStatusID = (int)RecordStatus.DELETED;
                                db.SaveChanges();
                            }
                            objSite.APIPartnerCode = model.LivePartnerCode;
                            objSite.APIAuthToken = model.LivePartnerAuthCode;
                            objSite.APIUsername = model.LivePartnerCode;
                            objSite.APIPassword = model.LivePartnerCode;
                            db.SaveChanges();

                            WBSSLStore.Gateways.RestAPIModels.Response.GetAllPricingResponse AllPrice = WBSSLStore.Gateways.RestAPIModels.Services.OrderService.GetAllProductPricing(objSite.APIPartnerCode, objSite.APIAuthToken);
                            var objResponse = ImportProducts(AllPrice.Product, objSite, 10);
                            UpdateConfigStage(db, ConfigurationStage.AdminSetup);
                            using (HelpConfig)
                            {
                                var _AllSettings = HelpConfig.GetAllSettings();
                                _AllSettings.DataBaseSetting.IsFinishAdminSetup = (objUser.ID > 0);
                                HelpConfig.SaveDBSettingsFile(_AllSettings);
                            }
                        }
                        else
                        {
                            return Json(new { isSuccess = false, msg = "Oops! Email address already exist. Enter another email address." }, JsonRequestBehavior.AllowGet);
                        }

                    }
                    return Json(new { isSuccess = true, url = Url.Action("generalsettings", "install", new { area = "runsetup" }), msg = "" }, JsonRequestBehavior.AllowGet);
                }

                catch (Exception ex)
                {
                    return Json(new { isSuccess = false, msg = "Opps! Error in admin user saved. Please try again!" }, JsonRequestBehavior.AllowGet);
                }
            }
            return View();
        }
        #endregion

        #region Step 4
        [HttpGet]
        public ActionResult generalsettings()
        {
            var re = GotoNextStep((int)ConfigurationStage.GeneralSetup);
            if (re != null)
                return re;

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult generalsettingssave(GeneralSettings model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (db)
                    {
                        var site = db.Sites.Where(x => x.isActive).OrderBy(x => x.ID).FirstOrDefault();
                        int siteid = site != null ? site.ID : 1;
                        SiteSettings item = db.Settings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITEFACEBOOK_KEY).FirstOrDefault();
                        SaveSettingsRow(item, db, SettingConstants.CURRENT_SITEFACEBOOK_KEY, model.FBUrl, siteid);

                        item = db.Settings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITETWITTER_KEY).FirstOrDefault();
                        SaveSettingsRow(item, db, SettingConstants.CURRENT_SITETWITTER_KEY, model.TwiterURL, siteid);

                        item = db.Settings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITEGOOGLEPLUS_KEY).FirstOrDefault();
                        SaveSettingsRow(item, db, SettingConstants.CURRENT_SITEGOOGLEPLUS_KEY, model.GPURL, siteid);

                        item = db.Settings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITEPNONE_KEY).FirstOrDefault();
                        SaveSettingsRow(item, db, SettingConstants.CURRENT_SITEPNONE_KEY, model.SitePhone, siteid);

                        item = db.Settings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITELANGUAGE_KEY).FirstOrDefault();
                        SaveSettingsRow(item, db, SettingConstants.CURRENT_SITELANGUAGE_KEY, model.SiteLanguage, siteid);

                        item = db.Settings.Where(ss => ss.Key.ToLower() == SettingConstants.TIMEZONE_DIFF_KEY).FirstOrDefault();
                        SaveSettingsRow(item, db, SettingConstants.TIMEZONE_DIFF_KEY, model.TimeZone, siteid);

                        item = db.Settings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITEBILLING_EMAIL_KEY).FirstOrDefault();
                        SaveSettingsRow(item, db, SettingConstants.CURRENT_SITEBILLING_EMAIL_KEY, model.BillingEmail, siteid);

                        item = db.Settings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITEADMIN_EMAIL_KEY).FirstOrDefault();
                        SaveSettingsRow(item, db, SettingConstants.CURRENT_SITEADMIN_EMAIL_KEY, model.AdminEmail, siteid);

                        item = db.Settings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITESUPPORT_EMAI_KEY).FirstOrDefault();
                        SaveSettingsRow(item, db, SettingConstants.CURRENT_SITESUPPORT_EMAI_KEY, model.SupportEmail, siteid);

                        item = db.Settings.Where(ss => ss.Key.ToLower() == SettingConstants.DEFAULT_CURRANCY_CODE).FirstOrDefault();
                        SaveSettingsRow(item, db, SettingConstants.DEFAULT_CURRANCY_CODE, model.BillingCurrency, siteid);                        
                        
                        if (site != null)
                        {
                            site.Alias = model.DomainName;
                            db.SaveChanges();
                        }
                        if (!string.IsNullOrEmpty(model.SupportEmail))
                        {
                            SqlParameter[] pParam = new SqlParameter[2];
                            pParam[0] = new SqlParameter("@fromEmail", model.SupportEmail);
                            pParam[1] = new SqlParameter("@SiteID", siteid);
                            db.Database.ExecuteSqlCommand("UPDATE dbo.EmailTemplates SET [From] = @fromEmail WHERE SiteID = 0 and isactive = 1", pParam);
                        }

                        List<PageUrl> productslugs = GeneralHelper.GetProductDetailSlugs();
                        int res = db.Database.ExecuteSqlCommand("INSERT INTO dbo.SiteLanguages (SiteID,[LangID]) SELECT @SiteID,ID FROM dbo.Languages WHERE   RecordStatusID = 1", new SqlParameter("@SiteID", siteid));

                        CMSPage objCmsPage = null;
                        int cnt = 0;
                        var languages = db.Languages.Where(x => x.LangCode.Replace("\n", "").Equals(model.SiteLanguage)).FirstOrDefault();
                        int langid = languages != null ? languages.ID : db.Languages.Where(x => x.LangCode.Replace("\n", "").Equals("en")).FirstOrDefault().ID;

                        foreach (PageUrl objItem in productslugs)
                        {

                            objCmsPage = new CMSPage();
                            objCmsPage.Pages = new Pages();
                            objCmsPage.Pages.BrandID = 99;
                            objCmsPage.Pages.Caption = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(objItem.Title);
                            objCmsPage.Pages.DisplayOrder = cnt + 1;
                            objCmsPage.Pages.EndDate = DateTime.Now.AddYears(10);
                            objCmsPage.Pages.PageStatus = PageStatus.HideInNavigation;
                            objCmsPage.Pages.PageStatusID = (int)PageStatus.HideInNavigation;
                            objCmsPage.Pages.ParentID = 0;
                            objCmsPage.Pages.SiteID = site.ID;
                            objCmsPage.Pages.slug = objItem.SlugUrl;
                            objCmsPage.Pages.StartDate = DateTime.Now;
                            objCmsPage.Pages.URLTarget = URLTarget._self;
                            objCmsPage.Pages.URLTargetID = (int)URLTarget._self;


                            objCmsPage.PageID = objCmsPage.Pages.ID;
                            objCmsPage.LangID = langid;
                            objCmsPage.Keywords = objItem.Keywords;
                            objCmsPage.Title = objItem.Title;
                            objCmsPage.Description = objItem.Description;
                            db.CmsPages.Add(objCmsPage);
                            cnt++;
                        }
                        db.SaveChanges();
                        List<ProductDetail> productdetailsdata = GeneralHelper.GetProductDetailsData();
                        List<Product> allpro = db.Products.Where(x => x.RecordStatusID.Equals((int)RecordStatus.ACTIVE)).ToList();

                        ProductDetail objProddetail = null;
                        try
                        {
                            foreach (Product objpro in allpro)
                            {
                                try
                                {
                                    objProddetail = new ProductDetail();
                                    objProddetail = productdetailsdata.Where(x => x.productcode.Equals(objpro.InternalProductCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();


                                    if (objProddetail != null)
                                    {
                                        objProddetail.ProductID = objpro.ID;
                                        db.ProductDetail.Add(objProddetail);
                                    }
                                }
                                catch
                                {
                                    throw;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        db.SaveChanges();

                        User obj = db.Users.Where(x => x.SiteID == site.ID && x.UserTypeID.Equals((int)UserType.ADMIN) && x.RecordStatusID.Equals((int)RecordStatus.ACTIVE)).OrderByDescending(x => x.ID).FirstOrDefault();
                        using (HelpConfig)
                        {
                            var _AllSettings = HelpConfig.GetAllSettings();
                            _AllSettings.DataBaseSetting.IsFinishGeneralSetup = (obj.ID > 0);
                            HelpConfig.SaveDBSettingsFile(_AllSettings);
                        }

                        FormsAuthentication.SetAuthCookie(obj.Email, false);
                        Request.RequestContext.HttpContext.User = new GenericPrincipal(new GenericIdentity(obj.Email, "Forms"), null);
                        UpdateConfigStage(db, ConfigurationStage.GeneralSetup);

                    }
                    return Json(new { isSuccess = true, url = Url.Action("Index", "home", new { area = "admin" }) }, JsonRequestBehavior.AllowGet);
                }

                catch (Exception ex)
                {
                    new Logger().LogException(ex);
                    return Json(new { isSuccess = false, msg = "Error on Save data. Please Try again" }, JsonRequestBehavior.AllowGet);
                    
                }

            }
            return Json(new { isSuccess = false, msg = "Model is not valid. Please Try again" }, JsonRequestBehavior.AllowGet);
        }
        #endregion



        #region Custom Method
        private ActionResult GotoNextStep(int? currentstage)
        {
            string path = string.Empty;
            var allsettings = HelpConfig.GetAllSettings();
            string connectionstring = ConfigurationManager.ConnectionStrings[connectionStringName] != null ? ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString : string.Empty;

            DataBaseSettings dbsettings = allsettings != null && allsettings.DataBaseSetting != null ? allsettings.DataBaseSetting : null;
            if (dbsettings != null && !string.IsNullOrEmpty(connectionstring))
            {
                var eConfigset = WBHelper.GetSiteConfiguration(dbsettings.ConnectionString);
                if ((int)ConfigurationStage.NoCreated != (int)(ConfigurationStage)eConfigset)
                {
                    if ((int)ConfigurationStage.GeneralSetup == (int)(ConfigurationStage)eConfigset)
                    {
                        // path = "StaticRender";
                        return RedirectToAction("StaticRender", "StaticPage", new { slug = "/" });
                    }
                    else if ((int)ConfigurationStage.AdminSetup == (int)(ConfigurationStage)eConfigset)
                    {
                        // path = "generalsettings";
                        if (currentstage != null && currentstage == (int)ConfigurationStage.GeneralSetup)
                        {
                            return null;
                        }
                        else
                            return RedirectToAction("generalsettings");
                    }
                    else if ((int)ConfigurationStage.PaymentSetting == (int)(ConfigurationStage)eConfigset)
                    {
                        //path = "adminusersetup";
                        if (currentstage != null && currentstage == (int)ConfigurationStage.AdminSetup)
                        {
                            return null;
                        }
                        else
                            return RedirectToAction("adminusersetup");
                    }
                    else if ((int)ConfigurationStage.CreateDB == (int)(ConfigurationStage)eConfigset)
                    {
                        // path = "paymentsettings";
                        if (currentstage != null && currentstage == (int)ConfigurationStage.PaymentSetting)
                        {
                            return null;
                        }
                        else
                            return RedirectToAction("paymentsettings");
                    }
                    else
                        path = "installindex";
                    //return RedirectToAction("paymentsettings");
                }
            }
            return null;
        }
        public Contract GetClientContract(int SiteID)
        {

            Contract resellerContract = db.Contracts.Where(cn => cn.SiteID == SiteID && cn.isAutoCalculation == false && cn.isForReseller == true && cn.ContractLevel == null && cn.RecordStatusID != (int)RecordStatus.DELETED).OrderBy(cn => cn.ID).FirstOrDefault();

            if (resellerContract == null)
            {
                resellerContract = new Contract();
                resellerContract.ContractLevel = null;
                resellerContract.ContractName = "Reseller Default Contract";
                resellerContract.isAutoCalculation = false;
                resellerContract.isForReseller = true;
                resellerContract.RecordStatusID = (int)RecordStatus.ACTIVE;
                resellerContract.SiteID = SiteID;

                db.Contracts.Add(resellerContract);
                db.SaveChanges();

            }

            Contract objContract = db.Contracts.Where(cn => cn.SiteID == SiteID && cn.isAutoCalculation == false && cn.isForReseller == false && cn.ContractLevel == null).FirstOrDefault();
            if (objContract != null)
                return objContract;
            else
            {
                objContract = new Contract();
                objContract.ContractLevel = null;
                objContract.ContractName = "Client Contract";
                objContract.isAutoCalculation = false;
                objContract.isForReseller = false;
                objContract.SiteID = SiteID;
                objContract.RecordStatusID = (int)RecordStatus.ACTIVE;
                db.Contracts.Add(objContract);
                db.SaveChanges();
                return objContract;
            }

           
        }
        private User GetDefaultAdminUser(WBSSLStoreDb db)
        {
            User objUser = db.Users.Where(x => x.RecordStatusID.Equals((int)RecordStatus.ACTIVE) && x.UserTypeID.Equals((int)UserType.ADMIN)).FirstOrDefault();
            return objUser;
        }

        private static Site GetCurrentSite(WBSSLStoreDb db)
        {
            return db.Sites.Where(x => x.isActive).ToList().FirstOrDefault();
        }

        public bool UpdateConfigStage(WBSSLStoreDb db, ConfigurationStage econfigstage)
        {
            try
            {
                var pSites = GetCurrentSite(db);

                if (pSites != null)
                {
                    SiteSettings item = db.Settings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITE_CONFIGURATIONSTATUS).FirstOrDefault();
                    SaveSettingsRow(item, db, SettingConstants.CURRENT_SITE_CONFIGURATIONSTATUS, ((int)econfigstage).ToString(), pSites.ID);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        
        private void SaveSettingsRow(SiteSettings item, WBSSLStoreDb db, string Key, string Value, int siteid)
        {

            if (item == null)
            {
                item = new SiteSettings();
                item.Key = Key;
                item.SiteID = siteid;
                item.Value = !string.IsNullOrEmpty(Value) ? Value : string.Empty;
                db.Settings.Add(item);
                db.SaveChanges();
            }
            else
            {
                item.Value = !string.IsNullOrEmpty(Value) ? Value : string.Empty;
                db.SaveChanges();
            }

        }

        private User CreateDefaultUser(WBSSLStoreDb db, int SiteID, User objUser = null, string Phone = "")
        {
            Address add = new Address();
            add.City = "St pete";
            add.CompanyName = objUser != null ? objUser.CompanyName : "system";
            add.Email = objUser != null ? objUser.Email : "admin@admin.com";
            add.CountryID = 5;
            add.Fax = "";
            add.Mobile = "";
            add.Phone = !string.IsNullOrEmpty(Phone) ? Phone : "134654632";
            add.State = "Florida";
            add.Street = "Stpet";
            add.Zip = "13245";
            db.Addresses.Add(add);
            db.SaveChanges();

            User u = new User();
            if (objUser != null)
            {
                u = objUser;
                u.SiteID = SiteID;
                u.AddressID = add.ID;
            }
            else
            {
                u.SiteID = SiteID;
                u.AddressID = add.ID;
                u.FirstName = "System";
                u.LastName = "Admin";
                u.Email = "admin@admin.com";
                u.CompanyName = "System";
                u.AlternativeEmail = "admin@admin.com";
                u.PasswordSalt = WBHelper.CreateSalt();
                u.PasswordHash =WBHelper.CreatePasswordHash("system20167", u.PasswordSalt);
                u.ConfirmPassword = WBHelper.CreatePasswordHash("system20167", u.PasswordSalt);
                u.RecordStatus = RecordStatus.ACTIVE;
                u.RecordStatusID = (int)RecordStatus.ACTIVE;
                u.UserTypeID = (int)UserType.ADMIN;
                u.UserType = UserType.ADMIN;
                u.AuditDetails = new Audit();
                u.AuditID = u.AuditDetails.ID;
                u.AuditDetails.ByUserID = 0;
                u.AuditDetails.DateCreated = DateTime.Now;
                u.AuditDetails.DateModified = DateTime.Now;
                u.AuditDetails.HttpHeaderDump = "SYSTEM";
                u.AuditDetails.IP = Request.UserHostAddress;

            }
            db.Users.Add(u);
            db.SaveChanges();
            return u;
        }
        private bool CreateAllDBsettings(string connectionstring)
        {
            try
            {
                return ConfigurationHelper.migration(connectionStringName, connectionstring);
            }
            catch
            {
                throw;
            }
        }
        private void SaveDBSettings(InstallModel model, bool isexist)
        {
            using (HelpConfig)
            {
                var _AllSettings = HelpConfig.GetAllSettings();

                _AllSettings.DataBaseSetting.ConnectionString = model.DatabaseConnectionString;
                _AllSettings.DataBaseSetting.ConnectionTimeout = Convert.ToInt32(model.ConnectionTimeout);
                _AllSettings.DataBaseSetting.IsWindowsAuthentication = model.SqlAuthenticationType.Equals(SQLAuthentication.INTEGRATED_WINDOWS_AUTHENTICATION.ToString(), StringComparison.OrdinalIgnoreCase);
                _AllSettings.DataBaseSetting.ServerName = model.SqlServerName;
                _AllSettings.DataBaseSetting.DatabaseName = model.SqlDatabaseName;
                _AllSettings.DataBaseSetting.UserName = model.SqlServerUsername;
                _AllSettings.DataBaseSetting.Password = model.SqlServerPassword;
                _AllSettings.DataBaseSetting.IsExists = isexist;
                _AllSettings.DataBaseSetting.IsFinishDBSetup = true;
                HelpConfig.SaveDBSettingsFile(_AllSettings);
            }
        }
        protected bool SqlServerDatabaseExists(string connectionString)
        {
            try
            {
                //just try to connect
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        protected string CreateConnectionString(bool trustedConnection,
           string serverName, string databaseName,
           string userName, string password, int timeout = 0)
        {
            var builder = new SqlConnectionStringBuilder();
            builder.IntegratedSecurity = trustedConnection;
            builder.DataSource = serverName;
            builder.InitialCatalog = databaseName;
            if (!trustedConnection)
            {
                builder.UserID = userName;
                builder.Password = password;
            }
            builder.PersistSecurityInfo = false;
            if (this.UseMars)
            {
                builder.MultipleActiveResultSets = true;
            }
            if (timeout > 0)
            {
                builder.ConnectTimeout = timeout;
            }

            return builder.ConnectionString;
        }
        protected string CreateDatabase(string connectionString, string collation)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;
                builder.InitialCatalog = "master";
                var masterCatalogConnectionString = builder.ToString();
                string query = string.Format("CREATE DATABASE [{0}]", databaseName);
                if (!String.IsNullOrWhiteSpace(collation))
                    query = string.Format("{0} COLLATE {1}", query, collation);
                using (var conn = new SqlConnection(masterCatalogConnectionString))
                {
                    conn.Open();
                    try
                    {
                        using (var command = new SqlCommand(query, conn))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    catch
                    { }
                    conn.Close();
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {

            }
        }


        protected string CreateDefaultStoreProcedure(string connectionString, string collation)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = connection;
                            cmd.CommandType = CommandType.Text;
                            connection.Open();
                            for (int i = 1; i <= 6; i++)
                            {
                                cmd.CommandText = readSPs(i);
                                cmd.ExecuteNonQuery();
                            }

                          
                            connection.Close();
                        }
                    }
                    catch
                    { }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
            }
        }

        public static string readSPs(int num)
        {


            string Query = string.Empty;

            switch (num)
            {
                case 1:
                    Query = @"CREATE PROCEDURE [dbo].[GetUserAccountDeatils] (@UserID as int,	@SiteID as INT) AS BEGIN	
	                        SET NOCOUNT ON;
	                        Declare @ContractName as Varchar(50),@TotalPurchase as money,@CuurentBalance as money,@TotalOrders as int,@TotalInCompleteOrders as int,@TotalSupportIncident as int
	                        
	                        SELECT @ContractName=ContractName from Contracts where ID=dbo.GetCurrentContractFromUserID(@UserID,@SiteID)
	                        
	                        SELECT @TotalPurchase=SUM(od.Price) from 
	                        Orders o inner join OrderDetails od on o.ID=od.OrderID 
	                        where od.OrderStatusID!=2 and o.UserID=@UserID and o.SiteID=@SiteID
	                        
	                        Select @CuurentBalance=SUM(TransactionAmount) from UserTransactions where UserID=@UserID and SiteID=@SiteID
	                        
                            
	                        Select @TotalOrders=count(od.ID)  from 
	                        Orders o inner join OrderDetails od on o.ID=od.OrderID 
	                        where od.OrderStatusID!=2 and o.UserID=@UserID and o.SiteID=@SiteID and isnull(od.ExternalOrderID,'')!='' and od.OrderStatusID!=2
	                        
	                        Select @TotalInCompleteOrders=count(od.ID)  from 
	                        Orders o inner join OrderDetails od on o.ID=od.OrderID 
	                        where od.OrderStatusID!=2 and o.UserID=@UserID and o.SiteID=@SiteID and isnull(od.ExternalOrderID,'')='' and od.OrderStatusID!=2
	                        
	                        select @TotalSupportIncident=COUNT(ID) from SupportRequests where UserID=@UserID and SiteID=@SiteID and isOpen=1
	                        
	                        select u.*,@ContractName as ContractName,
	                        a.DateCreated as RegisterDate,@TotalPurchase as TotalPurchase,@CuurentBalance as CuurentBalance,@TotalOrders as TotalOrders,@TotalInCompleteOrders as TotalInCompleteOrders
	                         from Users u inner join Audits a on u.AuditID=a.ID where u.ID=@UserID and SiteID=@SiteID 
	                        END";
                    break;
                case 2:
                    Query = @"Create PROCEDURE [dbo].[SSLAdmin_GetAdminDashBord] @SiteID INT    
	                         AS     
                             BEGIN    
                              
                                 DECLARE @Renew AS INT ,    
                                     @Orders AS INT ,    
                                     @ActiveCustomer AS INT ,    
                                     @RefundRequest AS INT ,    
                                     @ActiveContracts AS INT ,    
                                     @ActiveResellers AS INT ,    
                                     @InActiveResellers AS INT    
	                         --Renew    
                                 SELECT  @Renew = COUNT(od.ID)    
                                 FROM    dbo.OrderDetails od    
                                         INNER JOIN dbo.Orders o ON od.OrderID = o.ID    
                                 WHERE   o.SiteID = @SiteID    
                                         AND od.OrderStatusID = 1    
                                         AND ( DATEDIFF(day, GETDATE(), od.CertificateExpiresOn) <= 30 )      
                                             
	                         --Orders                    
                                 SELECT  @Orders = COUNT(OrderDetails.ID)    
                                 FROM    Orders    
                                         INNER JOIN OrderDetails ON Orders.ID = OrderDetails.OrderID    
                                 WHERE   Orders.SiteID = @SiteID    
                                         --AND ISNULL(OrderDetails.ExternalOrderID, '') <> ''
                                         AND dbo.OrderDetails.OrderStatusID != 2            
                              
	                         --Active Customer     
                                 SELECT  @ActiveCustomer = COUNT(ID)    
                                 FROM    Users    
                                 WHERE   SiteID = @SiteID    
                                         AND ISNULL(RecordStatusID, 0) = 1    
                                         AND ISNULL(UserTypeID, 0) = 0    
                                             
                                                
	                         --Refund Request     
                                 SELECT  @RefundRequest = COUNT(ID)    
                                 FROM    dbo.SupportRequests    
                                 WHERE   SiteID = @SiteID    
                                         AND isOpen = 1    
                                         --AND RefundStatusID = 1    
                              
	                         --Active Contracts     
                                 SELECT  @ActiveContracts = COUNT(ID)    
                                 FROM    dbo.Contracts    
                                 WHERE   SiteID = @SiteID    
                                         AND isForReseller = 1    
                                         AND RecordStatusID = 1    
                              
	                         --Active Resellers     
                                 SELECT  @ActiveResellers = COUNT(ID)    
                                 FROM    dbo.Users    
                                 WHERE   SiteID = @SiteID    
                                         AND ISNULL(RecordStatusID, 0) = 1    
                                         AND ISNULL(UserTypeID, 0) = 1    
                                             
	                         --InActive Resellers     
                                 SELECT  @InActiveResellers = COUNT(ID)    
                                 FROM    dbo.Users    
                                 WHERE   SiteID = @SiteID    
                                         AND ISNULL(RecordStatusID, 0) = 0    
                                         AND ISNULL(UserTypeID, 0) = 1    
                                             
                                 SELECT  @Renew Renew,    
                                         @Orders TotalOrders,    
                                         @ActiveCustomer ActiveUsers,    
                                         @RefundRequest RefundRequest,    
                                         @ActiveContracts ActiveContracts,    
                                         @ActiveResellers ActiveResellers,    
                                         @InActiveResellers InActiveResellers    
                                             
                             END ";
                    break;
                case 3:
                    Query = @"Create PROCEDURE [dbo].[SSLAdmin_GetSiteDefaultData] @APIPartnerCode INT
	                        AS 
                            BEGIN   
                                SELECT  dbo.Users.* ,
                                        dbo.Sites.CName ,
                                        dbo.Sites.Alias ,
                                        dbo.Sites.isActive ,
                                        dbo.Sites.DateCreated ,
                                        dbo.Sites.DateModified ,
                                        dbo.Sites.APIUsername ,
                                        dbo.Sites.APIPassword ,
                                        dbo.Sites.APIPartnerCode ,
                                        dbo.Sites.APIisInTest
                                FROM    dbo.Users
                                        INNER JOIN dbo.Sites ON dbo.Users.SiteID = dbo.Sites.ID
                                WHERE   Sites.APIPartnerCode = @APIPartnerCode
                                        AND dbo.Sites.isActive = 1
                            END";
                    break;
                case 4:
                    Query = @"Create PROCEDURE [dbo].[SSLAdmin_GetSites]  AS     BEGIN    Select * from sites END";
                    break;
                case 5:
                    Query = @"Create  FUNCTION [dbo].[Split](@data nvarchar(4000), @del char(1))
                            RETURNS @Result TABLE (Item nvarchar(4000))
                            AS
                            BEGIN
                                DECLARE @INDEX INT
                                DECLARE @part nvarchar(4000)
                                SET @INDEX = 1
                                WHILE @INDEX !=0
                                    BEGIN
                                    SET @INDEX = CHARINDEX(@del,@data)
                                    IF @INDEX !=0
                                    SET @part = LEFT(@data,@INDEX - 1)
                                    ELSE
                                    SET @part = @data
                                    INSERT INTO @Result(Item) VALUES(@part)
                                    SET @data = RIGHT(@data,LEN(@data) - @INDEX)
                                    IF LEN(@data) = 0 BREAK
                                END
                                RETURN
                            END";
                    break;

                case 6:
                    Query = @"CREATE FUNCTION [dbo].[GetCurrentContractFromUserID]  
                                (  
                                  @UserID AS INT ,  
                                  @SiteID AS INT   
                                )  
                            RETURNS INT  
                            AS   
                                BEGIN    
                                    DECLARE @ContractID AS INT  
                                    IF EXISTS ( SELECT  ID  
                                                FROM    ResellerContracts  
                                                WHERE   UserID = @UserID  
                                                        AND SiteID = @SiteID )   
                                        BEGIN  
                                 
                                            DECLARE @Balance AS MONEY ,  
                                                @TotalSale AS MONEY ,  
                                                @Total AS MONEY ,  
                                                @InitialFund AS MONEY ,  
                                                @isAutoCalculation AS INT  
                                 
                                            SELECT  @isAutoCalculation = ISNULL(@isAutoCalculation, 0)  
                                            FROM    Contracts  
                                            WHERE   SiteID = @SiteID  
                                                    AND ID = ( SELECT   ContractID  
                                                               FROM     ResellerContracts  
                                                               WHERE    UserID = @UserID  
                                                                        AND SiteID = @SiteID  
                                                             )  
                                  
                                            IF ( @isAutoCalculation = 1 )   
                                                BEGIN      
                                                    SET @Balance = ISNULL(( SELECT  SUM(UserTransactions.TransactionAmount)  
                                                                            FROM    UserTransactions  
                                                                            WHERE   UserID = @UserID  
                                                                                    AND SiteID = @SiteID  
                                                                          ), 0)          
                                                    SET @TotalSale = ISNULL(( SELECT    SUM(OrderDetails.Price)  
                                                                              FROM      Orders  
                                                                                        INNER JOIN OrderDetails ON OrderDetails.OrderID = Orders.ID  
                                                                              WHERE     OrderDetails.OrderStatusID <> 2  
                                                                                        AND Orders.UserID = @UserID  
                                                                                        AND SiteID = @SiteID  
                                                                            ), 0)            
                                                    SET @InitialFund = ISNULL(( SELECT  ISNULL(0, 0)  
                                                                                FROM    Users  
                                                                                WHERE   ID = @UserID  
                                                                                        AND SiteID = @SiteID  
                                                                              ), 0)             
                                                    SET @Total = ( @Balance + @TotalSale + @InitialFund )            
                                            
                                                    IF ( @Total > ( SELECT  MAX(ContractLevel)  
                                                                    FROM    dbo.Contracts  
                                                                    WHERE   SiteID = @SiteID  
                                                                  ) )   
                                                        SET @ContractID = ( SELECT  ID  
                                                                            FROM    Contracts  
                                                                            WHERE   ContractLevel = ( SELECT  
                                                                                          MAX(ContractLevel)  
                                                                                          FROM  
                                                                                          Contracts  
                                                                                          WHERE  
                                                                                          isAutoCalculation = 1  
                                                                                          AND isForReseller = 1  
                                                                                          AND ISNULL(RecordStatusID,  
                                                                                          0) = 1  
                                                                                          AND SiteID = @SiteID  
                                                                                          )  
                                                                          )  
                                                    ELSE   
                                                        SET @ContractID = ( SELECT TOP 1  
                                                                                    ID  
                                                                            FROM    Contracts  
                                                                            WHERE   isAutoCalculation = 1  
                                                                                    AND isForReseller = 1  
                                                                                    AND ContractLevel >= @Total  
                                                                                    AND ISNULL(RecordStatusID,  
                                                                                          0) = 1  
                                                                                    AND SiteID = @SiteID  
                                                                            ORDER BY ContractLevel  
                                                                          )    
                                    
                                                END  
                                            ELSE   
                                                BEGIN  
                                                    SELECT  @ContractID = ContractID  
                                                    FROM    ResellerContracts  
                                                    WHERE   UserID = @UserID  
                                                            AND SiteID = @SiteID  
                                   
                                                END  
                                        END  
                                    ELSE   
                                        BEGIN  
                               
                                            SELECT  @ContractID = ISNULL(ID, 0)  
                                            FROM    dbo.Contracts CS  
                                            WHERE   CS.ContractLevel IS NULL  
                                                    AND SiteID = @SiteID  
                                                    AND isAutoCalculation = 0  
                                                    AND isForReseller = 0  
                                    
                                        END    
                                    RETURN ISNULL(@ContractID,0)   
                                END ";
                    break;
            }
            return Query;
        }


        [AcceptVerbs(HttpVerbs.Post | HttpVerbs.Put | HttpVerbs.Delete)]
        public ActionResult filedelete(string hdndirectory, string name)
        {
            string serverpath = Server.MapPath("~") + "upload\\";
            if (System.IO.Directory.Exists(serverpath + hdndirectory))
            {
                System.IO.File.Delete(serverpath + hdndirectory + name); ;
            }
            return Json(new { issuccess = true }, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post | HttpVerbs.Put | HttpVerbs.Delete)]
        public ActionResult FileHandler()
        {
            string flname = string.Empty;
            try
            {
                string directory = HttpContext.Request.Form["hdndirectory"] + "/";
                string serverpath = Server.MapPath("~") + "upload\\";
                string filename = "logo.png";
                var upload = HttpContext.Request.Files;
                bool res = false;
                for (int i = 0; i < upload.Count; i++)
                {

                    System.Web.HttpPostedFileBase file = upload[i];

                    if (!System.IO.Directory.Exists(serverpath + directory))
                    {
                        System.IO.Directory.CreateDirectory(serverpath + directory);
                    }

                    if (!System.IO.File.Exists(serverpath + directory + filename))
                    {
                        file.SaveAs(serverpath + directory + filename);
                    }
                    else
                    {
                        System.IO.File.Delete(serverpath + directory + filename);
                        file.SaveAs(serverpath + directory + filename);
                    }
                    flname = file.FileName.ToString();
                    res = true;
                }

                return Json(new { issuccess = res, msg = res ? "" : "Error on upload logo.", flnm = flname }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region ImportProduct Price
        public bool AddEditProductPricing(ProductPricingModel model, int isRecommended, Site site, bool SaveOnlyPrice)
        {
            if (!SaveOnlyPrice)
            {
                if (string.IsNullOrEmpty(model.product.ReissueType))
                    model.product.ReissueType = "Included";
                if (model.product.RecordStatusID <= 0)
                    model.product.RecordStatusID = (int)RecordStatus.ACTIVE;
            }
            if (model.Month_12 != null)
            {
                model.Month_12.Product = model.product;
                model.Month_12.isRecommended = false;
            }
            if (model.Month_24 != null)
            {
                model.Month_24.Product = model.product;
                model.Month_24.isRecommended = false;
            }
            if (model.Month_36 != null)
            {
                model.Month_36.isRecommended = false;
                model.Month_36.Product = model.product;
            }
          
            if (isRecommended == 12)
                model.Month_12.isRecommended = true;

            if (isRecommended == 24)
                model.Month_24.isRecommended = true;

            if (isRecommended == 36)
                model.Month_36.isRecommended = true;

            ProductPricing pp = null;
            if (model.Month_12 != null && (model.Month_12.SalesPrice > 0 || model.Month_12.ID > 0 || model.product.InternalProductCode.Equals("freessl", StringComparison.OrdinalIgnoreCase)))
            {
                if (model.Month_12.ID > 0 && model.Month_12.SalesPrice > 0)
                {
                    pp = model.Month_12;
                    db.SaveChanges();
                }
                else if (model.Month_12.ID > 0 && model.Month_12.SalesPrice <= 0 && !model.product.InternalProductCode.Equals("freessl", StringComparison.OrdinalIgnoreCase))
                {
                    pp = db.ProductPricings.Where(x => x.ID == model.Month_12.ID).FirstOrDefault();
                    db.ProductPricings.Remove(pp);
                    db.SaveChanges();
                }
                else if (model.Month_12.ID <= 0)
                {
                    db.ProductPricings.Add(model.Month_12);
                    db.SaveChanges();
                }
            }

            if (model.Month_24 != null && (model.Month_24.SalesPrice > 0 || model.Month_24.ID > 0))
            {
                if (model.Month_24.ID > 0 && model.Month_24.SalesPrice > 0)
                {
                    AddUpdatePricing(model.Month_24, pp, 2);
                }
                else if (model.Month_24.ID > 0 && model.Month_24.SalesPrice <= 0)
                {
                     AddUpdatePricing(model.Month_24, pp, 3);
                }
                else
                {
                    AddUpdatePricing(model.Month_24, pp, 1);
                }
            }

            if (model.Month_36 != null && (model.Month_36.SalesPrice > 0 || model.Month_36.ID > 0))
            {
                if (model.Month_36.ID > 0 && model.Month_36.SalesPrice > 0)
                    AddUpdatePricing(model.Month_36, pp, 2);
                else if (model.Month_36.ID > 0 && model.Month_36.SalesPrice <= 0)
                {
                    AddUpdatePricing(model.Month_36, pp, 3);
                }
                else
                    AddUpdatePricing(model.Month_36, pp, 1);
            }

          
            return true;
        }

        private void AddUpdatePricing(ProductPricing model, ProductPricing pp, int action)
        {
            switch (action)
            {
                case 1:
                    db.ProductPricings.Add(model);
                    break;
                case 2:
                    pp = model;
                    break;
                case 3:
                    pp = db.ProductPricings.Where(x => x.ID == model.ID).FirstOrDefault();
                    db.ProductPricings.Remove(pp);
                    break;
            }
            db.SaveChanges();
        }
        public string SetMarginalPrice(ProductPricingModel model, WBSSLStore.Gateways.RestAPIModels.Response.ALLProduct apiProduct, Site site, decimal Margin, int ContractID, bool SaveOnlyPrice)
        {
            try
            {
                if (!SaveOnlyPrice)
                {
                    model.Month_12 = new ProductPricing() { NumberOfMonths = (int)SettingConstants.NumberOfMonths.Month12, RecordStatusID = (int)RecordStatus.ACTIVE, SiteID = site.ID, ContractID = ContractID };
                    model.Month_24 = new ProductPricing() { NumberOfMonths = (int)SettingConstants.NumberOfMonths.Month24, RecordStatusID = (int)RecordStatus.ACTIVE, SiteID = site.ID, ContractID = ContractID };
                    model.Month_36 = new ProductPricing() { NumberOfMonths = (int)SettingConstants.NumberOfMonths.Month36, RecordStatusID = (int)RecordStatus.ACTIVE, SiteID = site.ID, ContractID = ContractID };
                  
                }

                if (apiProduct.ProductCode.Equals("freessl") && (apiProduct.Pricings.Where(pr => pr.NumberOfMonth == 1).FirstOrDefault() != null))
                {
                    var apiPrice = apiProduct.Pricings.Where(pr => pr.NumberOfMonth == 12 || pr.NumberOfMonth == 1).FirstOrDefault();
                    model.Month_12.AdditionalSanPrice = apiPrice.AdditionalSanPrice + ((apiPrice.AdditionalSanPrice * Margin) / 100);
                    model.Month_12.ContractID = ContractID;
                    model.Month_12.RecordStatusID = (int)RecordStatus.ACTIVE;
                    model.Month_12.RetailPrice = apiPrice.SRP;
                    if (apiPrice.Price > 0)
                        model.Month_12.SalesPrice = apiPrice.Price + ((apiPrice.Price * Margin) / 100);
                    else
                        model.Month_12.SalesPrice = 0;
                    model.Month_12.SiteID = site.ID;
                    if (apiPrice.NumberOfMonth == 1)
                        model.Month_12.NumberOfMonths = 1;
                }

                if (apiProduct.Pricings.Where(pr => pr.NumberOfMonth == 12).FirstOrDefault() != null)
                {
                    var apiPrice = apiProduct.Pricings.Where(pr => pr.NumberOfMonth == 12 || pr.NumberOfMonth == 1).FirstOrDefault();
                    model.Month_12.AdditionalSanPrice = apiPrice.AdditionalSanPrice + ((apiPrice.AdditionalSanPrice * Margin) / 100);
                    model.Month_12.ContractID = ContractID;
                    model.Month_12.RecordStatusID = (int)RecordStatus.ACTIVE;
                    model.Month_12.RetailPrice = apiPrice.SRP;
                    if (apiPrice.Price > 0)
                        model.Month_12.SalesPrice = apiPrice.Price + ((apiPrice.Price * Margin) / 100);
                    else
                        model.Month_12.SalesPrice = 0;
                    model.Month_12.SiteID = site.ID;

                }

                if (apiProduct.Pricings.Where(pr => pr.NumberOfMonth == 24).FirstOrDefault() != null)
                {
                    var apiPrice = apiProduct.Pricings.Where(pr => pr.NumberOfMonth == 24).FirstOrDefault();
                    model.Month_24.AdditionalSanPrice = apiPrice.AdditionalSanPrice + ((apiPrice.AdditionalSanPrice * Margin) / 100);
                    model.Month_24.ContractID = ContractID;
                    model.Month_24.RecordStatusID = (int)RecordStatus.ACTIVE;
                    model.Month_24.RetailPrice = apiPrice.SRP;
                    if (apiPrice.Price > 0)
                        model.Month_24.SalesPrice = apiPrice.Price + ((apiPrice.Price * Margin) / 100);
                    else
                        model.Month_24.SalesPrice = 0;
                    model.Month_24.SiteID = site.ID;
                }

                if (model.Month_36 != null && apiProduct.Pricings.Where(pr => pr.NumberOfMonth == 36).FirstOrDefault() != null)
                {
                    var apiPrice = apiProduct.Pricings.Where(pr => pr.NumberOfMonth == 36).FirstOrDefault();
                    model.Month_36.AdditionalSanPrice = apiPrice.AdditionalSanPrice + ((apiPrice.AdditionalSanPrice * Margin) / 100);
                    model.Month_36.ContractID = ContractID;
                    model.Month_36.RecordStatusID = (int)RecordStatus.ACTIVE;
                    model.Month_36.RetailPrice = apiPrice.SRP;
                    if (apiPrice.Price > 0)
                        model.Month_36.SalesPrice = apiPrice.Price + ((apiPrice.Price * Margin) / 100);
                    else
                        model.Month_36.SalesPrice = 0;
                    model.Month_36.SiteID = site.ID;
                }

               
                int isRecommended = 12;
                if (model.Month_36 != null && model.Month_36.SalesPrice > 0)
                    isRecommended = 36;
                else if (model.Month_24 != null && model.Month_24.SalesPrice > 0)
                    isRecommended = 24;

                AddEditProductPricing(model, isRecommended, site, false);
               return model.product.ProductName + ": Successfully imported<br/>";
            }

            catch (Exception ex)

            {

                return model.product.ProductName + ": Error while import<br/>";
            }
        }
        public List<StringBuilder> ImportProducts(List<WBSSLStore.Gateways.RestAPIModels.Response.ALLProduct> ProductList, Site site, decimal Margin)
        {
            ProductPricing resellerProductPricing = null;
            Contract ClientContract = GetClientContract(site.ID);
            StringBuilder SuccessProduct = new StringBuilder();
            StringBuilder UnSuccessProduct = new StringBuilder();

            var productslugs = GeneralHelper.GetProductDetailSlugs();

            Contract resellerContract = db.Contracts.Where(cn => cn.SiteID == site.ID && cn.isAutoCalculation == false && cn.isForReseller == true && cn.ContractLevel == null && cn.RecordStatusID != (int)RecordStatus.DELETED).OrderBy(cn => cn.ID).FirstOrDefault();

            if (resellerContract != null)
                resellerProductPricing = db.ProductPricings.Where(pp => pp.ContractID == resellerContract.ID && pp.SiteID == site.ID).FirstOrDefault();            

            foreach (WBSSLStore.Gateways.RestAPIModels.Response.ALLProduct apiProduct in ProductList)
            {
                try
                {

                    ProductPricingModel model = new ProductPricingModel();
                    var Brands = db.Brands.Where(x => x.isActive).ToList();
                    var sl = productslugs.Where(x => x.ProductCode.Equals(apiProduct.ProductCode, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                   

                    var productavailablity = (from pa in db.ProductAvailablities join p in db.Products on pa.ProductID equals p.ID where pa.SiteID.Equals(site.ID) && p.RecordStatusID == (int)RecordStatus.ACTIVE && p.InternalProductCode.Equals(apiProduct.ProductCode, StringComparison.OrdinalIgnoreCase) select pa).FirstOrDefault();
                                      

                    if (productavailablity == null)
                    {
                        //Create Product Row           
                        Product objProduct = new Product();
                        apiProduct.Brand = apiProduct.Brand.ToLower() == "verisign" ? "symantec" : apiProduct.Brand;
                        objProduct.BrandID = Brands.Where(br => br.BrandName.Equals(apiProduct.Brand, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().ID;
                        objProduct.CanbeReissued = apiProduct.CanbeReissued;
                        objProduct.InternalProductCode = apiProduct.ProductCode;
                        objProduct.isCompetitiveUpgradeAllowed = apiProduct.IsCompetitiveUpgradeSupported;
                        objProduct.isNoOfServerFree = apiProduct.isNoOfServerFree;
                        objProduct.isSANEnabled = apiProduct.IsSanEnable;
                        objProduct.isWildcard = apiProduct.IsWildCardProduct;
                        objProduct.ProductDescription = apiProduct.ProductName;
                        objProduct.ProductName = apiProduct.ProductName;
                        objProduct.ProductTypeID = apiProduct.ProductType;
                        objProduct.RecordStatusID = (int)RecordStatus.ACTIVE;
                        objProduct.RefundDays = apiProduct.RefundDays;
                        objProduct.ReissueDays = apiProduct.ReissueDays;
                        objProduct.ReissueType = "Included";
                        objProduct.SanMax = apiProduct.SanMax;
                        objProduct.SanMin = apiProduct.SanMin;

                        objProduct.DetailPageslug = sl != null ? sl.SlugUrl : string.Empty;

                        ProductAvailablity objAvailablity = new ProductAvailablity();
                        objAvailablity.isActive = true;
                        objAvailablity.Product = objProduct;
                        objAvailablity.SiteID = site.ID;
                        if(objAvailablity.ID.Equals(0) && objProduct.ID.Equals(0))
                        {
                            db.Products.Add(objProduct);
                            db.ProductAvailablities.Add(objAvailablity);
                        }

                        model.productAvailablity = objAvailablity;
                        model.product = objProduct;


                        SuccessProduct.Append(SetMarginalPrice(model, apiProduct, site, Margin, ClientContract.ID, false));

                        //Add reseller pricing for default reseller contract
                        if (resellerContract != null && resellerProductPricing == null)
                            SetMarginalPrice(model, apiProduct, site, Margin, resellerContract.ID, false);
                    }
                    else
                    {
                        productavailablity.Product.DetailPageslug = sl != null ? sl.SlugUrl : string.Empty;

                        //model.product = productavailablity;
                        model.productAvailablity = productavailablity;
                        int Clientcontraid = ClientContract.ID;
                        model.Month_12 = db.ProductPricings.Where(pp => pp.SiteID == site.ID && pp.ContractID == Clientcontraid && pp.ProductID == productavailablity.ID && (pp.NumberOfMonths < 12 ? 12 : pp.NumberOfMonths) == (int)SettingConstants.NumberOfMonths.Month12).FirstOrDefault();
                        model.Month_24 = db.ProductPricings.Where(pp => pp.SiteID == site.ID && pp.ContractID == ClientContract.ID && pp.ProductID == productavailablity.ID && pp.NumberOfMonths == (int)SettingConstants.NumberOfMonths.Month24).FirstOrDefault();
                        model.Month_36 = db.ProductPricings.Where(pp => pp.SiteID == site.ID && pp.ContractID == ClientContract.ID && pp.ProductID == productavailablity.ID && pp.NumberOfMonths == (int)SettingConstants.NumberOfMonths.Month36).FirstOrDefault();

                        SuccessProduct.Append(SetMarginalPrice(model, apiProduct, site, Margin, ClientContract.ID, true));

                        //Add reseller pricing for default reseller contract
                        if (resellerContract != null && resellerProductPricing == null)
                        {
                            model.Month_12 = db.ProductPricings.Where(pp => pp.SiteID == site.ID && pp.ContractID == resellerContract.ID && pp.ProductID == productavailablity.ID && pp.NumberOfMonths == (int)SettingConstants.NumberOfMonths.Month12).FirstOrDefault();
                            model.Month_24 = db.ProductPricings.Where(pp => pp.SiteID == site.ID && pp.ContractID == resellerContract.ID && pp.ProductID == productavailablity.ID && pp.NumberOfMonths == (int)SettingConstants.NumberOfMonths.Month24).FirstOrDefault();
                            model.Month_36 = db.ProductPricings.Where(pp => pp.SiteID == site.ID && pp.ContractID == resellerContract.ID && pp.ProductID == productavailablity.ID && pp.NumberOfMonths == (int)SettingConstants.NumberOfMonths.Month36).FirstOrDefault();
                            SetMarginalPrice(model, apiProduct, site, Margin, resellerContract.ID, true);
                        }
                    }
                }

                catch (Exception ex)

                {
                }
            }

            return null;
        }
        #endregion
    }

}