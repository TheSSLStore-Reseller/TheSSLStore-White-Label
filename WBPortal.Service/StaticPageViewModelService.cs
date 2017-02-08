using System.Linq;
using WBSSLStore.Domain;
using WBSSLStore.Data.Infrastructure;
using System.Collections.Generic;
using System;
using System.Web;

namespace WBSSLStore.Service
{

    public interface IStaticPageViewModelService
    {
        List<ProductPricing> GetProductPricing(int SiteID, int ProductID, int contractid, int BrandID = 0, string code = "");

        List<ProductPricing> GetProductPricingByDetailSlug(int SiteID, int contractid, string Slug);
        List<ProductPricing> GetAllProductPricing(int SiteID, int contractid, int BrandID = 0, string code = "");
        CMSPage GetPageMetadata(int SiteID, int langId, string slug);
        CMSPageContent GetPageContent(int CMSPageID);
        string SendForgotPasswordEmail(string Email, int CurrentLangID, int SmtpID, int SiteID, string strPath);
        int SaveReseller(User user, int SiteID, int LangID, int SMTPID, string SiteAdmin);
    }
    public class StaticPageViewModelService : IStaticPageViewModelService
    {

        private readonly IRepository<ProductPricing> _productPricing;
        private readonly IRepository<Product> _products;
        private readonly IRepository<CMSPage> _CMSPage;
        private readonly IRepository<CMSPageContent> _CMSPageContent;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<User> _User;
        //private readonly IRepository<EmailTemplates> _EmailTemplates;
        private readonly IEmailQueueService _EmailQueueService;
        private readonly IRepository<ResellerContract> _ResellerContract;
        private readonly IRepository<Contract> _Contract;

        public StaticPageViewModelService(IRepository<ProductPricing> ProductPricing, IRepository<CMSPage> CMSPage, IRepository<CMSPageContent> CMSPageContent, IUnitOfWork UnitOfWork, IRepository<User> User,
             IEmailQueueService pEmailQueueService, IRepository<ResellerContract> ResellerContract, IRepository<Contract> Contract, IRepository<Product> ProductsData)
        {

            _productPricing = ProductPricing;
            _CMSPage = CMSPage;
            _CMSPageContent = CMSPageContent;
            _unitOfWork = UnitOfWork;
            _User = User;
            //_EmailTemplates = EmailTemplates;
            _EmailQueueService = pEmailQueueService;
            _ResellerContract = ResellerContract;
            _Contract = Contract;
            _products = ProductsData;
        }

        public int SaveReseller(User user, int SiteID, int LangID, int SMTPID, string SiteAdmin)
        {
            User dupplicate = _User.Find(u => u.Email.ToLower().Equals(user.Email) && u.SiteID == SiteID).FirstOrDefault();
            if (dupplicate == null)
            {
                user.AuditDetails = new Audit();
                user.AuditDetails.DateCreated = System.DateTimeWithZone.Now;
                user.AuditDetails.DateModified = System.DateTimeWithZone.Now;
                user.AuditDetails.ByUserID = user.ID;
                user.AuditDetails.HttpHeaderDump = HttpUtility.UrlDecode(System.Web.HttpContext.Current.Request.Headers.ToString());
                user.AuditDetails.IP = System.Web.HttpContext.Current.Request.UserHostAddress;

                user.UserTypeID = (int)UserType.RESELLER;
                user.Address.CompanyName = user.CompanyName;
                user.Address.Email = user.Email;
                user.ConfirmPassword = user.PasswordHash;

                user.SiteID = SiteID;

                Contract contrat = _Contract.Find(C => C.ContractLevel > 0 && C.isAutoCalculation == true && C.isForReseller == true && C.SiteID == SiteID && C.RecordStatusID == (int)RecordStatus.ACTIVE).OrderBy(C => C.ContractLevel).FirstOrDefault();
                if (contrat == null)
                    contrat = _Contract.Find(C => C.isAutoCalculation == false && C.isForReseller == true && C.SiteID == SiteID && C.RecordStatusID == (int)RecordStatus.ACTIVE).OrderBy(C => C.ID).FirstOrDefault();

                if (contrat == null)
                {
                    contrat = new Contract();
                    contrat.ContractLevel = 0;
                    contrat.ContractName = "Reseller Default Contract";
                    contrat.isAutoCalculation = false;
                    contrat.isForReseller = true;
                    contrat.RecordStatusID = (int)RecordStatus.ACTIVE;
                    contrat.SiteID = SiteID;

                    _Contract.Add(contrat);
                    _unitOfWork.Commit();

                }

                ResellerContract rc = new ResellerContract();
                rc.AuditDetails = user.AuditDetails;
                rc.ContractID = contrat.ID;
                rc.Reseller = user;
                rc.SiteID = SiteID;

                _ResellerContract.Add(rc);
                _unitOfWork.Commit();

                //Add EmailQueue
                User _newuser = _User.Find(u => u.ID == user.ID).EagerLoad(u => u.Address, u => u.Address.Country).FirstOrDefault();
                _EmailQueueService.PrepareEmailQueue(SiteID, LangID, EmailType.RESELLER_WELCOME_EMAIL, SMTPID, _newuser.Email, _newuser);

                //TO DO: set admin email address
                _EmailQueueService.PrepareEmailQueue(SiteID, LangID, EmailType.ADMIN_NEW_RESELLER, SMTPID, SiteAdmin, _newuser);
                _unitOfWork.Commit();
                // End


            }
            else
                return -1;

            return 1;
        }

        public List<ProductPricing> GetProductPricingByDetailSlug(int SiteID, int contractid, string Slug)
        {
            int ProductID = 0;

            if (string.IsNullOrEmpty(Slug))
                return null;

            var Prod = _products.Find(x => x.DetailPageslug.Equals(Slug, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (Prod != null && Prod.ID > 0)
                ProductID = Prod.ID;
            else
                return null;

            if (ProductID > 0)
                return _productPricing.Find(p => p.SiteID == SiteID && p.Product.RecordStatusID == (int)RecordStatus.ACTIVE && p.ProductID == ProductID && p.RecordStatusID == (int)RecordStatus.ACTIVE && p.ContractID == contractid).EagerLoad(x => x.Product.Brand).ToList();
            else
                return _productPricing.Find(p => p.SiteID == SiteID && p.Product.RecordStatusID == (int)RecordStatus.ACTIVE && p.RecordStatusID == (int)RecordStatus.ACTIVE && p.ContractID == contractid).ToList();

        }

        public List<ProductPricing> GetProductPricing(int SiteID, int ProductID, int contractid, int BrandID = 0, string code = "")
        {
            if (ProductID > 0)
                return _productPricing.Find(p => p.SiteID == SiteID && p.Product.RecordStatusID == (int)RecordStatus.ACTIVE && p.ProductID == ProductID && p.RecordStatusID == (int)RecordStatus.ACTIVE && p.ContractID == contractid && p.Product.BrandID.Equals(BrandID == 0 ? p.Product.BrandID : BrandID)).ToList();
            else if (!string.IsNullOrEmpty(code) && ProductID == 0)
            {
                string[] codes = code.Split(',');
                return _productPricing.Find(p => p.SiteID == SiteID && p.Product.RecordStatusID == (int)RecordStatus.ACTIVE && codes.Contains(p.Product.InternalProductCode) && p.RecordStatusID == (int)RecordStatus.ACTIVE && p.ContractID == contractid && p.Product.BrandID.Equals(BrandID == 0 ? p.Product.BrandID : BrandID)).EagerLoad(x => x.Product.Brand).ToList();
            }
            else
                return _productPricing.Find(p => p.SiteID == SiteID && p.Product.RecordStatusID == (int)RecordStatus.ACTIVE && p.RecordStatusID == (int)RecordStatus.ACTIVE && p.ContractID == contractid && p.Product.BrandID.Equals(BrandID == 0 ? p.Product.BrandID : BrandID)).ToList();

        }
        public List<ProductPricing> GetAllProductPricing(int SiteID, int contractid, int BrandID = 0, string code = "")
        {
            if (BrandID > 0)
                return _productPricing.Find(p => p.SiteID == SiteID && p.Product.RecordStatusID == (int)RecordStatus.ACTIVE && p.Product.BrandID.Equals(BrandID) && p.RecordStatusID == (int)RecordStatus.ACTIVE && p.ContractID == contractid && p.Product.BrandID.Equals(BrandID == 0 ? p.Product.BrandID : BrandID)).EagerLoad(x => x.Product.Brand).ToList();
            else if (!string.IsNullOrEmpty(code))
            {
                string[] codes = code.Split(',');
                return _productPricing.Find(p => p.SiteID == SiteID && p.Product.RecordStatusID == (int)RecordStatus.ACTIVE && codes.Contains(p.Product.InternalProductCode) && p.RecordStatusID == (int)RecordStatus.ACTIVE && p.ContractID == contractid && p.Product.BrandID.Equals(BrandID == 0 ? p.Product.BrandID : BrandID)).EagerLoad(x => x.Product.Brand).ToList();
            }
            else
                return _productPricing.Find(p => p.SiteID == SiteID && p.Product.RecordStatusID == (int)RecordStatus.ACTIVE && p.RecordStatusID == (int)RecordStatus.ACTIVE && p.ContractID == contractid && p.Product.BrandID.Equals(BrandID == 0 ? p.Product.BrandID : BrandID)).EagerLoad(x => x.Product.Brand).ToList();

        }
        public CMSPage GetPageMetadata(int SiteID, int langId, string slug)
        {
            return _CMSPage.Find(c => c.Pages.SiteID == SiteID && c.LangID == langId && c.Pages.slug == slug).FirstOrDefault();
        }

        public CMSPageContent GetPageContent(int CMSPageID)
        {
            return _CMSPageContent.Find(c => c.CMSPageID == CMSPageID).FirstOrDefault();
        }

        public string SendForgotPasswordEmail(string Email, int CurrentLangID, int SmtpID, int SiteID, string strPath)
        {
            string RetVal = string.Empty;
            Domain.User objUser = _User.Find(u => u.Email.ToLower().Equals(Email.ToLower()) && u.SiteID == SiteID && u.RecordStatusID == (int)RecordStatus.ACTIVE).FirstOrDefault();

            if (objUser != null)
            {
                if (_EmailQueueService.PrepareEmailQueue(SiteID, CurrentLangID, EmailType.ALL_FORGOTPASSWORD, SmtpID, objUser.Email, objUser))
                {
                    _unitOfWork.Commit();
                    RetVal = "<div class='alert-success'>Password reset link sent successfully.</div>";
                }
                else
                    RetVal = "<div class='errormsg'>Unable to send email, EmailTemplate is not set.</div>";
            }
            else
                RetVal = "<div class='errormsg'>Invalid Email Address</div>";
            return RetVal;
        }

        public bool SendMailContactus(string CompanyName, string Name, string Phone, string Email, string Comment, int SiteID, int LangID, int SmtpID)
        {

            return false;

        }
    }


}
