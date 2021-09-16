using System;

namespace Tests.Diagnostics
{
    public class Foo
    {

        private class ClassWontBeExtended //Noncompliant {{Private classes which are not derived in the current assembly should be marked as 'sealed'.}}
//                    ^^^^^^^^^^^^^^^^^^^
        {

        }

        private sealed class ClassWontBeExtendedButSealed
        {

        }

        private class PrivateDerivedClass
        {

        }

        private class PrivateDerivedClassExtension : PrivateDerivedClass //Noncompliant
        {

        }

        private class PrivateDerivedClassSecondExtension : PrivateDerivedClass
        {

        }

        private class TheThirdExtension : PrivateDerivedClassSecondExtension //Noncompliant
        {

        }

        private class PrivateClass //Noncompliant
        {
            private class NestedPrivateClass //Noncompliant
            {

            }

            private class NestedPrivateClassWillBeExtended
            {

            }

            private class NestedExtension : NestedPrivateClassWillBeExtended //Noncompliant
            {

            }
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
        private class SomeOtherClass : SomeClass //Noncompliant
    {

        }
    }

    public partial class ClassImplementedInTwoFiles
    {
        private class InnerPrivateClass
        {

        }
    }

}
