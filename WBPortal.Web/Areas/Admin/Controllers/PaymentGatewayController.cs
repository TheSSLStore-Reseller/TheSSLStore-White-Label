using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WBSSLStore.Service;
using WBSSLStore.Domain;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Web.Helpers.Authentication;

namespace WBSSLStore.Web.Areas.Admin.Controllers
{
    [CustomAuthorizeAttribute]
    [HandleError]
    public class PaymentGatewayController : WBController<PaymentGateways, IRepository<PaymentGateways>, SiteService>
    {
        private User _user;
        private User CurrentUser
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
        // GET: /Admin/PaymentGateway/
        public ActionResult Index(int? returnID)
        {
            var PGateways = _repository.Find(pg => pg.SiteID == Site.ID).EagerLoad(pg => pg.AuditDetails);
            ViewBag.SiteID = Site.ID;
            if (returnID != null && returnID == 1)
                ViewBag.Message = "<div class='normsg'>" + WBSSLStore.Resources.GeneralMessage.Message.PaypalSaved +"</div>";
            else if (returnID != null && returnID == -1)
                ViewBag.Message = "<div class='errormsg'>"+ WBSSLStore.Resources.GeneralMessage.Message.ErrorPaypalSave +"</div>";
            else if (returnID != null && returnID == -2)
                ViewBag.Message = "<div class='errormsg'>"+ WBSSLStore.Resources.GeneralMessage.Message.ErrorCCSave +"</div>";
            else if (returnID != null && returnID == 2)
                ViewBag.Message = "<div class='normsg'>"+ WBSSLStore.Resources.GeneralMessage.Message.CCSaved + "</div>";
            else if (returnID != null && returnID == -3)
                ViewBag.Message = "<div class='errormsg'>" + WBSSLStore.Resources.GeneralMessage.Message.ErrorMBSave + "</div>";
            else if (returnID != null && returnID == 3)
                ViewBag.Message = "<div class='normsg'>" + WBSSLStore.Resources.GeneralMessage.Message.MoneybookerSaved + "</div>";
            return View(PGateways.ToList());
        }

        //
        // POST: /Admin/PaymentGateway/

        [HttpPost]
        public ActionResult Index(PaymentGateways model)
        {
            int ReturnID = 0;
            try
            {
                if (model.InstancesID == (int)PGInstances.PayPalIPN)
                {
                    if (SavePayPalSettings(model))
                        ReturnID = 1;
                    else
                        ReturnID = -1;
                }
                else if (model.InstancesID == (int)PGInstances.Moneybookers)
                {
                    if (SaveMBSettings(model))
                        ReturnID = 3;
                    else
                        ReturnID = -3;
                }
                else
                {
                    if (SaveOtherSetting(model))
                        ReturnID = 2;
                    else
                        ReturnID = -2;
                }

                return RedirectToAction("Index", new { @returnID = ReturnID });
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return View();
            }
        }
        #region Private Method
        private bool SavePayPalSettings(PaymentGateways model)
        {
            if (Request.Form["chkPaypal"] == "on")
            {
                if (model.ID > 0)
                {
                    model.AuditDetails.DateModified = DateTimeWithZone.Now;
                    model.AuditDetails.HttpHeaderDump = Request.Headers.ToString();
                    model.AuditDetails.IP = Request.UserHostAddress;
                    _repository.Update(model);
                }
                else
                {
                    model.AuditDetails = DependencyResolver.Current.GetService<Audit>();
                    model.AuditDetails.DateModified = DateTimeWithZone.Now;
                    model.AuditDetails.HttpHeaderDump = Request.Headers.ToString();
                    model.AuditDetails.IP = Request.UserHostAddress;
                    model.AuditDetails.DateCreated = DateTimeWithZone.Now;
                    model.AuditDetails.ByUserID = CurrentUser.ID;
                    model.Name = model.PGInstances.ToString();
                    _repository.Add(model);
                }
            }
            else if (Request.Form["chkPaypal"] == null && model.ID > 0)
                _repository.Delete(_repository.FindByID(model.ID));
            _unitOfWork.Commit();
            return true;
        }
        private bool SaveMBSettings(PaymentGateways model)
        {
            if (Request.Form["chkmoneybookers"] == "on")
            {
                if (model.ID > 0)
                {
                    model.AuditDetails.DateModified = DateTimeWithZone.Now;
                    model.AuditDetails.HttpHeaderDump = Request.Headers.ToString();
                    model.AuditDetails.IP = Request.UserHostAddress;
                    _repository.Update(model);
                }
                else
                {
                    model.AuditDetails = DependencyResolver.Current.GetService<Audit>();
                    model.AuditDetails.DateModified = DateTimeWithZone.Now;
                    model.AuditDetails.HttpHeaderDump = Request.Headers.ToString();
                    model.AuditDetails.IP = Request.UserHostAddress;
                    model.AuditDetails.DateCreated = DateTimeWithZone.Now;
                    model.AuditDetails.ByUserID = CurrentUser.ID;
                    model.Name = model.PGInstances.ToString();
                    _repository.Add(model);
                }
            }
            else if (Request.Form["chkmoneybookers"] == null && model.ID > 0)
                _repository.Delete(_repository.FindByID(model.ID));
            _unitOfWork.Commit();
            return true;
        }
        private bool SaveOtherSetting(PaymentGateways model)
        {
            if (Request.Form["chkauth"] == "on")
            {
                if (model.ID > 0)
                {
                    model.AuditDetails.DateModified = DateTimeWithZone.Now;
                    model.AuditDetails.HttpHeaderDump = Request.Headers.ToString();
                    model.AuditDetails.IP = Request.UserHostAddress;
                    _repository.Update(model);
                }
                else
                {
                    model.AuditDetails = DependencyResolver.Current.GetService<Audit>();
                    model.AuditDetails.DateModified = DateTimeWithZone.Now;
                    model.AuditDetails.HttpHeaderDump = Request.Headers.ToString();
                    model.AuditDetails.IP = Request.UserHostAddress;
                    model.AuditDetails.DateCreated = DateTimeWithZone.Now;
                    model.AuditDetails.ByUserID = CurrentUser.ID;
                    model.Name = model.PGInstances.ToString();
                    _repository.Add(model);
                }
               
            }
            else if (Request.Form["chkauth"] == null && model.ID > 0)
            {

                _repository.Delete(_repository.FindByID(model.ID));
            }
            _unitOfWork.Commit();
            return true;
            
        }
        #endregion

    }
}
