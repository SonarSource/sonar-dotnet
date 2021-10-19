/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class CSharpExecutableLinesMetricTest
    {
        [TestMethod]
        public void No_Executable_Lines() =>
            AssertLinesOfCode(
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
            AssertLinesOfCode(
                @"class Program
                {
                }");

        [TestMethod]
        public void Checked_Unchecked() =>
            AssertLinesOfCode(
              @"
                static void Main(string[] args)
                {
                    checked // +1
                    {
                        unchecked // +1
                        {
                        }
                    }
                }",
              4, 6);

        [TestMethod]
        public void Blocks() =>
            AssertLinesOfCode(
              @"
                unsafe static void Main(int[] arr, object obj)
                {
                    lock (obj) { } // +1
                    fixed (int* p = arr) { } // +1
                    unsafe { } // +1
                    using ((IDisposable)obj) { } // +1
                }",
              4, 5, 6, 7);

        [TestMethod]
        public void Statements() =>
            AssertLinesOfCode(
              @"
                void Foo(int i)
                {
                    ; // +1
                    i++; // +1
                }",
              4, 5);

        [TestMethod]
        public void Loops() =>
            AssertLinesOfCode(
              @"
                void Foo(int[] arr)
                {
                    do {} // +1
                        while (true);
                    foreach (var a in arr) { }// +1
                    for (;;) { } // +1
                    while // +1
                        (true) { }
                }",
              4, 6, 7, 8);

        [TestMethod]
        public void Conditionals() =>
            AssertLinesOfCode(
              @"
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
                }",
              4, 5, 6, 11, 13, 14);

        [TestMethod]
        public void Conditionals2() =>
            AssertLinesOfCode(
              @"
                void Foo(Exception ex)
                {
                    goto home; // +1
                    throw ex; // +1
                    home: // +1

                    while (true) // +1
                    {
                        continue; // +1
                        break; // +1
                    }
                    return; // +1
                }",
              4, 5, 6, 8, 10, 11, 13);

        [TestMethod]
        public void Yields() =>
            AssertLinesOfCode(
              @"
               using System;
               using System.Collections.Generic;
               using System.Linq;

               namespace Test
               {
                   class Program
                   {
                       IEnumerable<string> Foo()
                       {
                           yield return ""; // +1
                           yield break; // +1
                       }
                   }
               }",
              12, 13);

        [TestMethod]
        public void AccessAndInvocation() =>
            AssertLinesOfCode(
              @"
                static void Main(string[] args)
                {
                    var x = args.Length; // +1
                    args.ToString(); // +1
                }",
              4, 5);

        [TestMethod]
        public void Initialization() =>
            AssertLinesOfCode(
              @"
                static string GetString() => "";

                static void Main()
                {
                    var arr = new object();
                    var arr2 = new int[] { 1 }; // +1

                    var ex = new Exception()
                    {
                        Source = GetString(), // +1
                        HelpLink = ""
                    };
                }",
              7, 11);

        [TestMethod]
        public void Property_Set() =>
            AssertLinesOfCode(
              @"
                class Program
                {
                    int Prop { get; set; }

                    void Foo()
                    {
                        Prop = 1; // + 1
                    }
                }",
              8);

        [TestMethod]
        public void Property_Get() =>
            AssertLinesOfCode(
              @"
                class Program
                {
                    int Prop { get; set; }

                    void Foo()
                    {
                        var x = Prop;
                    }
                }");

        [TestMethod]
        public void Lambdas() =>
            AssertLinesOfCode(
              @"
                using System;
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
                }",
              9, 10, 11);

        [TestMethod]
        public void TryCatch() =>
            AssertLinesOfCode(
              @"
                using System;
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
                }",
              9, 14);

        [TestMethod]
        public void Test_16() =>
            AssertLinesOfCode(
             @"using System;
               public void Foo(int x) {
               int i = 0; if (i == 0) {i++;i--;} else // +1
               { while(true){i--;} } // +1
               }",
             3, 4);

        [TestMethod]
        public void Test_17() =>
            AssertLinesOfCode(
             @"
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
                }",
             4);

        [TestMethod]
        public void ExcludeFromTestCoverage() =>
            AssertLinesOfCode(
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
                }",
              19, 22);

        [TestMethod]
        public void ExcludeFromTestCoverage_AttributeOnLocalFunction() =>
            AssertLinesOfCode(
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
                }",
              8, 14);

        [TestMethod]
        public void ExcludeFromTestCoverage_Variants()
        {
            var attributeVariants = new List<string>
            {
                "ExcludeFromCodeCoverage",
                "ExcludeFromCodeCoverage()",
                "ExcludeFromCodeCoverageAttribute",
                "ExcludeFromCodeCoverageAttribute()",
                "System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute()"
            };

            attributeVariants.ForEach(attr => AssertLinesOfCode(GenerateTest(attr)));

            string GenerateTest(string attribute)
            {
                return @"
                    using System.Diagnostics.CodeAnalysis;
                    public class ComplicatedCode
                    {
                        [" + attribute + @"]
                        public string ComplexFoo()
                        {
                            string text = null;
                            return text.ToLower();
                        }
                    }";
            }
        }

        [TestMethod]
        public void ExcludeClassFromTestCoverage() =>
            AssertLinesOfCode(
              @"
                using System;
                [ExcludeFromCodeCoverage]
                class Program
                {
                    static void Main(string[] args)
                    {
                        Main(null);
                    }
                }");

        [TestMethod]
        public void ExcludeRecordFromTestCoverage() =>
            AssertLinesOfCode(
              @"
                using System;
                [ExcludeFromCodeCoverage]
                record Program
                {
                    static void Main(string[] args)
                    {
                        Main(null);
                    }
                }");

        [TestMethod]
        public void ExcludeRecordStructFromTestCoverage() =>
            AssertLinesOfCode(
              @"
                using System;
                [ExcludeFromCodeCoverage]
                record struct Program
                {
                    static void Main(string[] args)
                    {
                        Main(null);
                    }
                }", 8); // Not correct should be 0

        [TestMethod]
        public void ExcludeStructFromTestCoverage() =>
            AssertLinesOfCode(
              @"namespace project_1
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
        public void ExcludePropertyFromTestCoverage() =>
            AssertLinesOfCode(
              @"[ExcludeFromCodeCoverage]
                class Program
                {
                    int FooProperty
                    {
                        get
                        {
                            return 1;
                        }
                    }
                }");

        [TestMethod]
        public void Constructor_ExcludeFromCodeCoverage() =>
            AssertLinesOfCode(
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
                }",
              14);

        [TestMethod]
        public void Property_ExcludeFromCodeCoverage() =>
            AssertLinesOfCode(
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
                }",
              15, 22, 23, 28, 29, 34);

        [TestMethod]
        public void Event_ExcludeFromCodeCoverage() =>
            AssertLinesOfCode(
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
                }",
              15, 22, 23);

        [TestMethod]
        public void PartialClasses_ExcludeFromCodeCoverage() =>
            AssertLinesOfCode(
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
                } ",
              20);

        [TestMethod]
        public void PartialMethods_ExcludeFromCodeCoverage() =>
            AssertLinesOfCode(
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
            AssertLinesOfCode(
                @"
                using System.Reflection;
                using System.Runtime.CompilerServices;
                [assembly: InternalsVisibleTo(""FOO"" + Signing.InternalsVisibleToPublicKey)]
                ");

        [TestMethod]
        public void OnlyAttributeAreIgnored() =>
            AssertLinesOfCode(
                @"
                [AnAttribute]
                public class Foo
                {
                    [AnAttribute]
                    void MyCode([AnAttribute] string foo)
                    {
                        System.Console.WriteLine(); // +1
                    }
                }",
                8);

        [TestMethod]
        public void ExpressionsAreCounted() =>
            AssertLinesOfCode(
                @"
                public void Foo(bool flag)
                {
                    if (flag) // +1
                    {
                        flag = true; flag = false; flag = true; // +1
                    }
                }",
                4, 6);

        [TestMethod]
        public void MultiLineLoop() =>
            AssertLinesOfCode(
              @"
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
                        }",
              4);

        [TestMethod]
        public void SwitchStatementWithMultipleCases() =>
            AssertLinesOfCode(
              @"
                        void Foo(int? i)
                        {
                            switch (i) // +1
                            {
                                case 1:
                                    Console.WriteLine(4); // +1
                                    break; // +1
                                case 2:
                                    Console.WriteLine(4); // +1
                                    break; // +1
                                default:
                                    break; // +1
                            }
                        }",
              4, 7, 8, 10, 11, 13);

        [TestMethod]
        public void SwitchExpressionWithMultipleCases() =>
            AssertLinesOfCode(
              @"
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
                ",
              12, 13);

        [TestMethod]
        public void MultiLineInterpolatedString() =>
            AssertLinesOfCode(
              @"
                        void Foo(int? i, string s)
                        {
                            string x = ""someString"";
                            x += @$""This is a Multi
                                                  Line
                                                  interpolated
                                                  string {i} {s}"";
                        }
                ",
              5);

        [TestMethod]
        public void MultiLineInterpolatedStringWithMultipleLineExpressions() =>
            AssertLinesOfCode(
              @"
                    public class C
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
                    }

               ",
              6, 7, 8, 9);

        [TestMethod]
        public void UsingDeclaration() =>
            AssertLinesOfCode(
              @"
                        void Foo(int? i, string s)
                        {
                            using var file = new System.IO.StreamWriter(""WriteLines2.txt"");
                        }
               ");

        [TestMethod]
        public void LocalFunctions() =>
            AssertLinesOfCode(
              @"
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
                ",
              5, 6, 15);

        [TestMethod]
        public void IndicesAndRanges() =>
            AssertLinesOfCode(
                @"
                    int M()
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
                        subArray = words[1..4]
                    }
                ",
                7, 18, 19);

        [TestMethod]
        public void NullCoalescingAsignment() =>
            AssertLinesOfCode(
                @"
                using System;
                using System.Collections.Generic;

                            int M()
                            {
                                List<int> numbers = null;
                                int? i = null;

                                numbers ??= new List<int>();
                            }
                        ",
                10);

        [TestMethod]
        public void NullCoalescingOperator() =>
            AssertLinesOfCode(
                @"
                        using System;
                        using System.Collections.Generic;

                            double SumNumbers(List<double[]> setsOfNumbers, int indexOfSetToSum)
                            {
                                return setsOfNumbers?[indexOfSetToSum]?.Sum() // +1
                                       ?? double.NaN; // +1
                            }
                 ",
                7, 8);

        [TestMethod]
        public void SingleLinePatternMatching() =>
            AssertLinesOfCode(
                @"
                        using System;
                        using System.Collections.Generic;

                            public static bool IsLetter(this char c) =>
                                c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
                 ");

        [TestMethod]
        public void MultiLinePatternMatching() =>
            AssertLinesOfCode(
                @"
                        using System;
                        using System.Collections.Generic;

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
                 ",
                7, 12, 14);

        [TestMethod]
        public void MultiLineInvocation() =>
            AssertLinesOfCode(
                @"
                        using System;
                        using System.Collections.Generic;

                            public static bool Foo(int a, int b)
                            {
                               return Foo(1,
                                          Bar());
                            }

                            public static int Bar() => 42;
                 ",
                7, 8);

        private static void AssertLinesOfCode(string code, params int[] expectedExecutableLines)
        {
            (var syntaxTree, var semanticModel) = TestHelper.Compile(code);
            Metrics.CSharp.CSharpExecutableLinesMetric.GetLineNumbers(syntaxTree, semanticModel).Should().BeEquivalentTo(expectedExecutableLines);
        }
    }
}
