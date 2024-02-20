namespace CSharpLatest.CSharp11
{
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
}
