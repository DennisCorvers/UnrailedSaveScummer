using SaveScummerLib.Logging;

namespace UnrailedSaveScummer
{
    internal class Logger : ILogger
    {
        private readonly ConsoleColor m_color;

        public Logger()
        {
            m_color = Console.ForegroundColor;
        }

        public void Log(string message)
            => Log(message, null);

        public void Log(string message, Exception? exception)
        {
            if (exception != null)
            {
                LogException(message, exception);
                return;
            }

            var type = "INFO";
            Log(message, type, ConsoleColor.Cyan);
        }

        public void LogException(string message, Exception exception)
        {
            var type = "ERROR";
            message += $" Exception: {exception.GetType().Name}: {exception.Message}";
            Log(message, type, ConsoleColor.Red);
        }

        private void Log(string message, string type, ConsoleColor typeColour)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");

            Console.Write(timestamp);
            Console.ForegroundColor = typeColour;
            Console.Write($" {type}: ");
            Console.ForegroundColor = m_color;
            Console.WriteLine(message);
        }
    }
}
