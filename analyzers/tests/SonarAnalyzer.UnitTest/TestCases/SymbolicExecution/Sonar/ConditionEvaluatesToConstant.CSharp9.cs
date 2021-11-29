using System;
using System.Text;

namespace Tests.Diagnostics
{
    public class CSharp8
    {
        void IsPattern()
        {
            object a = null;
            if (a is null) // Noncompliant
            {
                DoSomething();
            }
        }

        void IsNotPattern()
        {
            var b = "";
            if (b is not null) // FN
            {
                DoSomething();
            }
            else
            {
                DoSomething();
            }
        }

        void ArithmeticComparisonAndPattern()
        {
            int? c = null;
            if (c is > 10 and < 100) // FN
            {
                DoSomething();
            }
        }

        void ArithmeticComparisonOrPattern()
        {
            int c = 10;
            if (c is < 0 or > 100) // FN
            {
                DoSomething();
            }
        }

        void TargetTypedNew()
        {
            StringBuilder s = new();
            if (s is null) // FN
            {
                DoSomething();
            }
        }

        void LambdaStaticDiscardParameters()
        {
            Func<int, int, int, int> func = static (_, i, _) => i * 2;

            if (func is null) // FN
            {
                DoSomething();
            }
        }

        void TargetTypedConditional()
        {
            bool cond = true;
            Fruit f = cond ? new Apple() : new Orange();
//                    ^^^^ {{Change this condition so that it does not always evaluate to 'true'; some subsequent code is never executed.}}
//                                         ^^^^^^^^^^^^ Secondary@-1

            if (f is Apple) // FN
            {
                DoSomething();
            }
            else
            {
                DoSomething();
            }
        }

        void DoSomething() { }
    }

    abstract record Fruit { }
    record Apple : Fruit { }
    record Orange : Fruit { }

    class TestInitOnly
    {
        private object o;
        public object Property
        {
            init
            {
                value = null;
                if (value == null)  // Noncompliant
                {
                    o = value;
                }
                else
                {                   // Secondary
                    o = "";
                }
            }
        }

        public object InitWithTupleAssignment
        {
            init
            {
                var tmp = 0;
                var flag = true;
                while (flag) // Compliant, muted by presence of tuple assignment
                {
                    (flag, tmp) = (false, 5);
                }
                o = value;
            }
        }
    }
}


// See: https://github.com/SonarSource/sonar-dotnet/issues/4880
// See: https://github.com/SonarSource/sonar-dotnet/issues/4104
namespace Repros
{
    using System.Collections;
    using System.Collections.Generic;

    public class SonarExample
    {
        public void TypePattern(IEnumerable values)
        {
            switch (values)
            {
                case IEnumerable<int>: // TypePattern is not supported
                    break;
            }
        }

        public void RelationalPattern(object i)
        {
            switch (i)
            {
                case > 0: // RelationalPattern is not supported
                    break;
            }
        }

        public void AndPattern()
        {
            var value = true;
            switch (value)
            {
                case true and true: // AndPattern is not supported
                    break;
            }
        }

        public void OrPattern()
        {
            var value = true;
            switch (value)
            {
                case true or true: // OrPattern is not supported
                    break;
            }
        }

        public void NotPattern()
        {
            var value = true;
            switch (value)
            {
                case not false: // NotPattern is not supported
                    break;
            }
        }
    }
}
