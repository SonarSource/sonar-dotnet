public record StaticFieldVisible
{
    public static double Pi = 3.14;  // Noncompliant
//                       ^^
    public const double Pi2 = 3.14;
    public double Pi3 = 3.14;
    public static Shape Empty = Shape.Empty; // Noncompliant {{Change the visibility of 'Empty' or make it 'const' or 'readonly'.}}
}

public record Shape
{
    public static readonly Shape Empty = new EmptyShape();

    private record EmptyShape : Shape
    {
    }
}
