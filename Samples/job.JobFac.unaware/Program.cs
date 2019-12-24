using System;
using System.Threading.Tasks;

namespace job.JobFac.unaware
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int seconds = 0;
            if (args.Length != 1 || !int.TryParse(args[0], out seconds) || seconds < 1)
            {
                Environment.ExitCode = -1;
                Console.Error.WriteLine("JobFac-unaware sample requires an argument defining number of seconds to sleep.");
            }

            Console.WriteLine($"Sample sleeping for {seconds} secs in 5 second increments.");

            var exitAt = DateTimeOffset.Now.AddSeconds(seconds);
            int counter = 0;
            while(exitAt > DateTimeOffset.Now)
            {
                Console.WriteLine($"\nSleep interval #{++counter}");
                for(int i = 0; i < 5; i++)
                {
                    Console.Write(".");
                    await Task.Delay(1000);
                }
            }

            Console.WriteLine("\n\nWriting \"OK\" to stderr.");
            Console.Error.WriteLine("OK"); // also emitted to stdout

            Environment.ExitCode = 12345;
            Console.WriteLine($"\nSample set exit code {Environment.ExitCode}");
        }
    }
}