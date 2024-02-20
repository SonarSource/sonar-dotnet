namespace CSharpLatest.CSharp9Features;

public class S1067
{
    public void Foo()
    {
        object x = null;
        if (x is true or true or true or true or true) { }
    }
}
