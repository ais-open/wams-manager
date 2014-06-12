using Ais.Internal.Dcm.Web.Service;
using System;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;

namespace Ais.Internal.Dcm.Web.Filters
{
    public class BasicAuthenticationAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {   //
            ILoggerService loggerService = new LoggerService();
            if (actionContext.Request.Headers.Authorization == null)
            {
                actionContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                loggerService.LogException("authorization null", new Exception());
            }
            else
            {
                string authToken = actionContext.Request.Headers.Authorization.Parameter;
                string decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(authToken));

                string username = decodedToken.Substring(0, decodedToken.IndexOf(":"));
                string password = decodedToken.Substring(decodedToken.IndexOf(":") + 1);
                //already authenticated, avoid look up to Membership table
                if (!actionContext.Request.RequestUri.LocalPath.Contains("MediaServices") && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    base.OnActionExecuting(actionContext);
                }
                else
                {
                    if (Membership.ValidateUser(username, password))
                    {
                        var identity = new GenericIdentity(username, "Basic");
                        IPrincipal principal = new GenericPrincipal(identity, new string[] { "REST_CALLER" });
                        Thread.CurrentPrincipal = principal;
                        HttpContext.Current.User = principal;
                        base.OnActionExecuting(actionContext);
                    }
                    else
                    {
                        loggerService.LogException("authorization failed:" + username + ":" + password, new Exception());
                        actionContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                    }
                }
            }
        }
    }
}