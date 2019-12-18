using System;

namespace Tests.Diagnostics
{
    class InfiniteRecursion
    {
        int Pow(int num, int exponent)   // Noncompliant; no condition under which pow isn't re-called
//          ^^^
        {
            num = num * Pow(num, exponent - 1);
            return num;  // this is never reached
        }

        void Test1() // Noncompliant {{Add a way to break out of this method's recursion.}}
        {
            var i = 10;
            if (i == 10)
            {
                Test1();
            }
            else
            {
                switch (i)
                {
                    case 1:
                        Test1();
                        break;
                    default:
                        Test1();
                        break;
                }
            }
        }

        void Test2()
        {
            var i = 10;
            switch (i)
            {
                case 1:
                    goto default;
                default:
                    goto case 1; // Noncompliant {{Add a way to break out of this method.}}
            }
        }

        void Test3()
        {
            var i = 10;
            switch (i)
            {
                case 1:
                    goto default;
                case 2:
                    break;
                default:
                    goto case 1; // Noncompliant
            }

            switch (i)
            {
                case 1:
                    goto default;
                case 2:
                    break;
                default:
                    goto case 1; // Noncompliant
            }
        }

        int Test4() => Test4(); // Noncompliant

        int Test5() => 42;

        int Prop
        {
            get // Noncompliant {{Add a way to break out of this property accessor's recursion.}}
            {
                return Prop;
            }
        }

        string Prop0
        {
            get
            {
                return nameof(Prop0);
            }
        }

        object Prop1
        {
            get // FN - analyzer currently only checks for property access on the 'this' object
            {
                return (new InfiniteRecursion())?.Prop1;
            }
        }

        int Prop2
        {
            get // Not recognized, but the accessors are circularly infinitely recursive
            {
                (Prop2) = 10;
                return 10;
            }
            set
            {
                var x = Prop2;
            }
        }

        int Prop3 => 42;

        int Prop5 => Prop5; // Noncompliant

        int Prop6
        {
            get => 42;
        }

        int Prop7
        {
            get => Prop6; // Compliant - FP
        }

        private InfiniteRecursion infiniteRecursionField;

        object Prop8
        {
            get // FN - analyzer currently only checks for property access on the 'this' object
            {
                return infiniteRecursionField?.Prop1;
            }
        }

        void InternalRecursion(int i)
        {
            start:
            goto end;
            end:
            goto start; // Noncompliant

            switch (i)
            {
                case 1:
                    goto default;
                case 2:
                    break;
                default:
                    goto case 1; // Compliant, already not reachable
            }
        }

        int Pow2(int num, int exponent)
        {
            if (exponent > 1)
            {
                num = num * Pow2(num, exponent - 1);
            }
            return num;
        }

        void Generic<T>() // Noncompliant
        {
            Generic<T>();
        }

        void Generic2<T>() // Compliant
        {
            Generic2<int>();
        }

        // (FN) False Negative
        // See https://github.com/SonarSource/sonar-dotnet/issues/2342
        int Power(int num, int exponent)
        {
            try
            {
                num = num * Power(num, exponent - 1);
                return num;  // this is never reached
            }
            catch (Exception)
            {
                throw;
            }
        }

        int FixIt(int a)    // FN - Two methods calling each other are not recognized
        {
            return UpdateIt(a);
        }

        int UpdateIt(int a)
        {
            return FixIt(a);
        }

        int CSharp8_SwitchExpressions_OK(int a)
        {
            return a switch
            {
                0 => 1,
                1 => 0,
                _ => CSharp8_SwitchExpressions_OK(a) % 2
            };
        }

        int CSharp8_SwitchExpressions_Bad(int a)    //Noncompliant
        {
            return a switch
            {
                0 => CSharp8_SwitchExpressions_Bad(a + 1),
                1 => CSharp8_SwitchExpressions_Bad(a - 1),
                _ => CSharp8_SwitchExpressions_Bad(a) % 2
            };
        }

        int CSharp8_StaticLocalFunctions_OK(int a)
        {
            static int Calculate(int a, int b) => a + b + 1;

            return Calculate(a, 1);
        }

        int CSharp8_StaticLocalFunctions_Bad(int a, int b)
        {
            static int Calculate(int a, int b) => Calculate(a, b) + 1;  //Noncompliant

            return Calculate(a, b);
        }

        int CSharp8_StaticLocalFunctions_FN(int a, int b)
        {
            static int Add(int a, int b) => Fix(a, b);  // FN - Two methods calling each other are not recognized
            static int Fix(int a, int b) => Add(a, b);

            return Add(a, b);
        }

    }

    public interface IWithDefaultImplementation
    {
        decimal Count { get; set; }
        decimal Price { get; set; }

        decimal Total() //Noncompliant
        {
            return Count * Price + Total();
        }
    }

}
