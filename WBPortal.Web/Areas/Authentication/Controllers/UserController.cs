using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Domain;
using WBSSLStore.Service;
using WBSSLStore.Web.Helpers;
using WBSSLStore.Web.Helpers.Caching;
using System.Web.Security;
using System.Security.Principal;
using WBSSLStore.Web.Helpers.Authentication;

namespace WBSSLStore.Web.Areas.Authentication.Controllers
{
    public class UserController : WBController<User, IRepository<User>, IStaticPageViewModelService>
    {
        private User _user;
        private User CurrentUser
        {
            get
            {
                if (User.Identity.IsAuthenticated && _user == null)
                {
                    SSLStoreUser user1 = ((SSLStoreUser)Membership.GetUser());

                    if (user1 != null && user1.Details != null)
                        _user = user1.Details;
                    else if (User.Identity.IsAuthenticated && !string.IsNullOrEmpty(User.Identity.Name))
                    {
                        user1 = ((SSLStoreUser)Membership.GetUser(User.Identity.Name));

                        if (user1 != null && user1.Details != null)
                            _user = user1.Details;
                    }

                }

                return _user;
            }

        }
        private bool USESSL
        {
            get
            {
                return WBHelper.USESSL(Site);
            }

        }
        private bool NeedApproveReseller
        {
            get
            {
                try
                {
                    if (Site.Settings != null)
                    {
                        return Convert.ToBoolean(Site.Settings.Where(o => o.Key == SettingConstants.CURRENT_SITEAPPROVERESELLER_KEY && o.SiteID == Site.ID).FirstOrDefault().Value);
                    }
                }
                catch { }

                return false;
            }

        }
        //
        // GET: /Authentication/User/ 
        private ActionResult Redirect301(string slug, string Host)
        {


            if (Host.IndexOf('.') > 0 && !(Host.Split('.').Length > 2))
            {
                if (WBHelper.IsRunWithWWW(Site) && !slug.Contains("www."))
                {
                    return Redirect(slug.Replace(Host, "www." + Host));
                }
                else if (!WBHelper.IsRunWithWWW(Site) && slug.Contains("www."))
                    return Redirect(slug.Replace("www.", ""));
            }

            return Redirect(slug);
        }

        [HttpPost]
        //[Route("authentication/user/logon")]
        public ActionResult Logon(User model, string returnUrl)
        {
            Site Site = GetSite(model.SiteID);
            string AuthToken = "";
            string AllowedUserType = "NA";
            if (!string.IsNullOrEmpty(Request.QueryString["AllowedUser"]) && Request.QueryString["AllowedUser"].ToString().Equals("100"))
            {
                AllowedUserType = Request.QueryString["AllowedUser"];
            }

            if (ModelState.IsValidField("Email") && ModelState.IsValidField("PasswordHash"))
            {
                Membership.ApplicationName = model.SiteID.ToString();
                if (Membership.ValidateUser(model.Email.Trim(), model.PasswordHash))
                {

                    AuthToken = HttpUtility.UrlEncode(CryptorEngine.Encrypt("true" + SettingConstants.Seprate + model.Email + SettingConstants.Seprate + "NA" + SettingConstants.Seprate + AllowedUserType, true));
                }
                else
                {
                    AuthToken = HttpUtility.UrlEncode(CryptorEngine.Encrypt("false" + SettingConstants.Seprate + "NA" + SettingConstants.Seprate + "-1" + SettingConstants.Seprate + AllowedUserType, true));

                }
            }
            else
            {
                AuthToken = HttpUtility.UrlEncode(CryptorEngine.Encrypt("false" + SettingConstants.Seprate + "NA" + SettingConstants.Seprate + "-2" + SettingConstants.Seprate + AllowedUserType, true));
            }

            // If we got this far, something failed, redisplay form

            string url = string.Empty;
            if (!AllowedUserType.Equals("100"))
            {
                return RedirectToAction("logonresult", "staticpage", new { area = "", authtoken = AuthToken });
                
            }
            else
                url = "http://" + (string.IsNullOrEmpty(Site.Alias) ? Site.CName : Site.Alias) + "/home/undermaintenanceresult?authtoken=" + AuthToken;

            return Redirect301(url, (string.IsNullOrEmpty(Site.Alias) ? Site.CName : Site.Alias));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(FormCollection model, Uservalidate modelvalidate)
        {
            Site Site = GetSite(Convert.ToInt32(model["SiteID"]));
            string SiteAlias = WBHelper.USESSL(Site) ? (Request.Url.Scheme + "://" + (string.IsNullOrEmpty(Site.Alias) ? Site.CName : Site.Alias)) : (string.IsNullOrEmpty(Site.Alias) ? Site.CName : Site.Alias);
            string url = "/checkout/payment/index/" + model["co_has_sid"];
            string user = model["co_login_emailid"] ?? modelvalidate.Email;
            string pwd = model["co_login_passwd"] ?? modelvalidate.Password;
            string AuthToken = string.Empty;
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pwd))
                {
                    Membership.ApplicationName = model["SiteID"];
                    if (Membership.ValidateUser(user, pwd))
                    {
                        FormsAuthentication.SetAuthCookie(user, false);
                        Request.RequestContext.HttpContext.User = new GenericPrincipal(new GenericIdentity(user, "Forms"), null);
                        //Update Shopping Cart Here
                        User U = CurrentUser;
                        int CartID = 0;
                        if (U != null && U.ID > 0)
                        {
                            int contractID = WBHelper.GetCurrentContractID(U.ID, Site.ID);
                            var repo = DependencyResolver.Current.GetService<ICheckoutService>();
                            CartID = Convert.ToInt32(model["co_has_sid"]);
                            CartID = repo.UpdateShoppingCart(U, CartID, Site.ID, contractID, string.Empty);
                      
                        }
                        //End Here
                      
                        return RedirectToAction("index", "payment", new { id = CartID, area = "Checkout" });
                    }
                    else
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            FormsAuthentication.SignOut();
                        }
                        url = (USESSL ? "https" : "http") + "://" + ((WBHelper.IsRunWithWWW(Site) && !(string.IsNullOrEmpty(Site.Alias) ? Site.CName : Site.Alias).Contains("www.")) ? "www." : "") + (string.IsNullOrEmpty(Site.Alias) ? Site.CName : Site.Alias) + "/shoppingcart/checkoutresult?errorcode=-1&sid=" + model["co_has_sid"];
                        return Redirect(url);
                    }
                }
                else if (!string.IsNullOrEmpty(user) && string.IsNullOrEmpty(pwd))
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        FormsAuthentication.SignOut();
                    }
                    return RedirectToAction("index", "payment", new { id = model["co_has_sid"], eid = user, area = "Checkout" });
                }
                else
                {
                    url = (USESSL ? "https" : "http") + "://" + (string.IsNullOrEmpty(Site.Alias) ? Site.CName : Site.Alias) + "/shoppingcart/checkoutresult?errorcode=-2&sid=" + model["co_has_sid"];
                    return Redirect301(url, (string.IsNullOrEmpty(Site.Alias) ? Site.CName : Site.Alias));
                }
            }
            else
            {
                url = (USESSL ? "https" : "http") + "://" + (string.IsNullOrEmpty(Site.Alias) ? Site.CName : Site.Alias) + "/shoppingcart/checkoutresult?errorcode=-1&sid=" + model["co_has_sid"];
                return Redirect(url);
              
            }


        }

        [HttpPost]
        public ActionResult resellersignup(User user)
        {
            Site Site = GetSite(user.SiteID);
            string AuthToken = "";
            if (ModelState.IsValid)
            {
                user.RecordStatusID = NeedApproveReseller ? (int)RecordStatus.INACTIVE : (int)RecordStatus.ACTIVE;
                user.PasswordSalt = WBHelper.CreateSalt();
                user.PasswordHash = WBHelper.CreatePasswordHash(user.PasswordHash, user.PasswordSalt);

                int result = _service.SaveReseller(user, Site.ID, WBHelper.CurrentLangID(), SiteCacher.SiteSMTPDetail().ID, WBHelper.SiteAdminEmail(Site));
                if (result.Equals(1))
                {

                    if (!NeedApproveReseller)
                    {
                        AuthToken = HttpUtility.UrlEncode(CryptorEngine.Encrypt("true" + SettingConstants.Seprate + user.Email + SettingConstants.Seprate + "NA" + SettingConstants.Seprate + "true", true));

                    }
                    else
                    {
                        AuthToken = HttpUtility.UrlEncode(CryptorEngine.Encrypt("true" + SettingConstants.Seprate + user.Email + SettingConstants.Seprate + "NA" + SettingConstants.Seprate + "false", false));

                    }
                }
                else if (result.Equals(-1))
                {
                    AuthToken = HttpUtility.UrlEncode(CryptorEngine.Encrypt("false" + SettingConstants.Seprate + "NA" + SettingConstants.Seprate + "-1" + SettingConstants.Seprate + "false", true));

                }
                else
                    AuthToken = HttpUtility.UrlEncode(CryptorEngine.Encrypt("false" + SettingConstants.Seprate + "NA" + SettingConstants.Seprate + "-2" + SettingConstants.Seprate + "false", true));
            }
            else
                AuthToken = HttpUtility.UrlEncode(CryptorEngine.Encrypt("false" + SettingConstants.Seprate + "NA" + SettingConstants.Seprate + "-3" + SettingConstants.Seprate + "false", true));

            // If we got this far, something failed, redisplay form
            string url = "http://" + (string.IsNullOrEmpty(Site.Alias) ? Site.CName : Site.Alias) + "/staticpage/resellersignupresult?authtoken=" + AuthToken;

            return Redirect301(url, (string.IsNullOrEmpty(Site.Alias) ? Site.CName : Site.Alias));
        }

        public ActionResult SetAuthCookei(string AuthToken)
        {
            string CentralAPISiteAlias = (USESSL ? "https" : "http") + "://" + (string.IsNullOrEmpty(Site.Alias) ? Site.CName : Site.Alias);
            string ResultToken = string.Empty;
            if (!string.IsNullOrEmpty(AuthToken))
            {
                ResultToken = CryptorEngine.Decrypt(HttpUtility.UrlDecode(AuthToken), true);
                ResultToken = ResultToken.Replace("\0", string.Empty);
            }
            if (!string.IsNullOrEmpty(ResultToken))
            {
                string[] arrToken = null;
                if (ResultToken.IndexOf(SettingConstants.Seprate) > 0)
                {
                    arrToken = ResultToken.Split(SettingConstants.Seprate.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    int UserID = Convert.ToInt32(arrToken[0]);
                    string UserName = arrToken[1].Replace("NA", string.Empty);
                    string ReturnUrl = arrToken[2].Replace("NA", string.Empty);

                    if (UserID > 0 && !string.IsNullOrEmpty(UserName))
                    {
                        User user = _repository.FindByID(UserID);
                        if (user != null && UserName.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
                        {
                            FormsAuthentication.SetAuthCookie(user.Email, false);
                            Membership.ApplicationName = user.SiteID.ToString();
                            Request.RequestContext.HttpContext.User = new GenericPrincipal(new GenericIdentity(user.Email, "Forms"), null);
                        }

                        return Redirect(CentralAPISiteAlias + ReturnUrl);

                    }
                    else
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            FormsAuthentication.SignOut();

                        }

                        return Redirect(CentralAPISiteAlias + ReturnUrl);
                    }
                }

            }
            return Redirect(CentralAPISiteAlias + "/Error");
        }

        [HttpPost]
        public ActionResult PasswordReset(User user, FormCollection collection)
        {
            Site Site = GetSite(user.SiteID);
            bool ReturnCode = false;
            if (user.ID > 0)
            {
                User objUser = _repository.FindByID(user.ID);
                objUser.PasswordSalt = WBHelper.CreateSalt();
                objUser.PasswordHash = WBHelper.CreatePasswordHash(collection["txtPassword"], objUser.PasswordSalt);
                objUser.ConfirmPassword = objUser.PasswordHash;
                _repository.Update(objUser);
                _unitOfWork.Commit();
                ReturnCode = true;
            }
            string url = (USESSL ? "https" : "http") + "://" + (string.IsNullOrEmpty(Site.Alias) ? Site.CName : Site.Alias) + "/staticpage/passwordresetresult?authcode=" + HttpUtility.UrlEncode(CryptorEngine.Encrypt(ReturnCode.ToString() + SettingConstants.Seprate + user.ID, true));

            return Redirect301(url, (string.IsNullOrEmpty(Site.Alias) ? Site.CName : Site.Alias));
        }




    }
}
