using System;
using System.Collections.Generic;
using System.Linq;

public class EmptyNullableValueAccess
{
    private IEnumerable<TestClass> Numbers = new[]
    {
        new TestClass() { Number = 42 },
        new TestClass(),
        new TestClass() { Number = 1 },
        new TestClass() { Number = null }
    };

    void NullAssignment()
    {
        int? i = null;
        if (i.HasValue)
        {
            Console.WriteLine(i.Value);
        }

        Console.WriteLine(i.Value); // Noncompliant {{'i' is null on at least one execution path.}}
        //                ^
    }

    IEnumerable<TestClass> EnumerableExpressionWithCompilableCode() =>
        Numbers.OrderBy(x => x.Number.HasValue).ThenBy(i => i.Number);

    IEnumerable<int> EnumerableExpressionWithNonCompilableCode() =>
        Numbers.OrderBy(x => x.Number.HasValue).ThenBy(i => i.Number).Select(x => x.Number ?? 0);

    void NonEmpty()
    {
        int? i = 42;
        if (i.HasValue)
        {
            Console.WriteLine(i.Value);
        }

        Console.WriteLine(i.Value);    // Compliant, non-empty
    }

    void EmptyConstructor()
    {
        int? i = new Nullable<int>();
        if (i.HasValue)
        {
            Console.WriteLine(i.Value);
        }

        Console.WriteLine(i.Value);    // Noncompliant, empty
    }

    void NonEmptyConstructor()
    {
        int? i = new Nullable<int>(42);
        if (i.HasValue)
        {
            Console.WriteLine(i.Value);
        }

        Console.WriteLine(i.Value);
    }

    void ComplexCondition(int? i, double? d, float? f)
    {
        if (i.HasValue && i.Value == 42) { }
        if (i.HasValue == true && i.Value == 42) { }
        if (i.HasValue == !false && i.Value == 42) { }
        if (!!i.HasValue && i.Value == 42) { }

        if (!i.HasValue && i.Value == 42) { }           // FN - parameter should be null-constrained
        if (i.HasValue == false && i.Value == 42) { }   // FN
        if (i.HasValue == !true && i.Value == 42) { }   // FN
        if (!!!i.HasValue && i.Value == 42) { }         // FN
        if (!d.HasValue) { _ = d.Value; }               // FN
        if (f == null) { _ = d.Value; }                 // FN
        if (null == f) { _ = d.Value; }                 // FN
    }

    void Assignment(int? i1)
    {
        {
            int? i2;
            if (i1 == (i2 = null)) { _ = i1.Value; } // Noncompliant
            //                           ^^
        }
    }

    int SwitchExpressions(bool zero)
    {
        int? i = zero switch { true => 0, _ => null };
        return i.Value; // Noncompliant
    }

    int SwitchExpressions2(bool zero)
    {
        int? i = zero switch { true => 0, _ => 1 };
        return i.Value;
    }

    int SwitchExpressions3(int? value)
    {
        return value.HasValue switch { true => value.Value, false => 0 };
    }

    int SwitchExpressions4(int? value)
    {
        return value.HasValue switch { true => 0, false => value.Value }; // FN - switch expressions are not constrained
    }

    int SwitchExpressions5(int? value, bool flag)
    {
        return flag switch { true => value.Value, false => 0 };           // FN - switch expressions are not constrained
    }

    int StaticLocalFunctions(int? param)
    {
        static int ExtractValue(int? intOrNull)
        {
            return intOrNull.Value; // FN - content of static local function is not inspected by SE
        }

        return ExtractValue(param);
    }

    int NullCoalescingAssignment(int? param)
    {
        param ??= 42;
        return param.Value; // OK, value is always set
    }

    class TestClass
    {
        public int? Number { get; set; }
    }
}

class TestLoopWithBreak
{
    public static void LoopWithBreak(System.Collections.Generic.IEnumerable<string> list, bool condition)
    {
        int? i1 = null;
        foreach (string x in list)
        {
            try
            {
                if (condition)
                {
                    Console.WriteLine(i1.Value); // Noncompliant
                }
                break;
            }
            catch (Exception)
            {
                continue;
            }
        }
    }
}

public interface IWithDefaultImplementation
{
    int DoSomething()
    {
        int? i = null;
        return i.Value; // Noncompliant
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/4573
public class Repro_4573
{
    private DateTime? foo;

    public virtual DateTime? Foo
    {
        get => foo;
        set
        {
            if (value.HasValue)
            {
                //HasValue and NoValue constraints are set here
            }
            if (foo != value || foo.HasValue)
            {
                foo = value;
            }
        }
    }

    public void Sequence(DateTime? value)
    {
        if (value.HasValue)
        {
            //HasValue and NoValue constraints are set here
        }
        if (foo == value)   // Relationship is added here
        {
            if (foo.HasValue)
            {
                Console.WriteLine(foo.Value.ToString());
            }
            if (!foo.HasValue)
            {
                Console.WriteLine(foo.Value.ToString());    // FIXME Non-compliant
            }
            if (foo == null)
            {
                Console.WriteLine(foo.Value.ToString());    // Noncompliant
            }
            if (foo != null)
            {
                Console.WriteLine(foo.Value.ToString());
            }
        }
    }

    public void NestedIsolated1(DateTime? value)
    {
        if (foo == value)   // Relationship is added here
        {
            if (value.HasValue)
            {
                if (!foo.HasValue)
                {
                    Console.WriteLine(foo.Value.ToString());    // Compliant
                }
            }
        }
    }

    public void NestedIsolated2(DateTime? value)
    {
        if (foo == value)   // Relationship is added here
        {
            if (!value.HasValue)
            {
                if (foo == null)
                {
                    Console.WriteLine(foo.Value.ToString());    // Noncompliant
                }
            }
        }
    }

    public void NestedCombined(DateTime? value)
    {
        if (foo == value)   // Relationship is added here
        {
            if (value.HasValue)
            {
                if (foo.HasValue)
                {
                    Console.WriteLine(foo.Value.ToString());
                }
                if (!foo.HasValue)
                {
                    Console.WriteLine(foo.Value.ToString());
                }
                if (foo == null)
                {
                    // FIXME the following
                    Console.WriteLine(foo.Value.ToString());    // Noncompliant
                }
                if (foo != null)
                {
                    Console.WriteLine(foo.Value.ToString());
                }
            }
            else
            {
                if (foo.HasValue)
                {
                    Console.WriteLine(foo.Value.ToString());    // Compliant, unreachable
                }
                if (!foo.HasValue)
                {
                    Console.WriteLine(foo.Value.ToString());    // FIXME Non-compliant
                }
                if (foo == null)
                {
                    Console.WriteLine(foo.Value.ToString());    // Noncompliant
                }
                if (foo != null)
                {
                    Console.WriteLine(foo.Value.ToString());    // Compliant, unreachable
                }
            }
        }
    }
}
