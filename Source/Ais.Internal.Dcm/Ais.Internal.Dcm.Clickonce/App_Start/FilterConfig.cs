using System.Web.Mvc;

namespace Ais.Internal.Dcm.Clickonce
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}