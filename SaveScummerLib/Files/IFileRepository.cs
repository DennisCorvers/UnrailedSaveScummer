namespace SaveScummerLib.Files
{
    public interface IFileRepository
    {
        void BackupFile(string filePath);

        void RestoreFile(string fileName);

        IEnumerable<string> CreateFileStore();

        bool VerifyIsNewFile(string filePath);
    }
}
