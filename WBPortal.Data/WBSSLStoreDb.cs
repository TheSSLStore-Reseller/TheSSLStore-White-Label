using System;
using System.Data.Entity;
using System.Text;
using WBSSLStore.Domain;
using System.Data;

namespace WBSSLStore.Data
{
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
            
            modelBuilder.Entity<OrderDetail>().Ignore(x => x.InvoiceNumber);

           
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


    

}
