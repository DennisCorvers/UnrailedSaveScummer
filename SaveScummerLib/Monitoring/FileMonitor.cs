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

        public FileMonitor(IConfiguration config, IFileEventProcessor fileEventProcessor, ILogger logger)
        {
            m_configuration = config;
            m_eventProcessor = fileEventProcessor;
            m_logger = logger;

            if (!Directory.Exists(m_configuration.SaveFolderLocation))
            {
                throw new InvalidOperationException("Provided save folder location does not exist.");
            }

            var extension = Utils.StringUtils.NormaliseExtension(config.FileExtension);
            m_watcher = new FileSystemWatcher(config.SaveFolderLocation, extension)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                IncludeSubdirectories = false,
            };
            m_watcher.Deleted += OnWatcherEvent;
            m_watcher.Created += OnWatcherEvent;
            m_watcher.Changed += OnWatcherEvent;
        }

        ~FileMonitor()
        {
            Dispose();
        }

        private void OnWatcherEvent(object sender, FileSystemEventArgs e)
        {
            m_eventProcessor.ProcessEvent(e);
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
