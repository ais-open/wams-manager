using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Ais.Internal.Dcm.Business;
using Ais.Internal.Dcm.Web.Models;
using AzurePatterns.Entity;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;

namespace Ais.Internal.Dcm.Web.Service
{
    public class UserMediaService : IUserMediaService
    {
        IMetadataService metadataService = null;
        ILoggerService loggerService = null;
        private ISearchService searchService = null;
        string seperator = "B4283131-431C-409C-A649-D81262954B00";
        private string currentmediaServiceName;
        private string mediaServiceTag = "MediaServiceTag";

        #region Public Methods
        public UserMediaService(IMetadataService metadataRepositoryService, ILoggerService logger, ISearchService search)
        {
            this.metadataService = metadataRepositoryService;
            this.loggerService = logger;
            this.searchService = search;
        }

        public List<MediaServiceModel> GetAllMediaServices(string clientKey)
        {
            List<MediaServiceModel> mServices = null;
            try
            {
                var mediaServiceRepository = metadataService.GetMediaServiceRepository();
                clientKey = "";//no filter
                string filterExpression = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual,
                                                   clientKey);
                var mediaServices = mediaServiceRepository.Find(filterExpression);
                if (mediaServices == null)
                {
                    throw new KeyNotFoundException("No results with filtering criteria found");
                }
                mServices = new List<MediaServiceModel>();
                foreach (var mediaServiceEntity in mediaServices)
                {
                    var mservice = new MediaServiceModel
                    {
                        MediaServiceFriendlyName = mediaServiceEntity.FriendlyName,
                        AccountName = mediaServiceEntity.RowKey,
                        ClientKey = mediaServiceEntity.PartitionKey,
                        PrimaryAccountKey = mediaServiceEntity.AccessKey
                    };
                    mServices.Add(mservice);
                }
            }
            catch (StorageException storageException)
            {
                throw new InvalidOperationException("Storage account used is not valid.", storageException);
            }
            catch (Exception exp)
            {
                loggerService.LogException("GetAllMediaServices: " + exp.ToString(), exp);
                throw;
            }
            return mServices;
        }

        public List<AssetInfoModel> GetAllAssets(string mediaServiceName)
        {
            List<AssetInfoModel> assets = null;
            string mediaServiceFriendlyName = "";
            try
            {
                mediaServiceFriendlyName = GetMediaServiceFriendlyName(mediaServiceName);

                var assetRepository = metadataService.GetAssetRepository();
                var assetEntities = assetRepository.Find(string.Format("PartitionKey eq '{0}'", mediaServiceName));

                if (assetEntities == null)
                {
                    throw new KeyNotFoundException("No results with filtering criteria found");
                }
                assets = new List<AssetInfoModel>();
                foreach (var assetEntity in assetEntities)
                {
                    var assetInfo = new AssetInfoModel()
                    {
                        Id = assetEntity.RowKey,
                        MediaServiceName = assetEntity.PartitionKey,
                        DefaultThumbnailUrl = assetEntity.DefaultThumbnailUrl,
                        Name = assetEntity.Name,
                        CreatedDate = assetEntity.Timestamp.DateTime,
                        Tags = assetEntity.Tags
                    };
                    assetInfo.MediaServiceFriendlyName = string.IsNullOrEmpty(mediaServiceFriendlyName) ? assetInfo.MediaServiceName : mediaServiceFriendlyName;
                    assets.Add(assetInfo);
                }
            }
            catch (Exception exp)
            {
                loggerService.LogException("GetAllAssets " + exp.ToString(), exp);
                throw;
            }
            return assets;
        }

        private string GetMediaServiceFriendlyName(string mediaServiceName)
        {
            MediaServiceEntity mediaServiceEntity = null;
            var friendlyName = string.Empty;
            var repository = metadataService.GetMediaServiceRepository();
            var query = repository.Find(string.Format("RowKey eq '{0}'", mediaServiceName));
            if (query != null)
            {
                mediaServiceEntity = query.FirstOrDefault();
                if (mediaServiceEntity != null)
                {
                    friendlyName = mediaServiceEntity.FriendlyName;
                }
            }
            return friendlyName;
        }

        public List<AssetInfoModel> GetAllAssets()
        {
            List<AssetInfoModel> assets = null;
            List<MediaServiceEntity> mediaServicesEntities = null;
            try
            {
                mediaServicesEntities = GetAllMediaServiceEntities();
                var assetRepository = metadataService.GetAssetRepository();
                var assetEntities = assetRepository.Find(string.Format("PartitionKey ne '{0}'", ""));

                if (assetEntities == null)
                {
                    throw new KeyNotFoundException("No results with filtering criteria found");
                }
                assets = new List<AssetInfoModel>();
                foreach (var assetEntity in assetEntities)
                {
                    var assetInfo = new AssetInfoModel()
                    {
                        Id = assetEntity.RowKey,
                        MediaServiceName = assetEntity.PartitionKey,
                        DefaultThumbnailUrl = assetEntity.DefaultThumbnailUrl,
                        Name = assetEntity.Name,
                        CreatedDate = assetEntity.Timestamp.DateTime,
                        Tags = assetEntity.Tags
                    };
                    var assetFriendlyName = (from s in mediaServicesEntities
                                             where s.RowKey == assetInfo.MediaServiceName
                                             select s.FriendlyName).First();
                    assetInfo.MediaServiceFriendlyName = assetFriendlyName != null ? assetFriendlyName : assetInfo.MediaServiceName;

                    assets.Add(assetInfo);
                }
            }
            catch (Exception exp)
            {
                loggerService.LogException("GetAllAssets " + exp.ToString(), exp);
                throw;
            }
            return assets;
        }

        private List<MediaServiceEntity> GetAllMediaServiceEntities()
        {
            List<MediaServiceEntity> mediaServicesEntities = null;
            var mediaServiceRepository = metadataService.GetMediaServiceRepository();
            var mediaServicesQuery = mediaServiceRepository.Find("PartitionKey ne ''"); // get all
            if (mediaServicesQuery != null)
            {
                mediaServicesEntities = mediaServicesQuery.ToList();
            }
            return mediaServicesEntities;
        }

        public AssetInfoModel CreateNewAsset(string mediaServiceName, string assetName)
        {
            AssetInfoModel assetInfo = null;
            try
            {
                var mediaServiceContext = GetMediaServiceContext(mediaServiceName);
                var asset = mediaServiceContext.CreateNewAsset(assetName);
                var assetEntity = new AssetEntity
                {
                    Id = asset.Id,
                    Name = asset.Name,
                    MediaServiceName = mediaServiceName,
                    PartitionKey = mediaServiceName,
                    RowKey = asset.Id,
                    Tags = string.Empty
                };
                var assetRepository = metadataService.GetAssetRepository();
                assetRepository.Insert(assetEntity);
                if (asset == null)
                {
                    throw new KeyNotFoundException("No results with filtering criteria found");
                }
                assetInfo = new AssetInfoModel()
                {
                    Id = assetEntity.RowKey,
                    MediaServiceName = assetEntity.PartitionKey,
                    DefaultThumbnailUrl = assetEntity.DefaultThumbnailUrl,
                    Name = assetEntity.Name,
                    Tags = assetEntity.Tags
                };
            }
            catch (Exception exp)
            {
                loggerService.LogException("CreateNewAsset " + exp.ToString(), exp);
                throw;
            }
            return assetInfo;
        }

        public string GetUploadSasUrl(string mediaServiceName, string assetId)
        {
            string uploadSasUrl = string.Empty;
            try
            {
                var mediaServiceContext = GetMediaServiceContext(mediaServiceName);
                //Asset asset = mediaServiceContext.GetSasUrlForUpload(assetId);
                uploadSasUrl = mediaServiceContext.GetSasUrlForUpload(assetId);
            }
            catch (Exception exp)
            {
                loggerService.LogException("GetUploadSasUrl " + exp.ToString(), exp);
                throw;
            }
            return uploadSasUrl;
        }

        public bool IsAJobPending(string mediaServiceName, string assetId)
        {
            bool IsPending = false;
            try
            {
                var repository = metadataService.GetUnCommittedDataRepository();
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
                loggerService.LogException("IsAJobPending " + exp.ToString(), exp);
                throw;
            }
            return IsPending;
        }

        public List<ThumbnailAssetModel> GetThumbnailUrls(string assetId)
        {
            List<ThumbnailAssetModel> assetThumbnails = null;
            try
            {
                var thumbnailAssetRepository = metadataService.GetAssetThumbnailRepository();
                var thumnailEntities = thumbnailAssetRepository.Find(string.Format("PartitionKey eq '{0}'", assetId));

                if (thumnailEntities == null) throw new KeyNotFoundException("Thumbnails for this asset not found");

                assetThumbnails = new List<ThumbnailAssetModel>();
                foreach (var thumbnailEntity in thumnailEntities)
                {
                    var assetWithThumbnail = new ThumbnailAssetModel()
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
                loggerService.LogException("GetThumbnailUrls " + exp.ToString(), exp);
                throw;
            }
            return assetThumbnails;
        }

        public List<AssetWithFilesModel> GetOutputAssetFiles(string mediaServiceName, string assetId)
        {
            List<AssetWithFilesModel> assetOutputFiles = null;
            try
            {
                var outputAssetRepository = metadataService.GetAssetOutputRepository();
                var outputEntities = outputAssetRepository.Find(string.Format("PartitionKey eq '{0}'", assetId));

                if (outputEntities == null) throw new KeyNotFoundException("Output assets not found");

                assetOutputFiles = new List<AssetWithFilesModel>();
                foreach (var assetOutputEntity in outputEntities)
                {
                    var assetWithOutput = new AssetWithFilesModel()
                    {
                        Name = assetOutputEntity.FriendlyName,
                        URL = assetOutputEntity.DownloadStreamingUrl,
                        CreatedDate = assetOutputEntity.Timestamp.DateTime,
                        ParentAssetId = assetOutputEntity.PartitionKey,
                        OutputAssetId = assetOutputEntity.RowKey
                    };
                    assetOutputFiles.Add(assetWithOutput);
                }
            }
            catch (Exception exp)
            {
                loggerService.LogException("GetOutputAssetFiles " + exp.ToString(), exp);
                throw;
            }
            return assetOutputFiles;
        }

        public List<AssetWithFilesModel> GetOutputAssetFiles()
        {
            List<AssetWithFilesModel> assetOutputFiles = null;
            try
            {
                var outputAssetRepository = metadataService.GetAssetOutputRepository();
                var outputEntities = outputAssetRepository.Find(string.Format("PartitionKey ne '{0}'", ""));

                if (outputEntities == null) throw new KeyNotFoundException("Output assets not found");

                assetOutputFiles = new List<AssetWithFilesModel>();
                foreach (var assetOutputEntity in outputEntities)
                {
                    var assetWithOutput = new AssetWithFilesModel()
                    {
                        Name = assetOutputEntity.FriendlyName,
                        URL = assetOutputEntity.DownloadStreamingUrl,
                        CreatedDate = assetOutputEntity.Timestamp.DateTime,
                        ParentAssetId = assetOutputEntity.PartitionKey,
                        FriendlyName = assetOutputEntity.FriendlyName,
                        OutputAssetId = assetOutputEntity.RowKey
                    };
                    assetOutputFiles.Add(assetWithOutput);
                }
            }
            catch (Exception exp)
            {
                loggerService.LogException("GetOutputAssetFiles " + exp.ToString(), exp);
                throw;
            }
            return assetOutputFiles;
        }

        public List<AzurePatterns.Entity.AssetFileInfo> GetAssetFiles(string mediaServiceName, string assetId)
        {
            List<AssetFileInfo> assetFiles = null;
            try
            {
                var assetFileRepository = metadataService.GetAssetFileRepository();
                var assetFileEntities = assetFileRepository.Find(string.Format("PartitionKey eq '{0}'", assetId));

                if (assetFileEntities == null)
                {
                    throw new KeyNotFoundException("Asset files not found");
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
                        Tags = assetFileEntity.Tags
                        //Created = assetFileEntity.Created, LastModified = assetFileEntity.LastModified
                    };
                    assetFiles.Add(assetFileInfo);
                }
            }
            catch (Exception exp)
            {
                loggerService.LogException("GetAssetFiles " + exp.ToString(), exp);
                throw;
            }
            return assetFiles;
        }

        public List<AzurePatterns.Entity.AssetFileInfo> GetAllAssetFiles()
        {
            List<AssetFileInfo> assetFiles = null;
            try
            {
                var assetFileRepository = metadataService.GetAssetFileRepository();
                var assetFileEntities = assetFileRepository.Find(string.Format("PartitionKey ne '{0}'", ""));

                if (assetFileEntities == null)
                {
                    throw new KeyNotFoundException("Asset files not found");
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
                        Tags = assetFileEntity.Tags
                        //Created = assetFileEntity.Created, LastModified = assetFileEntity.LastModified
                    };
                    assetFiles.Add(assetFileInfo);
                }
            }
            catch (Exception exp)
            {
                loggerService.LogException("GetAssetFiles " + exp.ToString(), exp);
                throw;
            }
            return assetFiles;
        }

        public List<AzurePatterns.Entity.AssetFileInfo> GetAssetFilesContext(string mediaServiceName, string assetId)
        {
            List<AssetFileInfo> assetFileInfos = null;
            try
            {
                var mediaServiceContext = GetMediaServiceContext(mediaServiceName);
                var assetFiles = mediaServiceContext.ListAssetFiles(assetId);
                if (assetFiles == null)
                {
                    throw new KeyNotFoundException("Asset files not found");
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
                        ParentAssetId = assetFile.ParentAssetId,
                        Tags = string.Empty
                    };
                    assetFileInfos.Add(astFileInfo);
                }
            }
            catch (Exception exp)
            {
                loggerService.LogException("GetAssetFilesContext " + exp.ToString(), exp);
                throw;
            }
            return assetFileInfos;
        }

        public List<EncodingTypeModel> GetEncodingTypes(string mediaServiceName)
        {
            var list = new List<EncodingTypeModel>();
            try
            {
                var repository = metadataService.GetEncodingTypeRepository();
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
                    throw new KeyNotFoundException("Encoding Types not found");
                }
            }
            catch (Exception exp)
            {
                loggerService.LogException("GetEncodingTypes " + exp.ToString(), exp);
                throw;
            }
            return list;
        }

        public bool GenerateFileMetaData(string mediaServiceName, string assetId)
        {
            bool isSuccess = false;

            try
            {
                var mediaServiceContext = GetMediaServiceContext(mediaServiceName);
                isSuccess = mediaServiceContext.GenerateFileMetadata(assetId);
            }
            catch (Exception exp)
            {
                loggerService.LogException("GenFileMetadata " + exp.ToString(), exp);
                throw;
            }
            return isSuccess;
        }

        public bool MakePrimaryFile(string mediaServiceName, string assetId, string assetFileId)
        {
            bool isSuccess = false;
            try
            {
                var mediaServiceContext = GetMediaServiceContext(mediaServiceName);
                var files = mediaServiceContext.ListAssetFiles(assetId);
                var primaryFile = files.FirstOrDefault(file => file.IsPrimary);
                if (primaryFile != null)
                {
                    mediaServiceContext.MakePrimaryFile(primaryFile.Id, false);
                    UpdateAssetFileMetaData(assetId, primaryFile, false);
                }
                var latestFile = files.FirstOrDefault(file => file.Id == assetFileId);
                InsertAssetFileMetaData(assetId, latestFile);
                isSuccess = mediaServiceContext.MakePrimaryFile(assetFileId, true);
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
                loggerService.LogException("MakePrimaryFile " + exp.ToString(), exp);
                throw;
            }
            return isSuccess;
        }

        public string InitiateJob(string mediaServiceName, string assetId, string enStrings, string enFriendlyNames)
        {
            string jobID = string.Empty;
            try
            {
                string[] encodingStrings = enStrings.Split(',');
                string[] encodingFriendlyNames = enFriendlyNames.Split(',');

                var mediaServiceContext = GetMediaServiceContext(mediaServiceName);
                var taskList = new List<TaskStep>();
                int outputOrder = 0;
                for (int i = 0; i < encodingStrings.Length; i++, outputOrder++)
                {

                    string appleEncodingFormat = "Apple HLS Format";
                    if (String.CompareOrdinal(encodingStrings[i], appleEncodingFormat) == 0)
                    {
                        var appleTasks = CreateAppleFormatTask(assetId, appleEncodingFormat, outputOrder);
                        taskList.AddRange(appleTasks);
                        outputOrder += 2;
                    }
                    else
                    {
                        //string encodingTaskPart = Literals.ENCODING + "_" + encodingStrings[i];
                        string encodingTaskPart = Literals.ENCODING + "_" + encodingFriendlyNames[i];
                        // 
                        string outputAssetName = string.Format("{0}-{1}-{2}-{3}", Literals.CHILD_ASSET_APPEND, assetId,
                                                               encodingTaskPart,
                                                               DateTime.UtcNow.ToString("yyyy_MM_dd_hh_MM_ss_UTC"));
                        TaskStep taskStep = new TaskStep
                        {
                            Configuration = encodingStrings[i],
                            InputAssetName = assetId,
                            MediaProcessorID = MediaServiceConstants.MediaProcessorId,
                            Order = (short)outputOrder,
                            OutputAssetName = outputAssetName
                        };
                        taskList.Add(taskStep);
                    }
                }

                string jobName = string.Format("JOB_{0}_{1}", assetId, DateTime.UtcNow.ToString("yyyy_MM_dd_hh_MM_ss_UTC"));
                var jobInfo = new MediaServiceJobInfo(jobName);
                jobInfo.InputAssetIds = new List<string> { assetId };
                jobInfo.Tasks = taskList;
                string jobString = jobInfo.CreateJsonString();
                Job job = mediaServiceContext.CreateEncodingJobV2(assetId, jobString, "");
                if (job != null)
                {
                    var files = mediaServiceContext.ListAssetFiles(assetId);
                    var latestFile = files.FirstOrDefault(file => file.IsPrimary);
                    if (latestFile != null)
                    {
                        string additionalInformation = AppendAssetIDAndFile(assetId, latestFile.Id, latestFile.Name);

                        // Queue a record for uncommitted data
                        QueueUnCommittedJob(job.Id, UnCommittedEntityType.Job, assetId, additionalInformation);
                    }
                    jobID = job.Id;
                }
            }
            catch (Exception exp)
            {
                loggerService.LogException("InitiateJob " + exp.ToString(), exp);
                throw;
            }
            return jobID;
        }

        public string InitiateThumbnailJob(string mediaServiceName, string assetId, string enStrings)
        {
            string jobId = string.Empty;
            try
            {
                string[] encodingStrings = enStrings.Split(',');
                var mediaServiceContext = GetMediaServiceContext(mediaServiceName);

                int imgQuality = 100;//best
                if (!string.IsNullOrWhiteSpace(encodingStrings[0]))
                    imgQuality = GetImageQuality(encodingStrings[0]);

                int imgHeight = 0;
                Int32.TryParse(encodingStrings[1], out imgHeight);

                int imgWidth = 0; Int32.TryParse(encodingStrings[2], out imgWidth);

                int maxNumOfThumb = 0; Int32.TryParse(encodingStrings[3], out maxNumOfThumb);
                int imgDurationInSec = 0; Int32.TryParse(encodingStrings[4], out imgDurationInSec);
                int stopAfter = maxNumOfThumb * imgDurationInSec;
                Job job = mediaServiceContext.CreateThumbnailJob(assetId, imgQuality, imgHeight, imgWidth, imgDurationInSec, stopAfter);
                if (job != null)
                {
                    var files = mediaServiceContext.ListAssetFiles(assetId);
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
                loggerService.LogException("InitiateThumbnailJob " + exp.ToString(), exp);
                throw;
            }
            return jobId;
        }

        public void Sync(string mediaServiceName)
        {
            try
            {
                this.currentmediaServiceName = mediaServiceName;
                Task.Run(() =>
                    {
                        var repository = metadataService.GetUnCommittedDataRepository();
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
                loggerService.LogException("Refresh " + exp.ToString(), exp);
                throw;
            }
        }

        public List<AzurePatterns.Entity.JobInfo> GetJobStatus(string assetId)
        {
            var jobs = new List<JobInfo>();
            try
            {
                var repository = metadataService.GetUnCommittedDataRepository();
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
                loggerService.LogException("GetJobStatus " + exp.ToString(), exp);
                throw;
            }
            return jobs;
        }

        public void UpdateMediaService(string mediaServiceName)
        {
            try
            {
                var repository = metadataService.GetUnCommittedDataRepository();
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
                loggerService.LogException("GetJobStatus " + exp.ToString(), exp);
                throw;
            }
        }

        public void DeleteAssetEntity(AssetEntity assetEntity)
        {
            var assetRepository = metadataService.GetAssetRepository();
            assetRepository.Delete(assetEntity);
        }

        public List<AssetInfoModel> FindAssetEntity(string filterExpression)
        {
            var assetRepository = metadataService.GetAssetRepository();
            var assets = assetRepository.Find(filterExpression);
            var filterAssets = new List<AssetInfoModel>();
            if (assets != null)
            {
                foreach (var assetEntity in assets)
                {
                    var assetInfo = new AssetInfoModel
                        {
                            DefaultThumbnailUrl = assetEntity.DefaultThumbnailUrl,
                            Id = assetEntity.Id,
                            MediaServiceName = assetEntity.MediaServiceName,
                            Name = assetEntity.Name,
                            Tags = assetEntity.Tags
                        };
                    filterAssets.Add(assetInfo);
                }
            }
            return filterAssets;
        }

        public List<AssetFileInfo> FindAssetFileEntity(string filterExpression)
        {
            var assetFileRepository = metadataService.GetAssetFileRepository();
            var assetFiles = assetFileRepository.Find(filterExpression);
            var filterAssets = new List<AssetFileInfo>();
            if (assetFiles != null)
            {
                filterAssets.AddRange(assetFiles.Select(assetFileEntity => new AssetFileInfo
                    {
                        DefaultThumbnailUrl = assetFileEntity.DefaultThumnailUrl,
                        Id = assetFileEntity.Id,
                        ContentFileSize = assetFileEntity.ContentFileSize,
                        Created = assetFileEntity.Timestamp.DateTime,
                        IsPrimary = assetFileEntity.IsPrimary,
                        LastModified = assetFileEntity.Timestamp.DateTime,
                        MimeType = assetFileEntity.MimeType,
                        ParentAssetId = assetFileEntity.ParentAssetId,
                        Name = assetFileEntity.Name
                    }));
            }
            return filterAssets;
        }

        public List<AssetWithFiles> FindAssetOutputEntity(string filterExpression)
        {
            var assetOutputRepository = metadataService.GetAssetOutputRepository();
            var outputs = assetOutputRepository.Find(filterExpression);
            var filterOutputs = new List<AssetWithFiles>();
            if (outputs != null)
            {
                filterOutputs.AddRange(outputs.Select(outputEntity => new AssetWithFiles
                    {
                        Name = outputEntity.FriendlyName,
                        CreatedDate = outputEntity.Timestamp.DateTime,
                        URL = outputEntity.DownloadStreamingUrl
                    }));
            }
            return filterOutputs;
        }

        public Tag CreateTag(string mediaServiceName, string tagName)
        {
            Tag tag = null;
            try
            {
                var repository = metadataService.GetTagRepository();
                var id = Guid.NewGuid().ToString();
                var tagEntity = new TagEntity
                    {
                        Id = id,
                        Name = tagName,
                        PartitionKey = mediaServiceTag,
                        RowKey = id
                    };

                repository.Insert(tagEntity);
                tag = new Tag() { Id = tagEntity.Id, Name = tagEntity.Name };
            }
            catch (Exception exp)
            {
                loggerService.LogException("CreateNewTag " + exp.ToString(), exp);
                throw;
            }
            return tag;
        }

        public void UpdateAssetWithTags(string mediaServiceName, string assetId, string tags)
        {
            try
            {
                var repository = metadataService.GetAssetRepository();
                string filterExpression = string.Format("PartitionKey eq '{0}' and RowKey eq '{1}'", mediaServiceName,
                                                        assetId);
                var assets = repository.Find(filterExpression);
                if (assets != null)
                {
                    foreach (var assetEntity in assets)
                    {
                        var tag = assetEntity.Tags;
                        tag += tags + ",";
                        assetEntity.Tags = tag;
                        repository.InsertOrMerge(assetEntity);
                    }
                }
            }
            catch (Exception exp)
            {
                loggerService.LogException("UpdateAssetWithTags " + exp.ToString(), exp);
                throw;
            }
        }

        public void UpdateAssetFileWithTags(string mediaServiceName, string parentAssetId, string assetFileId, string tags)
        {
            try
            {
                var repository = metadataService.GetAssetFileRepository();
                string filterExpression = string.Format("PartitionKey eq '{0}' and RowKey eq '{1}'", parentAssetId,
                                                        assetFileId);
                var assetFiles = repository.Find(filterExpression);
                if (assetFiles != null)
                {
                    foreach (var assetFileEntity in assetFiles)
                    {
                        var tag = assetFileEntity.Tags;
                        tag += tags + ",";
                        assetFileEntity.Tags = tag;
                        repository.InsertOrMerge(assetFileEntity);
                    }
                }

                //UpdateMediaHistory(mediaServiceName, parentAssetId, assetFileId, tags);
            }
            catch (Exception exp)
            {
                loggerService.LogException("UpdateAssetFileWithTags " + exp.ToString(), exp);
                throw;
            }
        }

        //private void UpdateMediaHistory(string mediaServiceName, string parentAssetId, string assetFileId, string tags = "")
        private void UpdateMediaHistory(SearchResultViewModel searchViewModel)
        {
            //var searchViewModel = new SearchResultViewModel
            //    {
            //        CollectionName = mediaServiceName,
            //        MediaServiceName = mediaServiceName,
            //        ParentAssetId = parentAssetId,
            //        TagsForSearch = tags
            //    };
            try
            {
                var assetRepository = metadataService.GetAssetRepository();
                var assets =
                    assetRepository.Find(string.Format("PartitionKey eq '{0}' and RowKey eq '{1}'", searchViewModel.MediaServiceName,
                                                       searchViewModel.ParentAssetId));
                if (assets != null && assets.Any())
                {
                    searchViewModel.AlbumName = assets.ToList()[0].Name;
                }
                var assetFileRepository = metadataService.GetAssetFileRepository();
                var assetFiles =
                    assetFileRepository.Find(string.Format("PartitionKey eq '{0}' and RowKey eq '{1}'", searchViewModel.ParentAssetId,
                                                       searchViewModel.AssetFileId));
                if (assetFiles != null && assetFiles.Any())
                {
                    searchViewModel.FileName = assetFiles.ToList()[0].Name;
                    searchViewModel.NameForSearch = assetFiles.ToList()[0].Name;
                    searchViewModel.AssetFileId = assetFiles.ToList()[0].Id;
                    searchViewModel.Tags = new List<Tag>();
                    var tagString = assetFiles.ToList()[0].Tags;
                    if (!string.IsNullOrEmpty(tagString))
                    {
                        foreach (var tag in tagString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            searchViewModel.Tags.Add(new Tag { Name = tag.Trim() });
                        }
                    }

                    var tagEntity = tagString;
                    //tagEntity += tags + ",";
                    searchViewModel.TagsForSearch = tagEntity;

                    if (!string.IsNullOrEmpty(tagEntity))
                    //if (!string.IsNullOrEmpty(tags))
                    {
                        //foreach (var tag in tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        foreach (var tag in tagEntity.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            searchViewModel.Tags.Add(new Tag { Name = tag.Trim() });
                        }
                    }
                }
                searchViewModel.NameForSearch = BuildSearchFriendlyString(searchViewModel);
                searchService.InsertMediaHistory(searchViewModel);
            }
            catch (Exception exception)
            {
                loggerService.LogException("Insert Service" + exception.ToString(), exception);
            }
        }

        private string BuildSearchFriendlyString(SearchResultViewModel viewModel)
        {
            return Clean(viewModel.CollectionName) + Clean(viewModel.FileName) + Clean(viewModel.MediaServiceName) + Clean(viewModel.TagsForSearch);
        }

        private string Clean(string value)
        {
            return value == null ? "" : value;
        }

        public List<Tag> GetTags(string mediaServiceName)
        {
            var tagList = new List<Tag>();

            try
            {
                var repository = metadataService.GetTagRepository();
                var filterExpression = string.Format("PartitionKey eq '{0}'", mediaServiceTag);
                var tags = repository.Find(filterExpression);
                if (tags != null)
                {
                    foreach (var tagEntity in tags)
                    {
                        var tagInfo = new Tag { Id = tagEntity.Id, Name = tagEntity.Name };
                        tagList.Add(tagInfo);
                    }
                }
                return tagList;
            }
            catch (Exception exception)
            {
                loggerService.LogException("GetTags " + exception.ToString(), exception);
            }
            return tagList;
        }

        public List<Tag> GetAssetFileTags(string assetFileId)
        {
            var tagList = new List<Tag>();

            try
            {
                var repository = metadataService.GetAssetFileRepository();
                var filterExpression = string.Format("RowKey eq '{0}'", assetFileId);
                var assetFiles = repository.Find(filterExpression);
                if (assetFiles != null)
                {
                    var tagString = string.Empty;
                    foreach (var asset in assetFiles)
                    {
                        tagString += asset.Tags;
                    }
                    if (!string.IsNullOrWhiteSpace(tagString))
                    {
                        var lastIndex = tagString.LastIndexOf(",", System.StringComparison.Ordinal);
                        if (lastIndex > 0 && lastIndex == tagString.Length - 1)
                            tagString = tagString.Remove(tagString.LastIndexOf(",", System.StringComparison.Ordinal));
                        var tagArray = tagString.Split(',').ToList();
                        tagList.AddRange(tagArray.Select(tag => new Tag() { Id = Guid.Empty.ToString(), Name = tag }));
                    }
                }
                return tagList;
            }
            catch (Exception exception)
            {
                loggerService.LogException("GetAssetFileTags " + exception.ToString(), exception);
            }
            return tagList;
        }

        public SearchData SearchMedia(string searchString, int rowsToSkip, int rowsToRetrieve, int searchType)
        {
            var searchData = new SearchData();
            SearchType type = SearchType.FreeText;
            try
            {
                if (searchType > 0)
                {
                    type = SearchType.TagSearch;
                }
                searchData = searchService.SearchMedia(searchString, rowsToSkip, rowsToRetrieve, type);
            }
            catch (Exception exception)
            {
                loggerService.LogException("Search Media" + exception.ToString(), exception);
            }
            return searchData;
        }
        #endregion

        #region Private Methods
        private MediaServiceContext GetMediaServiceContext(string mediaServiceName)
        {
            var mediaServiceRepository = metadataService.GetMediaServiceRepository();
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

        private string AppendAssetIDAndFile(string assetId, string assetFileId, string fileName)
        {
            return string.Format("{0}{3}{1}{3}{2}", assetId, assetFileId, fileName, seperator);
        }

        private void UpdateAssetFileMetaData(string assetId, AssetFile primaryFile, bool isPrimary)
        {
            var assetFileRepository = metadataService.GetAssetFileRepository();
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
                RowKey = assetFile.Id,
                Tags = string.Empty
            };

            var assetFileRepository = metadataService.GetAssetFileRepository();
            var existsFile = assetFileRepository.Find(string.Format("RowKey eq '{0}'", assetFile.Id));
            if (existsFile != null && existsFile.Count() > 0)
                assetFileRepository.Replace(assetFileEntity);
            else
                assetFileRepository.Insert(assetFileEntity);
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
            var repository = metadataService.GetUnCommittedDataRepository();
            repository.InsertOrReplace(entity);
        }

        private void UpdateJobStatus(UnCommittedDataEntity entity)
        {
            string assetId = string.Empty;
            string assetFileId = string.Empty;
            string fileName = string.Empty;

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
                    string[] splittedString = entity.AdditionalInfo.Split(new string[] { seperator }, StringSplitOptions.RemoveEmptyEntries);
                    if (splittedString.Length == 3)
                    {
                        assetId = splittedString[0];
                        assetFileId = splittedString[1];
                        fileName = splittedString[2];
                    }
                    else assetId = entity.AdditionalInfo;

                    var searchOutputs = new List<VideoOutput>();
                    //assetId = entity.AdditionalInfo;
                    // get one thumbnail from Thumbnail asset
                    List<AssetWithFiles> urls = mediaContext.GetOutputAssetFiles(assetId);
                    foreach (var item in urls)
                    {
                        UpdateOutputEntity(assetId, item);
                        //for searchviewmodel
                        if (item.URL.Contains("/" + Path.GetFileNameWithoutExtension(fileName) + "."))
                        {
                            var searchOutput = new VideoOutput() {EncodingName = item.Name, Url = item.URL};
                            searchOutputs.Add(searchOutput);
                        }
                    }

                    //Update SearchMediaHistory SQLite DB
                    var searchViewModel = new SearchResultViewModel
                        {
                            CollectionName = this.currentmediaServiceName,
                            MediaServiceName = this.currentmediaServiceName,
                            ParentAssetId = assetId,
                            TagsForSearch = string.Empty,
                            FileName = fileName,
                            AssetFileId = assetFileId
                        };
                    searchViewModel.Outputs = searchOutputs;

                    // get one thumbnail from Thumbnail asset
                    List<string> thumburls = mediaContext.GetLatestThumbnailUrl(assetId, fileName);
                    if (thumburls != null && thumburls.Count > 0)
                    {
                        Random r = new Random();
                        var thumbnail = thumburls[r.Next(0, thumburls.Count - 1)];
                        if (thumbnail.Contains("/" + Path.GetFileNameWithoutExtension(fileName)))
                            searchViewModel.DefaultThumbnailUrl = thumbnail;
                    }
                    UpdateMediaHistory(searchViewModel);
                }
            }
            var repository = metadataService.GetUnCommittedDataRepository();
            repository.InsertOrMerge(entity);
        }

        private void UpdateOutputEntity(string assetID, AssetWithFiles item)
        {
            var repository = metadataService.GetAssetOutputRepository();
            var outputs = repository.Find(string.Format("RowKey eq '{0}'", item.OutputAssetId));
            if (outputs.FirstOrDefault<AssetOutputEntity>() != null)
            {
                return; // no update
            }
            repository.InsertOrMerge(new AssetOutputEntity(assetID, item.OutputAssetId, item.Name, item.Name, item.URL));
        }

        private List<TaskStep> CreateAppleFormatTask(string assetId, string appleFormat, int outputOrder)
        {
            var appleTaskList = new List<TaskStep>();
            string h264Encoding = "H264 Broadband 720p";
            string h264SmoothEncoding = "H264 Smooth Streaming 720p";
            string h264IosEncoding = "H264 Adaptive Bitrate MP4 Set 720p for iOS Cellular Only";

            string encodingTaskPart = Literals.ENCODING + "_" + appleFormat;
            string outputAssetName1 = string.Format("{0}-{1}-{2}-{3}_1", "TEMP-ASST", assetId, h264Encoding, DateTime.UtcNow.ToString("yyyy_MM_dd_hh_MM_ss_UTC"));
            string outputAssetName2 = string.Format("{0}-{1}-{2}-{3}_2", "TEMP-ASST", assetId, h264SmoothEncoding, DateTime.UtcNow.ToString("yyyy_MM_dd_hh_MM_ss_UTC")); ;
            string outputAssetName = string.Format("{0}-{1}-{2}-{3}", Literals.CHILD_ASSET_APPEND, assetId, encodingTaskPart, DateTime.UtcNow.ToString("yyyy_MM_dd_hh_MM_ss_UTC"));

            TaskStep taskStep = new TaskStep
            {
                Configuration = h264Encoding,
                InputAssetName = assetId,
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
            var repository = metadataService.GetUnCommittedDataRepository();
            repository.InsertOrReplace(entity);
        }

        private void UpdateThumbnailEntity(List<ThumbnailModel> thumbnails)
        {
            var repository = metadataService.GetAssetThumbnailRepository();
            foreach (var thumbnail in thumbnails)
            {
                repository.InsertOrMerge(new AssetThumbnailEntity(thumbnail.AssetId, thumbnail.ThumbnailFileId, thumbnail.AssetFileId, thumbnail.Name, thumbnail.URL));
            }
        }

        private void UpdateFileThumbnailUrl(string assetID, string assetFileID, string selectedUrl)
        {
            var repository = metadataService.GetAssetFileRepository();
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
            var repository = metadataService.GetAssetRepository();
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
        #endregion
    }
}