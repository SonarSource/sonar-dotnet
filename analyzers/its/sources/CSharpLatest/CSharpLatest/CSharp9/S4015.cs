namespace CSharpLatest.CSharp9
{
    public class S4015
    {
        class A
        {
            public int Property { get; init; }
        }

        class B : A
        {
            public int Property { get; private init; }
        }
    }
}
