using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using WBSSLStore.Domain;

namespace WBSSLStore.Web.Helpers.Localization
{
    public static class CultureSwitch
    { 
        public static void SwitchCulture(Site site,string langCode,string culturekey)
        {
            //if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToLower().Equals(langCode, StringComparison.OrdinalIgnoreCase) && Thread.CurrentThread.CurrentCulture.Name.ToLower().Equals(culturekey, StringComparison.OrdinalIgnoreCase))
            //    return;
            if (site != null && site.SupportedLanguages != null)
            {
                var lang =
                    site.SupportedLanguages.Where(o => o.LangCode == langCode && o.RecordStatus == RecordStatus.ACTIVE).
                        SingleOrDefault();
                if (lang != null)
                {
                    
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(langCode);
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culturekey);

                                     
                     
                }
            }
        }
    }
}