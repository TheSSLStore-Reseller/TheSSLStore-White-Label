using System;
using System.Linq;
using System.Linq.Expressions;
using WBSSLStore.Domain;

namespace WBSSLStore.Data.Infrastructure
{
    public interface IRepository<T> where T : class, IEntity
    {
        T Add(T entity);
        T Update(T entity);
        T Delete(T entity);
        void Delete(Expression<Func<T, bool>> where);
        T FindByID(int ID);
        IQueryable<T> Find(Expression<Func<T, bool>> where, params Expression<Func<T, object>>[] include);
        IQueryable<T> Include(IQueryable<T> res,System.Linq.Expressions.Expression<Func<T, object>> include);
        IQueryable<T> FindAll();
    }
}
