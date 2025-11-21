using System;
using System.Runtime.CompilerServices;
class Person
{
    const int forCoverage = 0;
    static int minAge = 0;
    static int Bar { get; set; } = 1;
    static event EventHandler Quix = (a, b) => { };

    static int maxAge = 42; // Noncompliant {{Remove the static member initializer, a static constructor or module initializer sets an initial value for the member.}}
    static string Name = "X"; // Noncompliant
    static string Job = "1"; // Noncompliant
    static bool Nice = true; // Noncompliant
    static int Foo { get; set; } = 1; // Noncompliant
    static event EventHandler Taz = (a, b) => { }; // Noncompliant

    public Person() { } // for coverage

    [ModuleInitializer]
    internal static void Initialize()
    {
        maxAge = 42;
    }

    [Obsolete]
    [ModuleInitializer]
    internal static void SecondInitialize()
    {
        Name = "Foo";
        Job = "2";
        Nice = false;
        Foo = 1;
        Taz = (a, b) => { };
    }

    [ModuleInitializer]
    internal static void ThirdInitialize()
    {
        Job = "3";
    }

    [Obsolete]
    internal static void Foo1()
    {
        minAge = 2;
    }

    [Obsolete("argument", false)]
    internal static void Foo2()
    {
        minAge = 2;
    }

    internal static void Foo3()
    {
        minAge = 3;
        Bar = 2;
        Quix = (a, b) => { };
    }
}

// we do not support records for instance members, see discussion in https://github.com/SonarSource/sonar-dotnet/pull/4756
record InstanceRecord
{
    static int foo = 1; // FN
    int bar = 42; // FN
    int fred { get; init; } = 42; // FN
    int quix = 9; // ok, not initialized in all constructors
    const int barney = 1;

    [ModuleInitializer]
    internal static void Initialize()
    {
        foo = 1;
    }

    public InstanceRecord()
    {
        bar = 30;
        quix = 10;
        fred = 10;
    }

    public InstanceRecord(int x)
    {
        bar = x;
        fred = 10;
    }

    public InstanceRecord(int x, int y) : this(x) { }
}

public record RecordWithParam(string bar)
{
    public string foo = "foo";
}

public record RecordWithParamAndConstructor(string bar)
{
    public string foo = "foo";
    public int quix = 1; // FN
    public RecordWithParamAndConstructor(string x, int y) : this(x)
    {
        quix = 2;
    }
}

public record StaticRecord
{
    public static int FOO = 1; // FN
    public static string BAR = "BAR";
    static StaticRecord()
    {
        FOO = 2;
    }
}

record EmptyRecordForCoverage { }

struct Struct1
{
    static int b = 1; // Noncompliant
    static int b2 = 2;

    [ModuleInitializer]
    internal static void Initialize() => b = 42;
}

interface SomeInterface
{
    static int Field = 1; // Noncompliant
    static int Field2 = 2;
    static event EventHandler Taz = (a, b) => { }; // Noncompliant

    [ModuleInitializer]
    internal static void Initialize()
    {
        Field = 2;
        Taz = (a, b) => { };
    }
}

// we do not support records for instance members, see discussion in https://github.com/SonarSource/sonar-dotnet/pull/4756
record struct InstanceStruct
{
    static int foo = 1; // FN
    int bar = 42; // FN
    int fred { get; init; } = 42; // FN
    int quix = 9; // ok, not initialized in all constructors
    const int barney = 1;

    [ModuleInitializer]
    internal static void Initialize()
    {
        foo = 1;
    }

    public InstanceStruct()
    {
        bar = 30;
        quix = 10;
        fred = 10;
    }

    public InstanceStruct(int x)
    {
        bar = x;
        fred = 10;
    }

    public InstanceStruct(int x, int y) : this(x) { }
}

public record struct RecordStructWithParam(string bar)
{
    public string foo = "foo";
}

public record struct RecordStructWithParamAndConstructor(string bar)
{
    public string foo = "foo";
    public int quix = 1; // FN
    public RecordStructWithParamAndConstructor(string x, int y) : this(x)
    {
        quix = 2;
    }
}

public record struct RecordStruct
{
    public static int FOO = 1; // FN
    public static string BAR = "BAR";
    static RecordStruct()
    {
        FOO = 2;
    }
}

record struct EmptyRecordStructForCoverage { }

struct StaticStruct
{
    static int b = 1; // Noncompliant
    static int b2 = 2;

    [ModuleInitializer]
    internal static void Initialize() => b = 42;
}

/// Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/7624
class WithPrimaryConstructor(object options)
{
    private readonly object _options = options;      // Compliant
    public object Options { get; } = options;        // Compliant
    public object[] AllOptions { get; } = [options]; // Compliant
}

class WithReferencedPrimaryConstructor(object options)
{
    public WithReferencedPrimaryConstructor() : this(null)
    {

    }
    private readonly object _options = options; // Compliant
}

class WithPrimaryConstructorAndAssignment(object options)
{
    public WithPrimaryConstructorAndAssignment() : this(null)
    {
        _options = null;
    }
    private readonly object _options = options; // Compliant
}

class CollectionExpression1
{
    private readonly int[] numbers = [1, 2, 3]; // Noncompliant
    //                             ^^^^^^^^^^^

    CollectionExpression1()
    {
        numbers = [4, 5, 6];
    }
}

class FieldKeyword
{
    public int Count { get => field; set => field = value; } = 10; // FN https://sonarsource.atlassian.net/browse/NET-2685

    public FieldKeyword()
    {
        Count = 20;
    }
}

class NullConditionalAssignmentTests
{
    int Count = 10;          // FN https://sonarsource.atlassian.net/browse/NET-2685
    int CompliantCount = 10; // FN unfixable without SE, unsupported
    public NullConditionalAssignmentTests()
    {
        this?.Count = 20;
        if (this is null)
        {
            CompliantCount = 20;
        }
    }
}

public partial class PartialConstructor
{
    int id = 100;   // FN https://sonarsource.atlassian.net/browse/NET-2685
    public partial PartialConstructor();
}

public partial class PartialConstructor
{
    public partial PartialConstructor()
    {
        id = 1;
    }
}
