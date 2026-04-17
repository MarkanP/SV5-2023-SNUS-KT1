using KT1.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT1.core
{
    public interface IProcessingSystem
    {
        public JobHandle Submit(Job job);
        public IEnumerable<Job> GetTopJobs(int n);
        public Job GetJob(Guid id);
        event Func<Job, int, Task> JobCompleted;
        event Func<Job, Exception, Task> JobFailed;
        event Func<Job, Exception, Task> JobAborted;
    }
}
