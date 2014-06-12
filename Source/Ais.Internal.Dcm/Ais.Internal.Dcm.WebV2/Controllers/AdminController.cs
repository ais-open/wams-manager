using Ais.Internal.Dcm.Web.Models;
using Ais.Internal.Dcm.Web.Service;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Ais.Internal.Dcm.Web.Controllers
{

    public class AdminController : ApiController
    {
        ILoggerService logger = null;
        IAdminMediaService adminMediaService = null;
        public AdminController(ILoggerService loggerService,IAdminMediaService adminMediaService)
        {
            try
            {
                this.logger = loggerService;
                this.adminMediaService = adminMediaService;
            }
            catch (Exception exp)
            {
                logger.LogException("AdminController Constructor: "+exp.ToString(), exp);
                throw;
            }
        }
        /// <summary>
        /// Returns a list of Encoding types.
        /// </summary>
        /// <param name="mediaServiceName"></param>
        /// <returns></returns>
        [HttpGet]
        public List<EncodingTypeModel> GetEncodingTypes(string mediaServiceName)
        {
            mediaServiceName = "";// get all data
            List<EncodingTypeModel> encodingTypes = null;
            try
            {
                encodingTypes = this.adminMediaService.GetEncodingTypes(mediaServiceName);
            }
            catch (Exception exp)
            {
                logger.LogException("GetEncodingTypes: " + exp.ToString(), exp);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exp.Message));
            }
            return encodingTypes;
        }
        
        /// <summary>
        /// Returns a list of All Media Services.
        /// </summary>
        /// <param name="clientKey"></param>
        /// <returns></returns>
        [HttpGet]
        public List<MediaServiceModel> GetAllMediaServices(string clientKey)
        {
            clientKey = ""; // get all : no filtering
            List<MediaServiceModel> mServices = null;
            try
            {
                mServices = this.adminMediaService.GetMediaServices(clientKey);
            }
            catch (Exception exp)
            {
         
                logger.LogException("GetAllMediaServices: " + exp.ToString(), exp);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exp.Message));
            }
            return mServices;
        }
        /// <summary>
        /// Creating a new Encoding type.
        /// </summary>
        /// <param name="encoding"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage CreateEncodingType(EncodingTypeModel encoding)
        {
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string message = string.Empty;
            try
            {
                bool result = this.adminMediaService.AddEncodingType(encoding);
                if (result)
                {
                    code = HttpStatusCode.Accepted;
                }
                else
                {
                    code = HttpStatusCode.InternalServerError;
                }
            }
            catch (Exception exp)
            {
                logger.LogException("CreateEncodingType: " + exp.ToString(), exp);
                message += exp.ToString();
                return Request.CreateErrorResponse(code,new WebException(message)); 
            }
            return new HttpResponseMessage(code);
        }

        /// <summary>
        /// Deleting an endoing type from the list.
        /// </summary>
        /// <param name="encoding"></param>
        /// <returns></returns>
        [HttpDelete]
        public HttpResponseMessage DeleteEncodingType(EncodingTypeModel encoding)
        {
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string message = string.Empty;
            try
            {
                bool result = this.adminMediaService.RemoveEncodingType(encoding);
                if (result)
                {
                    code = HttpStatusCode.Accepted;
                }
                else
                {
                    code = HttpStatusCode.InternalServerError;
                }
            }
            catch (Exception exp)
            {
                logger.LogException("DeleteEncodingType: " + exp.ToString(), exp);
                message += exp.ToString();
                return Request.CreateErrorResponse(code, new WebException(message));
            }
            return new HttpResponseMessage(code);
        }

        /// <summary>
        /// Checks if Media service is successfully created or not.
        /// </summary>
        /// <param name="mediaService"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage CreateMediaService(MediaServiceModel mediaService)
        {
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string message = string.Empty;
            try
            {
                bool result = this.adminMediaService.AddMediaService(mediaService);
                if (result)
                {
                    code = HttpStatusCode.Accepted;
                }
                else
                {
                    code = HttpStatusCode.InternalServerError;
                }
            }
            catch (Exception exp)
            {
                logger.LogException("CreateMediaService: " + exp.ToString(), exp);
                message += exp.ToString();
                return Request.CreateErrorResponse(code, new WebException(message));
            }
            return new HttpResponseMessage(code);
        }

        /// <summary>
        /// Checks if Media Service is sucessfully deleted or not.
        /// </summary>
        /// <param name="mediaService"></param>
        /// <returns></returns>
        [HttpDelete]
        public HttpResponseMessage DeleteMediaService(MediaServiceModel mediaService)
        {

            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string message = string.Empty;
            try
            {
                bool result = this.adminMediaService.RemoveMediaService(mediaService);
                if (result)
                {
                    code = HttpStatusCode.Accepted;
                }
                else
                {
                    code = HttpStatusCode.InternalServerError;
                }
            }
            catch (Exception exp)
            {
                logger.LogException("DeleteMediaService: " + exp.ToString(), exp);
                message += exp.ToString();
                return Request.CreateErrorResponse(code, new WebException(message));
            }
            return new HttpResponseMessage(code);
        }

        
        
    }
}
