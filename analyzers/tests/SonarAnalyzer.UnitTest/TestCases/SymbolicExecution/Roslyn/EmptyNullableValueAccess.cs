using System;
using System.Collections.Generic;
using System.Linq;

class Basics
{
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

    void Assignment1(int? i1)
    {
        int? i2, i3;
        if (i1 == (i2 = null)) { _ = i1.Value; }             // Noncompliant
        //                           ^^
        if ((i1 = 42) != (i2 = null)) { _ = i1.Value; }      // Compliant
        if ((i1 = 42) != (i2 = null)) { _ = i2.Value; }      // Noncompliant
        if ((i1 = 42) != (i2 = i3 = null)) { _ = i2.Value; } // Noncompliant

        bool? b1 = null;
        if (b1) { }                                          // Error[CS0266]
                                                             // Noncompliant@-1 FP
        if (!b1) { }                                         // Error[CS0266]
    }

    void Assignment2(object o)
    {
        if (o != null)
        {
        }

        var b = o as bool?;
        if (b != null)
        {
            _ = b.Value;                                     // Compliant
        }
    }

    void SwitchExpressions(bool zero)
    {
        int? i = zero switch { true => 0, _ => null };
        _ = i.Value; // Noncompliant
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
        return value.HasValue switch { true => 0, false => value.Value }; // Noncompliant
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

class NullableOfCustomTypes
{
    void Assignment(AStruct? nullable)
    {
        _ = nullable.Value;                                              // Compliant, unknown
        _ = (AStruct)nullable;                                           // Compliant, unknown
        _ = new AStruct?(nullable.Value).Value;

        nullable = null;
        _ = nullable.Value;                                              // Noncompliant
        nullable = new AStruct();
        _ = nullable.Value;
        nullable = new AStruct?(new AStruct());
        _ = nullable.Value;

        nullable = new AStruct?();
        _ = nullable.Value;                                              // Noncompliant
        nullable = new Nullable<AStruct>();
        _ = nullable.Value;                                              // Noncompliant

        _ = (new Nullable<AStruct>()).Value;                             // Noncompliant
        _ = (null as Nullable<AStruct>).Value;                           // Noncompliant

        nullable = null;
        _ = ((AStruct?)nullable).Value;                                  // Noncompliant
        _ = (nullable as AStruct?).Value;                                // FN
        _ = ((AStruct?)(nullable as AStruct?)).Value;                    // FN
    }

    void ForeachCast()
    {
        foreach (AStruct x in new AStruct?[] { null }) ;                 // FN
        foreach (AStruct x in new AStruct?[] { new AStruct() }) ;        // Compliant, all items not null
        foreach (AStruct x in new AStruct?[] { new AStruct(), null }) ;  // FN
        foreach (AStruct? x in new AStruct?[] { new AStruct(), null }) ; // Compliant, no value access
        foreach (var x in new AStruct?[] { new AStruct(), null }) ;      // Compliant, no value access

        foreach ((AStruct?, AStruct?) x in new (AStruct?, AStruct?)[] { (new AStruct(), null) }) ;  // Compliant, no value access
        foreach ((AStruct, AStruct?) x in new (AStruct?, AStruct?)[] { (new AStruct(), null) }) ;   // Compliant, value access on first item of the couple
        foreach ((AStruct?, AStruct) x in new (AStruct?, AStruct?)[] { (new AStruct(), null) }) ;   // FN, value access on second item of the couple
        foreach ((AStruct?, AStruct) x in
            new (AStruct?, AStruct?)[] { (new AStruct(), new AStruct()), (new AStruct(), null) }) ; // FN, value access on second item of the couple
    }

    void ForeachDestructuring()
    {
        foreach ((var x, var y) in new (AStruct?, AStruct?)[] { (new AStruct(), null) }) ;
        foreach ((AStruct? x, AStruct y) in new (AStruct?, AStruct?)[] { (new AStruct(), null) }) ; // Error[CS0266]
        foreach ((var x, AStruct y) in new (AStruct?, AStruct?)[] { (new AStruct(), null) }) ;      // Error[CS0266]
    }

    private struct AStruct { }
}

class Arithmetic
{
    int? MultiplicationByZeroAndAssignment(int? i) => 0 * (i = null) * i.Value;    // Noncompliant
    int? MultiplicationAndAssignments1(int? i) => (i = 42) * (i = null) * i.Value; // Noncompliant
    int? MultiplicationAndAssignments2(int? i) => (i = null) * (i = 42) * i.Value; // Compliant
}

class ComplexConditionsSingleNullable
{
    bool LogicalAndLearningNonNull1(bool? b) => b.HasValue && b.Value;
    bool LogicalAndLearningNonNull2(bool? b) => b.HasValue == true && b.Value;
    bool LogicalAndLearningNonNull3(bool? b) => b.HasValue != false && b.Value;
    bool LogicalAndLearningNonNull4(bool? b) => b.HasValue == !false && b.Value;
    bool LogicalAndLearningNonNull5(bool? b) => !!b.HasValue && b.Value;

    bool LogicalAndLeaningNull1(bool? b) => !b.HasValue && b.Value;         // Noncompliant - parameter should be null-constrained
    bool LogicalAndLeaningNull2(bool? b) => b.HasValue == false && b.Value; // Noncompliant
    bool LogicalAndLeaningNull3(bool? b) => b.HasValue == !true && b.Value; // Noncompliant
    bool LogicalAndLeaningNull4(bool? b) => !!!b.HasValue && b.Value;       // Noncompliant

    bool BitAndLearningNull(bool? b) => !b.HasValue & b.Value;              // Noncompliant - i.Value reached in any case
    bool BitAndLearningNonNull(bool? b) => b.HasValue & b.Value;            // Noncompliant - i.Value reached in any case

    bool LogicalOrLearningNonNull(bool? b) => b.HasValue || b.Value;        // Noncompliant - i.Value reached when i.HasValue is false

    bool BitOrLearningNonNull(bool? b) => b.HasValue | b.Value;             // Noncompliant - i.Value reached in any case

    bool Tautology(bool? b) => (!b.HasValue || b.HasValue) && b.Value;      // Noncompliant

    bool ShortCircuitedOr(bool? b) => (true || b.HasValue) && b.Value;      // Compliant
    bool ShortCircuitedAnd(bool? b) => (false &&  b.HasValue) || b.Value;   // Compliant

    bool XorWithTrue(bool? b) => (true ^ b.HasValue) && b.Value;            // Noncompliant
    bool XorWithFalse(bool? b) => (false ^ b.HasValue) && b.Value;          // Compliant
}

class ComplexConditionMultipleNullables
{
    bool IndependentConditions1(double? d, float? f) => f == null && d.Value == 42.0;  // Compliant, f imposes no constraints on d
    bool IndependentConditions2(double? d, float? f) => f != null && d.Value == 42.0;  // Compliant
    bool IndependentConditions3(double? d, float? f) => f.HasValue && d.Value == 42.0; // Compliant
    bool IndependentConditions4(double? d, float? f) => null == f && d.Value == 42.0;  // Compliant
    bool IndependentConditions5(double? d, float? f) => null == f && d.Value == 42.0;  // Compliant

    bool DependentConditions1(double? d, float? f) =>
        !d.HasValue && d.Value == 42.0;                                 // Noncompliant
    bool DependentConditions2(double? d, float? f) =>
        d.Value == 42.0 && d.HasValue;                                  // Compliant, d is constrained after access
    bool DependentConditions3(double? d, float? f) =>
        d.Value == 42.0 && d.HasValue == f.HasValue && f.Value == 42.0; // Compliant, d is non-null, as well as f
    bool DependentConditions4(double? d, float? f) =>
        d.Value == 42.0 && d.HasValue != f.HasValue && f.Value == 42.0; // Noncompliant, d is non-null, unlike f
}

class TernaryOperator
{
    bool Truth(bool? b) => true ? b.Value : false;                          // Compliant
    bool Tautology(bool? b) => b.HasValue || !b.HasValue ? b.Value : false; // Noncompliant
    bool Falsity(bool? b) => false ? b.Value : b.Value;                     // Compliant
    bool HasValue1(bool? b) => b.HasValue ? b.Value : false;                // Compliant
    bool HasValue2(bool? b) => b.HasValue ? true : b.Value;                 // Noncompliant

    bool TruthAndAssignment(bool? b) =>
        true || ((b = null) == null) ? b.Value : false;                     // FN
}

class Linq
{
    private IEnumerable<TestClass> Numbers = new[]
    {
        new TestClass() { Number = 42 },
        new TestClass(),
        new TestClass() { Number = 1 },
        new TestClass() { Number = null }
    };

    IEnumerable<int> EnumerableOfEmptyNullableValues1 =>
        Numbers.Where(x => !x.Number.HasValue).Select(x => x.Number.Value); // FN

    IEnumerable<int> EnumerableOfEmptyNullableValues2 =>
        Numbers.Select(x => null as int?).Select(x => x.Value);             // FN

    class TestClass
    {
        public int? Number { get; set; }
    }
}

interface IWithDefaultImplementation
{
    int DoSomething()
    {
        int? i = null;
        return i.Value; // Noncompliant
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/4573
class Repro_4573
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
            // HasValue and NoValue constraints are set here
        }
        if (foo == value)   // Relationship is added here
        {
            if (foo.HasValue)
            {
                Console.WriteLine(foo.Value.ToString());
            }
            if (!foo.HasValue)
            {
                Console.WriteLine(foo.Value.ToString());    // FN
            }
            if (foo == null)
            {
                Console.WriteLine(foo.Value.ToString());    // FN
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
                    Console.WriteLine(foo.Value.ToString());    // FN
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
                    Console.WriteLine(foo.Value.ToString());    // Compliant, non-empty
                }
                if (!foo.HasValue)
                {
                    Console.WriteLine(foo.Value.ToString());    // Compliant, unreachable
                }
                if (foo == null)
                {
                    Console.WriteLine(foo.Value.ToString());    // Compliant, unreachable
                }
                if (foo != null)
                {
                    Console.WriteLine(foo.Value.ToString());    // Compliant, non-empty
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
                    Console.WriteLine(foo.Value.ToString());    // FN, empty
                }
                if (foo == null)
                {
                    Console.WriteLine(foo.Value.ToString());    // FN, empty
                }
                if (foo != null)
                {
                    Console.WriteLine(foo.Value.ToString());    // Compliant, unreachable
                }
            }
        }
    }
}
