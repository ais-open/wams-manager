namespace Ais.Internal.Dcm.ModernUIV2.Common
{
    /// <summary>
    /// Represents File upload status
    /// </summary>
    public class UploadFileStatusInfo
    {
        /// <summary>
        /// Asset Id that is associated with the file being uploaded
        /// </summary>
        public string AssetId { get; set; }
        /// <summary>
        /// Name of the file
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Percentage of file blocks uploaded so far
        /// </summary>
        public int UploadPercentage { get; set; }
    }
}
