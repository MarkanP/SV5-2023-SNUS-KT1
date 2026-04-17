using KT1.infrastructure;
using KT1.model;
using KT1.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT1.core
{
    public class ProcessingSystem
    {
        private readonly ThreadSafePriorityQueue<Job> _queue;
        private readonly Dictionary<Guid, Job> _jobs = new Dictionary<Guid, Job>();
        private readonly Dictionary<Guid, TaskCompletionSource<int>> _handles = new Dictionary<Guid, TaskCompletionSource<int>>();
        private readonly HashSet<Guid> _processed = new HashSet<Guid>();
        private readonly List<Worker> _workers = new List<Worker>();
        private readonly object _lock = new object();
        private readonly int _maxQueueSize;
        private readonly ReportService _report;
        public event Func<Job, int, Task> JobCompleted;
        public event Func<Job, Exception, Task> JobFailed;
        public event Func<Job, Exception, Task> JobAborted;
        public ProcessingSystem(int workerCount,  int maxQueueSize, ReportService report)
        {
            _maxQueueSize = maxQueueSize;
            _queue = new ThreadSafePriorityQueue<Job>();
            _report = report;
            for (int i = 0; i < workerCount; i++)
            {
                var worker = new Worker(this, new JobExecutor(), report);
                _workers.Add(worker);
                worker.Start();
            }

            _report = report;
        }

        public JobHandle Submit(Job job) {
            lock (_lock)
            {
                if (_queue.Count >= _maxQueueSize)
                {
                    throw new Exception("Queue is full");
                }
                if (_jobs.ContainsKey(job.Id))
                {
                    throw new Exception("Duplicate job");
                }
                _jobs[job.Id] = job;
                var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
                _handles[job.Id] = tcs;
                _queue.Enqueue(job, job.Priority);
                return new JobHandle
                {
                    Id = job.Id,
                    Result = tcs.Task,
                };
            }
        }

        public bool TryTake(out Job job)
        {
            return _queue.TryDequeue(out job);
        }

        public bool TryMarkProcessed(Job job)
        {
            lock (_lock)
            {
                if( _processed.Contains(job.Id))
                {
                    return false;
                }
                _processed.Add(job.Id);
                return true;
            }
        }
        public void SetResult(Job job, int result)
        {
            if (_handles.TryGetValue(job.Id, out var tcs))
            {
                tcs.TrySetResult(result);
            }
        }
        public void SetAborted(Job job, Exception ex)
        {
            if(_handles.TryGetValue(job.Id, out var tcs))
            {
                tcs.TrySetException(ex);
            }
        }
        public IEnumerable<Job> GetTopJobs(int n)
        {
            return _queue.Snapshot().OrderBy(j => j.Priority).Take(n).ToList();
        }
        public Job GetJob(Guid id)
        {
            lock (_lock)
            {
                return _jobs.TryGetValue(id, out var job) ? job : null;
            }
        }
        public async Task RaiseCompleted(Job job, int result)
        {
            if (JobCompleted != null)
            {
                await JobCompleted.Invoke(job, result);
            }
        }
        public async Task RaiseFailed(Job job, Exception ex)
        {
            if(JobFailed != null)
            {
                await JobFailed.Invoke(job, ex);
            }
        }
        public async Task RaiseAborted(Job job, Exception ex)
        {
            if (JobAborted != null)
            {
                await JobAborted.Invoke(job, ex);
            }
        }
    }
}
