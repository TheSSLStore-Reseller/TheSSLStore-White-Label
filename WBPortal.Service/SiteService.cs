using System;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Data.Repository;
using WBSSLStore.Domain;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;

namespace WBSSLStore.Service
{

    public interface ISiteService  
    {
        Site GetSite(string alias);
        Site GetSite(int id);
        int GetCurrentContractID(int UserID, int SiteID);
        UserExt GetUserAccountSummary(int UserID, int SiteID);
        List<RecentlyViewProdctList> GetRecentlyViewProdctList(string codes, int SiteID, int ContractID);
    }

    public class SiteService : ISiteService
    {
       
        public readonly ISiteRepository  _siteRepository;
        private readonly IUnitOfWork _unitOfWork;
        

        public SiteService(ISiteRepository repository, IUnitOfWork unitOfWork)
        {
            _siteRepository = repository;
            _unitOfWork = unitOfWork;
        }

        public Site GetSite(string alias)
        {
          return _siteRepository.GetSite(alias);
        }
        public Site GetSite(int id)
        {
            return _siteRepository.Find(s => s.ID == id && s.isActive == true).EagerLoad(s => s.Settings, s => s.SupportedLanguages,s=>s.Pages).FirstOrDefault();  
        }
        public int GetCurrentContractID(int UserID, int SiteID)
        {
            return _siteRepository.GetCurrentContractID(UserID, SiteID);  
        }
        public UserExt GetUserAccountSummary(int UserID, int SiteID)
        {

            return _siteRepository.GetUserAccountSummary(UserID, SiteID); 
        }
        public List<RecentlyViewProdctList> GetRecentlyViewProdctList(string codes, int SiteID, int ContractID)
        {

            return _siteRepository.GetRecentlyViewProdctList(codes, SiteID,ContractID);
        }
    }
}
