using System;
using System.Linq;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Domain;
using System.Data.SqlClient;

namespace WBSSLStore.Data.Repository
{

    public class WBRepository : EFRepository<User>, IWBRepository
    {
        private IDatabaseFactory _dbfactory;
        public WBRepository(IDatabaseFactory dbfactory)
            : base(dbfactory)
        {
            _dbfactory = dbfactory;
        }


        public IQueryable<UserExt> GetResellerList(int SiteID, string ResellerName, string Email, RecordStatus? eRecordStatus, DateTime dtRegisterFromDate, DateTime dtRegisterToDate, decimal dMinPrice, decimal dMaxPrice)
        {
            var resellerQuery = (from u in DbContext.Users
                                 from rc in DbContext.ResellerContracts
                                            .Where(r => r.UserID == u.ID)
                                            .DefaultIfEmpty()
                                 from c in DbContext.Contracts
                                            .Where(ct => ct.ID == rc.ContractID)
                                            .DefaultIfEmpty()
                                 from o in DbContext.Orders
                                            .Where(ord => ord.UserID == u.ID)
                                            .DefaultIfEmpty()
                                 from od in DbContext.OrderDetails
                                            .Where(odt => odt.OrderID == o.ID && odt.OrderStatusID != (int)OrderStatus.REJECTED && odt.OrderStatusID != (int?)OrderStatus.REFUNDED)
                                            .DefaultIfEmpty()
                                 where u.SiteID == SiteID && u.UserTypeID == (int)UserType.RESELLER && u.RecordStatusID != (int)RecordStatus.DELETED
                                        && (u.FirstName + " " + u.LastName).Contains((string.IsNullOrEmpty(ResellerName) ? u.FirstName : ResellerName))
                                        && u.Email.Contains((string.IsNullOrEmpty(Email) ? u.Email : Email))
                                        && u.RecordStatusID == (eRecordStatus == null ? u.RecordStatusID : (int?)eRecordStatus)
                                        && (System.Data.Entity.DbFunctions.DiffDays(dtRegisterFromDate, u.AuditDetails.DateCreated) >= 0 && System.Data.Entity.DbFunctions.DiffDays(u.AuditDetails.DateCreated, dtRegisterToDate) >= 0)
                                 group od by new { u.ID, u.FirstName, u.LastName, u.CompanyName, c.ContractName, u.RecordStatusID, u.AuditDetails.DateCreated } into g
                                 orderby g.Sum(od => od.Price) descending
                                 select new UserExt
                                 {
                                     ID = g.Key.ID,
                                     FirstName = g.Key.FirstName,
                                     LastName = g.Key.LastName,
                                     CompanyName = g.Key.CompanyName,
                                     ContractName = g.Key.ContractName,
                                     RegisterDate = g.Key.DateCreated,
                                     RecordStatusID = g.Key.RecordStatusID,
                                     TotalPurchase = g.Sum(od => (od.Price == null ? 0 : od.Price))
                                 }).Where(tp => tp.TotalPurchase >= dMinPrice && tp.TotalPurchase <= dMaxPrice);
            return resellerQuery;

        }

        public IQueryable<UserExt> GetCustomerList(int SiteID, string ResellerName, string Email)
        {
            var customerQuery = (from u in DbContext.Users
                                 from o in DbContext.Orders
                                            .Where(ord => ord.UserID == u.ID)
                                            .DefaultIfEmpty()
                                 from od in DbContext.OrderDetails
                                            .Where(odt => odt.OrderID == o.ID && odt.OrderStatusID != (int)OrderStatus.REJECTED && odt.OrderStatusID != (int)OrderStatus.REFUNDED)
                                            .DefaultIfEmpty()
                                 where u.SiteID == SiteID && u.UserTypeID == (int)UserType.CUSTOMER && u.RecordStatusID != (int)RecordStatus.DELETED
                                        && (u.FirstName + " " + u.LastName).Contains((string.IsNullOrEmpty(ResellerName) ? u.FirstName : ResellerName))
                                        && u.Email.Contains((string.IsNullOrEmpty(Email) ? u.Email : Email))                                 
                                 group od by new { u.ID, u.FirstName, u.LastName, u.CompanyName, u.RecordStatusID, u.AuditDetails.DateCreated } into g
                                 orderby g.Sum(od => od.Price) descending
                                 select new UserExt { ID = g.Key.ID, FirstName = g.Key.FirstName, LastName = g.Key.LastName, CompanyName = g.Key.CompanyName, RegisterDate = g.Key.DateCreated, RecordStatusID = g.Key.RecordStatusID, TotalPurchase = g.Sum(od => od.Price) });
            return customerQuery;

        }

        public AdminDashboard GetAdminDashBoard(int SiteID)
        {
            SqlParameter[] param = new SqlParameter[1];
            param[0] = new SqlParameter("SiteID", SiteID);
            return DbContext.Database.SqlQuery<AdminDashboard>("SSLAdmin_GetAdminDashBord @SiteID", param).FirstOrDefault();

        }

        public IQueryable<SearchOderInfo> SearchOrder(int SiteID, int InvoiceID, string OrderNumber, string DomainName, DateTime dtStart, DateTime dtEnd, int ProductID, OrderStatus? eOrderStatus, string AuthCode)
        {
            var searchorder = (from od in DbContext.OrderDetails
                               from cr in DbContext.CertificateRequests
                                        .Where(c => c.ID == od.CertificateRequestID)
                                        .DefaultIfEmpty()
                               from o in DbContext.Orders
                                        .Where(or => or.ID == od.OrderID)
                                        .DefaultIfEmpty()
                               from ut in DbContext.UserTransactions
                                        .Where(u => u.OrderDetailID == od.ID)
                                        .DefaultIfEmpty()
                               from p in DbContext.Payments
                                        .Where(py => py.ID == ut.PaymentID)
                                        .DefaultIfEmpty()
                               from gt in DbContext.GatewayInteractions
                                        .Where(g => g.ID == p.GatewayInteractionID)
                                        .DefaultIfEmpty()
                               where od.Order.SiteID == SiteID
                                        && od.OrderStatusID == (eOrderStatus == null ? od.OrderStatusID : (int?)eOrderStatus)
                                        && (System.Data.Entity.DbFunctions.DiffDays(dtStart, od.Order.OrderDate) >= 0 && System.Data.Entity.DbFunctions.DiffDays(od.Order.OrderDate, dtEnd) >= 0)
                                        && od.ProductID == (ProductID > 0 ? ProductID : od.ProductID)
                                        && od.CertificateRequest.DomainName.Contains(DomainName)
                                        && od.ExternalOrderID.Equals(string.IsNullOrEmpty(OrderNumber) ? od.ExternalOrderID : OrderNumber.Trim())
                                        && od.OrderID.Equals(InvoiceID.Equals(-2) ? od.OrderID : InvoiceID)
                                        && (gt.GatewayAuthCode ?? "-").Contains(string.IsNullOrEmpty(AuthCode) ? (gt.GatewayAuthCode ?? "-") : AuthCode.Trim())
                               orderby o.OrderDate descending
                               select new SearchOderInfo
                               {
                                   OrderDetailID = od.ID,
                                   OrderID = o.ID,
                                   OrderDate = o.OrderDate,
                                   OrderNumber = od.ExternalOrderID,
                                   ProductName = od.ProductName,
                                   DomainName = cr.DomainName,
                                   OrderStatusID = od.OrderStatusID,
                                   Price = od.Price
                               }
                              );

            return searchorder.Distinct();
        }

        ////Example of Query to call complex stored procedure. Please also remember that order must be correct and return type should
        ////be exactly the Entity Fields. 
        //public IEnumerable<Site> GetAvailableSites()
        //{
        //    return DbContext.Sites.SqlQuery("sp_custom", new object[] { "parameters", 1, DateTimeWithZone.Now });
        //}
    }

    public interface IWBRepository : IRepository<User>
    {
        IQueryable<UserExt> GetResellerList(int SiteID, string ResellerName, string Email, RecordStatus? eRecordStatus, DateTime dtRegisterFromDate, DateTime dtRegisterToDate, decimal dMinPrice, decimal dMaxPrice);
        IQueryable<UserExt> GetCustomerList(int SiteID, string ResellerName, string Email);        
        AdminDashboard GetAdminDashBoard(int SiteID);
        IQueryable<SearchOderInfo> SearchOrder(int SiteID, int InvoiceID, string OrderNumber, string DomainName, DateTime dtStart, DateTime dtEnd, int ProductID, OrderStatus? eOrderStatus, string AuthCode);
    }

}
