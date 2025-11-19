namespace CSharp14
{
    partial class PartialPublicConstructor
    {
        public partial PartialPublicConstructor() { }
    }

    partial class PartialPrivateConstructor // Noncompliant
    {
        private partial PartialPrivateConstructor() { }
    }
}
