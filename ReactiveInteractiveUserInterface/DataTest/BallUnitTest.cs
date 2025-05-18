//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data.Test
{
  [TestClass]
  public class BallUnitTest
  {
    [TestMethod]
    public void ConstructorTestMethod()
    {
      Vector testingVector = new Vector(0.0, 0.0);
      double testingDouble = 1.0;
      Ball newInstance = new(testingVector, testingVector, testingDouble);
    }

    [TestMethod]
    public void MoveTestMethod()
    {
        Vector initialPosition = new(10.0, 10.0);
        double mass = 1.0;
        Ball newInstance = new(initialPosition, new Vector(0.0, 0.0), mass);
        IVector currentPosition = new Vector(0.0, 0.0);
        int numberOfCallBackCalled = 0;
        newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); currentPosition = position; numberOfCallBackCalled++; };
        newInstance.StartMoving();
        newInstance.Stop();
        //Assert.AreEqual<int>(1, numberOfCallBackCalled);
        //Assert.AreEqual<IVector>(initialPosition, currentPosition);
    }
  }
}