using System;

var weight = 200;
WeightMeasurement measurement = new(DateTime.Now, weight)
{
    Pounds = WeightMeasurement.GetPounds(weight)
};

Console.WriteLine(measurement);

public record WeightMeasurement(DateTime Date, double Kilograms)
{
    public double Pounds {get; init;}

    public static double GetPounds(double kilograms) => kilograms * 2.20462262;
}
