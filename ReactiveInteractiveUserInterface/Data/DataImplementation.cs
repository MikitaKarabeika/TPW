﻿//____________________________________________________________________________________________________________________________________
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

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region ctor

        public DataImplementation()
        {
            
        }

        #endregion ctor

        #region DataAbstractAPI

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            string logPath = Path.Combine(Path.GetDirectoryName(typeof(DiagnosticLogger).Assembly.Location)!, "ball_log.txt");
            if (File.Exists(logPath))
            {
                try { File.WriteAllText(logPath, string.Empty); } catch { }
            }

            Random random = new Random();
            for (int i = 0; i < numberOfBalls; i++)
            {
                Vector startingPosition = new(random.Next(100, 400 - 100), random.Next(100, 400 - 100));
                //double mass = random.Next(1, 10);
                double mass = 10;

                //double speed = random.Next(3, 9);
                //double speed = random.Next(100, 150);
                double speed = 160;
                Vector startingVelocity = new Vector(
                    (RandomGenerator.NextDouble() - 0.5) * speed,
                    (RandomGenerator.NextDouble() - 0.5) * speed
                );

                Ball newBall = new(startingPosition, startingVelocity, mass);
                upperLayerHandler(startingPosition, newBall);
                BallsList.Add(newBall);

                newBall.StartMoving();
            }
            
        }

        #endregion DataAbstractAPI

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    foreach (var ball in BallsList)
                    {
                        ball.Stop();
                    }
                    BallsList.Clear();

                    DiagnosticLogger.Instance.Finish();
                }
                Disposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        //private bool disposedValue;
        private bool Disposed = false;

        private Random RandomGenerator = new();
        private List<Ball> BallsList = [];
        
        public override IVector CreateVector(double x, double y)
        {
            return new Vector(x, y);
        }

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
        {
            returnBallsList(BallsList);
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
        {
            returnNumberOfBalls(BallsList.Count);
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}