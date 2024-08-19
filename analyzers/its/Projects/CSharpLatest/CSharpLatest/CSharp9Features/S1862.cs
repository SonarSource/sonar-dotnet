namespace CSharpLatest.CSharp9Features;

public class S1862
{
    public void Foo(object o)
    {
        if (o is not null) { }
        else if (o != null) { }

        if (o is null) { }
        else if (o == null) { }
    }
}
