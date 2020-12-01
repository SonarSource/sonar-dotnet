public record Compliant
{
    public static object operator +(Compliant a, Compliant b) => new object();
    public static object operator -(Compliant a, Compliant b) => new object();
}

public record OnlyPlus // Compliant - FN, should implement `-` too
{
    public static object operator +(OnlyPlus a, OnlyPlus b) => new object();
}

public record OnlyMinus // Compliant - FN, should implement `+` too
{
    public static object operator -(OnlyMinus a, OnlyMinus b) => new object();
}
