using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WBSSLStore.Domain;
using WBSSLStore.Data;
using WBSSLStore.Service;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Web.Helpers.Authentication;
using System.Web.Security;
using WBSSLStore.Web.Helpers;
using WBSSLStore.Service.ViewModels;
using WBSSLStore.Web.Util;
using WBSSLStore.Web.Helpers.PagedList;
//using System.Data.Objects;
using WBSSLStore.Web.Helpers.Caching;

namespace WBSSLStore.Web.Areas.Client.Controllers
{
    public class AccountController : WBController<User, IRepository<User>, IUserService>
    {
        private readonly IRepository<ProductPricing> _repoPricing;
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
        private string InvoicePrefix
        {
            get
            {
                return WBHelper.InvoicePrefix(Site);
            }
        }
        private int PageSize
        {
            get
            {
                return WBHelper.PageSize(Site);
            }
        }
        public AccountController(IRepository<ProductPricing> Pricing)
        {
            _repoPricing = Pricing;

        }

        //
        // GET: /Client/Account/Details/5

        public ViewResult Preferences()
        {
          
            return View(LoginUser);
        }
        public ViewResult Pricing()
        {
           
            int ContractID = WBHelper.GetCurrentContractID(LoginUser.ID, Site.ID);
            var pricing = _repoPricing.Find(p => p.SiteID == Site.ID && p.RecordStatusID == (int)RecordStatus.ACTIVE && p.ContractID == ContractID).Include(p => p.Product).Include(p => p.Contract).ToList();
            return View(pricing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Preferences(FormCollection collection)
        {
          
            if (!string.IsNullOrEmpty(collection["txtPassword"]))
            {
                string PasswordHash = WBHelper.CreatePasswordHash(collection["txtPassword"].ToString(), LoginUser.PasswordSalt);
                WBSSLStore.Domain.User objUser = LoginUser;
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

                ViewBag.Message = "<div class='alert-success'>" + WBSSLStore.Resources.GeneralMessage.Message.Pwdchanged + "</div>";
                return View(LoginUser);
            }
            else
            {
                ViewBag.Message = "<div class='alert-danger'>" + WBSSLStore.Resources.ErrorMessage.Message.PasswordRequired + "</div>";
                return View(LoginUser);
            }
        }
        //
        // GET: /Client/Account/Edit/5

        public ActionResult Edit()
        {

        
            _viewModel = _repository.Find(u => u.SiteID == Site.ID && u.ID == LoginUser.ID).EagerLoad(u => u.AuditDetails).EagerLoad(u => u.Address).FirstOrDefault();
            _viewModel.ConfirmPassword = _viewModel.PasswordHash;
            ViewBag.Country = CountryList.ToArray();
            return View(_viewModel);
        }

        //
        // POST: /Client/Account/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User user)
        {
            ViewBag.Country = CountryList.ToArray();
            ViewBag.Message = string.Empty;
            if (ModelState.IsValid)
            {
                if (_repository.Find(u => u.Email.ToLower().Equals(user.Email) && u.ID != user.ID && u.SiteID == user.SiteID).FirstOrDefault() == null)
                {
                    user.AuditDetails.DateModified = DateTimeWithZone.Now;
                    user.AuditDetails.ByUserID = user.ID;
                    user.AuditDetails.HttpHeaderDump = Request.Headers.ToString();
                    user.AuditDetails.IP = Request.UserHostAddress;
                    _service.SaveCustomer(user, WBHelper.CurrentLangID(), SiteCacher.SiteSMTPDetail().ID);
                    ViewBag.Message = "<div class='alert-success'>" + WBSSLStore.Resources.GeneralMessage.Message.SavedSuccessfully + "</div>";
                    //ViewBag.Message = WBSSLStore.Resources.ErrorMessage.Message.SavedSuccessfully;
                    return View(user);
                }
                else
                {
                    ViewBag.Message = "<div class='alert-danger'>" + WBSSLStore.Resources.GeneralMessage.Message.EmailExists + "</div>";
                    //ViewBag.Message = WBSSLStore.Resources.ErrorMessage.Message.EmailExists;
                    return View(user);
                }
            }
            else
                return View(user);
        }


        public ViewResult Statement(FormCollection collection, int? page)
        {
            ViewBag.UserName = LoginUser != null ? LoginUser.FirstName + " " + LoginUser.LastName : string.Empty; 
            if (!PagingInputValidator.IsPagingInputValid(ref page))
                return View();

            if (Request.Form["btnExport"] != null && Request.Form["btnExport"].ToString().ToLower().Equals("export to csv"))
            {
                ExportToCSV(collection);
                return View();
            }
            else
            {
                IPagedList<AccountStatement> lstAccountStatement = GetUserTransData(collection).ToPagedList(page.HasValue ? page.Value - 1 : 0, PageSize);

                if (!PagingInputValidator.IsPagingInputValid(ref page, lstAccountStatement))
                    return View();

                return View(lstAccountStatement);
            }
        }

        
        [HttpPost]
        public void ExportToCSV(FormCollection collection)
        {
            List<SelectListItem> lstheader = new List<SelectListItem>();
            lstheader.Add(new SelectListItem() { Text = "Order Date", Value = "TransactionDate" });
            lstheader.Add(new SelectListItem() { Text = "Last Transaction Details", Value = "TransactionDetail" });
            lstheader.Add(new SelectListItem() { Text = "Type", Value = "TransactionMode" });
            lstheader.Add(new SelectListItem() { Text = "Payment-Amount", Value = "PaymentAmount" });
            lstheader.Add(new SelectListItem() { Text = "Payment Mode", Value = "PaymentMode" });
            lstheader.Add(new SelectListItem() { Text = "Credit", Value = "CreditAmount" });

            WBHelper.GetCSVFile<AccountStatement>(GetUserTransData(collection), lstheader, "Statement");
        }

        private List<AccountStatement> GetUserTransData(FormCollection collection)
        {
            DateTime dtStart = collection["txtFromDate"] != null ? Convert.ToDateTime(collection["txtFromDate"]) : DateTimeWithZone.Now.AddYears(-1);
            DateTime dtEnd = collection["txtToDate"] != null ? Convert.ToDateTime(collection["txtToDate"]) : DateTimeWithZone.Now;

            ViewBag.FormDate = dtStart.ToShortDateString();
            ViewBag.ToDate = dtEnd.ToShortDateString();

        

            var _RepTransaction = DependencyResolver.Current.GetService<IRepository<UserTransaction>>();
            List<UserTransaction> lstTransaction = _RepTransaction.Find(ut => ut.UserID == LoginUser.ID && ut.SiteID == Site.ID
                                                                        && DbFunctions.DiffDays(dtStart, ut.AuditDetails.DateCreated) >= 0
                                                                        && DbFunctions.DiffDays(ut.AuditDetails.DateCreated, dtEnd) >= 0)
                                                                  .EagerLoad(ut => ut.Payment, ut => ut.OrderDetail, ut => ut.AuditDetails)
                                                                  .OrderByDescending(ut => ut.ID).ToList();
            return GetAccountStement(lstTransaction, LoginUser);
            
        }

        private List<AccountStatement> GetAccountStement(List<UserTransaction> lstTransaction, User LoginUser)
        {
            List<AccountStatement> lstAccountStatement = new List<AccountStatement>();
            if (lstTransaction != null && lstTransaction.Count > 0)
            {
                ViewBag.AccountBalance = lstTransaction.Sum(ut => ut.TransactionAmount);
                foreach (UserTransaction tran in lstTransaction)
                {


                    AccountStatement statement = new AccountStatement();
                    if (tran.TransactionMode == TransactionMode.ADDFUND)
                    {

                        statement.PaymentAmount = tran.TransactionAmount;
                        statement.OrderDetailID = 0;
                        statement.InvoiceID = 0;
                        statement.TransactionDate = tran.AuditDetails.DateCreated;
                        if (tran.PaymentID != null && tran.PaymentID > 0)
                        {
                            statement.TransactionDetail = "Fund Added By " + LoginUser.FirstName + " " + LoginUser.LastName;
                            statement.PaymentMode = tran.Payment.PaymentMode;
                        }
                        else
                        {
                            statement.TransactionDetail = "Fund Added By Admin" + "<br />" + tran.ReceipientInstrumentDetails;
                            statement.PaymentMode = null;
                        }
                        statement.TransactionMode = TransactionMode.ADDFUND;
                        statement.UserTransactionID = tran.ID;
                    }
                    else if (tran.TransactionMode == TransactionMode.REFUND)
                    {

                        statement.PaymentAmount = tran.TransactionAmount;
                        statement.OrderDetailID = 0;
                        statement.InvoiceID = 0;
                        statement.TransactionDate = tran.AuditDetails.DateCreated;
                        statement.TransactionDetail = "Credit given against cancelled order #" + InvoicePrefix + tran.OrderDetail.OrderID;
                        statement.TransactionMode = TransactionMode.REFUND;
                        statement.UserTransactionID = tran.ID;
                    }
                    else if (tran.TransactionMode == TransactionMode.ORDER)
                    {

                        if (lstAccountStatement.Where(ut => ut.OrderDetailID == tran.OrderDetailID).FirstOrDefault() == null)
                        {
                            statement.OrderDetailID = Convert.ToInt32(tran.OrderDetailID);
                            statement.InvoiceID = tran.OrderDetail.OrderID;
                            statement.TransactionDate = tran.AuditDetails.DateCreated;
                            statement.TransactionDetail = "Order Placed for #" + InvoicePrefix + tran.OrderDetail.OrderID;
                            statement.TransactionMode = TransactionMode.ORDER;
                            statement.UserTransactionID = tran.ID;
                            int ordCount = lstTransaction.Select(ut => ut.OrderDetailID == tran.OrderDetailID).Count();
                            UserTransaction ordCredit = lstTransaction.Where(ut => (ut.PaymentID ?? 0) <= 0 && ut.OrderDetailID == tran.OrderDetailID).FirstOrDefault();
                            UserTransaction ordPayment = lstTransaction.Where(ut => (ut.PaymentID ?? 0) > 0 && ut.OrderDetailID == tran.OrderDetailID).FirstOrDefault();
                            if (ordPayment != null)
                            {
                                statement.PaymentMode = ordPayment.Payment.PaymentMode;
                                statement.PaymentAmount = (ordCredit != null ? ordPayment.OrderDetail.Price + ordCredit.TransactionAmount : ordPayment.OrderDetail.Price);
                            }
                            if (ordCredit != null)
                                statement.CreditAmount = ordCredit.TransactionAmount * -1;
                        }
                    }
                    else if (tran.TransactionMode == TransactionMode.REISSUE)
                    {
                        statement.PaymentAmount = tran.TransactionAmount * -1;
                        statement.OrderDetailID = 0;
                        statement.InvoiceID = 0;
                        statement.TransactionDate = tran.AuditDetails.DateCreated;
                        statement.TransactionDetail = tran.ReceipientInstrumentDetails;
                        statement.TransactionMode = TransactionMode.REISSUE;
                        statement.UserTransactionID = tran.ID;
                    }
                    if (statement.UserTransactionID > 0)
                        lstAccountStatement.Add(statement);
                }
            }
            else
                ViewBag.AccountBalance = 0;
            return lstAccountStatement;
        }
    }
}