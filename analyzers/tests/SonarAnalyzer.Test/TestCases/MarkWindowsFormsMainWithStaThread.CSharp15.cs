public class Cat;
public class Dog;

public union Pet(Cat, Dog)
{
    // Error@+2 [CS0017] Program has more than one entry point
    // Secondary@+1 [CS0017] Program has more than one entry point
    public static void Main() // Noncompliant {{Add the 'STAThread' attribute to this entry point.}}
    {
    }
}

public closed class ClosedBase
{
    // Secondary@+1 [CS0017] Program has more than one entry point
    public static void Main() // Noncompliant {{Add the 'STAThread' attribute to this entry point.}}
    {
    }
}
