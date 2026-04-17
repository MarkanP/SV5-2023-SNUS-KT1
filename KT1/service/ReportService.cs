using KT1.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KT1.service
{
    public class ReportService
    {
        private readonly List<JobExecutionInfo> _history = new();
        private readonly object _lock = new();
        private int _reportIndex = 0;

        public void Record(Job job, TimeSpan duration, bool success)
        {
            lock (_lock)
            {
                _history.Add(new JobExecutionInfo
                {
                    Type = job.Type,
                    Duration = duration,
                    Success = success
                });
            }
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromMinutes(1));
                    GenerateReport();
                }
            });
        }

        private void GenerateReport()
        {
            List<JobExecutionInfo> snapshot;

            lock (_lock)
            {
                snapshot = _history.ToList();
            }

            var grouped = snapshot.GroupBy(x => x.Type);

            var report = new XElement("Report",
                grouped.Select(g => new XElement("JobType",
                    new XAttribute("Type", g.Key),

                    new XElement("TotalExecuted", g.Count(x => x.Success)),
                    new XElement("AverageTime",
                        g.Where(x => x.Success)
                         .DefaultIfEmpty()
                         .Average(x => x == null ? 0 : x.Duration.TotalMilliseconds)
                    ),
                    new XElement("FailedCount",
                        g.Count(x => !x.Success))
                ))
            );

            var fileName = $"report_{_reportIndex % 10}.xml";
            report.Save(fileName);

            _reportIndex++;
        }
    }
}
