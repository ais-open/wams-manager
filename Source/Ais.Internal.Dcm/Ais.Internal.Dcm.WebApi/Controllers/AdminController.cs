using Ais.Internal.Dcm.Business;
using Ais.Internal.Dcm.Web.Models;
using AzurePatterns.Entity;
using AzurePatterns.Repository;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Ais.Internal.Dcm.Web.Controllers
{

    public class AdminController : ApiController
    {
        private readonly string accountName;
        private readonly string accountKey;
        private readonly CloudStorageAccount storageAccount;
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public AdminController()
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
                logger.LogException(NLog.LogLevel.Error, "AdminController Constructor: "+exp.ToString(), exp);
                throw;
            }
        }


        [HttpGet]
        public List<EncodingTypeModel> GetEncodingTypes(string mediaServiceName)
        {
            mediaServiceName = "";// get all data
            List<EncodingTypeModel> encodingTypes = new List<EncodingTypeModel>();

            try
            {
                var encodingTypeRepository = new EncodingTypeRepository(storageAccount);
                string filterExpression = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual,
                                                   mediaServiceName);
                var mediaServices = encodingTypeRepository.Find(filterExpression);
                if (mediaServices == null)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, string.Format("Media Service {0} Not found", mediaServiceName)));
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
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Storage account used is not valid.",storageException));
            }
            catch (Exception exp)
            {
                // log the error;
                //encodingTypes = null; 
                logger.LogException(NLog.LogLevel.Error, "GetEncodingTypes: " + exp.ToString(), exp);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exp.Message));

            }
            return encodingTypes;
        }


        [HttpGet]
        public List<MediaServiceInfo> GetAllMediaServices(string clientKey)
        {
            clientKey = ""; // get all : no filtering
            List<MediaServiceInfo> mServices = null;
            try
            {

                var mediaServiceRepository = new MediaServiceRepository(storageAccount);
                string filterExpression = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual,
                                                   clientKey);
                var mediaServices = mediaServiceRepository.Find(filterExpression);
                if (mediaServices == null)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Media Service {0} Not found", clientKey)));
                }
                mServices = new List<MediaServiceInfo>();
                foreach (var mediaServiceEntity in mediaServices)
                {
                    var mservice = new MediaServiceInfo
                    {
                        FriendlyName = mediaServiceEntity.FriendlyName,
                        AccountName = mediaServiceEntity.RowKey,
                        /*ClientKey = mediaServiceEntity.PartitionKey,*/
                        PrimaryAccountKey = mediaServiceEntity.AccessKey
                    };
                    mServices.Add(mservice);
                }
            }
            catch (Exception exp)
            {
                // log the error;
                //encodingTypes = null; 
                logger.LogException(NLog.LogLevel.Error, "GetAllMediaServices: " + exp.ToString(), exp);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exp.Message));
                //log exception
            }
            return mServices;
        }



        [HttpPost]
        public HttpResponseMessage CreateEncodingType(EncodingTypeModel encoding)
        {
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string message = string.Empty;
            try
            {
                if (encoding == null)
                {
                    code = HttpStatusCode.BadRequest;
                    message = "Input is null";
                    throw new WebException(message);
                }
                if (string.IsNullOrWhiteSpace(encoding.TechnicalName) || string.IsNullOrWhiteSpace(encoding.FriendlyName))
                {
                    code = HttpStatusCode.BadRequest;
                    message = "Technical Name and Friendly Name both are required";
                    throw new WebException(message);
                }
                var encodingEntity = new EncodingTypeEntity(encoding.TechnicalName, encoding.FriendlyName);
                var encodingRepository = new EncodingTypeRepository(storageAccount);
                encodingRepository.InsertOrMerge(encodingEntity);
                code = HttpStatusCode.Accepted;
            }
            catch (Exception exp)
            {
                logger.LogException(NLog.LogLevel.Error, "CreateEncodingType: " + exp.ToString(), exp);
                message += exp.ToString();
                return Request.CreateErrorResponse(code,new WebException(message)); 
            }
            return new HttpResponseMessage(code);
        }

        [HttpDelete]
        public HttpResponseMessage DeleteEncodingType(EncodingTypeModel encoding)
        {
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string message = string.Empty;
            try
            {
                if (encoding == null)
                {
                    code = HttpStatusCode.BadRequest;
                    message = "Input is null";
                    throw new WebException(message);
                }
                if (string.IsNullOrWhiteSpace(encoding.TechnicalName))
                {
                    code = HttpStatusCode.BadRequest;
                    message = "Technical Name is required";
                    throw new WebException(message);
                }
                var encodingTypeRepository = new EncodingTypeRepository(storageAccount);
                string filterExpression = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal,
                                             encoding.TechnicalName);
                var entities = encodingTypeRepository.Find(filterExpression);
                var entity = entities.First<EncodingTypeEntity>();
                encodingTypeRepository.Delete(entity);
            }
            catch (Exception exp)
            {
                logger.LogException(NLog.LogLevel.Error, "DeleteEncodingType: " + exp.ToString(), exp);
                message += exp.ToString(); // include the stack trace
                return Request.CreateErrorResponse(code, new WebException(message));
            }
            return new HttpResponseMessage(code);
        }

        [HttpPost]
        public HttpResponseMessage CreateMediaService(MediaServiceInfo mediaService)
        {
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string message = string.Empty;
            try
            {
                if (mediaService == null)
                {
                    code = HttpStatusCode.BadRequest;
                    message = "Input is null";
                    throw new WebException(message);
                }
                if (string.IsNullOrWhiteSpace(mediaService.AccountName) || string.IsNullOrWhiteSpace(mediaService.FriendlyName) || string.IsNullOrWhiteSpace(mediaService.PrimaryAccountKey) )
                {
                    code = HttpStatusCode.BadRequest;
                    message = "Media service name, friendly name and key all fields required";
                    throw new WebException(message);
                }
                var mediaEntity = new MediaServiceEntity
                {
                    AccessKey = mediaService.PrimaryAccountKey,
                    FriendlyName = mediaService.FriendlyName,
                    RowKey = mediaService.AccountName,
                    PartitionKey = "Universal"
                };
                var encodingRepository = new MediaServiceRepository(storageAccount);
                encodingRepository.InsertOrMerge(mediaEntity);

            }
            catch (Exception exp)
            {
                logger.LogException(NLog.LogLevel.Error, "CreateMediaService: " + exp.ToString(), exp);
                message += exp.ToString(); // include the stack trace
                return Request.CreateErrorResponse(code, new WebException(message));
            }
            return new HttpResponseMessage(code);
        }

        [HttpDelete]
        public HttpResponseMessage DeleteMediaService(MediaServiceInfo mediaService)
        {

            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string message = string.Empty;
            try
            {
                var mediaServiceRepository = new MediaServiceRepository(storageAccount);
                string filterExpression = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal,
                                             mediaService.AccountName);
                var entities = mediaServiceRepository.Find(filterExpression);
                var entity = entities.First<MediaServiceEntity>();
                mediaServiceRepository.Delete(entity);
            }
            catch (Exception exp)
            {

                logger.LogException(NLog.LogLevel.Error, "DeleteMediaService: " + exp.ToString(), exp);
                message += exp.ToString(); // include the stack trace
                return Request.CreateErrorResponse(code, new WebException(message));
            }
            return new HttpResponseMessage(code);
        }

        
    }
}
