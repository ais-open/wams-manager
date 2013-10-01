using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ais.Internal.Dcm.ModernUIV2.ViewModels
{
    public class JobInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }
        public JobState State { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
