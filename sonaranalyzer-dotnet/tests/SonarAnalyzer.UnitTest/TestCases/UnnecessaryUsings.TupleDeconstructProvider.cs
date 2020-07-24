using System;

public static class ServiceReturningTuples
{
    public static Tuple<string, int> GetPair() => Tuple.Create("a", 1);
}
