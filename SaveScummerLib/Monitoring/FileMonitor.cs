using SaveScummerLib.Config;
using SaveScummerLib.Files;
using SaveScummerLib.Logging;

namespace SaveScummerLib.Monitoring
{
    public class FileMonitor : IFileMonitor
    {
        private static readonly DateTime DefaultFileAccessTime = new(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly ILogger m_logger;
        private readonly FileSystemWatcher m_watcher;
        private readonly IFileRepository m_repository;
        private readonly IDictionary<string, DateTime> m_fileLastChangeTime;

        public FileMonitor(IConfiguration config, ILogger logger, IFileRepository fileRepository)
        {
            m_logger = logger;
            m_repository = fileRepository;

            if (!Directory.Exists(config.SaveFolderLocation))
            {
                throw new InvalidOperationException("Provided save folder location does not exist.");
            }

            m_fileLastChangeTime = new Dictionary<string, DateTime>();

            var extension = Utils.StringUtils.NormaliseExtension(config.FileExtension);
            m_watcher = new FileSystemWatcher(config.SaveFolderLocation, extension)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                IncludeSubdirectories = false,
            };
            m_watcher.Deleted += OnFileDeleted;
            m_watcher.Created += OnFileCreated;
            m_watcher.Changed += OnFileChanged;
        }

        ~FileMonitor()
        {
            Dispose();
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            CanHandleFile(e);
            m_logger.Log($"File deletion detected: {e.Name}");
            m_repository.RestoreFile(e.FullPath);
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (CanHandleFile(e))
            {
                m_logger.Log($"File creation detected: {e.Name}");
                m_repository.BackupFile(e.FullPath);
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (CanHandleFile(e))
            {
                m_logger.Log($"File change detected: {e.Name}");
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

        private bool CanHandleFile(FileSystemEventArgs e)
        {
            var fileName = e.Name!;

            lock (m_fileLastChangeTime)
            {
                var fileAccessTime = File.GetLastWriteTimeUtc(e.FullPath);

                // Target file does not exist (deleted file)
                if (fileAccessTime == DefaultFileAccessTime)
                {
                    m_fileLastChangeTime[fileName] = DateTime.UtcNow;
                    return true;
                }

                // File has not yet been handled.
                if (!m_fileLastChangeTime.TryGetValue(fileName, out var cachedAccessTime))
                {
                    m_fileLastChangeTime[fileName] = DateTime.UtcNow;
                    return true;
                }

                if (fileAccessTime > cachedAccessTime)
                {
                    m_fileLastChangeTime[fileName] = fileAccessTime;
                    return true;
                }
            }

            return false;
        }
    }
}
