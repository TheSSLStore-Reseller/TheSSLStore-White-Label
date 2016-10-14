using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Domain;
using WBSSLStore.Service;
using WBSSLStore.Web.Helpers;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Data.Repository;
using WBSSLStore.Web.Helpers.PagedList;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Security.Principal;
using WBSSLStore.Web.Helpers.Caching;
using System.Configuration;


namespace WBSSLStore.Web.Controllers
{
    [HandleError]
    public class HomeController : WBController<User, IRepository<User>, ISiteService>
    {
        [Route("home/pagenotfound", Name = "pagenotfound")]
        public ActionResult pagenotfound()
        {
            return View("404");
        }

        [Route("error", Name = "pageerror")]
        public ActionResult pageerror()
        {
            return View("Error");
        }
        public ActionResult UnderMaintenance(int? error)
        {
            if (error.HasValue)
            {
                ViewBag.Error = WBSSLStore.Resources.ErrorMessage.Message.ValidUsernameandPass;
            }
            else
                ViewBag.Error = "";

            User user = new User();
            user.SiteID = Site.ID;
            return View(user);
        }
        public ActionResult UnderMaintenanceResult(string AuthToken)
        {
            WBSSLStore.Service.ViewModels.MemberShipValidationResult obj = new WBSSLStore.Service.ViewModels.MemberShipValidationResult();
            string ResultToken = "";
            string type = string.Empty;
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
                    obj.UserName = arrToken[1].Replace("NA", string.Empty);
                    obj.errormsg = arrToken[2].Replace("NA", string.Empty);

                    if (arrToken != null && arrToken.Length > 3)
                    {
                        type = arrToken[3];
                    }
                }
                else
                {
                    arrToken = new string[1] { ResultToken };
                    obj.IsSuccess = Convert.ToBoolean(arrToken[0]);
                }
            }

            if (obj != null && obj.IsSuccess && string.IsNullOrEmpty(obj.errormsg) && !string.IsNullOrEmpty(obj.UserName))
            {
                int siteid = Site.ID;
                FormsAuthentication.SetAuthCookie(obj.UserName, false);
                Request.RequestContext.HttpContext.User = new GenericPrincipal(new GenericIdentity(obj.UserName, "Forms"), null);


                if (!string.IsNullOrEmpty(type) && type.Equals("100"))
                {
                    if (Roles.GetRolesForUser(obj.UserName).Contains(UserType.ADMIN.ToString().ToLower()))
                        return Redirect("/admin/home");

                }

            }


            return RedirectToAction("UnderMaintenance", "home", new { error = -1 });
        }
        public ActionResult CheckEmailExist(string email)
        {
            var user = _repository.Find(u => u.Email == email && u.SiteID == Site.ID && u.RecordStatusID == (int)RecordStatus.ACTIVE).FirstOrDefault();      //EmailExist(HttpUtility.UrlDecode(Request.QueryString[SettingConstants.QS_EMAIL]), Site.ID, id);
            if (user != null && user.ID > 0)
                return Json(true);
            else
                return Json(false);
        }
        public ContentResult AutoCompleteResult(string searchText)
        {
            var filteredCountries = States.GetStates().Where(x => x.ToLower().Contains(searchText.ToLower())).ToList();

            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            var jsonString = jsonSerializer.Serialize(filteredCountries).ToString();
            return Content(jsonString);
        }



        #region AdminUser_Forget_Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UnderMaintenanceForgetPassword(WBSSLStore.Domain.User model)
        {
            string res = string.Empty;
            if (model != null)
            {
                var _userservice = DependencyResolver.Current.GetService<IRepository<User>>().Find(x => x.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase) && x.UserTypeID == (int)UserType.ADMIN && x.RecordStatusID == (int)RecordStatus.ACTIVE).ToList().FirstOrDefault();
                var _emailTemp = DependencyResolver.Current.GetService<IRepository<EmailTemplates>>().Find(x => x.EmailTypeId == (int)EmailType.ALL_FORGOTPASSWORD && x.SiteID == model.SiteID).ToList().FirstOrDefault();
                if (_userservice != null)
                {
                    using (System.Net.Mail.MailMessage email = new System.Net.Mail.MailMessage())
                    {
                        try
                        {
                            System.Net.Mail.SmtpClient SmtpMail = new System.Net.Mail.SmtpClient(ConfigurationManager.AppSettings["SmtpServer"]);
                            SmtpMail.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["SMTPUser"], ConfigurationManager.AppSettings["SMTPPassword"]);
                            SmtpMail.Port = Convert.ToInt16(ConfigurationManager.AppSettings["SMTPPort"]);
                            SmtpMail.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["SMTPUseSSL"]);
                            email.To.Add(_userservice.Email.ToLower());
                            email.From = new System.Net.Mail.MailAddress(ConfigurationManager.AppSettings["AdminForgetPasFromEmail"]);
                            email.Bcc.Add(Convert.ToString(ConfigurationManager.AppSettings["AdminForgetPasBCCEmail"]));
                            email.Subject = _emailTemp.EmailSubject;
                            email.Priority = System.Net.Mail.MailPriority.High;
                            email.Body = ForgotPassword(_emailTemp.EmailContent, _userservice, _userservice.Site.Alias);
                            email.IsBodyHtml = true;
                            SmtpMail.Send(email);
                            res = "true";
                        }
                        catch (Exception ex)
                        {
                            _logger.LogException(ex);
                        }
                    }
                }
                else
                {
                    res = "Email is not registered!";
                }
            }
            //else
            //{
            //    res = "Insput string is not valid";
            //}
            return Json(new { msg = res }, JsonRequestBehavior.AllowGet);
        }

        public string ForgotPassword(string content, User rowUser, string SiteAlias)
        {

            using (CurrentSiteSettings currentSiteSettings = new Domain.CurrentSiteSettings(rowUser.Site))
            {
                var site = rowUser.Site;

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

            }

            string sBody = ReplaceMailMerge.ForgotPassword(content, rowUser, "/d/home/resetadpassword?token=");
            return sBody;
        }
        public ActionResult resetadpassword()
        {
            string strToken = Request.QueryString["token"];
            User objUser = null;
            if (!string.IsNullOrEmpty(strToken))
            {
                int UserID = 0;
                string PasswordHash = string.Empty;
                try
                {
                    //string D = HttpUtility.HtmlDecode(strToken);
                    //string[] dm = WBSSLStore.CryptorEngine.Decrypt(D, true).Split(SettingConstants.Seprate.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    string[] strParam = WBSSLStore.CryptorEngine.Decrypt(HttpUtility.HtmlDecode(strToken), true).Split(SettingConstants.Seprate.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    UserID = Convert.ToInt32(strParam[0]);
                    PasswordHash = strParam[1];
                    var _UserRepo = DependencyResolver.Current.GetService<IRepository<User>>();
                    objUser = _UserRepo.Find(u => u.SiteID == Site.ID && u.ID == UserID && u.PasswordHash.Equals(PasswordHash, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    _logger.Log(ex.Message, WBSSLStore.Logger.LogType.ERROR);
                }

                if (objUser == null)
                    ViewBag.Message = "<div class='errormsg'>Invalid Token.</div>";

            }
            else
                ViewBag.Message = "<div class='errormsg'>Invalid Token.</div>";
            if (objUser == null)
                objUser = new User();
            return View(objUser);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult resetadpassword(User user, FormCollection collection)
        {
            Site Site = GetSite(user.SiteID);
            bool ReturnCode = false;
            string msg = string.Empty;
            if (user.ID > 0)
            {
                User objUser = _repository.FindByID(user.ID);
                objUser.PasswordSalt = WBSSLStore.Web.Helpers.WBHelper.CreateSalt();
                objUser.PasswordHash = WBSSLStore.Web.Helpers.WBHelper.CreatePasswordHash(collection["txtPassword"], objUser.PasswordSalt);
                objUser.ConfirmPassword = objUser.PasswordHash;
                _repository.Update(objUser);
                _unitOfWork.Commit();
                ReturnCode = true;

                msg = "<div class='normsg'>Password changed successfully. <a href='" + Request.Url.Scheme + "://" + Site.Alias + "/logon" + "'>Click here</a> to login.</div>";
            }
            return Json(new { d = msg }, JsonRequestBehavior.AllowGet);
        }
        #endregion



        

    }
}
