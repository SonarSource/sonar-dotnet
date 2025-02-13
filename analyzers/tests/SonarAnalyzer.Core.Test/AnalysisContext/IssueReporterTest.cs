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

using System.Reflection;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Core.AnalysisContext.Test;

#pragma warning disable CS0618 // Type or member is obsolete
[TestClass]
public class IssueReporterTest
{
    private readonly Version defaultVersion = new Version("4.9.2");
    private readonly DummyDiagnosticReporter reporter = new();

    [DataTestMethod]
    [DataRow("S1481")]
    [DataRow("S927")]
    [DataRow("S4487")]
    [DataRow("S2696")]
    [DataRow("S2259")]
    [DataRow("S1144")]
    [DataRow("S2325")]
    [DataRow("S1117")]
    [DataRow("S1481")]
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
        Version current = new(typeof(SemanticModel).Assembly.GetName().Version.ToString());
        Version greaterThanCurrent = new(current.Major, current.Minor, current.Build + 1);

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
    public void ReportIssueCore_MinimumRoslyVersion_Is_4_9_2() =>
        Assert.AreEqual(defaultVersion, IssueReporter.GetMinimumDesignTimeRoslynVersion());

    [DataTestMethod]
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

    private Diagnostic ReportDiagnostic(string filePath, bool hasMappedPath, string diagnosticId = "id", Version roslynVersion = null)
    {
        roslynVersion ??= defaultVersion;
        var tree = GetTree(filePath);
        var diagnostic = GetDiagnostic(diagnosticId, tree, hasMappedPath);

        IssueReporter.SetMinimumDesignTimeRoslynVersion(roslynVersion);

        IssueReporter.ReportIssueCore(
            x => true,
            x => new DummyReportingContext(reporter, x, tree),
            diagnostic);

        IssueReporter.SetMinimumDesignTimeRoslynVersion(defaultVersion);

        return diagnostic;
    }

    private static SyntaxTree GetTree(string filePath)
    {
        var tree = Substitute.For<SyntaxTree>();
        tree.FilePath.Returns(filePath);

        var root = Substitute.For<SyntaxNode>(null, null, 42);
        root.Language.Returns(LanguageNames.CSharp);
        tree.GetRoot().Returns(root);
        return tree;
    }

    private static Diagnostic GetDiagnostic(string diagnosticId, SyntaxTree tree, bool hasMappedPath)
    {
        var location = GetLocation(tree, hasMappedPath);
        var descriptor = new DiagnosticDescriptor(diagnosticId, "title", "message", "category", DiagnosticSeverity.Warning, true);
        return Diagnostic.Create(descriptor, location);
    }

    private static Location GetLocation(SyntaxTree tree, bool hasMappedPath)
    {
        var location = Substitute.For<Location>();
        location.Kind.Returns(LocationKind.ExternalFile);
        location.SourceTree.Returns(tree);
        location.GetMappedLineSpan().Returns(GetPosition(hasMappedPath));
        return location;
    }

    private static FileLinePositionSpan GetPosition(bool hasMappedPath)
    {
        // Unfortunately, this is the only way to set hasMappedPath to true for LinePositionSpan.
        // It's a struct, so it cannot be substituted.
        // And the property does not have a setter, so we have to use reflection at the constructor level.
        var ctor = typeof(FileLinePositionSpan).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [typeof(string), typeof(LinePositionSpan), typeof(bool)],
            null);

        return (FileLinePositionSpan)ctor.Invoke([string.Empty, default(LinePositionSpan), hasMappedPath]);
    }

    private class DummyReportingContext(DummyDiagnosticReporter reporter, Diagnostic diagnostic, SyntaxTree tree)
        : ReportingContext(diagnostic, reporter.ReportDiagnostic, null, tree);

    private class DummyDiagnosticReporter
    {
        public int Counter { get; private set; } = 0;
        public Diagnostic LastDiagnostic { get; private set; }

        public void ReportDiagnostic(Diagnostic diagnostic)
        {
            LastDiagnostic = diagnostic;
            Counter++;
        }
    }
}
