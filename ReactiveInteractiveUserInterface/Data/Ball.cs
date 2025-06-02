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
using System.Runtime.CompilerServices;

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

        //private readonly object _lock = new object();

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }
        private volatile bool isMoving = true;
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
            double dx = Velocity.x * deltaTime;
            double dy = Velocity.y * deltaTime;

            Position = new Vector(Position.x + dx, Position.y + dy);

            Logger.Log($"Time: {DateTime.Now:HH:mm:ss.fff}, Ball@{GetHashCode()} Pos=({Position.x:F2}, {Position.y:F2}) Vel=({Velocity.x:F2}, {Velocity.y:F2})");

            RaiseNewPositionChangeNotification();
        }

        #endregion private
    }
}