//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Numerics;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic
{
  internal class Ball : IBall
  {
    private const double Diameter = 20;
    private const double FieldWidth = 420;
    private const double FieldHeight = 400;
    private const double Margin = 8;
        public Ball(Data.IBall ball)
    {
      ball.NewPositionNotification += RaisePositionChangeEvent;
    }/*
    public class VectorAdapter : IVector
    {
        public double x { get; init; }
        public double y { get; init; }

        public VectorAdapter(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }*/

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
            bool isXOut = e.x < 0 || e.x > FieldWidth - Diameter - Margin;
            bool isYOut = e.y < 0 || e.y > FieldHeight - Diameter - Margin;

            if (isXOut || isYOut)
            {
                dataBall.Velocity = CreateVector(
                    isXOut ? -dataBall.Velocity.x : dataBall.Velocity.x, 
                    isYOut ? -dataBall.Velocity.y : dataBall.Velocity.y
                );
            }

            double boundedX = Math.Clamp(e.x, 0, FieldWidth - Diameter - Margin);
            double boundedY = Math.Clamp(e.y, 0, FieldHeight - Diameter - Margin);
            NewPositionNotification?.Invoke(this, new Position(boundedX, boundedY));
        }
    }

    #endregion private
  }
}