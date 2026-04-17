using KT1.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT1.service
{
    public class JobExecutor
    {
        public Task<int> Execute(Job job)
        {
            return job.Type switch
            {
                JobType.Prime => Task.Run(() => ProcessPrime(job.Payload)),
                JobType.IO => ProcessIO(job.Payload),
                _ => throw new NotSupportedException()
            };
        }
        private int ProcessPrime(string payload)
        {
            var (n, threads) = JobParser.ParsePrime(payload);
            int count = 0;
            Parallel.For(2, n + 1,
                new ParallelOptions { MaxDegreeOfParallelism = threads },
                i =>
                {
                    if (IsPrime(i))
                    {
                        Interlocked.Increment(ref count);
                    }
                });
            return count;
        }

        private bool IsPrime(int n)
        {
            if (n < 2)
            {
                return false;
            }
            for (int i = 2; i * i <= n; i++)
            {
                if(n % i == 0)
                {
                    return false;
                }
            }
            return true;
        }

        private async Task<int> ProcessIO(string payload)
        {
            await Task.Delay(JobParser.ParseIO(payload));
            return Random.Shared.Next(0, 101);
        }
    }
}
