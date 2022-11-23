public class Foo : IOperator<Foo> // Compliant - the '+' operator comes IOperator interface
{
    public static object operator -(Foo a, Foo b) => new object();
    public static object operator *(Foo a, Foo b) => new object();
    public static object operator /(Foo a, Foo b) => new object();
    public static object operator %(Foo a, Foo b) => new object();
    public static object operator ==(Foo a, Foo b) => new object();
    public static object operator !=(Foo a, Foo b) => new object();

    public override bool Equals(object obj) => false;
    public override int GetHashCode() => 0;
}

internal interface IOperator<T> where T : IOperator<T>
{
    static virtual T operator +(T a, T b) => a;
}
