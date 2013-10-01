using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ais.Internal.Dcm.ModernUIV2.ViewModels
{
    public enum JobState
    {
        Queued = 0,

        Scheduled = 1,

        Processing = 2,

        Finished = 3,

        Error = 4,

        Canceled = 5,

        Canceling = 6
    }
}
