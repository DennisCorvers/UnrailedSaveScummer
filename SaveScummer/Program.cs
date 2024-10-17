using Microsoft.Extensions.DependencyInjection;
using SaveScummerLib.Config;
using SaveScummerLib.Files;
using SaveScummerLib.Logging;
using SaveScummerLib.Monitoring;
using UnrailedSaveScummer.Config;

namespace UnrailedSaveScummer
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

            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection
                .AddSingleton<ILogger, Logger>()
                .AddSingleton<IConfiguration>(x => ConfigLoader.LoadConfig())
                .AddSingleton<IFileRepository, FileRepository>()
                .AddSingleton<IFileEventProcessor, FileEventProcessor>()
                .AddSingleton<IFileMonitor, FileMonitor>()
                .AddSingleton<Program>()
                .BuildServiceProvider();

            // Resolve the Program class and run it
            var program = serviceProvider.GetRequiredService<Program>();
            program.Run();
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

        public Program(ILogger logger, IFileRepository fileRepository, IFileMonitor fileMonitor)
        {
            m_logger = logger;
            m_fileRepository = fileRepository;
            m_fileMonitor = fileMonitor;
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
                m_fileMonitor.StopMonitoring();
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