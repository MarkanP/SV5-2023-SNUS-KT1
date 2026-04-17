using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT1.service
{
    public static class JobParser
    {
        public static (int n, int threads) ParsePrime(string payload)
        {
            string[] parts = payload.Split(",");
            int n = int.Parse(parts[0].Split(":")[1].Replace("_", ""));
            int threads = Math.Clamp(int.Parse(parts[1].Split(":")[1]), 1, 8);
            return (n, threads);
        }

        public static int ParseIO(string payload) {
            return int.Parse(payload.Split(":")[1].Replace("_", ""));
        }
    }
}
