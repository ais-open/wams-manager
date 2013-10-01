using Ais.Internal.Dcm.Business;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ais.Internal.Dcm.UI.Common;

namespace Ais.Internal.Dcm.UI
{
    public class MediaContextHelper
    {
        private static string accountName = string.Empty;
        private static string accountKey = string.Empty;

        private static MediaServiceContext instance;

        public static MediaServiceContext Instance
        {
            get
            {
                if (instance == null)
                {
                    if (CheckForSettings())
                        instance = new MediaServiceContext(accountName, accountKey);
                }
                return instance;
            }
        }


        private static bool CheckForSettings()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(ConfigurationSettings.AppSettings["AccountName"]) &&
                    !string.IsNullOrWhiteSpace(ConfigurationSettings.AppSettings["AccountKey"]))
                {
                    accountName = ConfigurationSettings.AppSettings["AccountName"];
                    accountKey = ConfigurationSettings.AppSettings["AccountKey"];
                    return true;
                }
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
            return false;
        }
    }
}
