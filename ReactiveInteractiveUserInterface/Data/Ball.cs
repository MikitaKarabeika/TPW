﻿//____________________________________________________________________________________________________________________________________
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


        private Vector _position;
        private IVector _velocity;
        private readonly object _velocityLock = new object();

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
                    velocityLength = Math.Sqrt(Velocity.x * Velocity.x + Velocity.y * Velocity.y);
                    Move();
                    int delay = 400 / (int)velocityLength;
                    Thread.Sleep(delay);
                }
            }).Start();
        }
        private void Move()
        {
            lock (_lock)
            {
                Position = new Vector(Position.x + Velocity.x / velocityLength, Position.y + Velocity.y / velocityLength);
            }
            RaiseNewPositionChangeNotification();
        }
        
        #endregion private
    }
}