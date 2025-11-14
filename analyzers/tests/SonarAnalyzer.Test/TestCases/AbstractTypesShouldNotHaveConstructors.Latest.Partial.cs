namespace CSharp14
{
    public abstract partial class PartialConstructor
    {
        public partial PartialConstructor() { } // Noncompliant
    }

    public abstract partial class NonPartialConstructor
    {

    }
}
