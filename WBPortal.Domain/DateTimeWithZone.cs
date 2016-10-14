using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{

    public static class DateTimeWithZone
    {
        public static int CurrentTimeZone
        {
            get;
            set;
        }
        public static DateTime Now
        {
            get { return System.DateTime.UtcNow.AddMinutes(CurrentTimeZone); }
        }

        public static DateTime ToDateTime(string dt)
        {
            return DateTime.ParseExact(dt, "yyyy-mm-dd", System.Globalization.CultureInfo.CurrentCulture);
        }
    }

}
