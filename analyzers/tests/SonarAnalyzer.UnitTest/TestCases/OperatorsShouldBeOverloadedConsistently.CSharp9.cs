public record Compliant // Compliant as == operator cannot be overloaded for records.
{
    public static object operator +(Compliant a, Compliant b) => new object();
    public static object operator -(Compliant a, Compliant b) => new object();
    public static object operator *(Compliant a, Compliant b) => new object();
    public static object operator /(Compliant a, Compliant b) => new object();
    public static object operator %(Compliant a, Compliant b) => new object();
}

public record OnlyPlus // Compliant
{
    public static object operator +(OnlyPlus a, OnlyPlus b) => new object();
}

public record OnlyMinus // Compliant
{
    public static object operator -(OnlyMinus a, OnlyMinus b) => new object();
}

public record OnlyMultiply // Compliant
{
    public static object operator -(OnlyMultiply a, OnlyMultiply b) => new object();
}

public record OnlyDevide // Compliant
{
    public static object operator -(OnlyDevide a, OnlyDevide b) => new object();
}

public record OnlyReminder // Compliant
{
    public static object operator -(OnlyReminder a, OnlyReminder b) => new object();
}
