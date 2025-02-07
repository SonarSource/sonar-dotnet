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

using SonarAnalyzer.Core.Metrics;
using SonarAnalyzer.CSharp.Metrics;
using SonarAnalyzer.VisualBasic.Metrics;

namespace SonarAnalyzer.Test.Common;

[TestClass]
public class MetricsTest
{
    [TestMethod]
    public void LinesOfCode()
    {
        LinesOfCode(AnalyzerLanguage.CSharp, string.Empty).Should().BeEmpty();
        LinesOfCode(AnalyzerLanguage.CSharp, "/* ... */\n").Should().BeEmpty();
        LinesOfCode(AnalyzerLanguage.CSharp, "namespace X { }").Should().Equal(1);
        LinesOfCode(AnalyzerLanguage.CSharp, "namespace X \n { \n }").Should().BeEquivalentTo(new[] { 1, 2, 3 });
        LinesOfCode(AnalyzerLanguage.CSharp, "public class Sample { public Sample() { System.Console.WriteLine(@\"line1 \n line2 \n line3 \n line 4\"); } }").Should().BeEquivalentTo(new[] { 1, 2, 3, 4 });

        LinesOfCode(AnalyzerLanguage.VisualBasic, string.Empty).Should().BeEmpty();
        LinesOfCode(AnalyzerLanguage.VisualBasic, "'\n").Should().BeEmpty();
        LinesOfCode(AnalyzerLanguage.VisualBasic, "Module Module1 : End Module").Should().BeEquivalentTo(new[] { 1 });
        LinesOfCode(AnalyzerLanguage.VisualBasic, "Module Module1 \n  \n End Module").Should().BeEquivalentTo(new[] { 1, 3 });
        LinesOfCode(AnalyzerLanguage.VisualBasic,
@"Public Class SomeClass
 Public Sub New()
  Console.WriteLine(""line1
line2
line3
line 4"")
 End Sub
End Class").Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8 });
    }

    [TestMethod]
    public void CommentsWithoutHeaders()
    {
        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, string.Empty).NonBlank.Should().BeEmpty();
        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, string.Empty).NoSonar.Should().BeEmpty();

        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "#if DEBUG\nfoo\n#endif").NonBlank.Should().BeEmpty();
        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; \n#if DEBUG\nfoo\n#endif").NonBlank.Should().BeEmpty();

        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "// foo").NonBlank.Should().BeEmpty();
        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "#if DEBUG\nfoo\n#endif\n// foo").NonBlank.Should().BeEmpty();

        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; // l1").NonBlank.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; // l1\n// l2").NonBlank.Should().BeEquivalentTo(new[] { 1, 2 });
        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /* l1 */").NonBlank.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /* l1 \n l2 */").NonBlank.Should().BeEquivalentTo(new[] { 1, 2 });
        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /* l1 \n l2 */").NonBlank.Should().BeEquivalentTo(new[] { 1, 2 });
        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /// foo").NonBlank.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /** foo */").NonBlank.Should().BeEquivalentTo(new[] { 1 });

        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /** foo \n \n bar */").NonBlank.Should().BeEquivalentTo(new[] { 1, 3 });
        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /** foo \r \r bar */").NonBlank.Should().BeEquivalentTo(new[] { 1, 3 });
        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /** foo \r\n \r\n bar */").NonBlank.Should().BeEquivalentTo(new[] { 1, 3 });

        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; // NOSONAR").NoSonar.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; // ooNOSONARoo").NoSonar.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; // nosonar").NoSonar.Should().BeEmpty();
        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; // nOSonAr").NoSonar.Should().BeEmpty();

        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /* NOSONAR */ /* foo*/").NoSonar.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /* NOSONAR */ /* foo */").NonBlank.Should().BeEmpty();

        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /* foo*/ /* NOSONAR */").NoSonar.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithoutHeaders(AnalyzerLanguage.CSharp, "using System; /* foo*/ /* NOSONAR */").NonBlank.Should().BeEmpty();

        CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, string.Empty).NonBlank.Should().BeEmpty();
        CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, string.Empty).NoSonar.Should().BeEmpty();

        CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "#If DEBUG Then\nfoo\n#End If").NonBlank.Should().BeEmpty();
        CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System \n#If DEBUG Then\nfoo\n#End If").NonBlank.Should().BeEmpty();

        CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "' foo").NonBlank.Should().BeEmpty();
        CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "#If DEBUG Then\nfoo\n#End If\n' foo").NonBlank.Should().BeEmpty();

        CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' l1").NonBlank.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' l1\n' l2").NonBlank.Should().BeEquivalentTo(new[] { 1, 2 });
        CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System ''' foo").NonBlank.Should().BeEquivalentTo(new[] { 1 });

        CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' NOSONAR").NoSonar.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' ooNOSONARoo").NoSonar.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' nosonar").NoSonar.Should().BeEmpty();
        CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' nOSonAr").NoSonar.Should().BeEmpty();

        CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' fndskgjsdkl \n ' {00000000-0000-0000-0000-000000000000}\n").NonBlank.Should().BeEquivalentTo(new[] { 1, 2 });
    }

    [TestMethod]
    public void CommentsWithHeaders()
    {
        CommentsWithHeaders(AnalyzerLanguage.CSharp, string.Empty).NonBlank.Should().BeEmpty();
        CommentsWithHeaders(AnalyzerLanguage.CSharp, string.Empty).NoSonar.Should().BeEmpty();

        CommentsWithHeaders(AnalyzerLanguage.CSharp, "#if DEBUG\nfoo\n#endif").NonBlank.Should().BeEmpty();
        CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; \n#if DEBUG\nfoo\n#endif").NonBlank.Should().BeEmpty();

        CommentsWithHeaders(AnalyzerLanguage.CSharp, "// foo").NonBlank.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithHeaders(AnalyzerLanguage.CSharp, "#if DEBUG\nfoo\n#endif\n// foo").NonBlank.Should().BeEquivalentTo(new[] { 4 });

        CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; // l1").NonBlank.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; // l1\n// l2").NonBlank.Should().BeEquivalentTo(new[] { 1, 2 });
        CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /* l1 */").NonBlank.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /* l1 \n l2 */").NonBlank.Should().BeEquivalentTo(new[] { 1, 2 });
        CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /* l1 \n l2 */").NonBlank.Should().BeEquivalentTo(new[] { 1, 2 });
        CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /// foo").NonBlank.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /** foo */").NonBlank.Should().BeEquivalentTo(new[] { 1 });

        CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /** foo \n \n bar */").NonBlank.Should().BeEquivalentTo(new[] { 1, 3 });
        CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /** foo \r \r bar */").NonBlank.Should().BeEquivalentTo(new[] { 1, 3 });
        CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /** foo \r\n \r\n bar */").NonBlank.Should().BeEquivalentTo(new[] { 1, 3 });

        CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; // NOSONAR").NoSonar.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; // ooNOSONARoo").NoSonar.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; // nosonar").NoSonar.Should().BeEmpty();
        CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; // nOSonAr").NoSonar.Should().BeEmpty();

        CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /* NOSONAR */ /* foo*/").NoSonar.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /* NOSONAR */ /* foo */").NonBlank.Should().BeEmpty();

        CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /* foo*/ /* NOSONAR */").NoSonar.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithHeaders(AnalyzerLanguage.CSharp, "using System; /* foo*/ /* NOSONAR */").NonBlank.Should().BeEmpty();

        CommentsWithHeaders(AnalyzerLanguage.VisualBasic, string.Empty).NonBlank.Should().BeEmpty();
        CommentsWithHeaders(AnalyzerLanguage.VisualBasic, string.Empty).NoSonar.Should().BeEmpty();

        CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "#If DEBUG Then\nfoo\n#End If").NonBlank.Should().BeEmpty();
        CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "Imports System \n#If DEBUG Then\nfoo\n#End If").NonBlank.Should().BeEmpty();

        CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "' foo").NonBlank.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "#If DEBUG Then\nfoo\n#End If\n' foo").NonBlank.Should().BeEquivalentTo(new[] { 4 });

        CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' l1").NonBlank.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' l1\n' l2").NonBlank.Should().BeEquivalentTo(new[] { 1, 2 });
        CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "Imports System ''' foo").NonBlank.Should().BeEquivalentTo(new[] { 1 });

        CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' NOSONAR").NoSonar.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' ooNOSONARoo").NoSonar.Should().BeEquivalentTo(new[] { 1 });
        CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' nosonar").NoSonar.Should().BeEmpty();
        CommentsWithHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' nOSonAr").NoSonar.Should().BeEmpty();

        CommentsWithoutHeaders(AnalyzerLanguage.VisualBasic, "Imports System ' fndskgjsdkl \n ' {00000000-0000-0000-0000-000000000000}\n").NonBlank.Should().BeEquivalentTo(new[] { 1, 2 });
    }

    [TestMethod]
    public void Classes()
    {
        Classes(AnalyzerLanguage.CSharp, string.Empty).Should().Be(0);
        Classes(AnalyzerLanguage.CSharp, "class Sample {}").Should().Be(1);
        Classes(AnalyzerLanguage.CSharp, "interface IMyInterface {}").Should().Be(1);
#if NET
        Classes(AnalyzerLanguage.CSharp, "record MyRecord {}").Should().Be(1);
        Classes(AnalyzerLanguage.CSharp, "record MyPositionalRecord(int Prop) {}").Should().Be(1);
        Classes(AnalyzerLanguage.CSharp, "record struct MyRecordStruct {}").Should().Be(1);
        Classes(AnalyzerLanguage.CSharp, "record struct MyPositionalRecordStruct(int Prop) {}").Should().Be(1);
        Classes(AnalyzerLanguage.CSharp, "file class FileScopedClass {}").Should().Be(1);
        Classes(AnalyzerLanguage.CSharp, "file record FileScopedRecord {}").Should().Be(1);
#endif
        Classes(AnalyzerLanguage.CSharp, "struct Sample {}").Should().Be(1);
        Classes(AnalyzerLanguage.CSharp, "enum MyEnum {}").Should().Be(0);
        Classes(AnalyzerLanguage.CSharp, "class Sample1 {} namespace MyNamespace { class Sample2 {} }").Should().Be(2);

        Classes(AnalyzerLanguage.VisualBasic, string.Empty).Should().Be(0);
        Classes(AnalyzerLanguage.VisualBasic, "Class M \n End Class").Should().Be(1);
        Classes(AnalyzerLanguage.VisualBasic, "Structure M \n End Structure").Should().Be(1);
        Classes(AnalyzerLanguage.VisualBasic, "Enum M \n None \n End Enum").Should().Be(0);
        Classes(AnalyzerLanguage.VisualBasic, "Interface M \n End Interface").Should().Be(1);
        Classes(AnalyzerLanguage.VisualBasic, "Module M \n End Module").Should().Be(1);
        Classes(AnalyzerLanguage.VisualBasic, "Class M \n End Class \n Namespace MyNamespace \n Class Sample2 \n End Class \n End Namespace").Should().Be(2);
    }

    [TestMethod]
    public void Accessors()
    {
        Functions(AnalyzerLanguage.CSharp, string.Empty).Should().Be(0);
        Functions(AnalyzerLanguage.CSharp, "class Sample { public int MyProperty { get; } }").Should().Be(1);
        Functions(AnalyzerLanguage.CSharp, "interface Sample { int MyProperty { get; } }").Should().Be(0);
        Functions(AnalyzerLanguage.CSharp, "abstract class Sample { public abstract int MyProperty { get; } }").Should().Be(0);
        Functions(AnalyzerLanguage.CSharp, "class Sample { public int MyProperty => 42; }").Should().Be(1);
        Functions(AnalyzerLanguage.CSharp, "class Sample { public int MyProperty { get; set; } }").Should().Be(2);
        Functions(AnalyzerLanguage.CSharp, "class Sample { public int MyProperty { get { return 0; } set { } } }").Should().Be(2);
        Functions(AnalyzerLanguage.CSharp, "class Sample { public event System.EventHandler OnSomething { add { } remove {} } }").Should().Be(2);
        Functions(AnalyzerLanguage.CSharp, "class Sample { public int MyMethod() { return 1; } }").Should().Be(1);
        Functions(AnalyzerLanguage.CSharp, "class Sample { public int MyMethod() => 1; }").Should().Be(1);
        Functions(AnalyzerLanguage.CSharp, "abstract class Sample { public abstract int MyMethod(); }").Should().Be(0);

        Functions(AnalyzerLanguage.VisualBasic, string.Empty).Should().Be(0);
        Functions(AnalyzerLanguage.VisualBasic,
            "Class Sample \n Public ReadOnly Property MyProperty As Integer \n End Class").Should().Be(0); // Is this the expected?
        Functions(AnalyzerLanguage.VisualBasic,
            "Class Sample \n Public Property MyProperty As Integer \n End Class")
            .Should().Be(0); // Is this the expected?
        Functions(AnalyzerLanguage.VisualBasic,
@"Class Sample
    Public Property MyProperty As Integer
        Get
            Return 0
        End Get
        Set(value As Integer)
        End Set
    End Property
End Class")
            .Should().Be(2);
        Functions(AnalyzerLanguage.VisualBasic,
@"Class Sample
    Public Custom Event Click As EventHandler
        AddHandler(ByVal value As EventHandler)
        End AddHandler
        RemoveHandler(ByVal value As EventHandler)
        End RemoveHandler
        RaiseEvent(ByVal sender As Object, ByVal e As EventArgs)
        End RaiseEvent
    End Event
End Class")
            .Should().Be(3);
    }

    [TestMethod]
    public void Statements()
    {
        Statements(AnalyzerLanguage.CSharp, string.Empty).Should().Be(0);
        Statements(AnalyzerLanguage.CSharp, "class Sample {}").Should().Be(0);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() {} }").Should().Be(0);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { {} } }").Should().Be(0);
        Statements(AnalyzerLanguage.CSharp, "class Sample { int MyMethod() { return 0; } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { int l = 42; } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { System.Console.WriteLine(); } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { ; } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { foo: ; } }").Should().Be(2);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { goto foo; foo: ;} }").Should().Be(3);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { while(true) break; } }").Should().Be(2);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { while(false) continue; } }").Should().Be(2);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { throw new System.Exception(); } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { System.Collections.Generic.IEnumerable<int> MyMethod() { yield return 42; } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { System.Collections.Generic.IEnumerable<int> MyMethod() { yield break; } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { while (false) {} } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { do {} while (false); } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { for (;;) {} } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { foreach (var e in new [] {1, 2, 3}) {} } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { using (var e = new System.IO.MemoryStream()) {} } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { unsafe { fixed (int* p = &this.x) {} } } int x; }").Should().Be(2);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { checked {} } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { unchecked {} } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { unsafe {} } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { if (false) {} } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod(int v) { switch (v) { case 0: break; } } }").Should().Be(2);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { try {} catch {} } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { try {} finally {} } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { try {} catch {} finally {} } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { int MyMethod() { int a = 42; System.Console.WriteLine(a); return a; } }").Should().Be(3);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void Bar() { void LocalFunction() { } } }").Should().Be(1);
        Statements(AnalyzerLanguage.CSharp, "class Sample { void Bar(System.Collections.Generic.List<(int, int)> names) { foreach ((int x, int y) in names){} } }").Should().Be(1);

        Statements(AnalyzerLanguage.VisualBasic, string.Empty).Should().Be(0);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n \n End Class").Should().Be(0);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n End Sub \n End Class").Should().Be(0);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n Function MyFunc() As Integer \n Return 0 \n End Function \n End Class").Should().Be(1);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n Dim a As Integer = 42 \n \n End Sub \n End Class").Should().Be(1);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n Console.WriteLine() \n End Sub \n End Class").Should().Be(1);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n Foo:  \n End Sub \n End Class").Should().Be(1);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n GoTo Foo \n Foo: \n End Sub \n End Class").Should().Be(2);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n Exit Sub \n End Sub \n End Class").Should().Be(1);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n Do : Continue Do : Loop\n End Sub \n End Class").Should().Be(2);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n Throw New Exception \n End Sub \n End Class").Should().Be(1);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n While False \n End While \n End Sub \n End Class").Should().Be(1);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n Do \n Loop While True \n End Sub \n End Class").Should().Be(1);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n For i As Integer = 0 To -1 \n Next \n End Sub \n End Class").Should().Be(1);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n For Each e As Integer In {1, 2, 3} \n Next\n End Sub \n End Class").Should().Be(1);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n Using e As New System.IO.MemoryStream() \n End Using \n End Sub \n End Class").Should().Be(1);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n If False Then \n End If \n End Sub \n End Class").Should().Be(1);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod(v As Integer) \n Select Case v \n Case 0 \n End Select \n End Sub \n End Class").Should().Be(1);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n Try \n Catch \n End Try \n End Sub \n End Class").Should().Be(1);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n Try \n Finally \n End Try \n End Sub \n End Class").Should().Be(1);
        Statements(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n Try \n Catch \n Finally \n End Try \n End Sub \n End Class").Should().Be(1);
        Statements(AnalyzerLanguage.VisualBasic,
            "Class Sample \n Function MyFunc() As Integer \n Dim a As Integer = 42 \n Console.WriteLine(a) \n Return a \n End " +
            "Function \n End Class")
            .Should().Be(3);
    }

#if NET

    [DataTestMethod]
    [DataRow("", 0)]
    [DataRow("class Sample { }", 0)]
    [DataRow("abstract class Sample { public abstract void MyMethod1(); }", 0)]
    [DataRow("interface Interface { void MyMethod1(); }", 0)]
    [DataRow("class Sample { static Sample() { } }", 1)]
    [DataRow("class Sample { public Sample() { } }", 1)]
    [DataRow("class Sample { ~Sample() { } }", 1)]
    [DataRow("class Sample { public void MyMethod2() { } }", 1)]
    [DataRow("class Sample { public static Sample operator +(Sample a) { return a; } }", 1)]
    [DataRow("class Sample { public int MyProperty { get; set; } }", 2)]
    [DataRow("class Sample { public int MyProperty { get { return 0; } } }", 1)]
    [DataRow("class Sample { public int MyProperty { set { } } }", 1)]
    [DataRow("class Sample { public int MyProperty { init { } } }", 1)]
    [DataRow("class Sample { public int MyProperty { get => 42; } }", 1)]
    [DataRow("class Sample { public int MyProperty { get { return 0; } set { } } }", 2)]
    [DataRow("class Sample { public event System.EventHandler OnSomething { add { } remove {} } }", 2)]
    [DataRow("class Sample { void Bar() { void LocalFunction() { } } }", 2)]
    [DataRow("""
        partial class Sample
        {
            public partial int MyProperty { get; set; } // The partial definition part of a property is not counted.
        }
        partial class Sample
        {
            public partial int MyProperty { get => 1; set { } }
        }
        """, 2)]
    [DataRow("""
        partial class Sample
        {
            public partial int this[int index] { get; set; }
        }
        partial class Sample
        {
            public partial int this[int index] { get => 1; set { } }
        }
        """, 2)]
    public void Functions_CSharp(string function, int expected) =>
        Functions(AnalyzerLanguage.CSharp, function).Should().Be(expected);

    [DataTestMethod]
    [DataRow("", 0)]
    [DataRow("Class Sample \n \n End Class", 0)]
    [DataRow("MustInherit Class Sample \n MustOverride Sub MyMethod() \n End Class", 0)]
    [DataRow("Interface MyInterface \n Sub MyMethod() \n End Interface", 0)]
    [DataRow("Class Sample \n Public Property MyProperty As Integer \n End Class", 0)]
    [DataRow("Class Sample \n Shared Sub New() \n End Sub \n End Class", 1)]
    [DataRow("Class Sample \n Sub New() \n End Sub \n End Class", 1)]
    [DataRow("Class Sample \n Protected Overrides Sub Finalize() \n End Sub \n End Class", 1)]
    [DataRow("Class Sample \n Sub MyMethod2() \n End Sub \n End Class", 1)]
    [DataRow("Class Sample \n Public Shared Operator +(a As Sample) As Sample \n Return a \n End Operator \n End Class", 1)]
    public void Functions_VisualBasic(string function, int expected) =>
        Functions(AnalyzerLanguage.VisualBasic, function).Should().Be(expected);
#endif

    [TestMethod]
    public void Complexity()
    {
        Complexity(AnalyzerLanguage.CSharp, string.Empty)
            .Should().Be(0);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { }")
            .Should().Be(0);
        Complexity(AnalyzerLanguage.CSharp, "abstract class Sample { public abstract void MyMethod(); }")
            .Should().Be(0);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { } }")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { return; } }")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { return; return; } }")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { { return; } } }")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { if (false) { } } }")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { if (false) { } else { } } }")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { var t = false ? 0 : 1; } }")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod(int p) { switch (p) { default: break; } } }")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod(int p) { var x = p switch { _ => 1  }; } }")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod(int p) { switch (p) { case 0: break; default: break; } } }")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod(int p) { var x = p switch { 0 => \"zero\", 1 => \"one\", _ => \"other\"  }; } }")
            .Should().Be(4);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod(bool first, bool second) { var _ = first switch { true => second switch { true => 1, _ => 2}, _ => 3}; } }")
            .Should().Be(5);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod(int p) { foo: ; } }")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod(int p) { do { } while (false); } }")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod(int p) { for (;;) { } } }")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod(System.Collections.Generic.List<int> p) { foreach (var i in p) { } } }")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod(int p) { var a = false; } }")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod(int p) { var a = false && false; } }")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod(int p) { var a = false || true; } }")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { int MyProperty { get; set; } }")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.CSharp, """
                partial class Sample { partial int MyProperty { get; set; } }
                partial class Sample { partial int MyProperty { get => 1; set { } } }
            """).Should().Be(4);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { int MyProperty { get => 0; set {} } }")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { public Sample() { } }")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { ~Sample() { } }")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { public static Sample operator +(Sample a) { return a; } }")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { public event System.EventHandler OnSomething { add { } remove {} } }")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { var t = (string)null ?? string.Empty; } }")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod(int? t) { t ??= 0; } }")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod(int? a, int? b, int? c) => a ??= b ??= c ??= 0; }")
            .Should().Be(4);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { int? t = null; t?.ToString(); } }")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { throw new System.Exception(); } }")
           .Should().Be(1);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { try { } catch(System.Exception e) { } } }")
           .Should().Be(1);
        Complexity(AnalyzerLanguage.CSharp, "class Sample { void MyMethod() { goto Foo; Foo: var i = 0; } }")
           .Should().Be(1);

        Complexity(AnalyzerLanguage.VisualBasic, string.Empty)
            .Should().Be(0);
        Complexity(AnalyzerLanguage.VisualBasic, "Class Sample \n End Class")
            .Should().Be(0);
        Complexity(AnalyzerLanguage.VisualBasic, "MustInherit Class Sample \n Public MustOverride Sub MyMethod() \n End Class")
            .Should().Be(0);
        Complexity(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n End Sub \n End Class")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n Return \n End Sub \n End Class")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n Return \n Return \n End Sub \n End Class")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n If False Then \n End If \n End Sub \n End Class")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n If False Then \n Else \n End If \n End Sub \n End Class")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod(p As Integer) \n Select Case p \n Case Else \n Exit Select \n End Select \n End Sub \n End Class")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.VisualBasic,
            "Class Sample \n Sub MyMethod(p As Integer) \n Select Case p \n Case 3 \n Exit Select \n Case Else \n Exit Select \n " +
            "End Select \n End Sub \n End Class")
            .Should().Be(4);
        Complexity(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n Foo: \n End Sub \n End Class")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n Do \n Loop While True \n End Sub \n End Class")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n For i As Integer = 0 To -1 \n Next \n End Sub \n End Class")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n For Each e As Integer In {1, 2, 3} \n Next\n End Sub \n End Class")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n Dim a As Boolean = False\n End Sub \n End Class")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n Dim a As Boolean = False And False\n End Sub \n End Class")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub MyMethod() \n Dim a As Boolean = False Or False\n End Sub \n End Class")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.VisualBasic, "Class Sample \n Public Property MyProperty As Integer \n End Class")
            .Should().Be(0);
        Complexity(AnalyzerLanguage.VisualBasic, "Class Sample \n Public Property MyProperty As Integer \n Get \n End Get " +
            "\n Set(value As Integer) \n End Set \n End Property \n End Class")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.VisualBasic, "Class Sample \n Sub New() \n End Sub \n End Class")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.VisualBasic, "Class Sample \n Protected Overrides Sub Finalize() \n End Sub \n End Class")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.VisualBasic, "Class Sample \n Public Shared Operator +(a As Sample) As Sample \n Return a \n End Operator \n End Class")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.VisualBasic,
@"Class Sample
    Public Custom Event OnSomething As EventHandler

        AddHandler(ByVal value As EventHandler)
        End AddHandler
        RemoveHandler(ByVal value As EventHandler)
        End RemoveHandler
        RaiseEvent(ByVal sender As Object, ByVal e As EventArgs)
        End RaiseEvent
    End Event
End Class")
            .Should().Be(3);

        Complexity(AnalyzerLanguage.VisualBasic, "Module Module1\n Class Sample\n Function Bar() \n Return 0\n End Function \n End Class\n End Module")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.VisualBasic,
            "Module Module1\n Class Sample\n Function Bar() \n If False Then \n Return 1 \n Else \n Return 0 " +
            "\n End If\n End Function\n End Class\n End Module")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.VisualBasic, "Module Module1\n Class Sample\n Function Foo() \n Return 42\n End Function\n End Class\n End Module")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.VisualBasic, "Module Module1\n Class Sample\n ReadOnly Property MyProp \n Get \n Return \"\" \n End Get \n End Property\n End Class\n End Module")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.VisualBasic, "Module Module1\n Class Sample\n Sub Foo() \n End Sub\n End Class\n End Module")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.VisualBasic, "Module Module1\n Class Sample\n Sub Foo() \n Dim Foo = If(True, True, False)\n End Sub\n End Class\n End Module")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.VisualBasic, "Module Module1\n Class Sample\n Sub Foo() \n Dim Foo = Function() 0\n End Sub\n End Class\n End Module")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.VisualBasic,
            "Module Module1\n Class Sample\n Sub Foo() \n Dim Foo = Function() \n Return False \n " +
            "End Function\n End Sub\n End Class\n End Module")
            .Should().Be(1);
        Complexity(AnalyzerLanguage.VisualBasic, "Module Module1\n Class Sample\n Sub Foo() \n Throw New AccessViolationException()\n End Sub\n End Class\n End Module")
            .Should().Be(2);
        Complexity(AnalyzerLanguage.VisualBasic, "Module Module1\n Class Sample\n Sub Method() \n GoTo Foo \n Foo: \n End Sub\n End Class\n End Module")
            .Should().Be(2);
    }

    [TestMethod]
    public void CognitiveComplexity()
    {
        var csharpText = System.IO.File.ReadAllText(@"TestCases\CognitiveComplexity.cs");
        CognitiveComplexity(AnalyzerLanguage.CSharp, csharpText).Should().Be(109);

        var csharpLatestText = System.IO.File.ReadAllText(@"TestCases\CognitiveComplexity.Latest.cs");
        var csharpLatestCompilation = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp, OutputKind.ConsoleApplication)
            .AddSnippet(csharpLatestText)
            .GetCompilation();
        var csharpLatestTree = csharpLatestCompilation.SyntaxTrees.Single();
        new CSharpMetrics(csharpLatestTree, csharpLatestCompilation.GetSemanticModel(csharpLatestTree)).CognitiveComplexity.Should().Be(46);

        var visualBasicCode = System.IO.File.ReadAllText(@"TestCases\CognitiveComplexity.vb");
        CognitiveComplexity(AnalyzerLanguage.VisualBasic, visualBasicCode).Should().Be(122);
    }

    [TestMethod]
    public void WrongMetrics_CSharp()
    {
        var (syntaxTree, semanticModel) = TestCompiler.CompileVB(string.Empty);
        Assert.ThrowsException<ArgumentException>(() => new CSharpMetrics(syntaxTree, semanticModel));
    }

    [TestMethod]
    public void WrongMetrics_VisualBasic()
    {
        var (syntaxTree, semanticModel) = TestCompiler.CompileCS(string.Empty);
        Assert.ThrowsException<ArgumentException>(() => new VisualBasicMetrics(syntaxTree, semanticModel));
    }

    [TestMethod]
    public void ExecutableLinesMetricsIsPopulated_CSharp() =>
        ExecutableLines(AnalyzerLanguage.CSharp,
            @"public class Sample { public void Foo(int x) { int i = 0; if (i == 0) {i++;i--;} else { while(true){i--;} } } }")
            .Should().Equal(1);

    [TestMethod]
    public void ExecutableLinesMetricsIsPopulated_VB() =>
        ExecutableLines(AnalyzerLanguage.VisualBasic,
@"Module MainMod
    Private Sub Foo(x As Integer)
        If x = 42 Then
        End If
    End Sub
End Module")
            .Should().Equal(3);

    private static ISet<int> LinesOfCode(AnalyzerLanguage language, string text) =>
        MetricsFor(language, text).CodeLines;

    private static FileComments CommentsWithoutHeaders(AnalyzerLanguage language, string text) =>
        MetricsFor(language, text).GetComments(true);

    private static FileComments CommentsWithHeaders(AnalyzerLanguage language, string text) =>
        MetricsFor(language, text).GetComments(false);

    private static int Classes(AnalyzerLanguage language, string text) =>
        MetricsFor(language, text).ClassCount;

    private static int Statements(AnalyzerLanguage language, string text) =>
        MetricsFor(language, text).StatementCount;

    private static int Functions(AnalyzerLanguage language, string text) =>
        MetricsFor(language, text).FunctionCount;

    private static int Complexity(AnalyzerLanguage language, string text) =>
        MetricsFor(language, text).Complexity;

    private static int CognitiveComplexity(AnalyzerLanguage language, string text) =>
        MetricsFor(language, text).CognitiveComplexity;

    private static ICollection<int> ExecutableLines(AnalyzerLanguage language, string text) =>
        MetricsFor(language, text).ExecutableLines;

    private static MetricsBase MetricsFor(AnalyzerLanguage language, string text)
    {
        if (language != AnalyzerLanguage.CSharp && language != AnalyzerLanguage.VisualBasic)
        {
            throw new ArgumentException("Supplied language is not C# neither VB.Net", nameof(language));
        }

        var (syntaxTree, semanticModel) = TestCompiler.Compile(text, false, language);

        return language == AnalyzerLanguage.CSharp
            ? new CSharpMetrics(syntaxTree, semanticModel)
            : new VisualBasicMetrics(syntaxTree, semanticModel);
    }
}
