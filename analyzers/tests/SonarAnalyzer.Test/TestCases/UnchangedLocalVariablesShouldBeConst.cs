using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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
            char cc = 'c';              // Noncompliant
            int intVar = 1;             // Noncompliant
            const int constIntVar = 1;
            int intVar2 = 1 + constIntVar;  // Noncompliant
            long longVal = 1;           // Noncompliant
            short shortVal = 1;         // Noncompliant
            double doubleVal = 1;       // Noncompliant
            System.Int16 int16Val = 1;  // Noncompliant
            ulong ulongVal = 1;         // Noncompliant
            int bufferSize = 512 * 1024;    // Noncompliant
            decimal val = 123.456m;     // Noncompliant
            double val2 = .1d;          // Noncompliant

            Colors c1 = Colors.Nice;    // Noncompliant
            Colors c2 = (Colors)1;      // Noncompliant
            Colors c3 = 0.0;            // Noncompliant

            string str1 = "";           // Noncompliant
            string str2 = null;         // Noncompliant
            string str3 = nameof(arg);  // Noncompliant
            string str4 = typeof(int).Name;
            string str5 = "a" + "b";    // Noncompliant
            string str6 = @"a" + $"b";  // Noncompliant. Was FN until Roslyn 3.11.0.
            string str7 = System.Environment.NewLine;
            string str8 = String.Empty;

            object obj1 = null;         // Noncompliant
            object obj2 = new object();
            object obj3 = TestUtils.GetObj();
            object obj4 = 1;

            IEnumerable<string> enumerable = null; // Noncompliant

            long? nullable = 100;   // Compliant. Nullables cannot be const.
            long? nullable2 = null; // Compliant. Nullables cannot be const.

            dynamic value = 42; // Compliant. Const only works with null.
            TestUtils x = 2;    // Compliant (type with implicit conversion). Const only works with null.

            int[] xx = { 1, 2, 3 };             // Compliant (expression on the right is not constant).
            int[] y = new int[] { 1, 2, 3 };    // Compliant (expression on the right is not constant).
            Exception[] z = { };                // Compliant (expression on the right is not constant).
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

        static readonly Func<string, bool> LambdaExpressionSyntax = year =>
        {
            bool isValid = false; // Compliant

            if (short.TryParse(year, out short shortYear))
            {
                isValid = true;
            }

            return isValid;
        };

        static readonly Func<string, bool> ParenthesizedLambdaExpressionSyntax = (year) =>
        {
            bool isValid = false; // Compliant

            if (short.TryParse(year, out short shortYear))
            {
                isValid = true;
            }

            return isValid;
        };

        static readonly Func<string, bool> UnchangedLocalValiableInLambda = year =>
        {
            bool isValid = false; // Noncompliant

            return isValid;
        };
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3271
    public class Repro_3271
    {
        public void GoGoGo()
        {
            var tmp = 0;
            var flag = true;
            (flag, tmp) = (false, 5);
        }

        public void UninitializedTuple()
        {
            int tmp;
            bool flag;
            (flag, tmp) = (false, 5);
        }

        public void ReadInTuple()
        {
            int tmp = 5;            // Noncompliant
            bool flag = false;      // Noncompliant
            var x = (flag, tmp);
        }

        public struct StructWithImplicitOperator
        {
            public static implicit operator StructWithImplicitOperator(int value) { return new StructWithImplicitOperator(); }
        }

        public StructWithImplicitOperator ImplicitOperatorStruct()
        {
            StructWithImplicitOperator x = 1;
            return x;
        }

        public class ClassWithImplicitOperator
        {
            public static implicit operator ClassWithImplicitOperator(int value) { return new ClassWithImplicitOperator(); }
        }

        public ClassWithImplicitOperator ImplicitOperatorClass()
        {
            ClassWithImplicitOperator x = 1;
            return x;
        }

        public class NormalClass { }

        public NormalClass NormalClassSetToNull()
        {
            NormalClass x = null; // Noncompliant
            return x;
        }
    }
}

class ShadowingTest
{
    private int Counter = 0;
    public void Tests()
    {
        int Counter = 1; // Noncompliant
        this.Counter++;
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/4015
public class Repro_4015
{
    Expression<Func<string>> fieldExpression;
    Func<string> fieldFunc;

    public void Compliant_SimpleLambda()
    {
        string localVariable = null;    // Compliant, changing the Expression<Func<T>> below changes the behavior of the code
        WithExpressionArgument(x => localVariable);
    }

    public void Compliant_ParenthesizedLambda()
    {
        string localVariable = null;    // Compliant, changing the Expression<Func<T>> below changes the behavior of the code
        WithExpressionArgument(() => localVariable);
    }

    public void Compliant_ParenthesizedExpression()
    {
        string localVariable = null;
        WithExpressionArgument(((() => localVariable)));
    }

    public void Noncompliant_Func()
    {
        string localVariable = null;    // Noncompliant, this can be const
        WithFuncArgument(() => localVariable);
    }

    public void Compliant_Assignment()
    {
        string a = null;
        string b = null;
        string c = null;
        string d = null;
        var repro = new Repro_4015();
        Expression<Func<string>> variableExpression;

        // Compliant cases, changing the Expression<Func<T>> below changes the behavior of the code
        fieldExpression = () => a;
        WithExpressionArgument(fieldExpression);

        repro.fieldExpression = () => b;
        WithExpressionArgument(repro.fieldExpression);

        variableExpression = () => c;
        WithExpressionArgument(variableExpression);

        variableExpression = ((() => d));
        WithExpressionArgument(variableExpression);
    }

    public void Noncompliant_Assignment()
    {
        string a = null;    // Noncompliant
        string b = null;    // Noncompliant
        string c = null;    // Noncompliant
        var repro = new Repro_4015();
        Func<string> variableFunc;

        fieldFunc = () => a;
        WithFuncArgument(fieldFunc);

        repro.fieldFunc = () => b;
        WithFuncArgument(repro.fieldFunc);

        variableFunc = () => c;
        WithFuncArgument(variableFunc);
    }

    private void WithExpressionArgument(Expression<Func<string>> expression)
    {
        if (!(expression.Body is MemberExpression))
        {
            throw new InvalidOperationException($"The expression body is a {expression.Body.GetType().Name}, but MemberExpression is expected.");
        }
    }

    private void WithExpressionArgument(Expression<Func<string, string>> expression)
    {
        if (!(expression.Body is MemberExpression))
        {
            throw new InvalidOperationException($"The expression body is a {expression.Body.GetType().Name}, but MemberExpression is expected.");
        }
    }

    private void WithFuncArgument(Func<string> expression)
    {
        expression();
    }

    public void UndefinedInvocationSymbol()
    {
        string localVariable = null;        // Noncompliant
        Undefined(() => localVariable);     // Error [CS0103]: The name 'Undefined' does not exist in the current context
        undefined = () => localVariable;    // Error [CS0103]: The name 'undefined' does not exist in the current context
    }

    public void IndexerIndexedByLambda_CrazyCoverage()
    {
        string localVariable = null;        // Noncompliant
        this[() => localVariable] = 42;     // Case with lambda on the left side of an assignment
    }

    public int this[Func<string> key]
    {
        get
        {
            var k = key();
            return 42;
        }
        set { }
    }
}
