using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ais.Internal.Dcm.Business
{
    public class MediaTask
    {
        public string Id { get; private set; }
        public string Configuration { get; private set; }
        public DateTime EndTime { get; private set; }
        public DateTime ErrorDetails { get; private set; }
        public DateTime HistoricalEvents { get; private set; }
        public string MediaProcessorId { get; private set; }
        public string Name { get; private set; }
        public string PerfMessage { get; private set; }
        public int Priority { get; private set; }
        public double Progress { get; private set; }
        public double RunningDuration { get; private set; }
        public DateTime StartTime { get; private set; }
        public MediaTaskState State { get; private set; }
        public string TaskBody { get; private set; }
        public MediaTaskOptions Options { get; private set; }
        public string EncryptionKeyId { get; private set; }
        public string EncryptionScheme { get; private set; }
        public string EncryptionVersion { get; private set; }
        public string InitializationVector { get; private set; }
        public List<Asset> InputMediaAssets { get; private set; }
        public List<Asset> OutputMediaAssets { get; private set; }
    }

    public enum MediaTaskState
    {
        None = 0,

        Active = 1,

        Running = 2,

        Completed = 3
    }

    public enum MediaTaskOptions
    {
        None = 0,

        ProtectedConfiguration = 1
    }
}
