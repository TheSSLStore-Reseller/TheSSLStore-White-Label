using System;
using System.Linq;
using System.Web.Mvc;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Service.ViewModels;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Domain;
using WBSSLStore.Service;
using WBSSLStore.Web.Helpers;

namespace WBSSLStore.Web.Areas.Checkout.Controllers
{
    public class ReissueController : WBController<ReIssueViewModel, IRepository<OrderDetail>, ICheckoutService>
    {
      
        private string SiteAlias(string Host)
        {
            

                    if (Host.IndexOf('.') > 0 && !(Host.Split('.').Length > 2))
                    {
                        if (WBHelper.IsRunWithWWW(Site) && !Host.Contains("www."))
                        {
                            Host = Host.Replace(Host, "www." + Host);
                        }
                        else if (!WBHelper.IsRunWithWWW(Site) && Host.Contains("www."))
                            Host = Host.Replace("www.", "");
                    }
                

                return "http://" + Host;
            
        }
        private void SetSiteIDInSession()
        {
            Session["CurrentSiteID"] = Site != null ? Site.ID : 0;
        }
        public ViewResult Payment(int id)
        {
            string ExternalOrderID = id.ToString();
            var _OrderDetail = _repository.Find(od => od.ID == id).EagerLoad(od => od.CertificateRequest, od => od.Order, od => od.Product).FirstOrDefault();

            Site Site = GetSite(_OrderDetail.Order.SiteID);
            SetSiteIDInSession();

            var PGInstance = _service.GetPGInstances(Site.ID);
            ViewBag.SiteID = Site.ID;
            PaymentGateways PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.AuthorizeNet).FirstOrDefault();
            if (PG != null)
                ViewBag.ISCC = true;
            else
                ViewBag.ISCC = false;

            PG = null;
            PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.PayPalIPN).FirstOrDefault();
            if (PG != null)
                ViewBag.IsPayPal = true;
            else
                ViewBag.IsPayPal = false;
            PG = null;

            PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.Moneybookers).FirstOrDefault();
            if (PG != null)
                ViewBag.IsMoneybookers = true;
            else
                ViewBag.IsMoneybookers = false;
            PG = null;

            var _ProdPricing = DependencyResolver.Current.GetService<IRepository<ProductPricing>>();
            int CuttentContractID = WBSSLStore.Web.Helpers.WBHelper.GetCurrentContractID(_OrderDetail.Order.UserID, Site.ID);
            ProductPricing objPricing = _ProdPricing.Find(pp => pp.ProductID == _OrderDetail.ProductID && pp.NumberOfMonths == _OrderDetail.NumberOfMonths && pp.ContractID == CuttentContractID && pp.RecordStatusID == (int)RecordStatus.ACTIVE).FirstOrDefault();
            ViewBag.Pricing = objPricing;
            ViewBag.AvailableCredit = _service.GetCreditAmount(_OrderDetail.Order.UserID, Site.ID);
            return View(_OrderDetail);
        }
        [HttpPost]
        public ActionResult Payment(OrderDetail model, FormCollection collection)
        {
            model = _repository.Find(od => od.ID == model.ID).EagerLoad(od => od.CertificateRequest, od => od.Order, od => od.Order.Site, od => od.Product, od => od.AuditDetails, od => od.Order.User).FirstOrDefault();
            Site Site = GetSite(model.Order.SiteID);
            SetSiteIDInSession();
            var _ProdPricing = DependencyResolver.Current.GetService<IRepository<ProductPricing>>();
            int CuttentContractID = WBSSLStore.Web.Helpers.WBHelper.GetCurrentContractID(model.Order.UserID, Site.ID);
            ProductPricing objPricing = _ProdPricing.Find(pp => pp.ProductID == model.ProductID && pp.NumberOfMonths == model.NumberOfMonths && pp.ContractID == CuttentContractID && pp.RecordStatusID == (int)RecordStatus.ACTIVE).FirstOrDefault();
            decimal AvailableCredit = _service.GetCreditAmount(model.Order.UserID, Site.ID);
            int NewAddedSAN = Convert.ToInt32(collection["drpNewSAN"]);
            if (NewAddedSAN > 0)
            {
                if (model.CertificateRequest.AdditionalDomains < model.Product.SanMin)
                {
                    NewAddedSAN = NewAddedSAN - (Convert.ToInt32(model.Product.SanMin) - model.CertificateRequest.AdditionalDomains);
                }
                if (NewAddedSAN > 0)
                {
                    decimal SANAmount = NewAddedSAN * objPricing.AdditionalSanPrice;
                    if (SANAmount > AvailableCredit)
                    {
                        _viewModel = new ReIssueViewModel();
                        _viewModel.ISCC = Convert.ToString(collection["rbtPaymentMethod"]).ToLower().Equals("cc") ? true : false;
                        _viewModel.IsPayPal = Convert.ToString(collection["rbtPaymentMethod"]).ToLower().Equals("paypal") ? true : false;
                        _viewModel.IsMoneybookers = Convert.ToString(collection["rbtPaymentMethod"]).ToLower().Equals("moneybookers") ? true : false;

                        _viewModel.user = model.Order.User;
                        _viewModel.PayableAmount = (SANAmount - AvailableCredit);
                        _viewModel.NewSANAdded = NewAddedSAN;
                        _viewModel.SANAmount = SANAmount;
                        _viewModel.OrderDetailID = model.ID;

                        _viewModel.BillingCountry = _viewModel.user.Address.Country.CountryName;

                        if (_viewModel.IsPayPal || _viewModel.ISCC || _viewModel.IsMoneybookers)
                        {
                            if (_viewModel.IsPayPal && !string.IsNullOrEmpty(collection["txtPaypalID"]))
                            {
                                _viewModel.PayPalID = collection["txtPaypalID"].ToString();
                                _viewModel.paymentmode = WBSSLStore.Domain.PaymentMode.PAYPAL;
                            }
                            else if (_viewModel.IsPayPal)
                                ViewBag.ErrorMessage = "Please enter paypalid.";
                            else if (_viewModel.IsMoneybookers)
                            {
                                _viewModel.MoneybookersID = collection["txtMoneybookersID"].ToString();
                                _viewModel.paymentmode = WBSSLStore.Domain.PaymentMode.MONEYBOOKERS;
                            }
                            else if (_viewModel.ISCC)
                            {
                                _viewModel.CCName = Convert.ToString(collection["txtNameOnCard"]);
                                _viewModel.CCNumber = Convert.ToString(collection["txtCCNumber"]);
                                _viewModel.CVV = Convert.ToString(collection["txtCCV"]);
                                _viewModel.Month = Convert.ToInt32(collection["drpMonth"]);
                                _viewModel.Year = Convert.ToInt32(collection["drpYear"]);
                                _viewModel.paymentmode = WBSSLStore.Domain.PaymentMode.CC;
                            }

                            string Message = _service.AddFundAndReIssue(_viewModel, Site);
                            if (!string.IsNullOrEmpty(Message) && (Message.ToLower().Equals("https://")))
                            {
                                ViewBag.TransferToEnterCsr = true;
                            }
                            else if (!string.IsNullOrEmpty(Message))
                                ViewBag.ErrorMessage = Message;
                            else
                                ViewBag.ErrorMessage = "No payment gateway selected.";
                        }
                        else
                            ViewBag.ErrorMessage = "No payment gateway selected.";
                    }
                    else
                    {

                        _service.ReIssueAndUpdateOrderREST(model, SANAmount, NewAddedSAN);
                    
                        ViewBag.TransferToEnterCsr = true;
                       

                    }
                }
                else
                {
                    model.CertificateRequest.AdditionalDomains = Convert.ToInt32(model.Product.SanMin);
                    model.AuditDetails.DateModified = DateTimeWithZone.Now;
                    model.Order = null;
                    model.Product = null;
                    _repository.Update(model);
                    _unitOfWork.Commit();
                    ViewBag.TransferToEnterCsr = true;
                }
            }
            else
            {
                ViewBag.TransferToEnterCsr = true;
            }

            if (Convert.ToBoolean(ViewBag.TransferToEnterCsr))
                return RedirectToAction("entercsr", new { id = model.ID });

            var PGInstance = _service.GetPGInstances(Site.ID);
            ViewBag.SiteID = Site.ID;
            PaymentGateways PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.AuthorizeNet).FirstOrDefault();
            if (PG != null)
                ViewBag.ISCC = true;
            else
                ViewBag.ISCC = false;

            PG = null;
            PG = PGInstance.Where(p => p.InstancesID == (int)PGInstances.PayPalIPN).FirstOrDefault();
            if (PG != null)
            {
                ViewBag.IsPayPal = true;

            }
            else
            {
                ViewBag.IsPayPal = false;

            }
            PG = null;
            ViewBag.Pricing = objPricing;
            ViewBag.AvailableCredit = AvailableCredit;
            return View(model);
        }

        public ViewResult entercsr(int id)
        {
            var _Orderdetail = _repository.Find(od => od.ID == id).EagerLoad(od => od.CertificateRequest, od => od.Order, od => od.Product).FirstOrDefault();
            return View(_Orderdetail);
        }

        [HttpPost]
        public ActionResult entercsr(int id, FormCollection collection)
        {
            WBSSLStore.Gateways.RestAPIModels.Response.OrderResponse ordresponse = new Gateways.RestAPIModels.Response.OrderResponse();
            OrderDetail model = new OrderDetail();
            model = _repository.Find(od => od.ID == id).EagerLoad(od => od.CertificateRequest, od => od.Order, od => od.Product, od => od.CertificateRequest.AdminContact).FirstOrDefault();
            int noofdomains = 0;

          
            noofdomains = Convert.ToInt32(collection["hdnTotalSan"]);
            WBSSLStore.Gateways.RestAPIModels.Request.ReissueOrderRequest request = new Gateways.RestAPIModels.Request.ReissueOrderRequest();
            request.AuthRequest = new WBSSLStore.Gateways.RestAPIModels.Request.AuthRequest();
           Site Site = GetSite(model.Order.SiteID);
            request.AuthRequest.PartnerCode = Site.APIPartnerCode;
            request.AuthRequest.AuthToken = Site.APIAuthToken;
            request.AuthRequest.UserAgent = Site.Alias;
            request.AuthRequest.ReplayToken = "SSL Store WhiteBrand Sites : " + Site.Alias;
            request.CSR = !string.IsNullOrEmpty(collection["txtcsr"].ToString()) ? Server.UrlEncode(collection["txtcsr"].ToString()) : string.Empty;
            request.TheSSLStoreOrderID = Convert.ToString(model.ExternalOrderID);
            //request.DNSNames
            request.ReissueEmail = model.CertificateRequest.AdminContact.EmailAddress != null && !string.IsNullOrEmpty(model.CertificateRequest.AdminContact.EmailAddress) ? model.CertificateRequest.AdminContact.EmailAddress : string.Empty;
            int iedit = 0;
            int iadd = 0;
            int idelete = 0;
            request.EditSAN = new Gateways.RestAPIModels.Request.Pair[99];
            request.AddSAN = new Gateways.RestAPIModels.Request.Pair[99];
            request.DeleteSAN = new Gateways.RestAPIModels.Request.Pair[99];
            for (int i = 1; i <= noofdomains; i++)
            {
                if (collection["hdDomain" + i] != null)
                {
                    if (!Convert.ToString(collection["hdDomain" + i]).Equals(Convert.ToString(collection["Domain" + i])) && !string.IsNullOrEmpty(Convert.ToString(collection["Domain" + i])))
                    {
                        WBSSLStore.Gateways.RestAPIModels.Request.Pair p = new Gateways.RestAPIModels.Request.Pair();
                        p.OldValue = Convert.ToString(collection["hdDomain" + i]);
                        p.NewValue = Convert.ToString(collection["Domain" + i]);

                        request.EditSAN[iedit] = p;
                        iedit++;
                    }
                    else if (!Convert.ToString(collection["hdDomain" + i]).Equals(Convert.ToString(collection["Domain" + i])) && string.IsNullOrEmpty(Convert.ToString(collection["Domain" + i])))
                    {
                        WBSSLStore.Gateways.RestAPIModels.Request.Pair p2 = new Gateways.RestAPIModels.Request.Pair();
                        p2.OldValue = Convert.ToString(collection["hdDomain" + i]);

                        request.DeleteSAN[idelete] = p2;
                        idelete++;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(collection["Domain" + i])))
                    {
                        WBSSLStore.Gateways.RestAPIModels.Request.Pair p1 = new Gateways.RestAPIModels.Request.Pair();
                        p1.NewValue = Convert.ToString(collection["Domain" + i]);

                        request.AddSAN[iadd] = p1;
                        iadd++;
                    }

                }
            }

            request.DNSNames = null;
            request.isRenewalOrder = false;
            if (model.Product.isWildcard)
                request.isWildCard = true;
            else
                request.isWildCard = false;
            ordresponse = WBSSLStore.Gateways.RestAPIModels.Services.OrderService.ReissueCerts(request);
            if (!ordresponse.AuthResponse.isError)
            {
                _service.UpdateDomainnames(model, ordresponse.DNSNames);
                string url = SiteAlias(  Site.Alias ) + "/client/Orders/Details/" + model.ID;
                return Redirect(url);
            }
            else
            {
                ViewBag.ErrorMessage = ordresponse.AuthResponse.Message[0];

                return View(model); 
              
            }
        }



    }
}
