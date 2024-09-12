/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.Enterprise.Core.Rules;
using SonarAnalyzer.Rules;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class SymbolicExecutionRunnerTest
{
    public TestContext TestContext { get; set; }

    // This test is meant to run all the symbolic execution rules together and verify different scenarios.
    [TestMethod]
    public void VerifySymbolicExecutionRules_CS() =>
        new VerifierBuilder<CS.SymbolicExecutionRunner>().AddPaths(@"SymbolicExecution\Sonar\SymbolicExecutionRules.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

    [TestMethod]
    public void Initialize_MethodBase_CS() =>
        VerifyClassMainCS(@"
public Sample() // ConstructorDeclaration
{
    string s = null;   // Noncompliant {{Message for SMain}}
}

public Sample(string s) => // ConstructorDeclaration
    s = null;   // Noncompliant {{Message for SMain}}

~Sample() // DestructorDeclaration
{
    string s = null;   // Noncompliant {{Message for SMain}}
}

public static implicit operator byte(Sample arg)
{
    string s = null;   // Noncompliant {{Message for SMain}}
    return 0;
}

public static explicit operator Sample(byte arg)
{
    string s = null;   // Noncompliant {{Message for SMain}}
    return null;
}

public static int operator +(Sample a, Sample b)  // OperatorDeclaration
{
    string s = null;   // Noncompliant {{Message for SMain}}
    return 0;
}

public void MethodDeclaration()
{
    string s = null;   // Noncompliant {{Message for SMain}}
}

public void MethodDeclaration(string s) =>
    s = null;   // Noncompliant {{Message for SMain}}");

    [TestMethod]
    public void Initialize_MethodBase_VB() =>
        VerifyClassMainVB(@"
Public Sub New() ' ConstructorDeclaration
    Dim S As String = Nothing ' Noncompliant {{Message for SMain}}
End Sub

Protected Overrides Sub Finalize()  ' Destructor
    Dim S As String = Nothing ' Noncompliant {{Message for SMain}}
End Sub

Public Shared Narrowing Operator CType(A As Sample) As Integer
    Dim S As String = Nothing ' Noncompliant {{Message for SMain}}
End Operator

Public Shared Widening Operator CType(A As Sample) As String
    Dim S As String = Nothing ' Noncompliant {{Message for SMain}}
End Operator

Public Shared Operator +(A As Sample, B As Sample)  ' OperatorDeclaration
    Dim S As String = Nothing ' Noncompliant {{Message for SMain}}
End Operator

Public Sub SubDeclaration()
    Dim S As String = Nothing ' Noncompliant {{Message for SMain}}
End Sub

Public Function FunctionDeclaration() As Integer
    Dim S As String = Nothing ' Noncompliant {{Message for SMain}}
End Function");

    [TestMethod]
    public void Initialize_Property_CS() =>
        VerifyClassMainCS(@"
private int target;
public int Property => target = 42;     // Noncompliant {{Message for SMain}}");

    [TestMethod]
    public void Initialize_Indexer_CS() =>
        VerifyClassMainCS(@"
private string target;
public string this[int index] => target = null; // Noncompliant {{Message for SMain}}");

    [TestMethod]
    public void Initialize_Accessors_CS() =>
        VerifyClassMainCS(@"
private string target;

public string BodyProperty
{
    get
    {
        string s = null;   // Noncompliant {{Message for SMain}}
        return s;
    }
    set
    {
        string s = value;   // Noncompliant {{Message for SMain}}
    }
}

public string ArrowProperty
{
    get => target = null;   // Noncompliant {{Message for SMain}}
    set => target = value;  // Noncompliant {{Message for SMain}}
}

public event EventHandler BlockEvent
{
    add
    {
        string s = null;   // Noncompliant {{Message for SMain}}
    }
    remove
    {
        string s = null;   // Noncompliant {{Message for SMain}}
    }
}

public event EventHandler ArrowEvent
{
    add => target = null;       // Noncompliant {{Message for SMain}}
    remove => target = null;    // Noncompliant {{Message for SMain}}
}

public string this[string index]
{
    get
    {
        string s = null;   // Noncompliant {{Message for SMain}}
        return s;
    }

    set
    {
        string s = null;   // Noncompliant {{Message for SMain}}
    }
}

public string this[float index]
{
    get => target = null;   // Noncompliant {{Message for SMain}}
    set => target = value;  // Noncompliant {{Message for SMain}}
}

");

    [TestMethod]
    public void Initialize_Accessors_VB() =>
        VerifyClassMainVB(@"
Public Property BodyProperty
    Get
        Dim S As String = Nothing ' Noncompliant {{Message for SMain}}
    End Get
    Set
        Dim S As String = Nothing ' Noncompliant {{Message for SMain}}
    End Set
End Property

Public Custom Event BlockEvent As EventHandler
    AddHandler(value As EventHandler)
        Dim S As String = Nothing ' Noncompliant {{Message for SMain}}
    End AddHandler
    RemoveHandler(value As EventHandler)
        Dim S As String = Nothing ' Noncompliant {{Message for SMain}}
    End RemoveHandler
    RaiseEvent(sender As Object, e As EventArgs)
        Dim S As String = Nothing ' Noncompliant {{Message for SMain}}
    End RaiseEvent
End Event");

#if NET

    [TestMethod]
    public void Initialize_Accessors_Init_CS() =>
        VerifyClassMainCS(@"
private string target;

public string InitOnlyPropertyBlock
{
    get => null;
    init
    {
        string s = null;   // Noncompliant {{Message for SMain}}
    }
}

public string InitOnlyPropertyArrow
{
    get => null;
    init => target = value; // Noncompliant {{Message for SMain}}
}");

#endif

    [TestMethod]
    public void Initialize_AnonymousFunction_CS() =>
        VerifyClassMainCS(@"
delegate void VoidDelegate();

public void Method()
{
    Action parenthesizedLambda = () =>  // Noncompliant {{Message for SMain}} - scaffolding noise
        {
            string s = null;            // Noncompliant {{Message for SMain}}
        };

    Action<int> simpleLambda = x =>     // Noncompliant {{Message for SMain}} - scaffolding noise
        {
            string s = null;            // Noncompliant {{Message for SMain}}
        };

    VoidDelegate anonymousMethod = delegate     // Noncompliant {{Message for SMain}} - scaffolding noise
    {
        string s = null;                // Noncompliant {{Message for SMain}}
    };
}");

    [TestMethod]
    public void Initialize_AnonymousFunction_VB() =>
        VerifyClassMainVB(@"
Public Sub Method()
    Dim S As String
    Use(Function() S = Nothing)     ' Noncompliant {{Message for SBinary}} This is not an assignment but binary comparison
    Use(Function()
            S = Nothing             ' Noncompliant {{Message for SMain}}
        End Function)
    Use(Sub() S = Nothing)          ' Noncompliant {{Message for SMain}}
    Use(Sub()
            S = Nothing             ' Noncompliant {{Message for SMain}}
        End Sub)
End Sub

Private Sub Use(A As Action)
End Sub

Private Sub Use(F As Func(Of Integer))
End Sub");

    [TestMethod]
    public void Initialize_LocalFunctions_CS() =>
        VerifyClassMainCS(@"
public void Method()
{
    void LocalFunction()
    {
        string s = null; // Noncompliant {{Message for SMain}}
    }

    static void StaticLocalFunction()
    {
        string s = null; // Noncompliant {{Message for SMain}}
    }

    void LocalFunctionWithExpressionBody(string s) => s = null; // Noncompliant {{Message for SMain}}
}");

    [TestMethod]
    public void Initialize_LocalFunction_TopLevelStatements() =>
        VerifyTopLevelStatements("""
            void LocalMethod(string s) =>
                s = null; // Noncompliant {{Message for SMain}}
            """);

    [TestMethod]
    public void Initialize_TopLevelStatements() =>
        VerifyTopLevelStatements("string s = null; // Noncompliant {{Message for SMain}}");

    [TestMethod]
    public void Analyze_DoNotRunWhenContainsDiagnostics() =>
        Verify(@"string s = null;   // Error [CS1525]: Invalid expression term '>' - misleading location, duplicate reporting from Roslyn
                     >>;            // Error [CS1525]: Invalid expression term '>' - this will set body.ContainsDiagnostics",
            ProjectType.Product,
            null);

    [TestMethod]
    public void Analyze_DoNotRunWhenInsideLinqExpression_CS() =>
        VerifyClassMainCS(@"
public void Main()
{
    WithExpression(() => 1 + 1);    // Compliant, SBinary is not triggered, because this method is not analyzed
    WithFunc(() => 1 + 1);          // Noncompliant {{Message for SBinary}} To be sure binary rule triggers

        System.Linq.Expressions.Expression<Func<object[], object>> expr;
        // Noncompliant@+1 {{Message for SMain}} This is scaffolding issue from the assignment to 'expr'
        expr = x => x.FirstOrDefault().ToString();      // Compliant
}

private void WithExpression(System.Linq.Expressions.Expression<Func<int>> arg) { }
private void WithFunc(Func<int> arg) { }");

    [TestMethod]
    public void Analyze_DoNotRunWhenInsideLinqExpression_VB() =>
        VerifyClassMainVB(@"
Public Sub Main()
    WithExpression(Function() 1 + 1)    ' Should be Compliant, SBinary is not triggered, because this method is not analyzed
    WithFunc(Function() 1 + 1)          ' Noncompliant {{Message for SBinary}} To be sure binary rule triggers

    Dim Expr As Linq.Expressions.Expression(Of Func(Of Integer))
    ' Noncompliant@+1 {{Message for SMain}} This is scaffolding issue from the assignment to 'Expr'
    Expr = Function() 1 + 1
End Sub

Private Sub WithExpression(Arg As Linq.Expressions.Expression(Of Func(Of Integer)))
End Sub

Private Sub WithFunc(Arg As Func(Of Integer))
End Sub");

    [TestMethod]
    public void Enabled_MainProject() =>
        Verify(@"string s = null;   // Noncompliant    {{Message for SAll}}
                                        // Noncompliant@-1 {{Message for SMain}}",
            ProjectType.Product,
            null);

    [TestMethod]
    public void Enabled_TestProject_StandaloneRun() =>
        Verify(@"string s = null;   // Noncompliant    {{Message for SAll}}
                                        // Noncompliant@-1 {{Message for STest}}",
            ProjectType.Test,
            null);

    [TestMethod]
    public void Enabled_TestProject_ScannerRun() =>
        Verify(@"string s = null;   // Noncompliant {{Message for STest}}",
            ProjectType.Test,
            AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Test));

    [TestMethod]
    public void Analyze_DescriptorsWithSameType_ExecutesOnce()
    {
        var code =
@"public class Sample
{
    public void Method()
    {
        string s = null;   // Noncompliant {{Message for SMain}} - this should be raised only once
    }
}";
        var another = AnalysisScaffolding.CreateDescriptorMain("SAnother");
        var sut = new ConfigurableSERunnerCS();
        sut.RegisterRule<MainScopeAssignmentRuleCheck>(MainScopeAssignmentRuleCheck.SMain);
        sut.RegisterRule<MainScopeAssignmentRuleCheck>(another);     // Register the same RuleCheck with another ID
        new VerifierBuilder().AddAnalyzer(() => sut)
            .AddSnippet(code)
            .WithOptions(ParseOptionsHelper.FromCSharp9)
            .WithOnlyDiagnostics(MainScopeAssignmentRuleCheck.SMain, another)
            .Verify();
    }

    [TestMethod]
    public void Analyze_Severity_ExecutesWhenAll() =>
        Verify(@"
var obj = new object(); // Noncompliant    {{Message for SAll}}
                        // Noncompliant@-1 {{Message for SMain}}
Monitor.Enter(obj);     // Noncompliant    {{Unlock this lock along all executions paths of this method.}} - S2222
if (condition)
{
    Monitor.Exit(obj);
}");

    [TestMethod]
    public void Analyze_Severity_ExecutesWhenMore() =>
        Verify("""
            string s = null;    // Noncompliant    {{Message for SAll}}
                                // Noncompliant@-1 {{Message for SMain}}
            s.ToString();       // Compliant, should not raise S2259
            """,
            expectingIssues: true,
            AllScopeAssignmentRuleCheck.SAll,
            MainScopeAssignmentRuleCheck.SMain);

    [TestMethod]
    public void Analyze_Severity_ExecutesWhenOne() =>
        Verify("""
            string s = null; // Noncompliant {{Message for SMain}}
            s.ToString();    // Compliant, should not raise S2259
            """,
            expectingIssues: true,
            MainScopeAssignmentRuleCheck.SMain);

    [TestMethod]
    public void Analyze_Severity_DoesNotExecutesWhenNone() =>
        Verify("""
            string s = null;   // Compliant, SMain and SAll are suppressed by test framework, because only 'SAnother' is active
            s.ToString();      // Compliant, should not raise S2259
            """,
            expectingIssues: false,
            AnalysisScaffolding.CreateDescriptorMain("SAnother"));

    [TestMethod]
    public void Analyze_ShouldExecute_ExcludesCheckFromExecution()
    {
        var sut = new ConfigurableSERunnerCS();
        sut.RegisterRule<InvocationAssignmentRuleCheck>(InvocationAssignmentRuleCheck.SInvocation);
        var builder = new VerifierBuilder().AddAnalyzer(() => sut);
        builder.AddSnippet("""
            public class Sample
            {
                public void Method()
                {
                    string s = null;    // Nothing is raised because InvocationAssignmentRuleCheck.ShouldExecute returns false
                }
            }
            """).VerifyNoIssues();
        builder.AddSnippet("""
            public class Sample
            {
                public void Method()
                {
                    string s = null;    // Noncompliant {{Message for SInvocation}} - because invocation is present in the method body
                    Method();
                }
            }
            """).Verify();
    }

    [TestMethod]
    public void Analyze_Rethrows_SymbolicExecutionException()
    {
        var code = @"
public class Sample
{
    public void Method()
    {
        string s = null;    // Nothing is raised because exception is thrown on the way
    }
}";
        var sut = new ConfigurableSERunnerCS();
        sut.RegisterRule<ThrowAssignmentRuleCheck>(ThrowAssignmentRuleCheck.SThrow);
        var compilation = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).AddSnippet(code).Solution.Compile(ParseOptionsHelper.CSharpLatest.ToArray()).Single();
        var errors = DiagnosticVerifier.AnalyzerExceptions(compilation, sut);
        errors.Should().ContainSingle().Which.GetMessage().Should()
            .Contain("SonarAnalyzer.SymbolicExecution.SymbolicExecutionException")
            .And.Contain("Error processing method: Method ## Method file: snippet0.cs ## Method line: 3,4 ## Inner exception: System.InvalidOperationException: This check is not useful.");
    }

    [TestMethod]
    public void IsActive_EditorConfig_Global() =>
        VerifyActivation(new TestSyntaxTreeOptionProvider("S3900"));

    [TestMethod]
    public void IsActive_EditorConfig_AllTrees() =>
        VerifyActivation(new TestSyntaxTreeOptionProvider("S3900", x => ReportDiagnostic.Warn));

    [DataTestMethod]
    [DataRow(ReportDiagnostic.Suppress)]    // Suppressed explicitly and directly
    [DataRow(ReportDiagnostic.Default)]     // Explicit Default will fall back to IsActivatedByDefault that should be False for that tree
    public void IsActive_EditorConfig_OneOfTwoTrees(ReportDiagnostic defaultSeverity)
    {
        const string additionalSnippet = """
            public class BetterSample
            {
                public string Method(string arg) =>
                    arg.ToString();     // S3900 only here, the default class Sample has S3900 suppressed
            }
            """;
        VerifyActivation(new TestSyntaxTreeOptionProvider("S3900", x => x.ToString().Contains("BetterSample") ? ReportDiagnostic.Warn : defaultSeverity), additionalSnippet);
    }

    private static void VerifyActivation(TestSyntaxTreeOptionProvider provider, string additionalSnippet = null)
    {
        var builder = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp);
        builder = builder.AddSnippet("""
            public class Sample
            {
                public string Method(string arg) =>
                    arg.ToString();     // S3900 here
            }
            """);
        if (additionalSnippet is not null)
        {
            builder = builder.AddSnippet(additionalSnippet);
        }
        var compilation = builder.GetCompilation();
        compilation = compilation.WithOptions(compilation.Options.WithSyntaxTreeOptionsProvider(provider));
        var allDiagnostics = DiagnosticVerifier.AnalyzerDiagnostics(compilation, new CS.SymbolicExecutionRunner(), CompilationErrorBehavior.Default);
        var diagnostic = allDiagnostics.Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be("S3900");
        diagnostic.Descriptor.IsEnabledByDefault.Should().BeFalse("we're explicitly activating non-SonarWay rule that is not active by default");
    }

    private static void Verify(string body, bool expectingIssues = true, params DiagnosticDescriptor[] onlyRules) =>
        Verify(body, ProjectType.Product, null, expectingIssues, onlyRules);

    private static void Verify(string body, ProjectType projectType, string sonarProjectConfigPath, bool expectingIssues = true, params DiagnosticDescriptor[] onlyRules)
    {
        var code = $$"""
            using System;
            using System.Threading;
            public class Sample
            {
                public void Main(bool condition)
                {
                    {{body}}
                }
            }
            """;
        VerifyCode<TestSERunnerCS>(code, projectType, ParseOptionsHelper.FromCSharp9, sonarProjectConfigPath, OutputKind.DynamicallyLinkedLibrary, expectingIssues, onlyRules);
    }

    private static void VerifyClassMainCS(string members)
    {
        var code = $$"""
            using System;
            using System.Linq;
            public class Sample
            {
                {{members}}
            }
            """;
        VerifyCode<TestSERunnerCS>(code, ProjectType.Product, ParseOptionsHelper.FromCSharp9, null, OutputKind.DynamicallyLinkedLibrary, true, MainScopeAssignmentRuleCheck.SMain, BinaryRuleCheck.SBinary);
    }

    private static void VerifyTopLevelStatements(string members)
    {
        var code = $"""
            using System;
            {members}
            """;
        VerifyCode<TestSERunnerCS>(code, ProjectType.Product, ParseOptionsHelper.FromCSharp9, null, OutputKind.ConsoleApplication, true, MainScopeAssignmentRuleCheck.SMain, BinaryRuleCheck.SBinary);
    }

    private static void VerifyClassMainVB(string members)
    {
        var code = $"""
            Public Class Sample
            {members}
            End Class
            """;
        VerifyCode<TestSERunnerVB>(code, ProjectType.Product, [], null, OutputKind.DynamicallyLinkedLibrary, true, MainScopeAssignmentRuleCheck.SMain, BinaryRuleCheck.SBinary);
    }

    private static void VerifyCode<TRunner>(string code,
                                            ProjectType projectType,
                                            ImmutableArray<ParseOptions> parseOptions,
                                            string sonarProjectConfigPath,
                                            OutputKind kind,
                                            bool expectingIssues,
                                            params DiagnosticDescriptor[] onlyRules) where TRunner : SymbolicExecutionRunnerBase, new()
    {
        var builder = new VerifierBuilder<TRunner>()
            .AddReferences(TestHelper.ProjectTypeReference(projectType))
            .AddSnippet(code)
            .WithAdditionalFilePath(sonarProjectConfigPath)
            .WithOptions(parseOptions)
            .WithOnlyDiagnostics(onlyRules)
            .WithConcurrentAnalysis(false)
            .WithOutputKind(kind);
        if (expectingIssues)
        {
            builder.Verify();
        }
        else
        {
            builder.VerifyNoIssues();
        }
    }

    private class TestSERunnerCS : CS.SymbolicExecutionRunner
    {
        public TestSERunnerCS() : base(AnalyzerConfiguration.AlwaysEnabled) { }

        protected override ImmutableDictionary<DiagnosticDescriptor, RuleFactory> AllRules => ImmutableDictionary<DiagnosticDescriptor, RuleFactory>.Empty
            .Add(BinaryRuleCheck.SBinary, CreateFactory<BinaryRuleCheck>())
            .Add(LocksReleasedAllPaths.S2222, CreateFactory<LocksReleasedAllPaths>())
            .Add(AllScopeAssignmentRuleCheck.SAll, CreateFactory<AllScopeAssignmentRuleCheck>())
            .Add(MainScopeAssignmentRuleCheck.SMain, CreateFactory<MainScopeAssignmentRuleCheck>())
            .Add(TestScopeAssignmentRuleCheck.STest, CreateFactory<TestScopeAssignmentRuleCheck>());
    }

    private class TestSERunnerVB : VB.SymbolicExecutionRunner
    {
        protected override ImmutableDictionary<DiagnosticDescriptor, RuleFactory> AllRules => ImmutableDictionary<DiagnosticDescriptor, RuleFactory>.Empty
            .Add(BinaryRuleCheck.SBinary, CreateFactory<BinaryRuleCheck>())
            .Add(AllScopeAssignmentRuleCheck.SAll, CreateFactory<AllScopeAssignmentRuleCheck>())
            .Add(MainScopeAssignmentRuleCheck.SMain, CreateFactory<MainScopeAssignmentRuleCheck>())
            .Add(TestScopeAssignmentRuleCheck.STest, CreateFactory<TestScopeAssignmentRuleCheck>());
    }

    private class ConfigurableSERunnerCS : CS.SymbolicExecutionRunner
    {
        private ImmutableDictionary<DiagnosticDescriptor, RuleFactory> allRules = ImmutableDictionary<DiagnosticDescriptor, RuleFactory>.Empty;

        public ConfigurableSERunnerCS() : base(AnalyzerConfiguration.AlwaysEnabled) { }

        protected override ImmutableDictionary<DiagnosticDescriptor, RuleFactory> AllRules => allRules;

        public void RegisterRule<TRuleCheck>(DiagnosticDescriptor descriptor) where TRuleCheck : SymbolicRuleCheck, new() =>
            allRules = allRules.Add(descriptor, CreateFactory<TRuleCheck>());
    }

    private class TestSyntaxTreeOptionProvider : SyntaxTreeOptionsProvider
    {
        private readonly string id;
        private readonly Func<SyntaxTree, ReportDiagnostic?> treeFilter;

        public TestSyntaxTreeOptionProvider(string id, Func<SyntaxTree, ReportDiagnostic?> treeFilter = null)
        {
            this.id = id;
            this.treeFilter = treeFilter;
        }

        public override GeneratedKind IsGenerated(SyntaxTree tree, CancellationToken cancellationToken) =>
            GeneratedKind.NotGenerated;

        public override bool TryGetDiagnosticValue(SyntaxTree tree, string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity)
        {
            if (diagnosticId == id && treeFilter is not null && treeFilter(tree) is { } treeSeverity)
            {
                severity = treeSeverity;
                return true;
            }
            else
            {
                severity = ReportDiagnostic.Default;
                return false;
            }
        }

        public override bool TryGetGlobalDiagnosticValue(string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity)
        {
            severity = ReportDiagnostic.Warn;
            return diagnosticId == id && treeFilter is null;
        }
    }
}
