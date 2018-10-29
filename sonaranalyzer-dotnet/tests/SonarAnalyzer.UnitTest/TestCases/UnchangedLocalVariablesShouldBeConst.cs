using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class TestUtils
    {
        public static void OutParam(out int x)
        {
            x = 42;
        }

        public static void RefParam(ref int x)
        {
            x = 42;
        }

        public static void NormalCall(int x)
        {
            x = 42;
        }

        public static object GetObj() => new object();

        public static implicit operator TestUtils(int val)
        {
            return new TestUtils();
        }
    }

    class Tests
    {
        // RSPEC sample
        public bool Seek(int[] input)
        {
            int target = 32;  // Noncompliant {{Add the 'const' modifier to 'target'.}}
//              ^^^^^^
            foreach (int i in input)
            {
                if (i == target)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Test_AlreadyConst()
        {
            const int target = 32;

            return false;
        }

        public bool Test_TwoAssignments()
        {
            int target = 32;
            target = 33;

            return false;
        }

        public bool Test_PostIncrement()
        {
            int target = 32;
            target++;

            return false;
        }

        public bool Test_PreDecrement()
        {
            int target = 32;
            --target;

            return false;
        }

        public bool Test_Compare()
        {
            int target = 32; // Noncompliant

            if (target == 32)
            {
                // bla bla
            }

            return false;
        }

        public bool Test_AssignOperators()
        {
            int i = 1, j = 1, k = 1, l = 1, m = 32;

            i <<= 1;
            j *= 1;
            k -= 1;
            l /= 1;
            m %= 1;

            return false;
        }

        public bool Test_MethodInvocation()
        {
            int target = 32; // Noncompliant
            TestUtils.NormalCall(target);

            return false;
        }

        public bool Test_RefParameter()
        {
            int target = 32;
            TestUtils.RefParam(ref target);

            return false;
        }

        public bool Test_OutParameter()
        {
            int target = 32;
            TestUtils.OutParam(out target);

            return false;
        }

        public bool Test_Lambda()
        {
            int target = 32;

            Func<int, int> fun = (t) => { target++; return 1; };

            var x = fun(target);

            return false;
        }

        public bool Test_InnerFunction()
        {
            int target = 32;

            var x = inner(target);

            return false;

            int inner(int t)
            {
                target++;
                return 1;
            }
        }

        public bool Test_MultipleVariables()
        {
            const int const1 = 386, const2 = 486;

            // Noncompliant@+1 {{Add the 'const' modifier to 'var1'.}}
            int var1 = 1, var2 = 2; // Noncompliant {{Add the 'const' modifier to 'var2'.}}
            int var3 = 1, var4 = 2; // Noncompliant {{Add the 'const' modifier to 'var4'.}}

            var3 += 1;

            return false;
        }

        enum Colors { Red, Blue, Green, Nice };

        public void Test_MultipleTypes(int arg)
        {
            char cc = 'c'; // Noncompliant
            int intVar = 1; // Noncompliant
            const int constIntVar = 1;
            int intVar2 = 1 + constIntVar; // Noncompliant
            long longVal = 1; // Noncompliant
            short shortVal = 1; // Noncompliant
            double doubleVal = 1; // Noncompliant
            System.Int16 int16Val = 1; // Noncompliant
            ulong ulongVal = 1; // Noncompliant
            int bufferSize = 512 * 1024; // Noncompliant
            decimal val = 123.456m; // Noncompliant
            double val2 = .1d; // Noncompliant

            Colors c1 = Colors.Nice; // Noncompliant
            Colors c2 = (Colors)1; // Noncompliant
            Colors c3 = 0.0; // Noncompliant

            string str1 = ""; // Noncompliant
            string str2 = null; // Noncompliant
            string str3 = nameof(arg); // Noncompliant
            string str4 = typeof(int).Name;
            string str5 = "a" + "b"; // Noncompliant
            string str6 = @"a" + $"b";
            string str7 = System.Environment.NewLine;
            string str8 = String.Empty;

            object obj1 = null; // Noncompliant
            object obj2 = new object();
            object obj3 = TestUtils.GetObj();
            object obj4 = 1;

            IEnumerable<string> enumerable = null; // Noncompliant

            long? nullable = 100; // Compliant. Nullables cannot be const.
            long? nullable2 = null; // Compliant. Nullables cannot be const.

            dynamic value = 42; // Compliant. Const only works with null.
            TestUtils x = 2; // Compliant (type with implicit conversion). Const only works with null.

            int[] xx = { 1, 2, 3 }; // Compliant (expression on the right is not constant).
            int[] y = new int[] { 1, 2, 3 }; // Compliant (expression on the right is not constant).
            Exception[] z = { }; // Compliant (expression on the right is not constant).
        }

        public int Test_Property
        {
            get
            {
                int a = 1; // Noncompliant
                return a;
            }
            set
            {
                int a = 1;
                int b = 1; // Noncompliant
                a = 2;
            }
        }

        public static Tests operator +(Tests a, Tests b)
        {
            int i = 1; // Noncompliant
            return null;
        }

        public Tests()
        {
            int a = 1; // Noncompliant
        }


        public void Test_FalseNegatives()
        {
            int a;
            a = 1;
        }

        public byte this[int idx]
        {
            get
            {
                var pos = 0;
                var seen = 0;

                seen += 1;
                pos += 1;

                return 1;
            }
        }
    }
}
