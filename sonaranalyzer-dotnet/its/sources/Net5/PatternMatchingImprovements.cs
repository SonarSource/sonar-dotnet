using System;
using Net5.CommercialRegistration;
using Net5.ConsumerVehicleRegistration;
using Net5.LiveryRegistration;

namespace Net5
{
    // https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/pattern-matching
    namespace ConsumerVehicleRegistration
    {
        public class Car
        {
            public int Passengers { get; set; }
        }
    }

    namespace CommercialRegistration
    {
        public class DeliveryTruck
        {
            public int GrossWeightClass { get; set; }
        }
    }

    namespace LiveryRegistration
    {
        public class Taxi
        {
            public int Fares { get; set; }
        }

        public class Bus
        {
            public int Capacity { get; set; }
            public int Riders { get; set; }
        }
    }

    public class PatternMatchingImprovements
    {
        public decimal TypePatterns(object vehicle) => vehicle switch
        {
            Car => 2.00m,
            Taxi => 3.50m,
            Bus => 5.00m,
            DeliveryTruck => 10.00m,
            { } => throw new ArgumentException(message: "Not a known vehicle type", paramName: nameof(vehicle)),
            null => throw new ArgumentNullException(nameof(vehicle))
        };

        public decimal RelationalPatterns(int value) => value switch
        {
            > 5000 => 10.00m + 5.00m,
            < 3000 => 10.00m - 2.00m,
            _ => 10.00m
        };

        public decimal LogicalPatterns(int value) => value switch
        {
            >= 3000 and <= 5000 => 10.00m,
            < 0 or > 10000 => 10.00m + 5.00m,
            not < -1000 => 10.00m + 5.00m,
        };

        public void NotPattern(object a, object b)
        {
            var isNull = a switch
            {
                not null => 1,
                null => 0
            };

            if (b is not string)
            {
                Console.WriteLine("it's not a string!");
            }
        }
    }
}
