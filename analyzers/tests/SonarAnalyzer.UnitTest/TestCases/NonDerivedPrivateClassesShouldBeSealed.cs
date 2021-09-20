using System;

namespace Tests.Diagnostics
{
    public interface PrivateInterface
    {

    }

    public class Foo
    {
        private class ClassWontBeExtended // Noncompliant {{Private classes or records which are not derived in the current assembly should be marked as 'sealed'.}}
//                    ^^^^^^^^^^^^^^^^^^^
        {

        }

        private sealed class ClassWontBeExtendedButSealed
        {

        }

        private class PrivateDerivedClass
        {

        }

        private class PrivateDerivedClassExtension : PrivateDerivedClass // Noncompliant
        {

        }

        private class PrivateDerivedClassSecondExtension : PrivateDerivedClass
        {

        }

        private class TheThirdExtension : PrivateDerivedClassSecondExtension // Noncompliant
        {

        }

        private class PrivateClass // Noncompliant
        {
            private class NestedPrivateClass // Noncompliant
            {

            }

            private class NestedPrivateClassWillBeExtended
            {

            }

            private class NestedExtension : NestedPrivateClassWillBeExtended // Noncompliant
            {

            }
        }

        private abstract class InnerPrivateClass // Compliant
        {

        }

        private struct AStruct // Compliant, structs cannot be inherited.
        {

        }
    }

    public partial class Bar
    {
        private class SomeClass
        {

        }
    }

    public partial class Bar
    {
        private class SomeOtherClass : SomeClass // Noncompliant
        {

        }
    }

    public partial class ClassImplementedInTwoFiles
    {
        // The class is extended \TestCases\NonDerivedPrivateClassesShouldBeSealed_PartialClass.cs
        private class InnerPrivateClass
        {

        }
    }

    public class AClassWithAnInnerInterface
    {

        private class APrivateClass // Noncompliant
        { 

        }

        public interface InnerInterface
        {

        }
    }
}
