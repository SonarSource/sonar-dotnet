using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        void ArithmeticComparisonAndPattern_Null()
        {
            int? c = null;
            if (c is > 10 and < 100)    // Noncompliant
            {
                DoSomething();          // Secondary
            }
        }

        void ArithmeticComparisonOrPattern_Value()
        {
            int c = 10;
            if (c is < 0 or > 100)      // Noncompliant
            {
                DoSomething();          // Secondary
            }
            if (c is > 0)               // Noncompliant
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
                while (flag)        // Compliant
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

    public class VarWhen_FP
    {
        public int Test(Flags flags)
        {
            return flags switch
            {
                var value when value.HasFlag(Flags.Foo) => 1,   // Compliant
                var value when value.HasFlag(Flags.Bar) => 2,   // Compliant
                _ => 0
            };
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8008
public class Repro_8008
{
    public int Compare(string name1, string name2) => (name1, name2) switch
    {
        (null, null) => 0,
        (null, _) => 1,
        (_, null) => -1,
        (_, _) => Comparer<string>.Default.Compare(name1, name2)
    };

    public int NestedTuples(string name1, string name2, string name3) => ((name1, name2), name3) switch
    {
        ((null, null), null) => 42,
        ((null, null), _) => 43,
        ((_, _), null) => 44,
        ((_, _), _) => 45
    };

    public int DeeperNestedTuples(string name1, string name2, string name3, string name4) => (((name1, name2), name3), name4) switch
    {
        (((null, null), null), null) => 42,
        (((_, _), null), null) => 43,
        (((_, _), _), null) => 44,
        (((_, _), _), _) => 45,
    };

    public int TupleWithKnownNullConstraint()
    {
        string name1 = null;
        string name2 = null;
        return (name1, name2) switch
        {
            (null, null) => 0,  // FN
            (null, _) => 1,     // FN
            (_, null) => -1,    // FN
            (_, _) => Comparer<string>.Default.Compare(name1, name2)
        };
    }

    public int TupleWithDeclaration(string name1, string name2) =>
        (name1, name2) switch
        {
            ("A", "B") => 0,
            (var x, _) => -1,    // Noncompliant - FP
        };
}

// https://github.com/SonarSource/sonar-dotnet/issues/9051
class Repro_9051
{
    string Method(bool b) =>
        TrySomething(out var s) switch
        {
            true => s,
            false => ""     // Noncompliant FP
        };

    bool TrySomething([NotNullWhen(true)] out string outValue) => throw new NotImplementedException();
}
