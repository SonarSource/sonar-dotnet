using System;

namespace Tests.Diagnostics
{
    public class Foo
    {

        private class NotExtended //Noncompliant
        {

        }

        private class IsExtended
        {

        }

        private class TheExtension : IsExtended //Noncompliant
        {

        }

        private class TheSecondExtension : IsExtended
        {

        }

        private class TheThirdExtension : TheSecondExtension //Noncompliant
        {

        }

        public class Baro
        {
            public class Nestception
            {

            }

        }

        internal class Bari
        {

        }

    }

}
