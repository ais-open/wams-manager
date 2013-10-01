using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ais.Internal.Dcm.ModernUIV2.Common
{
    /// <summary>
    /// Represents File upload request
    /// </summary>
    public class UploadFileRequest
    {
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
