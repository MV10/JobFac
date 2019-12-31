using System;

namespace JobFac.Library
{
    public static class Formatting
    {
        public static string NewInstanceKey { get => Guid.NewGuid().ToString("D"); }
        public static string FilenameTimestampUtcNow { get => DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss_UTC"); }

        public static bool ValidateInstanceKey(string instanceKey)
        => Guid.TryParse(instanceKey, out var _);

        public static string FormattedInstanceKey(string instanceKey)
        {
            if (Guid.TryParse(instanceKey, out var key))
                return key.ToString("D");

            return string.Empty;
        }
    }
}
