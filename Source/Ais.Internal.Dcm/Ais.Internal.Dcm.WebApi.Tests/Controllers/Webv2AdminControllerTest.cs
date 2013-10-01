using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ais.Internal.Dcm.Web.Controllers;
using Ais.Internal.Dcm.Web.Service;
using Ais.Internal.Dcm.Web.Models;

namespace Ais.Internal.Dcm.WebApi.Tests.Controllers
{
    [TestClass]
    public class Webv2AdminControllerTest
    {
        [TestMethod]
        public void CreateListDeleteEncodingType()
        {
            MockLoggerService loggerService = new MockLoggerService();
            IStorageAccountInformation account = new TestStorageAccountService();
            MetaDataService metaDataService = new MetaDataService(account);
            AdminMediaService mediaService = new AdminMediaService(metaDataService,loggerService);
            AdminController controller = new AdminController(loggerService, mediaService);
            var existingEncoding = controller.GetEncodingTypes("");
            var tempEncoding = new Web.Models.EncodingTypeModel
            {
                FriendlyName = "Test FriendlyName1",
                TechnicalName = "Test TechnicalName1"
            };
            controller.CreateEncodingType(tempEncoding);
            var newencodingType = controller.GetEncodingTypes("");
            Assert.AreEqual<int>(existingEncoding.Count + 1, newencodingType.Count, "GetEncoding and CreateEncoding failed.");
            controller.DeleteEncodingType(tempEncoding);
            newencodingType = controller.GetEncodingTypes("");
            Assert.AreEqual<int>(existingEncoding.Count, newencodingType.Count, "GetEncoding and CreateEncoding failed.");
        }

        [TestMethod]
        public void CreateListDeleteMediaService()
        {
                MockLoggerService loggerService = new MockLoggerService();
                IStorageAccountInformation account = new TestStorageAccountService();
                MetaDataService metaDataService = new MetaDataService(account);
                AdminMediaService mediaService = new AdminMediaService(metaDataService, loggerService);
                AdminController controller = new AdminController(loggerService, mediaService);
                var existingMediaService = controller.GetAllMediaServices("");
                var tempService = new MediaServiceModel
                {
                    PrimaryAccountKey = "some random string",
                    MediaServiceFriendlyName = "test friendly name",
                    AccountName = "test account name"
                };
                controller.CreateMediaService(tempService);
                var newList = controller.GetAllMediaServices("");
                Assert.AreEqual<int>(existingMediaService.Count + 1, newList.Count, "GetMediaService and CreateEncoding failed.");
                controller.DeleteMediaService(tempService);
                newList = controller.GetAllMediaServices("");
                Assert.AreEqual<int>(existingMediaService.Count, newList.Count, "GetMediaService and CreateEncoding failed.");
            
        }
    }

    public class MockLoggerService : ILoggerService
    {
        public void LogException(string message, Exception exp)
        {
            Console.Write(message + " :" + exp.ToString());
        }
    }

    public class TestStorageAccountService : IStorageAccountInformation
    {

        public string AccountName
        {
            get 
            {
                return "mediaservicetagstorage";
            }
        }

        public string Key
        {
            get { return "jCgSJAzYB2UEqz8f7A6RkmYzGVPoJbXyh+shV7Akwe3v5WVfBnxDj7yu1LBDKem96kyddLYy21ambyxiKf0krA=="; }
        }
    }
}
