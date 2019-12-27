﻿using System;

namespace JobFac.Library
{
    public static class Formatting
    {
        public static string NewInstanceKey { get => Guid.NewGuid().ToString("D"); }
        public static string FilenameTimestampUtcNow { get => DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss_UTC"); }
    }
}
