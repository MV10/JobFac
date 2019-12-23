using System;
using System.Threading.Tasks;

namespace job.JobFac.unaware
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int seconds = 0;
            if (args.Length != 1 || !int.TryParse(args[0], out seconds))
            {
                Environment.ExitCode = -1;
                Console.WriteLine("JobFac-unaware sample requires one argument defining number of seconds to sleep.");
            }
            Console.WriteLine($"Sample sleeping for {seconds} secs");
            await Task.Delay(seconds * 1000);
            Environment.ExitCode = 12345;
            Console.WriteLine($"Sample set exit code {Environment.ExitCode}");
        }
    }
}