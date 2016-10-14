using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WBSSLStore.Domain;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Service;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Web.Helpers.PagedList;

using WBSSLStore.Web.Helpers.Authentication;
using WBSSLStore.Web.Helpers;
using WBSSLStore.Web.Helpers.Caching;
using WBSSLStore.Gateways.RestAPIModels.Services;
using WBSSLStore.Gateways.RestAPIModels.Request;
using WBSSLStore.Gateways.RestAPIModels.Response;

namespace WBSSLStore.Web.Areas.Admin.Controllers
{
    [CustomAuthorizeAttribute]
    [HandleError]
    public class SupportController : WBController<SupportRequest, IRepository<SupportRequest>, IRepository<SupportDetail>>
    {
        private User _user;
        private User user
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
        private int LangID
        {
            get
            {
                return WBHelper.CurrentLangID();
            }
        }
        private int SMTPID
        {
            get
            {
                return SiteCacher.SiteSMTPDetail().ID;
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
        private string SiteAdminEmail
        {
            get
            {
                return WBHelper.SiteAdminEmail(Site);
            }

        }
        //
        // GET: /Admin/Support/
        public ActionResult Index(FormCollection collection, int? page)
        {
            ViewBag.StartDate = collection["txtFromDate"] ?? Request.QueryString["txtFromDate"];
            ViewBag.EndDate = collection["txtToDate"] ?? Request.QueryString["txtToDate"];
            ViewBag.Status = collection["ddlStatus"] ?? Request.QueryString["ddlStatus"];
            ViewBag.Keyword = collection["txtKeyword"] ?? Request.QueryString["txtKeyword"];

            if (!PagingInputValidator.IsPagingInputValid(ref page))
                return View();

            DateTime dtStart = ViewBag.StartDate == null ? DateTimeWithZone.Now.AddYears(-1) : Convert.ToDateTime(ViewBag.StartDate);
            DateTime dtEnd = ViewBag.EndDate == null ? DateTimeWithZone.Now : Convert.ToDateTime(ViewBag.EndDate);

            bool? bIsOpen = null;
            if (ViewBag.Status == null && string.IsNullOrEmpty(Request.QueryString["txtFromDate"]))
                bIsOpen = true;
            else if (!string.IsNullOrEmpty(ViewBag.Status))
                bIsOpen = Convert.ToBoolean(ViewBag.Status);


            string strKeyword = ViewBag.Keyword;
            int intID = string.IsNullOrEmpty(strKeyword) ? 0 : WBHelper.ToInt(strKeyword.Replace(InvoicePrefix, string.Empty).Replace("SSL-", string.Empty), 999999999);

            UserType eUserType;

            ViewBag.StartDate = dtStart.ToShortDateString();
            ViewBag.EndDate = dtEnd.ToShortDateString();
            ViewBag.Status = bIsOpen;

            ViewBag.Keyword = strKeyword;

            if (!string.IsNullOrEmpty(Request.QueryString[SettingConstants.QS_USER_TYPE]) && Request.QueryString[SettingConstants.QS_USER_TYPE].ToUpper() == "ALL")
            {
                var supportrequests = _repository.Find(s => s.User.UserTypeID == s.User.UserTypeID && s.SiteID == Site.ID
                                                              && (System.Data.Entity.DbFunctions.DiffDays(dtStart, s.AuditDetails.DateCreated) >= 0
                                                              && System.Data.Entity.DbFunctions.DiffDays(s.AuditDetails.DateCreated, dtEnd) >= 0)
                                                              && s.isOpen == (bIsOpen == null ? s.isOpen : bIsOpen)
                                                              && (s.ID.Equals(intID > 0 ? intID : s.ID) || s.OrderDetail.OrderID.Equals(intID > 0 ? intID : s.OrderDetail.OrderID)
                                                                     || (s.OrderDetail.ExternalOrderID ?? "-").Equals(intID > 0 ? strKeyword : (s.OrderDetail.ExternalOrderID ?? "-"))))
                                                        .EagerLoad(s => s.AuditDetails, s => s.OrderDetail, s => s.OrderDetail.CertificateRequest)
                                                        .OrderByDescending(ord => ord.AuditDetails.DateModified).ThenByDescending(ord => ord.AuditDetails.DateCreated)
                                                        .ToPagedList(page.HasValue ? page.Value - 1 : 0, PageSize);

                if (!PagingInputValidator.IsPagingInputValid(ref page, supportrequests))
                    return View();

                return View(supportrequests);
            }
            else if (!string.IsNullOrEmpty(Request.QueryString[SettingConstants.QS_USER_TYPE]) && Enum.TryParse<UserType>(Request.QueryString[SettingConstants.QS_USER_TYPE].ToUpper(), out eUserType))
            {
                if (Enum.IsDefined(typeof(UserType), eUserType))
                { 
                    var supportrequests = _repository.Find(s => s.User.UserTypeID == (int?)eUserType && s.SiteID == Site.ID
                                                           && (System.Data.Entity.DbFunctions.DiffDays(dtStart, s.AuditDetails.DateCreated) >= 0
                                                           && System.Data.Entity.DbFunctions.DiffDays(s.AuditDetails.DateCreated, dtEnd) >= 0)
                                                           && s.isOpen == (bIsOpen == null ? s.isOpen : bIsOpen)
                                                           && (s.ID.Equals(intID > 0 ? intID : s.ID) || s.OrderDetail.OrderID.Equals(intID > 0 ? intID : s.OrderDetail.OrderID)
                                                                  || (s.OrderDetail.ExternalOrderID ?? "-").Equals(intID > 0 ? strKeyword : (s.OrderDetail.ExternalOrderID ?? "-"))))
                                                     .EagerLoad(s => s.AuditDetails, s => s.OrderDetail, s => s.OrderDetail.CertificateRequest)
                                                     .OrderByDescending(ord => ord.AuditDetails.DateModified).ThenByDescending(ord => ord.AuditDetails.DateCreated)
                                                     .ToPagedList(page.HasValue ? page.Value - 1 : 0, PageSize);

                    if (!PagingInputValidator.IsPagingInputValid(ref page, supportrequests))
                        return View();

                    return View(supportrequests);
                }
            }

            return View();
        }

        public ActionResult Detail(int ID)
        {
            List<SupportDetail> lstSupportDetail = _service.Find(sd => sd.SupportRequestID == ID && sd.SupportRequest.SiteID == Site.ID)
                                                                 .EagerLoad(sd => sd.SupportRequest, sd => sd.AuditDetails, sd => sd.SupportRequest.OrderDetail, sd => sd.SupportRequest.User)
                                                                 .ToList();

            foreach (SupportDetail x in lstSupportDetail)
            {
                ViewBag.Comment += x.Comments + "<br />";
                ViewBag.StaffNotes += x.StaffNote + "<br />";
            }

            SupportDetail supportdetail = lstSupportDetail.FirstOrDefault();

            supportdetail.Comments = string.Empty;
            ViewBag.Status = lstSupportDetail.FirstOrDefault().SupportRequest.isOpen;

            if (supportdetail.SupportRequest.RefundStatus == RefundStatus.NOACTION)
                supportdetail.SupportRequest.RefundAmount = 0;
            else
                supportdetail.SupportRequest.RefundAmount = supportdetail.SupportRequest.OrderDetail.Price;

            //Get Support ticket status from TheSSLStore
            OrderRequest request = new OrderRequest();
            request.AuthRequest = new AuthRequest();
            request.AuthRequest.PartnerCode = Site.APIPartnerCode;
            request.AuthRequest.AuthToken = Site.APIAuthToken;
            request.AuthRequest.UserAgent = Site.Alias;
            request.AuthRequest.ReplayToken = "SSL Store WhiteBrand Sites : " + Site.Alias;
            request.RefundReason = string.Empty;
            request.RefundRequestID = !string.IsNullOrEmpty( supportdetail.SupportRequest.ExternalIncidentID) ? supportdetail.SupportRequest.ExternalIncidentID : (supportdetail.SupportRequest.OrderDetail != null ? supportdetail.SupportRequest.OrderDetail.ExternalOrderID : string.Empty )  ;

            OrderResponse response = new OrderResponse();
            response = OrderService.RefundRequestStatus(request);
            ViewBag.TheSSLStoreStatus = false;
            if (response != null && response.AuthResponse != null && !response.AuthResponse.isError)
                ViewBag.TheSSLStoreStatus = response.isRefundApproved;

            return View(lstSupportDetail.FirstOrDefault());
        }

        [HttpPost]
        public ActionResult Detail(SupportDetail supportdetail)
        {
            decimal dAmount = Convert.ToDecimal(supportdetail.SupportRequest.RefundAmount);
            RefundStatus eRefundStatus = supportdetail.SupportRequest.RefundStatus;

            SupportRequest sr = _repository.Find(x => x.ID == supportdetail.SupportRequestID && x.SiteID == Site.ID)
                                      .EagerLoad(c => c.AuditDetails, c => c.OrderDetail, c => c.User)
                                      .FirstOrDefault();

            supportdetail.SupportRequest = sr;

            if (ModelState.IsValid)
            {
                if (eRefundStatus != RefundStatus.NOACTION && dAmount > sr.OrderDetail.Price)
                {
                    ViewBag.Message = WBSSLStore.Resources.GeneralMessage.Message.Admin_RefundUnsuccessful;
                    return View(supportdetail);
                }

                User adminUser = user;
                sr.isOpen = Convert.ToBoolean(Request.Form["ddlStatus"]);
                sr.AuditDetails.DateModified = DateTimeWithZone.Now;
                sr.AuditDetails.HttpHeaderDump = HttpUtility.UrlDecode(System.Web.HttpContext.Current.Request.Headers.ToString());
                sr.AuditDetails.IP = System.Web.HttpContext.Current.Request.UserHostAddress;
                sr.AuditDetails.ByUserID = adminUser.ID;

                supportdetail.AuditDetails = new Audit();
                supportdetail.AuditDetails.ByUserID = adminUser.ID;
                supportdetail.AuditDetails.DateCreated = DateTimeWithZone.Now;
                supportdetail.AuditDetails.HttpHeaderDump = Request.Headers.ToString();
                supportdetail.AuditDetails.IP = Request.UserHostAddress;

                supportdetail.Comments = "<em><b>" + adminUser.FirstName + " " + adminUser.LastName + " ( " + DateTimeWithZone.Now + " )</b></em><br/>" + supportdetail.Comments;
                supportdetail.StaffNote = "<em><b>" + adminUser.FirstName + " " + adminUser.LastName + " ( " + DateTimeWithZone.Now + " )</b></em><br/>" + supportdetail.StaffNote;


                var _emailservice = DependencyResolver.Current.GetService<IEmailQueueService>();

                switch (eRefundStatus)
                {
                    case RefundStatus.NOACTION:
                        supportdetail.SupportRequest.RefundStatusID = (int)RefundStatus.NOACTION;
                        _service.Add(supportdetail);
                        _unitOfWork.Commit();

                        //send mai to admin
                        supportdetail.SupportRequest.OrderDetail.InvoiceNumber = InvoicePrefix + supportdetail.SupportRequest.OrderDetail.OrderID;
                        _emailservice.PrepareEmailQueue(Site.ID, LangID, EmailType.ADMIN_REFUND_NOTIFICATION, SMTPID, supportdetail.SupportRequest.User.Email, supportdetail);
                        _unitOfWork.Commit();

                        break;
                    case RefundStatus.CANCEL_ORDER_AND_REFUND:
                        supportdetail.SupportRequest.RefundStatusID = (int)RefundStatus.CANCEL_ORDER_AND_REFUND;
                        supportdetail.SupportRequest.RefundAmount = dAmount;

                        UserTransaction usertransactionRefund = new UserTransaction();
                        usertransactionRefund.Comment = "Refund given against order and " + (string.IsNullOrEmpty(supportdetail.SupportRequest.OrderDetail.ExternalOrderID) ? "InvoiceID #" + InvoicePrefix + supportdetail.SupportRequest.OrderDetail.OrderID : "order number #" + supportdetail.SupportRequest.OrderDetail.ExternalOrderID);
                        usertransactionRefund.ReceipientInstrumentDetails = "Refund given against order and " + (string.IsNullOrEmpty(supportdetail.SupportRequest.OrderDetail.ExternalOrderID) ? "InvoiceID #" + InvoicePrefix + supportdetail.SupportRequest.OrderDetail.OrderID : "order number #" + supportdetail.SupportRequest.OrderDetail.ExternalOrderID);
                        usertransactionRefund.TransactionAmount = 0;

                        InsertUpdateDataAndSendMail(usertransactionRefund, supportdetail, _emailservice);
                        break;
                    case RefundStatus.CANCEL_ORDER_AND_STORE_CREDIT:
                        supportdetail.SupportRequest.RefundStatusID = (int)RefundStatus.CANCEL_ORDER_AND_STORE_CREDIT;
                        supportdetail.SupportRequest.RefundAmount = dAmount;

                        UserTransaction usertransaction = new UserTransaction();
                        usertransaction.Comment = "Credit given against order and " + (string.IsNullOrEmpty(supportdetail.SupportRequest.OrderDetail.ExternalOrderID) ? "InvoiceID #" + InvoicePrefix + supportdetail.SupportRequest.OrderDetail.OrderID : "order number #" + supportdetail.SupportRequest.OrderDetail.ExternalOrderID);
                        usertransaction.ReceipientInstrumentDetails = "Credit given against order and " + (string.IsNullOrEmpty(supportdetail.SupportRequest.OrderDetail.ExternalOrderID) ? "InvoiceID #" + InvoicePrefix + supportdetail.SupportRequest.OrderDetail.OrderID : "order number #" + supportdetail.SupportRequest.OrderDetail.ExternalOrderID);
                        usertransaction.TransactionAmount = dAmount;

                        InsertUpdateDataAndSendMail(usertransaction, supportdetail, _emailservice);
                        break;
                }

                List<SupportDetail> lstSupportDetail = _service.Find(sd => sd.SupportRequestID == supportdetail.SupportRequestID && sd.SupportRequest.SiteID == Site.ID).ToList();

                foreach (SupportDetail x in lstSupportDetail)
                {
                    ViewBag.Comment += x.Comments + "<br />";
                    ViewBag.StaffNotes += x.StaffNote + "<br />";
                }

                ViewBag.Status = supportdetail.SupportRequest.isOpen;
                ViewBag.Message = WBSSLStore.Resources.GeneralMessage.Message.OrderStatusSuccess;
            }
            return View(supportdetail);
        }

        private void InsertUpdateDataAndSendMail(UserTransaction usertransaction, SupportDetail supportdetail, IEmailQueueService _emailservice)
        {
            supportdetail.SupportRequest.OrderDetail.OrderStatusID = (int)OrderStatus.REFUNDED;

            var _usertransaction = DependencyResolver.Current.GetService<IRepository<UserTransaction>>();

            usertransaction.AuditDetails = new Audit();
            usertransaction.AuditDetails = supportdetail.AuditDetails;
            usertransaction.OrderDetail = supportdetail.SupportRequest.OrderDetail;
            usertransaction.OrderDetailID = supportdetail.SupportRequest.OrderDetail.ID;
            usertransaction.RefundRequest = supportdetail.SupportRequest;
            usertransaction.RefundRequestID = supportdetail.SupportRequest.ID;
            usertransaction.SiteID = Site.ID;
            usertransaction.TransactionModeID = (int)TransactionMode.REFUND;
            usertransaction.UserID = supportdetail.SupportRequest.UserID;

            _usertransaction.Add(usertransaction);
            _service.Add(supportdetail);
            _unitOfWork.Commit();


            usertransaction.OrderDetail.InvoiceNumber = InvoicePrefix + usertransaction.OrderDetail.OrderID;

            if (usertransaction.User.UserType == UserType.RESELLER)
                _emailservice.PrepareEmailQueue(Site.ID, LangID, EmailType.RESELLER_REFUND_NOTIFICATION, SMTPID, usertransaction.User.Email, usertransaction);
            else
                _emailservice.PrepareEmailQueue(Site.ID, LangID, EmailType.CUSTOMER_REFUND_NOTIFICATION, SMTPID, usertransaction.User.Email, usertransaction);

            _emailservice.PrepareEmailQueue(Site.ID, LangID, EmailType.ADMIN_REFUND_NOTIFICATION, SMTPID, SiteAdminEmail, supportdetail);
            _unitOfWork.Commit();
        }

        public ActionResult CancellationRequest(int id)
        {
            var _orderdetail = DependencyResolver.Current.GetService<IRepository<OrderDetail>>();
            OrderDetail orderdetail = _orderdetail.Find(ord => ord.ID == id && ord.Order.SiteID == Site.ID).EagerLoad(ord => ord.Order, ord => ord.Order.User).FirstOrDefault();
            return View(orderdetail);
        }

        [HttpPost]
        public ActionResult CancellationRequest(OrderDetail orderdetail)
        {
            if (ModelState.IsValidField("ID") && ModelState.IsValidField("OrderID"))
            {

                Audit audit = new Audit();
                audit.ByUserID = user.ID;
                audit.DateCreated = System.DateTimeWithZone.Now;
                audit.DateModified = System.DateTimeWithZone.Now;
                audit.HttpHeaderDump = HttpUtility.UrlDecode(System.Web.HttpContext.Current.Request.Headers.ToString());
                audit.IP = System.Web.HttpContext.Current.Request.UserHostAddress;

                SupportRequest supportrequest = new SupportRequest();
                supportrequest.isOpen = true;
                supportrequest.OrderDetailID = orderdetail.ID;
                supportrequest.RefundStatusID = Convert.ToInt16(Request.Form["ddlRefundStatus"]);
                supportrequest.Reason = Request.Form["ddlReason"];
                supportrequest.SiteID = Site.ID;
                supportrequest.SupportTypeID = (int)SupportType.REFUNDREQUEST;
                supportrequest.UserID = orderdetail.Order.UserID;
                supportrequest.ExternalIncidentID = string.Empty;
                supportrequest.AuditDetails = audit;
                supportrequest.Subject = @WBSSLStore.Resources.GeneralMessage.Message.RefundRequest_Caption + "-" + @WBSSLStore.Resources.GeneralMessage.Message.Invoice_Header + "#" + (InvoicePrefix + orderdetail.OrderID);

                SupportDetail sd = new SupportDetail();
                sd.AuditDetails = audit;
                sd.SupportRequest = supportrequest;
                sd.StaffNote = "";
                sd.Comments = "<em><b>Admin ( " + DateTimeWithZone.Now + " )</b></em><br/>" + Request.Form["txtcomment"];

                _service.Add(sd);
                _unitOfWork.Commit();

                var _newsupportdetail = _service.Find(d => d.ID == sd.ID)
                                                .EagerLoad(d => d.SupportRequest, d => d.SupportRequest.User, d => d.SupportRequest.OrderDetail)
                                                .FirstOrDefault();
                _newsupportdetail.SupportRequest.OrderDetail.InvoiceNumber = InvoicePrefix + _newsupportdetail.SupportRequest.OrderDetail.OrderID;
                var _emailservice = DependencyResolver.Current.GetService<IEmailQueueService>();
                //Send mail to client
                if (orderdetail.Order.User.UserType == UserType.RESELLER)
                    _emailservice.PrepareEmailQueue(Site.ID, LangID, EmailType.RESELLER_SUPPORT_NOTIFICATION, SMTPID, _newsupportdetail.SupportRequest.User.Email, _newsupportdetail);
                else
                    _emailservice.PrepareEmailQueue(Site.ID, LangID, EmailType.CUSTOMER_SUPPORT_NOTIFICATION, SMTPID, _newsupportdetail.SupportRequest.User.Email, _newsupportdetail);
                //send mail to admin
                _emailservice.PrepareEmailQueue(Site.ID, LangID, EmailType.ADMIN_SUPPORT_NOTIFICATION, SMTPID, SiteAdminEmail, _newsupportdetail);
                _unitOfWork.Commit();

                return RedirectToAction("index", new { utype = ((UserType)orderdetail.Order.User.UserTypeID).ToString() });
            }
            else
                return View(orderdetail);
        }

        [HttpPost]
        public ActionResult sendrequest(int id)
        {
            string strMsg = string.Empty;
            var support = _repository.Find(s => s.ID == id && s.SiteID == Site.ID).EagerLoad(s => s.OrderDetail, s => s.OrderDetail.StoreAPIInteraction).FirstOrDefault();
            if (support != null && support.OrderDetail.StoreAPIInteraction != null)
            {
                OrderRequest request = new OrderRequest();
                request.AuthRequest = new AuthRequest();
                request.AuthRequest.PartnerCode = Site.APIPartnerCode;
                request.AuthRequest.AuthToken = Site.APIAuthToken;
                request.AuthRequest.UserAgent = Site.Alias;
                request.AuthRequest.ReplayToken = "SSL Store WhiteBrand Sites : " + Site.Alias;

                request.RefundReason = support.Subject + "<br/>" + support.Reason;
                request.TheSSLStoreOrderID = support.OrderDetail.ExternalOrderID;

                OrderResponse response = new OrderResponse();
                response = OrderService.RefundRequest(request);
                if (response != null && response.AuthResponse != null && !response.AuthResponse.isError)
                {
                    support.ExternalIncidentID = response.RefundRequestID;
                    _unitOfWork.Commit();
                }
                else if (response != null && response.AuthResponse != null && response.AuthResponse.isError)
                    strMsg = response.AuthResponse.Message[0];
            }
            return Json(strMsg);
        }
    }
}
