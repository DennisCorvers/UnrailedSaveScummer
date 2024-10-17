namespace SaveScummerLib.Monitoring
{
    public interface IFileEventProcessor
    {
        void ProcessEvent(FileSystemEventArgs e);
    }
}