/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using SonarAnalyzer.Core.Facade;
using SonarAnalyzer.Core.Trackers;
using SonarAnalyzer.CSharp.Core.Facade;
using SonarAnalyzer.CSharp.Core.Trackers;
using SonarAnalyzer.VisualBasic.Core.Facade;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.Test.Trackers;

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
        new VerifierBuilder<TestRule_CS>().AddSnippet(code).WithOptions(LanguageOptions.FromCSharp9).Verify();
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

    Public Custom Event Changed As EventHandler
        '               ^^^^^^^
        '               ^^^^^^^ @-1
        '               ^^^^^^^ @-2
        AddHandler(value As EventHandler)
        End AddHandler
        RemoveHandler(value As EventHandler)
        End RemoveHandler
        RaiseEvent(sender As Object, e As EventArgs)
        End RaiseEvent
    End Event

    Public Shared Operator +(A As Sample, B As Sample) As Integer
        '                  ^
    End Operator

    Declare Function ExternalMethod Lib ""foo.dll"" (lpBuffer As String) As Integer

End Class";
        new VerifierBuilder<TestRule_VB>().AddSnippet(code).Verify();
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
