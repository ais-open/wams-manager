using System.Security;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
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
    /// Class representing an asset in a media service
    /// </summary>
    public class Asset
    {
        private readonly MediaServiceContext _context;

        /// <summary>
        /// Creates an instance of <see cref="Asset"/>
        /// </summary>
        /// <param name="context">
        /// <see cref="MediaServiceContext"/>
        /// </param>
        internal Asset(MediaServiceContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates an instance of <see cref="Asset"/>
        /// </summary>
        /// <param name="context">
        /// <see cref="MediaServiceContext"/>
        /// </param>
        /// <param name="id">
        /// Asset id
        /// </param>
        public Asset(MediaServiceContext context, string id)
            : this(context)
        {
            Id = id;
        }

        /// <summary>
        /// Asset id.
        /// </summary>
        public string Id
        {
            get;
            internal set;
        }

        /// <summary>
        /// Asset state - Initialized (0), Deleted (1) [In Version 2.0, "Published" state is deprecated]   
        /// </summary>
        public AssetState State
        {
            get;
            set;
        }

        /// <summary>
        /// Date/time asset is created.
        /// </summary>
        public DateTime Created
        {
            get;
            internal set;
        }

        /// <summary>
        /// Date/time asset was last modified.
        /// </summary>
        public DateTime LastModified
        {
            get;
            internal set;
        }

        /// <summary>
        /// Alternate id for asset.
        /// </summary>
        public string AlternateId
        {
            get;
            set;
        }

        /// <summary>
        /// Friendly name for asset.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Encryption options for asset - None (0), StorageEncrypted (1), CommonEncryptionProtected (2)
        /// </summary>
        public AssetEncryptionOption Options
        {
            get;
            set;
        }


        /// <summary>
        /// Thumbnail URL of an asset
        /// </summary>
        public string ThumbnailUrl
        {
            get;
            set;
        }

        

       

        internal string BuildSasUrl(string locatorPath, string fileName)
        {
            string queryPart = new Uri(locatorPath).Query;
            string blobContainerUri = locatorPath.Substring(0, locatorPath.Length - queryPart.Length);
            string fileUrl = string.Format("{0}/{1}{2}", blobContainerUri, fileName, queryPart);
            return fileUrl;
        }
           
             
        /// <summary>
        /// Creates an asset
        /// </summary>
        internal Asset Create(string assetNAme)
        {
            try
            {
                this.Name = assetNAme;
                string assetCreateRequestPayloadFormat = @"{0} ""Name"": ""{1}"", ""State"": ""{2}"", ""Options"":""{3}"", ""Created"":""{4}"", ""LastModified"":""{5}"" {6}";
                string requestBody = string.Format(CultureInfo.InvariantCulture, assetCreateRequestPayloadFormat, "{", Name, (int)State, (int)Options,
                    Created.ToString("yyyy-MM-ddTHH:mm:ss"), LastModified.ToString("yyyy-MM-ddTHH:mm:ss"), "}");

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(CultureInfo.InvariantCulture, "{0}Assets/", _context.WamsEndpoint));
                request.Method = HttpVerbs.Post;
                request.ContentType = RequestContentType.Json;
                request.Accept = RequestContentType.Json;
                request.Headers.Add(RequestHeaders.XMsVersion, RequestHeaderValues.XMsVersion);
                request.Headers.Add(RequestHeaders.Authorization, string.Format(CultureInfo.InvariantCulture, RequestHeaderValues.Authorization, _context.AccessToken));
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
                        Id = d.Value<string>("Id");
                        State = (AssetState)d.Value<int>("State");
                        Options = (AssetEncryptionOption)d.Value<int>("Options");
                        AlternateId = d.Value<string>("AlternateId");
                        Created = d.Value<DateTime>("Created");
                        LastModified = d.Value<DateTime>("LastModified");
                    }
                }
            }
            catch (WebException webException) { Logger.WriteLog(webException); }
            catch (ProtocolViolationException protocolViolationException) { Logger.WriteLog(protocolViolationException); }
            catch (ObjectDisposedException objectDisposedException) { Logger.WriteLog(objectDisposedException); }
            catch (NotSupportedException notSupportedException) { Logger.WriteLog(notSupportedException); }
            catch (ArgumentNullException argumentNullException) { Logger.WriteLog(argumentNullException); }
            catch (SecurityException securityException) { Logger.WriteLog(securityException); }
            catch (UriFormatException uriFormatException) { Logger.WriteLog(uriFormatException); }
            catch (OutOfMemoryException outOfMemoryException) { Logger.WriteLog(outOfMemoryException); }
            catch (IOException ioException) { Logger.WriteLog(ioException); }
            catch (InvalidOperationException invalidOperationException) { Logger.WriteLog(invalidOperationException); }
            catch (Exception exp)
            {
                Logger.WriteLog(exp);
                Helper.HandleWebException(exp);
            }
            return this;
        }
      
    }
}
