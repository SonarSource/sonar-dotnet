namespace CSharp13
{
    public partial class PartialProperties
    {
        public partial int Foo { set; } // Noncompliant
        public partial int Foo2 { get; set; }
    }
}
