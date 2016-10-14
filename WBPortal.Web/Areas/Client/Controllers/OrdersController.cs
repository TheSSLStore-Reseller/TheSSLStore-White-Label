using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WBSSLStore.Domain;
using WBSSLStore.Service;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Web.Helpers.Authentication;
using WBSSLStore.Service.ViewModels;
using WBSSLStore.Web.Helpers;

using WBSSLStore.Web.Helpers.PagedList;
using System.Web;


namespace WBSSLStore.Web.Areas.Client.Controllers
{


    [Authorize]
    [HandleError]
    public class OrdersController : WBController<List<OrderDetail>, IRepository<OrderDetail>, ISiteService>
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
        private bool USESSL
        {
            get
            {
                return WBHelper.USESSL(Site);
            }

        }
        //
        // GET: /Client/IncompleteOrders/

        public ActionResult dashboard()
        {
            ViewBag.UserName = user.FirstName + " " + user.LastName;
            var orderdetails = GetAccountSummaryDetail();
            return View(orderdetails);
        }

        public ActionResult Index()
        {

            ViewBag.UserName = user.FirstName + " " + user.LastName;
            var orderdetails = GetAccountSummaryDetail();
            return View(orderdetails);
        }
       

        //
        //Get ordersList
        public ActionResult List(int? page, FormCollection collection)
        {
            ViewBag.UserName = user.FirstName + " " + user.LastName;
            if (!PagingInputValidator.IsPagingInputValid(ref page))
                return View();

            
            IPagedList<WBSSLStore.Domain.OrderDetail> orderdetails = getSearchModel(collection).ToPagedList(page.HasValue ? page.Value - 1 : 0, PageSize);
            orderdetails.ActionName = "list";
            orderdetails.ControllerName = "Orders";

            if (!PagingInputValidator.IsPagingInputValid(ref page, orderdetails))
                return View();

            return View(orderdetails);
        }

        [HttpPost]
        public ActionResult List(FormCollection collection, int? page)
        {
            ViewBag.UserName = user.FirstName + " " + user.LastName;
            if (!PagingInputValidator.IsPagingInputValid(ref page))
                return View();

            if (Request.Form["btnExport"] != null && Request.Form["btnExport"].ToString().ToLower().Equals("export to csv"))
            {
                ExportDataToCSV(collection);
                return View();
            }
            else
            {


                IPagedList<WBSSLStore.Domain.OrderDetail> orderdetails = getSearchModel(collection).ToPagedList(page.HasValue ? page.Value - 1 : 0, PageSize);
                if (!PagingInputValidator.IsPagingInputValid(ref page, orderdetails))
                    return View();

                orderdetails.ActionName = "list";
                orderdetails.ControllerName = "Orders";
                return View("list", orderdetails);
            }
        }

        [HttpGet]
        public ActionResult SearchOrders(int? page)
        {
            ViewBag.UserName = user.FirstName + " " + user.LastName;
            if (string.IsNullOrEmpty(Request.QueryString["lstStatus"]))
                return View();
            return SearchOrders(new FormCollection(), page);
        }

        [HttpPost]
        public ActionResult SearchOrders(FormCollection collection, int? page)
        {
            ViewBag.UserName = user.FirstName + " " + user.LastName;
            if (!PagingInputValidator.IsPagingInputValid(ref page))
                return View();
            if (Request.Form["btnExport"] != null && Request.Form["btnExport"].ToString().ToLower().Equals("export to csv"))
            {
                ExportDataToCSV(collection);
                return View();
            }
            else
            {
                IPagedList<WBSSLStore.Domain.OrderDetail> orderdetails = getSearchModel(collection).ToPagedList(page.HasValue ? page.Value - 1 : 0, PageSize);

                if (!PagingInputValidator.IsPagingInputValid(ref page, orderdetails))
                    return View();

                orderdetails.ActionName = "searchorders";
                orderdetails.ControllerName = "Orders";
                return View(orderdetails);
            }
        }
        //
        // GET: /Client/Orders/Details/5

        public ViewResult Details(int id)
        {
            ViewBag.ProductPricingID = 0;

            ViewBag.UserName = user.FirstName + " " + user.LastName;

            var _supportrequest = DependencyResolver.Current.GetService<IRepository<SupportRequest>>();
            SupportRequest objSupportReq = _supportrequest.Find(y => y.UserID == user.ID && y.OrderDetailID == id).FirstOrDefault();
            if (objSupportReq != null)
            {
                if (objSupportReq.OrderDetailID == id)
                    ViewBag.CancelationRequest = "Cancel";
            }

            OrderDetail orderdetail = _repository.Find(x => x.ID == id && x.Order.UserID == user.ID).Include(o => o.Order).Include(o => o.Product).Include(o => o.CertificateRequest).Include(o => o.AuditDetails).Include(o => o.StoreAPIInteraction).Include(o => o.CertificateRequest.AdminContact).Include(o => o.CertificateRequest.TechnicalContact).Include(o => o.Product).FirstOrDefault();
            if (orderdetail != null && !string.IsNullOrEmpty(orderdetail.ExternalOrderID) && orderdetail.OrderStatus != OrderStatus.REJECTED && orderdetail.OrderStatus != OrderStatus.REFUNDED)
            {

                if (orderdetail.OrderStatus == OrderStatus.ACTIVE && (orderdetail.CertificateExpiresOn.Value - DateTimeWithZone.Now.Date).Days <= 30)
                {
                    var _PricingRepo = DependencyResolver.Current.GetService<IRepository<ProductPricing>>();
                    int ContractID = WBHelper.GetCurrentContractID(orderdetail.Order.UserID, Site.ID);
                    ProductPricing objPrice = _PricingRepo.Find(pp => pp.ProductID == orderdetail.Product.ID && pp.NumberOfMonths == orderdetail.NumberOfMonths && pp.ContractID == ContractID).FirstOrDefault();
                    if (objPrice != null)
                    {
                        ViewBag.ProductPricingID = objPrice.ID;
                    }
                }


                GetOrderstatusThruRESTAPI(orderdetail, Site);

            }


            return View(orderdetail);
        }

        public ActionResult Downloadauthfile(string flnm, string flcon)
        {
            if (!string.IsNullOrEmpty(flnm) && !string.IsNullOrEmpty(flcon))
                DownloadauthfileContent(flnm, flcon);

            return new EmptyResult();
        }

        public void DownloadauthfileContent(string filename, string filecontent)
        {

            Response.Clear();
            Response.BufferOutput = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "application/text";
            Response.AppendHeader("content-disposition", "filename=" + filename);
            //filecontent = filecontent.Replace("+", Environment.NewLine);
            //filecontent = filecontent.Replace("|", Environment.NewLine);
            byte[] bytfilecontent = System.Text.ASCIIEncoding.ASCII.GetBytes(filecontent);
            Response.OutputStream.Write(bytfilecontent, 0, bytfilecontent.Count());
        }
        public ActionResult ReIssue(int id)
        {


            var _Orderdetail = _repository.Find(od => od.ID == id).EagerLoad(od => od.CertificateRequest, od => od.Order, od => od.Product).FirstOrDefault();
            return View("entercsr", _Orderdetail);


        }
        //
        //Download Certificate
        public ActionResult DownloadCertificate(int id)
        {

            GetDonwloadCertificateRESTAPI(Site, id);


            return Json("true");
        }

        public void GetDonwloadCertificateRESTAPI(Site s, int id)
        {
            //WBSSLStore.Gateways.RestAPIModels.Request.OrderRequest request = new WBSSLStore.Gateways.RestAPIModels.Request.OrderRequest();
            //request.AuthRequest = new WBSSLStore.Gateways.RestAPIModels.Request.AuthRequest();
            //request.AuthRequest.PartnerCode = s.APIPartnerCode;
            //request.AuthRequest.AuthToken = s.APIAuthToken;
            //request.AuthRequest.UserAgent = s.Alias;
            //request.AuthRequest.ReplayToken = "SSL Store WhiteBrand Sites : " + Site.Alias;
            //request.RefundReason = string.Empty;
            //request.TheSSLStoreOrderID = Convert.ToString(id);
            //request.CustomOrderID = string.Empty;
            //WBSSLStore.Gateways.RestAPIModels.Response.DownloadCertificateResponse objResponse = new WBSSLStore.Gateways.RestAPIModels.Response.DownloadCertificateResponse();
            //objResponse = WBSSLStore.Gateways.RestAPIModels.Services.OrderService.DownloadCertificateRequest(request);
            //if (objResponse != null && objResponse.Certificates != null && objResponse.Certificates.Count() > 0)
            //{
            //    Response.Clear();
            //    Response.BufferOutput = false;
            //    Response.ContentType = "application/zip";
            //    Response.AddHeader("content-disposition", "filename=" + id + ".zip");
            //    using (ZipFile zip = new ZipFile())
            //    {
            //        foreach (WBSSLStore.Gateways.RestAPIModels.Response.Certificate cf in objResponse.Certificates)
            //        {
            //            zip.AddEntry(cf.FileName, cf.FileContent);
            //        }
            //        zip.Save(Response.OutputStream);
            //        Response.End();
            //    }
            //}

            //////////////
            WBSSLStore.Gateways.RestAPIModels.Request.OrderRequest request = new WBSSLStore.Gateways.RestAPIModels.Request.OrderRequest();
            request.AuthRequest = new WBSSLStore.Gateways.RestAPIModels.Request.AuthRequest();
            request.AuthRequest.PartnerCode = s.APIPartnerCode;
            request.AuthRequest.AuthToken = s.APIAuthToken;
            request.AuthRequest.UserAgent = Site.Alias;
            request.AuthRequest.ReplayToken = "SSL Store WhiteBrand Sites : " + Site.Alias;

            request.RefundReason = string.Empty;
            request.TheSSLStoreOrderID = Convert.ToString(id);
            request.CustomOrderID = string.Empty;

            //WBSSLStore.Gateways.RestAPIModels.Response.DownloadCertificateResponse objResponse = new WBSSLStore.Gateways.RestAPIModels.Response.DownloadCertificateResponse();
            //objResponse = WBSSLStore.Gateways.RestAPIModels.Services.OrderService.DownloadCertificateRequest(request);


            //Start: Coded by veeral
            WBSSLStore.Gateways.RestAPIModels.Response.DownloadCertificateZipResponse objResponse = new WBSSLStore.Gateways.RestAPIModels.Response.DownloadCertificateZipResponse();
            objResponse = WBSSLStore.Gateways.RestAPIModels.Services.OrderService.DownloadCertificateZipResponse(request);
            if (objResponse != null && objResponse.Zip != null)
            {
                Response.Clear();
                Response.BufferOutput = false;
                Response.ClearContent();
                Response.ClearHeaders();
                Response.ContentType = "application/zip";
                Response.AppendHeader("content-disposition", "filename=" + id + ".zip");
                byte[] ZipContents = Convert.FromBase64String(objResponse.Zip.Replace(' ', '+'));
                Response.OutputStream.Write(ZipContents, 0, ZipContents.Count());
            }

        }
        // Ended By <<MEHUL>>
        //
        //Re-Send Approval Email Certificate
        public ActionResult ReSendApprovalEmail(int id, string Email)
        {

            string ApproverEmail = Convert.ToString(Request.QueryString[SettingConstants.QS_EMAIL]);

            string resjson = string.Empty;
            WBSSLStore.Gateways.RestAPIModels.Request.OrderRequest request = new WBSSLStore.Gateways.RestAPIModels.Request.OrderRequest();
            request.AuthRequest = new WBSSLStore.Gateways.RestAPIModels.Request.AuthRequest();
            request.AuthRequest.PartnerCode = Site.APIPartnerCode;
            request.AuthRequest.AuthToken = Site.APIAuthToken;
            request.AuthRequest.UserAgent = Site.Alias;
            request.AuthRequest.ReplayToken = "SSL Store WhiteBrand Sites : " + Site.Alias;
            request.RefundReason = string.Empty;
            request.TheSSLStoreOrderID = Convert.ToString(id);
            request.CustomOrderID = string.Empty;

            resjson = WBSSLStore.Gateways.RestAPIModels.Services.OrderService.ResendEmail(request);
            return Json(resjson);

        }

        public ActionResult ScanNow(int id)
        {
            return Json(false); //Json(ServiceAPI.ScanVulnerabilityNow(id.ToString(), Site.APIPartnerCode, Site.APIPassword, Site.APIUsername));
        }
        public ActionResult Invoice(int id)
        {
            ViewBag.CurrentUser = user;
            var orderdetails = _repository.Find(x => x.Order.UserID == user.ID && x.OrderID == id && x.Order.SiteID == Site.ID).EagerLoad(o => o.Order, o => o.CertificateRequest, o => o.Product).OrderBy(o => o.ID).ToList();
            return View(orderdetails);
        }

        private IQueryable<OrderDetail> getSearchModel(FormCollection collection)
        {

            IQueryable<OrderDetail> orderdetails = null;

            var vde = System.Globalization.CultureInfo.InvariantCulture;


            int Status = collection["lstStatus"] != null ? Convert.ToInt32(collection["lstStatus"]) : (!string.IsNullOrEmpty(Request.QueryString["lstStatus"]) ? Convert.ToInt32(Request.QueryString["lstStatus"]) : -1);
            DateTime dtStart = collection["txtFromDate"] != null ? Convert.ToDateTime(collection["txtFromDate"]) : DateTimeWithZone.Now.AddYears(-1);// collection["txtFromDate"] != null ? DateTime.ParseExact(collection["txtFromDate"], "d", System.Globalization.CultureInfo.InvariantCulture) : (!string.IsNullOrEmpty(Request.QueryString["txtFromDate"]) ? DateTime.ParseExact(collection["txtFromDate"], "d", System.Globalization.CultureInfo.InvariantCulture) : DateTimeWithZone.Now.AddYears(-1));
            DateTime dtEnd = collection["txtToDate"] != null ? Convert.ToDateTime(collection["txtToDate"]) : DateTimeWithZone.Now;// collection["txtToDate"] != null ? DateTime.ParseExact(collection["txtToDate"], "d", System.Globalization.CultureInfo.InvariantCulture) : (!string.IsNullOrEmpty(Request.QueryString["txtToDate"]) ? DateTime.ParseExact(collection["txtToDate"], "d", System.Globalization.CultureInfo.InvariantCulture) : DateTimeWithZone.Now);

            string DomainName = collection["txtDomainName"] != null ? Convert.ToString(collection["txtDomainName"]) : (!string.IsNullOrEmpty(Request.QueryString["txtDomainName"]) ? Convert.ToString(Request.QueryString["txtDomainName"]) : string.Empty);
            DomainName = string.IsNullOrEmpty(DomainName) ? "" : DomainName;
            string ExternalOrderID = collection["textOrderNumber"] != null ? Convert.ToString(collection["textOrderNumber"]) : (!string.IsNullOrEmpty(Request.QueryString["textOrderNumber"]) ? Convert.ToString(Request.QueryString["textOrderNumber"]) : string.Empty);
            string product = collection["textProductName"] != null ? Convert.ToString(collection["textProductName"]) : (!string.IsNullOrEmpty(Request.QueryString["textProductName"]) ? Convert.ToString(Request.QueryString["textProductName"]) : string.Empty);
            string InvoiceID = collection["textInvoiceID"] != null ? Convert.ToString(collection["textInvoiceID"]) : (!string.IsNullOrEmpty(Request.QueryString["textInvoiceID"]) ? Convert.ToString(Request.QueryString["textInvoiceID"]) : string.Empty);
            InvoiceID = string.IsNullOrEmpty(InvoiceID) ? "0" : InvoiceID.ToLower().Replace(InvoicePrefix.ToLower(), string.Empty);
            int invoiceid = (int.TryParse(InvoiceID, out invoiceid) ? invoiceid : 0);




            ViewBag.Status = Status.ToString();
            ViewBag.ToDate = dtEnd.ToShortDateString();
            ViewBag.FormDate = dtStart.ToShortDateString();
            ViewBag.DomainName = DomainName;
            ViewBag.ExternalOrderID = ExternalOrderID;
            ViewBag.product = product;
            ViewBag.InvoiceID = InvoiceID;

            //dtStart = Convert.ToDateTime(String.Format(new System.Globalization.CultureInfo("en-US"), "{0:dd/MM/yyyy}", collection["txtFromDate"]));// ?? DateTimeWithZone.Now.AddYears(-1).ToShortDateString()));
            //dtEnd = Convert.ToDateTime(String.Format(new System.Globalization.CultureInfo("en-US"), "{0:dd/MM/yyyy}", collection["txtToDate"]));//?? DateTimeWithZone.Now.ToShortDateString()));



            //string fdate = String.Format(new System.Globalization.CultureInfo("en-US"), "{0:dd/MM/yyyy}", dtStart);
            //string edate = String.Format(new System.Globalization.CultureInfo("en-US"), "{0:dd/MM/yyyy}", dtEnd);

            //dtStart = DateTimeWithZone.ToDateTime(dtStart.ToShortDateString());
            //dtEnd = DateTimeWithZone.ToDateTime(dtEnd.ToShortDateString());

            //dtStart = DateTime.ParseExact(fdate, "dd/MM/yyyy", cn.DateTimeFormat);
            //dtEnd = DateTime.ParseExact(edate, "dd/MM/yyyy", cn.DateTimeFormat);

            if (Status <= 2)
            {
                orderdetails = _repository.Find(x => x.Order.UserID == user.ID && x.Order.SiteID == Site.ID && x.ExternalOrderID != string.Empty && x.OrderStatusID == (Status == -1 ? x.OrderStatusID : Status)
                                                   && (System.Data.Entity.DbFunctions.DiffDays(dtStart, x.Order.OrderDate) >= 0 && System.Data.Entity.DbFunctions.DiffDays(x.Order.OrderDate, dtEnd) >= 0)
                                                   && x.ProductName.Contains(string.IsNullOrEmpty(product) ? x.ProductName : product)
                                                    && (x.CertificateRequest.DomainName.Contains(DomainName))
                                                    && x.ExternalOrderID.Equals(string.IsNullOrEmpty(ExternalOrderID) ? x.ExternalOrderID : ExternalOrderID)
                                                    && x.OrderID.Equals(invoiceid.Equals(0) ? x.OrderID : invoiceid)
                                                   ).Include(o => o.Order).Include(o => o.Product).Include(o => o.CertificateRequest)
                                                   .Include(o => o.AuditDetails).Include(o => o.StoreAPIInteraction)
                                                   .OrderByDescending(o => o.ID);

            }
            else if (Status == 3)
            {
                orderdetails = _repository.Find(x => x.Order.UserID == user.ID && x.Order.SiteID == Site.ID && x.ExternalOrderID != string.Empty && x.OrderStatusID == (int)OrderStatus.ACTIVE
                                                       && (DbFunctions.DiffDays(DateTimeWithZone.Now, x.CertificateExpiresOn) <= 0)
                                                       ).Include(o => o.Order).Include(o => o.Product)
                                                       .Include(o => o.CertificateRequest).Include(o => o.AuditDetails).Include(o => o.StoreAPIInteraction)
                                                       .OrderByDescending(o => o.ID);
            }
            else
            {
                int DateDiff = 0;
                if (Status == 4)
                    DateDiff = 30;
                else if (Status == 5)
                    DateDiff = 60;

                orderdetails = _repository.Find(x => x.Order.UserID == user.ID && x.Order.SiteID == Site.ID && x.ExternalOrderID != string.Empty
                                                && (DbFunctions.DiffDays(DateTimeWithZone.Now, x.CertificateExpiresOn) >= 0 && DbFunctions.DiffDays(DateTimeWithZone.Now, x.CertificateExpiresOn) <= DateDiff)
                                                 ).Include(o => o.Order)
                                                 .Include(o => o.Product).Include(o => o.CertificateRequest).Include(o => o.AuditDetails).Include(o => o.StoreAPIInteraction)
                                                 .OrderByDescending(o => o.ID);
            }
            return orderdetails;
        }

        private void ExportDataToCSV(FormCollection collection)
        {
            List<OrderDetail> lstorderdetail = getSearchModel(collection).ToList();

            List<string> lstData = new List<string>();
            lstData.Add("Order Date,OrderID,Product Name,InvoiceID,Domain Name,Order Status,Price,");
            foreach (OrderDetail ord in lstorderdetail)
            {
                lstData.Add(ord.AuditDetails.DateCreated.ToShortDateString() + "," + ord.ExternalOrderID + "," + ord.ProductName + "," +
                    InvoicePrefix + ord.OrderID + "," + (string.IsNullOrEmpty(ord.CertificateRequest.DomainName) ? "-" : ord.CertificateRequest.DomainName) + "," + ord.OrderStatus + "," + ord.Price.ToString("c").Replace(",", string.Empty));
            }
            WBHelper.GetCSVFile(lstData, "OrderList");
        }

        private IQueryable<OrderDetail> GetAccountSummaryDetail()
        {
            AccountSummary acc = new AccountSummary();

            UserExt uex = _service.GetUserAccountSummary(user.ID, Site.ID);
            if (uex != null)
            {
                acc.TotalInCompleteOrders = Convert.ToInt32(uex.TotalInCompleteOrders);
                acc.Balance = Convert.ToDecimal(uex.CuurentBalance);
                acc.TotalOrders = Convert.ToInt32(uex.TotalOrders);
                acc.TotalTransaction = Convert.ToDecimal(uex.TotalPurchase);
                acc.TotalSupportIncident = Convert.ToInt32(uex.TotalSupportIncident);
            }
            ViewBag.AccountSummary = acc;

            return _repository.Find(x => x.Order.UserID == user.ID && x.Order.SiteID == Site.ID && (x.ExternalOrderID == null || x.ExternalOrderID == string.Empty)).Include(o => o.Order).Include(o => o.Product).Include(o => o.CertificateRequest).Include(o => o.AuditDetails).Include(o => o.StoreAPIInteraction).OrderBy(o => o.ID);

        }

        public void GetOrderstatusThruRESTAPI(OrderDetail orderdetail, Site site)
        {

            WBSSLStore.Gateways.RestAPIModels.Request.OrderRequest request = new WBSSLStore.Gateways.RestAPIModels.Request.OrderRequest();
            request.AuthRequest = new WBSSLStore.Gateways.RestAPIModels.Request.AuthRequest();
            try
            {
                request.AuthRequest.PartnerCode = Site.APIPartnerCode;
                request.AuthRequest.AuthToken = Site.APIAuthToken;
                request.AuthRequest.UserAgent = Site.Alias;
                request.AuthRequest.ReplayToken = "SSL Store WhiteBrand Sites : " + Site.Alias;
                request.TheSSLStoreOrderID = Convert.ToString(orderdetail.ExternalOrderID);
                request.CustomOrderID = string.Empty;
                WBSSLStore.Gateways.RestAPIModels.Response.OrderResponse apiord = new WBSSLStore.Gateways.RestAPIModels.Response.OrderResponse();
                apiord = WBSSLStore.Gateways.RestAPIModels.Services.OrderService.OrderStatus(request);
                //TheSSLStoreIOrderDetail apiord = ServiceAPI.GetTheSSLStoreOrderDeatil(Site.APIPartnerCode, Site.APIPassword, Site.APIUsername, orderdetail.ExternalOrderID);

                if (apiord != null && (orderdetail.OrderStatus != OrderStatus.REFUNDED && orderdetail.OrderStatus != OrderStatus.REJECTED))
                {
                    //orderdetail.Product.InternalProductCode.ToLower() == "8" ||
                    if (orderdetail.Product.InternalProductCode.ToLower() == "tkpcidss" || orderdetail.Product.InternalProductCode.ToLower() == "tw14")
                        orderdetail.OrderStatus = OrderStatus.ACTIVE;
                    else
                        orderdetail.OrderStatus = WBSSLStore.Web.Helpers.WBHelper.GetOrderDetailStatusValue(apiord.OrderStatus.MajorStatus);

                    ViewBag.CertificateStatus = apiord.OrderStatus.MinorStatus;
                    ViewBag.OrderState = apiord.OrderStatus.MajorStatus + "( " + apiord.OrderStatus.MinorStatus + " )";

                    ViewBag.TokenCode = !string.IsNullOrEmpty(apiord.TokenCode) ? apiord.TokenCode : string.Empty;
                    ViewBag.TokenID = !string.IsNullOrEmpty(apiord.TokenID) ? apiord.TokenID : string.Empty;
                    ViewBag.AuthFileContent = !string.IsNullOrEmpty(apiord.AuthFileContent) ? Convert.ToString(apiord.AuthFileContent).Replace("\n", "").Replace("\r", "") : string.Empty;
                    ViewBag.AuthFileName = !string.IsNullOrEmpty(apiord.AuthFileName) ? apiord.AuthFileName : string.Empty;
                    ViewBag.PollStatus = !string.IsNullOrEmpty(apiord.PollStatus) ? apiord.PollStatus : string.Empty;
                    ViewBag.PollDate = !string.IsNullOrEmpty(apiord.PollDate) ? apiord.PollDate : string.Empty;

                    if (orderdetail.OrderStatus == OrderStatus.ACTIVE)
                    {
                        if (Convert.ToDateTime(apiord.CertificateStartDate, System.Globalization.CultureInfo.InvariantCulture) > Convert.ToDateTime(DateTime.MinValue))
                            orderdetail.ActivatedDate = Convert.ToDateTime(apiord.CertificateStartDate, System.Globalization.CultureInfo.InvariantCulture);
                        if (Convert.ToDateTime(apiord.CertificateEndDate, System.Globalization.CultureInfo.InvariantCulture) > DateTime.MinValue)
                            orderdetail.CertificateExpiresOn = Convert.ToDateTime(apiord.CertificateEndDate, System.Globalization.CultureInfo.InvariantCulture);
                    }

                    orderdetail.CertificateRequest.CertificateApproverEmail = string.IsNullOrEmpty(apiord.ApproverEmail) ? string.Empty : apiord.ApproverEmail;
                    orderdetail.CertificateRequest.CountryText = string.IsNullOrEmpty(apiord.Country) ? string.Empty : apiord.Country;
                    orderdetail.CertificateRequest.DomainName = string.IsNullOrEmpty(apiord.CommonName) ? string.Empty : apiord.CommonName;
                    orderdetail.CertificateRequest.Locality = string.IsNullOrEmpty(apiord.Locality) ? string.Empty : apiord.Locality;
                    //orderdetail.CertificateRequest.NumberOfMonths = apiord.c;
                    //orderdetail.CertificateRequest.NumberOfServers = apiord.ServerCount;
                    orderdetail.CertificateRequest.Organisation = string.IsNullOrEmpty(apiord.Organization) ? string.Empty : apiord.Organization;
                    orderdetail.CertificateRequest.OrganisationUnit = string.IsNullOrEmpty(apiord.OrganizationalUnit) ? string.Empty : apiord.OrganizationalUnit;
                    //orderdetail.CertificateRequest.SpecialInstructions = string.IsNullOrEmpty(apiord.s) ? string.Empty : apiord.SpecialInstructions;
                    orderdetail.CertificateRequest.State = string.IsNullOrEmpty(apiord.State) ? string.Empty : apiord.State;
                    orderdetail.CertificateRequest.AddDomainNames = apiord.DNSNames;
                    orderdetail.OrderStatusID = (int)orderdetail.OrderStatus;

                    if (apiord.AdminContact != null)
                    {
                        orderdetail.CertificateRequest.AdminContact.FirstName = string.IsNullOrEmpty(apiord.AdminContact.FirstName) ? string.Empty : apiord.AdminContact.FirstName;
                        orderdetail.CertificateRequest.AdminContact.LastName = string.IsNullOrEmpty(apiord.AdminContact.LastName) ? string.Empty : apiord.AdminContact.LastName;
                        orderdetail.CertificateRequest.AdminContact.PhoneNumber = string.IsNullOrEmpty(apiord.AdminContact.Phone) ? string.Empty : apiord.AdminContact.Phone;
                        orderdetail.CertificateRequest.AdminContact.EmailAddress = string.IsNullOrEmpty(apiord.AdminContact.Email) ? string.Empty : apiord.AdminContact.Email;
                        orderdetail.CertificateRequest.AdminContact.Title = string.IsNullOrEmpty(apiord.AdminContact.Title) ? string.Empty : apiord.AdminContact.Title;

                    }
                    if (apiord.TechnicalContact != null)
                    {
                        orderdetail.CertificateRequest.TechnicalContact.FirstName = string.IsNullOrEmpty(apiord.TechnicalContact.FirstName) ? string.Empty : apiord.TechnicalContact.FirstName;
                        orderdetail.CertificateRequest.TechnicalContact.LastName = string.IsNullOrEmpty(apiord.TechnicalContact.LastName) ? string.Empty : apiord.TechnicalContact.LastName;
                        orderdetail.CertificateRequest.TechnicalContact.PhoneNumber = string.IsNullOrEmpty(apiord.TechnicalContact.Phone) ? string.Empty : apiord.TechnicalContact.Phone;
                        orderdetail.CertificateRequest.TechnicalContact.EmailAddress = string.IsNullOrEmpty(apiord.TechnicalContact.Email) ? string.Empty : apiord.TechnicalContact.Email;
                        orderdetail.CertificateRequest.TechnicalContact.Title = string.IsNullOrEmpty(apiord.TechnicalContact.Title) ? string.Empty : apiord.TechnicalContact.Title;

                    }

                    _repository.Update(orderdetail);
                    _unitOfWork.Commit();
                }
            }
            catch
            {
            }
        }
        // Ended By <<MEHUL>>
    }
}