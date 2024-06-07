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
            get => Prop6;
        }

        private InfiniteRecursion infiniteRecursionField;

        object Prop8
        {
            get // FN - analyzer currently only checks for property access on the 'this' object
            {
                return infiniteRecursionField?.Prop1;
            }
        }

        object Prop9 { }  // Error [CS0548]

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

        // (FN) False Negative, was fixed in RoslynCFG. There will be no updates in SonarCFG in the future.
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
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/6642
    // this is added here for coverage purposes
    public class Repro_6642
    {
        public int this[int i]
        {
            get { return this[i]; } // FN
            set { this[i] = value; } // FN
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/6643
    public class Repro_6643
    {
        public static implicit operator byte(Repro_6643 d) => d; // FN
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/6644
    public delegate bool SomeDelegate();

    public class Repro_6644
    {
        public event SomeDelegate SomeEvent
        {
            add
            {
                SomeEvent += value; // FN
            }

            remove
            {
                SomeEvent -= value; // FN
            }
        }
    }
}
