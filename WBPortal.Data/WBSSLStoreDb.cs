using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

using WBSSLStore.Domain;
using System.Data;

namespace WBSSLStore.Data
{

    //public class MigrationsContextFactory : System.Data.Entity.Infrastructure.IDbContextFactory<WBSSLStoreDb>
    //{
    //    public WBSSLStoreDb Create()
    //    {
    //        return new WBSSLStoreDb("connectionStringName");
    //    }
    //}
    public class WBSSLStoreDb : DbContext
    {
        
        //Domain DbSets here        
        public DbSet<Site> Sites { get; set; }
        public DbSet<SiteSettings> Settings { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Audit> Audits { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<CertificateContact> CertificateContacts { get; set; }
        public DbSet<CertificateRequest> CertificateRequests { get; set; }
        public DbSet<CMSPage> CmsPages { get; set; }
        public DbSet<CMSPageContent> CmsPageContents { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<EmailQueue> EmailQueues { get; set; }
        public DbSet<EmailTemplates> EmailTemplateses { get; set; }
        public DbSet<GatewayInteraction> GatewayInteractions { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PayPalData> PayPalDatas { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductAvailablity> ProductAvailablities { get; set; }
        public DbSet<ProductPricing> ProductPricings { get; set; }
        public DbSet<PromoCode> PromoCodes { get; set; }
        public DbSet<ResellerContract> ResellerContracts { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<ShoppingCartDetail> ShoppingCartDetails { get; set; }
        public DbSet<SiteSMTP> SiteSmtps { get; set; }
        public DbSet<SupportDetail> SupportDetails { get; set; }
        public DbSet<SupportRequest> SupportRequests { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserTransaction> UserTransactions { get; set; }
        public DbSet<WebServerType> WebServerTypes { get; set; }
        public DbSet<PaymentGateways> PaymentGateways { get; set; }
        public DbSet<Pages> Pages { get; set; }
        public DbSet<Testimonials> Testimonials { get; set; }
        public DbSet<ProductDetail> ProductDetail { get; set; }

        public DbSet<UserOptions> UserOptions { get; set; }

        public WBSSLStoreDb()
            : base("WhiteLabelConnection")
        {
           
        }

        public WBSSLStoreDb(string ConnectionString)
            : base(ConnectionString)
        {

        }



        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Custom Model Logic
            

            // modelBuilder.Conventions.Remove<System.Data.Entity.Infrastructure.IncludeMetadataConvention>();
            modelBuilder.Entity<OrderDetail>().Ignore(x => x.InvoiceNumber);

            //Site Languages
            modelBuilder.Entity<Site>()
                .HasMany<Language>(r => r.SupportedLanguages).
                WithMany(u => u.Sites).Map(m =>
                                               {
                                                   m.ToTable("SiteLanguages");
                                                   m.MapLeftKey("SiteID");
                                                   m.MapRightKey("LangID");
                                               });


            modelBuilder.Entity<CertificateRequest>()
                .HasRequired(c => c.BillingContact)
                .WithMany()
                .HasForeignKey(c => c.BillingContactID)
                .WillCascadeOnDelete(false);


            modelBuilder.Entity<CertificateRequest>()
                .HasRequired(c => c.AdminContact)
                .WithMany()
                .HasForeignKey(c => c.AdminContactID)
                .WillCascadeOnDelete(false);


            modelBuilder.Entity<CertificateRequest>()
                .HasRequired(c => c.TechnicalContact)
                .WithMany()
                .HasForeignKey(c => c.TechnicalContactID)
                .WillCascadeOnDelete(false);


            modelBuilder.Entity<OrderDetail>()
                .HasRequired(o => o.Product)
                .WithMany()
                .HasForeignKey(o => o.ProductID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrderDetail>()
                .HasRequired(o => o.AuditDetails)
                .WithMany()
                .HasForeignKey(o => o.AuditID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Payment>()
                .HasRequired(o => o.AuditDetails)
                .WithMany()
                .HasForeignKey(o => o.AuditID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PaymentGateways>()
               .HasRequired(o => o.AuditDetails)
               .WithMany()
               .HasForeignKey(o => o.AuditID)
               .WillCascadeOnDelete(false);

            modelBuilder.Entity<PaymentGateways>()
               .HasRequired(o => o.Site)
               .WithMany()
               .HasForeignKey(o => o.SiteID)
               .WillCascadeOnDelete(false);

            modelBuilder.Entity<ProductPricing>()
                .HasRequired(o => o.Site)
                .WithMany()
                .HasForeignKey(p => p.SiteID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PromoCode>()
                .HasRequired(o => o.Site)
                .WithMany()
                .HasForeignKey(p => p.SiteID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ResellerContract>()
                .HasRequired(o => o.Site)
                .WithMany()
                .HasForeignKey(o => o.SiteID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ResellerContract>()
                .HasRequired(o => o.AuditDetails)
                .WithMany()
                .HasForeignKey(o => o.AuditID)
                .WillCascadeOnDelete(false);


            modelBuilder.Entity<ShoppingCartDetail>()
                .HasRequired(o => o.ProductPricing)
                .WithMany()
                .HasForeignKey(o => o.ProductPriceID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SiteSMTP>()
                .HasRequired(o => o.AuditDetails)
                .WithMany()
                .HasForeignKey(o => o.AuditID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<EmailQueue>()
                .HasRequired(o => o.SiteSMTP)
                .WithMany()
                .HasForeignKey(o => o.SiteSMTPID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SupportRequest>()
                .HasRequired(o => o.AuditDetails)
                .WithMany()
                .HasForeignKey(o => o.AuditID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasRequired(o => o.AuditDetails)
                .WithMany()
                .HasForeignKey(o => o.AuditID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasRequired(o => o.Site)
                .WithMany()
                .HasForeignKey(o => o.SiteID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CertificateRequest>()
                .HasRequired(o => o.WebServerType)
                .WithMany()
                .HasForeignKey(o => o.WebServerID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
              .HasRequired(o => o.Address)
              .WithMany()
              .HasForeignKey(o => o.AddressID)
              .WillCascadeOnDelete(false);


            modelBuilder.Entity<UserOptions>().HasRequired(x => x.User).WithMany().HasForeignKey(x => x.UserID).WillCascadeOnDelete(false);
            //modelBuilder.Entity<UserOptions>().HasRequired(x => x.Site).WithMany().HasForeignKey(x => x.SiteID).WillCascadeOnDelete(false);

            //modelBuilder.Entity<Pages>()
            // .HasRequired(o => o.Site)
            // .WithMany()
            // .HasForeignKey(o => o.SiteID)
            // .WillCascadeOnDelete(false);

            //modelBuilder.Entity<CMSPage>()
            //.HasRequired(o => o.Pages)
            //.WithMany()
            //.HasForeignKey(o => o.Pages)
            //.WillCascadeOnDelete(false);



        }



        /// <summary>
        /// Saves all the Changes
        /// </summary>
        public virtual void Commit()
        {
            try
            {
                base.Configuration.ValidateOnSaveEnabled = false;
                base.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {

                StringBuilder sbError = new StringBuilder();
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        sbError.AppendLine("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
                throw new Exception(sbError.ToString());

            }
            catch (DBConcurrencyException updex)
            {

            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                throw ex;
            }
            catch (System.Data.Entity.Infrastructure.UnintentionalCodeFirstException ex1)
            {
                throw ex1;
            }

        }
    }


    //    public class WBSSLStoreDbInitializer :  DropCreateDatabaseIfModelChanges<WBSSLStoreDb>
    //    {
    //        protected override void Seed(WBSSLStoreDb context)
    //        {
    //            base.Seed(context);
    //            //Add Required Rows here. Occurs in Test Environment where database is bootstrapped for the first time.

    //            //Add Unique Constraints
    //            //context.Database.SqlQuery<object>("ALTER TABLE Users ADD CONSTRAINT uc_Email UNIQUE(Email)");



    //            Country country = new Country();
    //            country.ID = 1;
    //            country.CountryName = "United States";
    //            country.ISOName = "US";
    //            country.RecordStatus = RecordStatus.ACTIVE;

    //            context.Countries.Add(country);

    //            //    context.SaveChanges();

    //            Address address = new Address();
    //            address.City = "St Pete";
    //            address.CompanyName = "JCT WEB";
    //            address.Country = country;
    //            address.Fax = "1234567890";
    //            address.Mobile = "1234567890";
    //            address.Phone = "1234567890";
    //            address.State = "Florida";
    //            address.Street = "123 any street";
    //            address.Zip = "33701";


    //            context.Addresses.Add(address);

    //            //    context.SaveChanges();



    //            Language lang = new Language();
    //            lang.LangCode = "en";
    //            lang.LangName = "English";
    //            lang.RecordStatus = RecordStatus.ACTIVE;

    //            //Add Default stuff
    //            Site site = new Site();
    //            site.Alias = "wbstore.com";
    //            site.APIisInTest = true;
    //            site.APIPartnerCode = "82911446";
    //            site.APIPassword = "parag99";
    //            site.APIUsername = "parag@paragm.com";
    //            site.DateCreated = System.DateTimeWithZone.Now;
    //            site.isActive = true;
    //            site.SupportedLanguages = new List<Language>();
    //            site.SupportedLanguages.Add(lang);
    //            context.Sites.Add(site);


    //            //Add SiteSetting


    //            var ss = new List<WBSSLStore.Domain.SiteSettings>
    //            {
    //                new WBSSLStore.Domain.SiteSettings { Key="layout", Value="layout1", Site=site },
    //                new WBSSLStore.Domain.SiteSettings { Key="colorcode", Value="color1", Site=site },
    //                new WBSSLStore.Domain.SiteSettings { Key="needbanner", Value="false", Site=site },
    //                new WBSSLStore.Domain.SiteSettings { Key="sitephone", Value="123.456.7890", Site=site },
    //            };
    //            ss.ForEach(b => context.Settings.Add(b));

    //            //Add User
    //            User adminuser = new User();
    //            adminuser.AuditDetails = new Audit()
    //                                         {
    //                                             ByUserID = 0,
    //                                             DateCreated = DateTimeWithZone.Now,
    //                                             DateModified = null,
    //                                             HttpHeaderDump = "system",
    //                                             IP = "127.0.0.1"
    //                                         };

    //            adminuser.Site = site;
    //            adminuser.Email = "parag@paragm.com";
    //            adminuser.PasswordHash = "sa";
    //            adminuser.PasswordSalt = "";
    //            adminuser.RecordStatus = RecordStatus.ACTIVE;
    //            adminuser.UserType = UserType.ADMIN;
    //            adminuser.Address = address;
    //            adminuser.CompanyName = "Kaushal Info";
    //            adminuser.FirstName = "Parag";
    //            adminuser.LastName = "Mehta";

    //            context.Users.Add(adminuser);

    //            //reseller
    //            User reseller = new User();
    //            reseller.AuditDetails = new Audit()
    //            {
    //                ByUserID = 0,
    //                DateCreated = DateTimeWithZone.Now,
    //                DateModified = null,
    //                HttpHeaderDump = "system",
    //                IP = "127.0.0.1"
    //            };

    //            reseller.Site = site;
    //            reseller.Email = "vinit.patel@jctweb.com";
    //            reseller.PasswordHash = "123456";
    //            reseller.PasswordSalt = "";
    //            reseller.RecordStatus = RecordStatus.ACTIVE;
    //            reseller.UserType = UserType.RESELLER;
    //            reseller.Address = address;
    //            reseller.CompanyName = "Kaushal Info";
    //            reseller.FirstName = "Parag";
    //            reseller.LastName = "Mehta";

    //            context.Users.Add(reseller);

    //            //Add SMTP
    //            SiteSMTP s = new SiteSMTP();
    //            s.Site = site;
    //            s.SMTPHost = "192.168.100.7";
    //            s.SMTPPort = 25;
    //            s.UseSSL = false;
    //            s.TimeOut = 30;
    //            s.AuditDetails = new Audit()
    //            {
    //                ByUserID = 0,
    //                DateCreated = DateTimeWithZone.Now,
    //                DateModified = null,
    //                HttpHeaderDump = "system",
    //                IP = "127.0.0.1"
    //            };



    //            context.SiteSmtps.Add(s);

    //            //Add Brand
    //            var Brand = new List<WBSSLStore.Domain.Brand>
    //            {
    //                new WBSSLStore.Domain.Brand { BrandName = "RapidSSL",   ID  = 1, isActive  =true },
    //                new WBSSLStore.Domain.Brand {  BrandName = "GeoTrust",   ID  = 2, isActive  =true  },
    //                new WBSSLStore.Domain.Brand {  BrandName = "VeriSign",   ID  = 3, isActive  =true  },
    //                new WBSSLStore.Domain.Brand {  BrandName = "Thawte",   ID  =4, isActive  =true }

    //            };
    //            Brand.ForEach(b => context.Brands.Add(b));

    //             //Add WebServerType
    //            WebServerType WS = new WebServerType();
    //            WS.BrandID = 1;
    //            WS.isActive = true;
    //            WS.WebServerName = "MicroSoft IIS";
    //            context.WebServerTypes.Add(WS);   


    //            //Add Product
    //            //Add Brand
    //            var product = new List<WBSSLStore.Domain.Product>
    //            {
    //                new WBSSLStore.Domain.Product {BrandID=1,ID=1,ProductName="RapidSSL Certificate",ProductDescription="RapidSSL Certificate",InternalProductCode="rapidssl",isCompetitiveUpgradeAllowed=true,CanbeReissued=true,isSANEnabled=false,isWildcard=false,ProductTypeID=0,RecordStatusID=1,RefundDays=30,ReissueDays=10,SanMax=0,SanMin=0,ReissueType="Included" ,isNoOfServerFree=true} ,
    //                new WBSSLStore.Domain.Product {BrandID=1,ID=2,ProductName="RapidSSL WildCard Certificate",ProductDescription="RapidSSL WildCard Certificate",InternalProductCode="rapidsslwildcard",isCompetitiveUpgradeAllowed=true,CanbeReissued=true,isSANEnabled=false,isWildcard=true,ProductTypeID=0,RecordStatusID=1,RefundDays=30,ReissueDays=10,SanMax=0,SanMin=0,ReissueType="Included",isNoOfServerFree=true} ,
    //                new WBSSLStore.Domain.Product {BrandID=2,ID=3,ProductName="GeoTrust True Bussiness ID",ProductDescription="GeoTrust True Bussiness ID",InternalProductCode="truebizid",isCompetitiveUpgradeAllowed=true,CanbeReissued=true,isSANEnabled=false,isWildcard=false,ProductTypeID=0,RecordStatusID=1,RefundDays=30,ReissueDays=10,SanMax=0,SanMin=0,ReissueType="Included",isNoOfServerFree=false},
    //                 new WBSSLStore.Domain.Product {BrandID=2,ID=4,ProductName="GeoTrust True Bussiness ID with Multi Domain",ProductDescription="GeoTrust True Bussiness ID with Multi Domain",InternalProductCode="truebizidmd",isCompetitiveUpgradeAllowed=true,CanbeReissued=true,isSANEnabled=true,isWildcard=false,ProductTypeID=0,RecordStatusID=1,RefundDays=30,ReissueDays=10,SanMax=24,SanMin=3,ReissueType="Included",isNoOfServerFree=false}   
    //            };
    //            product.ForEach(p => context.Products.Add(p));

    //            //Add reseller Contract
    //            WBSSLStore.Domain.ResellerContract RC = new ResellerContract();
    //            //Add ProductPricing
    //            RC.ID = 1;
    //            RC.Site = site;
    //            RC.Reseller = adminuser;

    //            RC.Contract = new Contract { ID = 1, ContractName = "Client", isAutoCalculation = false, isForReseller = false, RecordStatus = RecordStatus.ACTIVE, Site = site, ContractLevel = null };
    //            RC.AuditDetails = new Audit()
    //            {
    //                ByUserID = 0,
    //                DateCreated = DateTimeWithZone.Now,
    //                DateModified = null,
    //                HttpHeaderDump = "system",
    //                IP = "127.0.0.1"
    //            };

    //            context.ResellerContracts.Add(RC);

    //            //Add reseller Contract
    //            WBSSLStore.Domain.ResellerContract resellerContract = new ResellerContract();
    //            //Add ProductPricing
    //            resellerContract.ID = 1;
    //            resellerContract.Site = site;
    //            resellerContract.Reseller = reseller;

    //            resellerContract.ContractID = 1;
    //            resellerContract.AuditDetails = new Audit()
    //            {
    //                ByUserID = 0,
    //                DateCreated = DateTimeWithZone.Now,
    //                DateModified = null,
    //                HttpHeaderDump = "system",
    //                IP = "127.0.0.1"
    //            };

    //            context.ResellerContracts.Add(resellerContract);

    //            var productpricing = new List<WBSSLStore.Domain.ProductPricing>
    //            {
    //                new WBSSLStore.Domain.ProductPricing {ID=1,AdditionalSanPrice=0, NumberOfMonths=12,isRecommended=false,ProductID=1,RecordStatusID=1,Contract=RC.Contract,RetailPrice=10,SalesPrice=20,Site=site  },
    //                new WBSSLStore.Domain.ProductPricing {ID=2,AdditionalSanPrice=0, NumberOfMonths=24,isRecommended=false,ProductID=1,RecordStatusID=1,Contract=RC.Contract,RetailPrice=20,SalesPrice=40,Site=site},
    //                new WBSSLStore.Domain.ProductPricing {ID=3,AdditionalSanPrice=0, NumberOfMonths=36,isRecommended=true,ProductID=1,RecordStatusID=1,Contract=RC.Contract,RetailPrice=30,SalesPrice=60,Site=site},
    //                new WBSSLStore.Domain.ProductPricing {ID=4,AdditionalSanPrice=0, NumberOfMonths=48,isRecommended=false,ProductID=1,RecordStatusID=1,Contract=RC.Contract,RetailPrice=40,SalesPrice=80,Site=site},
    //                new WBSSLStore.Domain.ProductPricing {ID=5,AdditionalSanPrice=0, NumberOfMonths=60,isRecommended=false,ProductID=1,RecordStatusID=1,Contract=RC.Contract,RetailPrice=50,SalesPrice=100,Site=site},

    //                 new WBSSLStore.Domain.ProductPricing {ID=6,AdditionalSanPrice=0, NumberOfMonths=12,isRecommended=false,ProductID=2,RecordStatusID=1,Contract=RC.Contract,RetailPrice=110,SalesPrice=220,Site=site},
    //                new WBSSLStore.Domain.ProductPricing {ID=7,AdditionalSanPrice=0, NumberOfMonths=24,isRecommended=false,ProductID=2,RecordStatusID=1,Contract=RC.Contract,RetailPrice=120,SalesPrice=240,Site=site},
    //                new WBSSLStore.Domain.ProductPricing {ID=8,AdditionalSanPrice=0, NumberOfMonths=36,isRecommended=true,ProductID=2,RecordStatusID=1,Contract=RC.Contract,RetailPrice=130,SalesPrice=260,Site=site},
    //                new WBSSLStore.Domain.ProductPricing {ID=9,AdditionalSanPrice=0, NumberOfMonths=48,isRecommended=false,ProductID=2,RecordStatusID=1,Contract=RC.Contract,RetailPrice=140,SalesPrice=280,Site=site},
    //                new WBSSLStore.Domain.ProductPricing {ID=10,AdditionalSanPrice=0, NumberOfMonths=60,isRecommended=false,ProductID=2,RecordStatusID=1,Contract=RC.Contract,RetailPrice=150,SalesPrice=300,Site=site},

    //                  new WBSSLStore.Domain.ProductPricing {ID=11,AdditionalSanPrice=0, NumberOfMonths=12,isRecommended=false,ProductID=3,RecordStatusID=1,Contract=RC.Contract,RetailPrice=110,SalesPrice=320,Site=site},
    //                new WBSSLStore.Domain.ProductPricing {ID=12,AdditionalSanPrice=0, NumberOfMonths=24,isRecommended=false,ProductID=3,RecordStatusID=1,Contract=RC.Contract,RetailPrice=120,SalesPrice=340,Site=site},
    //                new WBSSLStore.Domain.ProductPricing {ID=13,AdditionalSanPrice=0, NumberOfMonths=36,isRecommended=true,ProductID=3,RecordStatusID=1,Contract=RC.Contract,RetailPrice=130,SalesPrice=360,Site=site},
    //                new WBSSLStore.Domain.ProductPricing {ID=14,AdditionalSanPrice=0, NumberOfMonths=48,isRecommended=false,ProductID=3,RecordStatusID=1,Contract=RC.Contract,RetailPrice=140,SalesPrice=380,Site=site},
    //                new WBSSLStore.Domain.ProductPricing {ID=15,AdditionalSanPrice=0, NumberOfMonths=60,isRecommended=false,ProductID=3,RecordStatusID=1,Contract=RC.Contract,RetailPrice=150,SalesPrice=400,Site=site},

    //                  new WBSSLStore.Domain.ProductPricing {ID=16,AdditionalSanPrice=50, NumberOfMonths=12,isRecommended=false,ProductID=4,RecordStatusID=1,Contract=RC.Contract,RetailPrice=110,SalesPrice=320,Site=site},
    //                new WBSSLStore.Domain.ProductPricing {ID=17,AdditionalSanPrice=50, NumberOfMonths=24,isRecommended=false,ProductID=4,RecordStatusID=1,Contract=RC.Contract,RetailPrice=120,SalesPrice=340,Site=site},
    //                new WBSSLStore.Domain.ProductPricing {ID=18,AdditionalSanPrice=50, NumberOfMonths=36,isRecommended=true,ProductID=4,RecordStatusID=1,Contract=RC.Contract,RetailPrice=130,SalesPrice=360,Site=site},
    //                new WBSSLStore.Domain.ProductPricing {ID=19,AdditionalSanPrice=50, NumberOfMonths=48,isRecommended=false,ProductID=4,RecordStatusID=1,Contract=RC.Contract,RetailPrice=140,SalesPrice=380,Site=site},
    //                new WBSSLStore.Domain.ProductPricing {ID=20,AdditionalSanPrice=50, NumberOfMonths=60,isRecommended=false,ProductID=4,RecordStatusID=1,Contract=RC.Contract,RetailPrice=150,SalesPrice=400,Site=site}
    //            };
    //            productpricing.ForEach(p => context.ProductPricings.Add(p));
    //            //Add ProductAvibility

    //            context.ProductAvailablities.Add(new ProductAvailablity { isActive = true, ProductID = 1, Site = site });
    //            context.ProductAvailablities.Add(new ProductAvailablity { isActive = true, ProductID = 2, Site = site });

    //            //Add CMS Page
    //            context.Pages.Add(new Pages { BrandID=0,  Site = site, Caption="Home", PageStatusID = (int)PageStatus.Show ,ParentID=0,slug="/",ID=1     });
    //            context.Pages.Add(new Pages { BrandID = 0, Site = site, Caption = "RapidSSL", PageStatusID = (int)PageStatus.Show, ParentID = 0, slug = "/rapidssl", ID = 2 });
    //            context.Pages.Add(new Pages { BrandID = 0, Site = site, Caption = "Aboutus", PageStatusID = (int)PageStatus.Show, ParentID = 0, slug = "/about", ID = 3 });


    //            context.CmsPages.Add(new CMSPage { Description = "WBStore.com", Keywords = "WBSSLStore.com", Language = lang,   Title = "WBSSLStore.com",PageID =1 });
    //            context.CmsPages.Add(new CMSPage { Description = "WBStore.com, Rapid SSL Certificate",  Keywords = "Rapid SSL Certificate,Rapid SSL Certificate", Language = lang, PageID  = 2,  Title = "Rapid SSL Certificate-WBSSLStore.com" });
    //            context.CmsPages.Add(new CMSPage { Description = "About US", Keywords = "About US", Language = lang, PageID = 3, Title = "About US" });

    //          //Add PaymentGateway
    //            PaymentGateways PG = new PaymentGateways();
    //            PG.Name = "Authorize.Net";
    //            PG.AcceptCards = "1,2";
    //            PG.AuditID = 1;
    //            PG.InstancesID = 2;
    //            PG.IsTestMode = true;
    //            PG.LiveURL = "https://secure.authorize.net/gateway/transact.dll";
    //            PG.Site = site;
    //            PG.TestURL = "https://test.authorize.net/gateway/transact.dll";
    //            PG.TransactionKey = "SR2P8g4jdEn7vFLQ";
    //            PG.LoginID = "cnpdev3801";

    //            context.PaymentGateways.Add(PG);   

    //            try
    //            {

    //                //    context.SaveChanges();
    //            }
    //            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
    //            {
    //#if DEBUG
    //                StringBuilder sbError = new StringBuilder();
    //                foreach (var validationErrors in dbEx.EntityValidationErrors)
    //                {
    //                    foreach (var validationError in validationErrors.ValidationErrors)
    //                    {
    //                        sbError.AppendLine("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
    //                    }
    //                }
    //                throw new Exception(sbError.ToString());
    //#endif
    //            }

    //        }
    //    }

}
