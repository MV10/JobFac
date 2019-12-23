using System.Threading;
using System.Threading.Tasks;

namespace System.Diagnostics
{
    public static class ProcessExtensions
    {
        public static async Task WaitForExitAsync(this Process process, CancellationToken token = default)
        {
            var completion = new TaskCompletionSource<bool>();
            process.EnableRaisingEvents = true;
            process.Exited += CompleteTaskOnExit;
            try
            {
                if (process.HasExited) return;
                using var reg = token.Register(() => Task.Run(() => completion.SetCanceled()));
                await completion.Task;
            }
            finally
            {
                process.Exited -= CompleteTaskOnExit;
            }

            void CompleteTaskOnExit(object s, EventArgs e)
                => Task.Run(() => completion.TrySetResult(true));
        }
    }
}
