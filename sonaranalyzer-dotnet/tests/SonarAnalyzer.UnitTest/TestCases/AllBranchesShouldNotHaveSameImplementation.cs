using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        public void IfElseCases(int b, int c)
        {
            if (b == 0)  // Noncompliant {{Remove this conditional structure or edit its code blocks so that they're not all the same.}}
//          ^^
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
//              ^^
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
            switch (i) // Noncompliant {{Remove this conditional structure or edit its code blocks so that they're not all the same.}}
//          ^^^^^^
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
            int b = a > 12 ? 4 : 4;  // Noncompliant {{This conditional operation returns the same value whether the condition is "true" or "false".}}
//                  ^^^^^^^^

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

        public int SwitchExpressionNoncompliant(string type) =>
            type switch // Noncompliant {{Remove this conditional structure or edit its code blocks so that they're not all the same.}}
//               ^^^^^^
            {
                "a" => GetNumber(),
                "b" => GetNumber(),
                _ => GetNumber()
            };

        public int SwitchExpressionNested(string type)
        {
            return type switch
            {
                "a" => GetNumber(),
                _ => type switch // Noncompliant
                {
                        "b" => GetNumber(),
                        "c" => GetNumber(),
                        _ => GetNumber()
                    }
            };
        }

        public int GetNumber() => 42;

        public int SwitchExpressionCompliant(string type)
        {
            var x = type switch // Compliant
            {
                "a" => 42,
                "b" => type.Length,
                _ => GetNumber(),
            };
            string y = type switch { }; // Compliant
            var z = type switch { "a" => 42 }; // Compliant
            var withoutDefault = type switch // Compliant, does not have the discard default arm
            {
                "a" => GetNumber(),
                "b" => GetNumber(),
                "c" => GetNumber(),
                "d" => GetNumber(),
                "e" => GetNumber(),
            };
            return x;
        }
    }
}
