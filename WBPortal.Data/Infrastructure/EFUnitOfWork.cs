using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WBSSLStore.Data.Infrastructure
{
    public class EFUnitOfWork : IUnitOfWork
    {
        private readonly IDatabaseFactory _databaseFactory;
        private WBSSLStoreDb _db;


        public EFUnitOfWork(IDatabaseFactory databaseFactory)
        {
            _databaseFactory = databaseFactory;
        }

        protected WBSSLStoreDb DataContext
        {
            get { return _db ?? (_db = _databaseFactory.Get()); }
        }

        public void Commit()
        {
            DataContext.Commit();
        }
    }
}
