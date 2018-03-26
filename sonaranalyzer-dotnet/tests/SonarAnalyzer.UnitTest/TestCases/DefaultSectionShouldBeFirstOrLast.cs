using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        public void Foo()
        {
            switch (a)
            {
                case 0:
                    break;
                default: // Compliant - last section
                    break;
            }

            switch (a)
            {
                default: // Compliant - first section
                    break;

                case 0:
                    break;
            }

            switch (a)
            {
                case 0:
                    break;
                default: // Noncompliant {{Move this 'default:' case to the beginning or end of this 'switch' statement.}}
//              ^^^^^^^^
                    break;
                case 1:
                    break;
            }

            switch (a)
            {
            }

            switch (a)
            {
                case 42:
                    break;
            }
        }
    }
}
