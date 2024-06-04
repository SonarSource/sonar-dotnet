using System;
using System.Collections.Generic;

public abstract partial class A
{
    public abstract void X();
}
public abstract partial class A
{
    public void Y() { }
}

public abstract class Empty
{
}

public abstract class Animal // Noncompliant {{Convert this 'abstract' class to an interface.}}
//                    ^^^^^^
{
    protected abstract void move();
    protected abstract void feed();
}

public class SomeBaseClass { }

public abstract class Animal2 : SomeBaseClass //Compliant
{
    protected abstract void move();
    protected abstract void feed();

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
    { }

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
