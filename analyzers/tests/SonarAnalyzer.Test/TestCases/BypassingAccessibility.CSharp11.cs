using System;

public class BindingFlagsImposter
{
    public int NonPublic;
}

public class Derived
{
    [Obsolete(nameof(BindingFlags.NonPublic))] // Compliant
    public void DoWork(BindingFlagsImposter BindingFlags)
    {
        var a = System.Reflection.BindingFlags.NonPublic; // Noncompliant
    }
}
