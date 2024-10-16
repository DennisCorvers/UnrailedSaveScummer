using SaveScummerLib.Config;
using SaveScummerLib.Files;
using SaveScummerLib.Logging;

namespace SaveScummerLib.Monitoring
{
    public class FileMonitor : IFileMonitor
    {
        private readonly ILogger m_logger;
        private readonly FileSystemWatcher m_watcher;
        private readonly IFileRepository m_repository;

        public FileMonitor(IConfiguration config, ILogger logger, IFileRepository fileRepository)
        {
            m_logger = logger;
            m_repository = fileRepository;

            if (!Directory.Exists(config.SaveFolderLocation))
            {
                throw new InvalidOperationException("Provided save folder location does not exist.");
            }

            var extension = Utils.StringUtils.NormaliseExtension(config.FileExtension);
            m_watcher = new FileSystemWatcher(config.SaveFolderLocation, extension)
            {
                NotifyFilter = NotifyFilters.FileName,
                IncludeSubdirectories = false,
            };
            m_watcher.Deleted += OnFileDeleted;
            m_watcher.Created += OnFileCreated;
        }

        ~FileMonitor()
        {
            Dispose();
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            m_logger.Log($"File deletion detected: {e.Name}");
            m_repository.RestoreFile(e.FullPath);
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            // When a backup is restored, this event is triggered.
            // This event also gets triggered when a new file is created.
            // We want to make sure the created file is a new file, and not a restored backup.
            if (m_repository.VerifyIsNewFile(e.FullPath))
            {
                m_logger.Log($"File creation detected: {e.Name}");
                m_repository.BackupFile(e.FullPath);
            }
        }

        public void Dispose()
        {
            if (m_watcher != null)
            {
                GC.SuppressFinalize(this);
                m_watcher.EnableRaisingEvents = false;
                m_watcher.Dispose();
            }
        }

        public void StartMonitoring()
        {
            m_watcher.EnableRaisingEvents = true;
        }
    }
}
