using System;
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

    public interface IAdminMediaService
    {
        /// <summary>
        /// Gets all the encoding types available with a given media service
        /// </summary>
        /// <param name="mediaServiceName">name of the media service</param>
        /// <returns>list of encoding informaiton</returns>
        List<EncodingTypeModel> GetEncodingTypes(string mediaServiceName);

        /// <summary>
        /// Gets all the media service available to a given client
        /// </summary>
        /// <param name="mediaServiceName">name of the media service</param>
        /// <returns>list of encoding informaiton</returns>
        List<MediaServiceModel> GetMediaServices(string clientKey);

        /// <summary>
        /// Adds an encoding in admin configuration
        /// </summary>
        /// <param name="encoding">new encoding</param>
        /// <returns>true if succuessful</returns>
        bool AddEncodingType(EncodingTypeModel encoding);

        /// <summary>
        ///  Removes a encoding from configuration
        /// </summary>
        /// <param name="encoding">encoding to delete</param>
        /// <returns>true if successful</returns>
        bool RemoveEncodingType(EncodingTypeModel encoding);

        /// <summary>
        /// Adds a media service in admin configuration
        /// </summary>
        /// <param name="mediaService">media service to be added</param>
        /// <returns>true if successful</returns>
        bool AddMediaService(MediaServiceModel mediaService);

        /// <summary>
        /// Removes a media serivce from admin configuration
        /// </summary>
        /// <param name="mediaService">media service to be removed</param>
        /// <returns>true if successful</returns>
        bool RemoveMediaService(MediaServiceModel mediaService);
    }
   
}
