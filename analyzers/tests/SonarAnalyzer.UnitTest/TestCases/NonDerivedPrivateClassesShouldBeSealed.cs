using System;

namespace Tests.Diagnostics
{
    public class Foo
    {
        private class ClassWontBeExtended { } // Noncompliant {{Private classes or records which are not derived in the current assembly should be marked as 'sealed'.}}
//                    ^^^^^^^^^^^^^^^^^^^
        private class PrivateClassVirtualMethod // Compliant, the class has a virtual member.
        {
            public virtual void AMethod() { }
        }

        private class PrivateClassVirtualProperty // Compliant, the class has a virtual member.
        {
            public virtual int Number { get;  set; }
        }

        private class PrivateClassVirtualIndexer // Compliant, the class has a virtual member.
        {
            public virtual int this[int key]
            {
                get { return 1; }
                set { key += 1; }
            }
        }

        private class PrivateClassVirtualEvent // Compliant, the class has a virtual member.
        {
            public virtual event EventHandler Foo
            {
                add
                {
                    Console.WriteLine("Base Foo.add called");
                }
                remove
                {
                    Console.WriteLine("Base Foo.remove called");
                }
            }
        }

        private sealed class ClassWontBeExtendedButSealed { }

        private class PrivateDerivedClass { }

        private class PrivateDerivedClassExtension : PrivateDerivedClass { } // Noncompliant

        private class PrivateDerivedClassSecondExtension : PrivateDerivedClass { }

        private class TheThirdExtension : PrivateDerivedClassSecondExtension { } // Noncompliant

        private class PrivateClass // Noncompliant
        {
            private class NestedPrivateClass { } // Noncompliant

            private class NestedPrivateClassWillBeExtended { }

            private class NestedExtension : NestedPrivateClassWillBeExtended { } // Noncompliant
        }

        private abstract class InnerPrivateClass { } // Compliant

        private struct AStruct { } // Compliant, structs cannot be inherited.

        private static class InnerPrivateStaticClass { } // Compliant, static classes cannot be inherited.
    }

    public partial class Bar
    {
        private class SomeClass { }
    }

    public partial class Bar
    {
        private class SomeOtherClass : SomeClass { } // Noncompliant
    }

    public partial class ClassImplementedInTwoFiles
    {
        // The class is extended \TestCases\NonDerivedPrivateClassesShouldBeSealed_PartialClass.cs
        private class InnerPrivateClass { }
    }

    public class AClassWithAnInnerInterface
    {
        private class APrivateClass { } // Noncompliant

        public interface InnerInterface { }
    }
}
