using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WBSSLStore.Domain;
using WBSSLStore.Service;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Service.ViewModels;

namespace WBSSLStore.Web.Areas.Admin.Controllers
{
    [CustomAuthorizeAttribute]
    [HandleError]
    public class ContractPricingController : WBController<ProductPricingModel, IRepository<Product>, IProductService>
    {
        //
        // GET: /Admin/ContractPricing/

        public ActionResult Index(int id)
        {
            var product = _repository.Find(pr => pr.RecordStatusID == (int)RecordStatus.ACTIVE).EagerLoad(pr => pr.Brand).ToList();
            ViewBag.ContractID = id;
            return View(product);
        }

        //
        // GET: /Admin/ContractPricing/Edit/5

        public ActionResult Edit(int id)
        {
            Contract ClientContract = _service.GetClientContract(Site.ID);
            int CurrentContractID = Convert.ToInt32(Request.QueryString["cid"]);
            List<ProductPricing> lstPricing = _service.GetProductPricing(Site.ID, Convert.ToInt16(id), CurrentContractID).ToList();

            if (lstPricing.Count() <= 0)
            {
                lstPricing = _service.GetProductPricing(Site.ID, Convert.ToInt32(id), ClientContract.ID).ToList();
                foreach (ProductPricing pp in lstPricing)
                {
                    pp.ContractID = CurrentContractID;
                    pp.ID = 0;
                }
            }

            _viewModel.product = _service.GetProduct(id);
            _viewModel.Month_12 = lstPricing.Where(price => price.NumberOfMonths == (_viewModel.product.InternalProductCode.Equals("freessl", StringComparison.OrdinalIgnoreCase) ? 1 : (int)SettingConstants.NumberOfMonths.Month12)).FirstOrDefault();
            _viewModel.Month_24 = lstPricing.Where(price => price.NumberOfMonths == (int)SettingConstants.NumberOfMonths.Month24).FirstOrDefault();
            _viewModel.Month_36 = lstPricing.Where(price => price.NumberOfMonths == (int)SettingConstants.NumberOfMonths.Month36).FirstOrDefault();
            _viewModel.Month_48 = lstPricing.Where(price => price.NumberOfMonths == (int)SettingConstants.NumberOfMonths.Month48).FirstOrDefault();
            _viewModel.Month_60 = lstPricing.Where(price => price.NumberOfMonths == (int)SettingConstants.NumberOfMonths.Month60).FirstOrDefault();



            if (_viewModel.Month_12 == null)
                _viewModel.Month_12 = new ProductPricing() { NumberOfMonths = (int)SettingConstants.NumberOfMonths.Month12, RecordStatusID = (int)RecordStatus.ACTIVE, SiteID = Site.ID, ContractID = CurrentContractID };

            if (_viewModel.Month_24 == null)
                _viewModel.Month_24 = new ProductPricing() { NumberOfMonths = (int)SettingConstants.NumberOfMonths.Month24, RecordStatusID = (int)RecordStatus.ACTIVE, SiteID = Site.ID, ContractID = CurrentContractID };

            if (_viewModel.Month_36 == null)
                _viewModel.Month_36 = new ProductPricing() { NumberOfMonths = (int)SettingConstants.NumberOfMonths.Month36, RecordStatusID = (int)RecordStatus.ACTIVE, SiteID = Site.ID, ContractID = CurrentContractID };

            if (_viewModel.Month_48 == null)
                _viewModel.Month_48 = new ProductPricing() { NumberOfMonths = (int)SettingConstants.NumberOfMonths.Month48, RecordStatusID = (int)RecordStatus.ACTIVE, SiteID = Site.ID, ContractID = CurrentContractID };

            if (_viewModel.Month_60 == null)
                _viewModel.Month_60 = new ProductPricing() { NumberOfMonths = (int)SettingConstants.NumberOfMonths.Month60, RecordStatusID = (int)RecordStatus.ACTIVE, SiteID = Site.ID, ContractID = CurrentContractID };

            //ViewBag.PartnerCode = Site.APIPartnerCode;
            //ViewBag.Password = Site.APIPassword;
            //ViewBag.Username = Site.APIUsername;

            return View(_viewModel);
        }

        //
        // POST: /Admin/ContractPricing/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, ProductPricingModel model)
        {
            try
            {
                // TODO: Add update logic here
                bool isPricingValid = true;
                model.product = _service.GetProduct(id);
                if (model.Month_12.SalesPrice <= 0 && model.Month_24.SalesPrice <= 0 && model.Month_36.SalesPrice <= 0 && model.Month_48.SalesPrice <= 0 && model.Month_60.SalesPrice <= 0 && !model.product.InternalProductCode.Equals("freessl", StringComparison.OrdinalIgnoreCase))
                    isPricingValid = false;
                if (ModelState.IsValid && isPricingValid)
                {
                    bool isSuccess = _service.AddEditProductPricing(model, Convert.ToInt16(Request[SettingConstants.QS_ISRECOMMENDED]), Site, true);
                    if (isSuccess)
                        return RedirectToAction("Index", new { id = Request.QueryString["cid"] });
                    else
                    {
                        return View(model);
                    }
                }
                return View(model);
            }
            catch
            {
                return View();
            }
        }

    }
}
