using System;

Battery battery = new Battery("CR2032", 0.235, 100);

Console.WriteLine (battery);

for (int i = battery.RemainingCapacityPercentage; i >= 0; i--)
{
    Battery updatedBattery = battery with {RemainingCapacityPercentage = i};
    battery = updatedBattery;
}

Console.WriteLine (battery);

public record Battery(string Model, double TotalCapacityAmpHours, int RemainingCapacityPercentage);
