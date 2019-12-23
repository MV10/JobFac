using System.Threading;
using System.Threading.Tasks;

namespace System.Diagnostics
{
    public static class WaitForExitAsync
    {
        public static async Task Wait(Process proc, CancellationToken token = default)
        {
            var completion = new TaskCompletionSource<bool>();
            proc.EnableRaisingEvents = true;
            proc.Exited += CompleteTaskOnExit;
            try
            {
                if (proc.HasExited) return;
                using var reg = token.Register(() => Task.Run(() => completion.SetCanceled()));
                await completion.Task;
            }
            finally
            {
                proc.Exited -= CompleteTaskOnExit;
            }

            void CompleteTaskOnExit(object s, EventArgs e)
                => Task.Run(() => completion.TrySetResult(true));
        }
    }
}
