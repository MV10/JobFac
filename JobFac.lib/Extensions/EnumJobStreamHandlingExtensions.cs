﻿
namespace JobFac.lib.DataModels
{
    public static class EnumJobStreamHandlingExtensions
    {
        public static bool IsFileBased(this JobStreamHandling handling)
            => handling > JobStreamHandling.Logger;

        public static bool IsTimestampedFile(this JobStreamHandling handling)
            => handling > JobStreamHandling.AppendFile;
    }
}
