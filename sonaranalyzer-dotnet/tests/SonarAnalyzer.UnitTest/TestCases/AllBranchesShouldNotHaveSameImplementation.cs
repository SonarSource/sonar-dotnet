using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        public void IfElseCases(int b, int c)
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

            if (b == 0) // Compliant - no else clause
            {
                DoSomething();
            }
            else if (b == 1)
            {
                DoSomething();
            }
        }

        public void SwitchCases(int i)
        {
            switch (i) // Noncompliant {{Remove this 'switch' or edit its sections so that they are not all the same.}}
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
        }

        public void TernaryCases(bool c, int a)
        {
            int b = a > 12 ? 4 : 4;  // Noncompliant

            var x = 1 > 18 ? true : true; // Noncompliant
            var y = 1 > 18 ? true : false;
            y = 1 > 18 ? (true) : true; // Noncompliant
            TernaryCases(1 > 18 ? (true) : true, a); // Noncompliant
        }

        private void DoSomething()
        {
        }

        private void DoSomethingElse()
        {
        }
    }
}
