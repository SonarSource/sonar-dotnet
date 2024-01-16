using System;
using System.Globalization;

public class Program
{
    private readonly DateTime Epoch = new DateTime(1970, 1, 1); // Compliant: the UnixEpoch property is available starting from .NET Core 2.1/.NET Standard 2.1, the .NET Framework does not support it

    private readonly DateTimeOffset EpochOff = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero); // Compliant
}
