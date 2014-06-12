using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ais.Internal.Dcm.Web.Controllers;

namespace Ais.Internal.Dcm.WebApi.Tests.Controllers
{
    [TestClass]
    public class Webv2HomeControllerTest
    {
        [TestMethod]
        public void IndexTest()
        {
            var controller = new HomeController();
            var result = (RedirectResult)controller.Index();
            Assert.AreEqual("/UserPages/MediaService.html", result.Url);
        }
    }
}
