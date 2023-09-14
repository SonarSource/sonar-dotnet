using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Diagnostics
{
    public class Patterns
    {
        void IsPattern()
        {
            object a = null;
            if (a is null)              // Noncompliant
            {
                DoSomething();
            }
        }

        void IsNotPattern()
        {
            var b = "";
            if (b is not null)          // Noncompliant
            {
                DoSomething();
            }
            else
            {
                DoSomething();          // Secondary
            }
        }

        void ArithmeticComparisonAndPattern()
        {
            int? c = null;
            if (c is > 10 and < 100)    // FN
            {
                DoSomething();
            }
        }

        void ArithmeticComparisonOrPattern()
        {
            int c = 10;
            if (c is < 0 or > 100)      // FN
            {
                DoSomething();
            }
        }

        void TargetTypedNew()
        {
            StringBuilder s = new();
            if (s is null)              // Noncompliant
            {
                DoSomething();          // Secondary
            }
        }

        void LambdaStaticDiscardParameters()
        {
            Func<int, int, int, int> func = static (_, i, _) => i * 2;

            if (func is null)           // Noncompliant
            {
                DoSomething();          // Secondary
            }
        }

        void TargetTypedConditional()
        {
            bool cond = true;
            Fruit f = cond ? new Apple() : new Orange();    // Noncompliant
                                                            // Secondary@-1
            if (f is Apple)             // FN
            {
                DoSomething();
            }
            else
            {
                DoSomething();
            }
        }

        void SwitchStatement(int score)
        {
            int newScore = score switch
            {
                > 90 => 1,
                _ => -1, // This is a discard pattern and is always true - it's exempted in the code to not raise an issue.
            };
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
                {
                    o = "";         // Secondary
                }
            }
        }

        public object InitWithTupleAssignment
        {
            init
            {
                var tmp = 0;
                var flag = true;
                while (flag)        // Noncompliant
                {
                    (flag, tmp) = (false, 5);
                }
                o = value;          // Secondary
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

// https://github.com/SonarSource/sonar-dotnet/issues/7096
namespace Repro_7096
{
    public interface I { }
    public class A : I { }
    public class B : I { }

    class C
    {
        public bool OrElsePattern(I obj) =>
            obj is A { } || obj is B { };   // Compliant

        public bool OrElse(I obj) =>
            obj is A || obj is B;

        public bool Or(I obj) =>
            obj is A { } or B { };
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/5002
namespace Repro_5002
{
    public class A { public string Inner { get; set; } }
    public class B { public string Inner { get; set; } }
    public class C { public string Inner { get; set; } }
    public class D { public string Inner { get; set; } }

    class Repro
    {
        static void Demo(object demo)
        {
            if (demo is A { Inner: var innerA })            // Compliant
            {
                Console.WriteLine($"A {innerA}");
            }
            else if (demo is B { Inner: var innerB })       // Compliant
            {
                Console.WriteLine($"B {innerB}");
            }
            else if (demo is C { Inner: string innerC })    // Compliant
            {
                Console.WriteLine($"C {innerC}");
            }
            else if (demo is D { Inner: "Test" })           // Compliant
            {
                Console.WriteLine($"D Test");
            }
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8011
namespace Repro_8011
{
    [Flags]
    public enum Flags
    {
        None,
        Foo,
        Bar
    }

    public class VarWhen
    {
        public int Test(Flags flags)
        {
            return flags switch
            {
                var value when value.HasFlag(Flags.Foo) => 1, // Noncompliant FP
              //^^^^^^^^^

                var value when value.HasFlag(Flags.Bar) => 2, // Noncompliant FP
              //^^^^^^^^^
                _ => 0
            };
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8008
namespace Repro_8008
{
    class Person
    {
        public int? Age { get; set; }
    }
    // ...
    class Double_Wildcard
    {
        public int Compare(Person x, Person y) => (x.Age, y.Age) switch
        {
            (null, null) => 0,
            (null, _) => 1,
            (_, null) => -1,
            (_, _) => Comparer<int>.Default.Compare(x.Age.Value, y.Age.Value) // Noncompliant FP
          //^^^^^^
        };
    }
}
