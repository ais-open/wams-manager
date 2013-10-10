using System.Security.Principal;
using System.Web.Security;
using Ais.Internal.Dcm.Web.App_Start;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using Ais.Internal.Dcm.Web.Filters;
using Ais.Internal.Dcm.Web.Models;

namespace Ais.Internal.Dcm.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
           
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Initialize the application roles 
            if (!Roles.RoleExists(Constants.ADMIN_ROLE))
            {
                Roles.CreateRole(Constants.ADMIN_ROLE);
            }

            if (!Roles.RoleExists(Constants.USER_ROLE))
            {
                Roles.CreateRole(Constants.USER_ROLE);
            }

            var rdr = new System.Configuration.AppSettingsReader();
            var adminUsername = (string)rdr.GetValue("DefaultAdminUsername", typeof(string));
            var defaultPassword = (string)rdr.GetValue("DefaultAdminPassword", typeof(string));
            if (Membership.GetUser(adminUsername) == null)
            {
                Membership.CreateUser(adminUsername, defaultPassword);
                Roles.AddUserToRole(adminUsername, Constants.ADMIN_ROLE);

            }
        }
    }
}