using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ais.Internal.Dcm.Business
{
    public class AssetFile
    {
        /// <summary>
        /// Asset File ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Asset File Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// File Size
        /// </summary>
        public long ContentFileSize { get; set; }
        public string ParentAssetId { get; set; }
        public string EncryptionVersion { get; set; }
        public string EncryptionScheme { get; set; }
        public string EncryptionKeyId { get; set; }
        public bool IsEncrypted { get; set; }
        public string InitializationVector { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime Created { get; set; }
        public string MimeType { get; set; }
        public string ContentChecksum { get; set; }
    }
}
