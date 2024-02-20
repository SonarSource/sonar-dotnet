namespace CSharpLatest.CSharp9Features;

public class S3240
{
    abstract class Fruit { }
    class Apple : Fruit { }

    public void Foo()
    {
        Apple a = null, b = null;
        a = a is not null ? (a) : b;
        a = a is null ? (b) : a;
    }
}
