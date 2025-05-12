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
        private static readonly ConcurrentDictionary<(int, int), bool> InCollision = new ConcurrentDictionary<(int, int), bool>(); // Dictionary do przechowywania kul w kolizji
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

        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            if (sender is Data.IBall dataBall)
            {
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

                double diameter = BisAPI.GetDimensions.BallDimension;
                double collidsionDistance = diameter * diameter;

                Parallel.ForEach(Balls, other =>
                {
                    if (ReferenceEquals(this, other)) return;

                    if (other.CurrentPosition is null) return;

                    var key = _hash < other._hash ? (_hash, other._hash) : (other._hash, _hash);
                    double dx = e.x - other.CurrentPosition.x;
                    double dy = e.y - other.CurrentPosition.y;
                    double distSq = dx * dx + dy * dy;

                    if (distSq < collidsionDistance)
                    {
                        if (InCollision.TryAdd(key, true))
                        {
                            _inner.ContactBall(other._inner);
                        }
                    }
                    else
                    {
                        InCollision.TryRemove(key, out _);
                    }
                });

                NewPositionNotification?.Invoke(this, _currentPosition);
            }
        }
        private readonly object _lock = new();
        /*
        public void ContactBall(IBall otherBall)
        {
            var other = (Ball)otherBall; // Rzutowanie do konkretnej klasy aby uzyskać dostęp do jej pól

            int h1 = RuntimeHelpers.GetHashCode(this); // Aby uniknąć zakleszczenia ustawiamy kolejność blokad na podstawie hashcode'ów
            int h2 = RuntimeHelpers.GetHashCode(other);
            var first = h1 < h2 ? this : other;
            var second = first == this ? other : this;

            lock (first._lock) // Blokujemy oba obiekty przed modyfikacją wspólnych danych.
                lock (second._lock)
                {
                    var posA = this.Position; // Pobieramy pozycje obu kul
                    var posB = other.Position;
                    var velA = this.Velocity;
                    var velB = other.Velocity;

                    double mA = this.Mass; // Masy każdej kuli i współczynnik sprężystości (e = 1 = idealnie sprężyste)
                    double mB = other.Mass;
                    double e = 1.0;

                    double dx = posA.x - posB.x; // Obliczamy wektor od środka B do środka A i jego długość
                    double dy = posA.y - posB.y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);

                    if (dist == 0) return;// Jeśli kule są dokładnie na sobie, pomijamy dalsze kroki

                    double nx = dx / dist; // Normalizujemy wektor do osi zderzenia
                    double ny = dy / dist;

                    double rvx = velA.x - velB.x; // Obliczamy komponent prędkości względnej wzdłuż normalnej:
                    double rvy = velA.y - velB.y;
                    double vAlong = rvx * nx + rvy * ny;
                    if (vAlong >= 0) return; // Jeśli komponent >= 0, kule się oddalają, więc brak reakcji

                    double j = -(1 + e) * vAlong / (1.0 / mA + 1.0 / mB); // Obliczamy skalarny impuls j wg wzoru:
                                                                          //    j = -(1 + e) * (v_rel · n) / (1/mA + 1/mB)
                    double ix = j * nx; // Składowa impulsu osi x
                    double iy = j * ny; // Składowa impulsu osi y

                    velA = new Vector(velA.x + ix / mA, velA.y + iy / mA); // Aktualizujemy prędkości obu kul:
                    velB = new Vector(velB.x - ix / mB, velB.y - iy / mB); // vA' = vA + (j/mA)*n,  vB' = vB - (j/mB)*n
                    this.Velocity = velA;
                    other.Velocity = velB;

                    this.Move(); // Używamy move() aby zaktualizować pozycję obu kulek
                    other.Move();
                }

        }
        */

        #endregion private
    }
}