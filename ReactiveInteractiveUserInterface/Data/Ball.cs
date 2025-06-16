//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Diagnostics;
using System.IO;
using TP.ConcurrentProgramming.Data.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        #region ctor

        internal Ball(Vector initialPosition, Vector initialVelocity, double mass)
        {
            Position = initialPosition;
            Velocity = initialVelocity;
            Mass = mass;

            SubmitSnapshotToLogger("Ball created");
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity
        {
            get
            {
                lock (_velocityLock)
                {
                    return _velocity;
                }
            }
            set
            {
                lock (_velocityLock)
                {
                    _velocity = value;
                }
            }
        }

        private Vector Position;
        public IVector CurrentPosition => Position;

        public double Mass { get; }

        public double velocityLength;

        #endregion IBall

        #region private

        private IVector _velocity;
        private readonly object _velocityLock = new object();
        private readonly object _stateLock = new object();
        private volatile bool isMoving = true;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        internal void Stop()
        {
            isMoving = false;
        }

        internal void StartMoving()
        {
            new Thread(() =>
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                long previousTime = stopwatch.ElapsedMilliseconds;
                long currentTime;
                double deltaTime;

                while (isMoving)
                {
                    currentTime = stopwatch.ElapsedMilliseconds;
                    deltaTime = (currentTime - previousTime) / 1000.0;
                    previousTime = currentTime;

                    velocityLength = Math.Sqrt(Velocity.x * Velocity.x + Velocity.y * Velocity.y);
                    Move(deltaTime);

                    Thread.Sleep(10);
                }
            }).Start();
        }

        private void Move(double deltaTime)
        {
            Vector currentVelocity, newPosition;
            lock (_stateLock)
            {
                currentVelocity = (Vector)Velocity;
                newPosition = new Vector(
                    Position.x + currentVelocity.x * deltaTime,
                    Position.y + currentVelocity.y * deltaTime
                );
                Position = newPosition;
            }

            SubmitSnapshotToLogger();

            RaiseNewPositionChangeNotification();
        }

        private void SubmitSnapshotToLogger(string? comment = null)
        {
            DiagnosticLogger.Instance.SubmitSnapshot(new SnapshotSerializer.SerializedSnapshot(
                ballId: GetHashCode(),
                measurementTime: DateTime.UtcNow,
                positionX: Position.x,
                positionY: Position.y,
                velocityX: Velocity.x,
                velocityY: Velocity.y,
                comment: comment
            ));
        }

        #endregion private
    }
}
