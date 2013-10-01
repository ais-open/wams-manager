﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzurePatterns.Repository;
using Microsoft.WindowsAzure.Storage;
using Ais.Internal.Dcm.Business;
using Ais.Internal.Dcm.Web.Models;
using Microsoft.WindowsAzure.Storage.Table;
using AzurePatterns.Entity;
namespace Ais.Internal.Dcm.Web.Service
{
    public class AdminMediaService : IAdminMediaService
    {
        IMetadataService metadataService = null;
        ILoggerService loggerService = null;
        public AdminMediaService(IMetadataService metadataRepositoryService,ILoggerService logger)
        {
            this.metadataService = metadataRepositoryService;
            this.loggerService = logger;
        }

        public List<EncodingTypeModel> GetEncodingTypes(string mediaServiceName)
        {
            List<EncodingTypeModel> encodingTypes = new List<EncodingTypeModel>();

            try
            {
                var encodingTypeRepository = metadataService.GetEncodingTypeRepository();
                string filterExpression = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual,
                                                   mediaServiceName);
                var mediaServices = encodingTypeRepository.Find(filterExpression);
                if (mediaServices == null)
                {
                   throw new KeyNotFoundException("No results with filtering criteria found");
                }
                encodingTypes = new List<EncodingTypeModel>();
                foreach (var mediaServiceEntity in mediaServices)
                {
                    var mservice = new EncodingTypeModel
                    {
                        FriendlyName = mediaServiceEntity.FriendlyName,
                        TechnicalName = mediaServiceEntity.RowKey,
                    };
                    encodingTypes.Add(mservice);
                }
            }
            catch (StorageException storageException)
            {
                throw new InvalidOperationException("Storage account used is not valid.",storageException);
            }
            catch (Exception exp)
            {
                // log the error;
                //encodingTypes = null; 
                loggerService.LogException("GetEncodingTypes: " + exp.ToString(), exp);
                throw;

            }
            return encodingTypes;
        }

        public List<MediaServiceModel> GetMediaServices(string clientKey)
        {
            List<MediaServiceModel> mServices = null;
            try
            {

                var mediaServiceRepository = metadataService.GetMediaServiceRepository();
                string filterExpression = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual,
                                                   clientKey);
                var mediaServices = mediaServiceRepository.Find(filterExpression);
                if (mediaServices == null)
                {
                    throw new KeyNotFoundException("Media services not found");   
                }
                mServices = new List<MediaServiceModel>();
                foreach (var mediaServiceEntity in mediaServices)
                {
                    var mservice = new MediaServiceModel
                    {
                        MediaServiceFriendlyName = mediaServiceEntity.FriendlyName,
                        AccountName = mediaServiceEntity.RowKey,
                        PrimaryAccountKey = mediaServiceEntity.AccessKey
                    };
                    mServices.Add(mservice);
                }
            }
            catch (Exception exp)
            {
                // log the error;
                //encodingTypes = null; 
                loggerService.LogException( "GetAllMediaServices: " + exp.ToString(), exp);
                throw;
                //log exception
            }
            return mServices;
        }

        public bool AddEncodingType(EncodingTypeModel encoding)
        {
            bool isSuccess = false;
            try
            {
                if (encoding == null)
                {
                    return isSuccess;
                }
                if (string.IsNullOrWhiteSpace(encoding.TechnicalName) || string.IsNullOrWhiteSpace(encoding.FriendlyName))
                {
                    return isSuccess;
                }
                var encodingEntity = new EncodingTypeEntity(encoding.TechnicalName, encoding.FriendlyName);
                var encodingRepository = metadataService.GetEncodingTypeRepository();
                encodingRepository.InsertOrMerge(encodingEntity);
                isSuccess = true;
            }
            catch (Exception exp)
            {
                loggerService.LogException("CreateEncodingType: " + exp.ToString(), exp);
                return false;
            }
            return isSuccess;
        }

        public bool RemoveEncodingType(EncodingTypeModel encoding)
        {
            bool isSuccess = false;
            try
            {
                if (encoding == null)
                {
                    return isSuccess;
                }
                if (string.IsNullOrWhiteSpace(encoding.TechnicalName))
                {
                    return isSuccess;
                }
                var encodingTypeRepository = metadataService.GetEncodingTypeRepository();
                string filterExpression = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal,
                                             encoding.TechnicalName);
                var entities = encodingTypeRepository.Find(filterExpression);
                var entity = entities.First<EncodingTypeEntity>();
                encodingTypeRepository.Delete(entity);
                isSuccess = true;

            }
            catch (Exception exp)
            {
                loggerService.LogException("DeleteEncodingType: " + exp.ToString(), exp);
                return false;
            }
            return isSuccess;
        }

        public bool AddMediaService(MediaServiceModel mediaService)
        {
            bool isSuccess = false;
            try
            {
                if (mediaService == null)
                {
                    isSuccess = false;
                }
                if (string.IsNullOrWhiteSpace(mediaService.AccountName) || string.IsNullOrWhiteSpace(mediaService.MediaServiceFriendlyName) || string.IsNullOrWhiteSpace(mediaService.PrimaryAccountKey))
                {
                    isSuccess = false;
                }
                var mediaEntity = new MediaServiceEntity
                {
                    AccessKey = mediaService.PrimaryAccountKey,
                    FriendlyName = mediaService.MediaServiceFriendlyName,
                    RowKey = mediaService.AccountName,
                    PartitionKey = "Universal"
                };
                var mediaRepository = metadataService.GetMediaServiceRepository();
                mediaRepository.InsertOrMerge(mediaEntity);
                isSuccess = true;

            }
           
            catch (Exception exp)
            {
                loggerService.LogException("CreateMediaService: " + exp.ToString(), exp);
                return false;
            }
            return isSuccess;
        }

        public bool RemoveMediaService(MediaServiceModel mediaService)
        {
            bool isSuccess = false;
            try
            {
                var mediaServiceRepository = metadataService.GetMediaServiceRepository();
                string filterExpression = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal,
                                             mediaService.AccountName);
                var entities = mediaServiceRepository.Find(filterExpression);
                var entity = entities.First<MediaServiceEntity>();
                mediaServiceRepository.Delete(entity);
                isSuccess = true;
            }
             catch (Exception exp)
            {
                loggerService.LogException("DeleteEncodingType: " + exp.ToString(), exp);
                return false;
            }
            return isSuccess;
        }
    }
}