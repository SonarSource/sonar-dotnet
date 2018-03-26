using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        public void Foo()
        {
            if (b == 0)  // Noncompliant
            {
                DoSomething();
            }
            else if (b == 1)
            {
                DoSomething();
            }
            else
            {
                DoSomething();
            }

            if (b == 0) // Noncompliant
            {
                if (c == 1) // Noncompliant
                {
                    DoSomething();
                }
                else
                {
                    DoSomething();
                }
            }
            else
            {
                if (c == 1) // Noncompliant
                {
                    DoSomething();
                }
                else
                {
                    DoSomething();
                }
            }

            if (b == 0)
            {
                DoSomething();
            }
            else
            {
                DoSomethingElse();
            }

            int b = a > 12 ? 4 : 4;  // Noncompliant

            switch (i) // Noncompliant
            {
                case 1:
                    DoSomething();
                    break;
                case 2:
                    DoSomething();
                    break;
                case 3:
                    DoSomething();
                    break;
                default:
                    DoSomething();
                    break;
            }

            switch (i) // Noncompliant
            {
                case 1:
                    {
                        DoSomething();
                        break;
                    }
                case 2:
                    {
                        DoSomething();
                        break;
                    }
                case 3:
                    {
                        DoSomething();
                        break;
                    }
                default:
                    {
                        DoSomething();
                        break;
                    }
            }

            switch (i)
            {
                case 1:
                    DoSomething();
                    break;
                default:
                    DoSomethingElse();
                    break;
            }

            switch (i) // Compliant - no default section
            {
                case 1:
                    DoSomething();
                    break;
                case 2:
                    DoSomething();
                    break;
                case 3:
                    DoSomething();
                    break;
            }

            if (b == 0) // Compliant - no else clause
            {
                DoSomething();
            }
            else if (b == 1)
            {
                DoSomething();
            }
        }

        private void DoSomething()
        {
        }

        private void DoSomethingElse()
        {
        }
    }
}
