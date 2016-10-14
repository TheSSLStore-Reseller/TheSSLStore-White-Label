using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using WBSSLStore.Domain;

namespace WBSSLStore.Data.Infrastructure
{
    public class EFRepository<T> : IRepository<T> where T : class, IEntity
    {
        private readonly IDbSet<T> _dbSet;
        protected IDatabaseFactory DatabaseFactory { get; private set; }
        private WBSSLStoreDb _db;

        protected WBSSLStoreDb DbContext
        {
            get { return _db ?? (_db = DatabaseFactory.Get()); }
        }

        public EFRepository(IDatabaseFactory dbfactory)
        {
            DatabaseFactory = dbfactory;
            _dbSet = DbContext.Set<T>();
        }


        public T Add(T entity)
        {
            return _dbSet.Add(entity);
        }

        public T Update(T entity)
        {

            var result = _dbSet.Attach(entity);

            DbContext.Entry<T>(entity).State = System.Data.Entity.EntityState.Modified;
            return result;



        }

        public T Delete(T entity)
        {
            return _dbSet.Remove(entity);
        }

        public void Delete(System.Linq.Expressions.Expression<Func<T, bool>> where)
        {
            IEnumerable<T> objects = _dbSet.Where<T>(where).AsEnumerable();
            foreach (var ob in objects)
                _dbSet.Remove(ob);
        }

        public T FindByID(int ID)
        {
            return _dbSet.Single(o => o.ID == ID);
        }

        public IQueryable<T> Find(System.Linq.Expressions.Expression<Func<T, bool>> where, params System.Linq.Expressions.Expression<Func<T, object>>[] include)
        {
            var result = _dbSet.Where(where);
            if (include != null)
                result = include.Aggregate(result, Include);
            return result;
        }


        public IQueryable<T> Include(IQueryable<T> res, System.Linq.Expressions.Expression<Func<T, object>> include)
        {
            return res.Include(include);
        }


        public IQueryable<T> FindAll()
        {
            return _dbSet;
        }
    }
}
