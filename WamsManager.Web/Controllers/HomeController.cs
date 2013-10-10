using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using WamsManager.Web.Models;

namespace WamsManager.Web.Controllers
{
    public class HomeController : Controller
    {
        public AzureTableService azureTableService = new AzureTableService();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(FormCollection formCollection)
        {
            string evaluationId = formCollection["invitationCode"];
            ViewBag.EvaluationId = evaluationId;
            TempData["invitationId"] = evaluationId;

            if (azureTableService.IsEvaluationIdValid(evaluationId))
            {
                return View("CustomerInfo");
            }
            else
            {
               if (!azureTableService.IsAlreadySiteProvisionedForInvitationId(evaluationId))
               {
                   string provisionedSiteUrl = azureTableService.GetProvisionedSiteUrl(evaluationId);
                   ViewBag.Feedback = "Evaluation Code already used. Your provisioned site url is " + provisionedSiteUrl;
                   return View("TenantProvisioner");
               }
               else
               {
                   ViewBag.Feedback = "There an error in site provisioning. Please contact admin support.";
                   return View("TenantProvisioner");
               }
            }

            //delete after testing is complete
            #region oldCode

            //if (!azureTableService.IsAlreadyUsedEvaluationId(evaluationId))
            //{
            //    azureTableService.InsertEvaluationCodeTable(evaluationId);
            //    return View("CustomerInfo");
            //}
            //else
            //{
            //    if (!azureTableService.IsAlreadySiteProvisionedForInvitationId(evaluationId))
            //    {
            //        string provisionedSiteUrl = azureTableService.GetProvisionedSiteUrl(evaluationId);
            //        ViewBag.Feedback = "Evaluation Code already used. Your provisioned site url is " + provisionedSiteUrl;
            //        return View("TenantProvisioner");
            //    }
            //    else
            //    {
            //        ViewBag.Feedback = "There an error in site provisioning. Please contact admin support.";
            //        return View("TenantProvisioner");
            //    }
            //}

            #endregion
        }

        [HttpPost]
        public ActionResult CustomerInfo(FormCollection custInfoFormCollection)
        {
            var customerInfo = new CustomerInfo()
                                   {
                                       InvitationId = (string) TempData["invitationId"],
                                       CustomerFirstName = custInfoFormCollection["firstName"],
                                       CustomerLastName = custInfoFormCollection["lastName"],
                                       SitePrefix = custInfoFormCollection["sitePrefix"],
                                       CustomerOrganization = custInfoFormCollection["custOrganization"],
                                       CustomerEmail = custInfoFormCollection["custEmail"],
                                       Username = custInfoFormCollection["username"],
                                       Password = custInfoFormCollection["password"],
                                       AzureStorageAccount = custInfoFormCollection["azureStorageAcctName"],
                                       AzureStoragePrimaryKey = custInfoFormCollection["azureStoragePrimaryKey"],
                                   };

            azureTableService.InsertCustomerInfoTable(customerInfo);

            var tenantName = custInfoFormCollection["sitePrefix"];
            var provisioner = new TenantProvisioner();
            if (tenantName != null)
            {
               ViewBag.Feedback = provisioner.ProvisionSite(customerInfo);
            }

            return View("TenantProvisioner");
        }

        public ActionResult About()
        {

            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult GetUrl(string evaluationId) 
        {
            string siteUrl = azureTableService.GetProvisionedSiteUrl(evaluationId);
            return Json((siteUrl ?? ""), JsonRequestBehavior.AllowGet);
        }
    }
}
