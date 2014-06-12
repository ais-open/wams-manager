using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security;
using System.Text;
using System.Web;

namespace Ais.Internal.Dcm.Business
{
    public class MediaServiceContext
    {
        private const string acsEndpoint = "https://wamsprodglobal001acs.accesscontrol.windows.net/v2/OAuth2-13";

        private const string acsRequestBodyFormat = "grant_type=client_credentials&client_id={0}&client_secret={1}&scope=urn%3aWindowsAzureMediaServices";

        private string _accountName;
        public string AccountName { get { return _accountName; } set { _accountName = value; } }

        private string _accountKey;

        private string _accessToken;

        private DateTime _accessTokenExpiry;

        private bool _isRedirectionChecked;

        private string _wamsEndpoint = "https://media.windows.net/";

        /// <summary>
        /// Creates a new instance of <see cref="MediaServiceContext"/>
        /// </summary>
        /// <param name="accountName">
        /// Media service account name.
        /// </param>
        /// <param name="accountKey">
        /// Media service account key.
        /// </param>
        public MediaServiceContext(string accountName, string accountKey)
        {
            this._accountName = accountName;
            this._accountKey = accountKey;
        }

        /// <summary>
        /// Gets the access token. If access token is not yet fetched or the access token has expired,
        /// it gets a new access token.
        /// </summary>
        public string AccessToken
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_accessToken) || _accessTokenExpiry < DateTime.UtcNow)
                {
                    var tuple = FetchAccessToken();
                    _accessToken = tuple.Item1;
                    _accessTokenExpiry = tuple.Item2;
                }
                return _accessToken;
            }
        }

        /// <summary>
        /// Gets the endpoint for making REST API calls.
        /// </summary>
        public string WamsEndpoint
        {
            get
            {
                if (!_isRedirectionChecked)
                {
                    CheckForRedirection();
                }
                return _wamsEndpoint;
            }
        }

        /// <summary>
        /// This function makes the web request and gets the access token.
        /// </summary>
        /// <returns>
        /// <see cref="System.Tuple"/> containing 2 items - 
        /// 1. The access token. 
        /// 2. Token expiry date/time.
        /// </returns>
        private Tuple<string, DateTime> FetchAccessToken()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(acsEndpoint);
                request.Method = HttpVerbs.Post;
                string requestBody = string.Format(CultureInfo.InvariantCulture, acsRequestBodyFormat, _accountName,
                                                   HttpUtility.UrlEncode(_accountKey));
                request.ContentLength = Encoding.UTF8.GetByteCount(requestBody);
                request.ContentType = "application/x-www-form-urlencoded";
                using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(requestBody);
                }
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream(), true))
                    {
                        var returnBody = streamReader.ReadToEnd();
                        var acsToken = Newtonsoft.Json.JsonConvert.DeserializeObject<AcsToken>(returnBody);
                        return new Tuple<string, DateTime>(acsToken.access_token,
                                                           DateTime.UtcNow.AddSeconds(acsToken.expires_in));
                    }
                }
            }
            catch (WebException webException)
            {
                if (webException.Status == WebExceptionStatus.ProtocolError)
                {
                    var message =
                        string.Format("Status Code : {0}", ((HttpWebResponse)webException.Response).StatusCode) +
                        string.Format("Status Description : {0}",
                                      ((HttpWebResponse)webException.Response).StatusDescription);

                    //throw new WAMSException(message);
                }
                Logger.WriteLog(webException);
            }
            catch (NotSupportedException notSupportedException) { Logger.WriteLog(notSupportedException); }
            catch (ArgumentNullException argumentNullException) { Logger.WriteLog(argumentNullException); }
            catch (SecurityException securityException) { Logger.WriteLog(securityException); }
            catch (UriFormatException uriFormatException) { Logger.WriteLog(uriFormatException); }
            catch (ProtocolViolationException protocolViolationException) { Logger.WriteLog(protocolViolationException); }
            catch (ObjectDisposedException objectDisposedException) { Logger.WriteLog(objectDisposedException); }
            catch (FormatException formatException) { Logger.WriteLog(formatException); }
            catch (InvalidOperationException invalidOperationException) { Logger.WriteLog(invalidOperationException); }
            catch (OutOfMemoryException outOfMemoryException) { Logger.WriteLog(outOfMemoryException); }
            catch (IOException ioException) { Logger.WriteLog(ioException); }
            return null;
        }

        /// <summary>
        /// This function checks if we need to redirect all WAMS requests.
        /// </summary>
        private void CheckForRedirection()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_wamsEndpoint);
                request.AllowAutoRedirect = false;
                request.Headers.Add(RequestHeaders.XMsVersion, RequestHeaderValues.XMsVersion);
                request.Headers.Add(RequestHeaders.Authorization,
                                    string.Format(CultureInfo.InvariantCulture, RequestHeaderValues.Authorization,
                                                  AccessToken));
                request.Method = HttpVerbs.Get;

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.Moved ||
                        response.StatusCode == HttpStatusCode.MovedPermanently)
                    {
                        string newLocation = response.Headers["Location"];
                        if (!newLocation.Equals(_wamsEndpoint))
                        {
                            _wamsEndpoint = newLocation;
                            _accessToken = string.Empty; //So that we can force to get a new access token.
                            _accessTokenExpiry = DateTime.MinValue;
                            _isRedirectionChecked = true;
                        }
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
            catch (InvalidOperationException invalidOperationException) { Logger.WriteLog(invalidOperationException); }
        }

        /// <summary>
        /// List all assets in a media service.
        /// </summary>
        /// <returns>
        /// </returns>
        private IEnumerable<Asset> GetAllAssets()
        {
            List<Asset> assets = null;
            try
            {
                assets = new List<Asset>();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(CultureInfo.InvariantCulture, "{0}Assets", WamsEndpoint));
                request.Accept = RequestContentType.Json;
                request.Method = HttpVerbs.Get;
                request.Headers.Add(RequestHeaders.XMsVersion, RequestHeaderValues.XMsVersion);
                request.Headers.Add(RequestHeaders.Authorization, string.Format(CultureInfo.InvariantCulture, RequestHeaderValues.Authorization, AccessToken));
                request.Headers.Add(RequestHeaders.DataServiceVersion, RequestHeaderValues.DataServiceVersion);
                request.Headers.Add(RequestHeaders.MaxDataServiceVersion, RequestHeaderValues.MaxDataServiceVersion);
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream(), true))
                    {
                        var returnBody = streamReader.ReadToEnd();
                        JObject responseJsonObject = JObject.Parse(returnBody);
                        var items = responseJsonObject["d"]["results"];
                        foreach (var d in items)
                        {
                            var Id = d.Value<string>("Id");
                            var State = (AssetState)d.Value<int>("State");
                            var Options = (AssetEncryptionOption)d.Value<int>("Options");
                            var AlternateId = d.Value<string>("AlternateId");
                            var Created = d.Value<DateTime>("Created");
                            var LastModified = d.Value<DateTime>("LastModified");
                            var Name = d.Value<string>("Name");
                            assets.Add(new Asset(this, Id)
                            {
                                Name = Name,
                                State = State,
                                Options = Options,
                                AlternateId = AlternateId,
                                Created = Created,
                                LastModified = LastModified,
                            });
                        }
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
            return assets;
        }

        private IEnumerable<Asset> GetFilteredAssets()
        {
            List<Asset> assets = null;

            assets = new List<Asset>();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(CultureInfo.InvariantCulture, "{0}Assets&$filter=", WamsEndpoint));
            request.Accept = RequestContentType.Json;
            request.Method = HttpVerbs.Get;
            request.Headers.Add(RequestHeaders.XMsVersion, RequestHeaderValues.XMsVersion);
            request.Headers.Add(RequestHeaders.Authorization, string.Format(CultureInfo.InvariantCulture, RequestHeaderValues.Authorization, AccessToken));
            request.Headers.Add(RequestHeaders.DataServiceVersion, RequestHeaderValues.DataServiceVersion);
            request.Headers.Add(RequestHeaders.MaxDataServiceVersion, RequestHeaderValues.MaxDataServiceVersion);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream(), true))
                {
                    var returnBody = streamReader.ReadToEnd();
                    JObject responseJsonObject = JObject.Parse(returnBody);
                    var items = responseJsonObject["d"]["results"];
                    foreach (var d in items)
                    {
                        var Id = d.Value<string>("Id");
                        var State = (AssetState)d.Value<int>("State");
                        var Options = (AssetEncryptionOption)d.Value<int>("Options");
                        var AlternateId = d.Value<string>("AlternateId");
                        var Created = d.Value<DateTime>("Created");
                        var LastModified = d.Value<DateTime>("LastModified");
                        var Name = d.Value<string>("Name");
                        assets.Add(new Asset(this, Id)
                        {
                            Name = Name,
                            State = State,
                            Options = Options,
                            AlternateId = AlternateId,
                            Created = Created,
                            LastModified = LastModified,
                        });
                    }
                }
            }


            return assets;
        }
        
        private string GetLocator(string assetId, string accessPolicyID, LocatorType locatorType)
        {
            string url = string.Empty;
            try
            {
                List<Locator> locators = this.GetAllLocators(assetId);
                Locator retLocator = null;
                foreach (var locator in locators)
                {
                    if (locator.AccessPolicyId == accessPolicyID && locator.Type == locatorType)
                    {
                        retLocator = locator;
                        url = locator.Path;
                        break;
                    }
                }
                if (retLocator == null)
                {
                    url = CreateLocator(accessPolicyID, assetId, locatorType);
                }
            }
            catch (Exception exception) { Logger.WriteLog(exception); }
            return url;
        }

        private string CreateLocator(string accessPolicyID, string assetID, LocatorType locatorType)
        {
            string locatorPath = string.Empty;
            try
            {
                string UtcTime = DateTime.UtcNow.AddMinutes(-5).ToString("MM/dd/yyyy hh:mm:ss tt");
                string requestBody = "{\"AccessPolicyId\": \"" + accessPolicyID + "\", \"AssetId\" : \"" + assetID +
                                     "\", \"StartTime\" : \"" + UtcTime + "\", \"Type\" :" + (int)locatorType + " }";
                HttpWebRequest request =
                    (HttpWebRequest)
                    WebRequest.Create(string.Format(CultureInfo.InvariantCulture, "{0}Locators ", this.WamsEndpoint));
                request.Accept = RequestContentType.Json;
                request.Method = HttpVerbs.Post;
                request.ContentType = RequestContentType.Json;
                request.Headers.Add(RequestHeaders.XMsVersion, RequestHeaderValues.XMsVersion);
                request.Headers.Add(RequestHeaders.Authorization,
                                    string.Format(CultureInfo.InvariantCulture, RequestHeaderValues.Authorization,
                                                  this.AccessToken));
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
                        locatorPath = d.Value<string>("Path");
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

            return locatorPath;
        }
        public string GetSasUrlForUpload(string assetId)
        {
            string locatorPath = string.Empty;
            AccessPolicy policy = this.GetAccessPolicyAssetID(AccessPolicyPermission.Write, assetId);
            if (policy != null && !string.IsNullOrWhiteSpace(policy.Id))
            {
                locatorPath = GetLocator(assetId, policy.Id, LocatorType.SaS);
                if (!string.IsNullOrWhiteSpace(locatorPath)) return locatorPath;
            }
            return locatorPath;
        }

        /// <summary>
        /// Creates New Asset with given name 
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns>Asset ID</returns>
        public Asset CreateNewAsset(string assetName)
        {
            Asset asset = null;
            try
            {
                asset = new Asset(this);
                asset.Create(assetName);
            }
            catch (WebException webEx)
            {
                using (StreamReader streamReader = new StreamReader(webEx.Response.GetResponseStream(), true))
                {
                    var rslt = streamReader.ReadToEnd();
                    Logger.WriteLog(rslt);
                }
            }
            return asset;
        }

        public AccessPolicy CreateNewPolicy(AccessPolicyPermission permission)
        {
            AccessPolicy retpolicy = null;
            string name = string.Empty;
            try
            {
                switch (permission)
                {
                    case AccessPolicyPermission.None:
                        break;
                    case AccessPolicyPermission.Read:
                        name = Literals.DOWNLOAD_POLICY_NAME;
                        break;
                    case AccessPolicyPermission.Write:
                        name = Literals.UPLOAD_POLICY_NAME;
                        break;
                    case AccessPolicyPermission.Delete:
                        name = Literals.DELETE_POLICY_NAME;
                        break;
                    case AccessPolicyPermission.List:
                        break;
                    default:
                        break;
                }
                retpolicy = new AccessPolicy(name, Literals.DURATION_IN_MINUTES, permission, this);
            }
            catch (Exception exp)
            {
                Logger.WriteLog(exp);
                // log or throw
                throw; //CA2200 (Rethrow to preserve stack details)
            }
            return retpolicy;
        }

        private string GetThumbnailUrl(string assetID, List<Asset> allAssets)
        {
            string fileSasUrl = string.Empty;

            try
            {
                List<AssetFile> files = null;
                AccessPolicy readAccessPolicy = null;
                string sasUrl = string.Empty;
                Asset asset = GetLatestThumbnailAsset(assetID, allAssets);
                bool filesExists = false;
                if (asset != null)
                {
                    files = ListAssetFiles(asset.Id);
                    if (files != null && files.Count > 1)
                    {
                        filesExists = true;
                        readAccessPolicy = this.GetAccessPolicyAssetID(AccessPolicyPermission.Read, asset.Id);
                        //create or get access policy
                        if (readAccessPolicy != null)
                        {
                            sasUrl = this.GetLocator(assetID, readAccessPolicy.Id, LocatorType.SaS); // create container SAS URL
                        }
                    }
                }
                if (filesExists)
                {
                    foreach (AssetFile file in files) // for each file create file SAS url
                    {
                        fileSasUrl = asset.BuildSasUrl(sasUrl, file.Name);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
            }
            return fileSasUrl;
        }

        public List<string> GetThumbnailUrls(string assetID)
        {
            List<string> thumbnailUrls = new List<string>();
            try
            {
                List<AssetFile> files = null;
                string sasUrl = string.Empty;
                bool filesExists = false;
                List<Asset> assets = GetThumbnailAsset(assetID); // GetThumbnail Assets.

                AccessPolicy readAccessPolicy = null;
                // Gaurav - 11-May-2013: I think there's something wrong with the code below. Since we're creating an asset for thumbnails,
                // shouldn't we be checking for access policy in that asset instead of main asset?
                //readAccessPolicy = this.GetAccessPolicyAssetID(AccessPolicyPermission.Read, assetID);//create access policy
                foreach (Asset asset in assets)
                {
                    filesExists = false;
                    if (asset != null)
                    {
                        // Gaurav - 11-May-2013: It may very well happen that the asset may not been created so we have to take that into consideration as well.
                        files = ListAssetFiles(asset.Id);
                        if (files != null && files.Count > 1)
                        {
                            filesExists = true;
                            readAccessPolicy = this.GetAccessPolicyAssetID(AccessPolicyPermission.Read, asset.Id);
                                //create or get access policy
                            // files = asset.ListAssetFiles();// Get All the files.
                            if (readAccessPolicy != null)
                            {
                                sasUrl = this.GetLocator(asset.Id,readAccessPolicy.Id, LocatorType.SaS);
                                    // create container SAS URL
                            }
                        }
                    }
                    if (filesExists)
                        foreach (AssetFile file in files) // for each file create file SAS url
                        {
                            string fileSasUrl = asset.BuildSasUrl(sasUrl, file.Name);
                            if (!string.IsNullOrWhiteSpace(fileSasUrl))
                            {
                                thumbnailUrls.Add(fileSasUrl);
                            }
                        }
                }
            }
            catch (Exception exp)
            {
                Logger.WriteLog(exp);
                throw Helper.HandleWebException(exp);
            }
            // return the list
            return thumbnailUrls;
        }

        private List<Asset> GetThumbnailAsset(string assetID)
        {
            List<Asset> thmbAssets = new List<Asset>();
            try
            {
                IEnumerable<Asset> assets = GetAllAssets();
                foreach (Asset asset in assets)
                {
                    if (asset.Name.StartsWith(string.Format("{0}-{1}-{2}", Literals.CHILD_ASSET_APPEND, assetID, Literals.THUMBNAIL)))
                    {
                        thmbAssets.Add(asset);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
            }
            return thmbAssets;
        }

        /// <summary>
        /// Returns the latest thumbnail asset
        /// </summary>
        /// <param name="assetID">assetID</param>
        /// <returns>Asset</returns>
        private Asset GetLatestThumbnailAsset(string assetID, List<Asset> allAssets)
        {
            Asset latestAsset = null;
            try
            {
                DateTime time = DateTime.MinValue;
                foreach (Asset asset in allAssets)
                {
                    if (
                        asset.Name.StartsWith(string.Format("{0}-{1}-{2}", Literals.CHILD_ASSET_APPEND, assetID,
                                                            Literals.THUMBNAIL)))
                    {
                        if (asset.LastModified != null && asset.LastModified > time)
                        {
                            latestAsset = asset;
                            time = asset.LastModified;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
            }
            return latestAsset;
        }

        public List<string> GetLatestThumbnailUrl(string assetID,string fileName)
        {
            List<string> thumbnailUrls = new List<string>();
            try
            {
                List<AssetFile> files = null;
                string sasUrl = string.Empty;
                bool filesExists = false;
                List<Asset> assets = GetThumbnailAsset(assetID); // GetThumbnail Assets.
                AccessPolicy readAccessPolicy = null;
                // Gaurav - 11-May-2013: I think there's something wrong with the code below. Since we're creating an asset for thumbnails,
                // shouldn't we be checking for access policy in that asset instead of main asset?
                //readAccessPolicy = this.GetAccessPolicyAssetID(AccessPolicyPermission.Read, assetID);//create access policy
                bool fileFound = false; // will be used to break the parent foreach
                foreach (Asset asset in assets)
                {
                    filesExists = false;
                    if (asset != null)
                    {
                        // Gaurav - 11-May-2013: It may very well happen that the asset may not been created so we have to take that into consideration as well.
                        files = ListAssetFiles(asset.Id);
                        if (files != null && files.Count > 1)
                        {
                            filesExists = true;
                            readAccessPolicy = this.GetAccessPolicyAssetID(AccessPolicyPermission.Read, asset.Id);//create or get access policy
                            // files = asset.ListAssetFiles();// Get All the files.
                            if (readAccessPolicy != null)
                            {
                                sasUrl = this.GetLocator(asset.Id,readAccessPolicy.Id, LocatorType.SaS); // create container SAS URL
                            }
                        }
                    }
                    if (filesExists)
                        foreach (AssetFile file in files) // for each file create file SAS url
                        {
                            if (file.Name.Contains(Path.GetFileNameWithoutExtension(fileName)))
                            {
                                string fileSasUrl = asset.BuildSasUrl(sasUrl, file.Name);
                                if (!string.IsNullOrWhiteSpace(fileSasUrl))
                                {
                                    thumbnailUrls.Add(fileSasUrl);
                                    fileFound = true;
                                    break;//break child foreach loop
                                }
                            }
                        }
                    if (fileFound)
                    {
                        //break the patent loop, no need to iterate further
                        break;
                    }
                }
            }
            catch (Exception exp)
            {
                Logger.WriteLog(exp);
                throw Helper.HandleWebException(exp);
            }
            // return the list
            return thumbnailUrls;
        }

        public List<AssetWithFiles> GetOutputAssetFiles(string assetID)
        {
            List<AssetWithFiles> mediaFileUrls = new List<AssetWithFiles>();
            try
            {
                List<AssetFile> files = null;
                string sasUrl = string.Empty;
                AccessPolicy readAccessPolicy = null;
                List<Asset> assets = GetEncodingAssets(assetID); // GetThumbnail Assets.
                bool filesExist = false;
                foreach (var asset in assets)
                {
                    DateTime createdDate = asset.Created;
                    if (asset != null)
                    {
                        files = ListAssetFiles(asset.Id);// Get All the files.

                        if (files != null && files.Count > 0)
                        {
                            filesExist = true;
                            readAccessPolicy = this.GetAccessPolicyAssetID(AccessPolicyPermission.Read, asset.Id);//create access policy
                        }

                    }
                    if (filesExist)
                    {
                        foreach (AssetFile file in files) // for each file create file SAS url
                        {
                            if (file.IsPrimary)
                            {
                                LocatorType type = (file.Name.ToLower().EndsWith(".ism")) ? LocatorType.OnDemandOrigin : LocatorType.SaS;
                                if (readAccessPolicy != null)
                                {
                                    sasUrl = GetLocator(asset.Id,
                                        readAccessPolicy.Id, type); // create container SAS URL
                                }
                                string fileSasUrl = type == LocatorType.OnDemandOrigin
                                                        ? (asset.BuildSasUrl(sasUrl, file.Name) + @"/Manifest")
                                                        : asset.BuildSasUrl(sasUrl, file.Name);
                                if (!string.IsNullOrWhiteSpace(fileSasUrl))
                                {
                                    mediaFileUrls.Add(new AssetWithFiles { OutputAssetId= asset.Id, Name = FormatName(asset.Name, asset.Id), URL = fileSasUrl, CreatedDate = createdDate });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                Logger.WriteLog(exp);
                Helper.HandleWebException(exp);
            }
            // return the list
            return mediaFileUrls;
        }

        private string FormatName(string assetName, string assetID)
        {
            return assetName.Replace(assetID, "").Replace(Literals.ENCODING, "").Replace(Literals.CHILD_ASSET_APPEND, "").Split('_')[1].Split('-')[0];
        }

        private List<Asset> GetEncodingAssets(string assetID)
        {
            List<Asset> outputAssets = new List<Asset>();
            try
            {
                IEnumerable<Asset> assets = GetAllAssets();
                foreach (Asset asset in assets)
                {
                    if (
                        asset.Name.StartsWith(string.Format("{0}-{1}-{2}", Literals.CHILD_ASSET_APPEND, assetID,
                                                            Literals.ENCODING)))
                    {
                        outputAssets.Add(asset);
                    }
                }
            }
            catch(FormatException formatException){Logger.WriteLog(formatException);}
            catch(ArgumentNullException argumentNullException){Logger.WriteLog(argumentNullException);}
            catch(Exception exception){Logger.WriteLog(exception);}
            return outputAssets;
        }

        internal List<Locator> GetAllLocators(string assetId)
        {
            List<Locator> locators = new List<Locator>();

            try
            {
                HttpWebRequest request =
                    (HttpWebRequest)
                    WebRequest.Create(string.Format(CultureInfo.InvariantCulture, "{0}Assets('{1}')/Locators",
                                                    this.WamsEndpoint, assetId));
                request.Method = HttpVerbs.Get;
                request.Accept = RequestContentType.Json;
                request.Headers.Add(RequestHeaders.XMsVersion, RequestHeaderValues.XMsVersion);
                request.Headers.Add(RequestHeaders.Authorization,
                                    string.Format(CultureInfo.InvariantCulture, RequestHeaderValues.Authorization,
                                                  this.AccessToken));
                request.Headers.Add(RequestHeaders.DataServiceVersion, RequestHeaderValues.DataServiceVersion);
                request.Headers.Add(RequestHeaders.MaxDataServiceVersion, RequestHeaderValues.MaxDataServiceVersion);
                using (var response = (HttpWebResponse) request.GetResponse())
                {
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream(), true))
                    {
                        var returnBody = streamReader.ReadToEnd();
                        JObject responseJsonObject = JObject.Parse(returnBody);
                        var items = responseJsonObject["d"]["results"];
                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                Locator file = Helper.ParseLocator(item, this);
                                locators.Add(file);
                            }
                        }
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

            return locators;
        }

        private AccessPolicy GetAccessPolicyById(string policyId)
        {
            AccessPolicy policy = null;
            try
            {
                HttpWebRequest request =
                    (HttpWebRequest)
                    WebRequest.Create(string.Format(CultureInfo.InvariantCulture, "{0}AccessPolicies('{1}')",
                                                    this.WamsEndpoint, policyId));
                request.Method = HttpVerbs.Get;
                request.Accept = RequestContentType.Json;
                request.Headers.Add(RequestHeaders.XMsVersion, RequestHeaderValues.XMsVersion);
                request.Headers.Add(RequestHeaders.Authorization,
                                    string.Format(CultureInfo.InvariantCulture, RequestHeaderValues.Authorization,
                                                  this.AccessToken));
                request.Headers.Add(RequestHeaders.DataServiceVersion, RequestHeaderValues.DataServiceVersion);
                request.Headers.Add(RequestHeaders.MaxDataServiceVersion, RequestHeaderValues.MaxDataServiceVersion);
                using (var response = (HttpWebResponse) request.GetResponse())
                {
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream(), true))
                    {
                        var returnBody = streamReader.ReadToEnd();
                        JObject responseJsonObject = JObject.Parse(returnBody);
                        var items = responseJsonObject["d"];

                        policy = Helper.ParseAccessPolicy(items);

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

            return policy;
        }

        public Job CreateThumbnailJob(string assetID, int imageQuality = 100, int height = 640, int width = 480, int invervalInSeconds = 10, int stopAfterInSeconds = 10, string imageType = "Jpeg")
        {
            string jobName = "JOB_" + assetID + "_" + Literals.THUMBNAIL;
            string outputAssetName =Helper.GetChildAssetName(Literals.THUMBNAIL,assetID);
            string intervalString = string.Format("{0}:{1}:{2}", (invervalInSeconds / 3600) % 60, (invervalInSeconds / 60) % 60, (invervalInSeconds) % 60);
            string stopAfterString = string.Format("{0}:{1}:{2}", (stopAfterInSeconds / 3600) % 60, (stopAfterInSeconds / 60) % 60, (stopAfterInSeconds) % 60);
            Job job = null;
            try
            {
                string uri = string.Format("https://media.windows.net/api/Assets('{0}')", assetID);
                string requestBody = "{\"Name\" : \"" + jobName + "\", \"InputMediaAssets\" : [{\"__metadata\" : {\"uri\" : \"" + uri + "\"}}],  \"Tasks\" : [{\"Configuration\" : \"<?xml version=\\\"1.0\\\" encoding=\\\"utf-8\\\"?><Thumbnail Size=\\\"" + width + "," + height + "\\\" Type=\\\"Jpeg\\\" JpegQuality=\\\"" + imageQuality + "\\\" Filename=\\\"{OriginalFilename}_{ThumbnailTime}.{DefaultExtension}\\\">  <Time Value=\\\"0:0:0\\\"/>  <Time Value=\\\"0:0:3\\\" Step=\\\"" + intervalString + "\\\" Stop=\\\"" + stopAfterString + "\\\"/></Thumbnail>\", \"MediaProcessorId\" : \"nb:mpid:UUID:70bdc2c3-ebf4-42a9-8542-5afc1e55d217\",  \"TaskBody\" :\"<?xml version=\\\"1.0\\\" encoding=\\\"utf-8\\\"?><taskBody><inputAsset>JobInputAsset(0)</inputAsset><outputAsset assetName=\\\"" + outputAssetName + "\\\">JobOutputAsset(0)</outputAsset></taskBody>\" }]}";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(CultureInfo.InvariantCulture, "{0}Jobs", this.WamsEndpoint));
                request.Accept = RequestContentType.Json;
                request.Method = HttpVerbs.Post;
                request.ContentType = RequestContentType.Json;
                request.Headers.Add(RequestHeaders.XMsVersion, RequestHeaderValues.XMsVersion);
                request.Headers.Add(RequestHeaders.Authorization, string.Format(CultureInfo.InvariantCulture, RequestHeaderValues.Authorization, this.AccessToken));
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
                        job = Helper.ParseJob(d, this);
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
            catch (Exception webEx)
            {
                Logger.WriteLog(webEx);
                throw Helper.HandleWebException(webEx);
            }
            return job;
        }

        internal AccessPolicy GetAccessPolicyAssetID(AccessPolicyPermission permission, string assetId)
        {
            AccessPolicy policy = null;
            try
            {
                List<Locator> locators = GetAllLocators(assetId);
                if (locators != null && locators.Count > 0)
                {
                    foreach (var locator in locators)
                    {
                        if (locator.Type == LocatorType.SaS && locator.ExpirationDateTime > DateTime.UtcNow.AddHours(1))
                        {
                            policy = GetAccessPolicyById(locator.AccessPolicyId);
                            //locator.Delete();
                            if (policy.Permissions == permission)
                            {
                                break;
                            }
                            else
                            {
                                policy = null; //reset not the policy we are looking for
                            }
                        }
                        if (locator.Type == LocatorType.OnDemandOrigin &&
                            locator.ExpirationDateTime > DateTime.UtcNow.AddHours(1))
                        {
                            policy = GetAccessPolicyById(locator.AccessPolicyId);
                            //locator.Delete();
                            if (policy.Permissions == permission)
                            {
                                break;
                            }
                            else
                            {
                                policy = null; //reset not the policy we are looking for
                            }
                        }
                    }
                }
                if (policy == null)
                {
                    policy = CreateNewPolicy(permission);
                }
            }
            catch(Exception exception)
            {
                Logger.WriteLog(exception);
            }
            return policy;
        }

        public List<ThumbnailModel> GetThumbnailModels(string assetId)
        {
            List<ThumbnailModel> thumbnailModels = new List<ThumbnailModel>();
            try
            {
                List<AssetFile> files = null;
                string sasUrl = string.Empty;
                bool filesExists = false;
                List<Asset> assets = GetThumbnailAsset(assetId); // GetThumbnail Assets.

                AccessPolicy readAccessPolicy = null;
                // Gaurav - 11-May-2013: I think there's something wrong with the code below. Since we're creating an asset for thumbnails,
                // shouldn't we be checking for access policy in that asset instead of main asset?
                //readAccessPolicy = this.GetAccessPolicyAssetID(AccessPolicyPermission.Read, assetID);//create access policy
                foreach (Asset asset in assets)
                {
                    filesExists = false;
                    if (asset != null)
                    {
                        // Gaurav - 11-May-2013: It may very well happen that the asset may not been created so we have to take that into consideration as well.
                        files = ListAssetFiles(asset.Id);
                        if (files != null && files.Count > 1)
                        {
                            filesExists = true;
                            readAccessPolicy = this.GetAccessPolicyAssetID(AccessPolicyPermission.Read, asset.Id);
                            //create or get access policy
                            // files = asset.ListAssetFiles();// Get All the files.
                            if (readAccessPolicy != null)
                            {
                                sasUrl = GetLocator(asset.Id,readAccessPolicy.Id, LocatorType.SaS);
                                // create container SAS URL
                            }
                        }
                    }
                    if (filesExists)
                        foreach (AssetFile file in files) // for each file create file SAS url
                        {
                            string fileSasUrl = asset.BuildSasUrl(sasUrl, file.Name);
                            if (!string.IsNullOrWhiteSpace(fileSasUrl))
                            {
                                thumbnailModels.Add(new ThumbnailModel
                                    {
                                        AssetId = assetId,
                                        AssetFileId = file.ParentAssetId,
                                        ThumbnailFileId = file.Id,
                                        CreatedDate = file.Created,
                                        URL = fileSasUrl
                                    });
                            }
                        }
                }
            }
            catch (Exception exp)
            {
                Logger.WriteLog(exp);
                throw Helper.HandleWebException(exp);
            }
            // return the list
            return thumbnailModels;
        }

        /// <summary>
        /// Returns List of Asset Files
        /// </summary>
        public List<AssetFile> ListAssetFiles(string assetId)
        {
            List<AssetFile> files = new List<AssetFile>();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(CultureInfo.InvariantCulture, "{0}Assets('{1}')/Files ", this.WamsEndpoint, assetId));
                request.Accept = RequestContentType.Json;
                request.Method = HttpVerbs.Get;
                request.ContentType = RequestContentType.Json;
                request.Headers.Add(RequestHeaders.XMsVersion, RequestHeaderValues.XMsVersion);
                request.Headers.Add(RequestHeaders.Authorization, string.Format(CultureInfo.InvariantCulture, RequestHeaderValues.Authorization, this.AccessToken));
                request.Headers.Add(RequestHeaders.DataServiceVersion, RequestHeaderValues.DataServiceVersion);
                request.Headers.Add(RequestHeaders.MaxDataServiceVersion, RequestHeaderValues.MaxDataServiceVersion);

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream(), true))
                    {
                        var returnBody = streamReader.ReadToEnd();
                        JObject responseJsonObject = JObject.Parse(returnBody);
                        var items = responseJsonObject["d"]["results"];
                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                AssetFile file = Helper.ParseAssetFile(item);
                                files.Add(file);
                            }
                        }
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
                throw Helper.HandleWebException(exp);
            }

            return files;
        }

        public bool GenerateFileMetadata(string assetID)
        {
            bool isSuccess = false;

            try
            {
                HttpWebRequest request =
                    (HttpWebRequest)
                    WebRequest.Create(string.Format(CultureInfo.InvariantCulture, "{0}CreateFileInfos?assetid='{1}'",
                                                    this.WamsEndpoint, assetID));
                request.Accept = RequestContentType.Json;
                request.Method = HttpVerbs.Get;
                request.ContentType = RequestContentType.Json;
                request.Headers.Add(RequestHeaders.XMsVersion, RequestHeaderValues.XMsVersion);
                request.Headers.Add(RequestHeaders.Authorization,
                                    string.Format(CultureInfo.InvariantCulture, RequestHeaderValues.Authorization,
                                                  this.AccessToken));
                request.Headers.Add(RequestHeaders.DataServiceVersion, RequestHeaderValues.DataServiceVersion);
                request.Headers.Add(RequestHeaders.MaxDataServiceVersion, RequestHeaderValues.MaxDataServiceVersion);
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.NoContent)
                    {
                        isSuccess = true;
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

            return isSuccess;
        }

        public Job CreateEncodingJobV2(string assetId, string taskBody, string encodingJobName)
        {
            string jobName = "JOB_" + assetId + "_" + Literals.ENCODING + encodingJobName;
            string outputAssetName = Helper.GetChildAssetName(Literals.ENCODING + "_" + encodingJobName, assetId);
            Job job = null;
            try
            {
                string uri = string.Format("https://media.windows.net/api/Assets('{0}')", assetId);//string.Format("https://media.windows.net/api/Assets('{0}')", Id);
                string requestBody = "";// "{\"Name\" : \"" + jobName + "\", \"InputMediaAssets\" : [{\"__metadata\" : {\"uri\" : \"" + uri + "\"}}],  \"Tasks\" : [{\"Configuration\" : \"" + encodingJobName + "\", \"MediaProcessorId\" : \"nb:mpid:UUID:70bdc2c3-ebf4-42a9-8542-5afc1e55d217\",  \"TaskBody\" : \"<?xml version=\\\"1.0\\\" encoding=\\\"utf-8\\\"?><taskBody><inputAsset>JobInputAsset(0)</inputAsset><outputAsset assetName=\\\"" + outputAssetName + "\\\">JobOutputAsset(0)</outputAsset></taskBody>\"}]}";
                requestBody = taskBody;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(CultureInfo.InvariantCulture, "{0}Jobs", this.WamsEndpoint));
                request.Accept = RequestContentType.Json;
                request.Method = HttpVerbs.Post;
                request.ContentType = RequestContentType.Json;
                request.Headers.Add(RequestHeaders.XMsVersion, RequestHeaderValues.XMsVersion);
                request.Headers.Add(RequestHeaders.Authorization, string.Format(CultureInfo.InvariantCulture, RequestHeaderValues.Authorization, this.AccessToken));
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
                        job = Helper.ParseJob(d, this);
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
            catch (Exception webEx)
            {
                Logger.WriteLog(webEx);
                throw Helper.HandleWebException(webEx);
            }
            return job;
        }

        public bool MakePrimaryFile(string assetFileID, bool makePrimary)
        {
            bool isSuccess = false;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(CultureInfo.InvariantCulture, "{0}Files('{1}')", this.WamsEndpoint, assetFileID));
                request.Accept = RequestContentType.Json;
                request.Method = HttpVerbs.Merge;
                request.ContentType = RequestContentType.Json;
                request.Headers.Add(RequestHeaders.XMsVersion, RequestHeaderValues.XMsVersion);
                request.Headers.Add(RequestHeaders.Authorization, string.Format(CultureInfo.InvariantCulture, RequestHeaderValues.Authorization, this.AccessToken));
                request.Headers.Add(RequestHeaders.DataServiceVersion, RequestHeaderValues.DataServiceVersion);
                request.Headers.Add(RequestHeaders.MaxDataServiceVersion, RequestHeaderValues.MaxDataServiceVersion);
                string requestBody = makePrimary ? "{\"IsPrimary\" : \"true\"}" : "{\"IsPrimary\" : \"false\"}";
                using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(requestBody);
                }
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.NoContent)
                        isSuccess = true;
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
                throw Helper.HandleWebException(exp);
            }
            return isSuccess;
        }

    }

    public class AssetWithFiles
    {
        public string OutputAssetId { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ThumbnailModel
    {
        public string AssetId { get; set; }
        public string AssetFileId { get; set; }
        public string ThumbnailFileId { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public DateTime CreatedDate { get; set; }
    }

}
