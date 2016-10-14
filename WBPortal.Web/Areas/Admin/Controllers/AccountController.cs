using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WBSSLStore.Domain;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Service;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Web.Helpers.Authentication;
using WBSSLStore.Web.Helpers.PagedList;
using WBSSLStore.Web.Util;
using WBSSLStore.Service.ViewModels;
using System.Web.Security;
//using System.Data.Objects;
using WBSSLStore.Web.Helpers;
using WBSSLStore.Web.Helpers.Caching;
using System.Globalization;
using System.Threading;
//using System.Data.Entity.Core.Objects;


namespace WBSSLStore.Web.Areas.Admin.Controllers
{
    [CustomAuthorizeAttribute]
    [HandleError]
    public class AccountController : WBController<UserTransaction, IRepository<UserTransaction>, IEmailQueueService>
    {
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
        private string SiteAdminEmail
        {
            get
            {
                return WBHelper.SiteAdminEmail(Site);
            }

        }
        // GET: /Admin/Account/
        public ActionResult Statement(FormCollection collection, int ID, int? page)
        {
            if (!PagingInputValidator.IsPagingInputValid(ref page))
                return View();

            if (Request.Form["btnExport"] != null && Request.Form["btnExport"].ToString().ToLower().Equals("export to csv"))
            {
                ExportToCSV(collection, ID);
                return View();
            }
            else
            {
                IPagedList<AccountStatement> lstAccountStatement = GetUserTransData(collection, ID).ToPagedList(page.HasValue ? page.Value - 1 : 0, PageSize);

                if (!PagingInputValidator.IsPagingInputValid(ref page, lstAccountStatement))
                    return View();

                return View(lstAccountStatement);
            }
        }

        [HttpPost]
        public void ExportToCSV(FormCollection collection, int ID)
        {
            List<SelectListItem> lstheader = new List<SelectListItem>();
            lstheader.Add(new SelectListItem() { Text = "Order Date", Value = "TransactionDate" });
            lstheader.Add(new SelectListItem() { Text = "Last Transaction Details", Value = "TransactionDetail" });
            lstheader.Add(new SelectListItem() { Text = "Type", Value = "TransactionMode" });
            lstheader.Add(new SelectListItem() { Text = "Payment-Amount", Value = "PaymentAmount" });
            lstheader.Add(new SelectListItem() { Text = "Payment Mode", Value = "PaymentMode" });
            lstheader.Add(new SelectListItem() { Text = "Credit", Value = "CreditAmount" });

            WBHelper.GetCSVFile<AccountStatement>(GetUserTransData(collection, ID), lstheader, "Statement");
        }



        public static DateTime ConvertDate(string date)
        {

            DateTime retval = DateTime.MinValue;
            CultureInfo ci = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
            Convert.ToDateTime(date, ci.DateTimeFormat).ToString("d");//

            if (System.DateTime.TryParseExact(date, Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out retval))
                return retval;

            // Could not convert date..
            return DateTime.MinValue;
        }
        private List<AccountStatement> GetUserTransData(FormCollection collection, int? UserID)
        {

            //DateTime FormDate = ConvertDate(collection["txtFromDate"] ?? Request.QueryString["txtFromDate"]);
            //DateTime ToDate = ConvertDate(collection["txtToDate"] ?? Request.QueryString["txtToDate"]);

            ////string a = ViewBag.FormDate.ToString();            
            ////DateTime dv =  Convert.ToDateTime(ViewBag.FormDate.ToString());
            ////a = dv.ToString("MM/dd/yyyy");
            ////dv = Convert.ToDateTime(a);
            ////string a = string.Format("{0:MM/dd/yyyy}",)
            ////DateTime dv = DateTime.ParseExact(collection["txtFromDate"].ToString(), "dd/MM/yyyy", null);

            ////DateTime dtStart = ViewBag.FormDate != null ? Convert.ToDateTime(ViewBag.FormDate) : DateTimeWithZone.Now.AddYears(-1);
            ////DateTime dtEnd = ViewBag.ToDate != null ? Convert.ToDateTime(ViewBag.ToDate) : DateTimeWithZone.Now;

            //DateTime dtStart = Convert.ToDateTime(FormDate.ToString("MM/dd/yyyy"));// collection["txtFromDate"] != null ? DateTime.ParseExact(collection["txtFromDate"], "d", System.Globalization.CultureInfo.InvariantCulture) : (!string.IsNullOrEmpty(Request.QueryString["txtFromDate"]) ? DateTime.ParseExact(collection["txtFromDate"], "d", System.Globalization.CultureInfo.InvariantCulture) : DateTimeWithZone.Now.AddYears(-1));
            //DateTime dtEnd = Convert.ToDateTime(ToDate.ToString("MM/dd/yyyy"));//  collection["txtToDate"] != null ? DateTime.ParseExact(collection["txtToDate"], "d", System.Globalization.CultureInfo.InvariantCulture) : (!string.IsNullOrEmpty(Request.QueryString["txtToDate"]) ? DateTime.ParseExact(collection["txtToDate"], "d", System.Globalization.CultureInfo.InvariantCulture) : DateTimeWithZone.Now);


            ViewBag.FormDate = collection["txtFromDate"] ?? Request.QueryString["txtFromDate"];
            ViewBag.ToDate = collection["txtToDate"] ?? Request.QueryString["txtToDate"];

            DateTime dtStart = ViewBag.FormDate != null ? Convert.ToDateTime(ViewBag.FormDate) : DateTimeWithZone.Now.AddYears(-1);
            DateTime dtEnd = ViewBag.ToDate != null ? Convert.ToDateTime(ViewBag.ToDate) : DateTimeWithZone.Now;


            ViewBag.FromDate = dtStart.ToShortDateString();
            ViewBag.ToDate = dtEnd.ToShortDateString();

            var _user = DependencyResolver.Current.GetService<IRepository<User>>();
            User objUser = _user.Find(u => u.ID == UserID && u.SiteID == Site.ID).FirstOrDefault();

            var _RepTransaction = DependencyResolver.Current.GetService<IRepository<UserTransaction>>();

            //var usertrans = _RepTransaction.Find(ut => ut.UserID == UserID && ut.SiteID == Site.ID
            //                                                            && EntityFunctions.DiffDays(dtStart, ut.AuditDetails.DateCreated) >= 0
            //                                                            && EntityFunctions.DiffDays(ut.AuditDetails.DateCreated, dtEnd) >= 0)
            //                                                      .EagerLoad(ut => ut.Payment, ut => ut.OrderDetail, ut => ut.AuditDetails)
            //                                                      .OrderByDescending(ut => ut.ID);

            List<UserTransaction> lstTransaction = _RepTransaction.Find(ut => ut.UserID == UserID && ut.SiteID == Site.ID
                                                                        && System.Data.Entity.DbFunctions.DiffDays(dtStart, ut.AuditDetails.DateCreated) >= 0
                                                                        && System.Data.Entity.DbFunctions.DiffDays(ut.AuditDetails.DateCreated, dtEnd) >= 0)
                                                                  .EagerLoad(ut => ut.Payment, ut => ut.OrderDetail, ut => ut.AuditDetails)
                                                                  .OrderByDescending(ut => ut.ID).ToList();
            var data = _RepTransaction.Find(ut => ut.UserID == UserID && ut.SiteID == Site.ID).ToList();

            decimal AccountBalance = data != null && data.Count > 0 ? Convert.ToDecimal(data.Sum(ut => ut.TransactionAmount)) : Convert.ToDecimal(0.00);

            return GetAccountStement(lstTransaction, objUser, AccountBalance);
        }

        private List<AccountStatement> GetAccountStement(List<UserTransaction> lstTransaction, User LoginUser, decimal AccountBalance)
        {
            List<AccountStatement> lstAccountStatement = new List<AccountStatement>();
            if (lstTransaction != null && lstTransaction.Count > 0)
            {
                ViewBag.AccountBalance = AccountBalance;
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

        public ActionResult AddFund(int ID)
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddFund(FormCollection collection, int ID)
        {
            User currentuser = null;
            if (User.Identity.IsAuthenticated)
            {
                SSLStoreUser loginuser = (SSLStoreUser)Membership.GetUser();
                if (loginuser != null && loginuser.Details != null)
                    currentuser = loginuser.Details;

            }


            var _user = DependencyResolver.Current.GetService<IRepository<User>>();
            User objUser = _user.Find(u => u.ID == ID && u.SiteID == Site.ID).FirstOrDefault();
            if (Convert.ToDecimal(collection["txtAmount"]) != 0)
            {
                if (objUser != null)
                {
                    Audit audit = new Audit();
                    audit.ByUserID = currentuser.ID;
                    audit.DateCreated = DateTimeWithZone.Now;
                    audit.HttpHeaderDump = HttpUtility.UrlDecode(Request.Headers.ToString());
                    audit.IP = Request.UserHostAddress;

                    UserTransaction usertransaction = new UserTransaction();
                    usertransaction.AuditDetails = audit;
                    usertransaction.TransactionModeID = (int)TransactionMode.ADDFUND;
                    usertransaction.ReceipientInstrumentDetails = collection["txtReceiptDetails"];
                    usertransaction.Comment = collection["txtRemark"];
                    usertransaction.TransactionAmount = Convert.ToDecimal(collection["txtAmount"]);

                    usertransaction.UserID = ID;
                    usertransaction.SiteID = Site.ID;

                    _repository.Add(usertransaction);
                    _unitOfWork.Commit();

                    var _newusertransaction = _repository.Find(ut => ut.ID == usertransaction.ID)
                                                         .EagerLoad(ut => ut.User)
                                                         .FirstOrDefault();
                    _service.PrepareEmailQueue(Site.ID, WBHelper.CurrentLangID(), EmailType.ADMIN_ADD_FUND_NOTIFICATION, SiteCacher.SiteSMTPDetail().ID, SiteAdminEmail, _newusertransaction);
                    _unitOfWork.Commit();

                    return RedirectToAction("edit", objUser.UserType.ToString().ToLower(), new { id = ID });
                }
            }
            else
            {
                ViewBag.AmountMsg = WBSSLStore.Resources.ErrorMessage.Message.AmtGreaterthanzero;
            }
            return View();
        }
    }
}
