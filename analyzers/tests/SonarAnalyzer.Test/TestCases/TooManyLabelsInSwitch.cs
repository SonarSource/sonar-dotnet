using System;

namespace Tests.Diagnostics
{
    public class TooManyLabelsInSwitch
    {
        public enum MyEnum
        {
            A,
            B,
            C,
            D
        }

        public TooManyLabelsInSwitch(int n, MyEnum en)
        {
            switch (n)
            {
                case 0:
                {
                    {
                        break;
                    }
                }
                default:
                    break;
            }

            switch(n) // Noncompliant
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        Console.WriteLine("");
                        break;
                    }
                case 2:
                    {
                        {
                            break;
                        }
                    }
            }

            switch(n) // Noncompliant
            {
                case 0:
                    Console.WriteLine("0");
                    Console.WriteLine("0+0");
                    break;
                case 1:
                    Console.WriteLine("1");
                    break;
                case 2:
                    Console.WriteLine("2");
                    break;
            }

            switch(n) // Noncompliant
            {
                case 0:
                    Console.WriteLine("0");
                    Console.WriteLine("0+0");
                    return;
                case 1:
                    Console.WriteLine("1");
                    break;
                case 2:
                    Console.WriteLine("2");
                    break;
            }

            switch(n) // Noncompliant
            {
                case 0:
                    Console.WriteLine("0");
                    Console.WriteLine("0+0");
                    throw new InvalidOperationException();
                case 1:
                    Console.WriteLine("1");
                    break;
                case 2:
                    Console.WriteLine("2");
                    break;
            }

            switch(n) // Compliant
            {
                case 0:
                    Console.WriteLine("0");
                    break;
                case 1:
                    Console.WriteLine("1");
                    break;
                case 2:
                    Console.WriteLine("2");
                    break;
                case 3:
                    Console.WriteLine("3");
                    throw new InvalidOperationException();
                case 4:
                    Console.WriteLine("4");
                    return;
            }

            switch (n)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    break;
                default:
                    break;
            }

            switch (en)
            {
                case MyEnum.A:
                    break;
                case MyEnum.B:
                    break;
                case MyEnum.C:
                    break;
                case MyEnum.D:
                    break;
                default:
                    break;
            }

            switch (n) // Compliant
            {
                case 0:
                case 1:
                    break;
                case 2:
                    break;
                default:
                    break;
            }
        }

        public int SwitchCase(char ch, int value)
        {
            switch(ch)  // Noncompliant {{Consider reworking this 'switch' to reduce the number of 'case' clause to at most 2 or have only one statement per 'case'.}}
//          ^^^^^^
            {
                case 'a':
                    return 1;
                case 'b':
                    return 2;
                case 'c':
                    throw new NotImplementedException();
                // ...
                case '-':
                    if (value > 10)
                    {
                        return 42;
                    }
                    else if (value < 5 && value > 1)
                    {
                        return 21;
                    }
                    return 99;
                default:
                    return 1000;
            }
        }

        public int SwitchCaseWithCodeBlock(char ch, int value)
        {
            switch(ch)  // Noncompliant {{Consider reworking this 'switch' to reduce the number of 'case' clause to at most 2 or have only one statement per 'case'.}}
//          ^^^^^^
            {
                case 'a':
                    return 1;
                case 'b':
                    throw new NotImplementedException();
                case 'c':
                    return 3;
                // ...
                case '-':
                    {
                        if (value > 10)
                        {
                            return 42;
                        }
                        else if (value < 5 && value > 1)
                        {
                            return 21;
                        }
                    }
                    return 99;
                default:
                    return 1000;
            }
        }

        public int SwitchCaseFallThrough(char ch, int value)
        {
            switch(ch) // Compliant
            {
                case 'a':
                case 'b':
                case 'c':
                case '-':
                    if (value > 10)
                    {
                        return 42;
                    }
                    else if (value < 5 && value > 1)
                    {
                        return 21;
                    }
                    return 99;
                default:
                    return 1000;
            }
        }

        public int FalseNegatives(int a)
        {
            switch (a) // FN
            {
                case 1:
                    return 1;
                case 2:
                    throw new NotImplementedException();
                case 3:
                    return 3;
                case 4:
                    if (a > 42) return 21; // This is one statement
                    return 42;
                case 5:
                    if (a > 42) return 21; else if (a < 21) return 3; // This is one statement
                    return 42;
            }

            return 0;
        }

        public int Test(string type)
        {
            return type switch // Compliant
            {
                "a" => 1,
                "b" => 2,
                "c" => 3,
                "d" => 4,
                "e" => 5,
                "f" => 6,
                "g" => 7,
                "h" => 8,
                "i" => 9,
                _ => 10
            };
        }
    }
}
