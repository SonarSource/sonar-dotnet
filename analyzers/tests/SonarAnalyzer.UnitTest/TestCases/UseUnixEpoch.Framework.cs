using System;
using System.Globalization;

public class Program
{
    private readonly DateTime Epoch = new DateTime(1970, 1, 1); // Compliant

    private readonly DateTimeOffset EpochOff = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero); // Compliant
}
