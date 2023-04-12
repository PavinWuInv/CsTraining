// using NUnit.Framework;
using Xunit;
using CS_Beginner_ConstructCar;
using System;
using System.Linq;

// Need to be in a separate project!

namespace CS_Beginner_ConstructCarTests
{
    public class CarTests
    {
        [Fact]
        public void TestMotorStartAndStop()
        {
            var car = new Car();

            Assert.False(car.EngineIsRunning, "Engine could not be running.");

            car.EngineStart();

            Assert.True(car.EngineIsRunning, "Engine should be running.");

            car.EngineStop();

            Assert.False(car.EngineIsRunning, "Engine could not be running.");
        }

        [Fact]
        public void TestFuelConsumptionOnIdle()
        {
            var car = new Car(1);

            car.EngineStart();

            Enumerable.Range(0, 3000).ToList().ForEach(s => car.RunningIdle());

            //Assert.Equal(0.10, car.fuelTankDisplay.FillLevel, "Wrong fuel tank fill level!");
            Assert.Equal(0.10, car.fuelTankDisplay.FillLevel);
        }

        [Fact]
        public void TestFuelTankDisplayIsComplete()
        {
            var car = new Car(60);

            Assert.True(car.fuelTankDisplay.IsComplete, "Fuel tank must be complete!");
        }

        [Fact]
        public void TestFuelTankDisplayIsOnReserve()
        {
            var car = new Car(4);

            Assert.True(car.fuelTankDisplay.IsOnReserve, "Fuel tank must be on reserve!");
        }

        [Fact]
        public void TestRefuel()
        {
            var car = new Car(5);

            car.Refuel(40);

            //Assert.Equal(45, car.fuelTankDisplay.FillLevel, "Wrong fuel tank fill level!");
            Assert.Equal(45, car.fuelTankDisplay.FillLevel);
        }

        // Hidden Tests reimplement
        [Fact]
        public void Car1RandomTest()
        {

        }

        [Fact]
        public void TestEngineStopsCauseOfNoFuelExactly()
        {
            var car = new Car(3);

            car.EngineStart();
            Assert.True(car.EngineIsRunning);

            Enumerable.Range(0, 10000).ToList().ForEach(s => car.RunningIdle());

            Assert.Equal(0, car.fuelTankDisplay.FillLevel);
            Assert.False(car.EngineIsRunning);
        }

        [Fact]
        public void TestEngineStopsCauseOfNoFuelOver()
        {
            var car = new Car(3);

            car.EngineStart();
            Assert.True(car.EngineIsRunning);

            Enumerable.Range(0, 10001).ToList().ForEach(s => car.RunningIdle());

            Assert.Equal(0, car.fuelTankDisplay.FillLevel);
            Assert.False(car.EngineIsRunning);
        }

        [Fact]
        public void TestFuelLevelAllowedUpTo60()
        {
            var car = new Car(65);

            Assert.Equal(60, car.fuelTankDisplay.FillLevel);
        }

        [Fact]
        public void TestRefuelLevelAllowedUpTo60()
        {
            var car = new Car(55);

            Assert.Equal(55, car.fuelTankDisplay.FillLevel);

            car.Refuel(10);
            Assert.Equal(60, car.fuelTankDisplay.FillLevel);
        }

        [Fact]
        public void TestMotorDoesntStartWithEmptyFuelTank()
        {
            var car = new Car(0);

            Assert.False(car.EngineIsRunning);

            car.EngineStart();
            Assert.False(car.EngineIsRunning);
        }

        [Fact]
        public void TestNoConsumptionWhenEngineNotRunning()
        {
            var car = new Car(3);

            Enumerable.Range(0, 10000).ToList().ForEach(s => car.RunningIdle());
            Assert.Equal(3, car.fuelTankDisplay.FillLevel);
        }

        [Fact]
        public void TestNoNegativeFuelLevelAllowed()
        {
            var car = new Car(-5);
            Assert.Equal(0, car.fuelTankDisplay.FillLevel);
        }

        // Additional Tests

        [Fact]
        public void TestEmptyTank()
        {
            var car = new Car(0.0003);

            car.EngineStart();
            car.RunningIdle();

            Assert.Equal(0, car.fuelTankDisplay.FillLevel);
        }

        [Fact]
        public void TestDefaultFill()
        {
            var car = new Car();

            Assert.Equal(20, car.fuelTankDisplay.FillLevel);
        }

        [Fact]
        public void TestNoRunningWithoutEngine()
        {
            var car = new Car();

            Enumerable.Range(0, 3000).ToList().ForEach(s => car.RunningIdle());
            Assert.Equal(20, car.fuelTankDisplay.FillLevel);

            car.EngineStart();
            Assert.Equal(20, car.fuelTankDisplay.FillLevel);
            Enumerable.Range(0, 3000).ToList().ForEach(s => car.RunningIdle());
            Assert.Equal(20 - 0.9, car.fuelTankDisplay.FillLevel);
        }

        [Fact]
        public void TestDisplayPrecision()
        {
            var car = new Car();
            car.EngineStart();
            Assert.Equal(20, car.fuelTankDisplay.FillLevel);

            car.RunningIdle();
            Assert.Equal(20, car.fuelTankDisplay.FillLevel);

            // Ran total 16 times => rounding not kicked in yet.
            Enumerable.Range(0, 15).ToList().ForEach(s => car.RunningIdle());
            Assert.Equal(20, car.fuelTankDisplay.FillLevel);

            // Ran total 17 times => rounding kicked in.
            Enumerable.Range(0, 16).ToList().ForEach(s => car.RunningIdle());
            Assert.Equal(20 - 0.01, car.fuelTankDisplay.FillLevel);
        }
    }

}