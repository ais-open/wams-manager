namespace Ais.Internal.Dcm.ModernUIV2.Common
{
    public class Literals
    {
        public const string URL_INIT_SEARCH = "api/media/InitializeSearch";

        public const string URL_LIST_MEDIASERVICE = "api/media/MediaServices?clientKey={0}";

        public const string URL_GET_ENCODING_TYPE = "api/media/EncodingTypes?mediaServiceName={0}";

        public const string URL_GET_ASSETS = "api/media/Assets?mediaServiceName={0}";

        public const string MESSAGE_ERROR_ABOUT = "Error while loading About  Window.";

        public const string MESSAGE_ERROR_MEDIA = "Error while loading Media Window.";

        public const string MESSAGE_ERROR_NAVIGATION_ASSET_PAGE = "Error while navigating to album page.";

        public const string MESSAGE_ERROR_NAVIGATION_MEDIASERVICE_PAGE = "Error while navigating to collection page.";

        public const string MESSAGE_ERROR_LOADING_ASSETS = "Error while loading albums.";

        public const string MESSAGE_ERROR_LOADING_ASSETS_PROPERTIES = "Error loading album.";

        public const string MESSAGE_ERROR_CREATING_ASSET = "Error creating album";

        public const string MESSAGE_ERROR_SEARCH = "Error searching video files";

        public const string MESSAGE_ERROR_NAVIGATION = "Navigation Error";

        public const string URL_GET_TAGS = "api/media/GetTags?mediaServiceName={0}";

        public const string MESSAGE_ERROR_THUMBNAIL_SETTINGS = "Error loading settings";
        public const string SETTINGS_MEDIA_FILE_TYPES = "MediaFiles (*.asf)|*.asf|(*.avi)|*.avi|(*.m2ts)|*.m2ts|(*.m2v)|*.m2v|(*.mp4)|*.mp4|(*.mpeg)|*.mpeg|(*.mpg)|*.mpg|(*.mts)|*.mts|(*.ts)|*.ts|(*.wmv)|*.wmv";
        public const string MESSAGE_ERROR_LOAD_CLOUD_FILE = "Error loading media files from album";
        public const string MESSAGE_WARNING_ONLY_UPLOAD = "This action will only upload the file. Are you sure you want to upload?";
        public const string MESSAGE_HEADER_UPLOAD_CONFIRMATION = "Upload confirmation";
        public const string URL_GET_ASSETFILES = "api/media/AssetFiles?mediaServiceName={0}&assetId={1}";

        public const string MESSAGE_INFO_CREATING_ASSET ="Creating album... please wait";

        public const string MESSAGE_INFO_HIDE_DETAIL = "Hide Detail";

        public const string MESSAGE_INFO_SHOW_DETAIL = "Show Detail";

        public const string MESSAGE_STATUS_UPLOAD_CHECKING_FILE = "Checking file";

        public const string MESSAGE_STATUS_UPLOAD_UPLOADING_FILE = "Uploading file..";

        public const string MESSAGE_STATUS_UPLOAD_UPLOADED_FILE = "File uploaded...";

        public const string URL_GET_THUMBNAILS = "api/media/Thumbnails?assetId={0}";

        public const string URL_GET_GROUPED_OUTPUT = "api/media/GetGroupedOutput?mediaServiceName={0}&assetId={1}";

        public const string URL_CREATE_ASSET = "api/media/CreateAsset?mediaServiceName={0}&assetName={1}";

        public const string MESSAGE_WARNING_ALREADY_JOB_RUNNING = "A job is already in progress.Please try again later.";

        public const string MESSAGE_HEADER_SERVICE_INFO = "Service Information";

        public const string MESSAGE_STATUS_UPLOAD_GENERATING_THUMBNAIL = "Generating Thumbnails";

        public const string MESSAGE_STATUS_UPLOAD_PROCESSING_ENCODING ="Processing Encoding";

        public const string MESSAGE_STATUS_UPLOAD_COMPLETED_ENCODING = "Completed Encoding";

        public const string URL_UPDATE_ASSET_TAG = "api/media/UpdateAssetTag?mediaServiceName={0}&assetId={1}&tagsUpdateString={2}";

        public const string URL_UPDATE_FILE_TAG = "api/media/UpdateAssetFileTag?mediaServiceName={0}&assetId={1}&assetFileId={2}&tagsUpdateString={3}";

        public const string URL_GET_JOB_STATUS = "api/media/IsAJobPending?mediaServiceName={0}&assetId={1}";

        public const string URL_INIT_THUMB_JOB = "api/media/InitiateThumbnailJob?mediaServiceName={0}&assetId={1}&enStrings={2}";

        public const string URl_INIT_JOB = "api/media/InitiateJob?mediaServiceName={0}&assetId={1}&enStrings={2}&enFriendlyNames={3}";

        public const string MESSAGE_ERROR_UPDATING_TAG = "Error updating Tag";

        public const string URL_ASSET_FILE_CONTEXT = "api/media/AssetFilesContext?mediaServiceName={0}&assetId={1}";

        public const string URL_MAKE_FILE_PRIMARY = "api/media/MakePrimary?mediaServiceName={0}&assetId={1}&assetFileId={2}&thumbnailJobSelected={3}";
        public const string MESSAGE_ERROR_ENCODING_ERROR = "Error while processing encoding for file.";

        public const string MESSAGE_ERROR_JOB_STATUS ="Error while checking for pending status for a job";

        public const string URL_GENERATE_FILE_METADATA = "api/media/GenFileMetadata?mediaServiceName={0}&assetId={1}";

        public const string URL_GET_SAS_URL_UPLOAD = "api/media/GetUploadSasUrl?mediaServiceName={0}&assetId={1}";

        public const string MESSAGE_HEADER_ERROR = "Error";

        public const string MESSAGE_WARNING_MUST_SELECT_THUMBNAIL_SETTING ="You must select an encoding or thumbnail setting.";
    }
}
