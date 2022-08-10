/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class CSharpExecutableLinesMetricTest
    {
        [TestMethod]
        public void No_Executable_Lines() =>
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
        public void Class() =>
            AssertLineNumbersOfExecutableLines(
@"class Program
{
}");

        [TestMethod]
        public void Checked_Unchecked() =>
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
        public void Blocks() =>
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
        public void Statements() =>
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
        public void Loops() =>
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
        public void Conditionals() =>
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
        public void Conditionals2() =>
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
        public void Yields() =>
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
        public void AccessAndInvocation() =>
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
        public void Initialization() =>
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
        public void Property_Set() =>
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
        public void Property_Get() =>
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
        public void Lambdas() =>
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
        public void TryCatch() =>
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
        public void Test_16() =>
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
        public void Test_17() =>
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
        public void Class_Excluded() =>
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
        public void AttributeOnLocalFunction_Excluded() =>
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
        public void ExcludeFromCodeCoverage_AttributeVariants(string attribute) =>
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
        public void Record_Excluded() =>
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
        public void RecordStruct_Excluded() =>
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
        public void Struct_Excluded() =>
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
        public void Constructor_Excluded() =>
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
        public void Property_Excluded() =>
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
        public void Event_Excluded() =>
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
        public void PartialClasses_Excluded() =>
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
        public void PartialMethods_Excluded() =>
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
        public void AttributeAreIgnored() =>
            AssertLineNumbersOfExecutableLines(
@"using System.Reflection;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo(""FOO"")]");

        [TestMethod]
        public void OnlyAttributeAreIgnored() =>
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
        public void AttributeOnLambdaIsIgnored() =>
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
        public void ExpressionsAreCounted() =>
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
        public void MultiLineLoop() =>
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
        public void SwitchStatementWithMultipleCases() =>
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
        public void SwitchExpressionWithMultipleCases() =>
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
        public void MultiLineInterpolatedString() =>
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
        public void MultiLineInterpolatedStringWithMultipleLineExpressions() =>
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
        public void UsingDeclaration() =>
            AssertLineNumbersOfExecutableLines(
@"class Program
{
    void Foo(int? i, string s)
    {
        using var file = new System.IO.StreamWriter(""WriteLines2.txt"");
    }
}");

        [TestMethod]
        public void LocalFunctions() =>
            AssertLineNumbersOfExecutableLines(
@"class Program
{
    int M1()
    {
        int y;
        LocalFunction();
        return y;

        void LocalFunction() => y = 0;
    }

    int M2()
    {
        int y = 5;
        int x = 7;
        return Add(x, y);

        static int Add(int left, int right) => left + right;
    }
}", 6, 7, 16);

#if NET

        [TestMethod]
        public void IndicesAndRanges() =>
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
        public void NullCoalescingAssignment() =>
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
        public void AssignmentAndDeclarationInTheSameDeconstruction() =>
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
        public void NullCoalescingOperator() =>
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
        public void SingleLinePatternMatching() =>
            AssertLineNumbersOfExecutableLines(
@"static class Program
{
    public static bool IsLetter(this char c) =>
        c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
}");

        [TestMethod]
        public void MultiLinePatternMatching() =>
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
        public void MultiLineInvocation() =>
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

        private static void AssertLineNumbersOfExecutableLines(string code, params int[] expectedExecutableLines)
        {
            (var syntaxTree, var semanticModel) = TestHelper.CompileCS(code);
            Metrics.CSharp.CSharpExecutableLinesMetric.GetLineNumbers(syntaxTree, semanticModel).Should().BeEquivalentTo(expectedExecutableLines);
        }
    }
}
