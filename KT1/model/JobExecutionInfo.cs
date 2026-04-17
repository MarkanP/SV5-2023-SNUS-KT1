using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT1.model
{
    public class JobExecutionInfo
    {
        public JobType Type { get; set; }
        public TimeSpan Duration { get; set; }
        public bool Success { get; set; }
    }
}
