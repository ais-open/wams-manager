using System.Security;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ais.Internal.Dcm.Business
{
    /// <summary>
    /// Class representing an access policy in a media service
    /// </summary>
    public class AccessPolicy
    {
        /// <summary>
        /// Access policy Id.
        /// </summary>
        public string Id
        {
            get;
            internal set;
        }

        /// <summary>
        /// Access policy friendly name.
        /// </summary>
        public string Name
        {
            get;
            internal set;
        }

        /// <summary>
        /// Double for which this access policy will be effective.
        /// </summary>
        public Double DurationInMinutes
        {
            get;
            internal set;
        }

        /// <summary>
        /// Effective permissions
        /// </summary>
        public AccessPolicyPermission Permissions
        {
            get;
            internal set;
        }

        /// <summary>
        /// Date/time access policy was created.
        /// </summary>
        public DateTime Created
        {
            get;
            internal set;
        }

        /// <summary>
        /// Date/time access policy was last modified.
        /// </summary>
        public DateTime LastModified
        {
            get;
            internal set;
        }

        private MediaServiceContext _context = null;

        public AccessPolicy()
        {

        }

        public AccessPolicy (string policyName, double durationInMinutes, AccessPolicyPermission permission,MediaServiceContext context)
        {
            try
            {
                _context = context;
                string requestBody = "{\"Name\": \"" + policyName + "\", \"DurationInMinutes\" : \"" + durationInMinutes + "\", \"Permissions\" : " + (int)permission + " }";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(CultureInfo.InvariantCulture, "{0}AccessPolicies", context.WamsEndpoint));
                request.Accept = RequestContentType.Json;
                request.Method = HttpVerbs.Post;
                request.ContentType = RequestContentType.Json;
                request.Headers.Add(RequestHeaders.XMsVersion, RequestHeaderValues.XMsVersion);
                request.Headers.Add(RequestHeaders.Authorization, string.Format(CultureInfo.InvariantCulture, RequestHeaderValues.Authorization, context.AccessToken));
                request.Headers.Add(RequestHeaders.DataServiceVersion, RequestHeaderValues.DataServiceVersion);
                request.Headers.Add(RequestHeaders.MaxDataServiceVersion, RequestHeaderValues.MaxDataServiceVersion);
                request.ContentLength = Encoding.UTF8.GetByteCount(requestBody);
                using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(requestBody);
                }
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream(), true))
                    {
                        var returnBody = streamReader.ReadToEnd();
                        JObject responseJsonObject = JObject.Parse(returnBody);
                        var d = responseJsonObject["d"];
                        var policyID = d.Value<string>("Id");
                        var Created = d.Value<DateTime>("Created");
                        var LastModified = d.Value<DateTime>("LastModified");
                        var pName = d.Value<string>("Name");
                        var DurationInMinutes = d.Value<double>("DurationInMinutes");
                        var Permissions = (AccessPolicyPermission)d.Value<int>("Permissions");

                        this.Id = policyID;
                        this.Created = Created;
                        this.LastModified = LastModified;
                        this.Name = pName;
                        this.DurationInMinutes = DurationInMinutes;
                        this.Permissions = Permissions;
                    }
                }
            }
            catch (WebException exp)
            {
                Logger.WriteLog(exp);
                throw Helper.HandleWebException(exp);
            }
            catch (ProtocolViolationException protocolViolationException) { Logger.WriteLog(protocolViolationException); }
            catch (ObjectDisposedException objectDisposedException) { Logger.WriteLog(objectDisposedException); }
            catch (NotSupportedException notSupportedException) { Logger.WriteLog(notSupportedException); }
            catch (ArgumentNullException argumentNullException) { Logger.WriteLog(argumentNullException); }
            catch (SecurityException securityException) { Logger.WriteLog(securityException); }
            catch (UriFormatException uriFormatException) { Logger.WriteLog(uriFormatException); }
            catch (OutOfMemoryException outOfMemoryException) { Logger.WriteLog(outOfMemoryException); }
            catch (IOException ioException) { Logger.WriteLog(ioException); }
            catch (InvalidOperationException invalidOperationException) { Logger.WriteLog(invalidOperationException); }

        }

        public bool Delete()
        {
            bool isSuccess = false;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(CultureInfo.InvariantCulture, "{0}AccessPolicies('{1}')", _context.WamsEndpoint,this.Id));
                request.Accept = RequestContentType.Json;
                request.Method = HttpVerbs.Delete;
                request.ContentType = RequestContentType.Json;
                request.Headers.Add(RequestHeaders.XMsVersion, RequestHeaderValues.XMsVersion);
                request.Headers.Add(RequestHeaders.Authorization, string.Format(CultureInfo.InvariantCulture, RequestHeaderValues.Authorization, _context.AccessToken));
                request.Headers.Add(RequestHeaders.DataServiceVersion, RequestHeaderValues.DataServiceVersion);
                request.Headers.Add(RequestHeaders.MaxDataServiceVersion, RequestHeaderValues.MaxDataServiceVersion);
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    isSuccess = true;
                }
            }
            catch (WebException exp)
            {
                Logger.WriteLog(exp);
                throw Helper.HandleWebException(exp);
            }
            catch (ProtocolViolationException protocolViolationException) { Logger.WriteLog(protocolViolationException); }
            catch (ObjectDisposedException objectDisposedException) { Logger.WriteLog(objectDisposedException); }
            catch (NotSupportedException notSupportedException) { Logger.WriteLog(notSupportedException); }
            catch (ArgumentNullException argumentNullException) { Logger.WriteLog(argumentNullException); }
            catch (SecurityException securityException) { Logger.WriteLog(securityException); }
            catch (UriFormatException uriFormatException) { Logger.WriteLog(uriFormatException); }
            catch (OutOfMemoryException outOfMemoryException) { Logger.WriteLog(outOfMemoryException); }
            catch (IOException ioException) { Logger.WriteLog(ioException); }
            catch (InvalidOperationException invalidOperationException) { Logger.WriteLog(invalidOperationException); }

            return isSuccess;
        }
    }

    public enum AccessPolicyPermission
    {
        None = 0,
        Read = 1,
        Write = 2,
        Delete = 4,
        List = 8,
    }
}
