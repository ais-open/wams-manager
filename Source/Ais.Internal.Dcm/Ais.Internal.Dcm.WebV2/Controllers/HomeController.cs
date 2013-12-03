using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ais.Internal.Dcm.Web.Filters;

namespace Ais.Internal.Dcm.Web.Controllers
{
    [BasicAuthentication]
    public class HomeController : Controller
    {
        [BasicAuthentication]
        public ActionResult Index()
        {
            return Redirect("/UserPages/UserLogin.html");
        }
    }
}
