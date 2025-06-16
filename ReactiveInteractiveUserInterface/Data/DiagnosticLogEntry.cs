using System;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.Data.Diagnostics
{
    internal abstract class LogEntry
    {
        public int BallId { get; }
        public DateTime MeasurementTime { get; }
        public IVector Position { get; }
        public IVector Velocity { get; }

        protected LogEntry(int ballId, DateTime time, IVector pos, IVector vel)
        {
            BallId = ballId;
            MeasurementTime = time;
            Position = pos;
            Velocity = vel;
        }

        public abstract string GetComment();
    }

    internal sealed class MovementLogEntry : LogEntry
    {
        public MovementLogEntry(int id, DateTime t, IVector pos, IVector vel)
            : base(id, t, pos, vel) { }

        public override string GetComment() => "Ball moved";
    }

    internal sealed class CollisionLogEntry : LogEntry
    {
        private readonly int _otherBallId;

        public CollisionLogEntry(int id, int otherId, DateTime t, IVector pos, IVector vel)
            : base(id, t, pos, vel)
        {
            _otherBallId = otherId;
        }

        public override string GetComment() => $"Ball#{BallId} collided with Ball#{_otherBallId}";
    }
}
