using System;
using System.Text;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using TP.ConcurrentProgramming.Data.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    internal static class DiagnosticLogger
    {
        private static BlockingCollection<SnapshotSerializer.SerializedSnapshot> snapshotQueue = new(5000);
        private static Thread? backgroundWriterThread;
        private static StreamWriter? outputStream;
        private static SnapshotSerializer serializer = new();
        private static CancellationTokenSource? cancelSource;
        private static bool isFinished = false;
        static DiagnosticLogger()
        {
            InitializeLogger();
        }
        private static void InitializeLogger()
        {
            cancelSource = new CancellationTokenSource();

            string targetFile = Path.Combine(Path.GetDirectoryName(typeof(DiagnosticLogger).Assembly.Location)!, "ball_log.txt");
            outputStream = new StreamWriter(targetFile, append: true, encoding: Encoding.ASCII) { AutoFlush = true };

            backgroundWriterThread = new Thread(() =>
            {
                try
                {
                    foreach (var snapshot in snapshotQueue.GetConsumingEnumerable(cancelSource.Token))
                    {
                        string line = serializer.SerializeToAscii(snapshot);
                        bool written = false;
                        int wait = 25;
                        while (!written && !cancelSource.IsCancellationRequested)
                        {
                            try
                            {
                                outputStream.WriteLine(line);
                                written = true;
                            }
                            catch (IOException)
                            {
                                Thread.Sleep(wait);
                                wait = Math.Min(wait * 2, 1000);
                            }
                        }
                    }
                }
                catch (OperationCanceledException) { }
                finally
                {
                    try { outputStream?.Flush(); } catch { }
                }
            })
            {
                IsBackground = true
            };

            backgroundWriterThread.Start();
            isFinished = false;
        }

        public static void SubmitSnapshot(SnapshotSerializer.SerializedSnapshot snapshot)
        {
            if (isFinished || snapshotQueue.IsAddingCompleted)
                return;

            try
            {
                snapshotQueue.TryAdd(snapshot);
            }
            catch (InvalidOperationException) { }
        }

        public static void Finish()
        {
            if (isFinished)
                return;

            isFinished = true;

            try { snapshotQueue.CompleteAdding(); } catch { }
            try { cancelSource?.Cancel(); } catch { }
            try { backgroundWriterThread?.Join(); } catch { }
            try { outputStream?.Dispose(); } catch { }
            try { cancelSource?.Dispose(); } catch { }
        }
    }
}
