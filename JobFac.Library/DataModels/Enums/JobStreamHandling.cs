
namespace JobFac.Library.DataModels
{
    public enum JobStreamHandling
    {
        None = 0,
        Database = 1,
        // file-based options must come after Database, see Enum extension
        OverwriteFile = 2,
        AppendFile = 3,
        TimestampedFile = 4
    }
}
