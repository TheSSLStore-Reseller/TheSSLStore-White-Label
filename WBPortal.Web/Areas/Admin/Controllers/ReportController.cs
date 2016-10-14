using System;
using System.Linq;
using System.Web.Mvc;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Domain;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Service;
using WBSSLStore.Web.Helpers.PagedList;
using WBSSLStore.Web.Util;
using System.Collections.Generic;
using WBSSLStore.Web.Helpers;

namespace WBSSLStore.Web.Areas.Admin.Controllers
{
    [CustomAuthorizeAttribute]
    [HandleError]
    public class ReportController : WBController<OrderDetail, IRepository<OrderDetail>, IProductService>
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
        public ActionResult DateWise(FormCollection collection, int? page)
        {
            ViewBag.StartDate = collection["txtFromDate"] ?? Request.QueryString["txtFromDate"];
            ViewBag.EndDate = collection["txtToDate"] ?? Request.QueryString["txtToDate"];
            ViewBag.Status = collection["ddlStatus"] ?? Request.QueryString["status"];

            OrderStatus? eOrderStatus = null;
            DateTime dtStart = ViewBag.StartDate == null ? DateTimeWithZone.Now.AddYears(-1) : Convert.ToDateTime(ViewBag.StartDate);
            DateTime dtEnd = ViewBag.EndDate == null ? DateTimeWithZone.Now : Convert.ToDateTime(ViewBag.EndDate);

            if (!PagingInputValidator.IsPagingInputValid(ref page))
                return View();

            if (!string.IsNullOrEmpty(ViewBag.Status))
                eOrderStatus = (OrderStatus?)Enum.Parse(typeof(OrderStatus), ViewBag.Status.ToString(), true);

            ViewBag.StartDate = dtStart.ToShortDateString();
            ViewBag.EndDate = dtEnd.ToShortDateString();
            ViewBag.Status = eOrderStatus.ToString();

            IQueryable<OrderDetail> orderdetailquery = GetDateWiseOrders(eOrderStatus, dtStart, dtEnd);

            List<OrderDetail> lstOrderDetail = orderdetailquery.Where(od => od.OrderStatusID != (int)OrderStatus.REFUNDED && od.OrderStatusID != (int)OrderStatus.REJECTED).ToList();
            ViewBag.TotalPurchase = lstOrderDetail.Sum(od => od.Price);
            ViewBag.NoOfCerts = lstOrderDetail.Count();

            if (collection["btnExport"] != null && collection["btnExport"].ToString().ToLower().Equals("export to csv"))
            {
                ExportDataToCSV(eOrderStatus, dtStart, dtEnd);
                return View();
            }
            else
            {
                var orderdetail = orderdetailquery.ToPagedList(page.HasValue ? page.Value - 1 : 0, PageSize);
                orderdetail.ActionName = "datewise";
                orderdetail.ControllerName = "report";

                if (!PagingInputValidator.IsPagingInputValid(ref page, orderdetail))
                    return View();

                return View(orderdetail);
            }
        }

        public ActionResult ProductWise(FormCollection collection, int? page)
        {
            ViewBag.StartDate = collection["txtFromDate"] ?? Request.QueryString["txtFromDate"];
            ViewBag.EndDate = collection["txtToDate"] ?? Request.QueryString["txtToDate"];
            ViewBag.BrandID = collection["ddlBrand"] ?? Request.QueryString["brandid"];
            ViewBag.ProductID = collection["ddlProduct"] ?? Request.QueryString["productid"];
            ViewBag.Month = collection["ddlYears"] ?? Request.QueryString["month"];

            DateTime dtStart = ViewBag.StartDate == null ? DateTimeWithZone.Now.AddYears(-1) : Convert.ToDateTime(ViewBag.StartDate);
            DateTime dtEnd = ViewBag.EndDate == null ? DateTimeWithZone.Now : Convert.ToDateTime(ViewBag.EndDate);
            int BrandID = string.IsNullOrEmpty(ViewBag.BrandID) ? 0 : Convert.ToInt16(ViewBag.BrandID);
            int ProductID = Convert.ToInt16(ViewBag.ProductID);
            int Month = Convert.ToInt16(ViewBag.Month);

            if (!PagingInputValidator.IsPagingInputValid(ref page))
                return View();

            ViewBag.StartDate = dtStart.ToShortDateString();
            ViewBag.EndDate = dtEnd.ToShortDateString();
            ViewBag.BrandID = BrandID;
            ViewBag.ProductID = ProductID;
            ViewBag.Month = Month;

            IQueryable<OrderDetail> orderdetailquery = GetDateWiseOrders(null, dtStart, dtEnd)
                                                    .Where(od => od.OrderStatusID != (int)OrderStatus.REFUNDED && od.OrderStatusID != (int)OrderStatus.REJECTED
                                                            && od.ProductID == (ProductID > 0 ? ProductID : od.ProductID)
                                                            && od.Product.BrandID == (BrandID > 0 ? BrandID : od.Product.BrandID)
                                                            && od.NumberOfMonths == (Month > 0 ? Month : od.NumberOfMonths)
                                                            );

            List<OrderDetail> lstOrderDetail = orderdetailquery.ToList();
            ViewBag.TotalPurchase = lstOrderDetail.Sum(od => od.Price);
            ViewBag.NoOfCerts = lstOrderDetail.Count();

            ViewBag.Brand = _service.GetBrand().ToList();
            GetProductList();

            if (collection["btnExport"] != null && collection["btnExport"].ToString().ToLower().Equals("export to csv"))
            {
                ExportDataToCSV(null, dtStart, dtEnd);
                return View();
            }
            else
            {
                var orderdetail = orderdetailquery.ToPagedList(page.HasValue ? page.Value - 1 : 0, PageSize);
                orderdetail.ActionName = "productwise";
                orderdetail.ControllerName = "report";

                if (!PagingInputValidator.IsPagingInputValid(ref page, orderdetail))
                    return View();

                return View(orderdetail);
            }
        }

        public ActionResult CustomerWise(FormCollection collection, int? page)
        {
            ViewBag.StartDate = collection["txtFromDate"] ?? Request.QueryString["txtFromDate"];
            ViewBag.EndDate = collection["txtToDate"] ?? Request.QueryString["txtToDate"];
            ViewBag.Name = collection["txtName"] ?? Request.QueryString["name"];
            ViewBag.Email = collection["txtEmail"] ?? Request.QueryString["email"];
            ViewBag.UserType = collection["ddlUserType"] ?? Request.QueryString[SettingConstants.QS_USER_TYPE];

            DateTime dtStart = ViewBag.StartDate == null ? DateTimeWithZone.Now.AddYears(-1) : Convert.ToDateTime(ViewBag.StartDate);
            DateTime dtEnd = ViewBag.EndDate == null ? DateTimeWithZone.Now : Convert.ToDateTime(ViewBag.EndDate);
            string strName = ViewBag.Name;
            string strEmail = ViewBag.Email;
            if (!PagingInputValidator.IsPagingInputValid(ref page))
                return View();

            ViewBag.StartDate = dtStart.ToShortDateString();
            ViewBag.EndDate = dtEnd.ToShortDateString();            

            UserType? eUserType = null;
            if (!string.IsNullOrEmpty(ViewBag.UserType))
                eUserType = (UserType?)Enum.Parse(typeof(UserType), ViewBag.UserType.ToString(), true);
            ViewBag.UserType = eUserType;

            IQueryable<OrderDetail> orderdetailquery = GetDateWiseOrders(null, dtStart, dtEnd)
                                                    .Where(od => od.OrderStatusID != (int)OrderStatus.REFUNDED && od.OrderStatusID != (int)OrderStatus.REJECTED
                                                            && (od.Order.User.FirstName + " " + od.Order.User.LastName).Contains((string.IsNullOrEmpty(strName) ? od.Order.User.FirstName : strName))
                                                            && od.Order.User.Email.Contains((string.IsNullOrEmpty(strEmail) ? od.Order.User.Email : strEmail))
                                                            && od.Order.User.UserTypeID == (eUserType != null ? (int?)eUserType : od.Order.User.UserTypeID));

            List<OrderDetail> lstOrderDetail = orderdetailquery.ToList();
            ViewBag.TotalPurchase = lstOrderDetail.Sum(od => od.Price);
            ViewBag.NoOfCerts = lstOrderDetail.Count();

            ViewBag.Brand = _service.GetBrand().ToList();
            GetProductList();

            if (collection["btnExport"] != null && collection["btnExport"].ToString().ToLower().Equals("export to csv"))
            {
                ExportDataToCSV(null, dtStart, dtEnd);
                return View();
            }
            else
            {
                var orderdetail = orderdetailquery.ToPagedList(page.HasValue ? page.Value - 1 : 0, PageSize);
                orderdetail.ActionName = "customerwise";
                orderdetail.ControllerName = "report";

                if (!PagingInputValidator.IsPagingInputValid(ref page, orderdetail))
                    return View();

                return View(orderdetail);
            }
        }

        public ActionResult TransactionWise(FormCollection collection, int? page)
        {
            ViewBag.StartDate = collection["txtFromDate"] ?? Request.QueryString["txtFromDate"];
            ViewBag.EndDate = collection["txtToDate"] ?? Request.QueryString["txtToDate"];
            ViewBag.Status = collection["ddlTransactionMode"] ?? Request.QueryString["status"];

            if (!PagingInputValidator.IsPagingInputValid(ref page))
                return View();

            DateTime dtStart = ViewBag.StartDate == null ? Convert.ToDateTime(DateTimeWithZone.Now.Month + "/01/" + DateTimeWithZone.Now.Year) : Convert.ToDateTime(ViewBag.StartDate);
            DateTime dtEnd = ViewBag.EndDate == null ? DateTimeWithZone.Now : Convert.ToDateTime(ViewBag.EndDate);
            TransactionMode? eTransactionMode = null;

            if (!string.IsNullOrEmpty(ViewBag.Status))
                eTransactionMode = (TransactionMode?)Enum.Parse(typeof(TransactionMode), ViewBag.Status.ToString(), true);

            ViewBag.StartDate = dtStart.ToShortDateString();
            ViewBag.EndDate = dtEnd.ToShortDateString();
            ViewBag.Status = eTransactionMode.ToString();

            var _usertransactions = DependencyResolver.Current.GetService<IRepository<UserTransaction>>();
            IQueryable<UserTransaction> usertransactionquery = _usertransactions.Find(ut => ut.SiteID == Site.ID
                                                                                    && ut.TransactionModeID == (eTransactionMode != null ? (int?)eTransactionMode : ut.TransactionModeID)
                                                                                    && (System.Data.Entity.DbFunctions.DiffDays(dtStart, ut.AuditDetails.DateCreated) >= 0
                                                                                    && System.Data.Entity.DbFunctions.DiffDays(ut.AuditDetails.DateCreated, dtEnd) >= 0))
                                                                                 .EagerLoad(ut => ut.Payment, ut => ut.Payment.GatewayInteraction)
                                                                                 .OrderByDescending(ut => ut.AuditDetails.DateCreated).ThenBy(ut => ut.OrderDetailID);
            
            if (collection["btnExport"] != null && collection["btnExport"].ToString().ToLower().Equals("export to csv"))
            {
                //ExportDataToCSV(nu, dtStart, dtEnd);
                return View();
            }
            else
            {
                var userTrans = usertransactionquery.ToPagedList(page.HasValue ? page.Value - 1 : 0, PageSize);
                userTrans.ActionName = "transactionwise";
                userTrans.ControllerName = "report";

                if (!PagingInputValidator.IsPagingInputValid(ref page, userTrans))
                    return View();

                return View(userTrans);
            }
        }

        public IQueryable<OrderDetail> GetDateWiseOrders(OrderStatus? eOrderStatus, DateTime dtStartDate, DateTime dtEndDate)
        {
            var orderdetail = _repository.Find(od => od.Order.SiteID == Site.ID
                                                    && od.OrderStatusID == (eOrderStatus != null ? (int?)eOrderStatus : od.OrderStatusID)
                                                    && (System.Data.Entity.DbFunctions.DiffDays(dtStartDate, od.AuditDetails.DateCreated) >= 0
                                                    && System.Data.Entity.DbFunctions.DiffDays(od.AuditDetails.DateCreated, dtEndDate) >= 0))
                                             .EagerLoad(od => od.AuditDetails, od => od.CertificateRequest, od => od.Order.User)
                                             .OrderByDescending(ord => ord.AuditDetails.DateCreated);
            return orderdetail;
        }

        private void GetProductList()
        {
            var _productavailablities = DependencyResolver.Current.GetService<IRepository<ProductAvailablity>>();
            ViewBag.Products = _productavailablities.Find(pa => pa.SiteID == Site.ID && pa.isActive == true && pa.Product.RecordStatusID == (int)RecordStatus.ACTIVE).ToList();
        }

        public void ExportDataToCSV(OrderStatus? eOrderStatus, DateTime dtStartDate, DateTime dtEndDate)
        {
            var orderdetail = GetDateWiseOrders(eOrderStatus, dtStartDate, dtEndDate).ToList();

            List<string> lstData = new List<string>();
            lstData.Add("Order Date,OrderID,Product Name,InvoiceID,Domain Name,Order Status,Price,");
            foreach (OrderDetail ord in orderdetail)
            {
                lstData.Add(ord.AuditDetails.DateCreated.ToShortDateString() + "," + ord.ExternalOrderID + "," + ord.ProductName + "," +
                    InvoicePrefix + ord.OrderID + "," + (string.IsNullOrEmpty(ord.CertificateRequest.DomainName) ? "-" : ord.CertificateRequest.DomainName) + "," + ord.OrderStatus + "," + ord.Price.ToString("c").Replace(",", string.Empty));
            }
            WBHelper.GetCSVFile(lstData, "OrderList");
        }
    }
}
