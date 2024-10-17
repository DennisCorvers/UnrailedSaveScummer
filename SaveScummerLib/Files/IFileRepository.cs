namespace SaveScummerLib.Files
{
    public interface IFileRepository
    {
        void BackupFile(string filePath);

        void RestoreFile(string fileName);

        IEnumerable<string> CreateFileStore();

        DateTime GetLastWriteTimeUTC(string filePath);
    }
}
