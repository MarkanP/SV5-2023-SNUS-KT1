using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT1.infrastructure
{
    public class Logger
    {
        private readonly string _filePath = "log.txt";
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public async Task Log(string message)
        {
            await _semaphore.WaitAsync();
            try
            {
                await File.AppendAllTextAsync(_filePath, message + Environment.NewLine);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
