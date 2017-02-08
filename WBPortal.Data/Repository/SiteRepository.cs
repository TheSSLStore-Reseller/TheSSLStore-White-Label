using System;
using System.Collections.Generic;
using System.Linq;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Domain;
using System.Data.SqlClient;

namespace WBSSLStore.Data.Repository
{
    public class SiteRepository : EFRepository<Site>, ISiteRepository
    {
        private IDatabaseFactory _dbfactory;
        public SiteRepository(IDatabaseFactory dbfactory)
            : base(dbfactory)
        {
            _dbfactory = dbfactory;
        }


        public Site GetSite(string alias)
        {

              var siteQuery = (from s in DbContext.Sites
                             where s.isActive == true
                             select s).EagerLoad(c => c.SupportedLanguages, c => c.Settings, c => c.Pages);

            return siteQuery.OrderByDescending(x => x.ID).FirstOrDefault();
        }
        public List<Country> GetCountryList()
        {
            return (from c in DbContext.Countries
                    where c.RecordStatusID == (int)RecordStatus.ACTIVE
                    orderby c.CountryName
                    select c).ToList();

        }

        public User GetSiteAdmin(int SiteID)
        {
            return (from u in DbContext.Users
                    where u.RecordStatusID == (int)RecordStatus.ACTIVE && u.UserTypeID == (int)UserType.ADMIN
                    && u.SiteID == SiteID
                    select u).EagerLoad(x => x.Address).FirstOrDefault();
        }

        //user SP here
        public int GetCurrentContractID(int UserID, int SiteID)
        {
            return DbContext.Database.SqlQuery<int>("Select dbo.GetCurrentContractFromUserID(" + UserID + "," + SiteID + ")").FirstOrDefault();

        }

        public List<RecentlyViewProdctList> GetRecentlyViewProdctList(String Codes, int SiteID, int ContractID)
        {
            SqlParameter[] param = new SqlParameter[3];
            param[0] = new SqlParameter("ProductCode",System.Data.SqlDbType.NVarChar,100);
            param[0].Value = Codes;
            param[1] = new SqlParameter("SiteID", SiteID);
            param[2] = new SqlParameter("ContractID", ContractID);
            return DbContext.Database.SqlQuery<RecentlyViewProdctList>("GetRecentlyViewedproduct @ProductCode,@SiteID,@ContractID",param).ToList();

        }

        //Example of Query to call complex stored procedure. Please also remember that order must be correct and return type should
        //be exactly the Entity Fields. 
        public UserExt GetUserAccountSummary(int UserID, int SiteID)
        {
            SqlParameter[] param = new SqlParameter[2];
            param[0] = new SqlParameter("UserID", UserID);
            param[1] = new SqlParameter("SiteID", SiteID);
            return DbContext.Database.SqlQuery<UserExt>("GetUserAccountDeatils @UserID,@SiteID", param).FirstOrDefault();
        }

        public SiteSMTP GetSMTPDetails(int SiteID)
        {
            return (from u in DbContext.SiteSmtps
                    where u.SiteID == SiteID
                    select u).FirstOrDefault();
        }
    }

    public interface ISiteRepository : IRepository<Site>
    {
        Site GetSite(string alias);
        List<Country> GetCountryList();
        int GetCurrentContractID(int UserID, int SiteID);
        UserExt GetUserAccountSummary(int UserID, int SiteID);
        User GetSiteAdmin(int SiteID);
        SiteSMTP GetSMTPDetails(int SiteID);
        List<RecentlyViewProdctList> GetRecentlyViewProdctList(String Codes, int SiteID, int ContractID);
    }
}
