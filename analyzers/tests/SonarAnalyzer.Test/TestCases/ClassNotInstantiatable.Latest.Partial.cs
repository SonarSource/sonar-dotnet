namespace CSharp14
{
    public partial class PartialWithNestedTypeInOtherFile // Compliant - The private constructor is declared in ClassNotInstantiatable.Latest.cs
    {
        public class Nested { }
    }

    public partial class PartialWithInstanceMemberInOtherFile // Noncompliant - Reported on every part of the class
    {
        public int Value => 1;
    }

    partial class PartialPublicConstructor
    {
        public partial PartialPublicConstructor() { }
    }

    partial class PartialPrivateConstructor // Noncompliant
    {
        private partial PartialPrivateConstructor() { }
    }
}
