using Microsoft.Extensions.Logging;
using System;

namespace JobFac.Library.Logging
{
    public struct LogMessage
    {
        public DateTimeOffset Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
    }
}
