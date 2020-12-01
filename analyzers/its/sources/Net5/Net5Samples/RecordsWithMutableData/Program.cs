using System;

Battery battery = new Battery("CR2032", 0.235)
{
    RemainingCapacityPercentage = 100
};

Console.WriteLine (battery);

for (int i = battery.RemainingCapacityPercentage; i >= 0; i--)
{
    battery.RemainingCapacityPercentage = i;
}

Console.WriteLine (battery);

public record Battery(string Model, double TotalCapacityAmpHours)
{
    public int RemainingCapacityPercentage {get;set;}
}
