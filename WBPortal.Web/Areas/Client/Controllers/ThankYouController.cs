using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WBSSLStore.Domain;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Web.Helpers.Authentication;
using System.Web.Security;
using System.Security.Principal;

namespace WBSSLStore.Web.Areas.Client.Controllers
{

    [HandleError]
    public class ThankYouController : WBController<List<OrderDetail>, IRepository<OrderDetail>, IRepository<User>>
    {
        private User _user;
        private User currentuser
        {
            get
            {
                if (_user == null)
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
            set
            {
                _user = value;
            }
        }
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (!User.Identity.IsAuthenticated)
            {
                User user = null;
                string strToken = CryptorEngine.Decrypt(HttpUtility.UrlDecode(Request.QueryString["token"]), true).Replace("\0", string.Empty);
                if (!string.IsNullOrEmpty(strToken))
                {
                    string[] strOrderIDAndUserID = strToken.Split(new string[] { SettingConstants.Seprate }, StringSplitOptions.RemoveEmptyEntries);
                    int uid = 0;
                    if (strOrderIDAndUserID != null && strOrderIDAndUserID.Length > 1)
                        uid = Convert.ToInt32(strOrderIDAndUserID[1]);
                    user = _service.FindByID(uid);
                }
                if (user != null)
                {
                    FormsAuthentication.SetAuthCookie(user.Email, false);
                    Request.RequestContext.HttpContext.User = new GenericPrincipal(new GenericIdentity(user.Email, "Forms"), null);
                    currentuser = ((SSLStoreUser)System.Web.Security.Membership.GetUser(user.Email)).Details;
                }
            }
        }
        //
        // GET: /Client/TahnkYou/

        public ActionResult Index(string token)
        {
            string strToken = CryptorEngine.Decrypt(HttpUtility.UrlDecode(token), true).Replace("\0", string.Empty);
            string[] strOrderIDAndUserID = strToken.Split(new string[] { SettingConstants.Seprate }, StringSplitOptions.RemoveEmptyEntries);
            int id = 0;
            if (strOrderIDAndUserID != null && strOrderIDAndUserID.Length > 1)
                id = Convert.ToInt32(strOrderIDAndUserID[0]);

            User user = currentuser;
            ViewBag.ClientEmail = user.Email;
            ViewBag.UserName = user.FirstName + " " + user.LastName;
            List<OrderDetail> ord = _repository.Find(o => o.OrderID == id && o.Order.SiteID == Site.ID && o.Order.UserID == user.ID).EagerLoad(x => x.Order).ToList();

            return View(ord);
        }

        [Authorize]
        public ActionResult GenerateNow(int id)
        {


            User user = currentuser;

            ViewBag.ClientEmail = user.Email;

            OrderDetail ord = _repository.Find(o => o.ID == id && o.Order.SiteID == Site.ID && o.Order.UserID == user.ID).EagerLoad(x => x.StoreAPIInteraction).FirstOrDefault();

            if (ord != null && !string.IsNullOrEmpty(ord.ExternalOrderID))
            {
                return Redirect(ord.StoreAPIInteraction.GatewayAuthCode);
            }
            else if (ord != null && string.IsNullOrEmpty(ord.ExternalOrderID))
            {
                ord = _repository.Find(o => o.ID == id && o.Order.SiteID == Site.ID && o.Order.UserID == user.ID).EagerLoad(x => x.StoreAPIInteraction, x => x.Product, x => x.CertificateRequest, x => x.AuditDetails).FirstOrDefault();

                ord = GenerateLinkThruRESTAPI(ord, Site, user);


                if (!string.IsNullOrEmpty(ord.StoreAPIInteraction.GatewayAuthCode) && ord.StoreAPIInteraction.isSuccess)
                    return Redirect(ord.StoreAPIInteraction.GatewayAuthCode);
                else
                {
                    object error = new object();
                    error = "9011 --- Error in Generating order link. Please contact the site admin.";

                    return View("error", error);// Content(objLink.ErrorMessage);
                }





            }

            return null;
        }


        private OrderDetail GenerateLinkThruRESTAPI(OrderDetail ord, Site site, User user)
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
                inviteReq.ExtraSAN = ord.CertificateRequest.AdditionalDomains;
                inviteReq.ProductCode = ord.Product.InternalProductCode;
                inviteReq.RequestorEmail = user.Email;
                inviteReq.ServerCount = ord.CertificateRequest.NumberOfServers == 0 ? 1 : ord.CertificateRequest.NumberOfServers;
                inviteReq.ValidityPeriod = ord.NumberOfMonths;
                inviteReq.AddInstallationSupport = false;
                inviteReq.CustomOrderID = string.Empty;
                inviteReq.EmailLanguageCode = "en";
                inviteReq.ExtraProductCode = string.Empty;
                inviteReq.PreferVendorLink = false;

                objLink = WBSSLStore.Gateways.RestAPIModels.Services.OrderService.InviteOrderRequest(inviteReq);
                if (objLink != null)
                {
                    GatewayInteraction GTSSLAPI = new GatewayInteraction();



                    if (!objLink.AuthResponse.isError)
                    {
                        ord.ExternalOrderID = objLink.TheSSLStoreOrderID;
                        ord.Cost = 0;
                        GTSSLAPI.GatewayAuthCode = objLink.TinyOrderLink;
                        GTSSLAPI.isSuccess = true;



                    }
                    else
                    {
                        ord.ExternalOrderID = string.Empty;
                        GTSSLAPI.GatewayAuthCode = string.Empty;
                        GTSSLAPI.isSuccess = false;


                    }

                    //GTSSLAPI.GatewayRequest = objLink.;
                    GTSSLAPI.GatewayResponse = objLink.AuthResponse.isError ? objLink.AuthResponse.Message[0] : objLink.AuthResponse.ToString();
                    //GTSSLAPI.GatewayErrorCode = objLink.AuthResponse.isError ? objLink.AuthResponse. : string.Empty;
                    GTSSLAPI.isPaymentTransaction = false;
                    GTSSLAPI.SiteID = site.ID;

                    GTSSLAPI.AuditDetails = new Audit();
                    GTSSLAPI.AuditDetails.ByUserID = user.ID;
                    GTSSLAPI.AuditDetails.DateCreated = System.DateTimeWithZone.Now;
                    GTSSLAPI.AuditDetails.DateModified = System.DateTimeWithZone.Now;
                    GTSSLAPI.AuditDetails.HttpHeaderDump = HttpUtility.UrlDecode(Request.Headers.ToString());
                    GTSSLAPI.AuditDetails.IP = Request.UserHostAddress;

                    ord.StoreAPIInteraction = GTSSLAPI;

                    _repository.Update(ord);
                    _unitOfWork.Commit();

                }
            }
            catch
            {
            }
            return ord;
        }
        // Ended By <<MEHUL>>
    }
}
