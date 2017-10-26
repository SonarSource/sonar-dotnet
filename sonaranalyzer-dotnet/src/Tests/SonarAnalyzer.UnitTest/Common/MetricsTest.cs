/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using Microsoft.CodeAnalysis.VisualBasic;
using System;

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class MetricsTest
    {
        internal const string MetricsTestCategoryName = "Metrics";

        [TestMethod]
        [TestCategory(MetricsTestCategoryName)]
        public void Lines()
        {
            Lines(AnalyzerLanguage.CSharp, "").Should().Be(1);
            Lines(AnalyzerLanguage.CSharp, "\n").Should().Be(2);
            Lines(AnalyzerLanguage.CSharp, "\r").Should().Be(2);
            Lines(AnalyzerLanguage.CSharp, "\r\n").Should().Be(2);
            Lines(AnalyzerLanguage.CSharp, "\n").Should().Be(2);
            Lines(AnalyzerLanguage.CSharp, "\n\r").Should().Be(3);
            Lines(AnalyzerLanguage.CSharp, "using System;\r\n/*hello\r\nworld*/").Should().Be(3);

            Lines(AnalyzerLanguage.VisualBasic, "").Should().Be(1);
            Lines(AnalyzerLanguage.VisualBasic, "\n").Should().Be(2);
            Lines(AnalyzerLanguage.VisualBasic, "\r").Should().Be(2);
            Lines(AnalyzerLanguage.VisualBasic, "\r\n").Should().Be(2);
            Lines(AnalyzerLanguage.VisualBasic, "\n").Should().Be(2);
            Lines(AnalyzerLanguage.VisualBasic, "\n\r").Should().Be(3);
            Lines(AnalyzerLanguage.VisualBasic, "Imports System;\r\n/*hello\r\nworld*/").Should().Be(3);
        }

        private static int Lines(AnalyzerLanguage language, string text) => MetricsFor(language, text).LineCount;

        [TestMethod]
        [TestCategory(MetricsTestCategoryName)]
        public void LinesOfCode()
        {
            LinesOfCode(AnalyzerLanguage.CSharp, "").Should().BeEquivalentTo();
            LinesOfCode(AnalyzerLanguage.CSharp, "/* ... */\n").Should().BeEquivalentTo();
            LinesOfCode(AnalyzerLanguage.CSharp, "namespace X { }").Should().BeEquivalentTo(1);
            LinesOfCode(AnalyzerLanguage.CSharp, "namespace X \n { \n }").Should().BeEquivalentTo(1, 2, 3);
            LinesOfCode(AnalyzerLanguage.CSharp, "public class MyClass { public MyClass() { Console.WriteLine(@\"line1 \n line2 \n line3 \n line 4\"); } }").Should().BeEquivalentTo(1, 2, 3, 4);

            LinesOfCode(AnalyzerLanguage.VisualBasic, "").Should().BeEquivalentTo();
            LinesOfCode(AnalyzerLanguage.VisualBasic, "'\n").Should().BeEquivalentTo();
            LinesOfCode(AnalyzerLanguage.VisualBasic, "Module Module1 End Module").Should().BeEquivalentTo(1);
            LinesOfCode(AnalyzerLanguage.VisualBasic, "Module Module1 \n  \n End Module").Should().BeEquivalentTo(1, 3);
            LinesOfCode(AnalyzerLanguage.VisualBasic,
@"Public Class SomeClass
 Public Sub New()
  Console.WriteLine(""line1
line2
line3
line 4"")
 End Sub
End Class").Should().BeEquivalentTo(1, 2, 3, 4, 5, 6, 7, 8);
        }

        private static IImmutableSet<int> LinesOfCode(AnalyzerLanguage language, string text) => MetricsFor(language, text).CodeLines;

        [TestMethod]
        [TestCategory(MetricsTestCategoryName)]
        public void CommentsWithoutHeaders()
        {
            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "").NonBlank.Should().BeEquivalentTo();
            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "").NoSonar.Should().BeEquivalentTo();

            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "#ifdef DEBUG\nfoo\n#endif").NonBlank.Should().BeEquivalentTo();
            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; #ifdef DEBUG\nfoo\n#endif").NonBlank.Should().BeEquivalentTo();

            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "// foo").NonBlank.Should().BeEquivalentTo();
            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "#if DEBUG\nfoo\n#endif\n// foo").NonBlank.Should().BeEquivalentTo();

            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; // l1").NonBlank.Should().BeEquivalentTo(1);
            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; // l1\n// l2").NonBlank.Should().BeEquivalentTo(1, 2);
            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /* l1 */").NonBlank.Should().BeEquivalentTo(1);
            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /* l1 \n l2 */").NonBlank.Should().BeEquivalentTo(1, 2);
            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /* l1 \n l2 */").NonBlank.Should().BeEquivalentTo(1, 2);
            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /// foo").NonBlank.Should().BeEquivalentTo(1);
            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /** foo */").NonBlank.Should().BeEquivalentTo(1);

            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /** foo \n \n bar */").NonBlank.Should().BeEquivalentTo(1, 3);
            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /** foo \r \r bar */").NonBlank.Should().BeEquivalentTo(1, 3);
            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /** foo \r\n \r\n bar */").NonBlank.Should().BeEquivalentTo(1, 3);

            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; // NOSONAR").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; // ooNOSONARoo").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; // nosonar").NoSonar.Should().BeEquivalentTo();
            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; // nOSonAr").NoSonar.Should().BeEquivalentTo();

            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /* NOSONAR */ /* foo*/").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /* NOSONAR */ /* foo */").NonBlank.Should().BeEquivalentTo();

            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /* foo*/ /* NOSONAR */").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /* foo*/ /* NOSONAR */").NonBlank.Should().BeEquivalentTo();

            CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "").NonBlank.Should().BeEquivalentTo();
            CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "").NoSonar.Should().BeEquivalentTo();

            CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "#If DEBUG Then\nfoo\n#End If").NonBlank.Should().BeEquivalentTo();
            CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System #If DEBUG Then\nfoo\n#End If").NonBlank.Should().BeEquivalentTo();

            CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "' foo").NonBlank.Should().BeEquivalentTo();
            CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "#If DEBUG Then\nfoo\n#End If\n' foo").NonBlank.Should().BeEquivalentTo();

            CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' l1").NonBlank.Should().BeEquivalentTo(1);
            CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' l1\n' l2").NonBlank.Should().BeEquivalentTo(1, 2);
            CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System ''' foo").NonBlank.Should().BeEquivalentTo(1);

            CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' NOSONAR").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' ooNOSONARoo").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' nosonar").NoSonar.Should().BeEquivalentTo();
            CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' nOSonAr").NoSonar.Should().BeEquivalentTo();

            CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' fndskgjsdkl \n ' {00000000-0000-0000-0000-000000000000}\n").NonBlank.Should().BeEquivalentTo(1, 2);
        }

        private static FileComments CommentsWithoutHeaders(AnalyzerLanguage language, string text)
        {
            return MetricsFor(language, text).GetComments(true);
        }

        [TestMethod]
        [TestCategory(MetricsTestCategoryName)]
        public void CommentsWithHeaders()
        {
            CommentsWithHeaders(AnalyzerLanguage.CSharp, "").NonBlank.Should().BeEquivalentTo();
            CommentsWithHeaders(AnalyzerLanguage.CSharp, "").NoSonar.Should().BeEquivalentTo();

            CommentsWithHeaders(AnalyzerLanguage.CSharp, "#ifdef DEBUG\nfoo\n#endif").NonBlank.Should().BeEquivalentTo();
            CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; #ifdef DEBUG\nfoo\n#endif").NonBlank.Should().BeEquivalentTo();

            CommentsWithHeaders(AnalyzerLanguage.CSharp, "// foo").NonBlank.Should().BeEquivalentTo(1);
            CommentsWithHeaders(AnalyzerLanguage.CSharp, "#if DEBUG\nfoo\n#endif\n// foo").NonBlank.Should().BeEquivalentTo(4);

            CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; // l1").NonBlank.Should().BeEquivalentTo(1);
            CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; // l1\n// l2").NonBlank.Should().BeEquivalentTo(1, 2);
            CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /* l1 */").NonBlank.Should().BeEquivalentTo(1);
            CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /* l1 \n l2 */").NonBlank.Should().BeEquivalentTo(1, 2);
            CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /* l1 \n l2 */").NonBlank.Should().BeEquivalentTo(1, 2);
            CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /// foo").NonBlank.Should().BeEquivalentTo(1);
            CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /** foo */").NonBlank.Should().BeEquivalentTo(1);

            CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /** foo \n \n bar */").NonBlank.Should().BeEquivalentTo(1, 3);
            CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /** foo \r \r bar */").NonBlank.Should().BeEquivalentTo(1, 3);
            CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /** foo \r\n \r\n bar */").NonBlank.Should().BeEquivalentTo(1, 3);

            CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; // NOSONAR").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; // ooNOSONARoo").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; // nosonar").NoSonar.Should().BeEquivalentTo();
            CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; // nOSonAr").NoSonar.Should().BeEquivalentTo();

            CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /* NOSONAR */ /* foo*/").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /* NOSONAR */ /* foo */").NonBlank.Should().BeEquivalentTo();

            CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /* foo*/ /* NOSONAR */").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /* foo*/ /* NOSONAR */").NonBlank.Should().BeEquivalentTo();

            CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "").NonBlank.Should().BeEquivalentTo();
            CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "").NoSonar.Should().BeEquivalentTo();

            CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "#If DEBUG Then\nfoo\n#End If").NonBlank.Should().BeEquivalentTo();
            CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "Imports System #If DEBUG Then\nfoo\n#End If").NonBlank.Should().BeEquivalentTo();

            CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "' foo").NonBlank.Should().BeEquivalentTo(1);
            CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "#If DEBUG Then\nfoo\n#End If\n' foo").NonBlank.Should().BeEquivalentTo(4);

            CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' l1").NonBlank.Should().BeEquivalentTo(1);
            CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' l1\n' l2").NonBlank.Should().BeEquivalentTo(1, 2);
            CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "Imports System ''' foo").NonBlank.Should().BeEquivalentTo(1);

            CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' NOSONAR").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' ooNOSONARoo").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' nosonar").NoSonar.Should().BeEquivalentTo();
            CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' nOSonAr").NoSonar.Should().BeEquivalentTo();

            CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' fndskgjsdkl \n ' {00000000-0000-0000-0000-000000000000}\n").NonBlank.Should().BeEquivalentTo(1, 2);
        }

        private static FileComments CommentsWithHeaders(AnalyzerLanguage language, string text)
        {
            return MetricsFor(language, text).GetComments(false);
        }

        [TestMethod]
        [TestCategory(MetricsTestCategoryName)]
        public void Classes()
        {
            Classes(AnalyzerLanguage.CSharp, "").Should().Be(0);
            Classes(AnalyzerLanguage.CSharp, "class MyClass {}").Should().Be(1);
            Classes(AnalyzerLanguage.CSharp, "interface IMyInterface {}").Should().Be(1);
            Classes(AnalyzerLanguage.CSharp, "struct MyClass {}").Should().Be(1);
            Classes(AnalyzerLanguage.CSharp, "class MyClass1 {} namespace MyNamespace { class MyClass2 {} }").Should().Be(2);

            Classes(AnalyzerLanguage.VisualBasic, "").Should().Be(0);
            Classes(AnalyzerLanguage.VisualBasic, "Class M \n End Class").Should().Be(1);
            Classes(AnalyzerLanguage.VisualBasic, "Structure M \n End Structure").Should().Be(1);
            Classes(AnalyzerLanguage.VisualBasic, "Interface M \n End Interface").Should().Be(1);
            Classes(AnalyzerLanguage.VisualBasic, "Module M \n End Module").Should().Be(1);
            Classes(AnalyzerLanguage.VisualBasic, "Class M \n End Class \n Namespace MyNamespace \n Class MyClass2 \n End Class \n End Namespace").Should().Be(2);

        }

        private static int Classes(AnalyzerLanguage language, string text) => MetricsFor(language, text).ClassCount;

        [TestMethod]
        [TestCategory(MetricsTestCategoryName)]
        public void Accessors()
        {
            Functions(AnalyzerLanguage.CSharp, "").Should().Be(0);
            Functions(AnalyzerLanguage.CSharp, "class MyClass { public int MyProperty { get; } }").Should().Be(1);
            Functions(AnalyzerLanguage.CSharp, "interface MyClass { int MyProperty { get; } }").Should().Be(0);
            Functions(AnalyzerLanguage.CSharp, "abstract class MyClass { abstract int MyProperty { get; } }").Should().Be(0);
            Functions(AnalyzerLanguage.CSharp, "class MyClass { public int MyProperty => 42; }").Should().Be(1);
            Functions(AnalyzerLanguage.CSharp, "class MyClass { public int MyProperty { get; set; } }").Should().Be(2);
            Functions(AnalyzerLanguage.CSharp, "class MyClass { public int MyProperty { get { return 0; } set { } } }").Should().Be(2);
            Functions(AnalyzerLanguage.CSharp, "class MyClass { public event EventHandler OnSomething { add { } remove {} }").Should().Be(2);


            Functions(AnalyzerLanguage.VisualBasic, "").Should().Be(0);
            Functions(AnalyzerLanguage.VisualBasic,
                "Class MyClass \n Public ReadOnly Property MyProperty As Integer \n End Class").Should().Be(0); // Is this the expected?
            Functions(AnalyzerLanguage.VisualBasic,
                "Class MyClass \n Public Property MyProperty As Integer \n End Class")
                .Should().Be(0); // Is this the expected?
            Functions(AnalyzerLanguage.VisualBasic,
                "Class MyClass \n Public Property MyProperty As Integer \n Get \n Return 0 \n End Get \n" +
                "Set(value As Integer) \n End Set \n End Property \n End Class")
                .Should().Be(2);
            Functions(AnalyzerLanguage.VisualBasic,
                "Class MyClass \n Public Custom Event Click As EventHandler \n " +
                "AddHandler(ByVal value As EventHandler) \n EventHandlerList.Add(value) \n End AddHandler \n " +
                "RemoveHandler(ByVal value As EventHandler) \n EventHandlerList.Remove(value) \n End RemoveHandler \n " +
                "RaiseEvent(ByVal sender As Object, ByVal e As EventArgs) \n End RaiseEvent \n End Event \n End Class")
                .Should().Be(3);
        }

        [TestMethod]
        [TestCategory(MetricsTestCategoryName)]
        public void Statements()
        {
            Statements(AnalyzerLanguage.CSharp, "").Should().Be(0);
            Statements(AnalyzerLanguage.CSharp, "class MyClass {}").Should().Be(0);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() {} }").Should().Be(0);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { {} } }").Should().Be(0);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { int MyMethod() { return 0; } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { int l = 42; } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { Console.WriteLine(); } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { ; } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { foo: ; } }").Should().Be(2);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { goto foo; } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { break; } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { continue; } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { throw; } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { yield return 42; } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { yield break; } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { while (false) {} } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { do {} while (false); } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { for (;;) {} } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { foreach (var e in c) {} } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { using (var e = new MyClass()) {} } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { fixed (int* p = &pt.x) {} } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { checked {} } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { unchecked {} } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { unsafe {} } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { if (false) {} } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { switch (v) { case 0: ; } } }").Should().Be(2);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { try {} catch {} } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { try {} finally {} } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { try {} catch {} finally {} } }").Should().Be(1);
            Statements(AnalyzerLanguage.CSharp, "class MyClass { int MyMethod() { int a = 42; Console.WriteLine(a); return a; } }").Should().Be(3);

            Statements(AnalyzerLanguage.VisualBasic, "").Should().Be(0);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n \n End Class").Should().Be(0);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n End Sub \n End Class").Should().Be(0);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n {} \n End Sub \n End Class").Should().Be(0);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Function MyFunc() As Integer \n Return 0 \n End Function \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Dim a As Integer = 42 \n \n End Sub \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Console.WriteLine() \n End Sub \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Foo:  \n End Sub \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n GoTo Foo \n End Sub \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Break \n End Sub \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Continue \n End Sub \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Throw \n End Sub \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Yield 42 \n End Sub \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n yield Exit While \n End Sub \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n While False \n End While \n End Sub \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Do \n Loop While True \n End Sub \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n For i As Integer = 0 To -1 \n Next \n End Sub \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n For Each e As var In c \n Next\n End Sub \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Using e = New MyClass() \n End Using \n End Sub \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n If False \n End If \n End Sub \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Select Case v \n Case 0 \n End Select \n End Sub \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Try \n Catch \n End Try \n End Sub \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Try \n Finally \n End Try \n End Sub \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Try \n Catch \n Finally \n End Try \n End Sub \n End Class").Should().Be(1);
            Statements(AnalyzerLanguage.VisualBasic,
                "Class MyClass \n Function MyFunc() As Integer \n Dim a As Integer = 42 \n Console.WriteLine(a) \n Return a \n End " +
                "Function \n End Class")
                .Should().Be(3);
        }

        private static int Statements(AnalyzerLanguage language, string text) => MetricsFor(language, text).StatementCount;

        [TestMethod]
        [TestCategory(MetricsTestCategoryName)]
        public void Functions()
        {
            Functions(AnalyzerLanguage.CSharp, "").Should().Be(0);
            Functions(AnalyzerLanguage.CSharp, "class MyClass { }").Should().Be(0);
            Functions(AnalyzerLanguage.CSharp, "abstract class MyClass { public abstract void MyMethod1(); }").Should().Be(0);
            Functions(AnalyzerLanguage.CSharp, "abstract interface Interface { void MyMethod1(); }").Should().Be(0);
            Functions(AnalyzerLanguage.CSharp, "class MyClass { static MyClass() { } }").Should().Be(1);
            Functions(AnalyzerLanguage.CSharp, "class MyClass { public MyClass() { } }").Should().Be(1);
            Functions(AnalyzerLanguage.CSharp, "class MyClass { ~MyClass() { } }").Should().Be(1);
            Functions(AnalyzerLanguage.CSharp, "class MyClass { public void MyMethod2() { } }").Should().Be(1);
            Functions(AnalyzerLanguage.CSharp, "class MyClass { public static MyClass operator +(MyClass a) { return a; } }").Should().Be(1);
            Functions(AnalyzerLanguage.CSharp, "class MyClass { public int MyProperty2 { get { return 0; } } }").Should().Be(1);
            Functions(AnalyzerLanguage.CSharp, "class MyClass { public int MyProperty3 { set { } } }").Should().Be(1);
            Functions(AnalyzerLanguage.CSharp, "class MyClass { public int MyProperty4 { get { return 0; } set { } } }").Should().Be(2);
            Functions(AnalyzerLanguage.CSharp, "class MyClass { public event EventHandler OnSomething { add { } remove {} } }").Should().Be(2);

            Functions(AnalyzerLanguage.VisualBasic, "").Should().Be(0);
            Functions(AnalyzerLanguage.VisualBasic, "Class MyClass \n \n End Class").Should().Be(0);
            Functions(AnalyzerLanguage.VisualBasic, "MustInherit Class MyClass \n MustOverride Sub MyMethod() \n End Class").Should().Be(0);
            Functions(AnalyzerLanguage.VisualBasic, "Interface MyInterface \n Sub MyMethod() \n End Interface").Should().Be(0);
            Functions(AnalyzerLanguage.VisualBasic, "Class MyClass \n Public Property MyProperty As Integer \n End Class").Should().Be(0);
            Functions(AnalyzerLanguage.VisualBasic, "Class MyClass \n Shared Sub New() \n End Sub \n End Class").Should().Be(1);
            Functions(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub New() \n End Sub \n End Class").Should().Be(1);
            Functions(AnalyzerLanguage.VisualBasic, "Class MyClass \n Protected Overrides Sub Finalize() \n End Sub \n End Class").Should().Be(1);
            Functions(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod2() \n End Sub \n End Class").Should().Be(1);
            Functions(AnalyzerLanguage.VisualBasic, "Class MyClass \n Public Shared Operator +(a As MyClass) As MyClass \n Return a \n End Operator \n End Class").Should().Be(1);
        }

        private static int Functions(AnalyzerLanguage language, string text) => MetricsFor(language, text).FunctionCount;

        [TestMethod]
        [TestCategory(MetricsTestCategoryName)]
        public void PublicApi()
        {
            PublicApi(AnalyzerLanguage.CSharp, "").Should().Be(0);
            PublicApi(AnalyzerLanguage.CSharp, "class MyClass { }").Should().Be(0);
            PublicApi(AnalyzerLanguage.CSharp, "public class MyClass { }").Should().Be(1);
            PublicApi(AnalyzerLanguage.CSharp, "namespace MyNS { }").Should().Be(0);
            PublicApi(AnalyzerLanguage.CSharp, "public class MyClass { public void MyMethod() { } }").Should().Be(2);
            PublicApi(AnalyzerLanguage.CSharp, "private class MyClass { public void MyMethod() { } }").Should().Be(0);
            PublicApi(AnalyzerLanguage.CSharp, "public class MyClass { public int MyField; }").Should().Be(2);
            PublicApi(AnalyzerLanguage.CSharp, "public interface MyInterface { public class MyClass { } }").Should().Be(2);
            PublicApi(AnalyzerLanguage.CSharp, "namespace MyNS { public class MyClass { } }").Should().Be(1);
            PublicApi(AnalyzerLanguage.CSharp, "public class MyClass { public event EventHandler OnSomething; }").Should().Be(2);
            PublicApi(AnalyzerLanguage.CSharp, "public class MyClass { public delegate void Foo(); }").Should().Be(2);
            PublicApi(AnalyzerLanguage.CSharp, "public class MyClass { public static MyClass operator +(MyClass a) { return a; } }").Should().Be(2);
            PublicApi(AnalyzerLanguage.CSharp, "public class MyClass { public class MyClass2 { public int MyField; } }").Should().Be(3);
            PublicApi(AnalyzerLanguage.CSharp, "public enum MyEnum { MyValue1, MyValue2 }").Should().Be(1);
            PublicApi(AnalyzerLanguage.CSharp, "public struct MyStruct { public int MyField; }").Should().Be(2);
            PublicApi(AnalyzerLanguage.CSharp, "public class MyClass { public MyClass this[int i] { get { return null; } } }").Should().Be(2);
            PublicApi(AnalyzerLanguage.CSharp, "public class MyClass { public int MyProperty { get; set; } }").Should().Be(2);
            PublicApi(AnalyzerLanguage.CSharp, "public class MyClass { void MyMethod() { } }").Should().Be(1);
            PublicApi(AnalyzerLanguage.CSharp, "public class MyClass { private void MyMethod() { } }").Should().Be(1);
            PublicApi(AnalyzerLanguage.CSharp, "public class MyClass { protected MyMethod() { } }").Should().Be(1);
        }

        private static int PublicApi(AnalyzerLanguage language, string text) => MetricsFor(language, text).PublicApiCount;

        [TestMethod]
        [TestCategory(MetricsTestCategoryName)]
        public void PublicUndocumentedApi()
        {
            PublicUndocumentedApi(AnalyzerLanguage.CSharp, "").Should().Be(0);
            PublicUndocumentedApi(AnalyzerLanguage.CSharp, "class MyClass { }").Should().Be(0);
            PublicUndocumentedApi(AnalyzerLanguage.CSharp, "public class MyClass { }").Should().Be(1);
            PublicUndocumentedApi(AnalyzerLanguage.CSharp, "/* ... */ public class MyClass { }").Should().Be(0);
            PublicUndocumentedApi(AnalyzerLanguage.CSharp, "// ... \n public class MyClass { }").Should().Be(0);
            PublicUndocumentedApi(AnalyzerLanguage.CSharp, "public class MyClass { \n public int MyField; }").Should().Be(2);
            PublicUndocumentedApi(AnalyzerLanguage.CSharp, "public class MyClass { \n /* ... */ public int MyField; }").Should().Be(1);
            PublicUndocumentedApi(AnalyzerLanguage.CSharp, "/// ... \n public class MyClass { \n /* ... */ public int MyField; }").Should().Be(0);
        }

        private static int PublicUndocumentedApi(AnalyzerLanguage language, string text) => MetricsFor(language, text).PublicUndocumentedApiCount;

        [TestMethod]
        [TestCategory(MetricsTestCategoryName)]
        public void Complexity()
        {
            Complexity(AnalyzerLanguage.CSharp, "")
                .Should().Be(0);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { }")
                .Should().Be(0);
            Complexity(AnalyzerLanguage.CSharp, "abstract class MyClass { abstract void MyMethod(); }")
                .Should().Be(0);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { } }")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { return; } }")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { return; return; } }")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { { return; } } }")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { if (false) { } } }")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { if (false) { } else { } } }")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { var t = false ? 0 : 1; } }")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod(int p) { switch (p) { default: break; } } }")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod(int p) { switch (p) { case 0: break; default: break; } } }")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod(int p) { foo: ; } }")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod(int p) { do { } while (false); } }")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod(int p) { for (;;) { } } }")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod(List<int> p) { foreach (var i in p) { } } }")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod(int p) { var a = false; } }")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod(int p) { var a = false && false; } }")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod(int p) { var a = false || true; } }")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { int MyProperty { get; set; } }")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { int MyProperty { get {} set {} } }")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { public MyClass() { } }")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { ~MyClass() { } }")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { public static MyClass operator +(MyClass a) { return a; } }")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { public event EventHandler OnSomething { add { } remove {} } }")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { var t = null ?? 0; } }")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { int? t = null; t?.ToString(); } }")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { throw new Exception(); } }")
               .Should().Be(1);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { try { } catch(Exception e) { } } }")
               .Should().Be(1);
            Complexity(AnalyzerLanguage.CSharp, "class MyClass { void MyMethod() { goto Foo; Foo: var i = 0; } }")
               .Should().Be(1);

            Complexity(AnalyzerLanguage.VisualBasic, "")
                .Should().Be(0);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n End Class")
                .Should().Be(0);
            Complexity(AnalyzerLanguage.VisualBasic, "MustInherit Class MyClass \n Private MustOverride Sub MyMethod() \n End Class")
                .Should().Be(0);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n End Sub \n End Class")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Return \n End Sub \n End Class")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Return \n Return \n End Sub \n End Class")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n If False Then \n End If \n End Sub \n End Class")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n If False Then \n Else \n End If \n End Sub \n End Class")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Select Case p \n Case Else \n Exit Select \n End Select \n End Sub \n End Class")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.VisualBasic,
                "Class MyClass \n Sub MyMethod() \n Select Case p \n Case 3 \n Exit Select \n Case Else \n Exit Select \n " +
                "End Select \n End Sub \n End Class")
                .Should().Be(4);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Foo: \n End Sub \n End Class")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Do \n Loop While True \n End Sub \n End Class")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n For i As Integer = 0 To -1 \n Next \n End Sub \n End Class")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n For Each e As var In c \n Next\n End Sub \n End Class")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Dim a As Boolean = False\n End Sub \n End Class")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Dim a As Boolean = False And False\n End Sub \n End Class")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub MyMethod() \n Dim a As Boolean = False Or False\n End Sub \n End Class")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n Public Property MyProperty As Integer \n End Class")
                .Should().Be(0);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n Public Property MyProperty As Integer \n Get \n End Get " +
                "\n Set(value As Integer) \n End Set \n End Property \n End Class")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub New() \n End Sub \n End Class")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n Protected Overrides Sub Finalize() \n End Sub \n End Class")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n Public Shared Operator +(a As MyClass) As MyClass \n Return a \n End Operator \n End Class")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.VisualBasic, "Class MyClass \n Public Custom Event OnSomething As EventHandler \n " +
                "AddHandler(ByVal value As EventHandler) \n End AddHandler \n RemoveHandler(ByVal value As EventHandler) \n " +
                "End RemoveHandler \n End Event \n End Class")
                .Should().Be(2);

            Complexity(AnalyzerLanguage.VisualBasic, "Module Module1\n Class Foo\n Function Bar() \n Return 0\n End Function \n End Class\n End Module")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.VisualBasic,
                "Module Module1\n Class Foo\n Function Bar() \n If False Then \n Return 1 \n Else \n Return 0 " +
                "\n End If\n End Function\n End Class\n End Module")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.VisualBasic, "Module Module1\n Class Foo\n Function Foo() \n Dim foo = Sub() Return 42\n End Function\n End Class\n End Module")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.VisualBasic, "Module Module1\n Class Foo\n ReadOnly Property MyProp \n Get \n Return \"\" \n End Get \n End Property\n End Class\n End Module")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.VisualBasic, "Module Module1\n Class Foo\n Sub Foo() \n End Sub\n End Class\n End Module")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.VisualBasic, "Module Module1\n Class Foo\n Sub Foo() \n Dim Foo = If(True, True, False)\n End Sub\n End Class\n End Module")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.VisualBasic, "Module Module1\n Class Foo\n Sub Foo() \n Dim Foo = Function() 0\n End Sub\n End Class\n End Module")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.VisualBasic,
                "Module Module1\n Class Foo\n Sub Foo() \n Dim Foo = Function() \n Return False \n " +
                "End Function\n End Sub\n End Class\n End Module")
                .Should().Be(1);
            Complexity(AnalyzerLanguage.VisualBasic, "Module Module1\n Class Foo\n Sub Foo() \n Throw New AccessViolationException()\n End Sub\n End Class\n End Module")
                .Should().Be(2);
            Complexity(AnalyzerLanguage.VisualBasic, "Module Module1\n Class Foo\n Sub Foo() \n GoTo Foo\n End Sub\n End Class\n End Module")
                .Should().Be(2);
        }

        private static int Complexity(AnalyzerLanguage language, string text) => MetricsFor(language, text).Complexity;

        [TestMethod]
        [TestCategory(MetricsTestCategoryName)]
        public void FunctionComplexityDistribution()
        {
            FunctionComplexityDistribution(AnalyzerLanguage.CSharp, "").Ranges.Should().BeEquivalentTo(1, 2, 4, 6, 8, 10, 12);
            FunctionComplexityDistribution(AnalyzerLanguage.CSharp, "").Values.Should().BeEquivalentTo(0, 0, 0, 0, 0, 0, 0);
            FunctionComplexityDistribution(AnalyzerLanguage.CSharp, "class MyClass { void M1() { } }").Values.Should().BeEquivalentTo(1, 0, 0, 0, 0, 0, 0);
            FunctionComplexityDistribution(AnalyzerLanguage.CSharp, "class MyClass { void M1() { } void M2() { } }").Values.Should().BeEquivalentTo(2, 0, 0, 0, 0, 0, 0);
            FunctionComplexityDistribution(AnalyzerLanguage.CSharp, "class MyClass { void M1() { if (false) { } } }").Values.Should().BeEquivalentTo(0, 1, 0, 0, 0, 0, 0);
            FunctionComplexityDistribution(AnalyzerLanguage.CSharp, "class MyClass { void M1() { if (false) { } if (false) { } if (false) { } } }").Values.Should().BeEquivalentTo(0, 0, 1, 0, 0, 0, 0);

            FunctionComplexityDistribution(AnalyzerLanguage.VisualBasic, "").Ranges.Should().BeEquivalentTo(1, 2, 4, 6, 8, 10, 12);
            FunctionComplexityDistribution(AnalyzerLanguage.VisualBasic, "").Values.Should().BeEquivalentTo(0, 0, 0, 0, 0, 0, 0);
            FunctionComplexityDistribution(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub M1() \n End Sub \n End Class").Values.Should().BeEquivalentTo(1, 0, 0, 0, 0, 0, 0);
            FunctionComplexityDistribution(AnalyzerLanguage.VisualBasic, "Class MyClass \n Sub M1() \n End Sub \n Sub M2() \n End Sub \nEnd Class").Values.Should().BeEquivalentTo(2, 0, 0, 0, 0, 0, 0);
            FunctionComplexityDistribution(AnalyzerLanguage.VisualBasic,
                "Class MyClass \n Sub M1() \n If False Then \n End If \n End Sub \n End Class").Values.Should().BeEquivalentTo(0, 1, 0, 0, 0, 0, 0);
            FunctionComplexityDistribution(AnalyzerLanguage.VisualBasic,
                "Class MyClass \n Sub M1() \n If False Then \n End If \n If False Then \n End If \n If False Then \n End If " +
                "\n End Sub \n End Class").Values.Should().BeEquivalentTo(0, 0, 1, 0, 0, 0, 0);
        }

        private static Distribution FunctionComplexityDistribution(AnalyzerLanguage language, string text) =>
            MetricsFor(language, text).FunctionComplexityDistribution;

        private static MetricsBase MetricsFor(AnalyzerLanguage language, string text)
        {
            if (language != AnalyzerLanguage.CSharp &&
                language != AnalyzerLanguage.VisualBasic)
            {
                throw new ArgumentException("Supplied language is not C# neither VB.Net", nameof(language));
            }

            return language == AnalyzerLanguage.CSharp
                ? (MetricsBase)new SonarAnalyzer.Metrics.CSharp.Metrics(CSharpSyntaxTree.ParseText(text))
                : new SonarAnalyzer.Common.VisualBasic.Metrics(VisualBasicSyntaxTree.ParseText(text));
        }

        [TestMethod]
        [TestCategory(MetricsTestCategoryName)]
        [ExpectedException(typeof(ArgumentException))]
        public void WrongMetrics_CSharp()
        {
            new SonarAnalyzer.Metrics.CSharp.Metrics(VisualBasicSyntaxTree.ParseText(""));
        }

        [TestMethod]
        [TestCategory(MetricsTestCategoryName)]
        [ExpectedException(typeof(ArgumentException))]
        public void WrongMetrics_VisualBasic()
        {
            new SonarAnalyzer.Common.VisualBasic.Metrics(CSharpSyntaxTree.ParseText(""));
        }
    }
}
