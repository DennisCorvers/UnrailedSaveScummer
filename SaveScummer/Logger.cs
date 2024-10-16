using SaveScummerLib.Logging;

namespace SaveScummer
{
    internal class Logger : ILogger
    {
        private readonly string m_categoryName;

        public Logger()
        {
            m_categoryName = "Program";
        }

        public void Log(string message)
            => Log(message, null);

        public void Log(string message, Exception? exception)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff");

            if (exception != null)
            {
                message += $" Exception: {exception.GetType().Name}: {exception.Message}";
            }

            Console.WriteLine($"{timestamp} {m_categoryName}: {message}");
        }
    }
}
