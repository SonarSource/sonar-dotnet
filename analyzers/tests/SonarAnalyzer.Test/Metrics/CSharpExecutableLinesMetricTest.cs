/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.CSharp.Metrics;

namespace SonarAnalyzer.Test.Common;

[TestClass]
public class CSharpExecutableLinesMetricTest
{
    private const string RazorFile = "Test.razor";

    [TestMethod]
    public void GetLineNumbers_NoExecutableLines() =>
        AssertLineNumbersOfExecutableLines(
@"using System;
using System.Linq;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}");

    [TestMethod]
    public void GetLineNumbers_Class() =>
        AssertLineNumbersOfExecutableLines(
@"class Program
{
}");

    [TestMethod]
    public void GetLineNumbers_CheckedUnchecked() =>
        AssertLineNumbersOfExecutableLines(
@"class Program
{
    static void Main(string[] args)
    {
        checked // +1
        {
            unchecked // +1
            {
            }
        }
    }
}", 5, 7);

    [TestMethod]
    public void GetLineNumbers_Blocks() =>
        AssertLineNumbersOfExecutableLines(
@"class Program
{
    unsafe static void Main(int[] arr, object obj)
    {
        lock (obj) { } // +1
        fixed (int* p = arr) { } // +1
        unsafe { } // +1
        using ((System.IDisposable)obj) { } // +1
    }
}", 5, 6, 7, 8);

    [TestMethod]
    public void GetLineNumbers_Statements() =>
        AssertLineNumbersOfExecutableLines(
@"class Program
{
    void Foo(int i)
    {
        ; // +1
        i++; // +1
    }
}", 5, 6);

    [TestMethod]
    public void GetLineNumbers_Loops() =>
        AssertLineNumbersOfExecutableLines(
@"class Program
{
    void Foo(int[] arr)
    {
        do {} // +1
            while (true);
        foreach (var a in arr) { }// +1
        for (;;) { } // +1
        while // +1
            (true) { }
    }
}", 5, 7, 8, 9);

    [TestMethod]
    public void GetLineNumbers_Conditionals() =>
        AssertLineNumbersOfExecutableLines(
@"class Program
{
    void Foo(int? i, string s)
    {
        if (true) { } // +1
        label: // +1
        switch (i) // +1
        {
            case 1:
            case 2:
            default:
                break; // +1
        }
        var x = s?.Length; // +1
        var xx = i == 1 ? 1 : 1; // +1
    }
}", 5, 6, 7, 12, 14, 15);

    [TestMethod]
    public void GetLineNumbers_Conditionals2() =>
        AssertLineNumbersOfExecutableLines(
@"class Program
{
    void Foo(System.Exception ex)
    {
        goto home;  // +1
        throw ex;   // +1
        home:       // +1

        while (true)    // +1
        {
            continue;   // +1
            break;      // +1
        }
        return;     // +1
    }
}", 5, 6, 7, 9, 11, 12, 14);

    [TestMethod]
    public void GetLineNumbers_Yields() =>
        AssertLineNumbersOfExecutableLines(
@"using System.Collections.Generic;

namespace Test
{
    class Program
    {
        IEnumerable<string> Foo()
        {
            yield return """";  // +1
            yield break;        // +1
        }
    }
}", 9, 10);

    [TestMethod]
    public void GetLineNumbers_AccessAndInvocation() =>
        AssertLineNumbersOfExecutableLines(
@"class Program
{
    static void Main(string[] args)
    {
        var x = args.Length; // +1
        args.ToString(); // +1
    }
}", 5, 6);

    [TestMethod]
    public void GetLineNumbers_Initialization() =>
        AssertLineNumbersOfExecutableLines(
@"class Program
{
    static string GetString() => """";

    static void Main()
    {
        var arr = new object();
        var arr2 = new int[] { 1 }; // +1

        var ex = new System.Exception()
        {
            Source = GetString(), // +1
            HelpLink = """"
        };
    }
}", 8, 12);

    [TestMethod]
    public void GetLineNumbers_PropertySet() =>
        AssertLineNumbersOfExecutableLines(
@"class Program
{
    int Prop { get; set; }

    void Foo()
    {
        Prop = 1; // + 1
    }
}", 7);

    [TestMethod]
    public void GetLineNumbers_PropertyGet() =>
        AssertLineNumbersOfExecutableLines(
@"class Program
{
    int Prop { get; set; }

    void Foo()
    {
        var x = Prop;
    }
}");

    [TestMethod]
    public void GetLineNumbers_Lambdas() =>
        AssertLineNumbersOfExecutableLines(
@"using System;
using System.Linq;
using System.Collections.Generic;
class Program
{
    static void Main(string[] args)
    {
        var x = args.Where(s => s != null) // +1
            .Select(s => s.ToUpper()) // +1
            .OrderBy(s => s) // +1
            .ToList();
    }
}", 8, 9, 10);

    [TestMethod]
    public void GetLineNumbers_TryCatch() =>
        AssertLineNumbersOfExecutableLines(
@"using System;
class Program
{
    static void Main(string[] args)
    {
        try
        {
            Main(null);  // +1
        }
        catch (InvalidOperationException)
        {
        }
        catch (ArgumentNullException ane) when (ane.ToString() != null) // +1
        {
        }
        finally
        {
        }
    }
}", 8, 13);

    [TestMethod]
    public void GetLineNumbers_MultipleStatementsSameLine() =>
        AssertLineNumbersOfExecutableLines(
@"using System;
class Program
{
    public void Foo(int x) {
        int i = 0; if (i == 0) {i++;i--;} else // +1
        { while(true){i--;} } // +1
    }
}", 5, 6);

    [TestMethod]
    public void GetLineNumbers_DoWhile() =>
        AssertLineNumbersOfExecutableLines(
@"class Program
{
    static void Main(string[] args)
    {
        do // +1
        {

        }

        while
        (
        true
        )
        ;
    }
}", 5);

    [TestMethod]
    public void GetLineNumbers_ClassExcluded() =>
        AssertLineNumbersOfExecutableLines(
@"using System.Diagnostics.CodeAnalysis;
public class ComplicatedCode
{
    [ExcludeFromCodeCoverage]
    public string ComplexFoo()
    {
        string text = null;
        return text.ToLower();
        string Method(string s)
        {
            return s.ToLower();
        }
    }

    [SomeAttribute]
    public string ComplexFoo2()
    {
        string text = null;
        return text.ToLower(); // +1
        string Method(string s)
        {
            return s.ToLower(); // +1
        }
    }
}

public class SomeAttribute : System.Attribute { }", 19, 22);

    [TestMethod]
    public void GetLineNumbers_AttributeOnLocalFunctionExcluded() =>
        AssertLineNumbersOfExecutableLines(
@"using System.Diagnostics.CodeAnalysis;
public class ComplicatedCode
{
    [SomeAttribute]
    public string ComplexFoo2()
    {
        string text = null;
        return text.ToLower(); // +1

        [ExcludeFromCodeCoverage]
        string ComplexFoo()
        {
            string text = null;
            return text.ToLower(); // +1 , FP
        }
    }
}

public class SomeAttribute : System.Attribute { }", 8, 14);

    [DataTestMethod]
    [DataRow("ExcludeFromCodeCoverage")]
    [DataRow("ExcludeFromCodeCoverage()")]
    [DataRow("ExcludeFromCodeCoverageAttribute")]
    [DataRow("ExcludeFromCodeCoverageAttribute()")]
    [DataRow("System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute()")]
    public void GetLineNumbers_ExcludeFromCodeCoverage_AttributeVariants(string attribute) =>
        AssertLineNumbersOfExecutableLines(
@$"using System.Diagnostics.CodeAnalysis;
public class ComplicatedCode
{{
    [{attribute}]
    public string ComplexFoo()
    {{
        string text = null;
        return text.ToLower();
    }}
}}");

    [TestMethod]
    public void GetLineNumbers_RecordExcluded() =>
        AssertLineNumbersOfExecutableLines(
@"using System.Diagnostics.CodeAnalysis;
[ExcludeFromCodeCoverage]
record Program
{
    static void Main(string[] args)
    {
        Main(null);
    }
}");

    [TestMethod]
    public void GetLineNumbers_RecordStructExcluded() =>
        AssertLineNumbersOfExecutableLines(
@"using System.Diagnostics.CodeAnalysis;
[ExcludeFromCodeCoverage]
record struct Program
{
    static void Main(string[] args)
    {
        Main(null);
    }
}");

    [TestMethod]
    public void GetLineNumbers_StructExcluded() =>
        AssertLineNumbersOfExecutableLines(
@"using System.Diagnostics.CodeAnalysis;
namespace project_1
{
    [ExcludeFromCodeCoverage]
    struct Program
    {
        static void Foo()
        {
            Foo();
        }
    }
}");

    [TestMethod]
    public void GetLineNumbers_ConstructorExcluded() =>
        AssertLineNumbersOfExecutableLines(
@"using System.Diagnostics.CodeAnalysis;
class Program
{
    int count;

    [ExcludeFromCodeCoverage]
    public Program() : this(1)
    {
        count = 123;            // excluded
    }

    public Program(int initialCount)
    {
        count = initialCount;   // +1
    }
}", 14);

#if NET

    [TestMethod]
    public void GetLineNumbers_PropertyExcluded() =>
        AssertLineNumbersOfExecutableLines(
@"using System.Diagnostics.CodeAnalysis;
class EventClass
{
private int _value;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public int Value1
{
    get { return _value; }      // Excluded
    set { _value = value; }     // Excluded
}

public int Value2
{
    get { return _value; }      // +1
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    set { _value = value; }     // Excluded
}

public int Value3
{
    get { return _value; }      // +1
    set { _value = value; }     // +1
}

public int Value4
{
    get { return _value; }      // +1
    init { _value = value; }     // +1
}

public int Value5
{
    get { return _value; }      // +1
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    init { _value = value; }     // Excluded
}
}", 15, 22, 23, 28, 29, 34);

#endif

    [TestMethod]
    public void GetLineNumbers_EventExcluded() =>
        AssertLineNumbersOfExecutableLines(
@"using System.Diagnostics.CodeAnalysis;
class EventClass
{
    private System.EventHandler _explicitEvent;

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public event System.EventHandler ExplicitEvent1
    {
        add { _explicitEvent += value; }        // Excluded
        remove { _explicitEvent -= value; }     // Excluded
    }

    public event System.EventHandler ExplicitEvent2
    {
        add { _explicitEvent += value; }        // +1
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        remove { _explicitEvent -= value; }     // excluded
    }

    public event System.EventHandler ExplicitEvent3
    {
        add { _explicitEvent += value; }        // +1
        remove { _explicitEvent -= value; }     // +1
    }
}", 15, 22, 23);

    [TestMethod]
    public void GetLineNumbers_PartialClassesExcluded() =>
        AssertLineNumbersOfExecutableLines(
@"using System.Diagnostics.CodeAnalysis;
[ExcludeFromCodeCoverage]
partial class Program
{
    int FooProperty { get { return 1; } }   // excluded
}

partial class Program
{
    void Method1()
    {
        System.Console.WriteLine();         // other class partial is excluded -> excluded
    }
}

partial class AnotherClass
{
    void Method2()
    {
        System.Console.WriteLine();         // +1
    }
} ", 20);

    [TestMethod]
    public void GetLineNumbers_PartialMethodsExcluded() =>
        AssertLineNumbersOfExecutableLines(
@"using System.Diagnostics.CodeAnalysis;
partial class Program
{
    [ExcludeFromCodeCoverage]
    partial void Method1();
}

partial class Program
{
    partial void Method1()
    {
        System.Console.WriteLine();         // other partial part of method is excluded -> excluded
    }
}

partial class AnotherClass
{
    partial void Method2();
}

partial class AnotherClass
{
    [ExcludeFromCodeCoverage]
    partial void Method2()
    {
        System.Console.WriteLine();         // excluded
    }
}");

    [TestMethod]
    public void GetLineNumbers_AttributeAreIgnored() =>
        AssertLineNumbersOfExecutableLines(
@"using System.Reflection;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo(""FOO"")]");

    [TestMethod]
    public void GetLineNumbers_OnlyAttributeAreIgnored() =>
        AssertLineNumbersOfExecutableLines(
@"[AnAttribute]
public class Foo
{
    [AnAttribute]
    void MyCode([AnAttribute] string foo)
    {
        System.Console.WriteLine(); // +1
    }
}

public class AnAttribute : System.Attribute { }", 7);

    [TestMethod]
    public void GetLineNumbers_AttributeOnLambdaIsIgnored() =>
        AssertLineNumbersOfExecutableLines(
@"using System;
using System.Linq;
using System.Collections.Generic;
class Test
{
    static void Main(string[] args)
    {
        [AnAttribute]
        List<string> LambdaWithAttribute(string[] args) =>
            args.Where(s => s != null).ToList(); // +1
    }
}

public class AnAttribute : System.Attribute { }", 10);

    [TestMethod]
    public void GetLineNumbers_ExpressionsAreCounted() =>
        AssertLineNumbersOfExecutableLines(
@"class Program
{
    public void Foo(bool flag)
    {
        if (flag) // +1
        {
            flag = true; flag = false; flag = true; // +1
        }
    }
}", 5, 7);

    [TestMethod]
    public void GetLineNumbers_MultiLineLoop() =>
        AssertLineNumbersOfExecutableLines(
@"class Program
{
    void Foo(int[] arr)
    {
        for         // +1
            (         // +0
            int i=0; // +0
            i < 10;  // +0
            i++      // +0
            )         // +0
        {           // +0
        }
    }
}", 5);

    [TestMethod]
    public void GetLineNumbers_SwitchStatementWithMultipleCases() =>
        AssertLineNumbersOfExecutableLines(
@"class Program
{
    void Foo(int? i)
    {
        switch (i) // +1
        {
            case 1:
                System.Console.WriteLine(4); // +1
                break; // +1
            case 2:
                System.Console.WriteLine(4); // +1
                break; // +1
            default:
                break; // +1
        }
    }
}", 5, 8, 9, 11, 12, 14);

    [TestMethod]
    public void GetLineNumbers_SwitchExpressionWithMultipleCases() =>
        AssertLineNumbersOfExecutableLines(
@"class Program
{
    void Foo(int? i, string s)
    {
        var x = s switch
        {
                ""a"" => true,
                ""b"" => false,
                _ => true
        };
        var y = s switch
        {
            ""a"" => Foo(""b""), // +1
            ""b"" => Foo(""a""), // +1
            _ => false
        };
    }
    bool Foo(string s) => true;
}", 13, 14);

    [TestMethod]
    public void GetLineNumbers_MultiLineInterpolatedString() =>
        AssertLineNumbersOfExecutableLines(
@"class Program
{
    void Foo(int? i, string s)
    {
        string x = ""someString"";
        x += @$""This is a Multi
                                Line
                                interpolated
                                string {i} {s}"";
    }
}", 6);

    [TestMethod]
    public void GetLineNumbers_MultiLineInterpolatedStringWithMultipleLineExpressions() =>
        AssertLineNumbersOfExecutableLines(
@"public class C
{
    public string M(int i)
    {
        return @$""
        {(i == 1 ? Bar(1) : Bar(2))}
        {(i == 2 ? Bar(2) : Bar(3))}
        { Bar(5)}
                    "";
    }

    public string Bar(int i) => ""y"" + i;
}", 5, 6, 7, 8);

    [TestMethod]
    public void GetLineNumbers_UsingDeclaration() =>
        AssertLineNumbersOfExecutableLines(
@"class Program
{
    void Foo(int? i, string s)
    {
        using var file = new System.IO.StreamWriter(""WriteLines2.txt"");
    }
}");

    [TestMethod]
    public void GetLineNumbers_LocalFunctions() =>
        AssertLineNumbersOfExecutableLines(
@"class Program
{
    int M1()
    {
        int y = MyMethod(); // +1
        MyMethod();         // +1
        return 1; // +1
        int MyMethod() => 0; // Not counted
    }

    int M2()
    {
        int y = 5;
        int x = 7;
        return Add(x, y);

        static int Add(int left, int right) => left + right;
    }
}", 5, 6, 7, 15);

#if NET

    [TestMethod]
    public void GetLineNumbers_IndicesAndRanges() =>
        AssertLineNumbersOfExecutableLines(
@"using System;
class Program
{
void M()
{
    string s = null;
    string[] subArray;
    var words = new string[]
        {
            ""The"",
            ""quick"",
            ""brown"",
            ""fox"",
            ""jumped"",
            ""over"",
            ""the"",
            ""lazy"",
            ""dog""
        };
    s = words[^1];
    subArray = words[1..4];
}
}", 9, 20, 21);

#endif

    [TestMethod]
    public void GetLineNumbers_NullCoalescingAssignment() =>
        AssertLineNumbersOfExecutableLines(
@"using System;
using System.Collections.Generic;
class Program
{
    void M()
    {
        List<int> numbers = null;
        int? i = null;

        numbers ??= new List<int>();
    }
}", 10);

    [TestMethod]
    public void GetLineNumbers_AssignmentAndDeclarationInTheSameDeconstruction() =>
        AssertLineNumbersOfExecutableLines(
@"using System;
using System.Collections.Generic;
class Program
{
    void M()
    {
        List<int> numbers = null;
        int? i = null;

        (i, int k) = (42, 42);
    }
}", 10);

    [TestMethod]
    public void GetLineNumbers_NullCoalescingOperator() =>
        AssertLineNumbersOfExecutableLines(
@"using System;
using System.Collections.Generic;
using System.Linq;
class Program
{
    double SumNumbers(List<double[]> setsOfNumbers, int indexOfSetToSum)
    {
        return setsOfNumbers?[indexOfSetToSum]?.Sum() // +1
                ?? double.NaN; // +1
    }
}", 8, 9);

    [TestMethod]
    public void GetLineNumbers_SingleLinePatternMatching() =>
        AssertLineNumbersOfExecutableLines(
@"static class Program
{
    public static bool IsLetter(this char c) =>
        c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
}");

    [TestMethod]
    public void GetLineNumbers_MultiLinePatternMatching() =>
        AssertLineNumbersOfExecutableLines(
@"using System;
using System.Collections.Generic;
class Program
{
    public static bool IsLetter(char c)
    {
        if (c is >= 'a'
                and <= 'z'
                        or >= 'A'
                            and <= 'Z')
        {
            return true;
        }
        return false;
    }
}", 7, 12, 14);

    [TestMethod]
    public void GetLineNumbers_MultiLineInvocation() =>
        AssertLineNumbersOfExecutableLines(
@"using System;
using System.Collections.Generic;
class Program
{
    public static bool Foo(int a, int b)
    {
        return Foo(1,
                    Bar());
    }

    public static int Bar() => 42;
}", 7, 8);

    [TestMethod]
    public void GetLineNumbers_PartialProperties() =>
        AssertLineNumbersOfExecutableLines(
            """
            partial class Partial
            {
                public partial int Property { get; set; }
            }
            partial class Partial
            {
                public partial int Property
                {
                    get => 1;
                    set { value.ToString(); } // +1
                }
            }
            """, 10);

    [TestMethod]
    public void GetLineNumbers_PartialProperties_Excluded() =>
        AssertLineNumbersOfExecutableLines(
            """
            partial class Partial
            {
                [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
                public partial int Property { get; set; }
            }
            partial class Partial
            {
                public partial int Property
                {
                    get => 1;
                    set { value.ToString(); } // The setter is ignored because of ExcludeFromCodeCoverage in the partial property declaration
                }
            }
            """);

    [TestMethod]
    public void GetLineNumbers_PartialIndexers_Excluded() =>
        AssertLineNumbersOfExecutableLines(
            """
            partial class Partial
            {
                [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
                public partial int this[int index] { get; set; }
            }
            partial class Partial
            {
                public partial int this[int index]
                {
                    get => 1;
                    set { } // The setter is ignored because of ExcludeFromCodeCoverage in the partial indexer declaration
                }
            }
            """);

#if NET

    [TestMethod]
    public void GetLineNumbers_Razor_NoExecutableLines() =>
        AssertLineNumbersOfExecutableLinesRazor("""
<p>Hello world!</p>
@* Noncompliant *@
@code
{
private void SomeMethod()
{
}
}
""", RazorFile);

    [TestMethod]
    public void GetLineNumbers_Razor_FieldReference() =>
        AssertLineNumbersOfExecutableLinesRazor("""
@page "/razor"
@using TestCases

<p>Current count: @currentCount</p>     <!-- Not counted -->

@currentCount                           <!-- Not counted -->

@code {
private int currentCount = 0;
}
""", RazorFile);

    [TestMethod]
    public void GetLineNumbers_Razor_MethodReferenceAndCall() =>
        AssertLineNumbersOfExecutableLinesRazor("""
<button @onclick="IncrementCount">Increment</button>    <!-- Not counted | Razor FN -->
<p> @(ShowAmount()) </p>                                <!-- +1 -->

@code {
[Parameter]
public int IncrementAmount { get; set; } = 1;

private void IncrementCount()
{
    IncrementAmount += 1;                           // +1
}

private string ShowAmount()
{
    return $"Amount: {IncrementAmount}";            // +1
}
}
""", RazorFile, 2, 10, 15);

    [TestMethod]
    public void GetLineNumbers_Razor_PropertyReference() =>
        AssertLineNumbersOfExecutableLinesRazor("""
@IncrementAmount <!-- Not counted -->

@code {
[Parameter]
public int IncrementAmount { get; set; } = 1;
}
""", RazorFile);

    [TestMethod]
    public void GetLineNumbers_Razor_Html() =>
        AssertLineNumbersOfExecutableLinesRazor("""
<ul>
@foreach (var todo in todos)                        <!-- +1 -->
{
    <li>@todo.Title</li>                            <!-- +1 -->
}
</ul>

<h3>Todo (@todos.Count(todo => !todo.IsDone))</h3>      <!-- +1 -->
""", RazorFile, 2, 4, 8);

    [TestMethod]
    public void GetLineNumbers_Razor_AssignmentAndDeclarationInTheSameDeconstruction() =>
        AssertLineNumbersOfExecutableLinesRazor("""
@code {
void Foo()
{
    int? i = null;
    (i, int j) = (42, 42);      // +1
}
}
""", RazorFile, 5);

    [TestMethod]
    public void GetLineNumbers_Razor_MultiLineInvocation() =>
        AssertLineNumbersOfExecutableLinesRazor("""
@code {
public static bool Foo(int a, int b)
{
    return Foo(1,               // +1
                Bar());         // +1
}

public static int Bar() => 42;
}
""", RazorFile, 4, 5);

    [TestMethod]
    public void GetLineNumbers_Razor_NullCoalescingAssignment() =>
        AssertLineNumbersOfExecutableLinesRazor("""
@code {
void Foo()
{
    List<int> numbers = null;
    numbers ??= new List<int>(); // +1
}
}
""", RazorFile, 5);

    [TestMethod]
    public void GetLineNumbers_Razor_MultiLinePatternMatching() =>
        AssertLineNumbersOfExecutableLinesRazor("""
@code {
bool IsLetter(char c)
{
    if (c is >= 'a'                     // +1
            and <= 'z'
                    or >= 'A'
                        and <= 'Z')
    {
        return true;                    // +1
    }
    return false;                       // +1
}
}
""", RazorFile, 4, 9, 11);

    [TestMethod]
    public void GetLineNumbers_Razor_NullCoalescingOperator() =>
        AssertLineNumbersOfExecutableLinesRazor("""
@code {
double SumNumbers(List<double[]> setsOfNumbers, int indexOfSetToSum)
{
    return setsOfNumbers?[indexOfSetToSum]?.Sum() // +1
            ?? double.NaN; // +1
}
}
""", RazorFile, 4, 5);

    [TestMethod]
    public void GetLineNumbers_Razor_LocalFunctions() =>
        AssertLineNumbersOfExecutableLinesRazor("""
@code {
void Foo()
{
    var x = LocalMethod();          // +1
    LocalMethod();                  // +1
    int LocalMethod() => 42;
}
}
""", RazorFile, 4, 5);

#endif

    private static void AssertLineNumbersOfExecutableLines(string code, params int[] expectedExecutableLines)
    {
        var (syntaxTree, semanticModel) = TestCompiler.CompileCS(code);
        CSharpExecutableLinesMetric.GetLineNumbers(syntaxTree, semanticModel).Should().BeEquivalentTo(expectedExecutableLines);
    }

    private static void AssertLineNumbersOfExecutableLinesRazor(string code, string fileName, params int[] expectedExecutableLines)
    {
        var compilation = new VerifierBuilder<DummyAnalyzerWithLocation>()
            .AddSnippet(code, fileName)
            .WithLanguageVersion(LanguageVersion.Latest)
            .Compile()
            .Single();

        var syntaxTree = compilation.SyntaxTrees.Single(x => x.ToString().Contains(fileName));
        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        var lineNumbers = CSharpExecutableLinesMetric.GetLineNumbers(syntaxTree, semanticModel);
        lineNumbers.Should().BeEquivalentTo(expectedExecutableLines);
    }
}
