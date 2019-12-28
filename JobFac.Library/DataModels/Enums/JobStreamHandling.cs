
namespace JobFac.Library.DataModels
{
    public enum JobStreamHandling
    {
        None,
        Database,
        // file-based options must come after Database, see Enum extension
        OverwriteFile,
        AppendFile,
        TimestampedFile
    }
}
