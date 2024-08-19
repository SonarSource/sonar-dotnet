using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CSharpLatest.CSharp9Features.RecordsUsingWith;

namespace CSharpLatest.CSharp9Features;

internal class RecordsWithMutableData
{
    public void Foo()
    {
        Battery battery = new Battery("CR2032", 0.235)
        {
            RemainingCapacityPercentage = 100
        };

        Console.WriteLine(battery);

        for (int i = battery.RemainingCapacityPercentage; i >= 0; i--)
        {
            battery.RemainingCapacityPercentage = i;
        }

        Console.WriteLine(battery);
    }

    public record Battery(string Model, double TotalCapacityAmpHours)
    {
        public int RemainingCapacityPercentage { get; set; }
    }
}
