using System;
using System.Collections.Generic;

public abstract record Empty { }

public abstract record Animal // Noncompliant {{Convert this 'abstract' record to an interface.}}
{
    public abstract void move();
    public abstract void feed();
}

public record SomeBaseRecord { }

public abstract record Animal2 : SomeBaseRecord // Compliant
{
    public abstract void move();
    public abstract void feed();
}

public abstract record RecordWithProtectedAbstractMethod // Noncompliant
{
    protected abstract void ProtectedMethod();
}

public abstract record Color
{
    private int red = 0;
    public int getRed() => red;
}

public interface AnimalCompliant
{
    void move();
    void feed();
}

public class ColorCompliant
{
    private int red = 0;

    private ColorCompliant()
    { }

    public int getRed() => red;
}

public abstract record LampCompliant
{

    private bool switchLamp = false;

    public abstract void glow();

    public void flipSwitch()
    {
        switchLamp = !switchLamp;
        if (switchLamp)
        {
            glow();
        }
    }
}

public abstract record View // Noncompliant {{Convert this 'abstract' record to an interface.}}
//                     ^^^^
{
    public abstract string Content { get; }
}

public abstract record View2() // Compliant, has abstract and non abstract members
{
    public abstract string Content { get; }
    public abstract string Content1 { get; }
    public string Content2 { get; }
}

public abstract record Record(string X);

public abstract record Record2(string X) // Compliant, this record has a propery X which is concrete
{
    public abstract string Content { get; }
}

// https://github.com/SonarSource/sonar-dotnet/issues/9494
public abstract class AbstractClassWithStaticField      // TN for .NET Framework, FN for .NET Core / .NET (where interfaces can have static members)
{
    protected static int _data;
    public abstract void SomeMethod();
}
