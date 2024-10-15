using SaveScummerLib.Config;
using SaveScummerLib.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveScummerLib.Files
{
    internal class FileRepository : IFileRepository
    {
        private readonly IConfiguration m_config;
        private readonly ILogger m_logger;

        private readonly string m_backupPath;

        public FileRepository(IConfiguration config, ILogger logger)
        {
            m_config = config;
            m_logger = logger;
            m_backupPath = Path.Combine(config.SaveFolderLocation, "ScumBackup");
        }

        public void BackupFile(string filePath)
        {
            m_logger.Log($"Creating backup for file: {Path.GetFileName(filePath)}");
            EnsureBackupDir();
            CopyFile(filePath, m_backupPath);
        }

        public void RestoreFile(string filePath)
        {
            m_logger.Log($"Restoring file from backup: {Path.GetFileName(filePath)}");
            EnsureBackupDir();
            CopyFile(filePath, m_config.SaveFolderLocation);
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
            var existingFiles = Directory.EnumerateFiles(m_backupPath, extensionFilter, SearchOption.TopDirectoryOnly);

            foreach (var file in existingFiles)
            {
                CopyFile(file, m_backupPath);
            }

            m_logger.Log($"Backed up {existingFiles.Count()} files.");
            return existingFiles;
        }

        private static void CopyFile(string filePath, string destinationFolder)
        {
            var fileName = Path.GetFileName(filePath);
            var destinationFilePath = Path.Combine(destinationFolder, fileName);

            File.Copy(filePath, destinationFilePath, true);
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
