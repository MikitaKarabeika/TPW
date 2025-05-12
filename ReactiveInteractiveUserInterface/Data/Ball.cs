//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Runtime.CompilerServices;

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        int x = 0;
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

        public IVector Velocity { get; set; }
        public double Mass { get; }

        #endregion IBall

        #region private

        private Vector Position;
        public IVector CurrentPosition => Position;
        
        private readonly object _lock = new object();

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
                while (isMoving)
                {
                    Move();
                    int delay = 10;
                    Thread.Sleep(delay);
                }
            }).Start();
        }
        private void Move()
        {
            lock (_lock)
            {
                Position = new Vector(Position.x + Velocity.x, Position.y + Velocity.y);
            }
            RaiseNewPositionChangeNotification();
        }

        public void ContactBall(IBall otherBall)
        {
            var other = (Ball)otherBall;

            int h1 = RuntimeHelpers.GetHashCode(this);
            int h2 = RuntimeHelpers.GetHashCode(other);
            var first = h1 < h2 ? this : other;
            var second = first == this ? other : this;

            lock (first._lock)
                lock (second._lock)
                {
                    var posA = this.Position;
                    var posB = other.Position;
                    var velA = this.Velocity;
                    var velB = other.Velocity;

                    double mA = this.Mass;
                    double mB = other.Mass;
                    double e = 1.0;

                    double dx = posA.x - posB.x;
                    double dy = posA.y - posB.y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);

                    if (dist == 0) return;

                    double nx = dx / dist;
                    double ny = dy / dist;

                    double rvx = velA.x - velB.x;
                    double rvy = velA.y - velB.y;
                    double vAlong = rvx * nx + rvy * ny;
                    if (vAlong >= 0) return; 

                    double j = -(1 + e) * vAlong / (1.0 / mA + 1.0 / mB);
                                                                        
                    double ix = j * nx;
                    double iy = j * ny;

                    velA = new Vector(velA.x + ix / mA, velA.y + iy / mA);
                    velB = new Vector(velB.x - ix / mB, velB.y - iy / mB);
                    this.Velocity = velA;
                    other.Velocity = velB;

                    this.Move(); 
                    other.Move();
                }

        }
        #endregion private
    }
}