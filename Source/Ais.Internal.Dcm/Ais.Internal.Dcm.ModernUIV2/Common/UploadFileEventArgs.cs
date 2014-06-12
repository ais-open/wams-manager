using System;
using System.Collections.Generic;

namespace Ais.Internal.Dcm.ModernUIV2.Common
{
    public class UploadFileEventArgs : EventArgs
    {
        /// <summary>
        /// Status of files being uploaded
        /// </summary>
        public List<UploadFileStatusInfo> FileStatus { get; set; }
    }
}
