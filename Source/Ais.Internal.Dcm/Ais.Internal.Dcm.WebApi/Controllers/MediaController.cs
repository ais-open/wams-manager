using Ais.Internal.Dcm.Business;
using Ais.Internal.Dcm.Web;
using Ais.Internal.Dcm.Web.Models;
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


namespace Ais.Internal.Dcm.Web.Controllers
{
    public class MediaController : ApiController
    {

        private MediaServiceContext mediaServiceContext;
        private string currentmediaServiceName;
        string seperator = "B4283131-431C-409C-A649-D81262954B00";
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public MediaController()
        {
            try
            {
                System.Configuration.AppSettingsReader reader = new System.Configuration.AppSettingsReader();
                this.accountName = (string)reader.GetValue("MetadataStorageAccountName", typeof(string));
                this.accountKey = (string)reader.GetValue("MetadataStorageKey", typeof(string));
                var storageCredentials = new StorageCredentials(accountName, accountKey);
                this.storageAccount = new CloudStorageAccount(storageCredentials, true);
            }
            catch (Exception exp)
            {
               logger.LogException(NLog.LogLevel.Error, "MediaController Constructor: "+exp.ToString(), exp);
                throw;
            }
        }

        private MediaServiceRepository mediaServiceRepository;
        private readonly string accountName;
        private readonly string accountKey;
        private readonly CloudStorageAccount storageAccount;

        [ActionName("MediaServices")]
        public IEnumerable<MediaServiceInfo> GetAllMediaServices(string clientKey)
        {
            List<MediaServiceInfo> mServices = null;
            try
            {
                mediaServiceRepository = new MediaServiceRepository(storageAccount);
                clientKey = "";//no filter
                string filterExpression = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual,
                                                   clientKey);
                var mediaServices = mediaServiceRepository.Find(filterExpression);
                if (mediaServices == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
                mServices = new List<MediaServiceInfo>();
                foreach (var mediaServiceEntity in mediaServices)
                {
                    var mservice = new MediaServiceInfo
                        {
                            FriendlyName = mediaServiceEntity.FriendlyName,
                            AccountName = mediaServiceEntity.RowKey,
                            ClientKey = mediaServiceEntity.PartitionKey,
                            PrimaryAccountKey = mediaServiceEntity.AccessKey
                        };
                    mServices.Add(mservice);
                }
            }
            catch (Exception exp)
            {
                logger.LogException(NLog.LogLevel.Error, "GetAllMediaServices: "+exp.ToString(), exp);
                throw;
            }
            return mServices;
        }

        [ActionName("Assets")]
        public IEnumerable<AssetInfo> GetAllAssets(string mediaServiceName)
        {
            List<AssetInfo> assets = null;
            try
            {
                var assetRepository = new AssetRepository(storageAccount);
                var assetEntities = assetRepository.Find(string.Format("PartitionKey eq '{0}'", mediaServiceName));

                if (assetEntities == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
                assets = new List<AssetInfo>();
                foreach (var assetEntity in assetEntities)
                {
                    var assetInfo = new AssetInfo()
                        {
                            Id = assetEntity.RowKey,
                            MediaServiceName = assetEntity.PartitionKey,
                            DefaultThumbnailUrl = assetEntity.DefaultThumbnailUrl,
                            Name = assetEntity.Name
                        };
                    assets.Add(assetInfo);
                }
            }
            catch (Exception exp)
            {
                logger.LogException(NLog.LogLevel.Error, "GetAllAssets "+exp.ToString(), exp);
                throw;
            }
            return assets;
        }

        [HttpPost]
        [ActionName("CreateAsset")]
        public AssetInfo CreateNewAsset(string mediaServiceName, string assetName)
        {
            AssetInfo assetInfo = null;
            try
            {

                mediaServiceContext = GetMediaServiceContext(mediaServiceName);
                var asset = mediaServiceContext.CreateNewAsset(assetName);
                var assetEntity = new AssetEntity
                    {
                        Id = asset.Id,
                        Name = asset.Name,
                        MediaServiceName = mediaServiceName,
                        PartitionKey = mediaServiceName,
                        RowKey = asset.Id
                    };
                var assetRepository = new AssetRepository(storageAccount);
                assetRepository.Insert(assetEntity);
                if (asset == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
                assetInfo = new AssetInfo()
                {
                    Id = assetEntity.RowKey,
                    MediaServiceName = assetEntity.PartitionKey,
                    DefaultThumbnailUrl = assetEntity.DefaultThumbnailUrl,
                    Name = assetEntity.Name
                };
            }
            catch (Exception exp)
            {
                logger.LogException(NLog.LogLevel.Error, "CreateNewAsset " + exp.ToString(), exp);
                throw;
            }
            return assetInfo;
        }

        [ActionName("GetUploadSasUrl")]
        public string GetUploadSasUrl(string mediaServiceName, string assetId)
        {
            string uploadSasUrl = string.Empty;
            try
            {
                mediaServiceContext = GetMediaServiceContext(mediaServiceName);
                Asset asset = mediaServiceContext.GetAssetById(assetId);
                uploadSasUrl = asset.GetSasUrlForUpload(assetId);
            }
            catch (Exception exp)
            {
                logger.LogException(NLog.LogLevel.Error, "GetUploadSasUrl " + exp.ToString(), exp);
                throw;
            }
            return uploadSasUrl;
        }

        [ActionName("IsAJobPending")]
        [HttpGet]
        public bool IsAJobPending(string mediaServiceName, string assetId)
        {
            bool IsPending = false;
            try
            {
                UnCommittedDataRepository repository = new UnCommittedDataRepository(storageAccount);
                string filterExpression = TableQuery.GenerateFilterCondition("AssetID", QueryComparisons.Equal,
                                                    assetId);
                var jobs = repository.Find(filterExpression);
                if (jobs != null)
                {
                    var query = from s in jobs
                                where s.UpdateRequired == true
                                select s;
                    if (query != null && query.Count<UnCommittedDataEntity>() > 0)
                    {
                        IsPending = true;
                    }

                }
            }
            catch (Exception exp)
            {
                logger.LogException(NLog.LogLevel.Error, "IsAJobPending " + exp.ToString(), exp);
                throw;
            }
            return IsPending;
        }

        [ActionName("Thumbnails")]
        public IEnumerable<ThumbnailModel> GetThumbnailUrls(string assetId)
        {
            List<ThumbnailModel> assetThumbnails = null;
            try
            {
                var thumbnailAssetRepository = new AssetThumbnailRepository(storageAccount);
                var thumnailEntities = thumbnailAssetRepository.Find(string.Format("PartitionKey eq '{0}'", assetId));

                if (thumnailEntities == null) throw new HttpResponseException(HttpStatusCode.NotFound);
                assetThumbnails = new List<ThumbnailModel>();
                foreach (var thumbnailEntity in thumnailEntities)
                {
                    var assetWithThumbnail = new ThumbnailModel()
                    {
                        AssetId = thumbnailEntity.PartitionKey,
                        AssetFileId = thumbnailEntity.AssetFileId,
                        ThumbnailFileId = thumbnailEntity.RowKey,
                        Name = thumbnailEntity.FriendlyName,
                        URL = thumbnailEntity.DownloadStreamingUrl,
                        CreatedDate = thumbnailEntity.Timestamp.Date
                    };
                    assetThumbnails.Add(assetWithThumbnail);
                }
            }
            catch (Exception exp)
            {
                logger.LogException(NLog.LogLevel.Error, "GetThumbnailUrls " + exp.ToString(), exp);
                throw;
            }
            return assetThumbnails;
        }

        [ActionName("EncodedOutputs")]
        public IEnumerable<AssetWithFiles> GetOutputAssetFiles(string mediaServiceName, string assetId)
        {
            List<AssetWithFiles> assetOutputFiles = null;
            try
            {
                var outputAssetRepository = new AssetOutputRepository(storageAccount);
                var outputEntities = outputAssetRepository.Find(string.Format("PartitionKey eq '{0}'", assetId));

                if (outputEntities == null) throw new HttpResponseException(HttpStatusCode.NotFound);
                assetOutputFiles = new List<AssetWithFiles>();
                foreach (var assetOutputEntity in outputEntities)
                {
                    var assetWithOutput = new AssetWithFiles()
                        {
                            Name = assetOutputEntity.FriendlyName,
                            URL = assetOutputEntity.DownloadStreamingUrl,
                            CreatedDate = assetOutputEntity.Timestamp.DateTime
                        };
                    assetOutputFiles.Add(assetWithOutput);
                }
            }
            catch (Exception exp)
            {
                logger.LogException(NLog.LogLevel.Error, "GetOutputAssetFiles " + exp.ToString(), exp);
                throw;
            }
            return assetOutputFiles;
        }

        [ActionName("AssetFiles")]
        public IEnumerable<AssetFileInfo> GetAssetFiles(string mediaServiceName, string assetId)
        {
            List<AssetFileInfo> assetFiles = null;
            try
            {
                var assetFileRepository = new AssetFileRepository(storageAccount);
                var assetFileEntities = assetFileRepository.Find(string.Format("PartitionKey eq '{0}'", assetId));

                if (assetFileEntities == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
                assetFiles = new List<AssetFileInfo>();
                foreach (var assetFileEntity in assetFileEntities)
                {
                    var assetFileInfo = new AssetFileInfo()
                    {
                        Id = assetFileEntity.RowKey,
                        ContentFileSize = assetFileEntity.ContentFileSize,
                        DefaultThumbnailUrl = assetFileEntity.DefaultThumnailUrl,
                        IsPrimary = assetFileEntity.IsPrimary,
                        MimeType = assetFileEntity.MimeType,
                        Name = assetFileEntity.Name,
                        ParentAssetId = assetFileEntity.ParentAssetId,
                        //Created = assetFileEntity.Created, LastModified = assetFileEntity.LastModified
                    };
                    assetFiles.Add(assetFileInfo);
                }
            }
            catch (Exception exp)
            {
                logger.LogException(NLog.LogLevel.Error, "GetAssetFiles " + exp.ToString(), exp);
                throw;
            }
            return assetFiles;
        }

        [ActionName("AssetFilesContext")]
        public IEnumerable<AssetFileInfo> GetAssetFilesContext(string mediaServiceName, string assetId)
        {
            List<AssetFileInfo> assetFileInfos = null;
            try
            {
                mediaServiceContext = GetMediaServiceContext(mediaServiceName);
                Asset asset = mediaServiceContext.GetAssetById(assetId);
                var assetFiles = asset.ListAssetFiles();
                if (assetFiles == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
                assetFileInfos = new List<AssetFileInfo>();
                foreach (var assetFile in assetFiles)
                {
                    var astFileInfo = new AssetFileInfo()
                        {
                            Id = assetFile.Id,
                            ContentFileSize = assetFile.ContentFileSize,
                            Name = assetFile.Name,
                            Created = assetFile.Created,
                            IsPrimary = assetFile.IsPrimary,
                            LastModified = assetFile.LastModified,
                            MimeType = assetFile.MimeType,
                            ParentAssetId = assetFile.ParentAssetId
                        };
                    assetFileInfos.Add(astFileInfo);
                }
            }
            catch (Exception exp)
            {
                logger.LogException(NLog.LogLevel.Error, "GetAssetFilesContext " + exp.ToString(), exp);
                throw;
            }
            return assetFileInfos;
        }

        [ActionName("EncodingTypes")]
        public IEnumerable<EncodingTypeModel> GetEncodingTypes(string mediaServiceName)
        {
            var list = new List<EncodingTypeModel>();
            try
            {
                EncodingTypeRepository repository = new EncodingTypeRepository(storageAccount);
                string filterExpression = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual,
                                                  "");
                var encodingTypes = repository.Find(filterExpression);
                foreach (var encodingType in encodingTypes)
                {
                    list.Add(new EncodingTypeModel
                    {
                        FriendlyName = encodingType.FriendlyName,
                        TechnicalName = encodingType.RowKey
                    });
                }
                if (list == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
            }
            catch (Exception exp)
            {
                logger.LogException(NLog.LogLevel.Error, "GetEncodingTypes " + exp.ToString(), exp);
                throw;
            }
            return list;
            
        }

        [ActionName("GenFileMetadata")]
        [HttpGet]
        public bool GenerateFileMetaData(string mediaServiceName, string assetId)
        {
            bool isSuccess = false;

            try
            {
                mediaServiceContext = GetMediaServiceContext(mediaServiceName);
                Asset asset = mediaServiceContext.GetAssetById(assetId);
                isSuccess = asset.GenerateFileMetadata(assetId);
            }
            catch (Exception exp)
            {
                logger.LogException(NLog.LogLevel.Error, "GenFileMetadata " + exp.ToString(), exp);
                throw;
            }
            return isSuccess;
        }

        [ActionName("MakePrimary")]
        [HttpGet]
        public bool MakePrimaryFile(string mediaServiceName, string assetId, string assetFileId)
        {
            bool isSuccess = false;
            try
            {
                mediaServiceContext = GetMediaServiceContext(mediaServiceName);
                Asset asset = mediaServiceContext.GetAssetById(assetId);
                var files = asset.ListAssetFiles();
                var primaryFile = files.FirstOrDefault(file => file.IsPrimary);
                if (primaryFile != null)
                {
                    asset.MakePrimaryFile(primaryFile.Id, false);
                    UpdateAssetFileMetaData(assetId, primaryFile, false);
                }
                var latestFile = files.FirstOrDefault(file => file.Id == assetFileId);
                InsertAssetFileMetaData(assetId, latestFile);
                isSuccess = asset.MakePrimaryFile(assetFileId, true);
                if (isSuccess)
                {
                    // generate thumbnail as well
                    Job thumbnailJob = mediaServiceContext.CreateThumbnailJob(assetId);
                    string additionalInformation = AppendAssetIDAndFile(assetId, latestFile.Id, latestFile.Name);
                    QueueUnCommittedJob(thumbnailJob.Id, UnCommittedEntityType.ThumbnailJob, assetId, additionalInformation);
                }
            }
            catch (Exception exp)
            {
                logger.LogException(NLog.LogLevel.Error, "MakePrimaryFile " + exp.ToString(), exp);
                throw;
            }
           return isSuccess;
        }

        private string AppendAssetIDAndFile(string assetId, string assetFileId, string fileName)
        {

            return string.Format("{0}{3}{1}{3}{2}", assetId, assetFileId, fileName, seperator);
        }

        [ActionName("InitiateJob")]
        [HttpGet]
        public string InitiateJob(string mediaServiceName, string assetId, string enStrings)
        {
            string jobID = string.Empty;
            try
            {
                string[] encodingStrings = enStrings.Split(',');

                mediaServiceContext = GetMediaServiceContext(mediaServiceName);
                Asset asset = mediaServiceContext.GetAssetById(assetId);
                var taskList = new List<TaskStep>();
                int outputOrder = 0;
                for (int i = 0; i < encodingStrings.Length; i++, outputOrder++)
                {

                    string appleEncodingFormat = "Apple HLS Format";
                    if (String.CompareOrdinal(encodingStrings[i], appleEncodingFormat) == 0)
                    {
                        var appleTasks = CreateAppleFormatTask(asset, appleEncodingFormat, outputOrder);
                        taskList.AddRange(appleTasks);
                        outputOrder += 2;
                    }
                    else
                    {
                        string encodingTaskPart = Literals.ENCODING + "_" + encodingStrings[i];
                        // 
                        string outputAssetName = string.Format("{0}-{1}-{2}-{3}", Literals.CHILD_ASSET_APPEND, asset.Id,
                                                               encodingTaskPart,
                                                               DateTime.UtcNow.ToString("yyyy_MM_dd_hh_MM_ss_UTC"));
                        TaskStep taskStep = new TaskStep
                            {
                                Configuration = encodingStrings[i],
                                InputAssetName = asset.Name,
                                MediaProcessorID = MediaServiceConstants.MediaProcessorId,
                                Order = (short)outputOrder,
                                OutputAssetName = outputAssetName
                            };
                        taskList.Add(taskStep);
                    }
                }

                string jobName = string.Format("JOB_{0}_{1}", asset.Id, DateTime.UtcNow.ToString("yyyy_MM_dd_hh_MM_ss_UTC"));
                var jobInfo = new MediaServiceJobInfo(jobName);
                jobInfo.InputAssetIds = new List<string> { asset.Id };
                jobInfo.Tasks = taskList;
                string jobString = jobInfo.CreateJsonString();
                Job job = asset.CreateEncodingJobV2(asset.Id, jobString, "");
                if (job != null)
                {
                    // Queue a record for uncommitted data
                    QueueUnCommittedJob(job.Id, UnCommittedEntityType.Job, asset.Id, asset.Id);
                    jobID = job.Id;
                }
            }
            catch (Exception exp)
            {
                logger.LogException(NLog.LogLevel.Error, "InitiateJob " + exp.ToString(), exp);
                throw;
            }
            return jobID;
          
        }

        private List<TaskStep> CreateAppleFormatTask(Asset asset, string appleFormat, int outputOrder)
        {
            var appleTaskList = new List<TaskStep>();
            string h264Encoding = "H264 Broadband 720p";
            string h264SmoothEncoding = "H264 Smooth Streaming 720p";
            string h264IosEncoding = "H264 Adaptive Bitrate MP4 Set 720p for iOS Cellular Only";

            string encodingTaskPart = Literals.ENCODING + "_" + appleFormat;
            string outputAssetName1 = string.Format("{0}-{1}-{2}-{3}_1", "TEMP-ASST", asset.Id, h264Encoding, DateTime.UtcNow.ToString("yyyy_MM_dd_hh_MM_ss_UTC"));
            string outputAssetName2 = string.Format("{0}-{1}-{2}-{3}_2", "TEMP-ASST", asset.Id, h264SmoothEncoding, DateTime.UtcNow.ToString("yyyy_MM_dd_hh_MM_ss_UTC")); ;
            string outputAssetName = string.Format("{0}-{1}-{2}-{3}", Literals.CHILD_ASSET_APPEND, asset.Id, encodingTaskPart, DateTime.UtcNow.ToString("yyyy_MM_dd_hh_MM_ss_UTC"));

            TaskStep taskStep = new TaskStep
            {
                Configuration = h264Encoding,
                InputAssetName = asset.Name,
                MediaProcessorID = MediaServiceConstants.MediaProcessorId,
                Order = (short)outputOrder,
                OutputAssetName = outputAssetName1
            };
            appleTaskList.Add(taskStep);

            taskStep = new TaskStep
            {
                Configuration = h264SmoothEncoding,
                InputAssetName = outputAssetName1,
                MediaProcessorID = MediaServiceConstants.MediaProcessorId,
                Order = (short)(outputOrder + 1),
                OutputAssetName = outputAssetName2
            };
            appleTaskList.Add(taskStep);

            taskStep = new TaskStep
            {
                Configuration = h264IosEncoding,
                InputAssetName = outputAssetName2,
                MediaProcessorID = MediaServiceConstants.MediaProcessorId,
                Order = (short)(outputOrder + 2),
                OutputAssetName = outputAssetName
            };
            appleTaskList.Add(taskStep);
            return appleTaskList;
        }

        [ActionName("InitiateThumbnailJob")]
        [HttpGet]
        public string InitiateThumbnailJob(string mediaServiceName, string assetId, string enStrings)
        {
            string jobId = string.Empty;
            try
            {
                string[] encodingStrings = enStrings.Split(',');
                mediaServiceContext = GetMediaServiceContext(mediaServiceName);
                Asset asset = mediaServiceContext.GetAssetById(assetId);

                int imgQuality = 100;//best
                if (!string.IsNullOrWhiteSpace(encodingStrings[0]))
                    imgQuality = GetImageQuality(encodingStrings[0]);

                int imgHeight = 0;
                Int32.TryParse(encodingStrings[1], out imgHeight);

                int imgWidth = 0; Int32.TryParse(encodingStrings[2], out imgWidth);

                int maxNumOfThumb = 0; Int32.TryParse(encodingStrings[3], out maxNumOfThumb);
                int imgDurationInSec = 0; Int32.TryParse(encodingStrings[4], out imgDurationInSec);
                int stopAfter = maxNumOfThumb * imgDurationInSec;
                Job job = asset.CreateThumbnailJob(imgQuality, imgHeight, imgWidth, imgDurationInSec, stopAfter);
                if (job != null)
                {
                    var files = asset.ListAssetFiles();
                    var latestFile = files.FirstOrDefault(file => file.IsPrimary);
                    if (latestFile != null)
                    {
                        string additionalInformation = AppendAssetIDAndFile(assetId, latestFile.Id, latestFile.Name);
                        // Queue a record for uncommitted data
                        QueueUnCommittedJob(job.Id, UnCommittedEntityType.ThumbnailJob, assetId,
                                            additionalInformation);
                    }
                    jobId = job.Id;

                }
            }
            catch (Exception exp)
            {

                logger.LogException(NLog.LogLevel.Error, "InitiateThumbnailJob " + exp.ToString(), exp);
                throw;
            }
            return jobId;
        }

        private int GetImageQuality(string p)
        {
            int quality = 100;
            switch (p.ToLower())
            {
                case "high":
                    quality = 100;
                    break;
                case "medium":
                    quality = 60;
                    break;
                case "low":
                    quality = 30;
                    break;
                default:
                    break;
            }
            return quality;
        }

        private void UpdateAssetFileMetaData(string assetId, AssetFile primaryFile, bool isPrimary)
        {
            var assetFileRepository = new AssetFileRepository(storageAccount);
            var assetFileEntities = assetFileRepository.Find(string.Format("PartitionKey eq '{0}'", assetId));
            var assetFileEntity = assetFileEntities.FirstOrDefault(file => file.Id == primaryFile.Id);
            if (assetFileEntity != null)
            {
                assetFileEntity.IsPrimary = isPrimary;

                assetFileRepository.Replace(assetFileEntity);
            }
        }

        private void InsertAssetFileMetaData(string assetId, AssetFile assetFile)
        {
            var assetFileEntity = new AssetFileEntity
                {
                    Id = assetFile.Id,
                    ParentAssetId = assetId,
                    Name = assetFile.Name,
                    ContentFileSize = assetFile.ContentFileSize,
                    MimeType = assetFile.MimeType,
                    IsPrimary = assetFile.IsPrimary,
                    PartitionKey = assetId,
                    RowKey = assetFile.Id
                };

            var assetFileRepository = new AssetFileRepository(storageAccount);
            var existsFile = assetFileRepository.Find(string.Format("RowKey eq '{0}'", assetFile.Id));
            if (existsFile != null && existsFile.Count() > 0)
                assetFileRepository.Replace(assetFileEntity);
            else
                assetFileRepository.Insert(assetFileEntity);
        }

        private MediaServiceContext GetMediaServiceContext(string mediaServiceName)
        {
            mediaServiceRepository = new MediaServiceRepository(storageAccount);
            var rowKey = mediaServiceName;
            string filterExpression = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal,
                                               rowKey);
            MediaServiceContext context = null;
            var mediaService = mediaServiceRepository.Find(filterExpression);
            foreach (var mediaServiceEntity in mediaService)
            {
                context = new MediaServiceContext(mediaServiceEntity.RowKey, mediaServiceEntity.AccessKey);
            }
            return context;
        }

        /// <summary>
        /// This method refreshes the status of un-committed data to Metadata
        /// </summary>
        [ActionName("Refresh")]
        [HttpGet]
        public void Refresh(string mediaServiceName)
        {
            try
            {
                this.currentmediaServiceName = mediaServiceName;
                Task.Run(() =>
                    {
                        UnCommittedDataRepository repository = new UnCommittedDataRepository(storageAccount);
                        string filterExpression = TableQuery.GenerateFilterConditionForBool("UpdateRequired",
                                                                                            QueryComparisons.Equal,
                                                                                            true);
                        var uncommittedrecords = repository.Find(filterExpression);
                        foreach (var item in uncommittedrecords)
                        {
                            Refresh(item);
                        }
                    });
            }
            catch (Exception exp)
            {
                logger.LogException(NLog.LogLevel.Error, "Refresh " + exp.ToString(), exp);
                throw;
            }
        }

        private void Refresh(UnCommittedDataEntity item)
        {
            switch (item.MediaServiceEntityType)
            {
                case UnCommittedEntityType.Job:
                    UpdateJobStatus(item);
                    break;
                case UnCommittedEntityType.ThumbnailJob:
                    UpdateThumbnailJobFileStatus(item);
                    break;
                default:
                    break;
            }
        }

        private void UpdateThumbnailJobFileStatus(UnCommittedDataEntity entity)
        {
            string assetID = string.Empty;
            string assetFileID = string.Empty;
            string fileName = string.Empty;
            string selectedUrl = string.Empty;
            var mediaContext = GetMediaServiceContext(this.currentmediaServiceName);
            Job job = new Job(mediaContext);
            JobState state = job.GetJobStatus(entity.EntityID);
            entity.Status = state.ToString();
            entity.LastUpdated = DateTime.UtcNow;
            if (state == JobState.Error || state == JobState.Finished)
            {
                entity.UpdateRequired = false;
                if (state == JobState.Finished)
                { 
                    // split asset ID and asset File ID
                    string[] splittedString = entity.AdditionalInfo.Split(new string[] { seperator }, StringSplitOptions.RemoveEmptyEntries);
                    if (splittedString.Length == 3)
                    {
                        assetID = splittedString[0];
                        assetFileID = splittedString[1];
                        fileName = splittedString[2];

                        // get one thumbnail from Thumbnail asset
                        List<string> urls = mediaContext.GetLatestThumbnailUrl(assetID, fileName);
                        if (urls != null && urls.Count > 0)
                        {
                            Random r = new Random();
                            selectedUrl = urls[r.Next(0, urls.Count - 1)];
                        }
                        // update File Thumbnail URL
                        UpdateFileThumbnailUrl(assetID, assetFileID, selectedUrl);
                        // update Asset Thumbnail URL
                        UpdateAssetThumbnailUrl(assetID, selectedUrl, mediaContext);

                        List<ThumbnailModel> thumbnailModels = mediaContext.GetThumbnailModels(assetID);
                        UpdateThumbnailEntity(thumbnailModels);
                    }
                }
            }
            UnCommittedDataRepository repository = new UnCommittedDataRepository(storageAccount);
            repository.InsertOrReplace(entity);
        }

        private void UpdateThumbnailEntity(List<ThumbnailModel> thumbnails)
        {
            AssetThumbnailRepository repository = new AssetThumbnailRepository(storageAccount);
            foreach (var thumbnail in thumbnails)
            {
                repository.InsertOrMerge(new AssetThumbnailEntity(thumbnail.AssetId, thumbnail.ThumbnailFileId, thumbnail.AssetFileId, thumbnail.Name, thumbnail.URL));
            }
        }

        private void UpdateFileThumbnailUrl(string assetID, string assetFileID, string selectedUrl)
        {
            AssetFileRepository repository = new AssetFileRepository(storageAccount);
            string filterExpression = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal,
                                     assetFileID);
            var fileRecords = repository.Find(filterExpression);
            if (fileRecords != null)
            {
                var fileRecord = fileRecords.FirstOrDefault();
                fileRecord.DefaultThumnailUrl = selectedUrl;
                repository.InsertOrMerge(fileRecord);
            }
        }

        private void UpdateAssetThumbnailUrl(string assetID, string selectedUrl, MediaServiceContext mediaContext)
        {
            AssetRepository repository = new AssetRepository(storageAccount);
            string assetFilterExpression = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal,
                                     assetID);
            var records = repository.Find(assetFilterExpression);
            if (records != null)
            {
                var assetRecord = records.FirstOrDefault();
                assetRecord.DefaultThumbnailUrl = selectedUrl;
                repository.InsertOrMerge(assetRecord);
            }
        }

        /// <summary>
        /// Queues
        /// </summary>
        /// <param name="jobID"></param>
        private void QueueUnCommittedJob(string jobID, string job, string assetID, string additionalInfo = "")
        {
            UnCommittedDataEntity entity = new UnCommittedDataEntity(jobID, job);
            entity.Status = JobState.Queued.ToString();
            entity.LastUpdated = DateTime.UtcNow;
            entity.AdditionalInfo = additionalInfo;
            entity.AssetID = assetID;
            UnCommittedDataRepository repository = new UnCommittedDataRepository(storageAccount);
            repository.InsertOrReplace(entity);
        }

        private void UpdateJobStatus(UnCommittedDataEntity entity)
        {
            string assetID = string.Empty;
            var mediaContext = GetMediaServiceContext(this.currentmediaServiceName);
            Job job = new Job(mediaContext);
            Ais.Internal.Dcm.Business.JobState state = job.GetJobStatus(entity.EntityID);
            entity.Status = state.ToString();
            entity.LastUpdated = DateTime.UtcNow;
            if (state == Ais.Internal.Dcm.Business.JobState.Error || state == Ais.Internal.Dcm.Business.JobState.Finished)
            {
                entity.UpdateRequired = false;
                if (state == Ais.Internal.Dcm.Business.JobState.Finished)
                {
                    // split asset ID and asset File ID

                    assetID = entity.AdditionalInfo;
                    // get one thumbnail from Thumbnail asset
                    List<AssetWithFiles> urls = mediaContext.GetOutputAssetFiles(assetID);
                    foreach (var item in urls)
                    {
                        UpdateOutputEntity(assetID, item);
                    }
                }
            }
            UnCommittedDataRepository repository = new UnCommittedDataRepository(storageAccount);
            repository.InsertOrMerge(entity);
        }

        private void UpdateOutputEntity(string assetID, AssetWithFiles item)
        {
            AssetOutputRepository repository = new AssetOutputRepository(storageAccount);
            var outputs = repository.Find(string.Format("RowKey eq '{0}'", item.OutputAssetId));
            if (outputs.FirstOrDefault<AssetOutputEntity>() != null)
            {
                return; // no update
            }
            repository.InsertOrMerge(new AssetOutputEntity(assetID, item.OutputAssetId, item.Name, item.Name, item.URL));
        }

        [ActionName("GetJobStatus")]
        public List<JobInfo> GetJobStatus(string assetId)
        {
            var jobs = new List<JobInfo>();
            try
            {
                UnCommittedDataRepository repository = new UnCommittedDataRepository(storageAccount);
                var jobEntities = repository.Find(string.Format("AdditionalInfo eq '{0}'", assetId));
                foreach (var unCommittedDataEntity in jobEntities)
                {
                    if ((String.CompareOrdinal(unCommittedDataEntity.Status, JobState.Finished.ToString()) == 0) ||
                        (String.CompareOrdinal(unCommittedDataEntity.Status, JobState.Error.ToString()) == 0)) continue;
                    var job = new JobInfo
                        {
                            Id = unCommittedDataEntity.EntityID,
                            Name = unCommittedDataEntity.MediaServiceEntityType,
                            Status = unCommittedDataEntity.Status,
                            CreatedDate = unCommittedDataEntity.Timestamp.DateTime
                        };
                    jobs.Add(job);
                }
            }
            catch (Exception exp)
            {
                logger.LogException(NLog.LogLevel.Error, "GetJobStatus " + exp.ToString(), exp);
                throw;
            }
            return jobs;
        }

        [ActionName("ResetUpdateRequired")]
        [HttpGet]
        public void SetUpdateRequiredTrue(string mediaServiceName)
        {
            try
            {
                UnCommittedDataRepository repository = new UnCommittedDataRepository(storageAccount);
                string filterExpression = TableQuery.GenerateFilterConditionForBool("UpdateRequired", QueryComparisons.Equal,
                                                    false);
                var jobs = repository.Find(filterExpression);
                if (jobs != null)
                {
                    foreach (var unCommittedDataEntity in jobs)
                    {
                        if (unCommittedDataEntity.MediaServiceEntityType == "Job")
                        {
                            unCommittedDataEntity.UpdateRequired = true;
                            repository.InsertOrMerge(unCommittedDataEntity);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                logger.LogException(NLog.LogLevel.Error, "GetJobStatus " + exp.ToString(), exp);
                throw;
            }
        }
    }
}
