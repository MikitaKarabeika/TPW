//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Diagnostics;
using System.Threading;
using TP.ConcurrentProgramming.Data.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        internal Ball(Vector initialPosition, Vector initialVelocity, double mass)
        {
            Position = initialPosition;
            Velocity = initialVelocity;
            Mass = mass;
        }

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

        private IVector _velocity;
        private readonly object _velocityLock = new object();
        private readonly object _stateLock = new object();
        private volatile bool isMoving = true;

        internal void Stop() => isMoving = false;

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

            SubmitMovementSnapshot();
            RaiseNewPositionChangeNotification();
        }

        private void SubmitMovementSnapshot()
        {
            SubmitSnapshotToLogger(new MovementLogEntry(
                GetHashCode(), DateTime.UtcNow, Position, Velocity
            ));
        }

        public void SubmitCollisionSnapshot(int otherBallId, IVector position, IVector velocity)
        {
            SubmitSnapshotToLogger(new CollisionLogEntry(
                GetHashCode(), otherBallId, DateTime.UtcNow, position, velocity
            ));
        }

        private void SubmitSnapshotToLogger(LogEntry entry)
        {
            DiagnosticLogger.Instance.SubmitSnapshot(entry);
        }

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }
    }
}
