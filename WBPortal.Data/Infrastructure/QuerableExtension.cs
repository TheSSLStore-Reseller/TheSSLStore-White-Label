using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace WBSSLStore.Data.Infrastructure
{
    public static class QuerableExtensions
    {
        public static IQueryable<T> EagerLoad<T>(this IQueryable<T> query, params Expression<Func<T, object>>[] includes) where T : class
        {
            if (includes != null)
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            return query;
        }

        public static string GetEnumDescription<T>(this T enumerationValue) where T : struct
        {
            var type = enumerationValue.GetType();
            if (!type.IsEnum) throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
            var str = enumerationValue.ToString();
            var memberInfo = type.GetMember(str);
            if (memberInfo != null && memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    return ((System.ComponentModel.DescriptionAttribute)attrs[0]).Description;
                }
            }
            return str;
        }
    }
}
