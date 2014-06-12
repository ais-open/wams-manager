namespace Ais.Internal.Dcm.Business
{
    public class AcsToken
    {
        public string token_type
        {
            get;
            set;
        }

        public string access_token
        {
            get;
            set;
        }

        public int expires_in
        {
            get;
            set;
        }

        public string scope
        {
            get;
            set;
        }
    }
    /// <summary>
    /// Values for various request headers
    /// </summary>
    internal static class RequestHeaderValues
    {
        /// <summary>
        /// DataServiceVersion request header (3.0)
        /// </summary>
        internal const string DataServiceVersion = "3.0";

        /// <summary>
        /// MaxDataServiceVersion request header (3.0)
        /// </summary>
        internal const string MaxDataServiceVersion = "3.0";

        /// <summary>
        /// x-ms-version request header (2.0)
        /// </summary>
        internal const string XMsVersion = "2.1";

        /// <summary>
        /// Authorization request header format
        /// </summary>
        internal const string Authorization = "Bearer {0}";

    }

    /// <summary>
    /// Request header names.
    /// </summary>
    internal static class RequestHeaders
    {
        /// <summary>
        /// DataServiceVersion request header
        /// </summary>
        internal const string DataServiceVersion = "DataServiceVersion";

        /// <summary>
        /// MaxDataServiceVersion request header
        /// </summary>
        internal const string MaxDataServiceVersion = "MaxDataServiceVersion";

        /// <summary>
        /// x-ms-version request header
        /// </summary>
        internal const string XMsVersion = "x-ms-version";

        /// <summary>
        /// x-ms-date request header
        /// </summary>
        internal const string XMsDate = "x-ms-date";

        /// <summary>
        /// x-ms-blob-type request header
        /// </summary>
        internal const string XMsBLOBType = "x-ms-blob-type";

        /// <summary>
        /// host request header
        /// </summary>
        internal const string Host = "Host";

        /// <summary>
        /// Authorization request header
        /// </summary>
        internal const string Authorization = "Authorization";
    }

    /// <summary>
    /// HTTP Verbs
    /// </summary>
    internal static class HttpVerbs
    {
        /// <summary>
        /// POST HTTP verb
        /// </summary>
        internal const string Post = "POST";

        /// <summary>
        /// GET HTTP verb
        /// </summary>
        internal const string Get = "GET";

        /// <summary>
        /// MERGE HTTP verb
        /// </summary>
        internal const string Merge = "MERGE";

        /// <summary>
        /// DELETE HTTP verb
        /// </summary>
        internal const string Delete = "DELETE";

        /// <summary>
        /// PUT HTTP verb
        /// </summary>
        internal const string Put = "PUT";
    }

    internal static class RequestContentType
    {
        internal const string Json = "application/json;odata=verbose";

        internal const string Atom = "application/atom+xml";
    }




    /// <summary>
    /// Enumeration indicating asset state.
    /// Please note that version 2.0, Publish Asset action is deprecated. Hence it is removed from here.
    /// </summary>
    public enum AssetState
    {
        Initialized = 0,
        //Published = 1,
        Deleted = 1,
    }

    /// <summary>
    /// Enumeration for asset encryption options.
    /// </summary>
    public enum AssetEncryptionOption
    {
        None = 0,
        StorageEncrypted = 1,
        CommonEncryptionProtected = 2,
    }

    public static class Literals
    {
        public const string CHILD_ASSET_APPEND = "CHLD-ASST";
        public const string THUMBNAIL = "TMBNL";
        public const string ENCODING = "ENCD";
        public const string UPLOAD_POLICY_NAME = "AIS-DCM_UPLOAD_POLICY";
        public const string DOWNLOAD_POLICY_NAME = "AIS-DCM_DOWNLOAD_POLICY";
        public const string DELETE_POLICY_NAME = "AIS-DCM_DELETE_POLICY";
        public const int DURATION_IN_MINUTES = 525600; //one year
        public const string  TEMP_ASSET = "TEMP-ASST";

        // Mobile Services REST API
        internal const string X_ZUMO_APPLICATION = "X-ZUMO-APPLICATION";

        internal const string JSON_CONTENT_TYPE = "application/json";
    }
}
