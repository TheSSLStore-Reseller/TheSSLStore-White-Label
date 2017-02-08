using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Data.Repository;
using WBSSLStore.Domain;
using WBSSLStore.Service.ViewModels;
using System.Web;
using WBSSLStore.Gateway;
using WBSSLStore.Gateway.PayPal;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using System.Web.Mvc;
using WBSSLStore.Gateways.PaymentGateway.MoneyBookers;
using System.Data.Entity.Validation;

namespace WBSSLStore.Service
{
    public interface ICheckoutService
    {

        int AddUserandUpdateCart(User user, int CartID, int SiteID, int LangID, int smtpid, string adminEmail);
        bool PlaceOrder(CheckOutViewModel _viewModel, Site site, int SMTPId, int LangID, string AdminEmail, string InvoicePrefix);
        int UpdateShoppingCart(User user, int CartID, int SiteID, int ContractID, string AnonymousID);
        decimal GetCreditAmount(int UserID, int SiteID);
        List<PaymentGateways> GetPGInstances(int SiteID);
        void ProcessPayPalIPNRequest(Site site, int SMTPId, int LangID, string AdminEmail, string InvoicePrefix);
        void ProcessPayPalIPNRequestReIssue(Site Site, int SMTPId, int LangID, string AdminEmail, string InvoicePrefix);
        void ProcessMBIPNRequestReIssue(Site Site, int SMTPId, int LangID, string AdminEmail, string InvoicePrefix);
        int ProcessPayPalWaitRequest(Site site);
        int ProcessMBWaitRequest(Site site);
        void ProcessMBIPNRequest(Site site, int SMTPId, int LangID, string AdminEmail, string InvoicePrefix);
        string AddFund(CheckOutViewModel _viewModel, Site site);
        string AddFundAndReIssue(ReIssueViewModel _viewModel, Site site);
        void ReIssueAndUpdateOrderREST(OrderDetail model, decimal SANAmount, int NewAddedSAN);
        void UpdateDomainnames(OrderDetail model, string DNSNames);
    }
    public class CheckoutService : ICheckoutService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CheckOutRepository _repository;
        private readonly IRepository<PayPalData> _PayPalData;
        private readonly IEmailQueueService _EmailQueueService;
        private readonly Logger.ILogger logger;
        private readonly IRepository<OrderDetail> _OrderDetail;
        private readonly IRepository<CertificateRequest> _CertRequest;
        public string ProductsInfo = "<table><tr><td><strong>ProductName</strong></td><td><strong>Validity</strong></td><td><strong>No. of Server</strong></td></tr>[TABLEROW]</table>";
        public string TABLEROW = string.Empty;


        public CheckoutService(IUnitOfWork UnitOfWork, CheckOutRepository CheckOutRepository, IRepository<PayPalData> PayPalData, IEmailQueueService pEmailQueueService, Logger.ILogger _logger, IRepository<OrderDetail> orderDetail, IRepository<CertificateRequest> certRequest)
        {
            _unitOfWork = UnitOfWork;
            _repository = CheckOutRepository;
            _PayPalData = PayPalData;
            _EmailQueueService = pEmailQueueService;
            logger = _logger;
            _OrderDetail = orderDetail;
            _CertRequest = certRequest;
        }

        public decimal GetCreditAmount(int UserID, int SiteID)
        {
            return _repository.GetCreditAmount(UserID, SiteID);
        }
        public List<PaymentGateways> GetPGInstances(int SiteID)
        {
            return _repository.GetPGInstances(SiteID).ToList();
        }
        public PromoCode GetPromoRow(string code, int SiteID)
        {
            return _repository.GetPromoRow(code, SiteID);
        }

        public int UpdateShoppingCart(User user, int CartID, int SiteID, int ContractID, string AnonymousID)
        {
            return _repository.UpdateShoppingCart(user, CartID, SiteID, ContractID, AnonymousID);

        }
        public int AddUserandUpdateCart(User user, int CartID, int SiteID, int LangID, int smtpid, string adminEmail)
        {
            try
            {

                Audit audit = new Audit();
                audit.DateCreated = DateTimeWithZone.Now;
                audit.IP = System.Web.HttpContext.Current.Request.UserHostAddress;
                audit.HttpHeaderDump = HttpUtility.UrlDecode(System.Web.HttpContext.Current.Request.Headers.ToString());
                audit.DateModified = DateTimeWithZone.Now;

                user.Address.CompanyName = user.CompanyName;
                user.Address.Email = user.Email;
                user.UserType = UserType.CUSTOMER;
                user.RecordStatus = RecordStatus.ACTIVE;
                user.AuditDetails = audit;
                user.SiteID = SiteID;

                int result = _repository.AddUSerandUpdateCart(user, CartID);


                //To Do Sent Email TO Customer and Admin
                if (result.Equals(1))
                {
                    var _emailservice = DependencyResolver.Current.GetService<IEmailQueueService>();
                    _emailservice.PrepareEmailQueue(SiteID, LangID, EmailType.CUSTOMER_WELCOME_EMAIL, smtpid, user.Email, user);
                    _emailservice.PrepareEmailQueue(SiteID, LangID, EmailType.ADMIN_NEW_CUSTOMER, smtpid, adminEmail, user);
                    _unitOfWork.Commit();
                }
                //End
                return result;

            }
            catch (Exception ex)
            {
                logger.LogException(ex);

                return -1;
            }

        }

        public bool PlaceOrder(CheckOutViewModel _viewModel, Site site, int LangID, int SMTPId, string AdminEmail, string InvoicePrefix)
        {
            WBSSLStore.Gateway.PaymentGatewayInteraction gt = null;
            if (_viewModel.PayableAmount > 0)
            {
                gt = PaymentProcess(_viewModel, site, "Order");
            }

            return PlaceOrder(_viewModel, site, gt, SMTPId, LangID, AdminEmail, InvoicePrefix);
        }
        public string ProcessAddFund(CheckOutViewModel _viewModel, int SiteID, PaymentGatewayInteraction gt)
        {
            Payment _payment = null;
            UserTransaction _Ut = null;
            GatewayInteraction _GT = null;
            Audit audit = null;



            //Set Audit Details
            audit = new Audit();
            audit.DateCreated = System.DateTimeWithZone.Now;
            audit.IP = System.Web.HttpContext.Current.Request.UserHostAddress;
            audit.HttpHeaderDump = HttpUtility.UrlDecode(System.Web.HttpContext.Current.Request.Headers.ToString());
            audit.DateModified = DateTimeWithZone.Now;
            if (gt != null && gt.TransactionWrapper.isSuccess)
            {
                try
                {
                    _GT = new GatewayInteraction();
                    _GT.AuditDetails = audit;
                    _GT.GatewayAuthCode = gt.TransactionWrapper.GatewayAuthCode;
                    _GT.GatewayErrorCode = gt.TransactionWrapper.GatewayErrorCode;
                    _GT.GatewayRequest = gt.TransactionWrapper.GatewayRequest;
                    _GT.GatewayResponse = gt.TransactionWrapper.GatewayResponse;
                    _GT.isPaymentTransaction = gt.TransactionWrapper.isPaymentTransaction;
                    _GT.isSuccess = gt.TransactionWrapper.isSuccess;
                    _GT.SiteID = SiteID;

                    //Set Payment Row
                    _payment = new Payment();
                    _payment.AuditDetails = audit;
                    _payment.BillingAddress = _viewModel.user.Address;
                    _payment.CCNumber = gt.InParams.CCNumber.Length > 4 ? gt.InParams.CCNumber.Substring(gt.InParams.CCNumber.Length - 4, 4) : gt.InParams.CCNumber;
                    _payment.GatewayInteraction = _GT;
                    _payment.TransactionAmount = gt.TransactionWrapper.TransactionAmount;
                    _payment.PaymentModeID = gt.TransactionWrapper.PaymentModeID;

                    _Ut = new UserTransaction();

                    _Ut.AuditDetails = audit;
                    _Ut.Comment = "Fund added by " + _viewModel.user.FirstName + " " + _viewModel.user.LastName + " On " + DateTimeWithZone.Now.ToShortDateString();
                    _Ut.Payment = _payment;
                    _Ut.ReceipientInstrumentDetails = _Ut.Comment;
                    _Ut.SiteID = SiteID;
                    _Ut.TransactionAmount = _viewModel.PayableAmount;
                    _Ut.TransactionMode = TransactionMode.ADDFUND;
                    _Ut.TransactionModeID = (int)TransactionMode.ADDFUND;
                    _Ut.UserID = _viewModel.user.ID;
                    _repository.AddOrder(_Ut);
                    _unitOfWork.Commit();
                    _viewModel.OrderID = _Ut.ID;
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            else if (gt != null)
            {
                try
                {
                    _GT = new GatewayInteraction();
                    _GT.AuditDetails = audit;
                    _GT.GatewayAuthCode = gt.TransactionWrapper.GatewayAuthCode;
                    _GT.GatewayErrorCode = gt.TransactionWrapper.GatewayErrorCode;
                    _GT.GatewayRequest = gt.TransactionWrapper.GatewayRequest;
                    _GT.GatewayResponse = gt.TransactionWrapper.GatewayResponse;
                    _GT.isPaymentTransaction = gt.TransactionWrapper.isPaymentTransaction;
                    _GT.isSuccess = gt.TransactionWrapper.isSuccess;
                    _GT.SiteID = SiteID;
                    _repository.SaveGatewayIntraction(_GT);
                    return gt.TransactionWrapper.GatewayErrorCode;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            else
            {
                return "Unknown error.";
            }

        }
        public string ProcessReIssue(ReIssueViewModel _viewModel, int SiteID, PaymentGatewayInteraction gt)
        {
            Payment _payment = null;
            UserTransaction _Ut = null;
            GatewayInteraction _GT = null;
            Audit audit = null;

            //Set Audit Details
            audit = new Audit();
            audit.DateCreated = System.DateTimeWithZone.Now;
            audit.IP = System.Web.HttpContext.Current.Request.UserHostAddress;
            audit.HttpHeaderDump = HttpUtility.UrlDecode(System.Web.HttpContext.Current.Request.Headers.ToString());
            audit.DateModified = DateTimeWithZone.Now;
            if (gt != null && gt.TransactionWrapper.isSuccess)
            {
                try
                {
                    _viewModel.IsPaymentSuccess = gt.TransactionWrapper.isSuccess;
                    _GT = new GatewayInteraction();
                    _GT.AuditDetails = audit;
                    _GT.GatewayAuthCode = gt.TransactionWrapper.GatewayAuthCode;
                    _GT.GatewayErrorCode = gt.TransactionWrapper.GatewayErrorCode;
                    _GT.GatewayRequest = gt.TransactionWrapper.GatewayRequest;
                    _GT.GatewayResponse = gt.TransactionWrapper.GatewayResponse;
                    _GT.isPaymentTransaction = gt.TransactionWrapper.isPaymentTransaction;
                    _GT.isSuccess = gt.TransactionWrapper.isSuccess;
                    _GT.SiteID = SiteID;

                    //Set Payment Row
                    _payment = new Payment();
                    _payment.AuditDetails = audit;
                    _payment.BillingAddress = _viewModel.user.Address;
                    _payment.CCNumber = gt.InParams.CCNumber.Length > 4 ? gt.InParams.CCNumber.Substring(gt.InParams.CCNumber.Length - 4, 4) : gt.InParams.CCNumber;
                    _payment.GatewayInteraction = _GT;
                    _payment.TransactionAmount = gt.TransactionWrapper.TransactionAmount;
                    _payment.PaymentModeID = gt.TransactionWrapper.PaymentModeID;

                    _Ut = new UserTransaction();

                    _Ut.AuditDetails = audit;
                    _Ut.Comment = "Fund added by " + _viewModel.user.FirstName + " " + _viewModel.user.LastName + " On " + DateTimeWithZone.Now.ToShortDateString() + " For Add Additional SAN Via ReIssue";
                    _Ut.Payment = _payment;
                    _Ut.ReceipientInstrumentDetails = _Ut.Comment;
                    _Ut.SiteID = SiteID;
                    _Ut.TransactionAmount = _viewModel.PayableAmount;
                    _Ut.TransactionMode = TransactionMode.ADDFUND;
                    _Ut.TransactionModeID = (int)TransactionMode.ADDFUND;
                    _Ut.UserID = _viewModel.user.ID;
                    _repository.AddOrder(_Ut);
                    _unitOfWork.Commit();

                    OrderDetail objOrderDetail = _OrderDetail.Find(od => od.ID == _viewModel.OrderDetailID).EagerLoad(od => od.AuditDetails, od => od.CertificateRequest, od => od.Order, od => od.Order.Site, od => od.Order.User, od => od.Order.User.Address).FirstOrDefault();
                    ReIssueAndUpdateOrderREST(objOrderDetail, _viewModel.SANAmount, _viewModel.NewSANAdded);// ReIssueAndUpdateOrder(objOrderDetail, _viewModel.SANAmount, _viewModel.NewSANAdded);
                    _viewModel.OrderDetailID = _Ut.ID;
                    _viewModel.ReIssueUrl = "https://";
                    return _viewModel.ReIssueUrl;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            else if (gt != null)
            {
                try
                {
                    _GT = new GatewayInteraction();
                    _GT.AuditDetails = audit;
                    _GT.GatewayAuthCode = gt.TransactionWrapper.GatewayAuthCode;
                    _GT.GatewayErrorCode = gt.TransactionWrapper.GatewayErrorCode;
                    _GT.GatewayRequest = gt.TransactionWrapper.GatewayRequest;
                    _GT.GatewayResponse = gt.TransactionWrapper.GatewayResponse;
                    _GT.isPaymentTransaction = gt.TransactionWrapper.isPaymentTransaction;
                    _GT.isSuccess = gt.TransactionWrapper.isSuccess;
                    _GT.SiteID = SiteID;
                    _repository.SaveGatewayIntraction(_GT);
                    _viewModel.IsPaymentSuccess = false;
                    _viewModel.ReIssueUrl = string.Empty;
                    return gt.TransactionWrapper.GatewayErrorCode;
                }
                catch (Exception ex)
                {
                    _viewModel.IsPaymentSuccess = false;
                    return ex.Message;
                }
            }
            else
            {
                return "Unknown error.";
            }

        }
        public string AddFund(CheckOutViewModel _viewModel, Site site)
        {
            WBSSLStore.Gateway.PaymentGatewayInteraction gt = null;

            if (_viewModel.PayableAmount > 0)
            {
                if (_viewModel.paymentmode == WBSSLStore.Domain.PaymentMode.PAYPAL || _viewModel.paymentmode == Domain.PaymentMode.MONEYBOOKERS)
                {
                    var PGInstance = _repository.GetPGInstances(site.ID);
                    PaymentGateways PG;
                    if (_viewModel.paymentmode == WBSSLStore.Domain.PaymentMode.PAYPAL)
                    {
                        PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.PayPalIPN).FirstOrDefault();
                        HttpContext.Current.Response.Redirect(GetOrRedirectPayPalUrl(_viewModel, site, "AddFund", PG, WBSSLStore.Domain.PaymentMode.PAYPAL), true);
                    }
                    else if (_viewModel.paymentmode == Domain.PaymentMode.MONEYBOOKERS)
                    {
                        PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.Moneybookers).FirstOrDefault();
                        HttpContext.Current.Response.Redirect(GetOrRedirectPayPalUrl(_viewModel, site, "AddFund", PG, WBSSLStore.Domain.PaymentMode.MONEYBOOKERS));
                    }
                }
                else
                    gt = PaymentProcess(_viewModel, site, "AddFund");
            }
            return ProcessAddFund(_viewModel, site.ID, gt);
        }
        public string AddFundAndReIssue(ReIssueViewModel _viewModel, Site site)
        {
            WBSSLStore.Gateway.PaymentGatewayInteraction gt = null;
            if (_viewModel.PayableAmount > 0)
            {
                if (_viewModel.paymentmode == WBSSLStore.Domain.PaymentMode.PAYPAL)
                {
                    var PGInstance = _repository.GetPGInstances(site.ID);
                    PaymentGateways PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.PayPalIPN).FirstOrDefault();
                    HttpContext.Current.Response.Redirect(GetReIssueRedirectPayPalUrl(_viewModel, site, "ReIssueAddFund", PG, Domain.PaymentMode.PAYPAL), true);
                }
                else if (_viewModel.paymentmode == WBSSLStore.Domain.PaymentMode.MONEYBOOKERS)
                {
                    var PGInstance = _repository.GetPGInstances(site.ID);
                    PaymentGateways PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.Moneybookers).FirstOrDefault();
                    HttpContext.Current.Response.Redirect(GetReIssueRedirectPayPalUrl(_viewModel, site, "ReIssueAddFund", PG, Domain.PaymentMode.MONEYBOOKERS), true);
                }
                else
                    gt = ReIssuePaymentProcess(_viewModel, site, "ReIssueAddFund");
            }
            return ProcessReIssue(_viewModel, site.ID, gt);
        }
        private bool PlaceOrder(CheckOutViewModel _viewModel, Site site, WBSSLStore.Gateway.PaymentGatewayInteraction gt, int SMTPId, int LangID, string AdminEmail, string InvoicePrefix)
        {
            bool result = false;
            Payment payment = null;
            GatewayInteraction GT = null;

            try
            {
                if (_viewModel.OrderAmount >= 0)
                {

                    Audit audit = null;
                    OrderDetail ordDetail = null;
                    PromoCode promo = null;
                    UserTransaction UT = null;
                    Order order = null;


                    //Set Audit Details
                    audit = new Audit();
                    audit.DateCreated = System.DateTimeWithZone.Now;
                    audit.IP = System.Web.HttpContext.Current.Request.UserHostAddress;
                    audit.HttpHeaderDump = HttpUtility.UrlDecode(System.Web.HttpContext.Current.Request.Headers.ToString());
                    audit.DateModified = DateTimeWithZone.Now;


                    if (_viewModel.PayableAmount > 0)
                    {
                        logger.Log(gt.TransactionWrapper.GatewayErrorCode + "Kaushal", Logger.LogType.INFO);
                        if (gt != null && gt.TransactionWrapper.isSuccess)
                        {

                            //Set GatewayIntractionRow
                            GT = new GatewayInteraction();
                            GT.isPaymentTransaction = gt.TransactionWrapper.isPaymentTransaction;
                            GT.isSuccess = gt.TransactionWrapper.isSuccess;
                            GT.SiteID = site.ID;
                            GT.GatewayResponse = gt.TransactionWrapper.GatewayResponse;
                            GT.GatewayRequest = gt.TransactionWrapper.GatewayRequest;
                            GT.GatewayErrorCode = gt.TransactionWrapper.GatewayErrorCode;
                            GT.GatewayAuthCode = gt.TransactionWrapper.GatewayAuthCode;
                            GT.AuditDetails = audit;

                            //Set Payment Row
                            payment = new Payment();
                            payment.AuditDetails = audit;
                            payment.BillingAddress = _viewModel.user.Address;
                            payment.CCNumber = gt.InParams.CCNumber.Length > 4 ? gt.InParams.CCNumber.Substring(gt.InParams.CCNumber.Length - 4, 4) : gt.InParams.CCNumber;
                            payment.GatewayInteraction = GT;
                            payment.TransactionAmount = gt.TransactionWrapper.TransactionAmount;
                            payment.PaymentModeID = gt.TransactionWrapper.PaymentModeID;

                            _unitOfWork.Commit();
                        }
                        else if (gt != null)
                        {
                            _viewModel.Errormsg = gt.TransactionWrapper.GatewayErrorCode;
                            GT = new GatewayInteraction();
                            GT.isPaymentTransaction = gt.TransactionWrapper.isPaymentTransaction;
                            GT.isSuccess = gt.TransactionWrapper.isSuccess;
                            GT.SiteID = site.ID;
                            GT.GatewayResponse = gt.TransactionWrapper.GatewayResponse;
                            GT.GatewayRequest = gt.TransactionWrapper.GatewayRequest;
                            GT.GatewayErrorCode = gt.TransactionWrapper.GatewayErrorCode;
                            GT.GatewayAuthCode = gt.TransactionWrapper.GatewayAuthCode;
                            GT.AuditDetails = audit;

                            _repository.SaveGatewayIntraction(GT);

                            return false;
                        }
                        else
                        {
                            _viewModel.Errormsg = "Error during order process. Please try again !";
                            return false;
                        }
                    }

                    //Set Order Row
                    order = new Order();
                    order.AuditDetails = audit;
                    order.BillingAddress = _viewModel.user.Address;
                    order.OrderDate = System.DateTimeWithZone.Now;

                    if (gt != null && gt.StatusCode.Equals(4))
                        order.OrderStatus = PymentProcessStatus.FRAUDDETECTION;
                    else if (gt != null && gt.StatusCode.Equals(1))
                        order.OrderStatus = PymentProcessStatus.COMPLETE;
                    else if (_viewModel.PayableAmount <= 0)
                        order.OrderStatus = PymentProcessStatus.COMPLETE;

                    order.SiteID = site.ID;
                    order.TotalPrice = (_viewModel.OrderAmount + _viewModel.Tax) - _viewModel.PromoDiscount;
                    order.UserID = _viewModel.user.ID;

                    order.VATNumber = _viewModel.VATNumber;




                    //Set Order Details and UserTransaction Row
                    var cartdeatil = _repository.GetCart(_viewModel.ShoppingCartID, site.ID).ToList();


                    decimal credit = _viewModel.AvailableCredit;

                    foreach (ShoppingCartDetail s in cartdeatil)
                    {

                        ordDetail = new OrderDetail();

                        ordDetail.AuditDetails = audit;
                        ordDetail.NumberOfMonths = s.ProductPricing.NumberOfMonths;
                        ordDetail.Order = order;
                        ordDetail.OrderStatus = OrderStatus.PENDING;

                        ordDetail.ProductID = s.ProductID;
                        ordDetail.ProductName = s.Product.ProductName;
                        if (!string.IsNullOrEmpty(s.PromoCode))
                        {
                            promo = this.GetPromoRow(s.PromoCode, site.ID);
                            if (promo != null)
                            {
                                ordDetail.PromoCodeID = promo.ID;
                                ordDetail.PromoDiscount = s.PromoDiscount;
                            }
                        }

                        ordDetail.VATAmount = (_viewModel.ISVatApplicable && _viewModel.VatPercent > 0) ? CalculateTax((s.Price - s.PromoDiscount), _viewModel.VatCountry, order.BillingAddress.CountryID, _viewModel.VatPercent) : 0;

                        ordDetail.Price = (s.Price + ordDetail.VATAmount) - s.PromoDiscount; //

                        ordDetail.GrossAmount = s.Price;

                        ordDetail.CertificateRequest = new CertificateRequest
                        {
                            SpecialInstructions = s.Comment,
                            NumberOfMonths = s.ProductPricing.NumberOfMonths,
                            ProductName = s.Product.ProductName,
                            AdditionalDomains = s.AdditionalDomains,
                            AdminContact = new CertificateContact() { EmailAddress = "", FirstName = "", LastName = "", PhoneNumber = "", Title = "" },
                            BillingContact = new CertificateContact() { EmailAddress = "", FirstName = "", LastName = "", PhoneNumber = "", Title = "" },
                            IsCompetitiveUpgrade = s.IsCompetitiveUpgrade,
                            isNewOrder = s.isNewOrder,
                            NumberOfServers = s.NumberOfServers.Equals(0) ? 1 : s.NumberOfServers,
                            TechnicalContact = new CertificateContact() { EmailAddress = "", FirstName = "", LastName = "", PhoneNumber = "", Title = "" },
                            WebServerID = 1,
                            CSR = string.Empty,
                            DomainName = string.Empty,
                            CertificateApproverEmail = string.Empty

                        };


                        ordDetail.ActivatedDate = SettingConstants.defaultDate;
                        ordDetail.CertificateExpiresOn = SettingConstants.defaultDate;

                        GenerateLinkThruRESTAPI(s, audit, _viewModel, ordDetail, site);


                        if (credit >= 0 && credit >= ordDetail.Price) // Full Credit is available====== Starts Here ==================
                        {
                            UT = new UserTransaction();
                            UT.AuditDetails = audit;
                            UT.Comment = "Use Full Credit";
                            UT.OrderDetail = ordDetail;
                            UT.SiteID = site.ID;
                            UT.UserID = _viewModel.user.ID;
                            UT.TransactionMode = TransactionMode.ORDER;
                            UT.TransactionAmount = (-1) * (ordDetail.Price);
                            UT.PaymentID = null;

                            _repository.AddOrder(UT);

                            credit = credit - (ordDetail.Price);
                        }
                        else if (credit > 0 && credit < (ordDetail.Price)) // Partial Credit Used
                        {
                            //First Entry For Credit Used
                            UT = new UserTransaction();
                            UT.AuditDetails = audit;
                            UT.Comment = "Use Partial Credit";
                            UT.OrderDetail = ordDetail;
                            UT.SiteID = site.ID;
                            UT.TransactionMode = TransactionMode.ORDER;
                            UT.TransactionAmount = credit * -1;
                            UT.UserID = _viewModel.user.ID;
                            UT.PaymentID = null;

                            _repository.AddOrder(UT);

                            //Second Entry For Payment with Payment Gateway
                            UT = new UserTransaction();
                            UT.AuditDetails = audit;
                            UT.Comment = "Payment Made";
                            UT.OrderDetail = ordDetail;
                            UT.Payment = payment;
                            UT.SiteID = site.ID;
                            UT.UserID = _viewModel.user.ID;
                            UT.TransactionMode = TransactionMode.ORDER;
                            UT.TransactionAmount = 0;
                            _repository.AddOrder(UT);

                            credit = 0;
                        }
                        else if (credit == 0 && ordDetail.Price > 0) // No credit used
                        {
                            UT = new UserTransaction();
                            UT.AuditDetails = audit;
                            UT.Comment = "Full Payment Made";
                            UT.OrderDetail = ordDetail;
                            UT.Payment = payment;
                            UT.SiteID = site.ID;
                            UT.UserID = _viewModel.user.ID;
                            UT.TransactionMode = TransactionMode.ORDER;
                            UT.TransactionAmount = 0;
                            _repository.AddOrder(UT);

                        }

                        //generate table html for invoice
                        TABLEROW += "<tr><td>" + s.Product.ProductName + "</td><td>" + s.ProductPricing.NumberOfMonths.ToString() + "</td><td>" + CurrentSiteSettings.GetNumberOfServers(s.Product, s.NumberOfServers) + "</td></tr>";
                    }


                    //Save order in Database
                    _unitOfWork.Commit();

                    result = true;
                    //Delete Shopping Cart Details
                    _repository.DeleteCartDetails(cartdeatil);

                    logger.Log(" Delete Cart=" + order.ID, Logger.LogType.INFO);
                    //End

                    int OrderID = order.ID;
                    _viewModel.OrderID = order.ID;

                    _viewModel.AllProductInfo = ProductsInfo.Replace("[TABLEROW]", TABLEROW);
                    _viewModel.InvoiceNumber = InvoicePrefix + _viewModel.OrderID;


                    //send Mail to Support For pending Order 
                    if (order.OrderStatus == PymentProcessStatus.FRAUDDETECTION)
                    {
                        _EmailQueueService.PrepareEmailQueue(site.ID, LangID, EmailType.SUPPORT_FRAUDORDER_NOTIFICATION, SMTPId, AdminEmail, gt.TransactionWrapper.GatewayAuthCode);

                    }

                    //End

                    //Add Order Email In Queue
                    if (_viewModel.user.UserType == UserType.RESELLER)
                        _EmailQueueService.PrepareEmailQueue(site.ID, LangID, EmailType.RESELLER_NEW_ORDER_EMAIL, SMTPId, _viewModel.user.Email, _viewModel);
                    else if (_viewModel.user.UserType == UserType.CUSTOMER)
                        _EmailQueueService.PrepareEmailQueue(site.ID, LangID, EmailType.CUSTOMER_NEW_ORDER, SMTPId, _viewModel.user.Email, _viewModel);

                    _EmailQueueService.PrepareEmailQueue(site.ID, LangID, EmailType.ADMIN_NEW_ORDER, SMTPId, AdminEmail, _viewModel);

                    _unitOfWork.Commit();
                    logger.Log(" EMail Queued Successfully", Logger.LogType.INFO);

                    result = true;

                }                
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {

                var errorMessages = ex.Entries.Select((x) => new { en = x.Entity.GetType().Name }).Select(x => x.en).ToArray();
                var fullErrorMessage = string.Join("; ", errorMessages);
                logger.Log(string.Format("Error During PlaceOrder-1 DbUpdateException : DateTime : {0} and UserID: {1}, Error message: {2} ", DateTime.Now, _viewModel.user.ID, fullErrorMessage), Logger.LogType.CRITICAL);                
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);

                // Join the list to a single string.
                var fullErrorMessage = string.Join("; ", errorMessages);

                // Combine the original exception message with the new one.
                var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                // Throw a new DbEntityValidationException with the improved exception message.
                logger.Log(string.Format("Error During PlaceOrder-2 DbEntityValidationException :  DateTime : {0} and UserID: {1}, Error message: {2} ", DateTime.Now, _viewModel.user.ID, fullErrorMessage), Logger.LogType.CRITICAL);
                //throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
            }                 
            catch (Exception ex)
            {
                _viewModel.Errormsg = ex.Message;

                logger.Log(string.Format("Error During PlaceOrder. DateTime : {0} and UserID: {1} and GatewayInteractionID: {2} ", DateTime.Now, _viewModel.user.ID, GT != null ? GT.ID : 0), Logger.LogType.CRITICAL);
                logger.LogException(ex);
                throw ex;

            }

            return result;

        }
        private decimal CalculateTax(decimal price, int VatCountryID, int CountryID, int vatpercent)
        {

            if (CountryID.Equals(VatCountryID))
            {

                if (vatpercent > 0)
                {
                    return (price * vatpercent) / 100;
                }

            }

            return 0;
        }

        private void GenerateLinkThruRESTAPI(ShoppingCartDetail s, Audit audit, CheckOutViewModel _viewModel, OrderDetail ordDetail, Site site)
        {
            try
            {

                WBSSLStore.Gateways.RestAPIModels.Response.OrderResponse objLink = new WBSSLStore.Gateways.RestAPIModels.Response.OrderResponse();
                WBSSLStore.Gateways.RestAPIModels.Request.TinyOrderRequest inviteReq = new Gateways.RestAPIModels.Request.TinyOrderRequest();
                inviteReq.AuthRequest = new Gateways.RestAPIModels.Request.AuthRequest();
                inviteReq.AuthRequest.AuthToken = site.APIAuthToken;
                inviteReq.AuthRequest.PartnerCode = site.APIPartnerCode;
                inviteReq.AuthRequest.UserAgent = site.Alias;
                inviteReq.AuthRequest.ReplayToken = DateTime.Now.ToLongDateString();
                inviteReq.ExtraSAN = s.AdditionalDomains;
                inviteReq.ProductCode = s.Product.InternalProductCode;
                inviteReq.RequestorEmail = _viewModel.user.Email;
                inviteReq.ServerCount = s.NumberOfServers;
                inviteReq.ValidityPeriod = s.ProductPricing.NumberOfMonths;
                inviteReq.AddInstallationSupport = false;
                inviteReq.CustomOrderID = string.Empty;
                inviteReq.EmailLanguageCode = "en";
                inviteReq.ExtraProductCode = string.Empty;
                inviteReq.PreferVendorLink = false;

                objLink = WBSSLStore.Gateways.RestAPIModels.Services.OrderService.InviteOrderRequest(inviteReq);


                if (objLink != null)
                {
                    GatewayInteraction GTSSLAPI = new GatewayInteraction();

                    GTSSLAPI.AuditDetails = audit;

                    if (!objLink.AuthResponse.isError)
                    {
                        ordDetail.ExternalOrderID = objLink.TheSSLStoreOrderID;
                        ordDetail.Cost = 0;
                        GTSSLAPI.GatewayAuthCode = objLink.TinyOrderLink;
                        GTSSLAPI.isSuccess = true;

                        logger.Log("Order's Centarl Api Link Generated successfully through API : Link=" + objLink.TinyOrderLink, Logger.LogType.INFO);

                    }
                    else
                    {
                        ordDetail.ExternalOrderID = string.Empty;
                        GTSSLAPI.GatewayAuthCode = string.Empty;
                        GTSSLAPI.isSuccess = false;

                        logger.Log("Order's Centarl Api Link Nor Generates through API : Error Maessagee=" + objLink.AuthResponse.Message, Logger.LogType.ERROR);
                    }

                    //GTSSLAPI.GatewayRequest = objLink.;
                    GTSSLAPI.GatewayResponse = objLink.AuthResponse.isError ? objLink.AuthResponse.Message[0] : objLink.AuthResponse.ToString();
                    //GTSSLAPI.GatewayErrorCode = objLink.AuthResponse.isError ? objLink.AuthResponse. : string.Empty;
                    GTSSLAPI.isPaymentTransaction = false;
                    GTSSLAPI.SiteID = site.ID;

                    ordDetail.StoreAPIInteraction = GTSSLAPI;
                }
            }
            catch
            {
            }
        }
        // Ended By <<MEHUL>>
        public void ProcessPayPalIPNRequest(Site Site, int SMTPId, int LangID, string AdminEmail, string InvoicePrefix)
        {
            var PGInstance = _repository.GetPGInstances(Site.ID);
            PaymentGateways PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.PayPalIPN).FirstOrDefault();

            PayPalResultData objResult = null;
            objResult = PayPalHandler.Results(PG.IsTestMode);
            string SessionID = HttpContext.Current.Request.QueryString[SettingConstants.PAYPAL_SESSION_KEY].ToString();
            int UserID = Convert.ToInt32(HttpContext.Current.Request.QueryString[SettingConstants.QS_USERID]);
            string PaymentType = HttpContext.Current.Request.QueryString[SettingConstants.PAYPAL_PAYMENTTYPE].ToString();
            if (objResult == null)
            {
                logger.Log(" Paypal IPN  Notify Result. Result Is Null", Logger.LogType.INFO);
                throw new Exception("Result is Null");


            }

            logger.Log(" Paypal IPN  Notify Result. Amount Validation Status  :" + objResult.Valid.ToString(), Logger.LogType.INFO);

            if (objResult.Valid)
            {

                CheckOutViewModel _viewModel = null;
                _viewModel = DeSerializeNow(SessionID, _viewModel, Site.ID, PaymentType);
                if (_viewModel != null && _viewModel.user.ID == UserID)
                {
                    PaymentGatewayInteraction gt = new PaymentGatewayInteraction();

                    gt.InParams = new GatewayInParameter();
                    gt.InParams.TransactionAmount = _viewModel.PayableAmount;
                    gt.InParams.CCNumber = "";
                    gt.TransactionWrapper.GatewayAuthCode = objResult.TransactionID;
                    gt.TransactionWrapper.GatewayRequest = _viewModel.PayPalID + "|" + objResult.Custom;
                    gt.TransactionWrapper.GatewayResponse = objResult.TransactionID + "|" + objResult.Valid + "|" + objResult.Amount + "|" + objResult.TransactionType + "|" + objResult.Custom + "|" + objResult.Name + "|" + objResult.ItemNumber + "|" + objResult.ReceiverEmail + "|" + objResult.email;
                    gt.TransactionWrapper.isPaymentTransaction = true;
                    gt.TransactionWrapper.TransactionAmount = objResult.Amount;
                    gt.TransactionWrapper.statusCode = objResult.Valid ? "1" : "0";
                    gt.TransactionWrapper.GatewayErrorCode = objResult.Valid ? "Success" : "Error during paypal transaction";
                    gt.TransactionWrapper.PaymentModeID = (int)WBSSLStore.Domain.PaymentMode.PAYPAL;
                    gt.TransactionWrapper.isSuccess = true;
                    _viewModel.IsPaymentSuccess = true;

                    //var repo1 = DependencyResolver.Current.GetService<IRepository < GatewayInteraction> >();
                    //repo1.Add(gt.TransactionWrapper  
                    if (PaymentType.ToLower().Equals("order"))
                    {
                        PlaceOrder(_viewModel, Site, gt, SMTPId, LangID, AdminEmail, InvoicePrefix);
                        logger.Log(" Paypal IPN  Place Order Success  :", Logger.LogType.INFO);
                    }
                    else if (PaymentType.ToLower().Equals("addfund"))
                    {

                        ProcessAddFund(_viewModel, Site.ID, gt);
                        logger.Log(" Paypal IPN  ProcessAddFund Success  :", Logger.LogType.INFO);
                    }

                    _viewModel.IsPaymentSuccess = true;
                    SerializeNow(SessionID, _viewModel, Site.ID, PaymentType);

                }
            }
            else
            {
                CheckOutViewModel _viewModel = null;
                _viewModel = DeSerializeNow(SessionID, _viewModel, Site.ID, PaymentType);
                _viewModel.IsPaymentSuccess = false;
                SerializeNow(SessionID, _viewModel, Site.ID, PaymentType);
            }
        }
        public void ProcessPayPalIPNRequestReIssue(Site Site, int SMTPId, int LangID, string AdminEmail, string InvoicePrefix)
        {
            var PGInstance = _repository.GetPGInstances(Site.ID);
            PaymentGateways PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.PayPalIPN).FirstOrDefault();

            PayPalResultData objResult = null;
            objResult = PayPalHandler.Results(PG.IsTestMode);
            string SessionID = HttpContext.Current.Request.QueryString[SettingConstants.PAYPAL_SESSION_KEY].ToString();
            int UserID = Convert.ToInt32(HttpContext.Current.Request.QueryString[SettingConstants.QS_USERID]);
            string PaymentType = HttpContext.Current.Request.QueryString[SettingConstants.PAYPAL_PAYMENTTYPE].ToString();
            if (objResult == null)
            {
                throw new Exception("Result is Null");
            }

            if (objResult.Valid)
            {
                ReIssueViewModel _viewModel = null;
                _viewModel = DeSerializeNow(SessionID, _viewModel, Site.ID, PaymentType);
                if (_viewModel != null && _viewModel.user.ID == UserID)
                {
                    PaymentGatewayInteraction gt = new PaymentGatewayInteraction();

                    gt.InParams = new GatewayInParameter();
                    gt.InParams.TransactionAmount = _viewModel.PayableAmount;
                    gt.InParams.CCNumber = "";
                    gt.TransactionWrapper.GatewayAuthCode = objResult.TransactionID;
                    gt.TransactionWrapper.GatewayRequest = _viewModel.PayPalID + "|" + objResult.Custom;
                    gt.TransactionWrapper.GatewayResponse = objResult.TransactionID + "|" + objResult.Valid + "|" + objResult.Amount + "|" + objResult.TransactionType + "|" + objResult.Custom + "|" + objResult.Name + "|" + objResult.ItemNumber + "|" + objResult.ReceiverEmail + "|" + objResult.email;
                    gt.TransactionWrapper.isPaymentTransaction = true;
                    gt.TransactionWrapper.TransactionAmount = objResult.Amount;
                    gt.TransactionWrapper.statusCode = objResult.Valid ? "1" : "0";
                    gt.TransactionWrapper.GatewayErrorCode = objResult.Valid ? "" : "Error during paypal transaction";
                    gt.TransactionWrapper.PaymentModeID = (int)WBSSLStore.Domain.PaymentMode.PAYPAL;
                    gt.TransactionWrapper.isSuccess = true;
                    _viewModel.IsPaymentSuccess = true;
                    ProcessReIssue(_viewModel, Site.ID, gt);
                    _viewModel.IsPaymentSuccess = true;
                    SerializeNow(SessionID, _viewModel, Site.ID, PaymentType);
                }
            }
            else
            {
                ReIssueViewModel _viewModel = null;
                _viewModel = DeSerializeNow(SessionID, _viewModel, Site.ID, PaymentType);
                _viewModel.IsPaymentSuccess = false;
                SerializeNow(SessionID, _viewModel, Site.ID, PaymentType);
            }
        }
        public int ProcessPayPalWaitRequest(Site site)
        {
            string SessionID = HttpContext.Current.Request.QueryString[SettingConstants.PAYPAL_SESSION_KEY].ToString();
            int UserID = Convert.ToInt32(HttpContext.Current.Request.QueryString[SettingConstants.QS_USERID]);
            string PaymentType = HttpContext.Current.Request.QueryString[SettingConstants.PAYPAL_PAYMENTTYPE].ToString();
            if (PaymentType.ToLower().Equals("reissueaddfund"))
            {
                ReIssueViewModel SessionData = null;
                SessionData = DeSerializeNow(SessionID, SessionData, site.ID, PaymentType);
                if (SessionData != null && SessionData.IsPaymentSuccess)
                {
                    if (!string.IsNullOrEmpty(SessionData.ReIssueUrl) && (SessionData.ReIssueUrl.ToLower().StartsWith("http://") || SessionData.ReIssueUrl.ToLower().StartsWith("https://")))
                    {
                        System.Web.HttpContext.Current.Response.Redirect("/entercsr/reissue/" + SessionData.OrderDetailID, true);
                        return 0;
                    }
                    else
                    {
                        logger.Log(SessionData.ReIssueUrl, Logger.LogType.CRITICAL);
                        return -99;
                    }
                }
                else
                    return 0;
            }
            else
            {
                CheckOutViewModel SessionData = null;
                SessionData = DeSerializeNow(SessionID, SessionData, site.ID, PaymentType);
                if (SessionData != null && SessionData.IsPaymentSuccess)
                {
                    if (PaymentType.ToLower().Equals("order") && SessionData.OrderID > 0)
                    {

                        return SessionData.OrderID;

                    }
                    else if (PaymentType.ToLower().Equals("addfund") && SessionData.OrderID > 0)
                    {
                        return -1;
                    }
                    else
                    {
                        // Return -99 for Make Second Request for Paypal Wait

                        return -99;
                    }
                }
                else
                {
                    return 0;

                }
            }
        }

        public void ProcessMBIPNRequest(Site Site, int SMTPId, int LangID, string AdminEmail, string InvoicePrefix)
        {
            var PGInstance = _repository.GetPGInstances(Site.ID);
            PaymentGateways PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.Moneybookers).FirstOrDefault();

            MBResultData objResult = MBHandler.Results(PG.Password);
            string SessionID = HttpContext.Current.Request.QueryString[SettingConstants.PAYPAL_SESSION_KEY].ToString();
            int UserID = Convert.ToInt32(HttpContext.Current.Request.QueryString[SettingConstants.QS_USERID]);
            string PaymentType = HttpContext.Current.Request.QueryString[SettingConstants.PAYPAL_PAYMENTTYPE].ToString();
            if (objResult == null)
            {
                logger.Log("Moneybookers IPN  Notify Result. Result Is Null", Logger.LogType.INFO);
                throw new Exception("Result is Null");
            }

            logger.Log("Moneybookers IPN  result. Result Status  :" + objResult.Status, Logger.LogType.INFO);

            if (objResult.Status == MBResponseStatus.Processed)
            {
                CheckOutViewModel _viewModel = null;
                _viewModel = DeSerializeNow(SessionID, _viewModel, Site.ID, PaymentType);
                if (_viewModel != null && _viewModel.user.ID == UserID)
                {
                    PaymentGatewayInteraction gt = new PaymentGatewayInteraction();

                    gt.InParams = new GatewayInParameter();
                    gt.InParams.TransactionAmount = _viewModel.PayableAmount;
                    gt.InParams.CCNumber = "";
                    gt.TransactionWrapper.GatewayAuthCode = objResult.MBTransactionID;
                    gt.TransactionWrapper.GatewayRequest = objResult.PayToEmail;
                    gt.TransactionWrapper.GatewayResponse = objResult.PayFromEmail + "|" + objResult.Status + "|" + "|" + objResult.FailedReasonCode + "|" + objResult.Amount + "|" + objResult.Currency + "|" + objResult.CustomerID + "|" + objResult.MBAmount + "|" + objResult.MBCurrency + "|" + objResult.MD5sig + "|" + objResult.MerchantID + "|" + objResult.TransactionID;
                    gt.TransactionWrapper.isPaymentTransaction = true;
                    gt.TransactionWrapper.TransactionAmount = objResult.Amount;
                    gt.TransactionWrapper.statusCode = (objResult.Status == MBResponseStatus.Processed ? "1" : "0");
                    gt.TransactionWrapper.GatewayErrorCode = (objResult.Status == MBResponseStatus.Processed ? "Success" : "Error during Moneybookers transaction");
                    gt.TransactionWrapper.PaymentModeID = (int)WBSSLStore.Domain.PaymentMode.MONEYBOOKERS;
                    gt.TransactionWrapper.isSuccess = true;
                    _viewModel.IsPaymentSuccess = true;

                    //var repo1 = DependencyResolver.Current.GetService<IRepository < GatewayInteraction> >();
                    //repo1.Add(gt.TransactionWrapper  
                    if (PaymentType.ToLower().Equals("order"))
                    {
                        PlaceOrder(_viewModel, Site, gt, SMTPId, LangID, AdminEmail, InvoicePrefix);
                        logger.Log("Moneybookers IPN  Place Order Success  :", Logger.LogType.INFO);
                    }
                    else if (PaymentType.ToLower().Equals("addfund"))
                    {

                        ProcessAddFund(_viewModel, Site.ID, gt);
                        logger.Log("Moneybookers IPN  ProcessAddFund Success  :", Logger.LogType.INFO);
                    }
                    _viewModel.IsPaymentSuccess = true;
                    SerializeNow(SessionID, _viewModel, Site.ID, PaymentType);
                }
            }
            else
            {
                CheckOutViewModel _viewModel = null;
                _viewModel = DeSerializeNow(SessionID, _viewModel, Site.ID, PaymentType);
                _viewModel.IsPaymentSuccess = false;
                SerializeNow(SessionID, _viewModel, Site.ID, PaymentType);
            }
        }
        public void ProcessMBIPNRequestReIssue(Site Site, int SMTPId, int LangID, string AdminEmail, string InvoicePrefix)
        {
            var PGInstance = _repository.GetPGInstances(Site.ID);
            PaymentGateways PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.Moneybookers).FirstOrDefault();

            MBResultData objResult = MBHandler.Results(PG.Password);
            string SessionID = HttpContext.Current.Request.QueryString[SettingConstants.PAYPAL_SESSION_KEY].ToString();
            int UserID = Convert.ToInt32(HttpContext.Current.Request.QueryString[SettingConstants.QS_USERID]);
            string PaymentType = HttpContext.Current.Request.QueryString[SettingConstants.PAYPAL_PAYMENTTYPE].ToString();
            if (objResult == null)
            {
                throw new Exception("ReIssue MoneybookersIPN Result is Null");
            }

            if (objResult.Status == MBResponseStatus.Processed)
            {
                ReIssueViewModel _viewModel = null;
                _viewModel = DeSerializeNow(SessionID, _viewModel, Site.ID, PaymentType);
                if (_viewModel != null && _viewModel.user.ID == UserID)
                {
                    PaymentGatewayInteraction gt = new PaymentGatewayInteraction();

                    gt.InParams = new GatewayInParameter();
                    gt.InParams.TransactionAmount = _viewModel.PayableAmount;
                    gt.InParams.CCNumber = "";
                    gt.TransactionWrapper.GatewayAuthCode = objResult.MBTransactionID;
                    gt.TransactionWrapper.GatewayRequest = objResult.PayToEmail;
                    gt.TransactionWrapper.GatewayResponse = objResult.PayFromEmail + "|" + objResult.Status + "|" + "|" + objResult.FailedReasonCode + "|" + objResult.Amount + "|" + objResult.Currency + "|" + objResult.CustomerID + "|" + objResult.MBAmount + "|" + objResult.MBCurrency + "|" + objResult.MD5sig + "|" + objResult.MerchantID + "|" + objResult.TransactionID;
                    gt.TransactionWrapper.isPaymentTransaction = true;
                    gt.TransactionWrapper.TransactionAmount = objResult.Amount;
                    gt.TransactionWrapper.statusCode = (objResult.Status == MBResponseStatus.Processed ? "1" : "0");
                    gt.TransactionWrapper.GatewayErrorCode = (objResult.Status == MBResponseStatus.Processed ? "Success" : "Error during Moneybookers transaction");
                    gt.TransactionWrapper.PaymentModeID = (int)WBSSLStore.Domain.PaymentMode.MONEYBOOKERS;
                    gt.TransactionWrapper.isSuccess = true;
                    _viewModel.IsPaymentSuccess = true;
                    ProcessReIssue(_viewModel, Site.ID, gt);
                    _viewModel.IsPaymentSuccess = true;
                    SerializeNow(SessionID, _viewModel, Site.ID, PaymentType);
                }
            }
            else
            {
                ReIssueViewModel _viewModel = null;
                _viewModel = DeSerializeNow(SessionID, _viewModel, Site.ID, PaymentType);
                _viewModel.IsPaymentSuccess = false;
                SerializeNow(SessionID, _viewModel, Site.ID, PaymentType);
            }
        }
        public int ProcessMBWaitRequest(Site site)
        {
            string SessionID = HttpContext.Current.Request.QueryString[SettingConstants.PAYPAL_SESSION_KEY].ToString();
            int UserID = Convert.ToInt32(HttpContext.Current.Request.QueryString[SettingConstants.QS_USERID]);
            string PaymentType = HttpContext.Current.Request.QueryString[SettingConstants.PAYPAL_PAYMENTTYPE].ToString();
            if (PaymentType.ToLower().Equals("reissueaddfund"))
            {
                ReIssueViewModel SessionData = null;
                SessionData = DeSerializeNow(SessionID, SessionData, site.ID, PaymentType);
                if (SessionData != null && SessionData.IsPaymentSuccess)
                {
                    if (!string.IsNullOrEmpty(SessionData.ReIssueUrl) && (SessionData.ReIssueUrl.ToLower().StartsWith("http://") || SessionData.ReIssueUrl.ToLower().StartsWith("https://")))
                    {
                        System.Web.HttpContext.Current.Response.Redirect("/entercsr/reissue/" + SessionData.OrderDetailID, true);
                        return 0;
                    }
                    else
                    {
                        logger.Log(SessionData.ReIssueUrl, Logger.LogType.CRITICAL);
                        return -99;
                    }
                }
                else
                    return 0;
            }
            else
            {
                CheckOutViewModel SessionData = null;
                SessionData = DeSerializeNow(SessionID, SessionData, site.ID, PaymentType);
                if (SessionData != null && SessionData.IsPaymentSuccess)
                {
                    if (PaymentType.ToLower().Equals("order") && SessionData.OrderID > 0)
                        return SessionData.OrderID;
                    else if (PaymentType.ToLower().Equals("addfund") && SessionData.OrderID > 0)
                        return -1;
                    else
                    {

                        return -99;
                    }
                }
                else
                    return 0;
            }
        }


        //Added By <<Mehul>>
        public void ReIssueAndUpdateOrderREST(OrderDetail model, decimal SANAmount, int NewAddedSAN)
        {

            UserTransaction objTransaction = new UserTransaction();
            objTransaction.AuditDetails = new Audit();
            objTransaction.AuditDetails.ByUserID = model.Order.UserID;
            objTransaction.AuditDetails.DateCreated = DateTimeWithZone.Now;
            objTransaction.AuditDetails.DateModified = DateTimeWithZone.Now;
            objTransaction.AuditDetails.HttpHeaderDump = System.Web.HttpContext.Current.Request.Headers.ToString();
            objTransaction.AuditDetails.IP = System.Web.HttpContext.Current.Request.UserHostName;
            objTransaction.Comment = "Add Additional SAN Via Reissue For Order " + model.ExternalOrderID;
            objTransaction.ReceipientInstrumentDetails = "Add Additional SAN Via Reissue For Order " + model.ExternalOrderID;
            objTransaction.SiteID = model.Order.SiteID;
            objTransaction.TransactionAmount = SANAmount * -1;
            objTransaction.TransactionModeID = (int)TransactionMode.REISSUE;
            objTransaction.UserID = model.Order.UserID;
            _repository.AddOrder(objTransaction);

            model.CertificateRequest.AdditionalDomains = model.CertificateRequest.AdditionalDomains + NewAddedSAN;
            model.CertificateRequest.AdminContact = null;
            model.CertificateRequest.TechnicalContact = null;
            model.CertificateRequest.BillingContact = null;
            _CertRequest.Update(model.CertificateRequest);
            _unitOfWork.Commit();

        }
        public void UpdateDomainnames(OrderDetail model, string DNSNames)
        {
            model.CertificateRequest.AddDomainNames = DNSNames;
            _CertRequest.Update(model.CertificateRequest);
            _unitOfWork.Commit();
        }
        //Ended By <<Mehul>>
        private WBSSLStore.Gateway.PaymentGatewayInteraction ReIssuePaymentProcess(ReIssueViewModel _viewModel, Site site, string PaymentType)
        {
            var PGInstance = _repository.GetPGInstances(site.ID);

            if (_viewModel.paymentmode == Domain.PaymentMode.CC)
            {

                PaymentGateways PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.AuthorizeNet).FirstOrDefault();
                if (PG == null)
                    return null;


                PGGatewayHandler pg = PGGatewayFactory.CreateInstance((int)(PG.InstancesID));
                GatewayInParameter oGatewayParameter = new GatewayInParameter();

                _viewModel.IsPaymentSuccess = true;
                oGatewayParameter.BillingAddress1 = _viewModel.user.Address.Street;

                oGatewayParameter.City = _viewModel.user.Address.City;
                oGatewayParameter.CountryName = _viewModel.BillingCountry;
                oGatewayParameter.State = _viewModel.user.Address.State;
                oGatewayParameter.ZipCode = _viewModel.user.Address.Zip;
                oGatewayParameter.BillingEmail = _viewModel.user.Email;
                oGatewayParameter.BillingPhone = _viewModel.user.Address.Phone;
                oGatewayParameter.BillingCompanyName = _viewModel.user.CompanyName;
                oGatewayParameter.CardFirstName = _viewModel.CCName;
                oGatewayParameter.CardFullName = _viewModel.CCName;
                oGatewayParameter.CardLastName = _viewModel.CCName;
                oGatewayParameter.CCExpMonth = _viewModel.Month;
                oGatewayParameter.CCExpYear = _viewModel.Year;
                oGatewayParameter.CCNumber = _viewModel.CCNumber;
                oGatewayParameter.CVV = _viewModel.CVV;
                //oGatewayParameter.CardType = _viewModel.CardType;
                oGatewayParameter.TransactionAmount = _viewModel.PayableAmount;
                //Set Gateway API Settings
                if (PG.InstancesID == (int)PGInstances.AuthorizeNet)
                {
                    oGatewayParameter.APITestUrl = PG.TestURL;
                    oGatewayParameter.APILoginID = PG.LoginID;
                    oGatewayParameter.APITransactionKey = PG.TransactionKey;
                    oGatewayParameter.IsTestMode = PG.IsTestMode;
                }
                //end

                return pg.ProcessAuthAndCapture(oGatewayParameter);
            }
            else if (_viewModel.paymentmode == Domain.PaymentMode.PAYPAL)
            {
                PaymentGateways PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.PayPalIPN).FirstOrDefault();
                if (PG == null)
                    return null;
                HttpContext.Current.Response.Redirect(GetReIssueRedirectPayPalUrl(_viewModel, site, PaymentType, PG, Domain.PaymentMode.PAYPAL));
                return null;
            }
            else if (_viewModel.paymentmode == Domain.PaymentMode.MONEYBOOKERS)
            {
                PaymentGateways PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.Moneybookers).FirstOrDefault();
                if (PG == null)
                    return null;
                HttpContext.Current.Response.Redirect(GetReIssueRedirectPayPalUrl(_viewModel, site, PaymentType, PG, Domain.PaymentMode.MONEYBOOKERS));
                return null;
            }
            else
                return null;
        }
        private WBSSLStore.Gateway.PaymentGatewayInteraction PaymentProcess(CheckOutViewModel _viewModel, Site site, string PaymentType)
        {
            var PGInstance = _repository.GetPGInstances(site.ID);

            if (_viewModel.paymentmode == Domain.PaymentMode.CC && _viewModel.ISCC)
            {

                PaymentGateways PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.AuthorizeNet).FirstOrDefault();
                if (PG == null)
                    return null;


                PGGatewayHandler pg = PGGatewayFactory.CreateInstance((int)(PG.InstancesID));
                GatewayInParameter oGatewayParameter = new GatewayInParameter();

                _viewModel.IsPaymentSuccess = true;
                oGatewayParameter.BillingAddress1 = _viewModel.user.Address.Street;

                oGatewayParameter.City = _viewModel.user.Address.City;
                oGatewayParameter.CountryName = _viewModel.BillingCountry;
                oGatewayParameter.State = _viewModel.user.Address.State;
                oGatewayParameter.ZipCode = _viewModel.user.Address.Zip;
                oGatewayParameter.BillingEmail = _viewModel.user.Email;
                oGatewayParameter.BillingPhone = _viewModel.user.Address.Phone;
                oGatewayParameter.BillingCompanyName = _viewModel.user.CompanyName;
                oGatewayParameter.CardFirstName = _viewModel.CCName;
                oGatewayParameter.CardFullName = _viewModel.CCName;
                oGatewayParameter.CardLastName = _viewModel.CCName;
                oGatewayParameter.CCExpMonth = _viewModel.Month;
                oGatewayParameter.CCExpYear = _viewModel.Year;
                oGatewayParameter.CCNumber = _viewModel.CCNumber;
                oGatewayParameter.CVV = _viewModel.CVV;
                oGatewayParameter.TransactionAmount = _viewModel.PayableAmount;
                //oGatewayParameter.InvoiceNumber = _viewModel.InvoiceNumber;
                //Set Gateway API Settings
                if (PG.InstancesID == (int)PGInstances.AuthorizeNet)
                {
                    oGatewayParameter.APITestUrl = PG.TestURL;
                    oGatewayParameter.APILiveUrl = PG.LiveURL;
                    oGatewayParameter.APILoginID = PG.LoginID;
                    oGatewayParameter.APITransactionKey = PG.TransactionKey;
                    oGatewayParameter.IsTestMode = PG.IsTestMode;
                }
                //end

                return pg.ProcessAuthAndCapture(oGatewayParameter);
            }
            //else if (_viewModel.paymentmode == Domain.PaymentMode.PAYPAL && !string.IsNullOrEmpty(_viewModel.PayPalID) && _viewModel.IsPayPal)
            else if (_viewModel.paymentmode == Domain.PaymentMode.PAYPAL && _viewModel.IsPayPal)
            {
                PaymentGateways PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.PayPalIPN).FirstOrDefault();
                if (PG == null)
                    return null;
                HttpContext.Current.Response.Redirect(GetOrRedirectPayPalUrl(_viewModel, site, PaymentType, PG, Domain.PaymentMode.PAYPAL));
                return null;
            }
            else if (_viewModel.paymentmode == Domain.PaymentMode.MONEYBOOKERS && _viewModel.IsMoneybookers)
            {
                PaymentGateways PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.Moneybookers).FirstOrDefault();
                if (PG == null)
                    return null;
                HttpContext.Current.Response.Redirect(GetOrRedirectPayPalUrl(_viewModel, site, PaymentType, PG, Domain.PaymentMode.MONEYBOOKERS));
                return null;
            }
            else
            {
                return null;
            }
        }
        private string GetOrRedirectPayPalUrl(CheckOutViewModel _viewModel, Site site, string PaymentType, PaymentGateways PG, Domain.PaymentMode ePaymentMode)
        {
            if (_viewModel.SiteID > 0)
            {
            }


            string strURL = string.Empty;
            string SessionID = System.Web.HttpContext.Current.Session.SessionID;
            _viewModel.IsPaymentSuccess = true;

            CheckOutViewModel SerializeModel = new CheckOutViewModel();
            SerializeModel.AllProductInfo = _viewModel.AllProductInfo;
            SerializeModel.AvailableCredit = _viewModel.AvailableCredit;
            SerializeModel.InvoiceNumber = _viewModel.InvoiceNumber;
            SerializeModel.ISCC = false;
            SerializeModel.IsNewuser = _viewModel.IsNewuser;
            SerializeModel.IsPaymentSuccess = _viewModel.IsPaymentSuccess;
            SerializeModel.IsPayPal = _viewModel.IsPayPal;
            SerializeModel.IsMoneybookers = _viewModel.IsMoneybookers;
            SerializeModel.ISVatApplicable = _viewModel.ISVatApplicable;
            SerializeModel.OrderAmount = _viewModel.OrderAmount;
            SerializeModel.OrderID = _viewModel.OrderID;
            SerializeModel.PayableAmount = _viewModel.PayableAmount;
            SerializeModel.paymentmode = _viewModel.paymentmode;
            SerializeModel.PayPalID = _viewModel.PayPalID;
            SerializeModel.MoneybookersID = _viewModel.MoneybookersID;
            SerializeModel.PromoDiscount = _viewModel.PromoDiscount;
            SerializeModel.ShoppingCartID = _viewModel.ShoppingCartID;
            SerializeModel.SiteID = _viewModel.SiteID;
            SerializeModel.Tax = _viewModel.Tax;
            SerializeModel.VatCountry = _viewModel.VatCountry;
            SerializeModel.VATNumber = _viewModel.VATNumber;
            SerializeModel.VatPercent = _viewModel.VatPercent;
            SerializeModel.user = new User();
            SerializeModel.user.Address = new Address();
            SerializeModel.user.Address = _viewModel.user.Address;
            SerializeModel.user.AddressID = _viewModel.user.AddressID;
            SerializeModel.user.AlternativeEmail = _viewModel.user.AlternativeEmail;
            SerializeModel.user.CompanyName = _viewModel.user.CompanyName;
            SerializeModel.user.ConfirmPassword = _viewModel.user.ConfirmPassword;
            SerializeModel.user.Email = _viewModel.user.Email;
            SerializeModel.user.FirstName = _viewModel.user.FirstName;
            SerializeModel.user.HeardBy = _viewModel.user.HeardBy;
            SerializeModel.user.ID = _viewModel.user.ID;
            SerializeModel.user.LastName = _viewModel.user.LastName;
            SerializeModel.user.PasswordHash = _viewModel.user.PasswordHash;
            SerializeModel.user.PasswordSalt = _viewModel.user.PasswordSalt;
            SerializeModel.user.RecordStatusID = _viewModel.user.RecordStatusID;

            SerializeModel.user.SiteID = _viewModel.user.SiteID;
            SerializeModel.user.RecordStatus = _viewModel.user.RecordStatus;
            SerializeModel.user.UserType = _viewModel.user.UserType;
            SerializeModel.user.UserTypeID = _viewModel.user.UserTypeID;

            SerializeNow(SessionID, SerializeModel, site.ID, PaymentType);

            using (CurrentSiteSettings currentSiteSettings = new Domain.CurrentSiteSettings(site))
            {

                string strAlias = (currentSiteSettings.USESSL && currentSiteSettings.IsSiteRunWithHTTPS ? "https://" : "http://") + (currentSiteSettings.IsRunWithWWW && !currentSiteSettings.CurrentSite.Alias.Contains("www.") ? "www." : "") + (string.IsNullOrEmpty(currentSiteSettings.CurrentSite.Alias) ? currentSiteSettings.CurrentSite.CName : currentSiteSettings.CurrentSite.Alias);

                if (ePaymentMode == Domain.PaymentMode.PAYPAL)
                {
                    PayPalOrderData objOrdData = new PayPalOrderData();
                    objOrdData.Name = "SSL Certificates";
                    objOrdData.Amount = _viewModel.PayableAmount; //0.01M; //
                    objOrdData.Qty = 1;
                    objOrdData.CurrencyCode = currentSiteSettings.CurrentCurrencyCode;
                    objOrdData.ItemNumber = "SSL-" + _viewModel.ShoppingCartID;
                    objOrdData.Invoice = site.Alias.ToLower().Replace("www", "").Replace(".", "_") + "-" + _viewModel.user.ID + "-" + DateTime.Now.Millisecond;
                    objOrdData.Custom = "SSLCertificates_Purchase_From:" + site.Alias;

                    objOrdData.NotifyUrl = strAlias + "/checkout/payment/paypalipn?" + WBSSLStore.Domain.SettingConstants.PAYPAL_SESSION_KEY + "=" + SessionID + "&PaypalFeedback=Notify&" + SettingConstants.PAYPAL_PAYMENTTYPE + "=" + PaymentType + "&" + WBSSLStore.Domain.SettingConstants.QS_USERID + "=" + _viewModel.user.ID;
                    objOrdData.ReturnUrl = strAlias + "/checkout/payment/paypalwait?" + WBSSLStore.Domain.SettingConstants.PAYPAL_SESSION_KEY + "=" + SessionID + "&" + WBSSLStore.Domain.SettingConstants.QS_USERID + "=" + _viewModel.user.ID + "&" + SettingConstants.PAYPAL_PAYMENTTYPE + "=" + PaymentType + "&IsAuthenticated=true&Qty=1";
                    objOrdData.CancelUrl = strAlias + "/checkout/payment/paypalcancel?" + WBSSLStore.Domain.SettingConstants.QS_USERID + "=" + _viewModel.user.ID + "&" + SettingConstants.PAYPAL_PAYMENTTYPE + "=" + PaymentType;

                    objOrdData.PayPalRecpientID = PG.LoginID;
                    objOrdData.PayPalLiveUrl = PG.LiveURL;
                    objOrdData.PaypalSandoxUrl = PG.TestURL;
                    strURL = PayPalHandler.GetPaypalUrl(objOrdData, PG.IsTestMode);
                }
            }

            return strURL;
        }
        private string GetReIssueRedirectPayPalUrl(ReIssueViewModel _viewModel, Site site, string PaymentType, PaymentGateways PG, Domain.PaymentMode ePaymentMode)
        {
            string SessionID = System.Web.HttpContext.Current.Session.SessionID;
            _viewModel.IsPaymentSuccess = true;

            ReIssueViewModel SerializeModel = new ReIssueViewModel();

            SerializeModel.AvailableCredit = _viewModel.AvailableCredit;
            SerializeModel.ISCC = false;
            SerializeModel.IsPaymentSuccess = _viewModel.IsPaymentSuccess;
            SerializeModel.IsPayPal = _viewModel.IsPayPal;
            SerializeModel.IsMoneybookers = _viewModel.IsMoneybookers;
            SerializeModel.OrderDetailID = _viewModel.OrderDetailID;
            SerializeModel.PayableAmount = _viewModel.PayableAmount;
            SerializeModel.paymentmode = _viewModel.paymentmode;
            SerializeModel.PayPalID = _viewModel.PayPalID;
            SerializeModel.SiteID = _viewModel.SiteID;
            SerializeModel.NewSANAdded = _viewModel.NewSANAdded;
            SerializeModel.PayableAmount = _viewModel.PayableAmount;
            SerializeModel.paymentmode = _viewModel.paymentmode;
            SerializeModel.MoneybookersID = _viewModel.MoneybookersID;
            SerializeModel.ReIssueUrl = _viewModel.ReIssueUrl;
            SerializeModel.SANAmount = _viewModel.SANAmount;
            SerializeModel.user = new User();
            SerializeModel.user.Address = new Address();
            SerializeModel.user.Address = _viewModel.user.Address;
            SerializeModel.user.AddressID = _viewModel.user.AddressID;
            SerializeModel.user.AlternativeEmail = _viewModel.user.AlternativeEmail;
            SerializeModel.user.CompanyName = _viewModel.user.CompanyName;
            SerializeModel.user.ConfirmPassword = _viewModel.user.ConfirmPassword;
            SerializeModel.user.Email = _viewModel.user.Email;
            SerializeModel.user.FirstName = _viewModel.user.FirstName;
            SerializeModel.user.HeardBy = _viewModel.user.HeardBy;
            SerializeModel.user.ID = _viewModel.user.ID;
            SerializeModel.user.LastName = _viewModel.user.LastName;
            SerializeModel.user.PasswordHash = _viewModel.user.PasswordHash;
            SerializeModel.user.PasswordSalt = _viewModel.user.PasswordSalt;
            SerializeModel.user.RecordStatusID = _viewModel.user.RecordStatusID;

            SerializeModel.user.SiteID = _viewModel.user.SiteID;
            SerializeModel.user.RecordStatus = _viewModel.user.RecordStatus;
            SerializeModel.user.UserType = _viewModel.user.UserType;
            SerializeModel.user.UserTypeID = _viewModel.user.UserTypeID;

            SerializeNow(SessionID, SerializeModel, site.ID, PaymentType);

            string strURL = string.Empty;

            using (CurrentSiteSettings currentSiteSettings = new CurrentSiteSettings(site))
            {

                string strAlias = (currentSiteSettings.USESSL && currentSiteSettings.IsSiteRunWithHTTPS ? "https://" : "http://") + (string.IsNullOrEmpty(currentSiteSettings.CurrentSite.Alias) ? currentSiteSettings.CurrentSite.CName : currentSiteSettings.CurrentSite.Alias);
                if (ePaymentMode == Domain.PaymentMode.PAYPAL)
                {
                    PayPalOrderData objOrdData = new PayPalOrderData();
                    objOrdData.Name = "SSL Certificates";
                    objOrdData.ItemNumber = "SSLReIssue-" + _viewModel.OrderDetailID;
                    objOrdData.Invoice = site.Alias.ToLower().Replace("www", "").Replace(".", "_") + "-" + _viewModel.OrderDetailID;
                    objOrdData.Amount = _viewModel.PayableAmount; //0.01M; //
                    objOrdData.Qty = 1;
                    objOrdData.CurrencyCode = currentSiteSettings.CurrentCurrencyCode;
                    objOrdData.Custom = "SSLCertificates_ReIssue_Purchase_From:" + site.Alias;

                    objOrdData.NotifyUrl = strAlias + "/Checkout/Payment/PayPalIPN?" + WBSSLStore.Domain.SettingConstants.PAYPAL_SESSION_KEY + "=" + SessionID + "&PaypalFeedback=Notify&" + SettingConstants.PAYPAL_PAYMENTTYPE + "=" + PaymentType + "&" + WBSSLStore.Domain.SettingConstants.QS_USERID + "=" + _viewModel.user.ID;
                    objOrdData.ReturnUrl = strAlias + "/Checkout/Payment/PayPalWait?" + WBSSLStore.Domain.SettingConstants.PAYPAL_SESSION_KEY + "=" + SessionID + "&" + WBSSLStore.Domain.SettingConstants.QS_USERID + "=" + _viewModel.user.ID + "&" + SettingConstants.PAYPAL_PAYMENTTYPE + "=" + PaymentType + "&IsAuthenticated=true&Qty=1";
                    objOrdData.CancelUrl = strAlias + "/Checkout/Payment/PayPalCancel?" + WBSSLStore.Domain.SettingConstants.QS_USERID + "=" + _viewModel.user.ID + "&" + SettingConstants.PAYPAL_PAYMENTTYPE + "=" + PaymentType;

                    objOrdData.PayPalRecpientID = PG.LoginID;
                    objOrdData.PayPalLiveUrl = PG.LiveURL;
                    objOrdData.PaypalSandoxUrl = PG.TestURL;
                    strURL = PayPalHandler.GetPaypalUrl(objOrdData, PG.IsTestMode);
                }
                else if (ePaymentMode == Domain.PaymentMode.MONEYBOOKERS)
                {
                    MBOrderData objMB = new MBOrderData();
                    objMB.Amount = _viewModel.PayableAmount;
                    objMB.Description = "Note : ";
                    objMB.DetailText = "You are making payment for " + currentSiteSettings.CurrentSite.Alias;
                    objMB.Currency = currentSiteSettings.CurrentCurrencyCode;
                    objMB.PayToEmail = PG.LoginID;
                    //objMB.ConfirmationNote = "";

                    objMB.StatusURL = strAlias + "/Checkout/Payment/MoneybookersIPN?" + WBSSLStore.Domain.SettingConstants.PAYPAL_SESSION_KEY + "=" + SessionID + "&" + SettingConstants.PAYPAL_PAYMENTTYPE + "=" + PaymentType + "&" + WBSSLStore.Domain.SettingConstants.QS_USERID + "=" + _viewModel.user.ID;
                    objMB.ReturnURL = strAlias + "/Checkout/Payment/MBWait?" + WBSSLStore.Domain.SettingConstants.PAYPAL_SESSION_KEY + "=" + SessionID + "&" + WBSSLStore.Domain.SettingConstants.QS_USERID + "=" + _viewModel.user.ID + "&" + SettingConstants.PAYPAL_PAYMENTTYPE + "=" + PaymentType + "&IsAuthenticated=true&Qty=1";
                    objMB.CancelURL = strAlias + "/Checkout/Payment/MBCancel?" + WBSSLStore.Domain.SettingConstants.QS_USERID + "=" + _viewModel.user.ID + "&" + SettingConstants.PAYPAL_PAYMENTTYPE + "=" + PaymentType;

                    objMB.ReturnURLText = "Back to " + currentSiteSettings.CurrentSite.Alias;

                    LanguageTye eLanguageTye = (LanguageTye)Enum.Parse(typeof(LanguageTye), currentSiteSettings.CurrentLangCode, true);
                    if (Enum.IsDefined(typeof(LanguageTye), eLanguageTye))
                        objMB.Language = eLanguageTye;
                    else
                        objMB.Language = LanguageTye.EN;

                    objMB.LogoURL = currentSiteSettings.CurrentSiteLogoPath.Replace("~", "http://" + currentSiteSettings.CurrentSite.Alias);
                    objMB.PayFromEmail = _viewModel.MoneybookersID;

                    objMB.FirstName = _viewModel.user.FirstName;
                    objMB.LastName = _viewModel.user.LastName;


                    strURL = MBHandler.GetMBUrl(objMB);
                }
            }
            return strURL;
        }
        private void SerializeNow(string SessionID, ViewModels.CheckOutViewModel SessionData, int SiteID, string PaymentType)
        {
            PayPalData pdata = _PayPalData.Find(p => p.SessionID == SessionID && p.OrderType == PaymentType && p.SiteID == SiteID).FirstOrDefault();

            if (pdata == null)
                pdata = new PayPalData();

            try
            {

                MemoryStream ms = new MemoryStream();
                string strData = string.Empty;
                SoapFormatter b = new SoapFormatter();
                b.Serialize(ms, SessionData);
                StreamReader sr = new StreamReader(ms);
                sr.BaseStream.Position = 0;
                strData = sr.ReadToEnd();
                ms.Dispose();
                sr.Dispose();
                byte[] byteArray = Encoding.ASCII.GetBytes(strData);
                if (pdata.ID > 0)
                {
                    pdata.PayPalDatas = strData; //byteArray.ToString();
                    _PayPalData.Update(pdata);
                }
                else
                {
                    pdata.SiteID = SiteID;
                    pdata.OrderType = PaymentType;
                    pdata.PayPalDatas = strData; //byteArray.ToString();
                    pdata.SessionID = SessionID;
                    _PayPalData.Add(pdata);
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        private void SerializeNow(string SessionID, ViewModels.ReIssueViewModel SessionData, int SiteID, string PaymentType)
        {
            PayPalData pdata = _PayPalData.Find(p => p.SessionID == SessionID && p.OrderType == PaymentType && p.SiteID == SiteID).FirstOrDefault();

            if (pdata == null)
                pdata = new PayPalData();

            try
            {

                MemoryStream ms = new MemoryStream();
                string strData = string.Empty;
                SoapFormatter b = new SoapFormatter();
                b.Serialize(ms, SessionData);
                StreamReader sr = new StreamReader(ms);
                sr.BaseStream.Position = 0;
                strData = sr.ReadToEnd();
                ms.Dispose();
                sr.Dispose();
                byte[] byteArray = Encoding.ASCII.GetBytes(strData);
                if (pdata.ID > 0)
                {
                    pdata.PayPalDatas = strData; //byteArray.ToString();
                    _PayPalData.Update(pdata);
                }
                else
                {
                    pdata.SiteID = SiteID;
                    pdata.OrderType = PaymentType;
                    pdata.PayPalDatas = strData; //byteArray.ToString();
                    pdata.SessionID = SessionID;
                    _PayPalData.Add(pdata);
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public CheckOutViewModel DeSerializeNow(string SessionID, ViewModels.CheckOutViewModel SessionData, int SiteID, string PaymentType)
        {
            PayPalData pdata = _PayPalData.Find(p => p.SessionID == SessionID && p.OrderType.ToLower().Equals(PaymentType.ToLower()) && p.SiteID == SiteID).FirstOrDefault();

            MemoryStream stream = null;

            try
            {

                stream = new MemoryStream(Encoding.ASCII.GetBytes(pdata.PayPalDatas));
                SoapFormatter b = new SoapFormatter();
                SessionData = (ViewModels.CheckOutViewModel)b.Deserialize(stream);

                return SessionData;

            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {

                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }
        public ReIssueViewModel DeSerializeNow(string SessionID, ViewModels.ReIssueViewModel SessionData, int SiteID, string PaymentType)
        {
            PayPalData pdata = _PayPalData.Find(p => p.SessionID == SessionID && p.OrderType.ToLower().Equals(PaymentType.ToLower()) && p.SiteID == SiteID).FirstOrDefault();

            MemoryStream stream = null;

            try
            {

                stream = new MemoryStream(Encoding.ASCII.GetBytes(pdata.PayPalDatas));
                SoapFormatter b = new SoapFormatter();
                SessionData = (ViewModels.ReIssueViewModel)b.Deserialize(stream);

                return SessionData;

            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {

                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }
    }
}
