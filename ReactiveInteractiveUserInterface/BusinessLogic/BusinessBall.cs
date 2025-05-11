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

namespace TP.ConcurrentProgramming.BusinessLogic
{
  internal class Ball : IBall
  {
     private const double Diameter = 20;

    public Ball(Data.IBall ball)
    {
      ball.NewPositionNotification += RaisePositionChangeEvent;
    }

    #region IBall

    public event EventHandler<IPosition>? NewPositionNotification;

    #endregion IBall

    #region private

    private void RaisePositionChangeEvent(object? sender, Data.IVector e)
    {
        if (sender is not Data.IBall dataBall) return;
        /*
        bool isXOut = e.x < 0 || e.x > 420 - Diameter;
        bool isYOut = e.y < 0 || e.y > 400 - Diameter;

        if (isXOut || isYOut)
        {
            dataBall.Velocity = new TP.ConcurrentProgramming.Data.Vector(isXOut ? -dataBall.Velocity.x : dataBall.Velocity.x, isYOut ? -dataBall.Velocity.y : dataBall.Velocity.y);
        }
        */
        double boundedX = Math.Clamp(e.x, 0, 420 - Diameter - 8);
        double boundedY = Math.Clamp(e.y, 0, 400 - Diameter - 8);
        NewPositionNotification?.Invoke(this, new Position(boundedX, boundedY));

        //NewPositionNotification?.Invoke(this, new Position(e.x, e.y));

    }

    #endregion private
  }
}