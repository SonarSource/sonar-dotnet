using System.Collections.Generic;
using System.Threading.Tasks;
using System;


// TopLevelStatements:
// This should do the trick: CheckForDeadStores(c, c.SemanticModel.GetDeclaredSymbol(c.Node), firstGlobalStatement)
// but registering for CompilationUnit triggers the analysis twice, causing duplicates.
var x = 100; // FN, we don't register for CompilationUnit yet.
x = 1;       // FN
(x, int y) = ReturnIntTuple(); // FN
string str = "";   // FN
str = """Test2"""; // FN
Foo(str);

void UnsignedShiftRightAssignment()
{
    int i = 0;
    i >>>= 5; // Noncompliant
}

void RawStringLiterals(string param)
{
    param = """Test""";      // Noncompliant

    string x = ""; // Compliant, ignored value
    x = """Test2""";
    Foo(x);

    string y = """Test1"""; // Noncompliant
    y = """Test2""";
    Foo(y);
}

void MultilineRawStringLiterals(string param)
{
    param = """ 
        This
        is
        multiline
        """; // Noncompliant@-4

    string x = """

        """; // Compliant (empty multi-line)
    x = """
        Something
        """;

    string z = """
        Something
        """; // Noncompliant@-2
    z = """
        
        """;

    string y = """
        This
        is
        multiline
        """; // Noncompliant@-4
    y = """
        This
        is
        also
        multiline
        """;

    Foo(x);
    Foo(y);
    Foo(z);
}

void InterpolatedRawStringLiterals(string param)
{
    string aux = """Test""";
    string auxMultiline = """
        This
        is
        multiline
        """;

    param = $"""{aux}Test""";      // Noncompliant
    param = $"""{auxMultiline}Test""";      // Noncompliant
    param = $"""
        {aux}
        Test
        """;      // Noncompliant@-3
    param = $"""
        {auxMultiline}
        Test
        """;      // Noncompliant@-3

    string empty = "";
    string x = $"""{empty}"""; // Noncompliant  string interpolation values are intentionally not evaluated
    x = $"""{empty}Test""";
    Foo(x);

    string emptyMultiline = """

        """;
    string q = $"""{emptyMultiline}"""; // Noncompliant  string interpolation values are intentionally not evaluated
    q = $"""{emptyMultiline}Test""";
    Foo(q);

    string y = $"""
        Test1{aux}
        """; // Noncompliant@-2
    y = $"""
        Test2{aux}
        """;
    Foo(y);
}

void NewlinesInStringInterpolation(string param)
{
    string aux = "Test";
    param = $"{aux
        .ToUpper()}"; // Noncompliant@-1
    param = $"{aux
        .ToUpper()}";
    Foo(param);

    string empty = "";
    string x = $"{empty +
        empty}"; // Noncompliant@-1 string interpolation values are intentionally not evaluated
    x = "Test";
    Foo(x);
}

void IgnoredValues()
{
    string emptyMultilineRawStringLiteral = $$"""

        """; // Compliant
    emptyMultilineRawStringLiteral = "other";

    Foo(emptyMultilineRawStringLiteral);
}

static (int a, int b) ReturnIntTuple()
{
    return (1, 2);
}

static (int, (int, (int, int))) ReturnNestedTuple()
{
    return (1, (2, (3, 4)));
}

static int ReturnAnInt()
{
    return 10;
}

void DoStuffWithInts()
{
    int x = ReturnAnInt();
    (x, int y) = ReturnIntTuple(); // Noncompliant {{Remove this useless assignment to local variable 'x'.}}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
}

void MultipleAssignmentInSingleDeconstruction()
{
    int x = 1;
    (x, x, (x, x)) = (1, 2, (3, 4));
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^      {{Remove this useless assignment to local variable 'x'.}}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^  @-1 {{Remove this useless assignment to local variable 'x'.}}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^  @-2 {{Remove this useless assignment to local variable 'x'.}}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^  @-3 {{Remove this useless assignment to local variable 'x'.}}
}

void ReAssignmentDeconstruction()
{
    int x = 1;
    (x, x, (x, x)) = (x, x, (x, x)); // Noncompliant [issue1, issue2, issue3, issue4]
}

void ReAssignmentDeconstructionFromMethodCall()
{
    (_, (var _, (int x, var _))) = ReturnNestedTuple();
    (x, _) = ReturnNestedTuple();    // Noncompliant
}

void DoStuffWithIntsAgain()
{
    int? x = null;
    (x, int y) = ReturnIntTuple();   // Noncompliant
}

void TwoAssigments()
{
    int a, b, c = 0;
    (a, (b, c)) = (1, (2, 3));
//  ^^^^^^^^^^^^^^^^^^^^^^^^^  Noncompliant    {{Remove this useless assignment to local variable 'a'.}}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^  Noncompliant@-1 {{Remove this useless assignment to local variable 'b'.}}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^  Noncompliant@-2 {{Remove this useless assignment to local variable 'c'.}}
}

void DoSimplerStuffWithIntsAgain()
{
    int x;
    (x, var y) = (1, 2); // Noncompliant
}

Action<int, int, int> StaticLambda() =>
    static (int a, int _, int _) =>
    {
        a = 200; // FN
        (a, int b) = ReturnIntTuple(); // FN
    };

void ReassignAfter()
{
    int x;
    (x, var y) = (1, 2); // Noncompliant
    x = 2;
    Console.WriteLine(x);
}

void TargetTypedNew()
{
    Decimal d = new(100f);  // Noncompliant
    d = new(2f);            // Noncompliant
}

void NativeInts(nuint param)
{
    param = 1;      // Noncompliant

    nuint zero = 0; // Compliant, ignored value
    zero = 1;
    Foo(zero);

    nint minusOne = -1; // Compliant, ignored value
    minusOne = 1;
    Foo(minusOne);

    nint one = 1;       // Compliant, ignored value
    one = 2;
    Foo(one);

    nint two = 2;       // Noncompliant
    two = 3;
    Foo(two);
}

// https://sonarsource.atlassian.net/browse/NET-2976
void DefaultLiteral()
{
    int i = default; // Compliant
    i = 42;
    Foo(i);

    string s = default; // Compliant
    s = "value";
    Foo(s);

    object o = default; // Compliant
    o = new object();
    Foo(o);

    bool b = default; // Compliant
    b = true;
    Foo(b);

    IntPtr ptr = default; // Compliant
    ptr = new IntPtr(42);
    Foo(ptr);

    UIntPtr uptr = default; // Compliant
    uptr = new UIntPtr(42);
    Foo(uptr);

    Guid guid = default; // Compliant
    guid = Guid.NewGuid();
    Foo(guid);
}

// https://sonarsource.atlassian.net/browse/NET-2976
void TypeSpecificDefaultValues()
{
    IntPtr ptr = IntPtr.Zero; // Compliant
    ptr = new IntPtr(42);
    Foo(ptr);

    UIntPtr uptr = UIntPtr.Zero; // Compliant
    uptr = new UIntPtr(42);
    Foo(uptr);

    Guid id = Guid.Empty; // Compliant
    id = Guid.NewGuid();
    Foo(id);
}

void PatternMatch(object param)
{
    object a = param;
    if (a is not null)
    {
        a = null; // Compliant
        Foo(a);
    }

    int i = 100;
    if (i is not > 50 and < 200)
    {
        i = 2;
        Foo(i);
    }
}

void PatternMatchFalseNegative(int a, int b)
{
    if (b is not 5)
    {
        a = 1;  // Noncompliant
    }
    else if (b is 5)
    {
        a = 2;  // Noncompliant
    }

    var c = 5;
    switch (c)
    {
        case < 5:
            c = 6; // Noncompliant
            break;
        case >= 5:
            c = 7; // Noncompliant
            break;
    }
}

Action<int, int, int> AnotherStaticLambda() =>
    static (int a, int _, int _) =>
{
    a = 100;        // FN, muted
    int b = 100;    // FN, muted
    b = 1;          // FN, muted
};

void Foo(object o) { }

public class C
{

    public static void Log() { }
    unsafe void FunctionPointer()
    {
        delegate*<void> ptr1 = &C.Log;  // Noncompliant
        ptr1 = &C.Log;                  // Noncompliant
    }

    Action<int, int, int> StaticLambda() =>
        static (int a, int _, int _) =>
        {
            a = 100;        // Noncompliant
            int b = 100;    // Noncompliant
            b = 1;          // Noncompliant
        };

}

record R
{
    public R(int x)
    {
        x = 1; // Noncompliant
    }

    int x;
    public int InitProperty
    {
        init
        {
            value = 1;      // Noncompliant
            int a = 100;    // Noncompliant
            a = 2;          // Noncompliant
        }
    }
}

class UnsafeContexts
{
    //https://sonarsource.atlassian.net/browse/NET-404
    IEnumerable<int> IteratorTests(int test)
    {
        unsafe
        {
            ref int x = ref test; 
            x = default;            // Compliant FN
        }
        yield return 1;
        local();
        async void local()
        {
            unsafe
            {
                int* p = null;
                p = (int*)10;   // Noncompliant
            }
            await Task.Yield();
        }
    }
}

static class Extensions
{
    extension(string s)
    {
        void Method()
        {
            var x = "unused";   // Noncompliant
            x = "new value";    // Noncompliant
        }
    }
}

class FieldKeyword
{
    string Property
    {
        get => field;
        set
        {
            field = "unused";       // Compliant: not a local variable
            field = "new value";    // Compliant: not a local variable
        }
    }
}

partial class Partial
{
    partial event EventHandler Event;
    partial Partial();
}

partial class Partial
{
    partial event EventHandler Event
    {
        add
        {

            var x = "unused";   // Noncompliant
            x = "new value";    // Noncompliant
        }
        remove
        {
            var x = "unused";   // Noncompliant
            x = "new value";    // Noncompliant
        }
    }
    partial Partial()
    {
        var x = "unused";       // Noncompliant
        x = "new value";        // Noncompliant
    }
}

class Sample
{
    string Property { get; set; }

    void Method(Sample s)
    {
        s?.Property = "unused";     // Compliant: not a local variable
        s?.Property = "new value";  // Compliant: not a local variable
    }
}

class CustomCompoundAssignment
{
    public int Value;

    public void operator +=(int x)
    {
        Value += x;
    }

    void Method(CustomCompoundAssignment x)
    {
        x += 1;     // Noncompliant
        x = null;   // Noncompliant
    }
}

namespace CSharp15
{
    public class Cat { }
    public class Dog { }
    public union Pet(Cat, Dog);

    public class Probes
    {
        void UnionConversion()
        {
            Pet pet = new Cat();    // Noncompliant
            pet = new Dog();
            Use(pet);
        }

        void UnionConversionRead()
        {
            var cat = new Cat();
            Pet pet = cat;
            Use(pet);
        }

        void UnionPattern(Cat cat)
        {
            Pet pet = cat;
            if (pet is Cat matched)
            {
                matched = new Cat();    // Noncompliant
            }
            var name = pet switch
            {
                Cat => "cat",
                Dog => "dog",
            };
            Use(name);
        }

        int LabeledBreakOutOfSwitch(int[] values, int mode)
        {
            var result = 0;
            selector: switch (mode)
            {
                case 1:
                    for (var i = 0; i < values.Length; i++)
                    {
                        if (values[i] == 0)
                        {
                            result = 42;
                            break selector;
                        }
                    }
                    result = 7;
                    break;
            }
            return result;
        }

        void ContinueOuterFromForeach(int[][] grid, List<int> items)
        {
            outer: for (var i = 0; i < grid.Length; i++)
            {
                var flag = 0;
                foreach (var item in items)
                {
                    if (item < 0)
                    {
                        flag = 42;      // FN NET-4914
                        continue outer;
                    }
                }
                Use(flag);
            }
        }

        void ContinueOuterFromForeachLive(int[][] grid, List<int> items)
        {
            var last = 0;
            outer: for (var i = 0; i < grid.Length; i++)
            {
                Use(last);
                foreach (var item in items)
                {
                    if (item < 0)
                    {
                        last = item;
                        continue outer;
                    }
                }
                last = 0;
            }
        }

        void BreakOuterFromForeach(int[][] grid, List<int> items)
        {
            var flag = 0;
            outer: for (var i = 0; i < grid.Length; i++)
            {
                foreach (var item in items)
                {
                    if (item < 0)
                    {
                        flag = 42;      // FN NET-4914
                        break outer;
                    }
                }
                Use(flag);
            }
            flag = 7;
            Use(flag);
        }

        void GotoOutOfForeach(int[][] grid, List<int> items)
        {
            for (var i = 0; i < grid.Length; i++)
            {
                var flag = 0;
                foreach (var item in items)
                {
                    if (item < 0)
                    {
                        flag = 42;      // FN NET-4914
                        goto next;
                    }
                }
                Use(flag);
            next:
                ;
            }
        }

        void GotoOutOfFor(int[][] grid, int[] items)
        {
            for (var i = 0; i < grid.Length; i++)
            {
                var flag = 0;
                for (var j = 0; j < items.Length; j++)
                {
                    if (items[j] < 0)
                    {
                        flag = 42;      // Noncompliant
                        goto next;
                    }
                }
                Use(flag);
            next:
                ;
            }
        }

        void CollectionExpressionArguments(int[] values)
        {
            var capacity = ReturnAnInt();                   // Noncompliant
            capacity = 42;
            List<int> list = [with(capacity), .. values];
            Use(list);

            var comparer = StringComparer.Ordinal;          // Noncompliant
            comparer = StringComparer.OrdinalIgnoreCase;
            HashSet<string> set = [with(comparer: comparer), "a"];
            Use(set);
        }

        int LabeledBreakCompliant(int[][] grid)
        {
            var result = 0;
            outer: for (var i = 0; i < grid.Length; i++)
            {
                for (var j = 0; j < grid[i].Length; j++)
                {
                    if (grid[i][j] == 0)
                    {
                        result = 42;
                        break outer;
                    }
                }
                result = 7;
            }
            return result;
        }

        int LabeledBreakNonCompliant(int[][] grid)
        {
            var result = 0;
            outer: for (var i = 0; i < grid.Length; i++)
            {
                for (var j = 0; j < grid[i].Length; j++)
                {
                    if (grid[i][j] == 0)
                    {
                        result = 42;    // Noncompliant
                        break outer;
                    }
                }
                Use(result);
            }
            result = 7;
            return result;
        }

        int LabeledContinueCompliant(int[][] grid)
        {
            var result = 0;
            outer: for (var i = 0; i < grid.Length; i++)
            {
                Use(result);
                for (var j = 0; j < grid[i].Length; j++)
                {
                    if (grid[i][j] == 0)
                    {
                        result = 42;
                        continue outer;
                    }
                }
                result = 7;
            }
            return result;
        }

        void LabeledContinueNoncompliant(int[][] grid)
        {
            outer: foreach (var row in grid)
            {
                var flag = 0;
                for (var i = 0; i < row.Length; i++)
                {
                    if (row[i] < 0)
                    {
                        flag = 42;      // Noncompliant
                        continue outer;
                    }
                }
                Use(flag);
            }
        }

        void ExtensionIndexer(Box box)
        {
            var key = "first";      // Noncompliant
            key = "second";
            box[key] = 1;
            Use(box[key]);
        }

        static int ReturnAnInt() => 42;
        static void Use(object o) { }
    }

    public class Box
    {
        public int Value;
    }

    public static class IndexerExtensions
    {
        extension(Box box)
        {
            public int this[string key]
            {
                get => box.Value;
                set => box.Value = value;
            }
        }
    }
}
