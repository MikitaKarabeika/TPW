using System.Collections.Concurrent;
using System.IO;

namespace TP.ConcurrentProgramming.Data
{
    internal static class Logger
    {
        private static readonly ConcurrentQueue<string> logQueue = new();
        private static bool isRunning = true;

        static Logger()
        {
            string basePath = Path.GetDirectoryName(typeof(Logger).Assembly.Location)!;
            string logPath = Path.Combine(basePath, "ball_log.txt");

            new Thread(() =>
            {
                using StreamWriter writer = new(logPath, append: true);
                while (isRunning || !logQueue.IsEmpty)
                {
                    while (logQueue.TryDequeue(out string? line))
                    {
                        writer.WriteLine(line);
                        writer.Flush();
                    }
                    Thread.Sleep(10);
                }
            })
            { IsBackground = true }.Start();
        }

        public static void Log(string message)
        {
            logQueue.Enqueue(message);
        }

        public static void Stop()
        {
            isRunning = false;
        }
    }
}
