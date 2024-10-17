using SaveScummerLib.Files;
using SaveScummerLib.Logging;

namespace SaveScummerLib.Monitoring
{
    public class FileEventProcessor : IFileEventProcessor
    {
        private static readonly DateTime DefaultFileAccessTime = new(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly IDictionary<string, DateTime> m_fileLastChangeTime;
        private readonly IFileRepository m_fileRepository;
        private readonly ILogger m_logger;

        public FileEventProcessor(ILogger logger, IFileRepository fileRepository)
        {
            m_logger = logger;
            m_fileRepository = fileRepository;
            m_fileLastChangeTime = new Dictionary<string, DateTime>();
        }

        public void ProcessEvent(FileSystemEventArgs e)
        {
            var eventType = e.ChangeType;
            var fileName = e.Name ?? throw new Exception("FileMonitor reported invalid file name.");
            var filePath = e.FullPath;

            if (!IsNewEvent(e))
            {
                return;
            }

            switch (eventType)
            {
                case WatcherChangeTypes.Created:
                    OnFileCreated(fileName, filePath); return;
                case WatcherChangeTypes.Deleted:
                    OnFileDeleted(fileName, filePath); return;
                case WatcherChangeTypes.Changed:
                    OnFileChanged(fileName, filePath); return;
                default:
                    break;
            }
        }

        private void OnFileCreated(string fileName, string filePath)
        {
            m_logger.Log($"File creation detected: {fileName}");
            m_fileRepository.BackupFile(filePath);
        }

        private void OnFileDeleted(string fileName, string filePath)
        {
            m_logger.Log($"File deletion detected: {fileName}");
            m_fileRepository.RestoreFile(filePath);
        }

        private void OnFileChanged(string fileName, string filePath)
        {
            m_logger.Log($"File change detected: {fileName}");
            m_fileRepository.BackupFile(filePath);
        }

        /// <summary>
        /// Verifies if the passed event is actually an event on a different file.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>Returns True if this event is a unique event.</returns>
        private bool IsNewEvent(FileSystemEventArgs e)
        {
            var fileName = e.Name!;

            lock (m_fileLastChangeTime)
            {
                var fileAccessTime = m_fileRepository.GetLastWriteTimeUTC(e.FullPath);

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
