using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Domain;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Service;
using WBSSLStore.Web.Helpers.PagedList;
using WBSSLStore.Web.Helpers;
using WBSSLStore.Web.Helpers.Caching;
using WBSSLStore.Web.Util;
using System.Web.Security;
using WBSSLStore.Web.Helpers.Authentication;

namespace WBSSLStore.Web.Areas.Admin.Controllers
{
    [CustomAuthorizeAttribute]
    [HandleError]
    public class AdminusersController : WBController<User, IRepository<User>, IUserService>
    {
        //
        // GET: /Admin/Adminusers/


        private User LoginUser
        {
            get
            {
                if (User.Identity.IsAuthenticated)
                {
                    SSLStoreUser loginuser = (SSLStoreUser)Membership.GetUser();
                    if (loginuser != null && loginuser.Details != null)
                        return loginuser.Details;
                    else if (!string.IsNullOrEmpty(User.Identity.Name))
                    {
                        loginuser = (WBSSLStore.Web.Helpers.Authentication.SSLStoreUser)Membership.GetUser(User.Identity.Name);
                        if (loginuser != null && loginuser.Details != null)
                        {
                            return loginuser.Details;
                        }
                    }

                }

                return null;
            }
        }

        public ViewResult Preferences()
        {
            return View(LoginUser);
        }
        [HttpPost]
        public ActionResult Preferences(FormCollection collection)
        {

            if (!string.IsNullOrEmpty(collection["txtPassword"]))
            {
                string PasswordHash = WBHelper.CreatePasswordHash(collection["txtPassword"].ToString(), LoginUser.PasswordSalt);
                WBSSLStore.Domain.User objUser = LoginUser;//_repository.Find(u => u.ID == LoginUser.ID && u.SiteID == Site.ID).FirstOrDefault();
                objUser.ConfirmPassword = PasswordHash;
                objUser.PasswordHash = PasswordHash;
                _repository.Update(objUser);
                _unitOfWork.Commit();

                var _emailservice = DependencyResolver.Current.GetService<IEmailQueueService>();
                if (LoginUser.UserType == UserType.CUSTOMER)
                    _emailservice.PrepareEmailQueue(Site.ID, WBHelper.CurrentLangID(), EmailType.CUSTOMER_CHANGE_PASSWORD, SiteCacher.SiteSMTPDetail().ID, LoginUser.Email, LoginUser);
                else
                    _emailservice.PrepareEmailQueue(Site.ID, WBHelper.CurrentLangID(), EmailType.RESELLER_CHANGE_PASSWORD_EMAIL, SiteCacher.SiteSMTPDetail().ID, LoginUser.Email, LoginUser);
                _unitOfWork.Commit();

                //ViewBag.Message = "<div class='normsg'>Password change successfully.</div>";
                ViewBag.Message = "<div class='normsg'>" + WBSSLStore.Resources.GeneralMessage.Message.Pwdchanged + "</div>";
                return View(LoginUser);
            }
            else
            {
                //ViewBag.Message = "<div class='errormsg'>Please Enter Password.</div>";
                ViewBag.Message = "<div class='errormsg'>" + WBSSLStore.Resources.ErrorMessage.Message.PasswordRequired + "</div>";
                return View(LoginUser);
            }
        }
        public ViewResult Index(FormCollection collection, int? page)
        {
            ViewBag.Email = collection["txtEmail"] ?? Request.QueryString["email"];
            ViewBag.Name = collection["txtName"] ?? Request.QueryString["name"];

            string strName = ViewBag.Name, strEmail = ViewBag.Email;
            if (Request.Form["btnExport"] != null && Request.Form["btnExport"].ToString().ToLower().Equals("export to csv"))
            {
                ExportDataToCSV(strName, strEmail);
                return View();
            }
            else
            {
                if (!PagingInputValidator.IsPagingInputValid(ref page))
                    return View();

                int userid = LoginUser.ID;
                var cutomers = _repository.Find(u => u.SiteID == Site.ID && u.ID != userid && u.UserTypeID != (int)UserType.RESELLER && u.UserTypeID != (int)UserType.CUSTOMER
                                        && u.RecordStatusID != (int)RecordStatus.DELETED
                                        && (u.FirstName + " " + u.LastName).Contains((string.IsNullOrEmpty(strName) ? u.FirstName : strName))
                                        && u.Email.Contains((string.IsNullOrEmpty(strEmail) ? u.Email : strEmail))).EagerLoad(u => u.AuditDetails).OrderBy(t => t.CompanyName).ThenBy(n => n.FirstName).ToPagedList(page.HasValue ? page.Value - 1 : 0, WBHelper.PageSize(Site));

                cutomers.ActionName = "Index";
                cutomers.ControllerName = "Adminusers";

                if (!PagingInputValidator.IsPagingInputValid(ref page, cutomers))
                    return View();

                return View(cutomers);
            }
        }

        // GET: /Admin/Adminusers/Create
        public ActionResult Create()
        {
            BindDefaultValues(0);
            _viewModel.Site = Site;
            _viewModel.AuditDetails = DependencyResolver.Current.GetService<Audit>();
            _viewModel.Address = DependencyResolver.Current.GetService<Address>();
            return View("Edit", _viewModel);
        }

        //// POST: /Admin/Reseller/Create
        [HttpPost]
        public ActionResult Create(User user)
        {
            if (ModelState.IsValid)
            {
                user.SiteID = Site.ID;
                if (AddEdit(user))
                    return RedirectToAction("index");
            }
            BindDefaultValues(user.ID);
            return View("Edit", user);
        }
        public ActionResult Edit(int id)
        {
            var user = _repository.Find(c => c.ID == id && c.SiteID == Site.ID).FirstOrDefault();
            if (id > 0)
                user.ConfirmPassword = user.PasswordHash;
            BindDefaultValues(id);

            return View(user);
        }

        [HttpPost]
        public ActionResult Edit(User user)
        {
            if (ModelState.IsValid)
            {
                if (AddEdit(user))
                    return RedirectToAction("index");
            }
            BindDefaultValues(user.ID);
            return View(user);
        }

        [HttpPost]
        public ActionResult ChangeStatus(int id)
        {
            var bStatus = _service.UpdateUserStatus(id, Site.ID, null, null);
            return Json(bStatus);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var bStatus = _service.DeleteUser(id, Site.ID);
            return Json(bStatus);
        }

        [HttpPost]
        public ActionResult CheckEmailExist(int id)
        {
            var bStatus = _service.EmailExist(HttpUtility.UrlDecode(Request.QueryString[SettingConstants.QS_EMAIL]), Site.ID, id);
            return Json(bStatus);
        }

        public void BindDefaultValues(int id)
        {
            ViewBag.Country = CountryList.ToArray();
        }

        public bool AddEdit(User user)
        {
            bool bIsEmailExist = _service.EmailExist(user.Email, Site.ID, user.ID);
            if (bIsEmailExist)
            {
                ViewBag.IsUserExist = true;
                return false;
            }
            else
                ViewBag.IsUserExist = false;

            user.AuditID = user.AuditDetails.ID;
            user.AuditDetails.DateModified = DateTimeWithZone.Now;
            user.AuditDetails.ByUserID = LoginUser.ID;
            user.AuditDetails.HttpHeaderDump = HttpUtility.UrlDecode(Request.Headers.ToString());
            user.AuditDetails.IP = Request.UserHostAddress;

            user.UserType = user.UserType;
            user.UserTypeID = user.UserTypeID;

            if (user.ID == 0)
            {
                user.AuditDetails.DateCreated = DateTimeWithZone.Now;
                user.SiteID = Site.ID;

                user.PasswordSalt = WBSSLStore.Web.Helpers.WBHelper.CreateSalt();
                user.PasswordHash = WBSSLStore.Web.Helpers.WBHelper.CreatePasswordHash(user.PasswordHash, user.PasswordSalt);
                user.ConfirmPassword = user.PasswordHash;
            }

            return _service.SaveAdminusers(user, WBHelper.CurrentLangID(), SiteCacher.SiteSMTPDetail().ID);
            //return _service.SaveCustomer(user);
        }

        [HttpPost]
        public void ExportDataToCSV(string Name, string Email)
        {
            //var cutomers = _service.GetCustomerList(Site.ID, Name, Email).OrderBy(ord => ord.TotalPurchase).ThenBy(t => t.CompanyName).ThenBy(n => n.FirstName);

            //List<SelectListItem> lstheader = new List<SelectListItem>();
            //lstheader.Add(new SelectListItem() { Text = "First Name", Value = "FirstName" });
            //lstheader.Add(new SelectListItem() { Text = "Last Name", Value = "LastName" });
            //lstheader.Add(new SelectListItem() { Text = "Reseller Since", Value = "RegisterDate" });
            //lstheader.Add(new SelectListItem() { Text = "Total Purchase", Value = "TotalPurchase" });
            //lstheader.Add(new SelectListItem() { Text = "IsActive", Value = "RecordStatusID" });

            //WBHelper.GetCSVFile<UserExt>(cutomers.ToList(), lstheader, "CustomerList");
        }

        public ActionResult MyAccount()
        {
            var user = _repository.Find(c => c.ID == LoginUser.ID && c.SiteID == Site.ID).FirstOrDefault();            
            user.ConfirmPassword = user.PasswordHash;
            BindDefaultValues(LoginUser.ID);
            return View("edit", LoginUser);
        }

        [HttpPost]
        public ActionResult MyAccount(User user)
        {
            if (ModelState.IsValid)
            {
                user.UserTypeID = LoginUser.UserTypeID;
                if (AddEdit(user))
                    return RedirectToAction("index","home");
            }
            BindDefaultValues(user.ID);
            return View(user);
        }
    }
}
