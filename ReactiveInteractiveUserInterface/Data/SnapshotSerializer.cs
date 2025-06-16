using System;
using System.Globalization;
using TP.ConcurrentProgramming.Data;
using TP.ConcurrentProgramming.Data.Diagnostics;

namespace TP.ConcurrentProgramming.Data.Diagnostics
{
    internal class SnapshotSerializer
    {
        public string SerializeToAscii(LogEntry entry)
        {
            string timeStr = entry.MeasurementTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff");
            string pos = $"({entry.Position.x:F2}, {entry.Position.y:F2})";
            string vel = $"({entry.Velocity.x:F2}, {entry.Velocity.y:F2})";
            string baseLine = $"{timeStr} | Diagnostic -> Ball#{entry.BallId} Pos={pos} Vel={vel}";

            return $"{baseLine} | Info: {entry.GetComment()}";
        }
    }
}
