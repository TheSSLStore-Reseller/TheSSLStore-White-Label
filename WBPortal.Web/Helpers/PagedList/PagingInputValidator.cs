using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WBSSLStore.Web.Helpers.PagedList
{
    public static class PagingInputValidator
    {

        public static bool IsPagingInputValid(ref int? PageParameter)
        {
            if (PageParameter.HasValue && PageParameter < 1)
                return false;
            return true;
        }

        public static bool IsPagingInputValid(ref int? PageParameter,IPagedList dataresult)
        {
            if (PageParameter.HasValue && PageParameter < 1)
                return false;

            if (dataresult.PageNumber != 1 && PageParameter.HasValue && PageParameter > dataresult.PageCount)
                return false;

            return true;
        }
    }
}