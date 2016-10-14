using System;
using System.Configuration;

namespace WBSSLStore.Data.Infrastructure
{
    public interface IDatabaseFactory : IDisposable
    {
        WBSSLStoreDb Get();
    }

    public class DatabaseFactory : Disposable, IDatabaseFactory
    {
        private string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["WhiteLabelConnection"].ToString();
        private WBSSLStoreDb _db;
        public WBSSLStoreDb Get()
        {
            return _db ?? (_db = new WBSSLStoreDb());
        }

        protected override void Disposer()
        {
            if (_db != null)
                _db.Dispose();
        }
    }
}
