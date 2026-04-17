using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT1.model
{
    public class Job
    {
        public Guid Id { get; set; }
        public JobType Type { get; set; }
        public string Payload { get; set; }
        public int Priority { get; set; }
    }
}
