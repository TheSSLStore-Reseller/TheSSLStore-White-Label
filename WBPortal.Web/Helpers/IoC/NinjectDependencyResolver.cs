using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ninject;
using Ninject.Parameters;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Data.Repository;
using WBSSLStore.Service;
using WBSSLStore.Logger;
using WBSSLStore.Domain;
using WBSSLStore.Service.ViewModels;

namespace WBSSLStore.Web.Helpers.IoC
{
    /// <summary>
    /// Global Service Locator
    /// </summary>
    public class NinjectDependencyResolver : IDependencyResolver
    {
        private readonly IKernel _kernel;
        public NinjectDependencyResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        public object GetService(Type serviceType)
        {
            try
            {
                return _kernel.TryGet(serviceType, new IParameter[0]);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return _kernel.GetAll(serviceType, new IParameter[0]);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public static class DependencyRegister
    {
        public static void RegisterDepenencyResolver()
        {
            var kernel = new StandardKernel();

            //Set up Binding Information
            kernel.Bind<ILogger>().To<Logger.Logger>().InRequestScope();
            kernel.Bind<IUnitOfWork>().To<EFUnitOfWork>().InRequestScope();
            kernel.Bind<IDatabaseFactory>().To<DatabaseFactory>().InRequestScope();

            //Custom Repositories
            kernel.Bind<ISiteRepository>().To<SiteRepository>().InRequestScope();
            kernel.Bind<IWBRepository>().To<WBRepository>().InRequestScope();
       

            //Set ViewModel
            kernel.Bind<Site>().To<Site>().InRequestScope();
          
            //kernel.Bind<ProductPricing>().To<ProductPricing>().InRequestScope();
            //kernel.Bind<ProductAvailablity>().To<ProductAvailablity>().InRequestScope();
            //kernel.Bind<CMSPage>().To<CMSPage>().InRequestScope();
            //kernel.Bind<CMSPageContent>().To<CMSPageContent>().InRequestScope();
            //kernel.Bind<ShoppingCart>().To<ShoppingCart>().InRequestScope();
            //kernel.Bind<ShoppingCartDetail>().To<ShoppingCartDetail>().InRequestScope();
            //kernel.Bind<ProductPricingModel>().To<ProductPricingModel>().InRequestScope();
            //kernel.Bind<StaticPageViewModel>().To<StaticPageViewModel>().InRequestScope();
            //kernel.Bind<ShoppingCartViewModel>().To<ShoppingCartViewModel>().InRequestScope();            

            //Custom Services
            kernel.Bind<ISiteService>().To<SiteService>().InRequestScope();
            kernel.Bind<IShoppingCartViewModelService>().To<ShoppingCartViewModelService>().InRequestScope();
            kernel.Bind<IStaticPageViewModelService>().To<StaticPageViewModelService>().InRequestScope();
            kernel.Bind<IUserService>().To<UserService>().InRequestScope();
            kernel.Bind<IProductService>().To<ProductService>().InRequestScope();
            kernel.Bind<ICheckoutService>().To<CheckoutService>().InRequestScope();
            kernel.Bind<IEmailQueueService>().To<EmailQueueService>().InRequestScope();
            kernel.Bind(typeof(IRepository<>)).To(typeof(EFRepository<>)).InRequestScope();
            //Set Resolver
            DependencyResolver.SetResolver(new NinjectDependencyResolver(kernel));
        }
    }
}