using System;
using System.Globalization;

namespace TP.ConcurrentProgramming.Data.Diagnostics
{
    internal class SnapshotSerializer
    {
        internal record SerializedSnapshot(
            int ballId,
            DateTime measurementTime,
            double positionX,
            double positionY,
            double velocityX,
            double velocityY,
            string? comment = null
        );

        public string SerializeToAscii(SerializedSnapshot inputSnapshot)
        {
            string timeStr = inputSnapshot.measurementTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff");
            string positionStr = $"({inputSnapshot.positionX.ToString("F2", CultureInfo.InvariantCulture)}, " +
                $"{inputSnapshot.positionY.ToString("F2", CultureInfo.InvariantCulture)})";
            string velocityStr = $"({inputSnapshot.velocityX.ToString("F2", CultureInfo.InvariantCulture)}, " +
                $"{inputSnapshot.velocityY.ToString("F2", CultureInfo.InvariantCulture)})";

            string baseLine = $"{timeStr} | Diagnostic -> Ball#{inputSnapshot.ballId} Pos={positionStr} Vel={velocityStr}";

            return inputSnapshot.comment != null
                ? $"{baseLine} | Info: {inputSnapshot.comment}"
                : baseLine;
        }
    }
}
