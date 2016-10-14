using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Domain;
using WBSSLStore.Service;
using WBSSLStore.Web.Helpers;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Data.Repository;
using WBSSLStore.Web.Helpers.PagedList;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Security.Principal;
using WBSSLStore.Gateways.RestAPIModels.Request;

namespace WBSSLStore.Web.Controllers
{
    public class CallbackController : WBController<User, IRepository<User>, ISiteService>
    {

       

        [HttpPost]
        public ActionResult Ordernotification(FormCollection collection)
        {

            if (collection != null)
            {
              
                string APIPartnerCode = string.Empty;
                string AuthTokens = string.Empty;

                NewOrderRequest objRequest = new NewOrderRequest();
                objRequest.AdminContact = new Contact();
                objRequest.TechnicalContact = new Contact();
                objRequest.OrganisationInfo = new OrganizationInfo();
                Contact objReqContact = new Contact();
                TinyOrderRequest objReqTinyorder = new TinyOrderRequest();


                objRequest.AuthRequest = new Gateways.RestAPIModels.Request.AuthRequest();
                objRequest.AuthRequest.PartnerCode = Convert.ToString(collection["CustomPartnerCode"]);
                objRequest.AuthRequest.AuthToken = Convert.ToString(collection["AuthRequest-AuthToken"]);
                //username not 

                //APIPartnerCode = Convert.ToString(collection["AuthRequest-PartnerCode"]);
                //AuthTokens = Convert.ToString(collection["AuthRequest-AuthToken"]);
                //objRequest.AddDomainNames = !string.IsNullOrEmpty(Convert.ToString(collection["CustomPartnerCode"])) ? Convert.ToString(collection["CustomPartnerCode"]) : string.Empty;
                int SANCount = string.IsNullOrEmpty(Convert.ToString(collection["ReserveSANCount"])) ? Convert.ToInt32(Convert.ToString(collection["ExtraSAN"])) : Convert.ToInt32(Convert.ToString(collection["ReserveSANCount"]));
                objRequest.AdditionalDomains = SANCount;

                objRequest.OrganisationInfo.OrganizationName = !string.IsNullOrEmpty(Convert.ToString(collection["OrganisationInfo-OrganizationName"])) ? Convert.ToString(collection["OrganisationInfo-OrganizationName"]) : string.Empty;
                objRequest.AdminContact.FirstName = !string.IsNullOrEmpty(Convert.ToString(collection["AdminContact-FirstName"])) ? Convert.ToString(collection["AdminContact-FirstName"]) : string.Empty;
                objRequest.AdminContact.Email = !string.IsNullOrEmpty(Convert.ToString(collection["AdminContact-Email"])) ? Convert.ToString(collection["AdminContact-Email"]) : string.Empty;
                
                objRequest.AdminContact.LastName = !string.IsNullOrEmpty(Convert.ToString(collection["AdminContact-LastName"])) ? Convert.ToString(collection["AdminContact-LastName"]) : string.Empty;
                objRequest.AdminContact.Phone = !string.IsNullOrEmpty(Convert.ToString(collection["AdminContact-Phone"])) ? Convert.ToString(collection["AdminContact-Phone"]) : string.Empty;
                objRequest.AdminContact.Title = !string.IsNullOrEmpty(Convert.ToString(collection["AdminContact-Title"])) ? Convert.ToString(collection["AdminContact-Title"]) : string.Empty;
                objRequest.ApproverEmail = !string.IsNullOrEmpty(Convert.ToString(collection["ApproverEmail"])) ? Convert.ToString(collection["ApproverEmail"]) : string.Empty;
                objRequest.DomainName = !string.IsNullOrEmpty(Convert.ToString(collection["DomainName"])) ? Convert.ToString(collection["DomainName"]) : string.Empty;

                objRequest.isCUOrder = !string.IsNullOrEmpty(Convert.ToString(collection["isCUOrder"])) ? Convert.ToBoolean(collection["DomainName"]) : false;
                objRequest.isRenewalOrder = !string.IsNullOrEmpty(Convert.ToString(collection["isRenewalOrder"])) ? Convert.ToBoolean(collection["isRenewalOrder"]) : false;

                objRequest.ValidityPeriod = !string.IsNullOrEmpty(Convert.ToString(collection["ValidityPeriod"])) ? Convert.ToInt32(Convert.ToString(collection["ValidityPeriod"])) : 0;
                objRequest.ValidityPeriod = !string.IsNullOrEmpty(Convert.ToString(collection["ServerCount"])) ? Convert.ToInt32(Convert.ToString(collection["ServerCount"])) : 0;

                
                objRequest.OrganisationInfo.Division = !string.IsNullOrEmpty(Convert.ToString(collection["OrganisationInfo-Division"])) ? Convert.ToString(collection["OrganisationInfo-Division"]) : string.Empty;
                objRequest.OrganisationInfo.JurisdictionCountry = !string.IsNullOrEmpty(Convert.ToString(collection["OrganisationInfo-JurisdictionCountry"])) ? Convert.ToString(collection["OrganisationInfo-JurisdictionCountry"]) : string.Empty;
                objRequest.OrganisationInfo.JurisdictionCity = !string.IsNullOrEmpty(Convert.ToString(collection["OrganisationInfo-JurisdictionCity"])) ? Convert.ToString(collection["OrganisationInfo-JurisdictionCity"]) : string.Empty;
                objRequest.OrganisationInfo.JurisdictionRegion = !string.IsNullOrEmpty(Convert.ToString(collection["OrganisationInfo-JurisdictionRegion"])) ? Convert.ToString(collection["OrganisationInfo-JurisdictionRegion"]) : string.Empty;

                objRequest.ProductCode = !string.IsNullOrEmpty(Convert.ToString(collection["ProductCode"])) ? Convert.ToString(collection["ProductCode"]) : string.Empty;
                //
                objRequest.SpecialInstructions = !string.IsNullOrEmpty(Convert.ToString(collection["SpecialInstructions"])) ? Convert.ToString(collection["SpecialInstructions"]) : string.Empty;
                objRequest.TechnicalContact.Email = !string.IsNullOrEmpty(Convert.ToString(collection["TechnicalContact-Email"])) ? Convert.ToString(collection["TechnicalContact-Email"]) : string.Empty;
                objRequest.TechnicalContact.FirstName = !string.IsNullOrEmpty(Convert.ToString(collection["TechnicalContact-FirstName"])) ? Convert.ToString(collection["TechnicalContact-FirstName"]) : string.Empty;
                objRequest.TechnicalContact.LastName = !string.IsNullOrEmpty(Convert.ToString(collection["TechnicalContact-LastName"])) ? Convert.ToString(collection["TechnicalContact-LastName"]) : string.Empty;
                objRequest.TechnicalContact.Phone = !string.IsNullOrEmpty(Convert.ToString(collection["TechnicalContact-Phone"])) ? Convert.ToString(collection["TechnicalContact-Phone"]) : string.Empty;
                objRequest.TechnicalContact.Title = !string.IsNullOrEmpty(Convert.ToString(collection["TechnicalContact-Title"])) ? Convert.ToString(collection["TechnicalContact-Title"]) : string.Empty;
                objRequest.WebServerType = !string.IsNullOrEmpty(Convert.ToString(collection["WebServerType"])) ? Convert.ToString(collection["WebServerType"]) : string.Empty;
                objRequest.PartnerOrderID = !string.IsNullOrEmpty(Convert.ToString(collection["CustomOrderID"])) ? Convert.ToString(collection["CustomOrderID"]) : string.Empty;
                objRequest.ReissueInsurance = false;
                objRequest.RequestorEmail = !string.IsNullOrEmpty(Convert.ToString(collection["RequestorEmail"])) ? Convert.ToString(collection["RequestorEmail"]) : string.Empty;
                objRequest.IsEnrollmentLink = !string.IsNullOrEmpty(objRequest.RequestorEmail) ? true : false;

                 

                return Json(true);
            }
            else
            {
                return Json(false);
            }
        }

        [HttpPost]
        public ActionResult Pricing(FormCollection collection)
        {

            return Json(true);
        }

        [HttpPost]
        public ActionResult Cancelation(FormCollection collection) 
        {
            return Json(true);
        }

    }
}
