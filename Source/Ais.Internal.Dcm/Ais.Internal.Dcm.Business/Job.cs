using System.Security;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Ais.Internal.Dcm.Business
{
    public class Job
    {
        public string Id { get; internal set; }
        public string Name { get; internal set; }
        public DateTime Created { get; internal set; }
        public DateTime LastModified { get; internal set; }
        public DateTime EndTime { get; internal set; }
        public DateTime StartTime { get; internal set; }
        public int Priority { get; internal set; }
        public JobState State { get; internal set; }
        public string TemplateId { get; internal set; }
        public double RunningDuration { get; internal set; }
        public List<Asset> InputMediaAssets { get; internal set; }
        public List<Asset> OutputMediaAssets { get; internal set; }
        public List<MediaTask> Tasks { get; internal set; }
        public List<JobNotificationSubscription> JobNotificationSubscriptions { get; internal set; }

        MediaServiceContext _context = null;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public Job(MediaServiceContext context)
        {
            this._context = context;
        }

        public JobState GetJobStatus(string jobID)
        {
            JobState state = JobState.Queued;
            try
            {
                HttpWebRequest request =
                    (HttpWebRequest)
                    WebRequest.Create(string.Format(CultureInfo.InvariantCulture, "{0}Jobs('{1}')",
                                                    _context.WamsEndpoint, jobID));
                request.Accept = RequestContentType.Json;
                request.Method = HttpVerbs.Get;
                request.ContentType = RequestContentType.Json;
                request.Headers.Add(RequestHeaders.XMsVersion, RequestHeaderValues.XMsVersion);
                request.Headers.Add(RequestHeaders.Authorization,
                                    string.Format(CultureInfo.InvariantCulture, RequestHeaderValues.Authorization,
                                                  _context.AccessToken));
                request.Headers.Add(RequestHeaders.DataServiceVersion, RequestHeaderValues.DataServiceVersion);
                request.Headers.Add(RequestHeaders.MaxDataServiceVersion, RequestHeaderValues.MaxDataServiceVersion);
                using (var response = (HttpWebResponse) request.GetResponse())
                {
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream(), true))
                    {
                        var returnBody = streamReader.ReadToEnd();
                        JObject responseJsonObject = JObject.Parse(returnBody);
                        var d = responseJsonObject["d"];
                        if (d != null)
                        {
                            state = (JobState) d.Value<int>("State");
                        }
                    }
                }
            }
            catch (WebException webException)
            {
                Logger.WriteLog(webException);
            }
            catch (ProtocolViolationException protocolViolationException)
            {
                Logger.WriteLog(protocolViolationException);
            }
            catch (ObjectDisposedException objectDisposedException)
            {
                Logger.WriteLog(objectDisposedException);
            }
            catch (NotSupportedException notSupportedException)
            {
                Logger.WriteLog(notSupportedException);
            }
            catch (ArgumentNullException argumentNullException)
            {
                Logger.WriteLog(argumentNullException);
            }
            catch (SecurityException securityException)
            {
                Logger.WriteLog(securityException);
            }
            catch (UriFormatException uriFormatException)
            {
                Logger.WriteLog(uriFormatException);
            }
            catch (OutOfMemoryException outOfMemoryException)
            {
                Logger.WriteLog(outOfMemoryException);
            }
            catch (IOException ioException)
            {
                Logger.WriteLog(ioException);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                Logger.WriteLog(invalidOperationException);
            }
            catch (Exception exp)
            {
                Logger.WriteLog(exp);
                throw Helper.HandleWebException(exp);
            }
            return state;
        }
    }
}
