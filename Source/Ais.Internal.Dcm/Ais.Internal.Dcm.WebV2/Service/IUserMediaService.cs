using Ais.Internal.Dcm.Business;
using Ais.Internal.Dcm.Web.Models;
using AzurePatterns.Entity;
using System.Collections.Generic;

namespace Ais.Internal.Dcm.Web.Service
{
    public interface IUserMediaService
    {
        /// <summary>
        /// Returns all media service accessible by a client
        /// </summary>
        /// <param name="clientKey">key that uniquely identifies a client</param>
        /// <returns>list of media service information</returns>
        List<MediaServiceModel> GetAllMediaServices(string clientKey);

        /// <summary>
        /// Return all original assets managed under a media service
        /// </summary>
        /// <param name="mediaServiceName">media service name</param>
        /// <returns>list of asset information</returns>
        List<AssetInfoModel> GetAllAssets(string mediaServiceName);
        
        /// <summary>
        /// Return all original assets
        /// </summary
        List<AssetInfoModel> GetAllAssets();


        /// <summary>
        /// Creates new asset
        /// </summary>
        /// <param name="mediaServiceName">media service under which the asset being managed</param>
        /// <param name="assetName">name of new asset</param>
        /// <returns>information of newly created asset</returns>
        AssetInfoModel CreateNewAsset(string mediaServiceName, string assetName);
        
        /// <summary>
        /// Gives a secured access url for upload operation for an asset
        /// </summary>
        /// <param name="mediaServiceName">media service under which the asset being managed</param>
        /// <param name="assetId">id of the asset for which the upload is requested</param>
        /// <returns>secure url string</returns>
        string GetUploadSasUrl(string mediaServiceName, string assetId);
        
        /// <summary>
        /// Returns true if a job is pending for the given asset
        /// </summary>
        /// <param name="mediaServiceName">media service under which the asset being managed</param>
        /// <param name="assetId">id of the asset</param>
        /// <returns>true if pending else false</returns>
        bool IsAJobPending(string mediaServiceName, string assetId);
        
        /// <summary>
        /// Get Thumbnail information of an asset
        /// </summary>
        /// <param name="assetId">id of the asset</param>
        /// <returns>list of thumbnail information</returns>
        List<ThumbnailAssetModel> GetThumbnailUrls(string assetId);
        
        /// <summary>
        /// Get output Encoding information of an asset
        /// </summary>
        /// <param name="mediaServiceName">media service under which the asset being managed</param>
        /// <param name="assetId">id of the asset</param>
        /// <returns>list of encoding information</returns>
        List<AssetWithFilesModel> GetOutputAssetFiles(string mediaServiceName, string assetId);

        /// <summary>
        ///  Returns all output from metadata system
        /// </summary>
        /// <returns></returns>
        List<AssetWithFilesModel> GetOutputAssetFiles();
        /// <summary>
        /// Returns files associated with an asset
        /// </summary>
        /// <param name="mediaServiceName">media service under which the asset being managed</param>
        /// <param name="assetId">id of the asset</param>
        /// <returns>list of file information</returns>
        List<AssetFileInfo> GetAssetFiles(string mediaServiceName, string assetId);

        /// <summary>
        /// Returns all the asset files files associated with an asset
        /// </summary>
        /// <returns></returns>
        List<AzurePatterns.Entity.AssetFileInfo> GetAllAssetFiles();
        
        /// <summary>
        ///  Returns files associated with an asset
        /// </summary>
        /// <param name="mediaServiceName">media service under which the asset being managed</param>
        /// <param name="assetId">id of the asset</param>
        /// <returns>list of file information</returns>
        List<AssetFileInfo> GetAssetFilesContext(string mediaServiceName, string assetId);
        
        /// <summary>
        /// Returns the encoding types supported by a given media service
        /// </summary>
        /// <param name="mediaServiceName">name of the media service</param>
        /// <returns>list of encoding information</returns>
        List<EncodingTypeModel> GetEncodingTypes(string mediaServiceName);
        
        /// <summary>
        /// Generates file metadata of an asset 
        /// </summary>
        /// <param name="mediaServiceName">media service which manages the asset</param>
        /// <param name="assetId">id of the asset</param>
        /// <returns>true if successful, else false</returns>
        bool GenerateFileMetaData(string mediaServiceName, string assetId);

        /// <summary>
        /// Marks a given file as 'primary' file under the asset
        /// </summary>
        /// <param name="mediaServiceName">name of the media service</param>
        /// <param name="assetId">id of the asset</param>
        /// <param name="assetFileId">id of the file</param>
        /// <returns>true if successful</returns>
        bool MakePrimaryFile(string mediaServiceName, string assetId, string assetFileId);
        
        /// <summary>
        /// Sends request to media service to run encoding jobs on a given asset
        /// </summary>
        /// <param name="mediaServiceName">name of the media service</param>
        /// <param name="assetId">id of the asset</param>
        /// <param name="enStrings">encoding to run</param>
        /// <returns>Encoding Job Id</returns>
        string InitiateJob(string mediaServiceName, string assetId, string enStrings, string enFriendlyNames);
        
        /// <summary>
        /// Sends request to media service to run thumbnail job on a given asset
        /// </summary>
        /// <param name="mediaServiceName">name of the media service</param>
        /// <param name="assetId">id of the asset</param>
        /// <param name="thumbnailSettings">thumbnail settings</param>
        /// <returns>Thumbnail job id</returns>
        string InitiateThumbnailJob(string mediaServiceName, string assetId, string thumbnailSettings);
        
        /// <summary>
        /// Syncs the metadata of a given media service
        /// </summary>
        /// <param name="mediaServiceName">name of the media Service</param>
        void Sync(string mediaServiceName);
        
        /// <summary>
        /// Returns the status of jobs on a given asset
        /// </summary>
        /// <param name="assetId">id of the asset</param>
        /// <returns>list of job information</returns>
        List<JobInfo> GetJobStatus(string assetId);
        
        /// <summary>
        /// Sends a request to update the metadata of a given media service
        /// </summary>
        /// <param name="mediaServiceName">name of the media service</param>
        void UpdateMediaService(string mediaServiceName);

        /// <summary>
        /// Deletes the specified entity from metadata
        /// </summary>
        /// <param name="assetEntity"></param>
        void DeleteAssetEntity(AssetEntity assetEntity);

        /// <summary>
        /// Search for an asset in metadata
        /// </summary>
        /// <param name="filterExpression"></param>
        List<AssetInfoModel> FindAssetEntity(string filterExpression);

        /// <summary>
        /// Search for an assetfile in metadata
        /// </summary>
        /// <param name="filterExpression"></param>
        List<AssetFileInfo> FindAssetFileEntity(string filterExpression);
        
        /// <summary>
        /// Search for an Asset output in metadata
        /// </summary>
        /// <param name="filterExpression"></param>
        List<AssetWithFiles> FindAssetOutputEntity(string filterExpression);

        Tag CreateTag(string mediaServiceName, string tagName);

        void UpdateAssetWithTags(string mediaServiceName, string assetId, string tags);

        List<Tag> GetTags(string mediaServiceName);

        void UpdateAssetFileWithTags(string mediaServiceName, string parentAssetId, string assetFileId, string tags);

        List<Tag> GetAssetFileTags(string assetFileId);

        SearchData SearchMedia(string searchString, int rowsToSkip, int rowsToRetrieve, int searchType);
    }
}
