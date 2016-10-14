using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WBSSLStore.Domain;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Service;
using WBSSLStore.Web.Helpers.Authentication;
using WBSSLStore.Web.Helpers;
using WBSSLStore.Web.Helpers.Caching;
using WBSSLStore.Web.Util;


namespace WBSSLStore.Web.Areas.Client.Controllers
{
    [Authorize]
    [HandleError]
    public class SupportController : WBController<IRepository<SupportDetail>, IRepository<SupportRequest>, IRepository<OrderDetail>>
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

        //
        // GET: /Client/Support/

        public ViewResult Index()
        {

            ViewBag.Username = user != null ? user.FirstName + " " + user.LastName : string.Empty; 
            var supportrequests = _repository.Find(s => s.UserID == user.ID && s.SiteID == Site.ID).Include(s => s.AuditDetails).Include(s => s.OrderDetail);

            return View(supportrequests.ToList());
        }

        //
        // GET: /Client/Support/Details/5

        public ViewResult Details(int id)
        {
            ViewBag.Username = user != null ? user.FirstName + " " + user.LastName : string.Empty; 
            
            List<SupportDetail> supportrequest = _viewModel.Find(s => s.SupportRequest.UserID == user.ID && s.SupportRequest.ID == id && s.SupportRequest.SiteID == Site.ID).Include(s => s.SupportRequest).EagerLoad(x => x.SupportRequest.OrderDetail).ToList();            
            ViewBag.Comment = string.Join("", supportrequest.Select(x => new { com = !string.IsNullOrEmpty(x.Comments) ? "<div class='supcom'>" + x.Comments + "</div>" : "" }).Select(x => x.com).ToArray());
            

            return View(supportrequest[0].SupportRequest);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Details(SupportRequest model)
        {
            if (ModelState.IsValid)
            {

                Audit audit = new Audit();
                audit.ByUserID = user.ID;
                audit.DateCreated = System.DateTimeWithZone.Now;
                audit.DateModified = System.DateTimeWithZone.Now;
                audit.HttpHeaderDump = HttpUtility.UrlDecode(System.Web.HttpContext.Current.Request.Headers.ToString());
                audit.IP = System.Web.HttpContext.Current.Request.UserHostAddress;

                SupportDetail sd = new SupportDetail();
                sd.AuditDetails = audit;
                sd.SupportRequestID = model.ID;
                sd.StaffNote = "";
                //if (!string.IsNullOrEmpty(model.Comment)) { }
                sd.Comments = "<span><b>" + user.FirstName + " " + user.LastName + " ( " + DateTimeWithZone.Now + " )</b></span><p>" + model.Comment + "<p/>";

                //if (!model.isOpen)
                //{
                SupportRequest sr = _repository.Find(x => x.ID == model.ID && x.UserID == user.ID && x.SiteID == Site.ID).Include(c => c.AuditDetails).FirstOrDefault();

                sr.isOpen = model.isOpen;
                sr.RefundStatusID = model.RefundStatusID;
                sr.Reason = Request.Form["ddlReason"];
                sd.SupportRequest = sr;

                sr.AuditDetails.DateModified = DateTimeWithZone.Now;
                sr.AuditDetails.HttpHeaderDump = HttpUtility.UrlDecode(System.Web.HttpContext.Current.Request.Headers.ToString());
                sr.AuditDetails.IP = System.Web.HttpContext.Current.Request.UserHostAddress;
                sr.AuditDetails.ByUserID = user.ID;
                //}

                _viewModel.Add(sd);
                _unitOfWork.Commit();
            }

            return RedirectToAction("index");
        }
        //
        // GET: /Client/Support/Create

        public ActionResult CancellationRequest(int id)
        {
            OrderDetail ord = _service.FindByID(id);
            
            if (ord != null)
            {
                ViewBag.OrderDetailID = ord.ID;
                ViewBag.IncidentID = WBHelper.InvoicePrefix(Site) + ord.OrderID;
                ViewBag.ProductName = ord.ProductName;
                ViewBag.ExOrderID = ord.ExternalOrderID;
            }
            ViewBag.Username = user != null ? user.FirstName + " " + user.LastName : string.Empty; 
            SupportRequest _supportrequest = DependencyResolver.Current.GetService<SupportRequest>();
            return View(_supportrequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancellationRequest(SupportRequest supportrequest, FormCollection collection)
        {
            if (ModelState.IsValid && collection != null)
            {

                Audit audit = new Audit();
                audit.ByUserID = user.ID;
                audit.DateCreated = System.DateTimeWithZone.Now;
                audit.DateModified = System.DateTimeWithZone.Now;
                audit.HttpHeaderDump = HttpUtility.UrlDecode(System.Web.HttpContext.Current.Request.Headers.ToString());
                audit.IP = System.Web.HttpContext.Current.Request.UserHostAddress;

                SupportRequest sr = new SupportRequest();
                sr.isOpen = true;
                sr.OrderDetailID = Convert.ToInt32(collection["requestid"]);
                sr.RefundStatusID = Convert.ToInt16(collection["RefundStatusID"]);
                sr.SiteID = Site.ID;
                sr.SupportTypeID = (int)SupportType.REFUNDREQUEST;
                sr.UserID = user.ID;
                sr.ExternalIncidentID = string.Empty;
                sr.AuditDetails = audit;
                sr.Subject = @WBSSLStore.Resources.GeneralMessage.Message.RefundRequest_Caption + "-" + @WBSSLStore.Resources.GeneralMessage.Message.Invoice_Header + "#" + (collection["IncidentID"]);
                sr.User = user;
                sr.Reason = collection["ddlReason"];

                SupportDetail sd = new SupportDetail();
                sd.AuditDetails = audit;
                sd.SupportRequest = sr;
                sd.StaffNote = "";                
                sd.Comments = "<span><b>" + user.FirstName + " " + user.LastName + " ( " + DateTimeWithZone.Now + " )</b></span><p>" + collection["txtcomment"] + "<p/>";
                _viewModel.Add(sd);
                _unitOfWork.Commit();

                var _newsupportdetail = _viewModel.Find(d => d.ID == sd.ID)
                                                .EagerLoad(d => d.SupportRequest, d => d.SupportRequest.User, d => d.SupportRequest.OrderDetail)
                                                .FirstOrDefault();
                _newsupportdetail.SupportRequest.OrderDetail.InvoiceNumber = WBHelper.InvoicePrefix(Site) + _newsupportdetail.SupportRequest.OrderDetail.OrderID;

                var _emailService = DependencyResolver.Current.GetService<IEmailQueueService>();
                if (sd.SupportRequest.User.UserType == UserType.RESELLER)
                    _emailService.PrepareEmailQueue(Site.ID, LangID, EmailType.RESELLER_SUPPORT_NOTIFICATION, SMTPID, sd.SupportRequest.User.Email, _newsupportdetail);
                else
                    _emailService.PrepareEmailQueue(Site.ID, LangID, EmailType.CUSTOMER_SUPPORT_NOTIFICATION, SMTPID, sd.SupportRequest.User.Email, _newsupportdetail);
                //send mail to admin
                _emailService.PrepareEmailQueue(Site.ID, LangID, EmailType.ADMIN_SUPPORT_NOTIFICATION, SMTPID, WBHelper.SiteAdminEmail(Site), _newsupportdetail);
                _unitOfWork.Commit();
            }

            return RedirectToAction("index");
        }
    }
}