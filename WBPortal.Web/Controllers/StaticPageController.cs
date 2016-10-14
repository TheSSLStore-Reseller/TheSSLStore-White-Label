using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Domain;
using WBSSLStore.Logger;
using WBSSLStore.Service;
using WBSSLStore.Service.ViewModels;
using WBSSLStore.Web.Helpers;
using WBSSLStore.Web.Helpers.Authentication;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Web.Helpers.Caching;
using WBSSLStore.Web.Util;
using System.Security.Principal;
using System.IO;
using System.Collections.Generic;




namespace WBSSLStore.Web.Controllers
{
    [HandleError]
    public class StaticPageController : WBController<StaticPageViewModel, IRepository<Contract>, IStaticPageViewModelService>
    {

        #region Properties
        private SSLStoreUser _loginuser = null;
        public SSLStoreUser loginuser
        {
            get
            {
                if (_loginuser == null)
                    _loginuser = ((SSLStoreUser)Membership.GetUser());
                return _loginuser;
            }

        }


        #endregion

        int ContractID = 0;
        [Route()]
        public ActionResult index(string slug = "")
        { 
            var currentsite = SiteCacher.CurrentSite;

            if (Convert.ToBoolean(Request.QueryString["init"]))
                SiteCacher.ClearCache(Site.ID);

            using (CurrentSiteSettings settings = new CurrentSiteSettings(currentsite))
            {
                _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/index");
                ReplaceMetaTag();
            }

            int UserID = 0;

            if (User.Identity.IsAuthenticated)
            {
                _viewModel.CurrentUserName = loginuser.Details.FirstName + " " + loginuser.Details.LastName;
                if (loginuser != null && loginuser.Details != null)
                {
                    UserID = loginuser.Details.ID;
                }
                else if (User.Identity.IsAuthenticated && !string.IsNullOrEmpty(User.Identity.Name))
                {
                    if (loginuser != null && loginuser.Details != null)
                    {

                        UserID = loginuser.Details.ID;
                    }
                }
            }

            if (ContractID.Equals(0))
                ContractID = WBSSLStore.Web.Helpers.WBHelper.GetCurrentContractID(UserID, Site.ID);

            _viewModel.Items = _service.GetAllProductPricing(currentsite.ID, ContractID);
            return View(_viewModel);
        }

        //[Route("{slug?}")] 
        public ActionResult indexz()
        {
            var currentsite = SiteCacher.CurrentSite;
            using (CurrentSiteSettings settings = new CurrentSiteSettings(currentsite))
            {
                _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/");
                ReplaceMetaTag();
            }

            int UserID = 0;

            if (User.Identity.IsAuthenticated)
            {
                _viewModel.CurrentUserName = loginuser.Details.FirstName + " " + loginuser.Details.LastName;
                if (loginuser != null && loginuser.Details != null)
                {
                    UserID = loginuser.Details.ID;
                }
                else if (User.Identity.IsAuthenticated && !string.IsNullOrEmpty(User.Identity.Name))
                {
                    if (loginuser != null && loginuser.Details != null)
                    {

                        UserID = loginuser.Details.ID;
                    }
                }
            }

            if (ContractID.Equals(0))
                ContractID = WBSSLStore.Web.Helpers.WBHelper.GetCurrentContractID(UserID, Site.ID);

            _viewModel.Items = _service.GetAllProductPricing(currentsite.ID, ContractID);
            return View(_viewModel);
        }

        [Route("{slug?}")]
        public ActionResult StaticRender(string slug, int? pid)
        {

            //if (WhiteBrandShrink.ConfigurationHelper.IsConfigurationFileExist())
            //{
            //    return RedirectToAction("index", "Install", new { area = "SiteInstallation" });
            //    //return RedirectToRoute("Siteinstallation_us");
            //}

            if (Convert.ToBoolean(Request.QueryString["init"]))
                SiteCacher.ClearCache(Site.ID);

            if (slug.Equals("shoppingcart"))
                return RedirectToRoute("shoppingcart_us", new { area = "" });


            

            if (slug.EndsWith("/"))
                slug = slug + "index"; //landing page

            //filter Page
            using (CurrentSiteSettings settings = new CurrentSiteSettings(SiteCacher.CurrentSite))
            {

                ViewBag.UserName = User.Identity.IsAuthenticated ? loginuser.Details.FirstName + " " + loginuser.Details.LastName : "";

                return FilterPage(slug, settings, pid);
            }


        }

        public ActionResult popup(string layout)
        {
            return View("~/views/shared/" + layout + "/popup.cshtml");
        }
        [Route("Logout", Name = "logout_us")]
        public ActionResult Logout()
        {

            FormsAuthentication.SignOut();

            return Redirect("/");
        }


        public ActionResult resellersignupResult(string AuthToken)
        {
            MemberShipValidationResult obj = new MemberShipValidationResult();
            string ResultToken = "";

            if (!string.IsNullOrEmpty(AuthToken))
            {
                ResultToken = WBSSLStore.CryptorEngine.Decrypt(HttpUtility.UrlDecode(AuthToken), true);
                ResultToken = ResultToken.Replace("\0", string.Empty);
            }
            if (!string.IsNullOrEmpty(ResultToken))
            {
                string[] arrToken = null;
                if (ResultToken.IndexOf(SettingConstants.Seprate) > 0)
                {
                    arrToken = ResultToken.Split(SettingConstants.Seprate.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    obj.IsSuccess = Convert.ToBoolean(arrToken[0]);
                    obj.UserName = arrToken[1].Replace("NA", string.Empty); ;
                    obj.errormsg = arrToken[2].Replace("NA", string.Empty);
                    obj.IsSetAuthCookie = Convert.ToBoolean(arrToken[3]);


                }
                else
                {
                    arrToken = new string[1] { ResultToken };
                    obj.IsSuccess = Convert.ToBoolean(arrToken[0]);
                }
            }

            int siteid = Site.ID;
            if (obj != null && obj.IsSuccess && string.IsNullOrEmpty(obj.errormsg) && !string.IsNullOrEmpty(obj.UserName))
            {
                if (obj.IsSetAuthCookie)
                {

                    FormsAuthentication.SetAuthCookie(obj.UserName, false);
                    return RedirectToAction("index", "orders", new { area = "client" });
                }
                else
                {

                    ViewBag.Country = CountryList.ToArray();
                    ViewBag.MetaData = new CMSPage();
                    User user = new Domain.User();
                    user.SiteID = siteid;
                    ViewBag.Message = "<div class='normsg'>Your request is received. As soon as its verified by our team, you will be notified via email.If you need help with your orders please do not hesitate to contact us at <a href='mailto: " + WBHelper.SiteSupportEmail + "'>" + WBHelper.SiteSupportEmail + "</a></div>";
                    return View("resellersignup", user);
                }
            }
            else
            {
                if (obj.errormsg.Equals("-1"))
                    ViewBag.Message = "<div class='errormsg'>Email alredy exist,Please enter another email.</div>";
                else if (obj.errormsg.Equals("-3"))
                    ViewBag.Message = "<div class='errormsg'>Please enter all required field value.</div>";
                else
                    ViewBag.Message = "<div class='errormsg'>Error during signup process. Please try again.</div>";

                ViewBag.Country = CountryList.ToArray();
                ViewBag.MetaData = new CMSPage();
                User user = new Domain.User();
                user.SiteID = siteid;
                return View("resellersignup", user);
            }
        }

        public ActionResult LogonResult(string AuthToken)
        {
            MemberShipValidationResult obj = new MemberShipValidationResult();
            string ResultToken = "";

            if (!string.IsNullOrEmpty(AuthToken))
            {
                ResultToken = WBSSLStore.CryptorEngine.Decrypt(HttpUtility.UrlDecode(AuthToken), true);
                ResultToken = ResultToken.Replace("\0", string.Empty);
            }
            if (!string.IsNullOrEmpty(ResultToken))
            {
                string[] arrToken = null;
                if (ResultToken.IndexOf(SettingConstants.Seprate) > 0)
                {
                    arrToken = ResultToken.Split(SettingConstants.Seprate.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    obj.IsSuccess = Convert.ToBoolean(arrToken[0]);
                    if (arrToken[1].ToLower() == "na")
                        arrToken[1] = arrToken[1].Replace("NA", string.Empty);
                    obj.UserName = arrToken[1];
                    if (arrToken[2].ToLower() == "na")
                        arrToken[2] = arrToken[2].Replace("NA", string.Empty);
                    obj.errormsg = arrToken[2];
                }
                else
                {
                    arrToken = new string[1] { ResultToken };
                    obj.IsSuccess = Convert.ToBoolean(arrToken[0]);
                }
            }

            if (obj != null && obj.IsSuccess && string.IsNullOrEmpty(obj.errormsg))
            {
                int siteid = Site.ID;
                FormsAuthentication.SetAuthCookie(obj.UserName, false);
                Request.RequestContext.HttpContext.User = new GenericPrincipal(new GenericIdentity(obj.UserName, "Forms"), null);


                SSLStoreUser U = ((SSLStoreUser)System.Web.Security.Membership.GetUser(obj.UserName));



                if (Roles.GetRolesForUser(obj.UserName).Contains(UserType.ADMIN.ToString().ToLower()) || Roles.GetRolesForUser(obj.UserName).Contains(UserType.FINANCE.ToString().ToLower()) || Roles.GetRolesForUser(obj.UserName).Contains(UserType.SUPPORT.ToString().ToLower()))
                {
                    if (!string.IsNullOrEmpty(obj.ReturnUrl))
                    {
                        return Redirect(obj.ReturnUrl);
                    }
                    else if (!string.IsNullOrEmpty(Request.QueryString["returnurl"]))
                    {
                        return Redirect(Request.QueryString["returnurl"]);
                    }
                    else
                        return Redirect("/admin/home");
                }

                if (U != null && U.Details != null)
                {
                    if (U != null && U.Details.ID > 0)
                    {
                        ContractID = Helpers.WBHelper.GetCurrentContractID(U.Details.ID, siteid);
                        var checkoutservice = DependencyResolver.Current.GetService<ICheckoutService>();
                        if (checkoutservice != null)
                        {

                            int cartid = checkoutservice.UpdateShoppingCart(U.Details, 0, siteid, ContractID, Request.AnonymousID);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(Request.QueryString["returnurl"]))
                {
                    return Redirect(Request.QueryString["returnurl"]);
                }
                else
                {
                    return Redirect("/");
                }
            }
            else
            {

                if (!string.IsNullOrEmpty(obj.errormsg))
                {
                    if (obj.errormsg.Equals("-1"))
                        obj.errormsg = "The user name or password provided is incorrect. Please try again.";

                    if (obj.errormsg.Equals("-2"))
                        obj.errormsg = "Please enter valid user name or password.";
                }
                else
                {
                    obj.errormsg = "Error during autentication process. Please try again.";
                }
                ViewBag.Error = obj.errormsg;
                User user = new User();
                user.SiteID = Site.ID;
                checkhttps();
                return View("logon", user);
            }

        }

        [Route("Logon", Name = "logon_us")]
        public ActionResult Logon()
        {
            //var currentsite = SiteCacher.CurrentSite;
            if (HttpContext.User != null && HttpContext.User.Identity.IsAuthenticated)
                return Redirect("/");
            User user = new User();
            user.Address = new Address();
            user.SiteID = Site.ID;
            return View(user);
        }

        [HttpPost]
        public ActionResult CheckEmailExist(int id)
        {
            var _userservice = DependencyResolver.Current.GetService<IUserService>();
            var bStatus = _userservice.EmailExist(HttpUtility.UrlDecode(Request.QueryString[SettingConstants.QS_EMAIL]), Site.ID, id);
            return Json(bStatus);
        }

        [HttpPost]
        public ActionResult Forgotpassword(FormCollection collection)
        {
            string EmailAddress = Convert.ToString(Request.Form["txtEmailAddress"]);
            if (!string.IsNullOrEmpty(EmailAddress))
            {

                ViewBag.Message = _service.SendForgotPasswordEmail(EmailAddress, WBHelper.CurrentLangID(), SiteCacher.SiteSMTPDetail().ID, Site.ID, WBSiteSettings.AppPath + "/resetpassword?token=");

            }
            return View();
        }

        [Route("contactus", Name = "Contact_us")]
        [HttpGet]
        public ActionResult contactus()
        {
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.Url.AbsolutePath);
            ReplaceMetaTag();
            if (User.Identity.IsAuthenticated)
            {
                _viewModel.CurrentUserName = loginuser.Details.FirstName + " " + loginuser.Details.LastName;
            }
            return View(_viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult contactuspost(FormCollection collection)
        {

            string CompanyName = string.Empty; string Name = string.Empty; string Phone = string.Empty; string Email = string.Empty; string Comment = string.Empty;
            string Subject = string.Empty;
            try
            {
                CompanyName = Convert.ToString(collection["txtCompany"]);
                Name = Convert.ToString(collection["txtFullname"]);
                Phone = Convert.ToString(collection["txtPhone"]);
                Email = Convert.ToString(collection["txtEmail"]);
                Comment = Convert.ToString(collection["txtComment"]);



                var subject = Site.Settings.Where(ss => ss.SiteID == Site.ID && ss.Key == SettingConstants.CURRENT_SITE_SUBJECTFEIELD).FirstOrDefault();
                var ToEmail = Site.Settings.Where(ss => ss.SiteID == Site.ID && ss.Key.ToLower() == SettingConstants.CURRENT_SITE_TOEMAIL.ToLower()).FirstOrDefault();
                SiteSettings thankyou = Site.Settings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITE_THANKYOUPAGE && o.SiteID == Site.ID).FirstOrDefault();


                Subject = Convert.ToString(subject.Value);
                var objEmailQue = DependencyResolver.Current.GetService<WBSSLStore.Service.IEmailQueueService>();
                string[] strValues = new string[] { CompanyName, Name, Phone, Email, Comment, Subject };
                objEmailQue.PrepareEmailQueue(Site.ID, WBHelper.CurrentLangID(), EmailType.CONTACTUS_EMAIL, SiteCacher.SiteSMTPDetail().ID, Convert.ToString(ToEmail.Value), strValues);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                return Json(new { issuccess = "false" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { issuccess = "true" }, JsonRequestBehavior.AllowGet);


            //ViewBag.ShowMessage = true;
            //if (string.IsNullOrEmpty(thankyou.Value))
            //    return View("thankyou");
            //else
            //{
            //    Response.Redirect(thankyou.Value);
            //    return View();
            //}
        }

        [Route("PasswordReset", Name = "PasswordReset_us")]
        [HttpGet]
        public ViewResult PasswordReset()
        {
            string strToken = Request.QueryString["token"];
            User objUser = null;
            if (!string.IsNullOrEmpty(strToken))
            {
                int UserID = 0;
                string PasswordHash = string.Empty;
                try
                {

                    string[] strParam = WBSSLStore.CryptorEngine.Decrypt(HttpUtility.HtmlDecode(strToken), true).Split(SettingConstants.Seprate.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    UserID = Convert.ToInt32(strParam[0]);
                    PasswordHash = strParam[1];
                    var _UserRepo = DependencyResolver.Current.GetService<IRepository<User>>();
                    objUser = _UserRepo.Find(u => u.SiteID == Site.ID && u.ID == UserID && u.PasswordHash.Equals(PasswordHash, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    _logger.Log(ex.Message, LogType.ERROR);
                }
                if (objUser != null && objUser.ID > 0)
                {


                }
                else
                {
                    ViewBag.Message = "<div class='errormsg'>Invalid Token.</div>";
                }
            }
            else
                ViewBag.Message = "<div class='errormsg'>Invalid Token.</div>";
            if (objUser == null)
                objUser = new User();
            return View("passwordreset", objUser);
        }

        public ActionResult PasswordResetResult(string AuthCode)
        {
            string[] param = CryptorEngine.Decrypt(HttpUtility.UrlDecode(AuthCode), true).Replace("\0", string.Empty).Split(SettingConstants.Seprate.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            bool ReturnResult = Convert.ToBoolean(param[0]);
            int UserID = Convert.ToInt32(param[1]);
            var _UserRepo = DependencyResolver.Current.GetService<IRepository<User>>();
            User objUser = _UserRepo.FindByID(UserID);
            if (ReturnResult)
            {
                ViewBag.Message = "<div class='normsg'>Password changed successfully. <a href='" + Request.Url.Scheme + "://" + Site.Alias + "/logon" + "'>Click here</a> to login.</div>";
            }
            else
            {
                ViewBag.Message = "<div class='errormsg'>Error while change password, Please try again.</div>";
            }
            return View("PasswordReset", objUser);
        }

        private void SendMailForResellerSignUp(ResellerSignup objReseller)
        {
            try
            {
                var ToEmail = Site.Settings.Where(ss => ss.SiteID == Site.ID && ss.Key.ToLower() == SettingConstants.CURRENT_SITE_TOEMAIL.ToLower()).FirstOrDefault();
                SiteSettings thankyou = Site.Settings.Where(o => o.Key == WBSSLStore.Domain.SettingConstants.CURRENT_SITE_THANKYOUPAGE && o.SiteID == Site.ID).FirstOrDefault();
                var objEmailQue = DependencyResolver.Current.GetService<WBSSLStore.Service.IEmailQueueService>();
                WBSSLStore.Domain.User objUser = new Domain.User();
                objUser.FirstName = objReseller.FirstName;
                objUser.LastName = objReseller.LastName;
                objUser.Email = objReseller.Email;
                objUser.Address = new Address();
                objUser.Address.Street = objReseller.Street;
                objUser.Address.City = objReseller.City;
                objUser.Address.CompanyName = objReseller.CompanyName;
                objUser.Address.Country = new Country();
                objUser.Address.Country.CountryName = objReseller.CountryName;
                objUser.Address.State = objReseller.State;
                objUser.Address.Fax = objReseller.Fax;
                objUser.Address.Phone = objReseller.Phone;
                objUser.CompanyName = objReseller.CountryName;
                objUser.HeardBy = objReseller.HearedBy;

                objEmailQue.PrepareEmailQueue(Site.ID, WBHelper.CurrentLangID(), EmailType.ADMIN_NEW_RESELLER, SiteCacher.SiteSMTPDetail().ID, !string.IsNullOrEmpty(Convert.ToString(ToEmail.Value)) ? Convert.ToString(ToEmail.Value) : "admin@thesslstore.com", objUser);
                objEmailQue.PrepareEmailQueue(Site.ID, WBHelper.CurrentLangID(), EmailType.RESELLER_WELCOME_EMAIL, SiteCacher.SiteSMTPDetail().ID, Convert.ToString(objReseller.Email), objUser);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region ProductType All
        [Route("ssl-solutions", Name = "Allsolutions_us")]
        public ActionResult sslsolutions()
        {
            var currentsite = SiteCacher.CurrentSite;
            using (CurrentSiteSettings settings = new CurrentSiteSettings(currentsite))
            {
                _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.RawUrl);
            }

            int UserID = 0;

            if (User.Identity.IsAuthenticated)
            {
                _viewModel.CurrentUserName = loginuser.Details.FirstName + " " + loginuser.Details.LastName;
                if (loginuser != null && loginuser.Details != null)
                {
                    UserID = loginuser.Details.ID;
                }
                else if (User.Identity.IsAuthenticated && !string.IsNullOrEmpty(User.Identity.Name))
                {
                    if (loginuser != null && loginuser.Details != null)
                    {

                        UserID = loginuser.Details.ID;
                    }
                }
            }

            //if (ContractID.Equals(0))
            //    ContractID = WBSSLStore.Web.Helpers.WBHelper.GetCurrentContractID(UserID, Site.ID);

            
            return View(_viewModel);
        }
        #endregion

        #region Static Pages
        
        [Route("aboutus", Name = "aboutus_us")]
        public ActionResult aboutus()
        {
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.Url.AbsolutePath);
            ReplaceMetaTag();
            if (User.Identity.IsAuthenticated)
            {
                _viewModel.CurrentUserName = loginuser.Details.FirstName + " " + loginuser.Details.LastName;
            }

            return View(_viewModel);
        }
        
        [Route("privacypolicy", Name = "privacypolicy_us")]
        public ActionResult privacypolicy()
        {
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.Url.AbsolutePath);
            ReplaceMetaTag();
            if (User.Identity.IsAuthenticated)
            {
                _viewModel.CurrentUserName = loginuser.Details.FirstName + " " + loginuser.Details.LastName;
            }

            return View(_viewModel);
        }
        
        [Route("refundpolicy", Name = "refundpolicy_us")]
        public ActionResult refundpolicy()
        {
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.Url.AbsolutePath);
            ReplaceMetaTag();
            if (User.Identity.IsAuthenticated)
            {
                _viewModel.CurrentUserName = loginuser.Details.FirstName + " " + loginuser.Details.LastName;
            }

            return View(_viewModel);
        }
        
        [Route("disclaimer", Name = "Disclaimer_us")]
        public ActionResult Disclaimer()
        {
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.Url.AbsolutePath);
            ReplaceMetaTag();
            if (User.Identity.IsAuthenticated)
            {
                _viewModel.CurrentUserName = loginuser.Details.FirstName + " " + loginuser.Details.LastName;
            }

            return View(_viewModel);
        }
        
        [Route("support", Name = "support_us")]
        public ActionResult support()
        {
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.Url.AbsolutePath);
            ReplaceMetaTag();
            if (User.Identity.IsAuthenticated)
            {
                _viewModel.CurrentUserName = loginuser.Details.FirstName + " " + loginuser.Details.LastName;
            }

            return View(_viewModel);
        }

        [Route("becamereseller", Name = "becamereseller_us")]
        public ActionResult becamereseller()
        {
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.Url.AbsolutePath);
            ReplaceMetaTag();
            if (User.Identity.IsAuthenticated)
            {
                _viewModel.CurrentUserName = loginuser.Details.FirstName + " " + loginuser.Details.LastName;
            }
            return View(_viewModel);
        }

        [Route("resellersignup", Name = "resellersignup_us")]
        public ActionResult resellersignup()
        {
            ViewBag.Country = CountryList.ToArray();
            ViewBag.MetaData = new CMSPage();
            User user = new Domain.User();
            user.SiteID =  Site.ID;;
            
            ViewBag.UserName = (User.Identity.IsAuthenticated) ? loginuser.Details.FirstName + " " + loginuser.Details.LastName : "";
            return View("resellersignup", user);
        }


        [Route("faq", Name = "faq_us")]
        public ActionResult faq()
        { 
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), Request.Url.AbsolutePath);
            ReplaceMetaTag();
            if (User.Identity.IsAuthenticated)
            {
                _viewModel.CurrentUserName = loginuser.Details.FirstName + " " + loginuser.Details.LastName;
            }

            return View(_viewModel);
        }

        #endregion

        #region "Private Methods"

        private void SetPricing(int pid, CurrentSiteSettings CurrentSiteSettings, int bid = 0, string code = "")
        {
            int UserID = 0;
            if (User.Identity.IsAuthenticated)
            {
                SSLStoreUser loginuser = ((SSLStoreUser)Membership.GetUser());
                if (loginuser != null && loginuser.Details != null)
                {
                    UserID = loginuser.Details.ID;
                }
                else if (User.Identity.IsAuthenticated && !string.IsNullOrEmpty(User.Identity.Name))
                {
                    loginuser = ((SSLStoreUser)Membership.GetUser());
                    if (loginuser != null && loginuser.Details != null)
                    {

                        UserID = loginuser.Details.ID;
                    }
                }

            }

            if (ContractID.Equals(0))
                ContractID = WBSSLStore.Web.Helpers.WBHelper.GetCurrentContractID(UserID, Site.ID);

            ViewBag.PunchLine = CurrentSiteSettings.PunchLine;
            ViewBag.PunchLine = string.IsNullOrEmpty(ViewBag.PunchLine) ? "Welcome " + SiteCacher.SiteAdminDetail(Site.ID).CompanyName : (ViewBag.PunchLine == "NA" ? string.Empty : ViewBag.PunchLine);


            _viewModel.Items = _service.GetProductPricing(Site.ID, pid, ContractID, bid, code);
        }
        private ActionResult FilterPage(string slug, CurrentSiteSettings CurrentSiteSettings, int? pid)
        {
            if (slug.Equals("JavascriptDisabled"))
                return View(slug);
            if (slug.Equals("favicon.ico"))
                return View(slug);
            if (slug.Equals("sitemap"))
                return View(slug);
            if (slug.Equals("testimonials"))
            {
                if (CurrentSiteSettings.IsTestimonials)
                    return View(slug);
                else
                    return Redirect("/");
            }

            if (slug.Equals("sitemap.xml"))
            {
                ViewBag.ISXML = true;
                return View("SiteMap");
            }
            else if (slug.Equals("robots.txt"))
            {
                var Repo = DependencyResolver.Current.GetService<IRepository<SiteSettings>>();
                var settings = Repo.Find(ss => ss.SiteID == Site.ID && ss.Key == SettingConstants.CURRENT_ROBOTS_FILE_VALUE).FirstOrDefault();

                if (settings == null)
                {
                    settings = new SiteSettings();
                    settings.Key = SettingConstants.CURRENT_ROBOTS_FILE_VALUE;
                    settings.SiteID = Site.ID;
                    settings.Value = string.Empty;
                }

                if (Request.Url.AbsoluteUri.Contains(Site.CName))
                    settings.Value = "disallow: /";

                return Content(settings.Value.Replace("\r\n", Environment.NewLine), "text/Html");
            }
            else
                return PageRendering(slug, CurrentSiteSettings, pid);

        }

        private void checkhttps()
        {
            //host,scheme
            try
            {
                string url = string.Empty;
                if (Request.Url.DnsSafeHost.Contains("localhost") && Request.Url.Scheme.Equals("http"))
                {
                    string dnsf = Convert.ToString(Request.Url.DnsSafeHost);
                    var sitedata = DependencyResolver.Current.GetService<IRepository<Site>>();
                    var data = sitedata.Find(s => s.ID == Site.ID && s.CName == dnsf).FirstOrDefault();
                    if (data != null)
                    {
                        if (Request.Url.Scheme.Equals("http"))
                        {
                            string path = string.Empty;
                            url = "https://" + Request.Url.DnsSafeHost + Request.RawUrl;
                            Response.Redirect(url);
                        }
                    }
                }

            }
            catch (Exception ex)
            { }
        }
        private ActionResult PageRendering(string slug, CurrentSiteSettings settings, int? pid)
        {
            bool isFileExists = false;
            string physicallocation = Server.MapPath(string.Concat(@"~\views\staticpage\", slug.Replace("/", @"\"), ".cshtml"));
            if (System.IO.File.Exists(physicallocation))
                isFileExists = true;

            Pages PG = Site.Pages.Where(P => (P.PageStatusID == (int)PageStatus.Show || P.PageStatusID == (int)PageStatus.HideInNavigation) && P.slug.Equals("/" + slug, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (!isFileExists && PG == null)
            {
                return RedirectToRoute("pagenotfound");
            }

          
            //Get MetaData
            GetMetaDataAndContext(slug, settings);

            if (isFileExists && _viewModel.Items == null)
            {
                int productid = pid.HasValue ? (int)pid : 0;
                SetPricing(productid, settings);
            }


            //End 
            slug = isFileExists ? slug : "innerpage";

            ViewBag.PageBrandID = PG != null ? PG.BrandID : 99;
            return View(slug, _viewModel);

        }
        private void GetMetaDataAndContext(string slug, CurrentSiteSettings settings)
        {
            _viewModel.CMSPage = _service.GetPageMetadata(Site.ID, WBHelper.CurrentLangID(), "/" + slug);
            if (_viewModel.CMSPage != null)
            {
                ReplaceMetaTag();
                _viewModel.CMSPageContent = _service.GetPageContent(_viewModel.CMSPage.ID);

                //get Data For 

                if (_viewModel.CMSPageContent != null && !string.IsNullOrEmpty(_viewModel.CMSPageContent.PageContent))
                {
                    //_viewModel.CMSPageContent.PageContent = ReplacePriceControl(_viewModel.CMSPageContent.PageContent, settings);
                }
                else if (_viewModel.CMSPageContent != null && string.IsNullOrEmpty(_viewModel.CMSPageContent.PageContent))
                {
                    _viewModel.CMSPageContent.PageContent = "<h1>Set Content of your Page.</h1>";
                }
                else if (_viewModel.CMSPageContent == null)
                {
                    _viewModel.CMSPageContent = new CMSPageContent();
                    _viewModel.CMSPageContent.PageContent = "<h1>Set Content of your Page.</h1>";
                    var a = Site.ID;
                    var b = WBHelper.CurrentLangID();

                }

            }
            else
            {
                _viewModel.CMSPage = new CMSPage();
                _viewModel.CMSPageContent = new CMSPageContent();
                _viewModel.CMSPageContent.PageContent = "<h1>Set Content of your Page.</h1>";
            }
        }
        
        private void ReplaceMetaTag()
        {
            if (_viewModel.CMSPage != null)
            {
                if (!string.IsNullOrEmpty(_viewModel.CMSPage.Description))
                    _viewModel.CMSPage.Description = _viewModel.CMSPage.Description.Replace("[DOMAINNAME]", Site.Alias);
                if (!string.IsNullOrEmpty(_viewModel.CMSPage.Title))
                    _viewModel.CMSPage.Title = _viewModel.CMSPage.Title.Replace("[DOMAINNAME]", Site.Alias);
                if (!string.IsNullOrEmpty(_viewModel.CMSPage.Keywords))
                    _viewModel.CMSPage.Keywords = _viewModel.CMSPage.Keywords.Replace("[DOMAINNAME]", Site.Alias);
            }
        }



        #endregion
    }
}
