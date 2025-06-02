using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using TP.ConcurrentProgramming.Data.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    internal static class DiagnosticLogger
    {
        private static readonly BlockingCollection<SnapshotSerializer.SerializedSnapshot> snapshotQueue = new(5000);
        private static readonly Thread backgroundWriterThread;
        private static readonly StreamWriter outputStream;
        private static readonly SnapshotSerializer serializer = new();
        private static readonly CancellationTokenSource cancelSource = new();

        static DiagnosticLogger()
        {
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
                    outputStream.Flush();
                }
            })
            {
                IsBackground = true
            };
            backgroundWriterThread.Start();
        }
        public static void Finish()
        {
            snapshotQueue.CompleteAdding();
            cancelSource.Cancel();
            backgroundWriterThread.Join();
            outputStream.Dispose();
            cancelSource.Dispose();
        }
        public static void SubmitSnapshot(SnapshotSerializer.SerializedSnapshot snapshot)
        {
            snapshotQueue.TryAdd(snapshot);
        }
    }
}

