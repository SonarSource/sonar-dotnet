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

using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.Trackers;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class MethodDeclarationTrackerTest
    {
        private const string TestInputCS = @"
public class Sample
{
    public void NoArgs() {}
}";

        [TestMethod]
        public void MatchMethodName()
        {
            var tracker = new CSharpMethodDeclarationTracker();
            var context = CreateContext(TestInputCS, AnalyzerLanguage.CSharp, "NoArgs");
            tracker.MatchMethodName("NoArgs")(context).Should().BeTrue();
            tracker.MatchMethodName("Something")(context).Should().BeFalse();
        }

#if NET

        [TestMethod]
        public void Track_VerifyMethodIdentifierLocations_CS()
        {
            const string code = @"
abstract record AbstractRecordHasMethodSymbol(string Y); //For Coverage

public class Sample
{
    public Sample() {}
//         ^^^^^^
    public void Method()
//              ^^^^^^
    {
        void LocalFunction() { } // Tracking of local functions is not supported
    }
    ~Sample() {}
//   ^^^^^^
    public int Property
//             ^^^^^^^^
//             ^^^^^^^^ @-1
    {
        get => 42;
        set { }
    }
    public int InitProperty
//             ^^^^^^^^^^^^
//             ^^^^^^^^^^^^ @-1
    {
        get => 42;
        init { }
    }
    public int this[int index]
//             ^^^^
//             ^^^^ @-1
    {
        get => 42;
        set { }
    }
    public event System.EventHandler Event
//                                   ^^^^^
//                                   ^^^^^ @-1
    {
        add {}
        remove {}
    }
    public static int operator +(Sample a, Sample b) => 42;
//                             ^
}";
            Verifier.VerifyCSharpAnalyzer(code, new TestRule_CS(), ParseOptionsHelper.FromCSharp9);
        }

#endif

        [TestMethod]
        public void Track_VerifyMethodIdentifierLocations_VB()
        {
            const string code = @"
Public Class Sample

    Public Sub New()
        '      ^^^
    End Sub

    Public Sub Procedure()
        '      ^^^^^^^^^
    End Sub

    Public Function SomeFunction() As Integer
        '           ^^^^^^^^^^^^
    End Function

    Protected Overrides Sub Finalize()
        '                   ^^^^^^^^
        MyBase.Finalize()
    End Sub

    Public Property Prop As Integer
        '           ^^^^
        '           ^^^^ @-1
        Get
        End Get
        Set(value As Integer)
        End Set
    End Property

    Default Public Property Indexer(x As Integer, y As Integer) As Integer
        '                   ^^^^^^^
        '                   ^^^^^^^ @-1
        Get
        End Get
        Set(value As Integer)
        End Set
    End Property

    Public Custom Event Changed As System.EventHandler
        '               ^^^^^^^
        '               ^^^^^^^ @-1
        '               ^^^^^^^ @-2
        AddHandler(value As System.EventHandler)
        End AddHandler
        RemoveHandler(value As System.EventHandler)
        End RemoveHandler
        RaiseEvent(sender As Object, e As System.EventArgs)
        End RaiseEvent
    End Event

    Public Shared Operator +(A As Sample, B As Sample) As Integer
        '                  ^
    End Operator

    Declare Function ExternalMethod Lib ""foo.dll""(lpBuffer As String) As Integer

End Class";
            Verifier.VerifyVisualBasicAnalyzer(code, new TestRule_VB());
        }

        private static MethodDeclarationContext CreateContext(string testInput, AnalyzerLanguage language, string methodName)
        {
            var testCode = new SnippetCompiler(testInput, false, language);
            var symbol = testCode.GetMethodSymbol("Sample." + methodName);
            return new MethodDeclarationContext(symbol, testCode.SemanticModel.Compilation);
        }

        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        private class TestRule_CS : TrackerHotspotDiagnosticAnalyzer<CS.SyntaxKind>
        {
            protected override ILanguageFacade<CS.SyntaxKind> Language => CSharpFacade.Instance;

            public TestRule_CS() : base(AnalyzerConfiguration.AlwaysEnabled, "S104", "Message") { } // Any existing rule ID

            protected override void Initialize(TrackerInput input) =>
                Language.Tracker.MethodDeclaration.Track(input);
        }

        [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
        private class TestRule_VB : TrackerHotspotDiagnosticAnalyzer<VB.SyntaxKind>
        {
            protected override ILanguageFacade<VB.SyntaxKind> Language => VisualBasicFacade.Instance;

            public TestRule_VB() : base(AnalyzerConfiguration.AlwaysEnabled, "S104", "Message") { } // Any existing rule ID

            protected override void Initialize(TrackerInput input) =>
                Language.Tracker.MethodDeclaration.Track(input);
        }
    }
}
