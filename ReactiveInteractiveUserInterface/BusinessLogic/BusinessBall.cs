//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using TP.ConcurrentProgramming.Data;
using BisAPI = TP.ConcurrentProgramming.BusinessLogic.BusinessLogicAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall
    {
        private static ConcurrentBag<Ball> Balls = new();
        private readonly Data.IBall _inner;
        private static readonly ConcurrentDictionary<(int, int), bool> InCollision = new ConcurrentDictionary<(int, int), bool>();
        private readonly int _hash;
        private Position _currentPosition;
        public Position CurrentPosition => _currentPosition;
        private const double Margin = 8;
        public Ball(Data.IBall ball)
        {
            _inner = ball;
            _hash = RuntimeHelpers.GetHashCode(this);
            Balls.Add(this);
            ball.NewPositionNotification += RaisePositionChangeEvent;
        }
        
        private IVector CreateVector(double x, double y)
        {
            return DataAbstractAPI.GetDataLayer().CreateVector(x, y);
        }

        #region IBall

        public event EventHandler<IPosition>? NewPositionNotification;

        private readonly object _lock = new();
        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            if (sender is Data.IBall dataBall)
            {
                _currentPosition = new Position(e.x, e.y);

                double diameter = BisAPI.GetDimensions.BallDimension;
                double collisionDistance = diameter * diameter;

                Parallel.ForEach(Balls, other =>
                {
                    if (ReferenceEquals(this, other)) return;

                    if (other.CurrentPosition is null) return;

                    var key = _hash < other._hash ? (_hash, other._hash) : (other._hash, _hash);
                    double dx = _currentPosition.x - other.CurrentPosition.x;
                    double dy = _currentPosition.y - other.CurrentPosition.y;
                    double distSq = dx * dx + dy * dy;

                    if ((distSq < collisionDistance) && (InCollision.TryAdd(key, true)))
                    {
                        BallCollision(_inner, other._inner);
                    }
                    else
                    {
                        InCollision.TryRemove(key, out _);
                    }
                });

                bool isXOut = e.x < 0 || e.x > BisAPI.GetDimensions.TableWidth - BisAPI.GetDimensions.BallDimension - Margin;
                bool isYOut = e.y < 0 || e.y > BisAPI.GetDimensions.TableHeight - BisAPI.GetDimensions.BallDimension - Margin;

                if (isXOut || isYOut)
                {
                    dataBall.Velocity = CreateVector(
                        isXOut ? -dataBall.Velocity.x : dataBall.Velocity.x,
                        isYOut ? -dataBall.Velocity.y : dataBall.Velocity.y
                    );
                }

                double boundedX = Math.Clamp(e.x, 0, BisAPI.GetDimensions.TableWidth - BisAPI.GetDimensions.BallDimension - Margin);
                double boundedY = Math.Clamp(e.y, 0, BisAPI.GetDimensions.TableHeight - BisAPI.GetDimensions.BallDimension - Margin);

                _currentPosition = new Position(boundedX, boundedY);

                NewPositionNotification?.Invoke(this, _currentPosition);
            }
        }
        
        private void BallCollision(Data.IBall innerBall, Data.IBall otherBall)
        {
            if (innerBall == null || otherBall == null) return;

            Ball firstBall = Balls.FirstOrDefault(b => b._inner == innerBall);
            Ball secondBall = Balls.FirstOrDefault(b => b._inner == otherBall);
            if (firstBall == null || secondBall == null) return;

            bool lockFirstFirst = firstBall.GetHashCode() < secondBall.GetHashCode();
            var firstToLock = lockFirstFirst ? firstBall : secondBall;
            var secondToLock = lockFirstFirst ? secondBall : firstBall;

            lock (firstToLock._lock)
            {
                lock (secondToLock._lock)
                {
                    var posA = innerBall.CurrentPosition;
                    var posB = otherBall.CurrentPosition;
                    var velA = innerBall.Velocity;
                    var velB = otherBall.Velocity;

                    double dx = posA.x - posB.x;
                    double dy = posA.y - posB.y;
                    double distanceSquared = dx * dx + dy * dy;
                    double minDistance = BisAPI.GetDimensions.BallDimension;
                    double minDistanceSquared = minDistance * minDistance;

                    if (distanceSquared > minDistanceSquared) return;

                    double distance = Math.Sqrt(distanceSquared);
                    double nx = dx / distance;
                    double ny = dy / distance;

                    double rvx = velA.x - velB.x;
                    double rvy = velA.y - velB.y;
                    double velocityAlongNormal = rvx * nx + rvy * ny;

                    if (velocityAlongNormal > 0) return;

                    double impulse = -2 * velocityAlongNormal / (1 / innerBall.Mass + 1 / otherBall.Mass);

                    double ix = impulse * nx;
                    double iy = impulse * ny;

                    innerBall.Velocity = CreateVector(
                        velA.x + ix / innerBall.Mass,
                        velA.y + iy / innerBall.Mass);

                    otherBall.Velocity = CreateVector(
                        velB.x - ix / otherBall.Mass,
                        velB.y - iy / otherBall.Mass);
                   
                }
            }
        }

        #endregion private
    }
}