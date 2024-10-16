using SaveScummerLib.Config;
using SaveScummerLib.Files;
using SaveScummerLib.Logging;
using SaveScummerLib.Monitoring;

namespace SaveScummer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WriteWelcomeMessage(new[]
            {
                "Save files are automatically restored while this application is running.",
                "Enter 'q' to stop the application."
            });

            var program = new Program();
            program.Run();
            program.Stop();
        }

        static void WriteWelcomeMessage(IEnumerable<string> text)
        {
            var originalColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var line in text)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine();
            Console.ForegroundColor = originalColour;
        }

        private readonly ILogger m_logger;
        private readonly IFileRepository m_fileRepository;
        private readonly IFileMonitor m_fileMonitor;
        private readonly IConfiguration m_config;

        public Program()
        {
            m_logger = new Logger();
            m_config = ConfigLoader.LoadConfiguration();
            m_fileRepository = new FileRepository(m_config, m_logger);
            m_fileMonitor = new FileMonitor(m_config, m_logger, m_fileRepository);
        }

        public void Run()
        {
            try
            {
                m_fileRepository.CreateFileStore();
                m_fileMonitor.StartMonitoring();

                while (Console.Read() != 'q') ;
            }
            catch (Exception e)
            {
                m_logger.Log("An exception has occured.", e);
            }
            finally
            {
                m_fileMonitor.Dispose();
            }
        }

        public void Stop()
        {
            if (m_fileMonitor != null)
            {
                m_fileMonitor.Dispose();
            }
        }
    }
}