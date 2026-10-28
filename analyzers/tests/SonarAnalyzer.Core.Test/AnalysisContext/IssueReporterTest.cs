/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Core.Configuration;

namespace SonarAnalyzer.Core.AnalysisContext.Test;

#pragma warning disable CS0618 // Type or member is obsolete
[TestClass]
public class IssueReporterTest
{
    private static readonly VerifierBuilder DummyBuilder = new VerifierBuilder<DummyAnalyzerCS>();
    private readonly Version defaultDesignTimeVersion = new("4.9.2");
    private readonly Version defaultBuildTimeVersion = new("4.9.0");
    private readonly DummyDiagnosticReporter reporter = new();

    public TestContext TestContext { get; set; }

    [TestMethod]
    [DataRow("S1481")]
    [DataRow("S927")]
    [DataRow("S4487")]
    [DataRow("S2696")]
    [DataRow("S2259")]
    [DataRow("S1144")]
    [DataRow("S2325")]
    [DataRow("S1117")]
    [DataRow("S1871")]
    [DataRow("S108")]
    public void ReportIssueCore_DesignTimeDiagnostic_ExcludedRule(string diagnosticId)
    {
        ReportDiagnostic(@"C:\SonarSource\SomeFile.razor.-6NXeWT5Akt4vxdz.ide.g.cs", true, diagnosticId);
        reporter.Counter.Should().Be(0);
        reporter.LastDiagnostic.Should().BeNull();
    }

    [TestMethod]
    public void ReportIssueCore_DesignTimeDiagnostic_HasMappedPath_True()
    {
        var diagnostic = ReportDiagnostic(@"C:\SonarSource\SomeFile.razor.-6NXeWT5Akt4vxdz.ide.g.cs", hasMappedPath: true);
        reporter.Counter.Should().Be(1);
        reporter.LastDiagnostic.Should().Be(diagnostic);
    }

    [TestMethod]
    public void ReportIssueCore_DesignTimeDiagnostic_HasMappedPath_False()
    {
        ReportDiagnostic(@"C:\SonarSource\SomeFile.razor.-6NXeWT5Akt4vxdz.ide.g.cs", hasMappedPath: false);
        reporter.Counter.Should().Be(0);
        reporter.LastDiagnostic.Should().BeNull();
    }

    [TestMethod]
    public void ReportIssueCore_DesignTimeDiagnostic_RoslynVersion_1000_0_0()
    {
        ReportDiagnostic(@"C:\SonarSource\SomeFile.razor.-6NXeWT5Akt4vxdz.ide.g.cs", hasMappedPath: true, roslynVersion: new Version(1000, 0, 0));
        reporter.Counter.Should().Be(0);
        reporter.LastDiagnostic.Should().BeNull();
    }

    [TestMethod]
    public void ReportIssueCore_DesignTimeDiagnostic_RoslynVersion_Greater_Than_Current()
    {
        var current = new Version(typeof(SemanticModel).Assembly.GetName().Version.ToString());
        var greaterThanCurrent = new Version(current.Major, current.Minor, current.Build + 1);

        ReportDiagnostic(@"C:\SonarSource\SomeFile.razor.-6NXeWT5Akt4vxdz.ide.g.cs", hasMappedPath: true, roslynVersion: greaterThanCurrent);
        reporter.Counter.Should().Be(0);
        reporter.LastDiagnostic.Should().BeNull();
    }

    [TestMethod]
    public void ReportIssueCore_DesignTimeDiagnostic_RoslynVersion_Less_Than_Current()
    {
        var lessthanCurrent = LessThanCurrent();

        var diagnostic = ReportDiagnostic(@"C:\SonarSource\SomeFile.razor.-6NXeWT5Akt4vxdz.ide.g.cs", hasMappedPath: true, roslynVersion: lessthanCurrent);
        reporter.Counter.Should().Be(1);
        reporter.LastDiagnostic.Should().Be(diagnostic);

        static Version LessThanCurrent()
        {
            var roslynVersion = new Version(typeof(SemanticModel).Assembly.GetName().Version.ToString());
            return roslynVersion.Build switch
            {
                > 0 => new Version(roslynVersion.Major, roslynVersion.Minor, roslynVersion.Build - 1),
                _ when roslynVersion.Minor > 0 => new Version(roslynVersion.Major, roslynVersion.Minor - 1),
                _ => new Version(roslynVersion.Major - 1, 99, 99),
            };
        }
    }

    [TestMethod]
    public void ReportIssueCore_NullSecondaryLocation_IsSkipped()
    {
        var compilation = CSharpCompilation.Create("test");
        var rule = new DiagnosticDescriptor("TEST001", "title", "message", "category", DiagnosticSeverity.Warning, true);
        var act = () =>
            IssueReporter.ReportIssueCore(
                compilation,
                _ => true,
                x => new DummyReportingContext(reporter, x, null),
                rule,
                Location.None,
                [null]);
        act.Should().NotThrow();
    }

    [TestMethod]
    public void ReportIssueCore_MinimumRoslynVersion() =>
        IssueReporter.MinimumDesignTimeRoslynVersion.Should().Be(defaultDesignTimeVersion);

    [TestMethod]
    public void ReportIssueCore_MinimumBuildTimeRoslynVersion() =>
        IssueReporter.MinimumBuildTimeRoslynVersion.Should().Be(defaultBuildTimeVersion);

    [TestMethod]
    [DataRow(@"C:\SonarSource\SomeFile.razor.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile.cshtml.g.cs")]
    public void ReportIssueCore_BuildTimeDiagnostic_NewRoslyn_KeepsGeneratedLocation(string filePath)
    {
        var diagnostic = ReportDiagnostic(filePath, hasMappedPath: true, minimumBuildTimeRoslynVersion: new Version(0, 0));

        reporter.Counter.Should().Be(1);
        reporter.LastDiagnostic.Should().BeSameAs(diagnostic);
    }

    [TestMethod]
    [DataRow(@"C:\SonarSource\SomeFile.razor.g.cs", "SomeFile.razor")]
    [DataRow(@"C:\SonarSource\SomeFile.cshtml.g.cs", "SomeFile.cshtml")]
    public void ReportIssueCore_BuildTimeDiagnostic_OldRoslyn_MapsLocation(string filePath, string expectedPath)
    {
        var diagnostic = ReportDiagnostic(filePath, hasMappedPath: true, minimumBuildTimeRoslynVersion: new Version(1000, 0), mappedPath: expectedPath);

        reporter.Counter.Should().Be(1);
        reporter.LastDiagnostic.Should().NotBeSameAs(diagnostic);
        reporter.LastDiagnostic.Location.SourceTree.Should().BeNull();
        Path.GetFileName(reporter.LastDiagnostic.Location.GetLineSpan().Path).Should().Be(expectedPath);
    }

    [TestMethod]
    [DataRow(@"C:\SonarSource\SomeFile.razor.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile.cshtml.g.cs")]
    public void ReportIssueCore_BuildTimeDiagnostic_WithoutMappedPath_IsNotReported(string filePath)
    {
        ReportDiagnostic(filePath, hasMappedPath: false);

        reporter.Counter.Should().Be(0);
        reporter.LastDiagnostic.Should().BeNull();
    }

    [TestMethod]
    [DataRow(null, false)]
    [DataRow("error", false)]
    [DataRow("error:", false)]
    [DataRow(" error:", true)]
    [DataRow("   error:", true)]
    [DataRow("   error", false)]
    [DataRow(" error :", true)]
    [DataRow(" ErRoR :", true)]
    [DataRow(" error   :", true)]
    [DataRow(" error   ", false)]
    [DataRow("error foo", false)]
    [DataRow("error foo:", false)]
    [DataRow(" error foo:", true)]
    [DataRow(" error foo :", true)]
    [DataRow(" error   foo:", true)]
    [DataRow(" error foo   :", true)]
    [DataRow(" errorfoo:", false)]
    [DataRow("   error foo:", true)]
    [DataRow("   eRrOr fOo:", true)]
    [DataRow("   error in foo:", false)]
    [DataRow("   error in foo :", false)]
    public void IsTextMatchingVbcErrorPattern_ReturnsExpected(string text, bool expectedResult) =>
        IssueReporter.IsTextMatchingVbcErrorPattern(text).Should().Be(expectedResult);

    [TestMethod]
    [DataRow("razor")]
    [DataRow("cshtml")]
    public void ReportIssueCore_Razor_PragmaWarning(string extension) =>
        CreateRazorBuilder(
            """
            @{
                void Method()
                {
                    _ = 42; // Noncompliant
                    #pragma warning disable SDummy
                    _ = 42;
                }
            }
            """,
            extension)
            .Verify();

    [TestMethod]
    [DataRow("razor")]
    [DataRow("cshtml")]
    public void ReportIssueCore_Razor_SuppressMessageAttribute(string extension) =>
        CreateRazorBuilder(
            """
            @{
                [System.Diagnostics.CodeAnalysis.SuppressMessage("DummyCategory", "SDummy")]
                void Suppressed()
                {
                    _ = 42;
                }

                void Reported()
                {
                    _ = 42; // Noncompliant
                }
            }
            """,
            extension)
            .Verify();

    [TestMethod]
    [DataRow("razor")]
    [DataRow("cshtml")]
    public void ReportIssueCore_Razor_OldRoslyn_MapsUnsuppressedDiagnostic(string extension)
    {
        var originalMinimumBuildTimeRoslynVersion = IssueReporter.MinimumBuildTimeRoslynVersion;
        IssueReporter.MinimumBuildTimeRoslynVersion = new Version(1000, 0);
        try
        {
            var builder = CreateRazorBuilder(
                """
                @{
                    _ = 42; // Noncompliant
                }
                """,
                extension);

            builder.Verify();

            var diagnostic = builder.Compile()
                .SelectMany(x => DiagnosticVerifier.AnalyzerDiagnostics(x, new DummyAnalyzerCS(), CompilationErrorBehavior.Default, builder.AdditionalFilePath))
                .Single(x => x.Id == "SDummy");
            diagnostic.Location.SourceTree.Should().BeNull("the forced legacy path maps the Razor location to an external location");
            Path.GetFileName(diagnostic.Location.GetLineSpan().Path).Should().Be($"SuppressDiagnostic.{extension}");
        }
        finally
        {
            IssueReporter.MinimumBuildTimeRoslynVersion = originalMinimumBuildTimeRoslynVersion;
        }
    }

    private VerifierBuilder CreateRazorBuilder(string snippet, string extension)
    {
        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "TestCases"));
        return DummyBuilder
            .AddSnippet(snippet, $"SuppressDiagnostic.{extension}")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .WithLanguageVersion(LanguageVersion.Latest);
    }

    private Diagnostic ReportDiagnostic(string filePath,
                                        bool hasMappedPath,
                                        string diagnosticId = "id",
                                        Version roslynVersion = null,
                                        Version minimumBuildTimeRoslynVersion = null,
                                        string mappedPath = "")
    {
        roslynVersion ??= defaultDesignTimeVersion;
        minimumBuildTimeRoslynVersion ??= defaultBuildTimeVersion;
        var tree = CreateTree(filePath);
        var diagnostic = CreateDiagnostic(diagnosticId, tree, hasMappedPath, mappedPath);

        IssueReporter.MinimumDesignTimeRoslynVersion = roslynVersion;
        IssueReporter.MinimumBuildTimeRoslynVersion = minimumBuildTimeRoslynVersion;
        try
        {
            IssueReporter.ReportIssueCore(
                x => true,
                x => new DummyReportingContext(reporter, x, tree),
                diagnostic);
        }
        finally
        {
            IssueReporter.MinimumDesignTimeRoslynVersion = defaultDesignTimeVersion;
            IssueReporter.MinimumBuildTimeRoslynVersion = defaultBuildTimeVersion;
        }

        return diagnostic;
    }

    private static SyntaxTree CreateTree(string filePath)
    {
        var tree = Substitute.For<SyntaxTree>();
        tree.FilePath.Returns(filePath);

        var root = Substitute.For<SyntaxNode>(null, null, 42);
        root.Language.Returns(LanguageNames.CSharp);
        tree.GetRoot().Returns(root);
        return tree;
    }

    private static Diagnostic CreateDiagnostic(string diagnosticId, SyntaxTree tree, bool hasMappedPath, string mappedPath)
    {
        var location = CreateLocation(tree, hasMappedPath, mappedPath);
        var descriptor = new DiagnosticDescriptor(diagnosticId, "title", "message", "category", DiagnosticSeverity.Warning, true);
        return Diagnostic.Create(descriptor, location);
    }

    private static Location CreateLocation(SyntaxTree tree, bool hasMappedPath, string mappedPath)
    {
        var location = Substitute.For<Location>();
        location.Kind.Returns(LocationKind.ExternalFile);
        location.SourceTree.Returns(tree);
        location.GetMappedLineSpan().Returns(CreatePosition(hasMappedPath, mappedPath));
        return location;
    }

    private static FileLinePositionSpan CreatePosition(bool hasMappedPath, string mappedPath)
    {
        // Unfortunately, this is the only way to set hasMappedPath to true for LinePositionSpan.
        // It's a struct, so it cannot be substituted.
        // And the property does not have a setter, so we have to use reflection at the constructor level.
        var ctor = typeof(FileLinePositionSpan).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [typeof(string), typeof(LinePositionSpan), typeof(bool)],
            null);

        return (FileLinePositionSpan)ctor.Invoke([mappedPath, default(LinePositionSpan), hasMappedPath]);
    }

    private class DummyReportingContext(DummyDiagnosticReporter reporter, Diagnostic diagnostic, SyntaxTree tree)
        : ReportingContext(diagnostic, reporter.ReportDiagnostic, null, tree);

    private class DummyDiagnosticReporter
    {
        public int Counter { get; private set; }
        public Diagnostic LastDiagnostic { get; private set; }

        public void ReportDiagnostic(Diagnostic diagnostic)
        {
            LastDiagnostic = diagnostic;
            Counter++;
        }
    }
}
