
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WBSSLStore.Web.Helpers
{
    public static class StringExtensions
    {
        public static int ToInt(this string data, int defaultValue)
        {
            if (!String.IsNullOrWhiteSpace(data))
                return Convert.ToInt32(data.Trim());
            else
                return defaultValue;
        }
    }
}