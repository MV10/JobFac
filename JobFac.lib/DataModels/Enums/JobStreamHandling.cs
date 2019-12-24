
namespace JobFac.lib.DataModels
{
    public enum JobStreamHandling
    {
        None,
        Database,
        Logger,
        // file-based options must come after Logger, see Enum extension
        OverwriteFile,
        AppendFile,
        // timestamp options must come after AppendFile, see Enum extension
        OverwriteFileWithTimestamp,
        AppendFileWithTimestamp
    }
}
