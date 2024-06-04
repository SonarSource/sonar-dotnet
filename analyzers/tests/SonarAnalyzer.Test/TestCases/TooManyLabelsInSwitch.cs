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
