using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PendulumTests
{
    [Test]
    public void TestAngleRatioReturnsCorrectValue()
    {
        // Given a pendulum
        var pendulum = new Pendulum();

        // When angle ratio is called with pi
        var angleRatio = pendulum.calcualteAngleRatio(3.1416f);

        // Then the expected value 0.5 is returned
        Assert.AreEqual(2, angleRatio);
        
    }

    [Test]
    public void TestPutNumberTo2DPReturnsCorrectValue()
    {
        // Given a pendulum
        var pendulum = new Pendulum();

        // When PutTo2DP is called with 5DP, 0.14314
        var nonDecimal = pendulum.putNumberTo2DP(0.14314f);

        // Then the expected value 0.14 is returned
        Assert.AreEqual(0.14f, nonDecimal);
        
    }

    [Test]
    public void TestGetXOutputReturnsX()
    {
        // Given a pendulum
        var pendulum = new Pendulum();

        // When X is called with RodLength 3, Gravity 9.81, AngleRatio 0.5pi
        var XOutput = pendulum.getXOutput(3, 9.81f, 1.5708f);

        // Then the expected value 3*sin(9.81t)*1.57 is returned
        Assert.AreEqual("3*sin(9.81t)*1.57", XOutput);
        
    }

    [Test]
    public void TestGetYOutputReturnsY()
    {
        // Given a pendulum
        var pendulum = new Pendulum();

        // When Y is called with RodLength 3, Gravity 9.81, AngleRatio 0.5pi
        var YOutput = pendulum.getYOutput(3, 9.81f, 1.5708f);

        // Then the expected value -3 + 3*cos(9.81t)*1.57 is returned
        Assert.AreEqual("-3 + 3*cos(9.81t)*1.57", YOutput);
        
    }

    [Test]
    public void TestNewVectorLocationOfPivot()
    {
        // Given a pendulum and a start vector
        var pendulum = new Pendulum();
        var startVector = new Vector3(3,3,3);

        // When the speed is a diagonal Vector 3, 3, 3 and a pivot location of 3
        var loactionOfPivot = pendulum.newVectorLocationOfPivot(startVector, 3f);

        // Then the expected value of the force is 2.42 in all directions
        var expectedOutput = new Vector3(2.42f, 2.42f, 2.42f);
        Assert.AreEqual(expectedOutput.ToString(), loactionOfPivot.ToString());
        
    }
    
    [Test]
    public void TestCalcualteCentripetalForce()
    {
        // Given a pendulum
        var pendulum = new Pendulum();
        var velocityVector = new Vector3(3,3,3);

        // When the speed is a diagonal vector 3, 3, 3 and the rodLength of 1.2
        var centripetalForce = pendulum.calcualteCentripetalForce(velocityVector, 1.2f);

        // Then the expected value is 22.5, ajusted for the margin of error from cubing a float
        Assert.AreEqual(22.4999981f, centripetalForce);
        
    }
}
