using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class Basics
{
    void NullAssignment()
    {
        int? i = null;
        if (i.HasValue)
        {
            Console.WriteLine(i.Value);
        }

        Console.WriteLine(i.Value);    // Noncompliant {{'i' is null on at least one execution path.}}
        //                ^
    }

    void DereferenceOnValueResult(int? i)
    {
        _ = i.Value.ToString();        // Compliant, unknown
        i = null;
        _ = i.Value.ToString();        // Noncompliant
        //  ^
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

    void AssignmentAndNullComparison(object o)
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

    void AssignmentTransitivity()
    {
        bool? b1 = null;
        bool? b2 = b1;
        _ = b1.Value;                                        // Noncompliant
    }

    void TwoWaySwappingViaTemporaryVar()
    {
        bool? b1 = null;
        bool? b2 = true;
        bool? t = b1;
        b1 = b2;
        b2 = t;
        _ = b1.Value;           // Compliant, after swapping is non-empty
        _ = b2.Value;           // Noncompliant, after swapping is empty
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
        return flag switch { true => value.Value, false => 0 };           // Compliant, constraint on flag doesn't constrain value
    }

    int StaticLocalFunctions(int? param)
    {
        return ExtractValue(param);

        static int ExtractValue(int? intOrNull)
        {
            intOrNull = null;
            return intOrNull.Value;   // Noncompliant
        }
    }
}

class AssignmentAndDeconstruction
{
    void TypeInference()
    {
        (int? discard, int? b) = (null, null);
        _ = b.Value;        // Noncompliant
    }

    void FirstLevel()
    {
        var (b, _) = (null as bool?, null as bool?);
        _ = b.Value;        // Noncompliant
    }

    void SecondLevel()
    {
        (int? i1, (int? i2, int? i3)) = (42, (42, null));
        _ = i1.Value;       // Compliant
        _ = i2.Value;       // Compliant
        _ = i3.Value;       // Noncompliant
    }

    void ThirdLevel()
    {
        (int? i1, (int? i2, (int? i3, int? i4), int? i5)) = (42, (42, (42, null), 42));
        _ = i1.Value;       // Compliant
        _ = i2.Value;       // Compliant
        _ = i3.Value;       // Compliant
        _ = i4.Value;       // Noncompliant
        _ = i5.Value;       // Compliant
    }

    void WithDiscard()
    {
        (_, (int? i1, (int?, int?) _, int? i2)) = (42, (42, (42, null), null));
        _ = i1.Value;       // Compliant
        _ = i2.Value;       // Noncompliant
    }

    void TwoWaySwapping()
    {
        bool? b1 = null;
        bool? b2 = true;
        (b1, b2) = (b2, b1);
        _ = b1.Value;       // Compliant: after swapping is non-empty
        _ = b2.Value;       // Noncompliant
    }

    void ThreeWaySwapping()
    {
        bool? b1 = null;
        bool? b2 = true;
        bool? b3 = null;
        (b1, b2, b3) = (b2, b3, b2);
        _ = b1.Value;       // Compliant: after swapping is non-empty
        _ = b2.Value;       // Noncompliant
        _ = b3.Value;       // Compliant: after swapping is non-empty
    }

    void CustomDeconstruction()
    {
        var deconstructed = new DeconstructableStruct();
        (_, (int? i1, (int? i2, int? i3), int? i4)) = (42, (42, deconstructed, null));
        _ = i1.Value;       // Compliant
        _ = i2.Value;       // Compliant, unknown
        _ = i3.Value;       // Compliant, unknown
        _ = i4.Value;       // Noncompliant
    }

    struct DeconstructableStruct
    {
        public void Deconstruct(out int? v1, out int? v2)
        {
            v1 = null;
            v2 = null;
        }
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
        _ = nullable.Value;                           // Compliant, unknown
        nullable = MethodReturningEmpty();
        _ = nullable.Value;                           // Compliant, unknown
        nullable = MethodReturningNonEmpty();
        _ = nullable.Value;                           // Compliant, unknown
        nullable = MethodReturningEmpty();
        _ = (AStruct)nullable;                        // Compliant, unknown
        nullable = MethodReturningNonEmpty();
        _ = (AStruct)nullable;                        // Compliant, unknown
        nullable = MethodReturningEmpty();
        _ = new AStruct?(nullable.Value).Value;       // Compliant, unknown then non-empty
        nullable = MethodReturningNonEmpty();
        _ = new AStruct?(nullable.Value).Value;       // Compliant, unknown then non-empty

        nullable = new AStruct();
        _ = nullable.Value;                           // Compliant, non-empty
        nullable = new AStruct?(new AStruct());
        _ = nullable.Value;                           // Compliant, non-empty

        nullable = null;
        _ = nullable.Value;                           // Noncompliant, empty
        nullable = new AStruct?();
        _ = nullable.Value;                           // Noncompliant, empty
        nullable = new Nullable<AStruct>();
        _ = nullable.Value;                           // Noncompliant, empty

        _ = (new Nullable<AStruct>()).Value;          // Noncompliant
        _ = (null as Nullable<AStruct>).Value;        // Noncompliant

        nullable = null;
        _ = ((AStruct?)nullable).Value;               // Noncompliant
        _ = (nullable as AStruct?).Value;             // Compliant, when reached .Value above implies nullable is not null
        _ = ((AStruct?)(nullable as AStruct?)).Value; // Compliant, same as above

        static AStruct? MethodReturningEmpty() => null;
        static AStruct? MethodReturningNonEmpty() => new AStruct();
    }

    void ForeachCast()
    {
        foreach (AStruct x in new AStruct?[] { null }) ;                 // FN
        foreach (AStruct x in new AStruct?[] { new AStruct() }) ;        // Compliant, all items are not empty
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

class Comparisons
{
    void NullIsDifferentThanANumber(int? i)
    {
        i = null;
        if (i == 42) _ = i.Value;           // Compliant, 42, therefore non-empty
        if (i == 0 || i == 1) _ = i.Value;  // Compliant, 0 or 1, therefore non-empty
    }

    void NullIsNeitherSmallerNorBiggerThanANumber(int? i)
    {
        i = null;
        if (i > 0) _ = i.Value;                 // Compliant, positive, therefore non-empty

        i = null;
        if (i < 0) _ = i.Value;                 // Compliant, negative, therefore non-empty

        i = Unknown();
        if (i > null) _ = (null as int?).Value; // Compliant, relational against null is always false, so unreachable

        i = Unknown();
        if (i < null) _ = (null as int?).Value; // Compliant, relational against null is always false, so unreachable

        static int? Unknown() => null;
    }
}

class ComplexConditionsSingleNullable
{
    bool LogicalAndLearningNonNull1(bool? b) => b.HasValue && b.Value;
    bool LogicalAndLearningNonNull2(bool? b) => b.HasValue == true && b.Value;
    bool LogicalAndLearningNonNull3(bool? b) => b.HasValue != false && b.Value;
    bool LogicalAndLearningNonNull4(bool? b) => b.HasValue == !false && b.Value;
    bool LogicalAndLearningNonNull5(bool? b) => !!b.HasValue && b.Value;
    bool LogicalAndLearningNonNull6(bool? b) => b.Value && b.HasValue;

    bool LogicalAndLeaningNull1(bool? b) => !b.HasValue && b.Value;         // Noncompliant - parameter should be null-constrained
    bool LogicalAndLeaningNull2(bool? b) => b.HasValue == false && b.Value; // Noncompliant
    bool LogicalAndLeaningNull3(bool? b) => b.HasValue == !true && b.Value; // Noncompliant
    bool LogicalAndLeaningNull4(bool? b) => !!!b.HasValue && b.Value;       // Noncompliant

    bool BitAndLearningNull(bool? b) => !b.HasValue & b.Value;              // Noncompliant - i.Value reached in any case
    bool BitAndLearningNonNull(bool? b) => b.HasValue & b.Value;            // Noncompliant - i.Value reached in any case

    bool LogicalOrLearningNonNull(bool? b) => b.HasValue || b.Value;        // Noncompliant - i.Value reached when i.HasValue is false

    bool BitOrLearningNonNull(bool? b) => b.HasValue | b.Value;             // Noncompliant - i.Value reached in any case

    bool Tautology(bool? b) => (!b.HasValue || b.HasValue) && b.Value;      // Noncompliant

    bool ShortCircuitedOr(bool? b) => (true || b.HasValue) && b.Value;      // FN, won't fix
    bool ShortCircuitedAnd(bool? b) => (false && b.HasValue) || b.Value;    // FN, won't fix

    bool XorWithTrue(bool? b) => (true ^ b.HasValue) && b.Value;            // Noncompliant
    bool XorWithFalse(bool? b) => (false ^ b.HasValue) && b.Value;          // Compliant

    void ReachabilityIsTakenIntoAccount()
    {
        bool? b = null;
        _ = true || b.Value; // Compliant, "||" is short-circuited
        _ = b.Value;         // Noncompliant
        _ = b.Value;         // Compliant, when reached previous b.Value implies b is not null
    }

    void ReachabilityStateIsPreserved()
    {
        bool? b = null;
        _ = true | b.Value;  // Noncompliant, "|" evaluates both sides
        _ = b.Value;         // Compliant, when reached previous b.Value implies b is not null
        _ = b.Value;         // Compliant, same as above
    }

    bool LiftedNot(bool? b) => !b == b && b.Value; // FN, b.Value reached only when b is empty
}

class ComplexConditionMultipleNullables
{
    bool IndependentConditions1(double? d, float? f) => f == null && d.Value == 42.0;       // Compliant, f imposes no constraints on d
    bool IndependentConditions2(double? d, float? f) => f != null && d.Value == 42.0;       // Compliant
    bool IndependentConditions3(double? d, float? f) => f.HasValue && d.Value == 42.0;      // Compliant
    bool IndependentConditions4(double? d, float? f) => null == f && d.Value == 42.0;       // Compliant
    bool IndependentConditions5(double? d, float? f) => null == f && d.Value == 42.0;       // Compliant

    bool DependentConditions1(double? d, float? f) =>
        !d.HasValue && d.Value == 42.0;                                 // Noncompliant, f presence doesn't affect d
    bool DependentConditions2(double? d, float? f) =>
        d.Value == 42.0 && d.HasValue == f.HasValue && f.Value == 42.0; // Compliant, d is non-null, as well as f
    bool DependentConditions3(double? d, float? f) =>
        d.Value == 42.0 && d.HasValue != f.HasValue && f.Value == 42.0; // Noncompliant, d is non-null, unlike f

    void ThirdExcluded(bool? b1, bool? b2)
    {
        if (b1 == b2 && b1 != b2) { _ = (null as int?).Value; }                             // Noncompliant, FP: logically unreachable
    }

    void Transitivity(bool? b1, bool? b2, bool? b3)
    {
        if (b1 == b2 && b1 != b3 && b2 == b3) { _ = (null as int?).Value; }                 // Noncompliant, FP: logically unreachable
        if (b1 != b2 && b1 != b3 && b2 != b3 && b1 != null && b2 != null) { _ = b3.Value; } // FN: b3 is empty
    }

    void RelationsEquality(bool? b1)
    {
        bool? b2 = null;
        if (b1 == b2)
        {
            _ = b1.Value;     // Noncompliant
            _ = b2.Value;     // Noncompliant, FP: when reached previous b1.Value should imply b1 is not null, hence b2
        }
    }

    void RelationsBitOr(bool? b1, bool? b2)
    {
        _ = b1.Value | b2.Value;
        _ = b1.Value;         // Compliant, "|" evaluates both sides
        _ = b2.Value;         // Compliant, "|" evaluates both sides
    }
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

class NullConditionalOperator
{
    void Basics(int? i)
    {
        i = Unknown();
        _ = (i?.GetHashCode()).Value;                  // Noncompliant, ?. branches the state of i
        i = Unknown();
        _ = ((i?.GetHashCode())?.GetHashCode()).Value; // Noncompliant
        i = Unknown();
        _ = (i?.GetHashCode().GetHashCode()).Value;    // Noncompliant

        static int? Unknown() => null;
    }

    void WithFuncOfNullable(Func<int?> f)
    {
        f = Unknown();
        _ = f().Value;                // Compliant, f unknown
        f = Unknown();
        _ = f.Invoke().Value;         // Compliant, f unknown
        f = Unknown();
        _ = f?.Invoke().Value;        // Compliant, Value never called when f is null
        f = Unknown();
        _ = (f?.Invoke()).Value;      // Noncompliant, ?. branches the state of f, and Value called on a null path
        f = Unknown();
        _ = (f?.Invoke()).ToString(); // Compliant, ?. branches the state of f, but Value never called anyway
        f = Unknown();
        _ = f?.Invoke()?.ToString();  // Compliant, ?. branches the state of f, but Value never called anyway

        static Func<int?> Unknown() => null;
    }

    void WithFuncOfFuncOfNullable(Func<Func<int?>> f)
    {
        f = Unknown();
        _ = f()().Value;               // Compliant, f unknown
        f = Unknown();
        _ = f?.Invoke()().Value;       // Compliant, Value never called when f is null
        f = Unknown();
        _ = (f?.Invoke()()).Value;     // Noncompliant, ?. branches the state of f
        f = Unknown();
        _ = (f?.Invoke())().Value;     // Compliant, ?. branches the state of f, but f.Invoke() is unknown

        static Func<Func<int?>> Unknown() => null;
    }
}

class ValueTuple
{
    void MembersAccess((int? first, int? second)? tuple, int? i)
    {
        tuple = Unknown();
        _ = tuple.Value;              // Compliant, unknown
        tuple = Unknown();
        _ = tuple.Value.first.Value;  // Compliant, tuple unknown, then first unknown
        _ = tuple.Value.second.Value; // Compliant, tuple unknown, then second unknown
        tuple = (null, i);
        _ = tuple.Value;              // Compliant, non-empty
        _ = tuple.Value.first.Value;  // FN, empty
        _ = tuple.Value.second.Value; // Compliant, second still unknown
        tuple = (i, null);
        _ = tuple.Value;              // Compliant, non-empty
        _ = tuple.Value.first.Value;  // Compliant, first still unknown
        _ = tuple.Value.second.Value; // FN, empty

        static (int? first, int? second)? Unknown() => null;
    }
}

class NullCoaleascingAssignment
{
    void WithNullableOfInt(int? i1, int? i2, int? i3)
    {
        i1 ??= 42;
        _ = i1.Value;  // Compliant, always non-empty
        i1 = null;
        i1 ??= null;
        _ = i1.Value;  // Noncompliant, always empty
        i1 = 42;
        i1 ??= null;
        _ = i1.Value;  // Compliant, always non-empty

        i2 ??= i3;
        _ = i2.Value;  // Compliant, both i2 and i3 are unknown
    }

    void WithNullableOfValueTuple()
    {
        (int? first, int? second)? tuple = null;
        tuple ??= (42, 42);
        _ = tuple.Value.first.Value;  // Compliant
        _ = tuple.Value.second.Value; // Compliant, both tuple and tuple.Value.second non-empty
        tuple = null;
        tuple ??= (42, 42);
        _ = tuple.Value.second.Value; // Compliant
        _ = tuple.Value.first.Value;  // Compliant, both tuple and tuple.Value.first non-empty
        tuple = null;
        tuple ??= (null, 42);
        _ = tuple.Value.first.Value;  // FN: should report about first instead
        _ = tuple.Value.second.Value; // Compliant, both tuple and tuple.Value.second non-empty
        tuple = null;
        tuple ??= (null, 42);
        _ = tuple.Value.second.Value; // Compliant
        _ = tuple.Value.first.Value;  // FN: tuple non-empty but tuple.Value.first empty
    }

    public void NullCoalescenceResult_NotNull()
    {
        int? i = null;
        i = i ?? 1;         // This uses IsNullOperation
        var r1 = (int)i;    // Compliant
    }

    public void NullCoalescenceAssignmentResult_NotNull()
    {
        int? i = null;
        i ??= 1;            // This uses int?.HasValue.get() invocation
        var r1 = (int)i;    // Compliant, this doesn't call property int?.HasValue, but method int?.HasValue.get
    }

    public void Learn_NotNull(int? arg)
    {
        arg ??= 0;
        if (arg.HasValue)
        {
            _ = arg.Value;
        }
        else
        {
            _ = arg.Value; // Compliant, this path is unreachable
        }
    }
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

class MemberAccessSequence
{
    void Basics(DateTime? dt)
    {
        _ = dt.Value.ToString(); // Compliant, unknown
        dt = null;
        _ = dt.Value.ToString(); // Noncompliant, empty
        _ = dt.Value.ToString(); // Compliant, when reached previous dt.Value implies dt is empty
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
                // HasValue and NoValue constraints are set here
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
        if (foo == value)   // Relationship should be created here
        {
            if (foo.HasValue)
            {
                Console.WriteLine(foo.Value.ToString());
            }
            if (!foo.HasValue)
            {
                Console.WriteLine(foo.Value.ToString());    // Noncompliant, foo is empty
            }
            if (foo == null)
            {
                Console.WriteLine(foo.Value.ToString());    // Compliant, logically unreachable
            }
            if (foo != null)
            {
                Console.WriteLine(foo.Value.ToString());
            }
        }
    }

    public void NestedIsolated1(DateTime? value)
    {
        if (foo == value)   // Relationship should be created here
        {
            if (value.HasValue)
            {
                if (!foo.HasValue)
                {
                    Console.WriteLine(foo.Value.ToString());    // Noncompliant, FP: logically unreachable
                }
            }
        }
    }

    public void NestedIsolated2(DateTime? value)
    {
        if (foo == value)   // Relationship should be created here
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

    public void NestedConditionsWithHasValue(DateTime? value)
    {
        if (foo == value)   // Relationship should be created here
        {
            if (value.HasValue)
            {
                if (foo.HasValue)
                {
                    Console.WriteLine(foo.Value.ToString());    // Compliant, non-empty
                }
                if (!foo.HasValue)
                {
                    Console.WriteLine(foo.Value.ToString());    // Noncompliant, FP: logically unreachable
                }
            }
            else
            {
                if (foo.HasValue)
                {
                    Console.WriteLine(foo.Value.ToString());    // Compliant, logically unreachable
                }
                if (!foo.HasValue)
                {
                    Console.WriteLine(foo.Value.ToString());    // Noncompliant, foo is empty
                }
            }
        }
    }

    public void NestedConditionsUsingNullComparisons(DateTime? value)
    {
        if (foo == value)   // Relationship should be created here
        {
            if (value.HasValue)
            {
                if (foo == null)
                {
                    Console.WriteLine(foo.Value.ToString());    // Noncompliant, FP: logically unreachable
                }
                if (foo != null)
                {
                    Console.WriteLine(foo.Value.ToString());    // Compliant, non-empty
                }
            }
            else
            {
                if (foo != null)
                {
                    Console.WriteLine(foo.Value.ToString());    // Compliant, logically unreachable
                }
                if (foo == null)
                {
                    Console.WriteLine(foo.Value.ToString());    // Noncompliant, empty
                }
            }
        }
    }
}

class Casts
{
    void DowncastWithReassignment(int? i)
    {
        _ = (int)i;                 // Compliant, same as i.Value with i unknown
        i = null;
        _ = (int)i;                 // Noncompliant, empty
        i = null;
        _ = (double)i;              // Noncompliant, empty
        i = null;
        _ = (double?)i;             // Compliant, target type allows null
        i = 42;
        _ = (int)i;                 // Compliant
    }

    void DowncastAfterLearnedNotNullViaLiteral(int? i)
    {
        i = 42;
        _ = (int)i;                 // Compliant
    }

    void DowncastAfterLearnedNotNullViaValue()
    {
        int? i = null;
        _ = i.Value;                // Noncompliant, empty
        _ = (int)i;                 // Compliant, when reached i.Value above implies i is not null
    }

    void UpcastWithNull()
    {
        _ = ((int?)null).Value;     // Noncompliant
    }

    void UpcastWithReassignment(int? i)
    {
        _ = ((int?)null).Value;     // Noncompliant
        i = null;
        _ = ((int?)i).Value;        // Noncompliant, when reached i.Value above implies i is not null
    }

    void UpcastWithNonNullLiteral()
    {
        _ = ((int?)42).Value;       // Compliant
    }

    void AsOperatorAndUnreachability(int? i)
    {
        _ = (null as int?).Value;   // Noncompliant
        i = null;
        _ = (i as int?).Value;      // Noncompliant, when reached assignment above implies i is null
    }

    void AsOperatorWithUnknownAndReassignment(int? i)
    {
        _ = (i as int?).Value;      // Compliant
        i = null;
        _ = (i as int?).Value;      // Noncompliant, empty
    }

    void AsOperatorWithNonNullLiteral(int? i)
    {
        _ = (42 as int?).Value;     // Compliant
    }

    public void ToObject()
    {
        int? value = null;
        object o = value;
        object[] arr = { value };
    }

    public void ToDynamic()
    {
        int? value = null;
        var d = (dynamic)value;
    }

    public void WithCustomOperator()
    {
        int? i = null;
        var n = (Casts)i;           // Compliant, custom cast
    }

    public static explicit operator Casts(int? i) => null;
}

class OutAndRefParams
{
    int? nullableField;
    readonly int? nullableReadonlyField = null;

    OutAndRefParams()
    {
        nullableReadonlyField = null;
        ModifyOutParamInCtor(out nullableReadonlyField);
        _ = nullableReadonlyField.Value;  // Compliant

        static void ModifyOutParamInCtor(out int? i) => i = null;
    }

    void OutParams(int? iParam)
    {
        iParam = null;
        ModifyOutParam(out iParam);
        _ = iParam.Value;                                // Compliant, unknown after method call

        int? iLocal;
        ModifyOutParam(out iLocal);
        _ = iLocal.Value;                                // Compliant

        iLocal = null;
        ModifyOutParam(out iLocal);
        _ = iLocal.Value;                                // Compliant

        iLocal = null;
        ModifyOutParamAndRead(out iLocal, iLocal.Value); // FN

        iLocal = null;
        ReadAndModifyOutParam(iLocal.Value, out iLocal); // Noncompliant

        nullableField = null;
        ModifyOutParam(out nullableField);
        _ = nullableField.Value;                         // Compliant

        ModifyOutParam(out var newLocal);
        _ = newLocal.Value;                             // Unknown
        newLocal = null;
        _ = newLocal.Value;                             // Noncompliant

        static void ModifyOutParam(out int? i) => i = null;
        static void ModifyOutParamAndRead(out int? i1, int? i2) => i1 = i2;
        static void ReadAndModifyOutParam(int? i1, out int? i2) => i2 = i1;
    }

    void RefParams(int? iParam)
    {
        iParam = null;
        ModifyRefParam(ref iParam);
        _ = iParam.Value;                                // Compliant, unknown after method call

        int? iLocal = null;
        ModifyRefParam(ref iLocal);
        _ = iLocal.Value;                                // Compliant

        iLocal = null;
        ModifyRefParamAndRead(ref iLocal, iLocal.Value); // FN

        iLocal = null;
        ReadAndModifyRefParam(iLocal.Value, ref iLocal); // Noncompliant

        nullableField = null;
        ModifyRefParam(ref nullableField);
        _ = nullableField.Value;                         // Compliant

        static void ModifyRefParam(ref int? i) => i = null;
        static void ModifyRefParamAndRead(ref int? i1, int? i2) => i1 = i2;
        static void ReadAndModifyRefParam(int? i1, ref int? i2) => i2 = i1;
    }
}

class InParams
{
    void InParamOfTheMethodItself(in int? iParam)
    {
        _ = iParam.Value;       // Compliant, unknown
        iParam = null;          // Error [CS8331]
        _ = iParam.Value;       // Compliant, iParam is "in" and its value can't be modified
    }

    void InParamOfAnotherMethod(in int? iParam)
    {
        _ = iParam.Value;       // Compliant, unknown
        InParam(in iParam);
        _ = iParam.Value;       // Compliant, iParam is "in" and its value can't be modified
    }

    void InParamToAnotherMethod(int? iParam)
    {
        int? iLocal = null;
        InParam(in iLocal);
        _ = iLocal.Value;       // Noncompliant, arg is "in" and its value must still be null here

        iParam = null;
        InParam(in iParam);
        _ = iParam.Value;       // Noncompliant, empty

        iParam = 42;
        InParam(in iParam);
        _ = iParam.Value;       // Compliant, non-empty
    }

    static void InParam(in int? i) { }
}

class MutableField
{
    int? theField;

    void Basics()
    {
        _ = theField.Value;          // Compliant, unknown

        theField = null;
        _ = theField.Value;          // Noncompliant, empty
        _ = theField.Value;          // Compliant, when reached the ".Value" above implies theField is not null
    }

    void LocalFunctions()
    {
        theField = 42;
        LocalFunctionChangingTheField();
        _ = theField.Value;          // Compliant, unknown

        theField = null;
        LocalFunctionChangingTheField();
        _ = theField.Value;          // Noncompliant, FP, local functions don't reset the state

        theField = 42;
        LocalFunctionNotChangingTheField();
        _ = theField.Value;          // Compliant, unknown

        void LocalFunctionChangingTheField() => theField = null;
        void LocalFunctionNotChangingTheField() { }
    }

    void Methods()
    {
        theField = 42;
        MethodChangingTheField();
        _ = theField.Value;          // Compliant, unknown

        theField = null;
        MethodChangingTheField();
        _ = theField.Value;          // Compliant, instance methods reset the state

        theField = 42;
        MethodNotChangingTheField();
        _ = theField.Value;          // Compliant, unknown
    }

    void MethodChangingTheField() => theField = null;

    void MethodNotChangingTheField() { }

    IEnumerable<int> WithYield()
    {
        _ = theField.Value;          // Compliant, unknown
        yield return 42;

        theField = null;
        yield return 42;
        _ = theField.Value;          // Noncompliant, FP, should be unknown

        theField = 42;
        yield return 42;
        _ = theField.Value;          // Compliant, unknown

        theField = null;
        yield return theField.Value; // Noncompliant, empty
        _ = theField.Value;          // Compliant, unknown

        theField = null;
        yield break;
        _ = theField.Value;          // Compliant, unreachable
    }

    async Task WithAsyncAwait()
    {
        _ = theField.Value;                          // Compliant, unknown
        await AnAsyncOperation();

        theField = null;
        await AnAsyncOperation();
        _ = theField.Value;                          // Compliant, unknown

        theField = 42;
        await AnAsyncOperation();
        _ = theField.Value;                          // Compliant, unknown

        theField = null;
        await AnotherAsyncOperation(theField.Value); // Noncompliant, empty
        _ = theField.Value;                          // Compliant, unknown

        theField = null;
        await AnAsyncOperation();
        _ = theField.Value;                          // Compliant, unknown

        var task = AnAsyncOperation();
        theField = null;
        await task;
        _ = theField.Value;                          // Compliant, unknown

        theField = await AnAsyncOperation();
        _ = theField.Value;                          // Compliant, unknown

        theField = null;
        theField = await AnotherAsyncOperation(theField.Value); // Noncompliant, empty
        _ = theField.Value;                                     // Compliant, unknown

        async Task<int?> AnAsyncOperation() => await Task.FromResult(42);
        async Task<int?> AnotherAsyncOperation(int i) => await Task.FromResult(42);
    }
}

class Boxing
{
    void EmptyNullable()
    {
        int? nullable = null;
        object implicitBoxed = nullable;
        _ = (int)implicitBoxed;             // Compliant, true null-dereference -> no nullable value access
        var explicitBoxed = (object)nullable;
        _ = (int)explicitBoxed;             // Compliant
        _ = (int)(object)(int?)(null);      // Compliant
        _ = (int)(object)(null as int?);    // Compliant
    }

    void EmptyNullableRoundTrip()
    {
        int? nullable = null;
        object implicitBoxed = nullable;
        int? unboxedNullable = (int?)implicitBoxed;
        _ = unboxedNullable.Value;          // Noncompliant, empty
    }

    void NonEmptyNullableRoundTrip()
    {
        int? nullable = 42;
        object implicitBoxed = nullable;
        int? unboxedNullable = (int?)implicitBoxed;
        _ = unboxedNullable.Value;          // Compliant, non-empty
    }

    void UnknownNullableRoundTrip(int? nullable)
    {
        object implicitBoxed = nullable;
        int? unboxedNullable = (int?)implicitBoxed;
        _ = unboxedNullable.Value;          // Compliant, unknown
    }

    void NonEmptyNullable()
    {
        int? nullable = 42;
        object implicitBoxed = nullable;
        _ = (int)implicitBoxed;             // Compliant
        var explicitBoxed = (object)nullable;
        _ = (int)explicitBoxed;             // Compliant
    }

    void ImplicitAndExplicitConversion()
    {
        int? intNullable = 42;
        object boxedInt = intNullable;
        _ = (short)intNullable;             // Compliant, cast exception from int unboxing -> no nullable value access
    }

    void ForeachExplicitConversion()
    {
        foreach (int i in new int?[] { null, 42 }) { }          // FN
        foreach (int i in new long?[] { null, 42, 42L }) { }    // FN
    }

    void ForeachImplictConversion()
    {
        foreach (object boxed in new int?[] { null, 42 })
        {
            _ = (int)boxed;                 // Compliant
        }
    }

    void CollectionImplicitBoxing()
    {
        foreach (var boxed in new object[] { null as int? })
        {
            _ = (int)boxed;                 // Compliant
        }
        foreach (var boxed in new List<object> { null as int? })
        {
            _ = (int)boxed;                 // Compliant
        }
        foreach (var (boxed1, boxed2) in new Dictionary<object, object> { { 42 as int?, null as int? } })
        {
            _ = (int)boxed1;                // Compliant
            _ = (int)boxed2;                // Compliant
        }
    }
}

namespace WithAliases
{
    using MaybeInt = Nullable<System.Int32>;

    class Test
    {
        void Basics(MaybeInt i)
        {
            _ = i.Value;              // Compliant, value unknown
            i = null;
            _ = (i as int?).Value;    // Noncompliant
        }
    }
}

namespace TypeWithInstancePropertyCalledValue
{
    class Test
    {
        void Basics1()
        {
            ClassWithPropertyCalledValue i = null;
            _ = i.Value;                                                     // Compliant, not a nullable type
        }

        void Basics2()
        {
            ClassWithPropertyCalledValue i = null;
            _ = i.APropertyNotCalledValue;                                   // Compliant, not a nullable type
        }

        void ImplicitCast()
        {
            StructWithPropertyCalledValueAndCastOperators i = null as int?;  // Noncompliant, FP
            _ = i.Value;                                                     // Compliant, not a nullable type
        }

        int ExplicitCast1 => ((StructWithPropertyCalledValueAndCastOperators)(null as long?)).Value;                                    // Noncompliant, FP, just gives 42
        StructWithPropertyCalledValueAndCastOperators ExplicitCast2 => (null as StructWithPropertyCalledValueAndCastOperators?).Value;  // Noncompliant, FP, just gives a struct
        int ExplicitCast3 => (null as StructWithPropertyCalledValueAndCastOperators?).Value.Value;                                      // Noncompliant, FP, just gives 42
    }

    class ClassWithPropertyCalledValue
    {
        public int Value => 42;
        public int APropertyNotCalledValue => 42;
    }

    struct StructWithPropertyCalledValueAndCastOperators
    {
        public int Value => 42;
        public int APropertyNotCalledValue => 42;

        public static implicit operator StructWithPropertyCalledValueAndCastOperators(int? value) => new StructWithPropertyCalledValueAndCastOperators();
        public static explicit operator StructWithPropertyCalledValueAndCastOperators(long? value) => new StructWithPropertyCalledValueAndCastOperators();
    }
}

namespace TypeWithStaticPropertyCalledValue
{
    class Test
    {
        void Basics()
        {
            // Ensures rule doesn't raise NRE on custom property called Value having no instance (static)
            _ = StaticValue.Value;                                // Compliant, not on nullable value type

            // Ensures rule doesn't raise NRE on nested Value property
            _ = StaticValue.Value.Value;                          // Compliant
            _ = StaticValue.Value.Value.InstanceProperty;         // Compliant
            _ = StaticValue.Value.Value.InstanceProperty.Value;   // Compliant
            _ = new InstanceValue().Value;                        // Compliant
        }
    }

    class StaticValue
    {
        public InstanceValue InstanceProperty => null;

        public static InstanceValue Value => null;
    }

    class InstanceValue
    {
        public StaticValue Value => null;
    }
}

namespace TypeWithStaticFieldCalledValue
{
    class Test
    {
        void Basics()
        {
            // Ensures rule doesn't raise NRE on custom field called Value having no instance (static)
            _ = StaticValue.Value;                            // Compliant, not on nullable value type

            // Ensures rule doesn't raise NRE on nested Value field
            _ = StaticValue.Value.Value;                      // Compliant
            _ = StaticValue.Value.Value.InstanceField;        // Compliant
            _ = StaticValue.Value.Value.InstanceField.Value;  // Compliant
            _ = new InstanceValue().Value;                    // Compliant
        }
    }

    class StaticValue
    {
        public InstanceValue InstanceField = null;

        public static InstanceValue Value = null;
    }

    class InstanceValue
    {
        public StaticValue Value = null;
    }
}
