using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WBSSLStore.Domain;
using WBSSLStore.Data;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Service;
using System.IO;
using System.Web.Security;
using WBSSLStore.Web.Helpers.Authentication;
using System.Drawing.Drawing2D;
using WBSSLStore.Web.Helpers.Caching;
using WBSSLStore.Service.ViewModels;
using System.Threading;
using System.Text;
using WBSSLStore.Web.Helpers;
using WBSSLStore.Gateways.RestAPIModels.Response;


namespace WBSSLStore.Web.Areas.Admin.Controllers
{
    [ValidateInput(false)]
    [CustomAuthorizeAttribute]
    [HandleError]
    public class SiteSettingsController : WBController<SiteSettings, IRepository<SiteSettings>, IRepository<SiteSMTP>>
    {

        //
        // GET: /Admin/Default1/
        public User CurrentUser
        {
            get
            {
                if (User.Identity.IsAuthenticated)
                {
                    SSLStoreUser loginuser = (SSLStoreUser)Membership.GetUser();
                    if (loginuser != null && loginuser.Details != null)
                        return loginuser.Details;
                }
                else if (User.Identity.IsAuthenticated && !string.IsNullOrEmpty(User.Identity.Name))
                {
                    SSLStoreUser loginuser = (SSLStoreUser)Membership.GetUser(User.Identity.Name);
                    if (loginuser != null && loginuser.Details != null)
                        return loginuser.Details;
                }
                return null;
            }
        }

        public ViewResult Index()
        {
            var settings = _repository.Find(ss => ss.SiteID == Site.ID);
            ViewBag.SiteID = Site.ID;
            ViewBag.CommonName = Site.Alias;
            ViewBag.Country = WBSSLStore.Web.Helpers.Caching.SiteCacher.GetCountryCachedList();
            ViewBag.CompanyName = CurrentUser.CompanyName;

            ViewBag.ErrMsg = "";
            if (Request.QueryString["msg"] == "1")
            {

                ViewBag.Message = "<div class='normsg'>" + @WBSSLStore.Resources.GeneralMessage.Message.GeneralSettings_SuccessMsg + "</div>";
            }
            if (Request.QueryString["msg"] == "2")
            {
                ViewBag.ErrMsg = @WBSSLStore.Resources.GeneralMessage.Message.SmtpandPayment_Configure_Msg;
            }
            return View(settings.ToList());
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(FormCollection Method)
        {

            var objsettings = _repository.Find(ss => ss.SiteID == Site.ID).ToList<SiteSettings>();
            if (ModelState.IsValid)
            {
                SaveSettings(objsettings, Method);

                _unitOfWork.Commit();
                WBSSLStore.Web.Helpers.Caching.SiteCacher.ClearCache(Site.ID);

                return RedirectToAction("Index", new { @msg = (string.IsNullOrEmpty(ViewBag.ErrMsg) ? 1 : 2) });
            }

            ViewBag.SiteID = Site.ID;
            ViewBag.CommonName = Site.Alias;
            ViewBag.Country = WBSSLStore.Web.Helpers.Caching.SiteCacher.GetCountryCachedList();
            ViewBag.CompanyName = CurrentUser.CompanyName;
            //return RedirectToAction("Index");
            return View(objsettings.ToList());
        }

        public ActionResult RefreshBrand()
        {
            string allowbrands = string.Empty;

            AllowedBrandResponse objResponse = new AllowedBrandResponse();

            GetAllPricingResponse prdResponse = new GetAllPricingResponse();
            prdResponse = WBSSLStore.Gateways.RestAPIModels.Services.OrderService.GetAllProductPricing(Site.APIPartnerCode, Site.APIAuthToken);
            foreach (ALLProduct prd in prdResponse.Product.ToList())
            {
                if (prd != null)
                {
                    var _productservice = DependencyResolver.Current.GetService<IProductService>();
                    Product prod = new Product();
                    prod = _productservice.GetByProductCode(prd.ProductCode);
                    if (prod != null)
                    {

                        if (!allowbrands.Contains(Convert.ToString(prod.BrandID)))
                            allowbrands += "," + Convert.ToString(prod.BrandID);


                    }

                }


            }
            objResponse.AllowedBrand = allowbrands.Trim(',');
            objResponse.ErrorCode = prdResponse.ErrorCode;
            objResponse.ErrorMessage = prdResponse.ErrorMessage;

            if (objResponse != null && string.IsNullOrEmpty(objResponse.ErrorMessage))
            {
                SiteSettings objBrandSettings = _repository.Find(ss => ss.Key.Equals(SettingConstants.CURRENT_ALLOWEDBRAND_KEY, StringComparison.OrdinalIgnoreCase) && ss.SiteID == Site.ID).FirstOrDefault();
                if (objBrandSettings != null)
                    objBrandSettings.Site = null;
                SaveSettingsRow(objBrandSettings, SettingConstants.CURRENT_ALLOWEDBRAND_KEY, "0,99," + objResponse.AllowedBrand);
                _unitOfWork.Commit();
                WBSSLStore.Web.Helpers.Caching.SiteCacher.ClearCache(Site.ID);
                return Json(objResponse.AllowedBrand);
            }
            else
                return Json("Error:" + objResponse.ErrorMessage.Replace("'", string.Empty));
        }


        [HttpPost]
        public ActionResult SaveLogo()
        {
            FileUploadResponse objResp = null;
            try
            {
                if (Request.Files["fupBanner"] != null && Request.Files["fupBanner"].ContentLength > 0)
                    return SaveBanner();
                else
                {
                    //string strFilePath = Request.PhysicalApplicationPath + "\\Upload\\" + Site.ID + "\\Temp";
                    string strFilePath = Request.PhysicalApplicationPath + "\\Upload\\sitelogo\\Temp";
                    //string FileName = "logo" + Site.ID + System.IO.Path.GetExtension(Request.Files["fupLogo"].FileName);
                    string FileName = "logo" + System.IO.Path.GetExtension(Request.Files["fupLogo"].FileName);
                    if (Directory.Exists(strFilePath))
                    {
                        Directory.Delete(strFilePath, true);

                    }
                    Directory.CreateDirectory(strFilePath);

                    if (System.IO.File.Exists(strFilePath + "\\" + FileName))
                        System.IO.File.Delete(strFilePath + "\\" + FileName);
                    Request.Files["fupLogo"].SaveAs(strFilePath + "\\" + FileName);
                    int Width = 0, Height = 0;
                    GetImageWidthAndHeight(strFilePath + "\\" + FileName, ref Width, ref Height);
                    bool NeedToCrop = false;
                    if (Width > Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["LogoWidth"]) || Height > Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["LogoHeight"]))
                    {
                        NeedToCrop = true;
                    }
                    else
                    {
                        try
                        {
                            if (System.IO.File.Exists(Request.PhysicalApplicationPath + "\\Upload\\sitelogo\\" + FileName))
                                System.IO.File.Delete(Request.PhysicalApplicationPath + "\\Upload\\sitelogo\\" + FileName);
                            System.IO.File.Move(strFilePath + "\\" + FileName, Request.PhysicalApplicationPath + "\\Upload\\sitelogo\\" + FileName);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogException(ex);
                        }


                        var _Settings = DependencyResolver.Current.GetService<IRepository<SiteSettings>>();
                        var _item = _Settings.Find(ss => ss.SiteID == Site.ID && ss.Key.ToLower().Equals(SettingConstants.CURRENT_SITELOGO_KEY)).FirstOrDefault();
                        if (_item == null)
                            _item = new SiteSettings();


                        _item.Key = SettingConstants.CURRENT_SITELOGO_KEY;
                        _item.Value = FileName;
                        _item.SiteID = Site.ID;
                        if (_item.ID > 0)
                            _Settings.Update(_item);
                        else
                            _Settings.Add(_item);
                        _unitOfWork.Commit();
                        SiteCacher.ClearCache(Site.ID);
                    }

                    objResp = new FileUploadResponse();
                    objResp.FilePath = "/upload/sitelogo/" + FileName;
                    objResp.NeedToCrop = NeedToCrop;
                    objResp.PhysicalPath = strFilePath + "\\" + FileName;
                    objResp.Type = "logo";
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            return Json(objResp);
        }

        

        private void GetImageWidthAndHeight(string strPath, ref int Width, ref int Height)
        {
            FileStream fs = new FileStream(strPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            System.Drawing.Image image = System.Drawing.Image.FromStream(fs);

            try
            {

                Width = image.Width;
                Height = image.Height;
                image.Dispose();
                fs.Dispose();
                fs = null;

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }

        }

        [HttpPost]
        public ActionResult SaveBanner()
        {
            string strFilePath = Request.PhysicalApplicationPath + "\\Upload\\" + Site.ID + "\\Temp";
            string FileName = "banner-" + Site.ID + System.IO.Path.GetExtension(Request.Files["fupBanner"].FileName);
            if (Directory.Exists(strFilePath))
            {
                Directory.Delete(strFilePath, true);

            }
            Directory.CreateDirectory(strFilePath);
            if (System.IO.File.Exists(strFilePath + "\\" + FileName))
                System.IO.File.Delete(strFilePath + "\\" + FileName);
            Request.Files["fupBanner"].SaveAs(strFilePath + "\\" + FileName);
            int Width = 0, Height = 0;
            GetImageWidthAndHeight(strFilePath + "\\" + FileName, ref Width, ref Height);
            bool NeedToCrop = false;
            if (Width > Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["BannerWidth"]) || Height > Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["BannerHeight"]))
            {
                NeedToCrop = true;
            }
            else
            {
                if (System.IO.File.Exists(Request.PhysicalApplicationPath + "\\Upload\\" + Site.ID + "\\" + FileName))
                    System.IO.File.Delete(Request.PhysicalApplicationPath + "\\Upload\\" + Site.ID + "\\" + FileName);
                System.IO.File.Move(strFilePath + "\\" + FileName, Request.PhysicalApplicationPath + "\\Upload\\" + Site.ID + "\\" + FileName);
                var _Settings = DependencyResolver.Current.GetService<IRepository<SiteSettings>>();
                var _item = _Settings.Find(ss => ss.SiteID == Site.ID && ss.Key.ToLower().Equals(SettingConstants.CURRENT_BANNERFILE_KEY)).FirstOrDefault();
                if (_item != null)
                    _item.Site = null;
                else
                    _item = new SiteSettings();
                _item.Key = SettingConstants.CURRENT_BANNERFILE_KEY;
                _item.Value = FileName;
                _item.SiteID = Site.ID;
                if (_item.ID > 0)
                    _Settings.Update(_item);
                else
                    _Settings.Add(_item);

                var _item2 = _Settings.Find(ss => ss.SiteID == Site.ID && ss.Key.ToLower().Equals(SettingConstants.CURRENT_NEEDBANNER_KEY)).FirstOrDefault();
                if (_item2 != null)
                    _item2.Site = null;
                else
                    _item2 = new SiteSettings();
                _item2.Key = SettingConstants.CURRENT_NEEDBANNER_KEY;
                _item2.Value = "True";
                _item2.SiteID = Site.ID;
                if (_item2.ID > 0)
                    _Settings.Update(_item2);
                else
                    _Settings.Add(_item2);

                _unitOfWork.Commit();
                SiteCacher.ClearCache(Site.ID);
            }
            FileUploadResponse objResp = new FileUploadResponse();
            objResp.FilePath = "/upload/" + Site.ID + "/" + FileName;
            objResp.NeedToCrop = NeedToCrop;
            objResp.PhysicalPath = strFilePath + "\\" + FileName;
            objResp.Type = "banner";
            return Json(objResp);
        }





        public ViewResult SMTPSettings()
        {

            SiteSMTP _SMTP = _service.Find(sm => sm.SiteID == Site.ID).EagerLoad(sm => sm.AuditDetails).FirstOrDefault();
            if (_SMTP == null)
            {

                _SMTP = new SiteSMTP() { SiteID = Site.ID };
            }

            return View(_SMTP);
        }
        [HttpPost]
        public ActionResult SMTPSettings(SiteSMTP model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.ID > 0)
                    {

                        model.AuditDetails.DateModified = DateTimeWithZone.Now;
                        model.AuditDetails.ByUserID = CurrentUser.ID;
                        model.AuditDetails.HttpHeaderDump = Request.Headers.ToString();
                        model.AuditDetails.IP = Request.UserHostAddress;


                        _service.Update(model);
                    }
                    else
                    {
                        model.AuditDetails = new Audit() { ByUserID = CurrentUser.ID, DateCreated = DateTimeWithZone.Now, DateModified = DateTimeWithZone.Now, HttpHeaderDump = Request.Headers.ToString(), IP = Request.UserHostAddress, ID = 0 };
                        _service.Add(model);
                    }
                    _unitOfWork.Commit();
                    WBSSLStore.Web.Helpers.Caching.SiteCacher.ClearCache(Site.ID);
                    ViewBag.Message = "<div class='normsg'>" + @WBSSLStore.Resources.GeneralMessage.Message.GeneralSettings_SuccessMsg + "</div>";
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                ViewBag.Message = "<div class='errormsg'>" + @WBSSLStore.Resources.GeneralMessage.Message.ErrorMsg + "</div>";
            }
            return View(model);
        }

        public ActionResult SendEmail()
        {
            SiteSMTP smtpSettings = _service.Find(ss => ss.SiteID == Site.ID).FirstOrDefault();
            string returnMSG = string.Empty;
            using (System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient())
            {
                smtpClient.EnableSsl = smtpSettings.UseSSL;
                smtpClient.UseDefaultCredentials = (!string.IsNullOrEmpty(smtpSettings.SMTPUser) && !string.IsNullOrEmpty(smtpSettings.SMTPPassword));
                if (smtpClient.UseDefaultCredentials && !String.IsNullOrWhiteSpace(smtpSettings.SMTPUser))
                {
                    smtpClient.Credentials = new System.Net.NetworkCredential(smtpSettings.SMTPUser, smtpSettings.SMTPPassword);
                }
                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage(CurrentUser.Email, CurrentUser.Email);

                string strAlias = (!string.IsNullOrEmpty(Site.Alias) ? Site.Alias : Site.CName);
                message.Subject = "Welcome to " + strAlias;

                StringBuilder sbHTML = new StringBuilder(WBHelper.GetTemplateText(Server.MapPath("/content/Templates/SMTPCheckMail.htm")));
                sbHTML.Replace("[SITEALIAS]", strAlias);
                sbHTML.Replace("[EMAIL]", CurrentUser.Email);
                message.Body = sbHTML.ToString();

                if (smtpSettings.SMTPHost != null)
                    smtpClient.Host = smtpSettings.SMTPHost;

                smtpClient.Port = smtpSettings.SMTPPort;

                smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;


                message.IsBodyHtml = true;

                try
                {
                    smtpClient.Send(message);
                    returnMSG = "Email Send Successfully.";
                }
                catch (Exception e)
                {
                    returnMSG = e.Message;
                }
            }
            return Json(returnMSG);
        }

        private bool SaveSettings(List<SiteSettings> objsettings, FormCollection collection)
        {

            try
            {
                if (objsettings == null)
                    objsettings = new List<SiteSettings>();
                SiteSettings item = null;
                //Save CName
                string strAlias = collection["txtCommonName"];
                if (!string.IsNullOrEmpty(strAlias))
                {
                    strAlias = strAlias.ToLower().StartsWith("www.") ? strAlias.ToLower().Replace("www.", string.Empty) : strAlias.ToLower();
                    var _Site = DependencyResolver.Current.GetService<IRepository<Site>>();
                    var objSite = _Site.FindByID(Site.ID);
                    //if (string.IsNullOrEmpty(objSite.Alias))
                    //{
                    string DirPath = Server.MapPath("/Upload/") + strAlias.Replace(".", string.Empty) + "\\upload-files";
                    if (!Directory.Exists(DirPath))
                        Directory.CreateDirectory(DirPath);
                    //}

                    objSite.Alias = strAlias;
                    _Site.Update(objSite);
                }


                //Save Under maintenance
                item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITEUNDERMAINTANACE).FirstOrDefault();
                if (!Convert.ToBoolean(collection["chkUnderMaintanance"]))
                {
                    if (@WBSSLStore.Web.Util.WBSiteSettings.IsSMTPConfigure && @WBSSLStore.Web.Util.WBSiteSettings.IsPaymentSettingConfigure)
                        SaveSettingsRow(item, SettingConstants.CURRENT_SITEUNDERMAINTANACE, false.ToString());
                    else
                        ViewBag.ErrMsg = @WBSSLStore.Resources.GeneralMessage.Message.SmtpandPayment_Configure_Msg;
                }
                else
                    SaveSettingsRow(item, SettingConstants.CURRENT_SITEUNDERMAINTANACE, true.ToString());

                //Save Live Chat Script            
                item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITELIVECHATE_KEY).FirstOrDefault();
                SaveSettingsRow(item, SettingConstants.CURRENT_SITELIVECHATE_KEY, Convert.ToString(collection["txtLiveChate"]));

                //Save Face book             
                item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITEFACEBOOK_KEY).FirstOrDefault();
                SaveSettingsRow(item, SettingConstants.CURRENT_SITEFACEBOOK_KEY, Convert.ToString(collection["txtFacebook"]));

                //Save Payment Note
                item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITE_PAYMENTNOTE).FirstOrDefault();
                SaveSettingsRow(item, SettingConstants.CURRENT_SITE_PAYMENTNOTE, Convert.ToString(collection["txtpaymentnote"]));

                //Save Twitter            
                item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITETWITTER_KEY).FirstOrDefault();
                SaveSettingsRow(item, SettingConstants.CURRENT_SITETWITTER_KEY, Convert.ToString(collection["txtTwitter"]));

                //Save Googleplus
                item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITEGOOGLEPLUS_KEY).FirstOrDefault();
                SaveSettingsRow(item, SettingConstants.CURRENT_SITEGOOGLEPLUS_KEY, Convert.ToString(collection["txtGoogleplus"]));

                //Save Contact US Subject feild
                item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITE_SUBJECTFEIELD).FirstOrDefault();
                SaveSettingsRow(item, SettingConstants.CURRENT_SITE_SUBJECTFEIELD, Convert.ToString(collection["txtSubjectfield"]));

                //Save Contact US Email feild
                item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITE_TOEMAIL).FirstOrDefault();
                SaveSettingsRow(item, SettingConstants.CURRENT_SITE_TOEMAIL, Convert.ToString(collection["txtToEmail"]));

                //Save Thankyou page URL
                item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITE_THANKYOUPAGE).FirstOrDefault();
                SaveSettingsRow(item, SettingConstants.CURRENT_SITE_THANKYOUPAGE, Convert.ToString(collection["txtThankyou"]));

                //Save Site Signature (Address of site)
                item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITE_SIGNATURE).FirstOrDefault();
                SaveSettingsRow(item, SettingConstants.CURRENT_SITE_SIGNATURE, Convert.ToString(collection["txtInvoiceAddress"]));

                //Save Phone            
                item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITEPNONE_KEY).FirstOrDefault();
                SaveSettingsRow(item, SettingConstants.CURRENT_SITEPNONE_KEY, Convert.ToString(collection["txtPhone"]));

                //Need Banner
                item = objsettings.Where(ss => ss.Key.ToLower() == WBSSLStore.Domain.SettingConstants.CURRENT_NEEDBANNER_KEY).FirstOrDefault();
                SaveSettingsRow(item, WBSSLStore.Domain.SettingConstants.CURRENT_NEEDBANNER_KEY, Convert.ToBoolean(Request.Form["rbtNeedBanner"]).ToString());

                //Admin Email
                if (!string.IsNullOrEmpty(Request.Form["txtAdminEmail"]))
                {
                    item = objsettings.Where(ss => ss.Key.ToLower() == WBSSLStore.Domain.SettingConstants.CURRENT_SITEADMIN_EMAIL_KEY).FirstOrDefault();
                    SaveSettingsRow(item, WBSSLStore.Domain.SettingConstants.CURRENT_SITEADMIN_EMAIL_KEY, Convert.ToString(Request.Form["txtAdminEmail"]));
                }
                //Support Email
                if (!string.IsNullOrEmpty(Request.Form["txtSupportEmail"]))
                {
                    item = objsettings.Where(ss => ss.Key.ToLower() == WBSSLStore.Domain.SettingConstants.CURRENT_SITESUPPORT_EMAI_KEY).FirstOrDefault();
                    SaveSettingsRow(item, WBSSLStore.Domain.SettingConstants.CURRENT_SITESUPPORT_EMAI_KEY, Convert.ToString(Request.Form["txtSupportEmail"]));
                }
                //Billing Email
                if (!string.IsNullOrEmpty(Request.Form["txtBillingEmail"]))
                {
                    item = objsettings.Where(ss => ss.Key.ToLower() == WBSSLStore.Domain.SettingConstants.CURRENT_SITEBILLING_EMAIL_KEY).FirstOrDefault();
                    SaveSettingsRow(item, WBSSLStore.Domain.SettingConstants.CURRENT_SITEBILLING_EMAIL_KEY, Convert.ToString(Request.Form["txtBillingEmail"]));
                }
                //Billing Email
                if (!string.IsNullOrEmpty(Request.Form["txtAllowedBrand"]))
                {
                    item = objsettings.Where(ss => ss.Key.ToLower() == WBSSLStore.Domain.SettingConstants.CURRENT_ALLOWEDBRAND_KEY).FirstOrDefault();
                    SaveSettingsRow(item, WBSSLStore.Domain.SettingConstants.CURRENT_ALLOWEDBRAND_KEY, Convert.ToString(Request.Form["txtAllowedBrand"]));
                }
                //Blog Url
                if (!string.IsNullOrEmpty(Request.Form["txtBlogUrl"]))
                {
                    item = objsettings.Where(ss => ss.Key.ToLower() == WBSSLStore.Domain.SettingConstants.CURRENT_SITEBLOG_URL).FirstOrDefault();
                    SaveSettingsRow(item, WBSSLStore.Domain.SettingConstants.CURRENT_SITEBLOG_URL, Convert.ToString(Request.Form["txtBlogUrl"]));
                }

                //Save Language & Time Zone
                if (!string.IsNullOrEmpty(Request.Form["rbtLanguage"]))
                {
                    item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITELANGUAGE_KEY).FirstOrDefault();
                    SaveSettingsRow(item, SettingConstants.CURRENT_SITELANGUAGE_KEY, Convert.ToString(Request.Form["rbtLanguage"]));
                }
                if (!string.IsNullOrEmpty(Request.Form["drpTimezone"]))
                {
                    item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITETIMEZONE_KEY).FirstOrDefault();
                    SaveSettingsRow(item, SettingConstants.CURRENT_SITETIMEZONE_KEY, Convert.ToString(Request.Form["drpTimezone"]));
                }
                if (!string.IsNullOrEmpty(Request.Form["ddCurrancy"]))
                {
                    item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITECURRANCY_KEY).FirstOrDefault();
                    SaveSettingsRow(item, SettingConstants.CURRENT_SITECURRANCY_KEY, Convert.ToString(Request.Form["ddCurrancy"]));
                }

                //Save Currency Culture Key
                if (!string.IsNullOrEmpty(Request.Form["ddCurCulture"]))
                {
                    item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITECULTURE_KEY).FirstOrDefault();
                    SaveSettingsRow(item, SettingConstants.CURRENT_SITECULTURE_KEY, Convert.ToString(Request.Form["ddCurCulture"]));
                }

                //Save Invoice Number Prefix
                item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITEINVOICEPREFIX_KEY).FirstOrDefault();
                if (!string.IsNullOrEmpty(Request.Form["txtInvoicePrefix"]))
                    SaveSettingsRow(item, SettingConstants.CURRENT_SITEINVOICEPREFIX_KEY, Convert.ToString(Request.Form["txtInvoicePrefix"]));
                else
                    DeleteSettingsRow(item);

                //Save Need to Approve Reseller
                item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITEAPPROVERESELLER_KEY).FirstOrDefault();
                SaveSettingsRow(item, SettingConstants.CURRENT_SITEAPPROVERESELLER_KEY, Convert.ToBoolean(Request.Form["chkNeedToApproveReseller"]).ToString());

                //Save Page Size
                if (!string.IsNullOrEmpty(Request.Form["txtPageSize"]))
                {
                    item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITEPAGESIZE_KEY).FirstOrDefault();
                    SaveSettingsRow(item, SettingConstants.CURRENT_SITEPAGESIZE_KEY, Convert.ToString(Request.Form["txtPageSize"]));
                }

                //Save Punch Line                        
                if (collection["txtPunchLine"] != null)
                {
                    item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_PUNCHLINE_KEY).FirstOrDefault();
                    SaveSettingsRow(item, SettingConstants.CURRENT_PUNCHLINE_KEY, Convert.ToString(Request.Form["txtPunchLine"] == string.Empty ? "NA" : Request.Form["txtPunchLine"]));
                }

                //Save Use SSL
                if (Request.Form["chkUseSSL"] != null)
                {
                    item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_USESSL_KEY).FirstOrDefault();
                    SaveSettingsRow(item, SettingConstants.CURRENT_USESSL_KEY, Convert.ToBoolean(Request.Form["chkUseSSL"]).ToString());
                }

                //Save Refund30Days Logo SSL
                item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_REFUND30DAYS_KEY).FirstOrDefault();
                SaveSettingsRow(item, SettingConstants.CURRENT_REFUND30DAYS_KEY, Convert.ToBoolean(Request.Form["chkRefund30Days"]).ToString());

                //Save VAT Applicable
                item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_VAT_APPLICABLE_KEY).FirstOrDefault();
                SaveSettingsRow(item, SettingConstants.CURRENT_VAT_APPLICABLE_KEY, Convert.ToBoolean(Request.Form["chkVAT"]).ToString());

                //Save CartButton
                if (Request.Files.Count > 0 && Request.Files["fupCartButton"].ContentLength > 0)
                {
                    string strFilePath = Request.PhysicalApplicationPath + "\\Upload\\" + Site.ID;
                    string FileName = "cart-" + Site.ID.ToString() + System.IO.Path.GetExtension(Request.Files["fupCartButton"].FileName);
                    SaveFile(strFilePath, FileName, "fupCartButton");
                    item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITEMYCART_KEY).FirstOrDefault();
                    SaveSettingsRow(item, SettingConstants.CURRENT_SITEMYCART_KEY, FileName);
                }

                if (Convert.ToBoolean(Request.Form["chkVAT"]))
                {
                    //Vat Country
                    item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_VAT_COUNTRY_KEY).FirstOrDefault();
                    SaveSettingsRow(item, SettingConstants.CURRENT_VAT_COUNTRY_KEY, Convert.ToString(Request.Form["drpVATCountry"]).ToString());

                    //Vat Country
                    item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_VAT_NUMBER_KEY).FirstOrDefault();
                    SaveSettingsRow(item, SettingConstants.CURRENT_VAT_NUMBER_KEY, Convert.ToString(Request.Form["txtVATNumber"]).ToString());
                }
                else
                {
                    item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_VAT_COUNTRY_KEY).FirstOrDefault();
                    if (item != null)
                        _repository.Delete(item);
                    item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_VAT_NUMBER_KEY).FirstOrDefault();
                    if (item != null)
                        _repository.Delete(item);
                }
                if (Convert.ToBoolean(Request.Form["chkWWW"]))
                {
                    //USe with WWW
                    item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_IS_SITE_RUN_WITH_WWW).FirstOrDefault();
                    SaveSettingsRow(item, SettingConstants.CURRENT_IS_SITE_RUN_WITH_WWW, "true");


                }
                else
                {
                    item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_IS_SITE_RUN_WITH_WWW).FirstOrDefault();
                    SaveSettingsRow(item, SettingConstants.CURRENT_IS_SITE_RUN_WITH_WWW, "false");

                }

                if (Convert.ToBoolean(Request.Form["chkTestimonials"]))
                {
                    // for testimonials
                    item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_TESTIMONIAL_APPLICABLE_KEY).FirstOrDefault();
                    SaveSettingsRow(item, SettingConstants.CURRENT_TESTIMONIAL_APPLICABLE_KEY, "true");


                }
                else
                {
                    item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_TESTIMONIAL_APPLICABLE_KEY).FirstOrDefault();
                    SaveSettingsRow(item, SettingConstants.CURRENT_TESTIMONIAL_APPLICABLE_KEY, "false");

                }

                //set from email of reseller's register email for reseller's customer's renwal notification email 
                item = objsettings.Where(ss => ss.Key.ToLower() == SettingConstants.CUSTOM_EMAILID_RESELLER_CUSTOMERS_RENEWALS_NOTIFICATION).FirstOrDefault();
                SaveSettingsRow(item, SettingConstants.CUSTOM_EMAILID_RESELLER_CUSTOMERS_RENEWALS_NOTIFICATION, (Convert.ToBoolean(Request.Form["chkCustomEmailResellersCustomersNotification"])) ? "true" : "false");


                // set setting for banner show only home page not detail page
                item = objsettings.Where(ss => ss.Key.ToLower().Equals(SettingConstants.CURRENT_SITE_SHOWBANNER_HOME_PAGE_ONLY)).FirstOrDefault();
                SaveSettingsRow(item, SettingConstants.CURRENT_SITE_SHOWBANNER_HOME_PAGE_ONLY, (Convert.ToBoolean(Request.Form["chkShowbannerOnlyHomepage"])) ? "true" : "false");

                return true;
            }
            catch
            {
                return false;
            }

        }
        private void SaveSettingsRow(SiteSettings item, string Key, string Value)
        {
            if (item == null)
            {
                item = new SiteSettings();
                item.Key = Key;
                item.SiteID = Site.ID;
                item.Value = Value;
                _repository.Add(item);
            }
            else
            {
                item.Value = Value;
                _repository.Update(item);
            }
        }

        private void DeleteSettingsRow(SiteSettings item)
        {
            if (item != null)
                _repository.Delete(item);
        }
        private void SaveFile(string strFilePath, string FileName, string RequestParam)
        {
            if (!Directory.Exists(strFilePath))
            {
                Directory.CreateDirectory(strFilePath);
            }
            if (System.IO.File.Exists(strFilePath + "\\" + FileName))
                System.IO.File.Delete(strFilePath + "\\" + FileName);
            Request.Files[RequestParam].SaveAs(strFilePath + "\\" + FileName);
        }
        public ViewResult CropImage(string imgType)
        {
            ViewBag.ImageType = Convert.ToString(Request.QueryString["imgType"]);
            string NameStartWith = Convert.ToString(ViewBag.ImageType) == "logo" ? "logo-" : "banner-";
            string strFilePath = Request.PhysicalApplicationPath + "\\Upload\\" + Site.ID + "\\Temp";
            if (Directory.Exists(strFilePath))
            {
                List<string> Files = Directory.GetFiles(strFilePath).ToList();
                if (Files != null && Files.Count() > 0 && Files.Where(f => f.Contains(NameStartWith)).Count() > 0)
                {
                    string strFileName = System.IO.Path.GetFileName(Files.Where(f => f.Contains(NameStartWith)).FirstOrDefault());
                    ViewBag.ImageUrl = "/upload/" + Site.ID + "/temp/" + strFileName;
                }
                else
                    ViewBag.ImageUrl = string.Empty;
            }
            return View();
        }
        [HttpPost]
        public ActionResult CropImage(FormCollection collection)
        {
            int w = Convert.ToInt32(collection["w"]);
            //int w = 230;
            int h = Convert.ToInt32(collection["h"]);
            //int h = 60;
            int x = Convert.ToInt32(collection["x"]);

            int y = Convert.ToInt32(collection["y"]);
            string NameStartWith = Convert.ToString(collection["ImageType"]) == "logo" ? "logo-" : "banner-";
            string strFilePath = Request.PhysicalApplicationPath + "\\Upload\\" + Site.ID + "\\Temp";
            string ImageName = Path.GetFileName(Directory.GetFiles(strFilePath).Where(f => f.Contains(NameStartWith)).FirstOrDefault());

            byte[] CropImage = Crop(strFilePath + "\\" + ImageName, w, h, x, y);

            using (MemoryStream ms = new MemoryStream(CropImage, 0, CropImage.Length))
            {

                ms.Write(CropImage, 0, CropImage.Length);

                using (System.Drawing.Image CroppedImage = System.Drawing.Image.FromStream(ms, true))
                {
                    string SaveTo = Request.PhysicalApplicationPath + "\\Upload\\" + Site.ID + "\\" + ImageName;
                    CroppedImage.Save(SaveTo, CroppedImage.RawFormat);
                }

            }
            SiteSettings item = null;
            if (collection["ImageType"].ToLower().Equals("logo"))
                item = Site.Settings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_SITELOGO_KEY).FirstOrDefault();
            else
                item = Site.Settings.Where(ss => ss.Key.ToLower() == SettingConstants.CURRENT_BANNERFILE_KEY).FirstOrDefault();
            if (item != null)
                item.Site = null;
            SaveSettingsRow(item, collection["ImageType"].ToLower().Equals("logo") ? SettingConstants.CURRENT_SITELOGO_KEY : SettingConstants.CURRENT_BANNERFILE_KEY, ImageName);
            _unitOfWork.Commit();
            WBSSLStore.Web.Helpers.Caching.SiteCacher.ClearCache(Site.ID);
            return RedirectToAction("index");
        }

        static byte[] Crop(string Img, int Width, int Height, int X, int Y)
        {

            try
            {

                using (System.Drawing.Image OriginalImage = System.Drawing.Image.FromFile(Img))
                {

                    using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(Width, Height))
                    {

                        bmp.SetResolution(OriginalImage.HorizontalResolution, OriginalImage.VerticalResolution);

                        using (System.Drawing.Graphics Graphic = System.Drawing.Graphics.FromImage(bmp))
                        {

                            Graphic.SmoothingMode = SmoothingMode.AntiAlias;

                            Graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

                            Graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;

                            Graphic.DrawImage(OriginalImage, new System.Drawing.Rectangle(0, 0, Width, Height), X, Y, Width, Height, System.Drawing.GraphicsUnit.Pixel);

                            MemoryStream ms = new MemoryStream();

                            bmp.Save(ms, OriginalImage.RawFormat);

                            return ms.GetBuffer();

                        }

                    }

                }

            }

            catch (Exception Ex)
            {

                throw (Ex);

            }

        }

        public ActionResult robots()
        {
            var settings = _repository.Find(ss => ss.SiteID == Site.ID && ss.Key == SettingConstants.CURRENT_ROBOTS_FILE_VALUE).FirstOrDefault();


            if (settings == null)
            {
                settings = new SiteSettings();
                settings.Key = SettingConstants.CURRENT_ROBOTS_FILE_VALUE;
                settings.SiteID = Site.ID;

            }


            return View(settings);
        }
        [HttpPost]
        public ActionResult robots(SiteSettings Model)
        {
            try
            {
                if (Model.ID.Equals(0))
                    _repository.Add(Model);
                else
                {
                    if (Model.Value == null)
                        Model.Value = string.Empty;
                    _repository.Update(Model);
                }
                _unitOfWork.Commit();
                WBSSLStore.Web.Helpers.Caching.SiteCacher.ClearCache(Site.ID);
                ViewBag.Message = "<div class='normsg'>" + @WBSSLStore.Resources.GeneralMessage.Message.GeneralSettings_SuccessMsg + "</div>";
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                ViewBag.Message = "<div class='errormsg'>" + @WBSSLStore.Resources.GeneralMessage.Message.ErrorMsg + "</div>";
            }
            return View(Model);
        }

        public ActionResult googleanalytics()
        {
            var settings1 = _repository.Find(ss1 => ss1.SiteID == Site.ID && ss1.Key == SettingConstants.GOOGLE_ANALYTIC_VALUE).FirstOrDefault();
            if (settings1 == null)
            {
                settings1 = new SiteSettings();
                settings1.Key = SettingConstants.GOOGLE_ANALYTIC_VALUE;
                settings1.SiteID = Site.ID;
            }
            if (Request.QueryString[SettingConstants.QS_MESSAGE] != null)
            {
                if (Convert.ToInt16(Request.QueryString[SettingConstants.QS_MESSAGE]) == 1)
                    ViewBag.Message = "<div class='normsg'>" + @WBSSLStore.Resources.GeneralMessage.Message.GeneralSettings_SuccessMsg + "</div>";
                else if (Convert.ToInt16(Request.QueryString[SettingConstants.QS_MESSAGE]) == 2)
                    ViewBag.Message = "<div class='errormsg'>" + @WBSSLStore.Resources.GeneralMessage.Message.ErrorMsg + "</div>";
            }
            return View(settings1);
        }
        [HttpPost]
        public ActionResult googleanalytics(SiteSettings model)
        {
            SiteSettings _newModel = null;
            try
            {
                if (string.IsNullOrEmpty(model.Value))
                {
                    _newModel = _repository.Find(ss => ss.ID == model.ID && ss.SiteID == Site.ID).FirstOrDefault();
                    if (_newModel != null)
                        _repository.Delete(_newModel);
                }
                else
                {
                    if (model.ID.Equals(0))
                        _repository.Add(model);
                    else
                        _repository.Update(model);
                }

                _unitOfWork.Commit();
                WBSSLStore.Web.Helpers.Caching.SiteCacher.ClearCache(Site.ID);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return RedirectToAction("googleanalytics", new { msg = "2" });
            }
            return RedirectToAction("googleanalytics", new { msg = "1" });
        }

        public ActionResult banner()
        {
            SiteSettings setting = _repository.Find(ss => ss.SiteID == Site.ID && ss.Key.ToLower() == SettingConstants.CURRENT_BANNER_HTML_KEY.ToLower()).FirstOrDefault();
            if (setting == null)
            {
                setting = new SiteSettings();
                setting.Key = SettingConstants.CURRENT_BANNER_HTML_KEY;
                setting.SiteID = Site.ID;
            }
            return View(setting);
        }

        [HttpPost]
        public ActionResult banner(SiteSettings model)
        {
            SiteSettings setting = null;
            if (ModelState.IsValid)
            {
                if (Convert.ToBoolean(Request.Form["rbtNeedBanner"]))
                {
                    setting = _repository.Find(ss => ss.SiteID == Site.ID && ss.Key.ToLower() == SettingConstants.CURRENT_BANNER_HTML_KEY.ToLower()).FirstOrDefault();
                    if (setting == null)
                    {
                        setting = new SiteSettings();
                        setting.Key = SettingConstants.CURRENT_ANIMATED_BANNER_KEY;
                        setting.SiteID = Site.ID;
                        setting.Value = "true";
                        _repository.Add(setting);

                        setting = new SiteSettings();
                        setting.Key = SettingConstants.CURRENT_BANNER_HTML_KEY;
                        setting.SiteID = Site.ID;
                        setting.Value = model.Value;
                        _repository.Add(setting);
                    }
                    else
                    {
                        setting.Value = model.Value;
                        _repository.Update(setting);
                    }
                }
                else
                {
                    _repository.Delete((ss => ss.SiteID == Site.ID && ss.Key.ToLower() == SettingConstants.CURRENT_ANIMATED_BANNER_KEY.ToLower()));
                    _repository.Delete((ss => ss.SiteID == Site.ID && ss.Key.ToLower() == SettingConstants.CURRENT_BANNER_HTML_KEY.ToLower()));
                }
                _unitOfWork.Commit();
                ViewBag.Message = "<div class='normsg'>" + @WBSSLStore.Resources.GeneralMessage.Message.GeneralSettings_SuccessMsg + "</div>";
                WBSSLStore.Web.Helpers.Caching.SiteCacher.ClearCache(Site.ID);
            }
            return View(setting ?? model);
        }

        public ActionResult DeleteLogo()
        {
            var _data = DependencyResolver.Current.GetService<IRepository<SiteSettings>>();
            var _items = _data.Find(ss => ss.SiteID == Site.ID && ss.Key.ToLower().Equals(SettingConstants.CURRENT_SITELOGO_KEY.ToLower())).FirstOrDefault();
            try
            {
                if (_items != null)
                {
                    try
                    {
                        if (System.IO.File.Exists(Request.PhysicalApplicationPath + "\\Upload\\" + Site.ID + "\\" + _items.Value))
                            System.IO.File.Delete(Request.PhysicalApplicationPath + "\\Upload\\" + Site.ID + "\\" + _items.Value);
                    }
                    catch { }

                    _items.Key = SettingConstants.CURRENT_SITELOGO_KEY;
                    _data.Delete(_items);
                    _unitOfWork.Commit();
                    WBSSLStore.Web.Helpers.Caching.SiteCacher.ClearCache(Site.ID);

                }
                return Json(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return Json(false);
            }
        }

        public ActionResult apicredential()
        {
            var objsite = DependencyResolver.Current.GetService<IRepository<Site>>();
            var _objdata = objsite.Find(ss => ss.ID == Site.ID).FirstOrDefault();
            return View(_objdata);
        }

        [HttpPost]
        public ActionResult apicredential(Site model)
        {
            var _site = DependencyResolver.Current.GetService<IRepository<Site>>();
            var objSite = _site.FindByID(Site.ID);
            objSite.APIAuthToken = model.APIAuthToken;

            _site.Update(objSite);
            _unitOfWork.Commit();
            WBSSLStore.Web.Helpers.Caching.SiteCacher.ClearCache(Site.ID);
            ViewBag.Message = @WBSSLStore.Resources.GeneralMessage.Message.GeneralSettings_SuccessMsg;

            return View(objSite);
        }

        public ActionResult DeleteCartLogo()
        {
            var _data = DependencyResolver.Current.GetService<IRepository<SiteSettings>>();
            var _items = _data.Find(ss => ss.SiteID == Site.ID && ss.Key.ToLower().Equals(SettingConstants.CURRENT_SITEMYCART_KEY.ToLower())).FirstOrDefault();
            try
            {
                if (_items != null)
                {

                    if (System.IO.File.Exists(Request.PhysicalApplicationPath + "\\Upload\\" + Site.ID + "\\" + _items.Value))
                        System.IO.File.Delete(Request.PhysicalApplicationPath + "\\Upload\\" + Site.ID + "\\" + _items.Value);
                    _items.Key = SettingConstants.CURRENT_SITEMYCART_KEY;
                    _data.Delete(_items);
                    _unitOfWork.Commit();
                    WBSSLStore.Web.Helpers.Caching.SiteCacher.ClearCache(Site.ID);

                }
                return Json(true);
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                return Json(false);
            }
        }
    }
}