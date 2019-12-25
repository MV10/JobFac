using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace JobFac.Services.Runner
{
    public static class KillCommand
    {
        // If this task exits, that means the Job service connected to this named pipe to request a process-kill
        public static async Task MonitorNamedPipe(string jobKey, CancellationToken token)
        {
            NamedPipeServerStream server = null;
            try
            {
                server = new NamedPipeServerStream(jobKey, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await server.WaitForConnectionAsync(token);
                server.Disconnect();
            }
            finally
            {
                server?.Dispose();
            }
        }
    }
}
