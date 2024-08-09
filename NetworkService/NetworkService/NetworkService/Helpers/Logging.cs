using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Helpers
{
    public static class Logging
    {
        private static readonly object lockObject = new object(); // for deadlock avoidance

        public static void AppendToFile(string fileName, string data)
        {
            try
            {
                lock (lockObject)
                {
                    using (StreamWriter writer = File.AppendText(fileName))
                    {
                        writer.WriteLine($"{DateTime.Now} - {data}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file '{fileName}': {ex.Message}");
            }
        }
    }
}
