using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using WBSSLStore.Domain;
using WBSSLStore.Web.Helpers.Base;
using WBSSLStore.Service;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Service.ViewModels;
using WBSSLStore.Gateways.RestAPIModels.Response;
using WBSSLStore.Gateways.RestAPIModels.Services;


namespace WBSSLStore.Web.Areas.Admin.Controllers
{
    [CustomAuthorizeAttribute]
    [HandleError]
    public class ProductController : WBController<ProductPricingModel, IRepository<Product>, IProductService>
    {


        //GET: /Admin/Product/

        public ViewResult Index()
        {
            //var products = _repository.Find(pro => pro.RecordStatusID != (int)RecordStatus.DELETED );

            var products = _repository.Find(x => x.RecordStatusID != (int)RecordStatus.DELETED);
            return View(products);
        }

        ////
        //// GET: /Admin/Product/ChangeStatus/5

        [HttpPost]
        public ActionResult ChangeStatus(int id)
        {
            var bStatus = _service.UpdateProductStatus(id, false, Site.ID);
            return Json(bStatus);
        }

        //
        // GET: /Admin/Product/Delete/5

        public ActionResult Delete(int id)
        {
            var bStatus = _service.UpdateProductStatus(id, true, Site.ID);
            return Json(bStatus);
        }

        //
        // GET: /Admin/Product/Create

        public ActionResult Add()
        {
            Contract ClientContract = _service.GetClientContract(Site.ID);
            _viewModel.Month_12 = new Domain.ProductPricing() { NumberOfMonths = (int)SettingConstants.NumberOfMonths.Month12, RecordStatusID = (int)RecordStatus.ACTIVE, SiteID = Site.ID, ContractID = ClientContract.ID };
            _viewModel.Month_24 = new Domain.ProductPricing() { NumberOfMonths = (int)SettingConstants.NumberOfMonths.Month24, RecordStatusID = (int)RecordStatus.ACTIVE, SiteID = Site.ID, ContractID = ClientContract.ID };
            _viewModel.Month_36 = new Domain.ProductPricing() { NumberOfMonths = (int)SettingConstants.NumberOfMonths.Month36, RecordStatusID = (int)RecordStatus.ACTIVE, SiteID = Site.ID, ContractID = ClientContract.ID };
            _viewModel.Month_48 = new Domain.ProductPricing() { NumberOfMonths = (int)SettingConstants.NumberOfMonths.Month48, RecordStatusID = (int)RecordStatus.ACTIVE, SiteID = Site.ID, ContractID = ClientContract.ID };
            _viewModel.Month_60 = new Domain.ProductPricing() { NumberOfMonths = (int)SettingConstants.NumberOfMonths.Month60, RecordStatusID = (int)RecordStatus.ACTIVE, SiteID = Site.ID, ContractID = ClientContract.ID };
            if (_viewModel.productAvailablity == null)
                _viewModel.productAvailablity = new ProductAvailablity() { isActive = true, SiteID = Site.ID };
            _viewModel.product = new Product();
            ViewBag.APIProduct = OrderService.GetAllProductPricing(Site.APIPartnerCode, Site.APIAuthToken).Product;
            ViewBag.Brand = _service.GetBrand();
            return View(_viewModel);
        }

        //
        // POST: /Admin/Product/Create

        [HttpPost]
        public ActionResult Add(ProductPricingModel model)
        {
            bool isPricingValid = true;
            if (model.Month_12.SalesPrice <= 0 && model.Month_24.SalesPrice <= 0 && model.Month_36.SalesPrice <= 0 && model.Month_48.SalesPrice <= 0 && model.Month_60.SalesPrice <= 0)
                isPricingValid = false;
            if (!isPricingValid && model.product.InternalProductCode.Equals("freessl", StringComparison.OrdinalIgnoreCase))
            {
                model.Month_12.NumberOfMonths = 1;
                isPricingValid = true;
            }
            if (ModelState.IsValid && isPricingValid)
            {
                if (model.productAvailablity == null)
                {
                    model.productAvailablity = DependencyResolver.Current.GetService<ProductAvailablity>();
                    model.productAvailablity.isActive = true;
                    model.productAvailablity.SiteID = Site.ID;
                }
                bool isSuccess = _service.AddEditProductPricing(model, Convert.ToInt16(Request[SettingConstants.QS_ISRECOMMENDED]), Site, false);
                if (isSuccess)
                    return RedirectToAction("Index");
                else
                {
                    ViewBag.Brand = _service.GetBrand();
                    return View(model);
                }
            }

            ViewBag.APIProduct = WBSSLStore.Gateways.RestAPIModels.Services.OrderService.GetAllProductPricing(Site.APIPartnerCode, Site.APIAuthToken);


            ViewBag.Brand = _service.GetBrand();
            if (!isPricingValid)
                model.Message = WBSSLStore.Resources.ErrorMessage.Message.NoPricingAdded;
            return View(model);
        }

        //
        // GET: /Admin/Product/Edit/5

        public ActionResult Edit(int id)
        {
            if (id > 0)
            {

                Contract ClientContract = _service.GetClientContract(Site.ID);
                List<Domain.ProductPricing> lstPricing = _service.GetProductPricing(Site.ID, Convert.ToInt16(id), ClientContract.ID).ToList();
                _viewModel.Month_12 = lstPricing.Where(price => price.NumberOfMonths == (int)SettingConstants.NumberOfMonths.Month12).FirstOrDefault();
                _viewModel.Month_24 = lstPricing.Where(price => price.NumberOfMonths == (int)SettingConstants.NumberOfMonths.Month24).FirstOrDefault();
                _viewModel.Month_36 = lstPricing.Where(price => price.NumberOfMonths == (int)SettingConstants.NumberOfMonths.Month36).FirstOrDefault();
                _viewModel.Month_48 = lstPricing.Where(price => price.NumberOfMonths == (int)SettingConstants.NumberOfMonths.Month48).FirstOrDefault();
                _viewModel.Month_60 = lstPricing.Where(price => price.NumberOfMonths == (int)SettingConstants.NumberOfMonths.Month60).FirstOrDefault();

                if (_viewModel.Month_12 == null)
                    _viewModel.Month_12 = new Domain.ProductPricing() { NumberOfMonths = (int)SettingConstants.NumberOfMonths.Month12, RecordStatusID = (int)RecordStatus.ACTIVE, SiteID = Site.ID, ContractID = ClientContract.ID };

                if (_viewModel.Month_24 == null)
                    _viewModel.Month_24 = new Domain.ProductPricing() { NumberOfMonths = (int)SettingConstants.NumberOfMonths.Month24, RecordStatusID = (int)RecordStatus.ACTIVE, SiteID = Site.ID, ContractID = ClientContract.ID };

                if (_viewModel.Month_36 == null)
                    _viewModel.Month_36 = new Domain.ProductPricing() { NumberOfMonths = (int)SettingConstants.NumberOfMonths.Month36, RecordStatusID = (int)RecordStatus.ACTIVE, SiteID = Site.ID, ContractID = ClientContract.ID };


                if (!(lstPricing != null && lstPricing.Count > 0))
                    _viewModel.product = _service.GetProduct(Convert.ToInt16(id));
                else
                    _viewModel.product = lstPricing.Where(pp => pp.ID > 0).FirstOrDefault().Product;

                _viewModel.productAvailablity = _service.GetProductAvailablity(id);

                ViewBag.Brand = _service.GetBrand();

                return View("Add", _viewModel);
            }
            else
                return RedirectToAction("Index");
        }


        //POST: /Admin/Product/Edit/5

        [HttpPost]
        public ActionResult Edit(ProductPricingModel model)
        {

            var m = ModelState.Values.Where(x => x.Errors.Count > 0);

            if (ModelState.IsValid)
            {
                Add(model);
                return RedirectToAction("Index");
            }
            ViewBag.Brand = _service.GetBrand();
            return View("add", model);
        }
        public ActionResult Pricing()
        {

            GetAllPricingResponse AllPrice = OrderService.GetAllProductPricing(Site.APIPartnerCode, Site.APIAuthToken);
            if (!string.IsNullOrEmpty(AllPrice.ErrorCode))
                ViewBag.APIError = AllPrice.ErrorMessage;


            return View(AllPrice.Product);


        }

        [HttpPost]
        public ActionResult Pricing(FormCollection collection)
        {
            decimal ProfitMargin = Convert.ToDecimal(collection["txtPricePer"]);

            Request.RequestContext.HttpContext.Server.MapPath(".");
            GetAllPricingResponse AllPrice = OrderService.GetAllProductPricing(Site.APIPartnerCode, Site.APIAuthToken);


            var objResponse = _service.ImportProducts(AllPrice.Product, Site, ProfitMargin);

            ViewBag.SuccessProduct = objResponse[0].ToString();
            ViewBag.UnSuccessProduct = objResponse[1].ToString();
            return View(AllPrice.Product);


        }
        protected override void Dispose(bool disposing)
        {

            base.Dispose(disposing);
        }
    }
}