using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WBSSLStore.Domain;
using WBSSLStore.Data.Infrastructure;
using System.Data.Entity;

namespace WBSSLStore.Data.Repository
{
    public interface ICheckOutRepository : IRepository<User>
    {
        IQueryable<ShoppingCartDetail> GetCart(int ShoppingCartID, int SiteID);
        int UpdateShoppingCart(User user, int CartID, int SiteID, int ContractID, string AnonymousID);
        int AddUSerandUpdateCart(User user, int CartID);
        IQueryable<PaymentGateways> GetPGInstances(int SiteID);
        void AddOrder(UserTransaction UT);
        void SaveGatewayIntraction(GatewayInteraction GT);
        PromoCode GetPromoRow(string code, int SiteID, UserType oUserType);
        PromoCode GetPromoRow(string code, int SiteID);
        SiteSMTP GetSiteSMTP(int SiteID);
        void DeleteCartDetails(List<ShoppingCartDetail> cartdetails);
        void AddEmailQueue(EmailQueue eq);
        EmailTemplates GetEmailTemplate(EmailType emailtype, int siteid,int LangID);
        decimal GetCreditAmount(int UserID, int SiteID);

    }

    public class CheckOutRepository : EFRepository<User>, ICheckOutRepository
    {
        private IDatabaseFactory _dbfactory;
        public CheckOutRepository(IDatabaseFactory dbfactory)
            : base(dbfactory)
        {
            _dbfactory = dbfactory;
        }

        public decimal GetCreditAmount(int UserID, int SiteID)
        {
            decimal? amount = DbContext.UserTransactions.Where(x => x.UserID == UserID && x.SiteID == SiteID).Sum(x => (decimal?)x.TransactionAmount);
            return amount ?? 0;

        }
        public void AddEmailQueue(EmailQueue eq)
        {
            DbContext.EmailQueues.Add(eq);
            DbContext.Commit();  
        }
        public EmailTemplates GetEmailTemplate(EmailType emailtype, int siteid, int LangID)
        {
            return DbContext.EmailTemplateses.Where(x => x.EmailTypeId == (int)emailtype && x.SiteID == siteid && x.LangID == LangID && x.isActive == true).FirstOrDefault();
        }
        public IQueryable<ShoppingCartDetail> GetCart(int ShoppingCartID, int SiteID)
        {
            return (from s in DbContext.ShoppingCartDetails
                    where s.ShoppingCartID == ShoppingCartID
                    select s).EagerLoad(c => c.ShoppingCart, c => c.Product, c => c.ProductPricing);

        }


        public int UpdateShoppingCart(User user, int CartID, int SiteID, int ContractID, string AnonymousID)
        {
            List<ShoppingCartDetail> currentcart = null;
            int returnCartID = CartID;

            ShoppingCart currentSC = null;
            if (CartID > 0)
            {
                currentSC = DbContext.ShoppingCarts.Where(sc => sc.ID == CartID && sc.SiteID == SiteID).FirstOrDefault();

            }
            else if (!string.IsNullOrEmpty(AnonymousID))
            {
                currentSC = DbContext.ShoppingCarts.Where(sc => sc.UserAnonymousToken == AnonymousID && sc.SiteID == SiteID).FirstOrDefault();
            }

            if (currentSC != null && currentSC.ID > 0)
            {

                currentcart = (from s in DbContext.ShoppingCartDetails
                               where s.ShoppingCartID == currentSC.ID
                               select s).EagerLoad(x => x.ProductPricing).ToList();

                returnCartID = currentSC.ID;

            }

            if (currentcart != null && currentcart.Count > 0)
            {
                List<ShoppingCartDetail> usercart = null;

                ShoppingCart userSC = DbContext.ShoppingCarts.Where(sc => sc.UserID == user.ID && sc.SiteID == SiteID).FirstOrDefault();


                if (userSC != null && userSC.ID > 0)
                {
                    usercart = (from s in DbContext.ShoppingCartDetails
                                where s.ShoppingCartID == userSC.ID
                                select s).EagerLoad(x => x.ProductPricing).ToList();


                }

                if (userSC != null && userSC.ID > 0 && currentSC.ID != userSC.ID)
                {
                    foreach (ShoppingCartDetail s1 in currentcart)
                    {
                        s1.ShoppingCartID = userSC.ID;
                        usercart.Add(s1);
                    }

                    returnCartID = userSC.ID;
                    DbContext.ShoppingCarts.Remove(currentSC);
                }
                else
                {


                    currentSC.UserAnonymousToken = string.Empty;
                    currentSC.UserID = user.ID;

                    usercart = currentcart;
                    returnCartID = currentSC.ID;
                }


                if (usercart != null && usercart.Count > 0)
                {

                    ProductPricing pp = null;
                    PromoCode promo = null;
                    foreach (ShoppingCartDetail s1 in usercart)
                    {

                        if (user.UserType == UserType.RESELLER)
                        {
                            pp = (from p in DbContext.ProductPricings
                                  where p.ContractID == ContractID && p.ProductID == s1.ProductID && p.NumberOfMonths == s1.ProductPricing.NumberOfMonths && p.SiteID == SiteID
                                  select p).FirstOrDefault();

                            if (pp != null)
                            {
                                s1.ProductPricing = pp;
                                s1.ProductPriceID = pp.ID;
                                s1.Price = CalCulatePrice(pp, s1.NumberOfServers, s1.AdditionalDomains);
                                if (!string.IsNullOrEmpty(s1.PromoCode))
                                {
                                    promo = this.GetPromoRow(s1.PromoCode, SiteID, UserType.RESELLER);
                                    if (promo != null)
                                    {
                                        this.AssignPromoInCart(promo, s1, pp);
                                    }
                                    else
                                    {
                                        s1.PromoCode = "";
                                        s1.PromoDiscount = 0;
                                    }
                                }
                            }


                        }

                        DbContext.ShoppingCartDetails.Attach(s1);
                        DbContext.Entry(s1).State = EntityState.Modified;


                    }




                    DbContext.Commit();



                }
            }
            else
            {
                var Cart = DbContext.ShoppingCarts.Where(x => x.UserID == user.ID && x.SiteID == user.SiteID);
                if (Cart != null && Cart.Count() > 0)
                    returnCartID = Cart.FirstOrDefault().ID;  
            }
            return returnCartID;
        }
        private decimal CalCulatePrice(ProductPricing pp, int NoOfServer, int NoOfDomains)
        {
            decimal price = 0;
            decimal AdditionDomainPrice = 0;
            if (pp != null)
            {
                if (pp.Product.isSANEnabled)
                {
                    int SanMin = Convert.ToInt32(pp.Product.SanMin);
                    if (NoOfDomains > SanMin)
                    {
                        AdditionDomainPrice = pp.AdditionalSanPrice * (NoOfDomains - SanMin);
                    }


                }

                if (!pp.Product.isNoOfServerFree)
                {
                    price = pp.SalesPrice * NoOfServer;
                }
                else
                    price = pp.SalesPrice;


                price += AdditionDomainPrice;
            }

            return price;
        }
        private void AssignPromoInCart(PromoCode prow, ShoppingCartDetail cartDetails, ProductPricing pp)
        {
            decimal promodiscount = 0;

            if (prow == null)
            {
                cartDetails.PromoCode = string.Empty;
                cartDetails.PromoDiscount = 0;
                return;
            }

            if (prow != null && cartDetails != null && cartDetails.ShoppingCartID > 0)
            {

                if (cartDetails.ProductID == prow.ProductID && cartDetails.ProductPricing.NumberOfMonths == prow.NoOfMonths)
                {

                    if (prow.Discount > 0)
                    {
                        if (prow.DiscountModeID == (int)DiscountMode.FLAT)
                        {
                            promodiscount = prow.Discount;
                            if (!cartDetails.Product.isNoOfServerFree)
                                promodiscount = (prow.Discount * (cartDetails.NumberOfServers > 0 ? cartDetails.NumberOfServers : 1));
                        }
                        else if (prow.DiscountModeID == (int)DiscountMode.PERCENTAGE)
                        {
                            promodiscount = (pp.SalesPrice * prow.Discount) / 100;
                            if (!cartDetails.Product.isNoOfServerFree)
                                promodiscount = (promodiscount * (cartDetails.NumberOfServers > 0 ? cartDetails.NumberOfServers : 1));

                        }
                    }

                    cartDetails.PromoCode = prow.Code;
                    cartDetails.PromoDiscount = promodiscount;

                }
                else
                {
                    cartDetails.PromoCode = string.Empty;
                    cartDetails.PromoDiscount = 0;
                }

            }
        }
        public void DeleteCartDetails(List<ShoppingCartDetail> cartdetails)
        {
            foreach (ShoppingCartDetail s in cartdetails)
            {
                DbContext.ShoppingCartDetails.Remove(s);
            }

            DbContext.Commit();
        }
       
        public SiteSMTP GetSiteSMTP(int SiteID)
        {
            return (from smtp in DbContext.SiteSmtps
                    where smtp.SiteID == SiteID
                    select smtp).FirstOrDefault();
        }

        public PromoCode GetPromoRow(string code, int SiteID)
        {

            return (from p in DbContext.PromoCodes
                    where p.Code == code && p.SiteID == SiteID && p.EndDate >= System.DateTimeWithZone.Now && p.StartDate <= System.DateTimeWithZone.Now
                    select p).FirstOrDefault();


        }
        public PromoCode GetPromoRow(string code, int SiteID, UserType oUserType)
        {
            bool promoIsValidForUser = false;
            PromoCode promo = (from p in DbContext.PromoCodes
                               where p.Code == code && p.SiteID == SiteID && p.EndDate >= System.DateTimeWithZone.Now && p.StartDate <= System.DateTimeWithZone.Now
                               select p).FirstOrDefault();

            if (promo != null && promo.ID > 0)
            {
                if (oUserType == UserType.RESELLER && promo.isForReseller)
                {
                    promoIsValidForUser = true;
                }
                else if (oUserType == UserType.CUSTOMER && promo.isForClient)
                {
                    promoIsValidForUser = true;
                }

                if (promoIsValidForUser)
                {
                    int MaxOrder = DbContext.OrderDetails.Where(o => o.PromoCodeID == promo.ID).Count();

                    if (MaxOrder >= promo.MaxOrders)
                        return null;
                    else
                        return promo;
                }
                else
                    return null;

            }
            return null;
        }

        public int AddUSerandUpdateCart(User user, int CartID)
        {
            //return 2 for duplicate
            //return 1 fro Success
            //retirn -1 for error
            //Check For Duplicate USer
            User DuplicateUser = DbContext.Users.Where(x => x.SiteID == user.SiteID && x.Email == user.Email && x.RecordStatusID == 1 && x.ID != user.ID).FirstOrDefault();
            if (DuplicateUser != null && DuplicateUser.ID > 0)
            {
                DuplicateUser = null;
                return 2;
            }

            //
            try
            {

                DbContext.Users.Add(user);

                DbContext.Commit();

                ShoppingCart cart = (from s in DbContext.ShoppingCarts
                                     where s.ID == CartID
                                     select s).FirstOrDefault();

                if (cart != null)
                {
                    cart.UserID = user.ID;
                    cart.UserAnonymousToken = string.Empty;
                    DbContext.ShoppingCarts.Attach(cart);
                    DbContext.Entry(cart).State = System.Data.Entity.EntityState.Modified;

                }

                DbContext.Commit();
                return 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IQueryable<PaymentGateways> GetPGInstances(int SiteID)
        {
            return from pg in DbContext.PaymentGateways
                   where pg.SiteID == SiteID
                   select pg;
        }

        public void AddOrder(UserTransaction UT)
        {
            DbContext.UserTransactions.Add(UT);

        }

        public void SaveGatewayIntraction(GatewayInteraction GT)
        {
            DbContext.GatewayInteractions.Add(GT);
            DbContext.Commit();
        }
    }
}
