using KT1.config;
using KT1.core;
using KT1.infrastructure;
using KT1.model;
using KT1.service;

class Program
{
    static async Task Main(string[] args)
    {
        var config = SystemConfigParser.Parse("SystemConfig.xml");

        var reportService = new ReportService();

        var system = new ProcessingSystem(
            config.WorkerCount,
            config.MaxQueueSize,
            reportService);

        reportService.Start();

        var logger = new Logger();

        system.JobCompleted += async (job, result) =>
        {
            await logger.Log($"[{DateTime.Now}] [COMPLETED] {job.Id}, {result}");
        };

        system.JobFailed += async (job, ex) =>
        {
            await logger.Log($"[{DateTime.Now}] [FAILED] {job.Id}, {ex.Message}");
        };

        system.JobAborted += async (job, ex) =>
        {
            await logger.Log($"[{DateTime.Now}] [ABORT] {job.Id}, {ex.Message}");
        };

        foreach (var job in config.InitialJobs)
        {
            try
            {
                system.Submit(job);
            }
            catch (Exception ex)
            {
                await logger.Log($"[{DateTime.Now}] [INIT_FAIL] {job.Id}, {ex.Message}");
            }
        }

        for (int i = 0; i < config.WorkerCount; i++)
        {
            Task.Run(async () =>
            {
                var rand = Random.Shared;

                while (true)
                {
                    var job = GenerateRandomJob(rand);

                    try
                    {
                        system.Submit(job);
                    }
                    catch (Exception ex) 
                    {
                        await logger.Log($"[{DateTime.Now}] [SUBMIT_FAIL] {ex.Message}");
                    }

                    await Task.Delay(rand.Next(100, 500));
                }
            });
        }
        var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        await Task.Delay(Timeout.Infinite, cts.Token);
    }

    static Job GenerateRandomJob(Random rand)
    {
        var type = rand.Next(2) == 0 ? JobType.Prime : JobType.IO;

        string payload = type switch
        {
            JobType.Prime => $"numbers:{rand.Next(1000, 5000)},threads:{rand.Next(1, 9)}",
            JobType.IO => $"delay:{rand.Next(100, 1000)}",
            _ => ""
        };
        return new Job
        {
            Id = Guid.NewGuid(),
            Type = type,
            Payload = payload,
            Priority = rand.Next(1, 10)
        };
    }
}