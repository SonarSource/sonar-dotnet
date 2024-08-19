namespace CSharpLatest.CSharp11Features;

internal class GenericAttributes
{
    public class GenericAttribute<T> : Attribute { }

    [GenericAttribute<string>()]
    public string Method() => default;

    public class GenericType<T>
    {
        // [GenericAttribute<T>()] // Not allowed! generic attributes must be fully constructed types.
        public string Method() => default;
    }
}
