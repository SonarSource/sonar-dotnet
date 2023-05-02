using System;

namespace Tests.Diagnostics
{
    public class Foo
    {
        private class ClassWontBeExtended { } // Noncompliant {{Private classes which are not derived in the current assembly should be marked as 'sealed'.}}
//                    ^^^^^^^^^^^^^^^^^^^

        private class PrivateClassVirtualMethod // Compliant, the class has a virtual member.
        {
            public virtual void AMethod() { }
        }

        private class PrivateClassVirtualProperty // Compliant, the class has a virtual member.
        {
            public virtual int Number { get; set; }
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

        private class PrivateExternalA { } // Compliant, it's extended in 'ExtendsExternalPrivateClass'
        private class PrivateExternalB { } // Compliant, it's extended in 'SeriouslyNestedClass'

        private class PrivateClass
        {
            private class NestedPrivateClass { } // Noncompliant

            private sealed class ExtendsOuterPrivateClass : PrivateClass { }

            private sealed class ExtendsExternalPrivateClass : PrivateExternalA { }

            private class NestedPrivateClassWillBeExtended { }

            private class NestedExtension : NestedPrivateClassWillBeExtended { } // Noncompliant

            private sealed class SeriouslyNestedClass
            {
                private sealed class ClassA
                {
                    private sealed class ClassB
                    {
                        private sealed class ClassC : PrivateExternalB { }
                    }
                }
            }
        }

        private abstract class PrivateAbstractClass { } // Compliant

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

// https://github.com/SonarSource/sonar-dotnet/issues/5160
public class Repro_5160
{
    private class Inner { }

    private class Inner<T> : Inner { }

    private class Inner<T, V> : Inner<T> { }

    private sealed class Inner<T, V, W> : Inner<T, V> { }

    private class Inner<A, B, C, D> : Inner { } // Noncompliant 
}
