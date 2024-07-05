using System;
using System.Collections.Generic;

public abstract partial class PartialMixed
{
    public abstract void X();
}

public abstract partial class PartialMixed
{
    public void Y() { }
}

public abstract partial class PartialAbstract // Noncompliant
{
    public abstract void X();
}

public abstract partial class PartialAbstract // Noncompliant
{
    public abstract void Y();
}

public abstract class Empty
{
}

public abstract class Animal // Noncompliant {{Convert this 'abstract' class to an interface.}}
//                    ^^^^^^
{
    public abstract void move();
    public abstract void feed();
}

public class SomeBaseClass { }

public abstract class Animal2 : SomeBaseClass // Compliant
{
    public abstract void move();
    public abstract void feed();
}

public abstract class Color
{
    private int red = 0;
    private int green = 0;
    private int blue = 0;

    public int getRed()
    {
        return red;
    }
}

public interface AnimalCompliant
{
    void move();
    void feed();
}

public class ColorCompliant
{
    private int red = 0;
    private int green = 0;
    private int blue = 0;

    private ColorCompliant()
    {
    }

    public int getRed()
    {
        return red;
    }
}

public abstract class LampCompliant
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

public abstract class View // Noncompliant, should be an interface
{
    public abstract string Content { get; }
}

public abstract class View2 // Compliant, has abstract and non abstract members
{
    public abstract string Content { get; }
    public abstract string Content1 { get; }
    public string Content2 { get; }
}

public abstract class View2Derived : View2 // Compliant, still has abstract parts
{
    public string Content3 { get; }
    public override string Content1 { get { return ""; } }
}

public abstract class View3Derived : SomeUnknownType // Error [CS0246]
{
    public string Content3 { get; }
    public override int Content1 { get { return 1; } }
}

public abstract class WithStaticConstructor         // TN for .NET Framework, FN for .NET Core / .NET (where interfaces can have static constructors)
{
    public abstract void ToOverride();

    static WithStaticConstructor()
    {
        // Do something here
    }
}

public abstract class WithNonStaticConstructor      // Compliant: the class has a non-static constructor, it cannot be converted to an interface
{
    public abstract void ToOverride();

    public WithNonStaticConstructor()
    {
        // Do something here
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/9494
public abstract class AbstractClassWithField            // Compliant: the class has a field, it cannot be converted to an interface
{
    protected int _data;
    public abstract void SomeMethod();
}

public abstract class AbstractProtectedMethod           // Noncompliant
{
    protected abstract void SomeMethod();
}

public abstract class AbstractProtectedInternalMethod   // Noncompliant
{
    protected internal abstract void SomeMethod();
}
