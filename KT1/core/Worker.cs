using KT1.service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KT1.core
{
    public class Worker
    {
        private readonly ProcessingSystem _system;
        private readonly JobExecutor _executor;
        private readonly ReportService _report;
        public Worker(ProcessingSystem system, JobExecutor executor, ReportService report)
        {
            _system = system;
            _executor = executor;
            _report = report;
        }
        public void Start()
        {
            Task.Run(Loop);
        }
        private async Task Loop()
        {
            while (true)
            {
                if(!_system.TryTake(out var job))
                {
                    continue;
                }
                if (!_system.TryMarkProcessed(job))
                {
                    continue;
                }
                int retries = 0;
                var sw = Stopwatch.StartNew();
                while (retries < 3)
                {
                    try
                    {
                        var cts = new CancellationTokenSource(2000);
                        var task = _executor.Execute(job);
                        var result = await task.WaitAsync(cts.Token);
                        sw.Stop();
                        _system.SetResult(job, result);
                        await _system.RaiseCompleted(job, result);
                        _report.Record(job, sw.Elapsed, true);
                        break;
                    }
                    catch (Exception ex)
                    {
                        retries++;
                        
                        if (retries == 3)
                        {
                            sw.Stop();
                            _system.SetAborted(job, ex);
                            await _system.RaiseAborted(job, ex);
                            _report.Record(job, sw.Elapsed, false);
                        }
                        else
                        {
                            await _system.RaiseFailed(job, ex);
                        }
                    }
                } 
            }
        }
    }
}
