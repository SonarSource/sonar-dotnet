using System;

public record StaticFieldVisible
{
    public static double Pi = 3.14;  // Noncompliant
//                       ^^
    public const double Pi2 = 3.14;
    public double Pi3 = 3.14;
    private protected static double Pi4 = 3.14; // Noncompliant
    protected private static double Pi15 = 3.14; // Noncompliant

    public static Shape Empty = Shape.Empty; // Noncompliant {{Change the visibility of 'Empty' or make it 'const' or 'readonly'.}}

    [ThreadStatic]
    public static int value; // Compliant, thread static field values are not shared between threads
}

public record Shape
{
    public static Shape Empty = new EmptyShape(); // Noncompliant {{Change the visibility of 'Empty' or make it 'const' or 'readonly'.}}
    public static readonly Shape Empty2 = new EmptyShape();

    private record EmptyShape : Shape { }
}

public record PositionalShape(int Property)
{
    public static PositionalShape Empty = new EmptyShape(42); // Noncompliant {{Change the visibility of 'Empty' or make it 'const' or 'readonly'.}}
    public static readonly PositionalShape Empty2 = new EmptyShape(42);

    private record EmptyShape(int Property) : PositionalShape(Property) { }
}
