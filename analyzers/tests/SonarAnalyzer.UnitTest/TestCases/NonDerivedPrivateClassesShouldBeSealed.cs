using System;

namespace Tests.Diagnostics
{
    public class Foo
    {

        private class ClassWontBeExtended //Noncompliant
        {

        }

        private sealed class ClassWontBeExtendedButSealed
        {

        }

        private class Extended
        {

        }

        private class TheExtension : Extended //Noncompliant
        {

        }

        private class TheSecondExtension : Extended
        {

        }

        private class TheThirdExtension : TheSecondExtension //Noncompliant
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

}
