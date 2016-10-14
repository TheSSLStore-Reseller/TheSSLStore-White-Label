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
using System.Collections;
using WBSSLStore.Web.Helpers.Caching;
using WBSSLStore.Web.Helpers.Authentication;

namespace WBSSLStore.Web.Areas.Admin.Controllers
{
    [CustomAuthorizeAttribute]
    [HandleError]
    public class ResellerController : WBController<ResellerContract, IRepository<ResellerContract>, IUserService>
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
            ViewBag.StartDate = collection["txtFromDate"] ?? Request.QueryString["txtFromDate"];
            ViewBag.EndDate = collection["txtToDate"] ?? Request.QueryString["txtToDate"];
            ViewBag.Status = collection["ddlStatus"] ?? Request.QueryString["ddlStatus"];
            ViewBag.Email = collection["txtEmail"] ?? Request.QueryString["txtEmail"];
            ViewBag.ResellerName = collection["txtResellerName"] ?? Request.QueryString["txtResellerName"];
            ViewBag.TotalPurchase = collection["ddlTotalPurchase"] ?? Request.QueryString["ddlTotalPurchase"];

            RecordStatus? eRecordStatus = null;
            if (!PagingInputValidator.IsPagingInputValid(ref page))
                return View();

            DateTime dtStartDate = ViewBag.StartDate == null ? DateTimeWithZone.Now.AddYears(-1) : Convert.ToDateTime(ViewBag.StartDate);
            DateTime dtEndDate = ViewBag.EndDate == null ? DateTimeWithZone.Now : Convert.ToDateTime(ViewBag.EndDate);

            decimal MinPrice = 0, MaxPrice = Decimal.MaxValue;
            if (!string.IsNullOrEmpty(ViewBag.TotalPurchase))
            {
                string[] strVal = ViewBag.TotalPurchase.ToString().Split('-');
                if (strVal != null && strVal.Length > 1)
                {
                    MinPrice = Convert.ToDecimal(strVal[0]);
                    MaxPrice = Convert.ToDecimal(strVal[1]);
                }
            }
            if (!string.IsNullOrEmpty(ViewBag.Status))
                eRecordStatus = (RecordStatus?)Enum.Parse(typeof(RecordStatus), ViewBag.Status.ToString(), true);

            ViewBag.Status = eRecordStatus.ToString();

            string strResellerName = ViewBag.ResellerName, strEmail = ViewBag.Email;

            if (collection["btnExport"] != null && collection["btnExport"].ToString().ToLower().Equals("export to csv"))
            {
                ExportDataToCSV(strResellerName, strEmail, eRecordStatus, dtStartDate, dtEndDate, MinPrice, MaxPrice);
                return View();
            }
            else
            {
                var resellers = _service.GetResellerData(Site.ID, strResellerName, strEmail, eRecordStatus, dtStartDate, dtEndDate, MinPrice, MaxPrice).OrderBy(ord => ord.TotalPurchase).ThenBy(t => t.CompanyName).ThenBy(n => n.FirstName).ToPagedList(page.HasValue ? page.Value - 1 : 0, WBHelper.PageSize(Site));
                resellers.ActionName = "Index";
                resellers.ControllerName = "Reseller";

                if (!PagingInputValidator.IsPagingInputValid(ref page, resellers))
                    return View();

                return View(resellers);
            }
        }

        public ActionResult Create()
        {
            BindDefaultValues(0);
            _viewModel.Site = Site;
            _viewModel.Reseller = new User();
            _viewModel.Contract = new Contract();
            _viewModel.AuditDetails = new Audit();
            _viewModel.Reseller.AuditDetails = new Audit();
            _viewModel.Reseller.Address = new Address();

            CusUserUserOption objModel = new CusUserUserOption();
            objModel.objResContract = _viewModel;
            objModel.objUserOption = new UserOptions();
            return View("Edit", objModel);
        }

        //// POST: /Admin/Reseller/Create
        [HttpPost]
        public ActionResult Create(CusUserUserOption resellercontract)
        {
            //var d = ModelState.Values.ToList().Where(x => x.Errors.Count > 0).ToList();
            if (ModelState.IsValid)
            {
                resellercontract.objResContract.SiteID = Site.ID;
                resellercontract.objResContract.Reseller.SiteID = Site.ID;
                resellercontract.objResContract.Reseller.UserType = UserType.RESELLER;

                if (AddEdit(resellercontract))
                    return RedirectToAction("index");
            }
            BindDefaultValues(resellercontract.objResContract.ID);
            return View("Edit", resellercontract);
        }

        //// GET: /Admin/Reseller/Edit/5
        public ActionResult Edit(int id)
        {
            var resellercontracts = _repository.Find(c => c.Reseller.ID == id && c.Reseller.SiteID == Site.ID).EagerLoad(c => c.Reseller, c => c.Reseller.Address, c => c.Contract).FirstOrDefault();
            UserOptions _useroption = null;
            if (resellercontracts != null && resellercontracts.UserID > 0)
                _useroption = _service.GetUserOptions(resellercontracts.UserID, resellercontracts.SiteID);

            if (_useroption == null)
            {
                _useroption = new UserOptions();
                _useroption.SiteID = resellercontracts.SiteID;
                _useroption.UserID = resellercontracts.UserID;
                _useroption.DateAdded = DateTimeWithZone.Now;
            }

           if (id > 0){
               resellercontracts.Reseller.ConfirmPassword = resellercontracts.Reseller.PasswordHash;                
            }
            BindDefaultValues(id);

            CusUserUserOption objData = new CusUserUserOption();
            objData.objResContract = resellercontracts;
            objData.objUserOption = _useroption;


            //var _allData = new Tuple<ResellerContract, UserOptions>(resellercontracts, _useroption);


            return View(objData);
        }

        //// POST: /Admin/Reseller/Edit/5
        [HttpPost]
        public ActionResult Edit(CusUserUserOption resellercontract,FormCollection frmcoll)
        {
            if (ModelState.IsValid)
            {
                resellercontract.objUserOption.StopResellerEmail = !frmcoll["chkStopResellerEmail"].ToString().ToLower().Equals("false");
                resellercontract.objUserOption.StopResellerCustomerEmail = !frmcoll["chkStopResellerCustomerEmail"].ToString().ToLower().Equals("false");
                if (AddEdit(resellercontract))
                    return RedirectToAction("index");
            }

            

            BindDefaultValues(resellercontract.objResContract.ID);
            return View(resellercontract);
        }

        [HttpPost]
        public ActionResult ChangeStatus(int id)
        {
            var bStatus = _service.UpdateUserStatus(id, Site.ID, WBHelper.CurrentLangID(), SiteCacher.SiteSMTPDetail().ID);
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

        [HttpPost]
        public void ExportDataToCSV(string ResellerName, string Email, RecordStatus? eRecordStatus, DateTime dtStartDate, DateTime dtEndDate, decimal MinPrice, decimal MaxPrice)
        {
            var resellers = _service.GetResellerData(Site.ID, ResellerName, Email, eRecordStatus, dtStartDate, dtEndDate, MinPrice, MaxPrice).OrderBy(ord => ord.TotalPurchase).ThenBy(t => t.CompanyName).ThenBy(n => n.FirstName);

            List<SelectListItem> lstheader = new List<SelectListItem>();
            lstheader.Add(new SelectListItem() { Text = "First Name", Value = "FirstName" });
            lstheader.Add(new SelectListItem() { Text = "Last Name", Value = "LastName" });
            lstheader.Add(new SelectListItem() { Text = "Contract Name", Value = "ContractName" });
            lstheader.Add(new SelectListItem() { Text = "Reseller Since", Value = "RegisterDate" });
            lstheader.Add(new SelectListItem() { Text = "Total Purchase", Value = "TotalPurchase" });
            lstheader.Add(new SelectListItem() { Text = "IsActive", Value = "RecordStatusID" });

            WBHelper.GetCSVFile<UserExt>(resellers.ToList(), lstheader, "ResellerList");
        }

        public void BindDefaultValues(int id)
        {
            ViewBag.Country = CountryList.ToArray();
            ViewBag.TotalPurchase = 0;
            ViewBag.TotalCertificates = 0;

            ViewBag.Contract = _service.GetAllContract(Site.ID);

            if (id > 0)
            {
                decimal? dTotalPurchase = _service.GetTotalPurchase(id);
                ViewBag.TotalPurchase = dTotalPurchase == null ? 0.ToString("C") : Convert.ToDecimal(dTotalPurchase).ToString("C");
                ViewBag.TotalCertificates = _service.GetCertificateCount(id);
            }
        }

        //public bool AddEdit(ResellerContract resellercontract)
        public bool AddEdit(CusUserUserOption resellercontract)
        {
            try
            {
                bool bIsEmailExist = _service.EmailExist(resellercontract.objResContract.Reseller.Email, Site.ID, resellercontract.objResContract.Reseller.ID);
                if (bIsEmailExist)
                {
                    ViewBag.IsUserExist = true;
                    return false;
                }
                else
                    ViewBag.IsUserExist = false;
                //resellercontract.Reseller.AuditDetails.ID = resellercontract.Reseller.AuditID;
                resellercontract.objResContract.Reseller.AuditDetails.DateModified = DateTimeWithZone.Now;
                resellercontract.objResContract.Reseller.AuditDetails.ByUserID = CurrentUser.ID;
                resellercontract.objResContract.Reseller.AuditDetails.HttpHeaderDump = HttpUtility.UrlDecode(Request.Headers.ToString());
                resellercontract.objResContract.Reseller.AuditDetails.IP = Request.UserHostAddress;

                //  resellercontract.AuditID = resellercontract.AuditDetails.ID;
                resellercontract.objResContract.AuditDetails.DateModified = DateTimeWithZone.Now;
                resellercontract.objResContract.AuditDetails.ByUserID = 0;
                resellercontract.objResContract.AuditDetails.HttpHeaderDump = "admin system";
                resellercontract.objResContract.AuditDetails.IP = Request.UserHostAddress;

                resellercontract.objResContract.Reseller.UserType = UserType.RESELLER;
                resellercontract.objResContract.Reseller.UserTypeID = (int)UserType.RESELLER;

                if (resellercontract.objResContract.UserID == 0)
                {
                    resellercontract.objResContract.Reseller.AuditDetails.DateCreated = DateTimeWithZone.Now;
                    resellercontract.objResContract.AuditDetails.DateCreated = DateTimeWithZone.Now;
                    resellercontract.objResContract.Reseller.SiteID = Site.ID;
                    resellercontract.objResContract.SiteID = Site.ID;

                    resellercontract.objResContract.Reseller.PasswordSalt = WBSSLStore.Web.Helpers.WBHelper.CreateSalt();
                    resellercontract.objResContract.Reseller.PasswordHash = WBSSLStore.Web.Helpers.WBHelper.CreatePasswordHash(resellercontract.objResContract.Reseller.PasswordHash, resellercontract.objResContract.Reseller.PasswordSalt);
                    resellercontract.objResContract.Reseller.ConfirmPassword = resellercontract.objResContract.Reseller.PasswordHash;
                }

                if (resellercontract.objUserOption.ID == 0)
                {
                    resellercontract.objUserOption.SiteID = Site.ID;
                    resellercontract.objUserOption.DateAdded = DateTimeWithZone.Now;
                }

                return _service.SaveReseller(resellercontract.objResContract, WBHelper.CurrentLangID(), SiteCacher.SiteSMTPDetail().ID, resellercontract.objUserOption);
            }
            catch (Exception exc)
            {
                ViewBag.ErrMsg = exc.Message.ToString();
                return false;
            }
        }
    }
}