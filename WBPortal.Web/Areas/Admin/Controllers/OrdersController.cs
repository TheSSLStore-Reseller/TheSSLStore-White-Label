using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Domain;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Web.Helpers.PagedList;

using WBSSLStore.Web.Helpers;
using WBSSLStore.Data.Repository;

namespace WBSSLStore.Web.Areas.Admin.Controllers
{
    [CustomAuthorizeAttribute]
    [HandleError]
    public class OrdersController : WBController<OrderDetail, IRepository<OrderDetail>, IWBRepository>
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
        // GET: /Admin/Orders/
        public ViewResult Index(FormCollection collection, int? ID, int? page)
        {
            ViewBag.StartDate = collection["txtFromDate"] ?? Request.QueryString["txtFromDate"];
            ViewBag.EndDate = collection["txtToDate"] ?? Request.QueryString["txtToDate"];
            ViewBag.OrderStatus = collection["ddlStatus"] ?? Request.QueryString["ddlStatus"];

            OrderStatus? eOrderStatus = null;
            
           
            //DateTime dtStart = ViewBag.StartDate == null ? DateTimeWithZone.Now.AddYears(-1) : DateTime.ParseExact(ViewBag.StartDate, "d", System.Globalization.CultureInfo.InvariantCulture);
            //DateTime dtEnd = ViewBag.EndDate == null ? DateTimeWithZone.Now : DateTime.ParseExact(ViewBag.EndDate, "d", System.Globalization.CultureInfo.InvariantCulture);

            ViewBag.EndDate = !string.IsNullOrEmpty(ViewBag.EndDate) ? Convert.ToString(ViewBag.EndDate).Trim() : ViewBag.EndDate;
            ViewBag.StartDate = !string.IsNullOrEmpty(ViewBag.StartDate) ? Convert.ToString(ViewBag.StartDate).Trim() : ViewBag.StartDate;

            DateTime dtStart = ViewBag.StartDate == null ? DateTimeWithZone.Now.AddYears(-1) : DateTime.ParseExact(ViewBag.StartDate, "d", System.Globalization.CultureInfo.InvariantCulture);
            DateTime dtEnd = ViewBag.EndDate == null ? DateTimeWithZone.Now : DateTime.ParseExact(ViewBag.EndDate, "d", System.Globalization.CultureInfo.InvariantCulture);



            if (!PagingInputValidator.IsPagingInputValid(ref page))
                return View();

            if (!string.IsNullOrEmpty(ViewBag.OrderStatus))
                eOrderStatus = (OrderStatus?)Enum.Parse(typeof(OrderStatus), ViewBag.OrderStatus.ToString(), true);

            ViewBag.StartDate = Convert.ToDateTime(dtStart).ToString("MM/dd/yyyy");
            ViewBag.EndDate = Convert.ToDateTime(dtEnd).ToString("MM/dd/yyyy");

            ViewBag.OrderStatus = eOrderStatus.ToString();

            if (collection["btnExport"] != null && collection["btnExport"].ToString().ToLower().Equals("export to csv"))
            {
                ExportDataToCSV(ID, eOrderStatus, dtStart, dtEnd);
                return View();
            }
            else
            {
                var orderdetail = _repository.Find(od => od.Order.SiteID == Site.ID && od.Order.UserID == (ID == null ? od.Order.UserID : ID)
                                            && od.OrderStatusID == (eOrderStatus != null ? (int?)eOrderStatus : od.OrderStatusID)
                                            && (System.Data.Entity.DbFunctions.DiffDays(dtStart, od.AuditDetails.DateCreated) >= 0
                                            && System.Data.Entity.DbFunctions.DiffDays(od.AuditDetails.DateCreated, dtEnd) >= 0))
                            .EagerLoad(od => od.AuditDetails, od => od.CertificateRequest)
                            .OrderByDescending(ord => ord.AuditDetails.DateCreated)
                            .ToPagedList(page.HasValue ? page.Value - 1 : 0, PageSize);
                orderdetail.ActionName = "index";
                orderdetail.ControllerName = "orders";

                if (!PagingInputValidator.IsPagingInputValid(ref page, orderdetail))
                    return View();

                return View(orderdetail);
            }
        }

        public ViewResult Search(FormCollection collection, int? page)
        {
            ViewBag.StartDate = collection["txtFromDate"] ?? Request.QueryString["txtFromDate"];
            ViewBag.EndDate = collection["txtToDate"] ?? Request.QueryString["txtToDate"];
            ViewBag.OrderStatus = collection["ddlStatus"] ?? Request.QueryString["ddlStatus"];
            ViewBag.AuthCode = collection["txtAuthCode"] ?? Request.QueryString["txtAuthCode"];
            ViewBag.ExternalOrderID = collection["txtOrderNumber"] ?? Request.QueryString["txtOrderNumber"];
            ViewBag.DomainName = collection["txtDomainName"] ?? Request.QueryString["txtDomainName"];
            ViewBag.Product = collection["ddlProduct"] ?? Request.QueryString["ddlProduct"];
            ViewBag.InvoiceID = collection["txtInvoiceID"] ?? Request.QueryString["txtInvoiceID"];



            BindProducts();

            if (ViewBag.StartDate != null)
            {

                OrderStatus? eOrderStatus = null;
                if (!string.IsNullOrEmpty(ViewBag.OrderStatus))
                    eOrderStatus = (OrderStatus?)Enum.Parse(typeof(OrderStatus), ViewBag.OrderStatus.ToString(), true);

                
                //var d = DateTime.ParseExact(Convert.ToString(ViewBag.StartDate), "d", System.Globalization.CultureInfo.InvariantCulture);                
           
                ViewBag.EndDate = !string.IsNullOrEmpty(ViewBag.EndDate) ? Convert.ToString(ViewBag.EndDate).Trim() : ViewBag.EndDate;
                ViewBag.StartDate = !string.IsNullOrEmpty(ViewBag.StartDate) ? Convert.ToString(ViewBag.StartDate).Trim() : ViewBag.StartDate;


                DateTime dtStart = ViewBag.StartDate == null ? DateTimeWithZone.Now.Month + "/01/" + DateTimeWithZone.Now.Year : DateTime.ParseExact(ViewBag.StartDate, "d", System.Globalization.CultureInfo.InvariantCulture);
                DateTime dtEnd = ViewBag.EndDate == null ? DateTimeWithZone.Now : DateTime.ParseExact(ViewBag.EndDate, "d", System.Globalization.CultureInfo.InvariantCulture);

                string DomainName = ViewBag.DomainName;
                DomainName = string.IsNullOrEmpty(DomainName) ? "" : DomainName.Trim();

                int product = string.IsNullOrEmpty(ViewBag.Product) ? 0 : Convert.ToInt16(ViewBag.Product);
                string InvoiceID = ViewBag.InvoiceID;
                InvoiceID = string.IsNullOrEmpty(InvoiceID) ? "-2" : InvoiceID.ToLower().Replace(InvoicePrefix.ToLower(), string.Empty);
                int invoiceid = WBHelper.ToInt(InvoiceID, -1);

                string strExternalOrderID = ViewBag.ExternalOrderID, strAuthCode = ViewBag.AuthCode;


                ViewBag.StartDate = Convert.ToDateTime(dtStart).ToString("MM/dd/yyyy");
                ViewBag.EndDate = Convert.ToDateTime(dtEnd).ToString("MM/dd/yyyy");

                ViewBag.DomainName = DomainName;
                ViewBag.OrderStatus = eOrderStatus.ToString();

                if (!PagingInputValidator.IsPagingInputValid(ref page))
                    return View();

                var orderInfo = _service.SearchOrder(Site.ID, invoiceid, strExternalOrderID, DomainName, dtStart, dtEnd, product, eOrderStatus, strAuthCode)
                                        .OrderByDescending(od => od.OrderDate)
                                        .ToPagedList(page.HasValue ? page.Value - 1 : 0, PageSize);
                orderInfo.ActionName = "Search";
                orderInfo.ControllerName = "orders";

                if (!PagingInputValidator.IsPagingInputValid(ref page, orderInfo))
                    return View();

                ViewBag.Message = (orderInfo != null && orderInfo.Count > 0 ? string.Empty : "No order found.");
                return View(orderInfo);
            }
            return View();
        }

        public ViewResult Detail(int id)
        {
            ViewBag.ProductPricingID = 0;

            var _supportreq = DependencyResolver.Current.GetService<IRepository<SupportRequest>>();
            SupportRequest objSppReq = _supportreq.Find(y => y.OrderDetailID == id && y.SupportTypeID == (int)SupportType.REFUNDREQUEST).FirstOrDefault();

            if (objSppReq != null)
            {
                if (objSppReq.OrderDetailID == id)
                    ViewBag.CancelationRequest = "Cancel";
            }



            OrderDetail orderdetail = _repository.Find(x => x.ID == id)
                                                 .EagerLoad(o => o.Order, o => o.Product, o => o.CertificateRequest, o => o.AuditDetails, o => o.StoreAPIInteraction, o => o.CertificateRequest.AdminContact, o => o.CertificateRequest.TechnicalContact, o => o.Product)
                                                 .FirstOrDefault();
            if (orderdetail != null && orderdetail.Price > 0)
            {
                var _usertransaction = DependencyResolver.Current.GetService<IRepository<UserTransaction>>();
                UserTransaction objTrans = _usertransaction.Find(ut => ut.OrderDetailID == id && ut.PaymentID != null)
                                                           .EagerLoad(ut => ut.Payment, ut => ut.Payment.GatewayInteraction)
                                                           .FirstOrDefault();
                if (objTrans != null)
                {
                    ViewBag.PaymentType = objTrans.Payment.PaymentMode.GetEnumDescription<PaymentMode>();
                    ViewBag.AuthCode = objTrans.Payment.GatewayInteraction.GatewayAuthCode;
                }
            }
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

        public ActionResult Invoice(int id)
        {
            var orderdetails = _repository.Find(x => x.OrderID == id && x.Order.SiteID == Site.ID)
                                          .EagerLoad(o => o.Order, o => o.CertificateRequest, o => o.Order.User, o => o.Product).OrderBy(o => o.ID).ToList();

            return View(orderdetails);
        }

        [HttpPost]
        public void ExportDataToCSV(int? UserID, OrderStatus? eOrderStatus, DateTime dtStartDate, DateTime dtEndDate)
        {
            var orderdetail = (_repository.Find(od => od.Order.SiteID == Site.ID && od.Order.UserID == (UserID == null ? od.Order.UserID : UserID)
                                       && od.OrderStatusID == (eOrderStatus != null ? (int?)eOrderStatus : od.OrderStatusID)
                                       && (System.Data.Entity.DbFunctions.DiffDays(dtStartDate, od.AuditDetails.DateCreated) >= 0 && System.Data.Entity.DbFunctions.DiffDays(od.AuditDetails.DateCreated, dtEndDate) >= 0))
                       .EagerLoad(ut => ut.AuditDetails, ut => ut.CertificateRequest).OrderByDescending(ord => ord.AuditDetails.DateCreated)).ToList();

            List<string> lstData = new List<string>();
            lstData.Add("Order Date,OrderID,Product Name,InvoiceID,Domain Name,Order Status,Price,");
            foreach (OrderDetail ord in orderdetail)
            {
                lstData.Add(ord.AuditDetails.DateCreated.ToShortDateString() + "," + ord.ExternalOrderID + "," + ord.ProductName + "," +
                    InvoicePrefix + ord.OrderID + "," + (string.IsNullOrEmpty(ord.CertificateRequest.DomainName) ? "-" : ord.CertificateRequest.DomainName) + "," + ord.OrderStatus + "," + ord.Price.ToString("c").Replace(",", string.Empty));
            }
            WBHelper.GetCSVFile(lstData, "OrderList");
        }

        private void BindProducts()
        {
            var _product = DependencyResolver.Current.GetService<IRepository<ProductAvailablity>>();
            ViewBag.ProductList = from price in (_product.Find(pa => pa.SiteID == Site.ID && pa.Product.RecordStatusID != (int)RecordStatus.DELETED))
                                  select new { Text = price.Product.ProductName, Value = price.Product.ID };
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
            byte[] bytfilecontent = System.Text.ASCIIEncoding.ASCII.GetBytes(filecontent);
            Response.OutputStream.Write(bytfilecontent, 0, bytfilecontent.Count());
        }


        public ActionResult DownloadCertificate(int id)
        {

            GetDonwloadCertificateRESTAPI(Site, id);
            return Json("true", JsonRequestBehavior.AllowGet);

        }


        public void GetDonwloadCertificateRESTAPI(Site s, int id)
        {
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
            //End: Coded by veeral
        }

        // Ended By <<MEHUL>>
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


        private void GetOrderstatusThruRESTAPI(OrderDetail orderdetail, Site site)
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

                if (apiord != null && (orderdetail.OrderStatus != OrderStatus.REJECTED && orderdetail.OrderStatus != OrderStatus.REFUNDED))
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
                    ViewBag.AuthFileContent = !string.IsNullOrEmpty(apiord.AuthFileContent) ? Url.Encode(apiord.AuthFileContent) : string.Empty;
                    ViewBag.AuthFileName = !string.IsNullOrEmpty(apiord.AuthFileName) ? Url.Encode(apiord.AuthFileName) : string.Empty;
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
