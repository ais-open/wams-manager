using Ais.Internal.Dcm.Business;
using Ais.Internal.Dcm.Web;
using Ais.Internal.Dcm.Web.Models;
using Ais.Internal.Dcm.Web.Service;
using AzurePatterns.Entity;
using AzurePatterns.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;
using Ais.Internal.Dcm.Web.Filters;
using System.IO;


namespace Ais.Internal.Dcm.Web.Controllers
{
    public class MediaController : ApiController
    {
        ILoggerService logger = null;
        IUserMediaService mediaServingService = null;

      
        public MediaController(ILoggerService loggerService, IUserMediaService mediaServingService)
        {
            try
            {
                this.logger = loggerService;
                this.mediaServingService = mediaServingService;
            }
            catch (Exception exp)
            {
               logger.LogException("MediaController Constructor: "+exp.ToString(), exp);
                throw;
            }
        }

        [ActionName("MediaServices")]
        [BasicAuthentication]
        public IEnumerable<MediaServiceModel> GetAllMediaServices(string clientKey)
        {
            List<MediaServiceModel> mServices = null;
            try
            {
                mServices = mediaServingService.GetAllMediaServices(clientKey);
            }
            catch (Exception exp)
            {
                logger.LogException("GetAllMediaServices: "+exp.ToString(), exp);
                throw;
            }
            return mServices;
        }

        [ActionName("Assets")]
        [BasicAuthentication]
        public IEnumerable<AssetInfoModel> GetAllAssets(string mediaServiceName)
        {
            List<AssetInfoModel> assets = null;
            try
            {
                assets = mediaServingService.GetAllAssets(mediaServiceName);
                if (assets != null)
                {
                    foreach (var item in assets)
                    {
                        if (string.IsNullOrEmpty(item.DefaultThumbnailUrl))
                        {
                            try
                            {
                                var noPreviewUrl = System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/Content/img/nopreviewavailable.jpg";
                                item.DefaultThumbnailUrl = noPreviewUrl;
                            }
                            catch (Exception exp)
                            {
                                //swallow the expextion as it's not important;
                            }
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                logger.LogException("GetAllAssets " + exp.ToString(), exp);
                throw;
            }
            return assets;
        }

        [HttpPost]
        [ActionName("CreateAsset")]
        [BasicAuthentication]
        public AssetInfoModel CreateNewAsset(string mediaServiceName, string assetName)
        {
            AssetInfoModel assetInfo = null;
            try
            {
                assetInfo = mediaServingService.CreateNewAsset(mediaServiceName, assetName);
            }
            catch (Exception exp)
            {
                logger.LogException("CreateNewAsset " + exp.ToString(), exp);
                throw;
            }
            return assetInfo;
        }

        [ActionName("GetUploadSasUrl")]
        [BasicAuthentication]
        public string GetUploadSasUrl(string mediaServiceName, string assetId)
        {
            string uploadSasUrl = string.Empty;
            try
            {
                uploadSasUrl = mediaServingService.GetUploadSasUrl(mediaServiceName, assetId);
            }
            catch (Exception exp)
            {
                logger.LogException("GetUploadSasUrl " + exp.ToString(), exp);
                throw;
            }
            return uploadSasUrl;
        }

        [ActionName("IsAJobPending")]
        [HttpGet]
        [BasicAuthentication]
        public bool IsAJobPending(string mediaServiceName, string assetId)
        {
            bool IsPending = false;
            try
            {
                IsPending = mediaServingService.IsAJobPending(mediaServiceName, assetId);
            }
            catch (Exception exp)
            {
                logger.LogException("IsAJobPending " + exp.ToString(), exp);
                throw;
            }
            return IsPending;
        }

        [ActionName("Thumbnails")]
        [BasicAuthentication]
        public IEnumerable<ThumbnailAssetModel> GetThumbnailUrls(string assetId)
        {
            List<ThumbnailAssetModel> assetThumbnails = null;
            try
            {
                assetThumbnails = mediaServingService.GetThumbnailUrls(assetId);
            }
            catch (Exception exp)
            {
                logger.LogException("GetThumbnailUrls " + exp.ToString(), exp);
                throw;
            }
            return assetThumbnails;
        }

        [ActionName("EncodedOutputs")]
        [BasicAuthentication]
        public IEnumerable<AssetWithFilesModel> GetOutputAssetFiles(string mediaServiceName, string assetId)
        {
            List<AssetWithFilesModel> assetOutputFiles = null;
            try
            {
                assetOutputFiles = mediaServingService.GetOutputAssetFiles(mediaServiceName, assetId);
            }
            catch (Exception exp)
            {
                logger.LogException("GetOutputAssetFiles " + exp.ToString(), exp);
                throw;
            }
            return assetOutputFiles;
        }

        [ActionName("AssetFiles")]
        [BasicAuthentication]
        public IEnumerable<AssetFileInfo> GetAssetFiles(string mediaServiceName, string assetId)
        {
            List<AssetFileInfo> assetFiles = null;
            try
            {
                assetFiles = mediaServingService.GetAssetFiles(mediaServiceName, assetId);
            }
            catch (Exception exp)
            {
                logger.LogException("GetAssetFiles " + exp.ToString(), exp);
                throw;
            }
            return assetFiles;
        }

        [ActionName("AssetFilesContext")]
        [BasicAuthentication]
        public IEnumerable<AssetFileInfo> GetAssetFilesContext(string mediaServiceName, string assetId)
        {
            List<AssetFileInfo> assetFileInfos = null;
            try
            {
                assetFileInfos = mediaServingService.GetAssetFilesContext(mediaServiceName, assetId);
            }
            catch (Exception exp)
            {
                logger.LogException("GetAssetFilesContext " + exp.ToString(), exp);
                throw;
            }
            return assetFileInfos;
        }

        [ActionName("EncodingTypes")]
        [BasicAuthentication]
        public IEnumerable<EncodingTypeModel> GetEncodingTypes(string mediaServiceName)
        {
            var encodingModels = new List<EncodingTypeModel>();
            try
            {
                encodingModels = mediaServingService.GetEncodingTypes(mediaServiceName);
            }
            catch (Exception exp)
            {
                logger.LogException("GetEncodingTypes " + exp.ToString(), exp);
                throw;
            }
            return encodingModels;
        }

        [ActionName("GenFileMetadata")]
        [HttpGet]
        [BasicAuthentication]
        public bool GenerateFileMetaData(string mediaServiceName, string assetId)
        {
            bool isSuccess = false;

            try
            {
                isSuccess = mediaServingService.GenerateFileMetaData(mediaServiceName, assetId);
            }
            catch (Exception exp)
            {
                logger.LogException("GenFileMetadata " + exp.ToString(), exp);
                throw;
            }
            return isSuccess;
        }

        [ActionName("MakePrimary")]
        [HttpGet]
        [BasicAuthentication]
        public bool MakePrimaryFile(string mediaServiceName, string assetId, string assetFileId)
        {
            bool isSuccess = false;
            try
            {
                isSuccess = mediaServingService.MakePrimaryFile(mediaServiceName, assetId, assetFileId);
            }
            catch (Exception exp)
            {
                logger.LogException("MakePrimaryFile " + exp.ToString(), exp);
                throw;
            }
           return isSuccess;
        }

        [ActionName("InitiateJob")]
        [HttpGet]
        [BasicAuthentication]
        public string InitiateJob(string mediaServiceName, string assetId, string enStrings, string enFriendlyNames)
        {
            string jobId = string.Empty;
            try
            {
                jobId = mediaServingService.InitiateJob(mediaServiceName, assetId, enStrings, enFriendlyNames);
            }
            catch (Exception exp)
            {
                logger.LogException("InitiateJob " + exp.ToString(), exp);
                throw;
            }
            return jobId;
        }

        [ActionName("InitiateThumbnailJob")]
        [HttpGet]
        [BasicAuthentication]
        public string InitiateThumbnailJob(string mediaServiceName, string assetId, string enStrings)
        {
            string jobId = string.Empty;
            try
            {
                jobId = mediaServingService.InitiateThumbnailJob(mediaServiceName, assetId, enStrings);
            }
            catch (Exception exp)
            {
                logger.LogException("InitiateThumbnailJob " + exp.ToString(), exp);
                throw;
            }
            return jobId;
        }

        /// <summary>
        /// This method refreshes the status of un-committed data to Metadata
        /// </summary>
        [ActionName("Refresh")]
        [HttpGet]
        [BasicAuthentication]
        public void Refresh(string mediaServiceName)
        {
            try
            {
                mediaServingService.Sync(mediaServiceName);
            }
            catch (Exception exp)
            {
                logger.LogException("Refresh " + exp.ToString(), exp);
                throw;
            }
        }

        [ActionName("GetJobStatus")]
        [BasicAuthentication]
        public List<JobInfo> GetJobStatus(string assetId)
        {
            var jobs = new List<JobInfo>();
            try
            {
                jobs = mediaServingService.GetJobStatus(assetId);
            }
            catch (Exception exp)
            {
                logger.LogException("GetJobStatus " + exp.ToString(), exp);
                throw;
            }
            return jobs;
        }

        [ActionName("ResetUpdateRequired")]
        [HttpGet]
        [BasicAuthentication]
        public void SetUpdateRequiredTrue(string mediaServiceName)
        {
            try
            {
               mediaServingService.UpdateMediaService(mediaServiceName);
            }
            catch (Exception exp)
            {
                logger.LogException("GetJobStatus " + exp.ToString(), exp);
                throw;
            }
        }

        // read all asset metadata
        // read all output
        // read all thumbnails
        // create relation ship
        // return master data on which user perform search operations
        /// <summary>
        /// Intializes Search Data for users to perform search
        /// </summary>
        [ActionName("InitializeSearch")]
        [HttpGet]
        [BasicAuthentication]
        public List<SearchResultViewModel> InitializeSearch()
        {
            // read all asset metadata
            List<AssetInfoModel> assets = null;
            List<AssetWithFilesModel> outputs = null;
            List<AssetFileInfo> files = null;
            List<SearchResultViewModel> results = new List<SearchResultViewModel>();
            Dictionary<string, List<SearchResultViewModel>> refinedResults = new Dictionary<string, List<SearchResultViewModel>>();
            try
            {
                assets = mediaServingService.GetAllAssets();
                outputs = mediaServingService.GetOutputAssetFiles();
                files = mediaServingService.GetAllAssetFiles();
                foreach (var item in files)
                {
                    string key = item.ParentAssetId;
                    if (!refinedResults.ContainsKey(key))
                    {
                        var list = GetFilesOfAsset(key, assets, outputs, files);
                        refinedResults[key] = list;
                    }
                }
                foreach (var item in refinedResults.Values)
                {
                    results = results.Concat(item).ToList();
                }
                return results;

            }
            catch (Exception exp)
            {
                logger.LogException("GetAllAssets " + exp.ToString(), exp);
                throw;
            }
        }

        private List<SearchResultViewModel> GetFilesOfAsset(string assetId, List<AssetInfoModel> assets, List<AssetWithFilesModel> outputs, List<AssetFileInfo> files)
        {
            
            var list = new List<SearchResultViewModel> ();
            var fileQuery = from file in files
                    where file.ParentAssetId == assetId
                    select file;
            var parentAsset = (from asset in assets
                              where asset.Id == assetId
                              select asset).FirstOrDefault();

            var outputQuery = from output in outputs
                              where output.ParentAssetId == assetId
                              select output;
            if (parentAsset != null)
            {
                foreach (var item in fileQuery)
                {
                    var model = new SearchResultViewModel
                    {
                        FileName = item.Name,
                        ParentAssetId = parentAsset.Id,
                        AlbumName = parentAsset.Name,
                        MediaServiceName = parentAsset.MediaServiceName,
                        CollectionName = parentAsset.MediaServiceFriendlyName,
                        Tags = new List<Tag>(),
                        Outputs = new List<VideoOutput>(),
                        NameForSearch = GetSeachableText(parentAsset, item),
                        TagsForSearch = string.IsNullOrEmpty(item.Tags) ? "" : item.Tags.ToLower(), 
                        DefaultThumbnailUrl = item.DefaultThumbnailUrl
                    };

                    if (string.IsNullOrEmpty(item.DefaultThumbnailUrl))
                    {
                        try
                        {
                            var noPreviewUrl = System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/Content/img/nopreviewavailable.jpg";
                            item.DefaultThumbnailUrl = noPreviewUrl;
                        }
                        catch (Exception exp)
                        {
                            //swallow the expextion as it's not important;
                        }
                    }
                    if (!string.IsNullOrEmpty(item.Tags)) 
                    {
                        foreach (var tag in item.Tags.Split(new char[] { ',' },StringSplitOptions.RemoveEmptyEntries))
                        {
                            model.Tags.Add(new Tag { Name = tag.Trim() });
                        }
                    }

                    foreach (var output in outputQuery)
                    {
                        if (output.URL.Contains("/" + Path.GetFileNameWithoutExtension(model.FileName) + "."))
                        {
                            model.Outputs.Add(new VideoOutput { EncodingName = (output.FriendlyName), Url = output.URL });
                        }
                    }
                    list.Add(model);
                }
            }
            return list;
        }

        private static string GetSeachableText(AssetInfoModel parentAsset, AssetFileInfo item)
        {
            // check null and append
            return ((parentAsset.MediaServiceFriendlyName == null ? "" : parentAsset.MediaServiceFriendlyName) +
                (item.Name == null ? "" : item.Name) +
                (parentAsset.Name == null ? "" : parentAsset.Name) +
                (parentAsset.MediaServiceName == null ? "" : parentAsset.MediaServiceName)+
                (string.IsNullOrEmpty(item.Tags) ? "" : item.Tags)).ToLower();
        }

        [ActionName("GetGroupedOutput")]
        [HttpGet]
        [BasicAuthentication]
        public List<GroupedOutputViewModel> GetGroupedOutput(string mediaServiceName, string assetId)
        {
            // read all asset metadata
            List<AssetWithFilesModel> outputs = null;
            List<ThumbnailAssetModel> thumbnails = null;
            List<AssetFileInfo> files = null;
            List<Tag> tags = null;
            List<GroupedOutputViewModel> results = new List<GroupedOutputViewModel>();
            try
            {
                files = mediaServingService.GetAssetFiles(mediaServiceName, assetId);
                thumbnails = mediaServingService.GetThumbnailUrls(assetId);
                outputs = mediaServingService.GetOutputAssetFiles(mediaServiceName, assetId);

                results = GroupFileAndOutput(assetId, outputs, files, thumbnails, tags);

            }
            catch (Exception exp)
            {
                logger.LogException("GetGroupedOutput " + exp.ToString(), exp);
                throw;
            }
            return results;
        }

        private List<GroupedOutputViewModel> GroupFileAndOutput(string assetId,  List<AssetWithFilesModel> outputs, List<AssetFileInfo> files,List<ThumbnailAssetModel> thumbnails, List<Tag> tags)
        {
            var list = new List<GroupedOutputViewModel>();
            
          
                foreach (var item in files)
                {
                    var model = new GroupedOutputViewModel
                    {
                        FileName = item.Name,
                        //Tags = tags,
                        Outputs = new List<VideoOutput>(),
                        Thumbnails = new List<string>(),
                        DefaultThumbnailUrl = item.DefaultThumbnailUrl
                    };

                    model.Tags = mediaServingService.GetAssetFileTags(item.Id);

                    foreach (var output in outputs)
                    {
                        if (output.URL.Contains("/" + Path.GetFileNameWithoutExtension(model.FileName) + "."))
                        {
                            model.Outputs.Add(new VideoOutput { EncodingName = output.Name, Url = output.URL });
                        }
                    }

                    foreach (var thumbnail in thumbnails)
                    {
                        if (thumbnail.URL.Contains("/" + Path.GetFileNameWithoutExtension(model.FileName) ))
                        {
                            model.Thumbnails.Add(thumbnail.URL);
                        }
                    }
                    list.Add(model);
                
            }
            return list;
        }

        [ActionName("GetTags")]
        [BasicAuthentication]
        public IEnumerable<Tag> GetTags(string mediaServiceName)
        {
            var tags = new List<Tag>();
            try
            {
                tags = mediaServingService.GetTags(mediaServiceName);
            }
            catch (Exception exp)
            {
                logger.LogException("GetTags " + exp.ToString(), exp);
                throw;
            }
            return tags;
        }

        [ActionName("UpdateAssetTag")]
        [HttpGet]
        [BasicAuthentication]
        public void CreateOrUpdateAssetWithTag(string mediaServiceName, string assetId, string tagsUpdateString)
        {
            try
            {
                Tag newTag = null;
                var tags = GetTags(mediaServiceName);
                string tagString = string.Empty;
                //var tagsUpdate = tagsUpdateString.Split(',').ToList();
                var tagsUpdate = !string.IsNullOrWhiteSpace(tagsUpdateString) ? tagsUpdateString.Split(',').ToList() : new List<string>();
                foreach (var tag in tagsUpdate)
                {
                    if (tags.Count() == 0 || tags.FirstOrDefault(t => System.String.CompareOrdinal(t.Name, tag) == 0) == null)
                    {
                        newTag = CreateTag(mediaServiceName, tag);
                        tagString += newTag.Name + ",";
                    }
                    else
                    {
                        tagString += tag + ",";
                    }
                }
                if (tagString.LastIndexOf(",", System.StringComparison.Ordinal) > 0)
                    tagString = tagString.Remove(tagString.LastIndexOf(",", System.StringComparison.Ordinal));
                mediaServingService.UpdateAssetWithTags(mediaServiceName, assetId, tagString);
            }
            catch (Exception exception)
            {
                logger.LogException("Problem with updating tags to Asset",exception);
                throw;
            }
        }

        [ActionName("UpdateAssetFileTag")]
        [HttpGet]
        [BasicAuthentication]
        public void CreateOrUpdateAssetFileWithTag(string mediaServiceName, string assetId, string assetFileId,
                                                   string tagsUpdateString)
        {
            try
            {
                Tag newTag = null;
                var tags = GetTags(mediaServiceName);
                string tagString = string.Empty;
                var tagsUpdate = !string.IsNullOrWhiteSpace(tagsUpdateString) ? tagsUpdateString.Split(',').ToList() : new List<string>();
                foreach (var tag in tagsUpdate)
                {
                    if (tags.Count() == 0 ||
                        tags.FirstOrDefault(t => System.String.CompareOrdinal(t.Name, tag) == 0) == null)
                    {
                        newTag = CreateTag(mediaServiceName, tag);
                        tagString += newTag.Name + ",";
                    }
                    else
                    {
                        tagString += tag + ",";
                    }
                }
                if (tagString.LastIndexOf(",", System.StringComparison.Ordinal) > 0)
                    tagString = tagString.Remove(tagString.LastIndexOf(",", System.StringComparison.Ordinal));
                mediaServingService.UpdateAssetFileWithTags(mediaServiceName, assetId, assetFileId, tagString);
            }
            catch (Exception exception)
            {
                logger.LogException("Problem with updating tags to Asset", exception);
                throw;
            }
        }

        [ActionName("SearchMedia")]
        [HttpGet]
        [BasicAuthentication]
        public SearchData SearchMedia(string searchString, int rowsToSkip, int rowsToRetrieve,int searchType)
        {
            var searchData = new SearchData();
            try
            {
                searchData = mediaServingService.SearchMedia(searchString, rowsToSkip, rowsToRetrieve,searchType);
            }
            catch (Exception exception)
            {
                logger.LogException("Problem with updating tags to Asset", exception);
                throw;
            }
            return searchData;
        }

        private Tag CreateTag(string mediaServiceName, string tagName)
        {
            Tag tag = null;
            try
            {
                tag = mediaServingService.CreateTag(mediaServiceName, tagName);
            }
            catch (Exception exp)
            {
                logger.LogException("CreateNewTag " + exp.ToString(), exp);
                throw;
            }
            return tag;
        }
    }
}
