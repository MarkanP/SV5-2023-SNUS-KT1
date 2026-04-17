using KT1.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace KT1.config
{
    public class SystemConfigParser
    {
        public int WorkerCount { get; set; }
        public int MaxQueueSize { get; set; }
        public List<Job> InitialJobs { get; set; }

        public static SystemConfigParser Parse(string path)
        {
            var doc = XDocument.Load(path);
            var root = doc.Root;
            var workerCount = int.Parse(root.Element("WorkerCount").Value);
            var maxQueueSize = int.Parse(root.Element("MaxQueueSize").Value);
            var initialJobs = root.Element("Jobs").Elements("Job").Select(ParseJob).ToList();
            return new SystemConfigParser
            {
                WorkerCount = workerCount,
                MaxQueueSize = maxQueueSize,
                InitialJobs = initialJobs
            };
        }

        public static Job ParseJob(XElement element)
        {
            var type = Enum.Parse<JobType>(element.Attribute("Type").Value);
            var payload = element.Attribute("Payload").Value;
            var priority = int.Parse(element.Attribute("Priority").Value);
            return new Job
            {
                Id = Guid.NewGuid(),
                Type = type,
                Payload = payload,
                Priority = priority
            };
        }
    }
}
