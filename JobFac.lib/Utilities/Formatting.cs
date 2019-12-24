using System;
using System.Collections.Generic;
using System.Text;

namespace JobFac.lib
{
    public static class Formatting
    {

        // TODO figure out best filename timestamp format
        public static string FilenameTimestampUtcNow { get => DateTimeOffset.UtcNow.ToString(); }

    }
}
