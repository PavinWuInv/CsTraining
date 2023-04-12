using System.Runtime.CompilerServices;
using System;

namespace CS_Beginner_ConstructCar
{
    public class Car : ICar
    {
        public IFuelTankDisplay fuelTankDisplay;

        private IEngine engine;

        private IFuelTank fuelTank;

        public Car() : this(20)
        {
        }

        public Car(double fuelLevel)
        {
            fuelTank = new FuelTank(fuelLevel);
            engine = new Engine(fuelTank);
            fuelTankDisplay = new FuelTankDisplay(fuelTank);
        }

        public void EngineStart()
        {
            if (fuelTank.FillLevel > 0)
            {
                engine.Start();
            }
        }

        public void EngineStop()
        {
            engine.Stop();
        }
        
        public void Refuel(double liters)
        {
            fuelTank.Refuel(liters);
        }

        public void RunningIdle()
        {
            if (EngineIsRunning)
            {
                engine.Consume(IdleConsumption);
            }
        }

        public bool EngineIsRunning => engine.IsRunning;

        private double IdleConsumption = 0.0003;
    }

    public class Engine : IEngine
    {
        public Engine(IFuelTank fuelTank)
        {
            m_fuelTank = fuelTank;
        }

        public bool IsRunning { get; private set; } = false;

        public void Consume(double liters)
        {
            // Make more sense to have car do this
            //if (!IsRunning)
            //{
            //    return;
            //}

            m_fuelTank.Consume(liters);

            // No distance implemented, so don't have to worry about partial fuel consumption.
            if (m_fuelTank.FillLevel <= 0)
            {
                Stop();
            }
        }

        public void Start()
        {
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
        }

        private readonly IFuelTank m_fuelTank;
    }

    public class FuelTank : IFuelTank
    {
        public FuelTank(double fill)
        {
            LimitAddFuel(fill);
        }

        public double FillLevel { get; private set; } = 0;

        public bool IsOnReserve => FillLevel < ReserveFill;

        public bool IsComplete => Math.Round(FillLevel, Precision) == MaxFill;

        public void Consume(double liters)
        {
            if (liters < 0)
            {
                // Example doesn't raise exception
                //throw new ArgumentOutOfRangeException(nameof(liters), "Amount consumed must be greater than 0");
                return;
            }

            if (FillLevel - liters <= EmptyFill)
            {
                FillLevel = EmptyFill;
            }
            else
            {
                FillLevel -= liters;
            }
        }

        public void Refuel(double liters)
        {
            LimitAddFuel(liters);
        }

        private void LimitAddFuel(double additionalFuel)
        {
            if (additionalFuel < 0)
            {
                //throw new ArgumentOutOfRangeException(nameof(additionalFuel), "Refuel amount must be greater than 0");
                return;
            }

            if (FillLevel + additionalFuel > MaxFill)
            {
                FillLevel = MaxFill;
            }
            else
            {
                FillLevel += additionalFuel;
            }
        }

        private readonly int Precision = 10;
        private readonly double EmptyFill = 0;
        private readonly double ReserveFill = 5;
        private readonly double MaxFill = 60;
    }

    public class FuelTankDisplay : IFuelTankDisplay
    {
        public FuelTankDisplay(IFuelTank fuelTank)
        {
            m_fuelTank = fuelTank;
        }

        public double FillLevel => Math.Round(m_fuelTank.FillLevel, displayPrecision);

        public bool IsOnReserve => m_fuelTank.IsOnReserve;

        public bool IsComplete => m_fuelTank.IsComplete;

        private int displayPrecision = 2;
        private readonly IFuelTank m_fuelTank;
    }
}