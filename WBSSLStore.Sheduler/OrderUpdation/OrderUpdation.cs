using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WBSSLStore.Domain;
using WBSSLStore.Data.Infrastructure;
using System.Data.Entity;

namespace WBSSLStore.Scheduler.OrderUpdation
{
    public class OrderUpdation : EFRepository<OrderDetail>
    {
        private Logger.Logger logger = null;

        private IDatabaseFactory _dbfactory;
        public OrderUpdation(IDatabaseFactory dbfactory)
            : base(dbfactory)
        {
            _dbfactory = dbfactory;
            logger = new Logger.Logger();
        }


        //public static void Main(string[] args)
        //{

        //    var obj = new WBSSLStore.Scheduler.OrderUpdation.OrderUpdation(new Data.Infrastructure.DatabaseFactory());
        //    obj.FetchApiOrders();
        //    //obj.FetAPIOrderBySite();

        //}
        public void FetAPIOrderBySite()
        {
            try
            {
                if (Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["isUpdateCustomSite"]))
                {
                    string siteids = System.Configuration.ConfigurationManager.AppSettings["CustomSiteid"].ToString();
                    var _cussite = siteids.Split(new char[] { ',' }).ToArray();
                    var Sites = (from s in DbContext.Sites
                                 where s.isActive == true && _cussite.Contains(s.ID.ToString())
                                 select s).ToList();
                    foreach (Site s in Sites)
                    {
                        try
                        {
                            logger.Log("-----Start Custom Site Order Updating For :" + s.Alias + "-----", Logger.LogType.STATUSUPDATE);


                            FetchOrderThruRESTAPI(s);


                            logger.Log("-----End Custom Site Order Updating For :" + s.Alias + "-----", Logger.LogType.STATUSUPDATE);
                        }
                        catch (Exception ex)
                        {
                            logger.LogException(ex);
                        }
                    }
                }

            }
            catch (Exception ex)
            { }
        }
        public void FetchApiOrders()
        {
            try
            {
              //  var rv = DbContext.OrderDetails.ToList().Where(x => x.ID.Equals(508)).FirstOrDefault();
                var Sites = (from s in DbContext.Sites
                             where s.isActive == true && s.ID == 276
                             select s).ToList();
                foreach (Site s in Sites)
                {
                    try
                    {
                        logger.Log("-----Start Order Updating For :" + s.Alias + "-----", Logger.LogType.STATUSUPDATE);


                        FetchOrderThruRESTAPI(s);


                        logger.Log("-----End Order Updating For :" + s.Alias + "-----", Logger.LogType.STATUSUPDATE);
                    }
                    catch (Exception ex)
                    {
                        logger.LogException(ex);
                    }
                }
            }
            catch (Exception e)
            { logger.LogException(e); }
        }

        private void FetchOrderThruRESTAPI(Site s)
        {
            WBSSLStore.Gateways.RestAPIModels.Request.OrderQueryRequest request = new Gateways.RestAPIModels.Request.OrderQueryRequest();
            request.AuthRequest = new WBSSLStore.Gateways.RestAPIModels.Request.AuthRequest();
            request.AuthRequest.PartnerCode = s.APIPartnerCode;
            request.AuthRequest.AuthToken = s.APIAuthToken;
            request.AuthRequest.UserAgent = s.Alias;
            request.AuthRequest.ReplayToken = "SSL Store WhiteBrand Sites : " + s.Alias;
            request.StartDate = DateTimeWithZone.Now.AddMonths(-3);
            request.EndDate = DateTimeWithZone.Now;
            request.ProductCode = "0";
            List<WBSSLStore.Gateways.RestAPIModels.Response.OrderResponse> ords = WBSSLStore.Gateways.RestAPIModels.Services.OrderService.GetOrderDeatilsByDateRange(request);

            if (ords != null && ords.Count > 0)
            {
                if (!ords[0].AuthResponse.isError)
                {
                    StringBuilder sbLogger = new StringBuilder();
                    string ID = string.Empty;
                    foreach (WBSSLStore.Gateways.RestAPIModels.Response.OrderResponse apiord in ords)
                    {
                        try
                        {
                            ID = apiord.TheSSLStoreOrderID.ToString();

                            OrderDetail orderdetail = DbContext.OrderDetails.Where(x => x.ExternalOrderID.Equals(ID)).Include(c => c.CertificateRequest).Include(o => o.CertificateRequest.AdminContact).Include(o => o.CertificateRequest.TechnicalContact).FirstOrDefault();

                            if (apiord != null && orderdetail != null && !string.IsNullOrEmpty(apiord.CommonName))
                            {



                                if (GetOrderDetailStatusValue(apiord.OrderStatus.MajorStatus).Equals(1))
                                {
                                    orderdetail.ActivatedDate = Convert.ToDateTime(apiord.CertificateStartDate);
                                    orderdetail.CertificateExpiresOn = Convert.ToDateTime(apiord.CertificateEndDate);
                                }

                                orderdetail.CertificateRequest.CertificateApproverEmail = apiord.ApproverEmail;
                                orderdetail.CertificateRequest.CountryText = apiord.Country;
                                orderdetail.CertificateRequest.DomainName = apiord.CommonName;
                                orderdetail.CertificateRequest.Locality = apiord.Locality;
                                orderdetail.CertificateRequest.Organisation = apiord.Organization;
                                orderdetail.CertificateRequest.OrganisationUnit = apiord.OrganizationalUnit;
                                orderdetail.CertificateRequest.State = apiord.State;
                                orderdetail.CertificateRequest.AddDomainNames = apiord.DNSNames;
                                orderdetail.OrderStatusID = GetOrderDetailStatusValue(apiord.OrderStatus.MajorStatus);

                                if (apiord.AdminContact != null)
                                {
                                    orderdetail.CertificateRequest.AdminContact.FirstName = apiord.AdminContact.FirstName;
                                    orderdetail.CertificateRequest.AdminContact.LastName = apiord.AdminContact.LastName;
                                    orderdetail.CertificateRequest.AdminContact.PhoneNumber = apiord.AdminContact.Phone;
                                    orderdetail.CertificateRequest.AdminContact.EmailAddress = apiord.AdminContact.Email;
                                    orderdetail.CertificateRequest.AdminContact.Title = apiord.AdminContact.Title;

                                }
                                if (apiord.TechnicalContact != null)
                                {
                                    orderdetail.CertificateRequest.TechnicalContact.FirstName = apiord.TechnicalContact.FirstName;
                                    orderdetail.CertificateRequest.TechnicalContact.LastName = apiord.TechnicalContact.LastName;
                                    orderdetail.CertificateRequest.TechnicalContact.PhoneNumber = apiord.TechnicalContact.Phone;
                                    orderdetail.CertificateRequest.TechnicalContact.EmailAddress = apiord.TechnicalContact.Email;
                                    orderdetail.CertificateRequest.TechnicalContact.Title = apiord.TechnicalContact.Title;

                                }

                                DbContext.OrderDetails.Attach(orderdetail);
                                sbLogger.Append(orderdetail.ID + ",");
                            }

                            DbContext.SaveChanges();
                        }
                        catch (Exception e)
                        { logger.LogException(e); }
                    }
                    logger.Log("Updated OrderDetailIDs are : " + sbLogger.ToString(), Logger.LogType.STATUSUPDATE);
                }
                else
                {
                    logger.Log(ords[0].AuthResponse.Message.ToString(), Logger.LogType.INFO);
                }
            }
        }
        // Added By <<MEHUL>> To call Both the API thessl and REST As per APIAuthToken

        public static int GetOrderDetailStatusValue(string Status)
        {
            switch (Status.ToLower())
            {
                case "pending":
                    return 0;
                case "active":
                case "valid":
                case "enrolled":
                case "complete":
                case "issued":
                case "start":
                case "completed":
                case "replaced":
                    return 1;
                case "cancelled":
                case "refunded":
                case "rejected":
                case "revoked":
                case "certgen_failed":
                case "order rejected":
                    return 2;
                case "expired":
                    return 3;
                default:
                    return 0;
            }
        }
    }

}
