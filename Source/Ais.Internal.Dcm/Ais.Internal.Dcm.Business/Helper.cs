using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Ais.Internal.Dcm.Business
{
    class Helper
    {
        public static Job ParseJob(JToken token, MediaServiceContext context)
        {
            Job job = new Job(context);
            job.Id = token.Value<string>("Id");
            job.Name = token.Value<string>("Name");
            job.State = (JobState)token.Value<int>("State");
            return job;
        }

        public static List<Asset> ParseAssets(JToken items, MediaServiceContext context)
        {
            List<Asset> assets = new List<Asset>();
            foreach (var d in items)
            {
                var Id = d.Value<string>("Id");
                var State = (AssetState)d.Value<int>("State");
                var Options = (AssetEncryptionOption)d.Value<int>("Options");
                var AlternateId = d.Value<string>("AlternateId");
                var Created = d.Value<DateTime>("Created");
                var LastModified = d.Value<DateTime>("LastModified");
                var Name = d.Value<string>("Name");
                //if (!Name.StartsWith(Literals.CHILD_ASSET_APPEND) && !Name.StartsWith(Literals.TEMP_ASSET))
                //{
                    Asset newAsset = new Asset(context, Id)
                    {
                        Name = Name,
                        State = State,
                        Options = Options,
                        AlternateId = AlternateId,
                        Created = Created,
                        LastModified = LastModified
                    };
                    assets.Add(newAsset);
                //}
            }
            return assets;
        }


        public static Locator ParseLocator(JToken token, MediaServiceContext context)
        {
            Locator locator = new Locator(context);
            locator.Id = token.Value<string>("Id");
            locator.ExpirationDateTime = token.Value<DateTime>("ExpirationDateTime");
            locator.Type = (LocatorType)token.Value<int>("Type");
            locator.Path = token.Value<string>("Path");
            locator.BaseUri = token.Value<string>("BaseUri");
            locator.ContentAccessComponent = token.Value<string>("ContentAccessComponent");
            locator.AccessPolicyId = token.Value<string>("AccessPolicyId");
            locator.AssetId = token.Value<string>("AssetId");
            return locator;
        }

        public static AssetFile ParseAssetFile(JToken token)
        {
            var fileID = token.Value<string>("Id");
            var fileName = token.Value<string>("Name");
            var ContentFileSize = token.Value<long>("ContentFileSize");
            var ParentAssetId = token.Value<string>("ParentAssetId");
            var EncryptionVersion = token.Value<string>("EncryptionVersion");
            var EncryptionScheme = token.Value<string>("EncryptionScheme");
            var IsEncrypted = token.Value<bool>("IsEncrypted");
            var EncryptionKeyId = token.Value<string>("EncryptionKeyId");
            var InitializationVector = token.Value<string>("InitializationVector");
            var IsPrimary = token.Value<bool>("IsPrimary");
            var LastModified = token.Value<DateTime>("LastModified");
            var Created = token.Value<DateTime>("Created");
            var MimeType = token.Value<string>("MimeType");
            var ContentChecksum = token.Value<string>("ContentChecksum");

            return new AssetFile
                            {
                                Id = fileID,
                                Name = fileName,
                                ContentFileSize = ContentFileSize,
                                ParentAssetId = ParentAssetId,
                                EncryptionVersion = EncryptionVersion,
                                EncryptionScheme = EncryptionScheme,
                                IsEncrypted = IsEncrypted,
                                EncryptionKeyId = EncryptionKeyId,
                                InitializationVector = InitializationVector,
                                IsPrimary = IsPrimary,
                                LastModified = LastModified,
                                Created = Created,
                                MimeType = MimeType,
                                ContentChecksum = ContentChecksum,

                            };
        }

        public static WAMSException HandleWebException(Exception exp)
        {
            string message = string.Empty;
            WAMSException exception = new WAMSException();//empty exception in case of exception while parsing the exception
            WebExceptionStatus code ;
            if (exp is WebException)
            {
                WebException webEx = (WebException)exp;
                using (StreamReader streamReader = new StreamReader(webEx.Response.GetResponseStream(), true))
                {
                    code = webEx.Status;
                    message = streamReader.ReadToEnd();
                }
                exception = new WAMSException { WAMSMessage = string.Format("Web Exception: Status Code {0}", code), Detail = message };
            }
            else
            {
                exception = new WAMSException { WAMSMessage = exp.Message, Detail = exp.ToString() };
            }

            return exception;
        }

        public static AccessPolicy ParseAccessPolicy(JToken token)
        {
            AccessPolicy policy = new AccessPolicy();
            var policyID = token.Value<string>("Id");
            var Created = token.Value<DateTime>("Created");
            var LastModified = token.Value<DateTime>("LastModified");
            var pName = token.Value<string>("Name");
            var DurationInMinutes = token.Value<double>("DurationInMinutes");
            var Permissions = (AccessPolicyPermission)token.Value<int>("Permissions");

            policy.Id = policyID;
            policy.Created = Created;
            policy.LastModified = LastModified;
            policy.Name = pName;
            policy.DurationInMinutes = DurationInMinutes;
            policy.Permissions = Permissions;
            return policy;
        }

        public static string GetChildAssetName(string jobName, string assetID)
        {
            return string.Format("{0}-{1}-{2}-{3}", Literals.CHILD_ASSET_APPEND, assetID, jobName, DateTime.UtcNow.ToString("yyyy_MM_dd_hh_MM_ss_UTC"));
        }
    }
}
