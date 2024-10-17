using SaveScummerLib.Config;
using SaveScummerLib.Logging;

namespace SaveScummerLib.Files
{
    public class FileRepository : IFileRepository
    {
        private readonly IConfiguration m_config;
        private readonly ILogger m_logger;

        private readonly string m_backupPath;
        private readonly string m_savePath;

        private readonly IDictionary<string, string> m_trackedFiles;

        public FileRepository(IConfiguration config, ILogger logger)
        {
            m_config = config;
            m_logger = logger;
            m_savePath = config.SaveFolderLocation;
            m_backupPath = Path.Combine(m_savePath, "ScumBackup");
            m_trackedFiles = new Dictionary<string, string>();
        }

        public void BackupFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            m_logger.Log($"Creating backup for file: {fileName}");

            EnsureBackupDir();

            var targetPath = CopyFile(filePath, m_backupPath);
            m_trackedFiles[fileName] = targetPath;
        }

        public void RestoreFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            m_logger.Log($"Restoring file from backup: {fileName}");

            // Find file to restore
            if (!m_trackedFiles.TryGetValue(fileName, out string? backupPath))
            {
                m_logger.Log("Unable to find file in backup location.");
                return;
            }

            EnsureBackupDir();
            CopyFile(backupPath, m_savePath);
        }

        public IEnumerable<string> CreateFileStore()
        {
            m_logger.Log("Creating a backup of the save folder...");
            // Deletes entire backup folder to prevent file name collisions.
            if (Directory.Exists(m_backupPath))
            {
                m_logger.Log("Deleting old backup folder...");
                Directory.Delete(m_backupPath, true);
            }

            EnsureBackupDir();
            var extensionFilter = Utils.StringUtils.NormaliseExtension(m_config.FileExtension);
            var existingFiles = Directory.EnumerateFiles(m_config.SaveFolderLocation, extensionFilter, SearchOption.TopDirectoryOnly);

            foreach (var file in existingFiles)
            {
                var targetPath = CopyFile(file, m_backupPath);
                m_trackedFiles.Add(Path.GetFileName(file), targetPath);
            }

            m_logger.Log($"Backed up {existingFiles.Count()} files.");
            return existingFiles;
        }

        public DateTime GetLastWriteTimeUTC(string filePath)
            => File.GetLastWriteTimeUtc(filePath);

        private static string CopyFile(string filePath, string destinationFolder)
        {
            var fileName = Path.GetFileName(filePath);
            var destinationFilePath = Path.Combine(destinationFolder, fileName);

            File.Copy(filePath, destinationFilePath, true);
            return destinationFilePath;
        }

        private void EnsureBackupDir()
        {
            if (!Directory.Exists(m_backupPath))
            {
                Directory.CreateDirectory(m_backupPath);
            }
        }
    }
}
