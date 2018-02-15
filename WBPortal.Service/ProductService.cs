using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WBSSLStore.Domain;
using WBSSLStore.Data.Repository;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Service.ViewModels;
using System.Collections;

namespace WBSSLStore.Service
{
    public interface IProductService
    {
        IQueryable<ProductPricing> GetProductPricing(int SiteID, int ProductID, int ContractID);
        IQueryable<Brand> GetBrand();
        Product GetProduct(int ProductID);
        bool UpdateProductStatus(int id, bool DeleteProduct, int SiteID);
        ProductAvailablity AddEditProductAvailablity(ProductAvailablity productavailablity);
        bool AddEditProductPricing(ProductPricingModel model, int isRecommended, Site site, bool SaveOnlyPrice);
        ProductAvailablity GetProductAvailablity(int ProductID);
        Contract GetClientContract(int SiteID);
        IQueryable<ProductAvailablity> GetAllProductAvailablity(int SiteID);
        List<StringBuilder> ImportProducts(List<Gateways.RestAPIModels.Response.ALLProduct> ProductList, Site site, decimal Margin);
        bool ImportProductsInContract(Site site, decimal Margin, int ContractID);
        Product GetByProductCode(string prdcode);
        ProductDetail GetProductDetailRow(string prodcode);
    }
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<ProductPricing> _productPricing;
        private readonly IRepository<Product> _product;
        private readonly IRepository<Brand> _brand;
        private readonly IRepository<ProductAvailablity> _productavailablity;
        private readonly IRepository<Contract> _contract;
        private readonly IRepository<ProductDetail> _ProductDetail;
        private readonly Logger.Logger _logger;
        public ProductService(IRepository<ProductPricing> ProductPricing, IRepository<Product> Product, IRepository<Brand> Brand, IRepository<ProductAvailablity> ProductAvailablity, IRepository<Contract> Contract, IRepository<ProductDetail> ProductDetail, Logger.Logger Logger, IUnitOfWork UnitOfWork)
        {
            _unitOfWork = UnitOfWork;
            _productPricing = ProductPricing;
            _product = Product;
            _brand = Brand;
            _productavailablity = ProductAvailablity;
            _ProductDetail = ProductDetail;
            _contract = Contract;
            _logger = Logger;
        }



        public IQueryable<ProductPricing> GetProductPricing(int SiteID, int ProductID, int ContractID)
        {


            if (ProductID > 0)
                return _productPricing.Find(p => p.SiteID == SiteID && p.ProductID == ProductID && p.RecordStatusID == (int)RecordStatus.ACTIVE && p.ContractID == ContractID);
            else
                return _productPricing.Find(p => p.SiteID == SiteID && p.RecordStatusID == (int)RecordStatus.ACTIVE && p.ContractID == ContractID);

        }

        public Product GetProduct(int ProductID)
        {
            return _product.FindByID(ProductID);
        }
        public Product GetByProductCode(string prdcode)
        {
            if (!string.IsNullOrEmpty(prdcode))
                return _product.Find(p => p.InternalProductCode == prdcode).FirstOrDefault();
            else
                return null;
        }

        public ProductDetail GetProductDetailRow(string prodcode)
        {
            return _ProductDetail.Find(pd => pd.productcode.ToLower().Equals(prodcode.ToLower())).FirstOrDefault();


        }
        public IQueryable<Brand> GetBrand()
        {
            return _brand.FindAll();
        }
        public bool UpdateProductStatus(int id, bool DeleteProduct, int SiteID)
        {
            if (id > 0)
            {
                Product ObjProduct = _product.Find(pr => pr.ID == id).FirstOrDefault();
             
                if (ObjProduct != null)
                {
                    if (DeleteProduct)
                        ObjProduct.RecordStatusID = (int)RecordStatus.DELETED;
                    else
                        ObjProduct.RecordStatusID = (ObjProduct.RecordStatusID == (int)RecordStatus.ACTIVE ? (int)RecordStatus.INACTIVE : (int)RecordStatus.ACTIVE);
                    _product.Update(ObjProduct);
                    _unitOfWork.Commit();
                    return true;
                }
                else
                    return false;

            }
            else
                return false;

        }
        public ProductAvailablity AddEditProductAvailablity(ProductAvailablity productavailablity)
        {
            if (productavailablity.ID > 0)
                return _productavailablity.Update(productavailablity);
            else
                return _productavailablity.Add(productavailablity);
        }
        public bool AddEditProductPricing(ProductPricingModel model, int isRecommended, Site site, bool SaveOnlyPrice)
        {
            if (!SaveOnlyPrice)
            {
                if (string.IsNullOrEmpty(model.product.ReissueType))
                    model.product.ReissueType = "Included";
                if (model.product.RecordStatusID <= 0)
                    model.product.RecordStatusID = (int)RecordStatus.ACTIVE;
            }
            if (model.Month_12 != null)
            {
                model.Month_12.Product = model.product;
                model.Month_12.isRecommended = false;
            }
            if (model.Month_24 != null)
            {
                model.Month_24.Product = model.product;
                model.Month_24.isRecommended = false;
            }
         
            if (isRecommended == 12)
                model.Month_12.isRecommended = true;

            if (isRecommended == 24)
                model.Month_24.isRecommended = true;

            

            if (model.Month_12 != null && (model.Month_12.SalesPrice > 0 || model.Month_12.ID > 0 || model.product.InternalProductCode.Equals("freessl", StringComparison.OrdinalIgnoreCase)))
            {
                if (model.Month_12.ID > 0 && model.Month_12.SalesPrice > 0)
                {

                    _productPricing.Update(model.Month_12);
                }
                else if (model.Month_12.ID > 0 && model.Month_12.SalesPrice <= 0 && !model.product.InternalProductCode.Equals("freessl", StringComparison.OrdinalIgnoreCase))
                    _productPricing.Delete(_productPricing.FindByID(model.Month_12.ID));
                else if (model.Month_12.ID <= 0)
                    _productPricing.Add(model.Month_12);
            }

            if (model.Month_24 != null && (model.Month_24.SalesPrice > 0 || model.Month_24.ID > 0))
            {
                if (model.Month_24.ID > 0 && model.Month_24.SalesPrice > 0)
                    _productPricing.Update(model.Month_24);
                else if (model.Month_24.ID > 0 && model.Month_24.SalesPrice <= 0)
                    _productPricing.Delete(_productPricing.FindByID(model.Month_24.ID));
                else
                    _productPricing.Add(model.Month_24);
            }

          

            if (!SaveOnlyPrice)
            {
                if (model.product.ID > 0)
                {
                    _product.Update(model.product);
                }

                model.productAvailablity.Product = model.product;


                if (model.productAvailablity.ID > 0)
                    _productavailablity.Update(model.productAvailablity);
                else
                    _productavailablity.Add(model.productAvailablity);
            }
            _unitOfWork.Commit();
            return true;
        }
        public ProductAvailablity GetProductAvailablity(int ProductID)
        {
            return _productavailablity.Find(pa => pa.ProductID == ProductID).FirstOrDefault();
        }
        public IQueryable<ProductAvailablity> GetAllProductAvailablity(int SiteID)
        {
            return _productavailablity.Find(pa => pa.SiteID == SiteID).EagerLoad(x => x.Product);
        }
        public Contract GetClientContract(int SiteID)
        {
            Contract objContract = _contract.Find(cn => cn.SiteID == SiteID && cn.isAutoCalculation == false && cn.isForReseller == false && cn.ContractLevel == null).FirstOrDefault();
            if (objContract != null)
                return objContract;
            else
            {
                objContract = new Contract();
                objContract.ContractLevel = null;
                objContract.ContractName = "Client Contract";
                objContract.isAutoCalculation = false;
                objContract.isForReseller = false;
                objContract.SiteID = SiteID;
                objContract.RecordStatusID = (int)RecordStatus.ACTIVE;
                objContract = _contract.Add(objContract);
                _unitOfWork.Commit();
                return objContract;
            }
        }
        public List<StringBuilder> ImportProducts(List<Gateways.RestAPIModels.Response.ALLProduct> ProductList, Site site, decimal Margin)
        {
            ProductPricing resellerProductPricing = null;
            Contract ClientContract = GetClientContract(site.ID);
            StringBuilder SuccessProduct = new StringBuilder();
            StringBuilder UnSuccessProduct = new StringBuilder();

            var ProductSlugs = GeneralHelper.GetProductDetailSlugs();

            Contract resellerContract = _contract.Find(cn => cn.SiteID == site.ID && cn.isAutoCalculation == false && cn.isForReseller == true && cn.ContractLevel == null && cn.RecordStatusID != (int)RecordStatus.DELETED).OrderBy(cn => cn.ID).FirstOrDefault();
            if (resellerContract != null)
                resellerProductPricing = _productPricing.Find(pp => pp.ContractID == resellerContract.ID && pp.SiteID == site.ID).FirstOrDefault();



            foreach (Gateways.RestAPIModels.Response.ALLProduct apiProduct in ProductList)
            {
                try
                {
                    ProductPricingModel model = new ProductPricingModel();                    
                    
                    var productavailablity = _productavailablity.Find(pro => pro.Product.InternalProductCode.Equals(apiProduct.ProductCode, StringComparison.OrdinalIgnoreCase) && pro.SiteID == site.ID && pro.Product.RecordStatusID != (int)RecordStatus.DELETED).EagerLoad(p => p.Product).FirstOrDefault(); 
                    var sl = ProductSlugs.Where(x => x.ProductCode.Equals(apiProduct.ProductCode, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                    if (productavailablity == null)
                    {
                        //Create Product Row                                              
                        Product objProduct = new Product();
                        apiProduct.Brand = apiProduct.Brand.ToLower() == "verisign" ? "symantec" : apiProduct.Brand;
                        objProduct.BrandID = GetBrand().Where(br => br.BrandName.Equals(apiProduct.Brand, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().ID;
                        objProduct.CanbeReissued = apiProduct.CanbeReissued;
                        objProduct.InternalProductCode = apiProduct.ProductCode;
                        objProduct.isCompetitiveUpgradeAllowed = apiProduct.IsCompetitiveUpgradeSupported;
                        objProduct.isNoOfServerFree = apiProduct.isNoOfServerFree;
                        objProduct.isSANEnabled = apiProduct.IsSanEnable;
                        objProduct.isWildcard = apiProduct.IsWildCardProduct;
                        objProduct.ProductDescription = apiProduct.ProductName;
                        objProduct.ProductName = apiProduct.ProductName;
                        objProduct.ProductTypeID = apiProduct.ProductType;
                        objProduct.RecordStatusID = (int)RecordStatus.ACTIVE;
                        objProduct.RefundDays = apiProduct.RefundDays;
                        objProduct.ReissueDays = apiProduct.ReissueDays;
                        objProduct.ReissueType = "Included";
                        objProduct.SanMax = apiProduct.SanMax;
                        objProduct.SanMin = apiProduct.SanMin;

                        objProduct.DetailPageslug = sl != null ? sl.SlugUrl : string.Empty;
                        model.product = objProduct;

                        ProductAvailablity objAvailablity = new ProductAvailablity();
                        objAvailablity.isActive = true;
                        objAvailablity.Product = objProduct;
                        objAvailablity.SiteID = site.ID;
                        model.productAvailablity = objAvailablity;

                        SuccessProduct.Append(SetMarginalPrice(model, apiProduct, site, Margin, ClientContract.ID, false));

                        //Add reseller pricing for default reseller contract
                        if (resellerContract != null && resellerProductPricing == null)
                            SetMarginalPrice(model, apiProduct, site, Margin, resellerContract.ID, false);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(productavailablity.Product.DetailPageslug))
                            productavailablity.Product.DetailPageslug = sl != null ? sl.SlugUrl : string.Empty;

                        model.product = productavailablity.Product;
                        model.productAvailablity = productavailablity;

                        model.Month_12 = _productPricing.Find(pp => pp.SiteID == site.ID && pp.ProductID == productavailablity.ProductID && pp.ContractID == ClientContract.ID && (pp.NumberOfMonths < 12 ? 12 : pp.NumberOfMonths) == (int)SettingConstants.NumberOfMonths.Month12).FirstOrDefault();
                        model.Month_24 = _productPricing.Find(pp => pp.SiteID == site.ID && pp.ProductID == productavailablity.ProductID && pp.ContractID == ClientContract.ID && pp.NumberOfMonths == (int)SettingConstants.NumberOfMonths.Month24).FirstOrDefault();
                     
                        SuccessProduct.Append(SetMarginalPrice(model, apiProduct, site, Margin, ClientContract.ID, true));

                        //Add reseller pricing for default reseller contract
                        if (resellerContract != null && resellerProductPricing == null)
                        {
                            model.Month_12 = _productPricing.Find(pp => pp.SiteID == site.ID && pp.ContractID == resellerContract.ID && pp.ProductID == productavailablity.ProductID && pp.NumberOfMonths == (int)SettingConstants.NumberOfMonths.Month12).FirstOrDefault();
                            model.Month_24 = _productPricing.Find(pp => pp.SiteID == site.ID && pp.ContractID == resellerContract.ID && pp.ProductID == productavailablity.ProductID && pp.NumberOfMonths == (int)SettingConstants.NumberOfMonths.Month24).FirstOrDefault();
                            SetMarginalPrice(model, apiProduct, site, Margin, resellerContract.ID, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log("Error while importing product : " + (apiProduct != null ? apiProduct.ProductName : string.Empty), Logger.LogType.INFO);
                    _logger.LogException(ex);
                }
            }

            List<StringBuilder> lstReturn = new List<StringBuilder>();
            lstReturn.Add(SuccessProduct);
            lstReturn.Add(UnSuccessProduct);
            return lstReturn;
        }

        public Hashtable Gethastable()
        {

            Hashtable hastable = new Hashtable();

            //string path = 
            //var a = Request.RequestContext.HttpContext.Server.MapPath("."); 
            hastable.Add("8", "/comodo/codesigning");
            hastable.Add("301", "/comodo/essentialssl");
            hastable.Add("343", "/comodo/essentialssl-wildcard");
            hastable.Add("410", "/comodo/ev-ssl-multi-domain");
            hastable.Add("338", "/comodo/ev-ssl-sgc");
            hastable.Add("337", "/comodo/comodo-ev-ssl");
            hastable.Add("346", "");
            hastable.Add("24", "/comodo/instantssl");
            hastable.Add("34", "/comodo/instantssl-pro");
            hastable.Add("44", "");
            hastable.Add("335", "/comodo/comodo-multi-domain-ssl");
            hastable.Add("287", "/comodo/positivessl");
            hastable.Add("289", "");
            hastable.Add("7", "/comodo/premiumssl");
            hastable.Add("35", "/comodo/premiumssl-wildcard");
            hastable.Add("317", "/comodo/ssl-certificate-sgc");
            hastable.Add("323", "/comodo/ssl-certificate-sgc-wildcard");
            hastable.Add("488", "");
            hastable.Add("361", "/comodo/comodo-ucc-ssl");
            hastable.Add("489", "");
            hastable.Add("freessl", "/rapidssl/freessl");
            hastable.Add("quicksslpremium", "/geotrust/quickssl-premium");
            hastable.Add("truebusinessidevmd", "/geotrust/true-businessid-with-ev-multi-domain");
            hastable.Add("truebizid", "/geotrust/true-businessid");
            hastable.Add("truebusinessidwildcard", "/geotrust/true-businessid-wildcard");
            hastable.Add("truebusinessidev", "/geotrust/true-businessid-with-ev");
            hastable.Add("truebusinessidev_CU", "");
            hastable.Add("truebizidmd", "/geotrust/true-businessid-multi-domain");
            hastable.Add("malwarescan", "");
            hastable.Add("rapidssl", "/rapidssl/rapidssl-certificate");
            hastable.Add("rapidssl_CU", "/rapidssl/rapidssl-certificate");
            hastable.Add("rapidsslwildcard", "/rapidssl/rapidssl-wildcard");
            hastable.Add("thawteCSC", "/thawte/code-signing");
            hastable.Add("sgcsupercerts", "/thawte/sgc-supercerts");
            hastable.Add("SSLWebServer", "/thawte/ssl-webserver-certificates");
            hastable.Add("ssl123", "/thawte/ssl123");
            hastable.Add("SSLWebServerEV", "/thawte/ssl-webserver-ev");
            hastable.Add("SSLWebServerWildcard", "/thawte/ssl-webserver-wildcard");
            hastable.Add("TKPCIDSS", "");
            hastable.Add("TW21", "/trustwave/trustkeeper-plus-ev-ssl");
            hastable.Add("TW18", "/trustwave/trustkeeper-ssl-plus");
            hastable.Add("TW30", "/trustwave/trustwave-domain-validated-ssl");
            hastable.Add("TW4", "/trustwave/trustwave-enterprise-ssl");
            hastable.Add("TW10", "/trustwave/trustwave-premium-ev-ssl");
            hastable.Add("TW1", "/trustwave/trustwave-premium-ssl");
            hastable.Add("TW7", "/trustwave/trustwave-premium-wildcard-ssl");
            hastable.Add("TW14", "/trustwave/trustwave-secure-email-digital-id");
            hastable.Add("verisigncsc", "/symantec/symantec-code-signing");
            hastable.Add("TrustSealOrg", "/symantec/symantec-safe-site");
            hastable.Add("SecureSite", "/symantec/secure-site");
            hastable.Add("SecureSitePro", "/symantec/secure-site-pro");
            hastable.Add("SecureSiteProEV", "/symantec/secure-site-pro-with-ev");
            hastable.Add("SecureSiteEV", "/symantec/secure-site-with-ev");

            hastable.Add("ubasicid", "/certum/basicid-certificate");
            hastable.Add("uprofessionalid", "/certum/professionalid-certificate");
            hastable.Add("uenterpriseid", "/certum/enterpriseid-certificate");
            hastable.Add("ucommercialssl", "/certum/commercial-ssl-certificate");
            hastable.Add("ucommercialwildcard", "/certum/commercial-ssl-wild-card");
            hastable.Add("utrustedssl", "");
            hastable.Add("utrustedwildcard", "");



            return hastable;
        }
        public string SetMarginalPrice(ProductPricingModel model, Gateways.RestAPIModels.Response.ALLProduct apiProduct, Site site, decimal Margin, int ContractID, bool SaveOnlyPrice)
        {
            try
            {
                if (apiProduct.ProductCode.Equals("rapidssl"))
                { 
                
                }
                if (!SaveOnlyPrice)
                {
                    model.Month_12 = new ProductPricing() { NumberOfMonths = (int)SettingConstants.NumberOfMonths.Month12, RecordStatusID = (int)RecordStatus.ACTIVE, SiteID = site.ID, ContractID = ContractID };
                    model.Month_24 = new ProductPricing() { NumberOfMonths = (int)SettingConstants.NumberOfMonths.Month24, RecordStatusID = (int)RecordStatus.ACTIVE, SiteID = site.ID, ContractID = ContractID };
                   
                }
                if (apiProduct.Pricings.Where(pr => pr.NumberOfMonth == 12).FirstOrDefault() != null || apiProduct.Pricings.Where(pr => pr.NumberOfMonth == 1).FirstOrDefault() != null)
                {
                    var apiPrice = apiProduct.Pricings.Where(pr => pr.NumberOfMonth == 12 || pr.NumberOfMonth == 1).FirstOrDefault();
                    model.Month_12.AdditionalSanPrice = apiPrice.AdditionalSanPrice + ((apiPrice.AdditionalSanPrice * Margin) / 100);
                    model.Month_12.ContractID = ContractID;
                    model.Month_12.RecordStatusID = (int)RecordStatus.ACTIVE;
                    model.Month_12.RetailPrice = apiPrice.SRP;
                    if (apiPrice.Price > 0)
                        model.Month_12.SalesPrice = apiPrice.Price + ((apiPrice.Price * Margin) / 100);
                    else
                        model.Month_12.SalesPrice = 0;
                    model.Month_12.SiteID = site.ID;
                    if (apiPrice.NumberOfMonth == 1)
                        model.Month_12.NumberOfMonths = 1;
                }

                if (apiProduct.Pricings.Where(pr => pr.NumberOfMonth == 24).FirstOrDefault() != null)
                {
                    var apiPrice = apiProduct.Pricings.Where(pr => pr.NumberOfMonth == 24).FirstOrDefault();
                    model.Month_24.AdditionalSanPrice = apiPrice.AdditionalSanPrice + ((apiPrice.AdditionalSanPrice * Margin) / 100);
                    model.Month_24.ContractID = ContractID;
                    model.Month_24.RecordStatusID = (int)RecordStatus.ACTIVE;
                    model.Month_24.RetailPrice = apiPrice.SRP;
                    if (apiPrice.Price > 0)
                        model.Month_24.SalesPrice = apiPrice.Price + ((apiPrice.Price * Margin) / 100);
                    else
                        model.Month_24.SalesPrice = 0;
                    model.Month_24.SiteID = site.ID;
                }

             
                int isRecommended = 24;
            
               

                AddEditProductPricing(model, isRecommended, site, false);
                //_logger.Log(model.product.ProductName + ": Imported Successfully.", Logger.LogType.INFO);
                return model.product.ProductName + ": Successfully imported<br/>";
            }
            catch (Exception ex)
            {
                _logger.Log(model.product.ProductName + ": Error While Import ErrorMessage:" + ex.Message, Logger.LogType.INFO);
                return model.product.ProductName + ": Error while import<br/>";
            }
        }

        public bool ImportProductsInContract(Site site, decimal Margin, int ContractID)
        {
            var ClientContract = GetClientContract(site.ID);
            var ProductPriceList = _productPricing.Find(price => price.SiteID == site.ID && price.ContractID == ClientContract.ID);
            if (ProductPriceList != null && ProductPriceList.Count() > 0)
            {
                foreach (ProductPricing pricing in ProductPriceList)
                {
                    ProductPricing objNewPrice = new ProductPricing();
                    objNewPrice.isRecommended = pricing.isRecommended;
                    objNewPrice.NumberOfMonths = pricing.NumberOfMonths;

                    objNewPrice.ProductID = pricing.ProductID;

                    objNewPrice.RecordStatusID = pricing.RecordStatusID;
                    objNewPrice.RetailPrice = pricing.RetailPrice;
                    if (pricing.AdditionalSanPrice > 0)
                        objNewPrice.AdditionalSanPrice = pricing.AdditionalSanPrice - ((pricing.AdditionalSanPrice * Margin) / 100);
                    else
                        objNewPrice.AdditionalSanPrice = 0;
                    objNewPrice.ContractID = ContractID;
                    objNewPrice.Contract = null;
                    if (pricing.SalesPrice > 0)
                        objNewPrice.SalesPrice = pricing.SalesPrice - ((pricing.SalesPrice * Margin) / 100);
                    else
                        objNewPrice.SalesPrice = 0;

                    objNewPrice.SiteID = pricing.SiteID;
                    _productPricing.Add(objNewPrice);

                }
                _unitOfWork.Commit();
            }
            return true;
        }
    }
}
