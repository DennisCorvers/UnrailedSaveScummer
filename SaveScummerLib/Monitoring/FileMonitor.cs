using SaveScummerLib.Config;
using SaveScummerLib.Files;
using SaveScummerLib.Logging;

namespace SaveScummerLib.Monitoring
{
    public class FileMonitor : IFileMonitor
    {
        private readonly FileSystemWatcher m_watcher;
        private readonly IConfiguration m_configuration;
        private readonly IFileEventProcessor m_eventProcessor;
        private readonly ILogger m_logger;
        private readonly ISet<string> m_excludedFiles;

        public FileMonitor(IConfiguration config, IFileEventProcessor fileEventProcessor, ILogger logger)
        {
            m_configuration = config;
            m_eventProcessor = fileEventProcessor;
            m_logger = logger;

            if (!Directory.Exists(m_configuration.SaveFolderLocation))
            {
                throw new InvalidOperationException("Provided save folder location does not exist.");
            }

            m_watcher = InitWatcher(config);
            m_excludedFiles = InitFileFilter(config);
        }

        ~FileMonitor()
        {
            Dispose();
        }

        private ISet<string> InitFileFilter(IConfiguration configuration)
        {
            var ignoredFiles = configuration.IgnoredFiles
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            m_logger.Log($"The following files have been excluded: {string.Join(", ", ignoredFiles)}");
            return ignoredFiles;
        }

        private FileSystemWatcher InitWatcher(IConfiguration configuration)
        {
            var extension = Utils.StringUtils.NormaliseExtension(configuration.FileExtension);
            var targetFolder = configuration.SaveFolderLocation;

            var watcher = new FileSystemWatcher(targetFolder, extension)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                IncludeSubdirectories = false,
            };
            watcher.Deleted += OnWatcherEvent;
            watcher.Created += OnWatcherEvent;
            watcher.Changed += OnWatcherEvent;

            return watcher;
        }

        private void OnWatcherEvent(object sender, FileSystemEventArgs e)
        {
            if (!m_excludedFiles.Contains(e.Name!))
            {
                m_eventProcessor.ProcessEvent(e);
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

            var extension = Utils.StringUtils.NormaliseExtension(m_configuration.FileExtension);
            var dir = m_configuration.SaveFolderLocation;

            m_logger.Log($"Started monitoring {extension} file(s) in directory {dir}");
        }

        public void StopMonitoring()
        {
            m_watcher.EnableRaisingEvents = false;

            m_logger.Log("Stopped file monitor.");
        }
    }
}
