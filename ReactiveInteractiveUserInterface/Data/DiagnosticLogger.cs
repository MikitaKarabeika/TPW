using System;
using System.Text;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using TP.ConcurrentProgramming.Data.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    internal sealed class DiagnosticLogger
    {
        private static readonly Lazy<DiagnosticLogger> _instance = new(() => new DiagnosticLogger());
        public static DiagnosticLogger Instance => _instance.Value;

        private readonly BlockingCollection<LogEntry> snapshotQueue;
        private readonly Thread backgroundWriterThread;
        private readonly StreamWriter outputStream;
        private readonly SnapshotSerializer serializer;
        private readonly CancellationTokenSource cancelSource;
        private bool isFinished = false;

        private DiagnosticLogger()
        {
            snapshotQueue = new BlockingCollection<LogEntry>(5000);
            cancelSource = new CancellationTokenSource();
            serializer = new SnapshotSerializer();

            string targetFile = Path.Combine(Path.GetDirectoryName(typeof(DiagnosticLogger).Assembly.Location)!, "ball_log.txt");
            outputStream = new StreamWriter(targetFile, append: true, encoding: Encoding.ASCII)
            {
                AutoFlush = true
            };

            backgroundWriterThread = new Thread(() =>
            {
                try
                {
                    foreach (var entry in snapshotQueue.GetConsumingEnumerable(cancelSource.Token))
                    {
                        string line = serializer.SerializeToAscii(entry);
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
                    try { outputStream.Flush(); } catch { }
                    try { outputStream.Dispose(); } catch { }
                }
            });

            backgroundWriterThread.IsBackground = true;
            backgroundWriterThread.Start();
        }

        public void SubmitSnapshot(LogEntry snapshot)
        {
            if (isFinished || snapshotQueue.IsAddingCompleted)
                return;

            try
            {
                snapshotQueue.TryAdd(snapshot);
            }
            catch (InvalidOperationException) { }
        }

        public void Finish()
        {
            if (isFinished)
                return;

            isFinished = true;

            try { snapshotQueue.CompleteAdding(); } catch { }
            try { cancelSource.Cancel(); } catch { }
            try { backgroundWriterThread.Join(); } catch { }
            try { cancelSource.Dispose(); } catch { }
        }
    }
}
