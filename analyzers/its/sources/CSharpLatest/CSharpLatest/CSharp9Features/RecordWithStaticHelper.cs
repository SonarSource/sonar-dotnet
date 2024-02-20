using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpLatest.CSharp9Features;

internal class RecordWithStaticHelper
{
    public void Foo()
    {
        var weight = 200;
        WeightMeasurement measurement = new(DateTime.Now, weight)
        {
            Pounds = WeightMeasurement.GetPounds(weight)
        };

        Console.WriteLine(measurement);
    }

    public record WeightMeasurement(DateTime Date, double Kilograms)
    {
        public double Pounds { get; init; }

        public static double GetPounds(double kilograms) => kilograms * 2.20462262;
    }
}
