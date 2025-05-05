//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
  internal class Ball : IBall
  {

    #region ctor

    internal Ball(Vector initialPosition, Vector initialVelocity)
    {
      Position = initialPosition;
      Velocity = initialVelocity;
    }

    #endregion ctor

    #region IBall

    public event EventHandler<IVector>? NewPositionNotification;

    public IVector Velocity { get; set; }

    #endregion IBall

    #region private

    private Vector Position;

    private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }
    internal void Move(Vector delta)
    {
        const double fieldWidth = 385;
        const double fieldHeight = 405;
        const double ballDiameter = 20;
        const double ballRadius = ballDiameter / 2;

        double nextX = Position.x + delta.x;
        double nextY = Position.y + delta.y;

        double correctedX = Position.x;
        double correctedY = Position.y;
        Vector correctedVelocity = (Vector)Velocity;

        if (nextX - ballRadius < 0)
        {
            correctedX = ballRadius;
            correctedVelocity = new Vector(0, correctedVelocity.y);
        }
        else if (nextX + ballRadius > fieldWidth)
        {
            correctedX = fieldWidth - ballRadius;
            correctedVelocity = new Vector(0, correctedVelocity.y);
        }
        else
        {
            correctedX = nextX;
        }

        if (nextY - ballRadius < 0)
        {
            correctedY = ballRadius;
            correctedVelocity = new Vector(correctedVelocity.x, 0);
        }
        else if (nextY + ballRadius > fieldHeight)
        {
            correctedY = fieldHeight - ballRadius;
            correctedVelocity = new Vector(correctedVelocity.x, 0);
        }
        else
        {
            correctedY = nextY;
        }

        Position = new Vector(correctedX, correctedY);
        Velocity = correctedVelocity;

        RaiseNewPositionChangeNotification();
    }

    #endregion private
    }
}