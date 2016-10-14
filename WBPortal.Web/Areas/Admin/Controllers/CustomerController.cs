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
using WBSSLStore.Web.Helpers.PagedList;
using WBSSLStore.Web.Util;
using WBSSLStore.Web.Helpers;
using WBSSLStore.Web.Helpers.Caching;
using WBSSLStore.Web.Helpers.Authentication;

namespace WBSSLStore.Web.Areas.Admin.Controllers
{
    [CustomAuthorizeAttribute]
    [HandleError]
    public class CustomerController : WBController<User, IRepository<User>, IUserService>
    {
        private User _user;
        private User CurrentUser
        {
            get
            {
                if (User.Identity.IsAuthenticated && _user == null)
                {
                    SSLStoreUser user1 = ((SSLStoreUser)System.Web.Security.Membership.GetUser());
                    if (user1 != null && user1.Details != null)
                        _user = user1.Details;
                    else if (User.Identity.IsAuthenticated && !string.IsNullOrEmpty(User.Identity.Name))
                    {
                        user1 = ((SSLStoreUser)System.Web.Security.Membership.GetUser(User.Identity.Name));
                        if (user1 != null && user1.Details != null)
                            _user = user1.Details;
                    }
                }
                return _user;
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

                var cutomers = _service.GetCustomerList(Site.ID, strName, strEmail).OrderBy(ord => ord.TotalPurchase).ThenBy(t => t.CompanyName).ThenBy(n => n.FirstName).ToPagedList(page.HasValue ? page.Value - 1 : 0, WBHelper.PageSize(Site));
                cutomers.ActionName = "Index";
                cutomers.ControllerName = "Customer";

                if (!PagingInputValidator.IsPagingInputValid(ref page, cutomers))
                    return View();

                return View(cutomers);
            }
        }

        // GET: /Admin/Reseller/Create
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
                user.UserType = UserType.CUSTOMER;

                if (AddEdit(user))
                    return RedirectToAction("index");
            }            
            BindDefaultValues(user.ID);
            return View("Edit", user);
        }

        //// GET: /Admin/Reseller/Edit/5
        public ActionResult Edit(int id)
        {
            var user = _repository.Find(c => c.ID == id && c.SiteID == Site.ID).FirstOrDefault();
            if (id > 0)
                user.ConfirmPassword = user.PasswordHash;
            BindDefaultValues(id);

            return View(user);
        }

        //// POST: /Admin/Reseller/Edit/5
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

        ////// GET: /Admin/logonuser/5
        //public ActionResult logonuser(int id)
        //{
        //    var user = _repository.Find(c => c.ID == id && c.SiteID == Site.ID).FirstOrDefault();
        //    if (id > 0)
        //        user.ConfirmPassword = user.PasswordHash;
        //    BindDefaultValues(id);

        //    return View(user);
        //}

        //// POST: /Admin/logonuser/5
        //[HttpPost]
        public ActionResult logonuser(int id)
        {
            var user = _repository.Find(c => c.ID == id && c.SiteID == Site.ID).FirstOrDefault();
            string authtoken = string.Empty;
            authtoken = HttpUtility.UrlEncode(WBSSLStore.CryptorEngine.Encrypt("true" + SettingConstants.Seprate + user.Email + SettingConstants.Seprate + "NA" + SettingConstants.Seprate + "100", true));
            if (user != null)
                return new RedirectResult("/staticpage/logonresult?authtoken=" + authtoken);
            else
            {
                BindDefaultValues(id);
                return View(user);
            }
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
            ViewBag.TotalPurchase = 0;
            ViewBag.TotalCertificates = 0;

            if (id > 0)
            {
                decimal? dTotalPurchase = _service.GetTotalPurchase(id);
                ViewBag.TotalPurchase = dTotalPurchase == null ? 0.ToString("C") : Convert.ToDecimal(dTotalPurchase).ToString("C");
                ViewBag.TotalCertificates = _service.GetCertificateCount(id);
            }
        }

        public bool AddEdit(User user)
        {
            try
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
                user.AuditDetails.ByUserID = CurrentUser.ID;
                user.AuditDetails.HttpHeaderDump = HttpUtility.UrlDecode(Request.Headers.ToString());
                user.AuditDetails.IP = Request.UserHostAddress;

                user.UserType = UserType.CUSTOMER;
                user.UserTypeID = (int)UserType.CUSTOMER;

                if (user.ID == 0)
                {
                    user.AuditDetails.DateCreated = DateTimeWithZone.Now;
                    user.SiteID = Site.ID;

                    user.PasswordSalt = WBSSLStore.Web.Helpers.WBHelper.CreateSalt();
                    user.PasswordHash = WBSSLStore.Web.Helpers.WBHelper.CreatePasswordHash(user.PasswordHash, user.PasswordSalt);
                    user.ConfirmPassword = user.PasswordHash;
                }
                return _service.SaveCustomer(user, WBHelper.CurrentLangID(), SiteCacher.SiteSMTPDetail().ID);
            }
            catch(Exception exc)
            {
                ViewBag.ErrMsg = exc.Message.ToString();
                return false;
            }
        }

        [HttpPost]
        public void ExportDataToCSV(string Name, string Email)
        {
            var cutomers = _service.GetCustomerList(Site.ID, Name, Email).OrderBy(ord => ord.TotalPurchase).ThenBy(t => t.CompanyName).ThenBy(n => n.FirstName);

            List<SelectListItem> lstheader = new List<SelectListItem>();
            lstheader.Add(new SelectListItem() { Text = "First Name", Value = "FirstName" });
            lstheader.Add(new SelectListItem() { Text = "Last Name", Value = "LastName" });
            lstheader.Add(new SelectListItem() { Text = "Reseller Since", Value = "RegisterDate" });
            lstheader.Add(new SelectListItem() { Text = "Total Purchase", Value = "TotalPurchase" });
            lstheader.Add(new SelectListItem() { Text = "IsActive", Value = "RecordStatusID" });

            WBHelper.GetCSVFile<UserExt>(cutomers.ToList(), lstheader, "CustomerList");
        }
    }
}

