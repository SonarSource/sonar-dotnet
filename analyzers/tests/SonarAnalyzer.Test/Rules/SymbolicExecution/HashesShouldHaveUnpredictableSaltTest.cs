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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.SymbolicExecution.Sonar.Analyzers;

using ChecksCS = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using ChecksVB = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.VisualBasic;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class HashesShouldHaveUnpredictableSaltTest
{
    private readonly VerifierBuilder sonar = new VerifierBuilder()
        .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg))
        .WithBasePath(@"SymbolicExecution\Sonar")
        .WithOnlyDiagnostics(HashesShouldHaveUnpredictableSalt.S2053)
        .AddReferences(MetadataReferenceFacade.SystemSecurityCryptography);

    private readonly VerifierBuilder roslynCS = new VerifierBuilder()
        .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))
        .WithBasePath(@"SymbolicExecution\Roslyn")
        .WithOnlyDiagnostics(ChecksCS.HashesShouldHaveUnpredictableSalt.S2053)
        .AddReferences(MetadataReferenceFacade.SystemSecurityCryptography);

    private readonly VerifierBuilder roslynVB = new VerifierBuilder<VB.SymbolicExecutionRunner>()
        .WithBasePath(@"SymbolicExecution\Roslyn")
        .WithOnlyDiagnostics(ChecksVB.HashesShouldHaveUnpredictableSalt.S2053)
        .AddReferences(MetadataReferenceFacade.SystemSecurityCryptography);

    [TestMethod]
    public void HashesShouldHaveUnpredictableSalt_Roslyn_CS() =>
        roslynCS.AddPaths("HashesShouldHaveUnpredictableSalt.cs").Verify();

    [TestMethod]
    public void HashesShouldHaveUnpredictableSalt_Roslyn_VB() =>
        roslynVB.AddPaths("HashesShouldHaveUnpredictableSalt.vb").Verify();

    [TestMethod]
    public void HashesShouldHaveUnpredictableSalt_Sonar_CS() =>
        sonar.AddPaths("HashesShouldHaveUnpredictableSalt.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .Verify();

    [TestMethod]
    public void HashesShouldHaveUnpredictableSalt_Sonar_DoesNotRaiseIssuesForTestProject() =>
        sonar.AddPaths("HashesShouldHaveUnpredictableSalt.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .AddTestReference()
            .VerifyNoIssues();

    [TestMethod]
    public void HashesShouldHaveUnpredictableSalt_Roslyn_DoesNotRaiseIssuesForTestProject() =>
        roslynCS.AddPaths("HashesShouldHaveUnpredictableSalt.cs")
            .AddTestReference()
            .VerifyNoIssues();

    [TestMethod]
    public void HashesShouldHaveUnpredictableSalt_Roslyn_CSharp8() =>
        roslynCS.AddPaths("HashesShouldHaveUnpredictableSalt.CSharp8.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .Verify();

#if NET

    [TestMethod]
    public void HashesShouldHaveUnpredictableSalt_Roslyn_CS_Latest() =>
        roslynCS.AddPaths("HashesShouldHaveUnpredictableSalt.Latest.cs")
            .WithOptions(ParseOptionsHelper.CSharpLatest)
            .WithTopLevelStatements()
            .Verify();

    [TestMethod]
    public void HashesShouldHaveUnpredictableSalt_Sonar_CS_Latest() =>
        sonar.AddPaths("HashesShouldHaveUnpredictableSalt.Latest.cs")
            .WithOptions(ParseOptionsHelper.CSharpLatest)
            .WithTopLevelStatements()
            .Verify();

#endif

    [TestMethod]
    public void HashesShouldHaveUnpredictableSalt_Sonar_LocationContext_Equals()
    {
        var tree = SyntaxFactory.ParseSyntaxTree("public class Test {}");
        var location = Location.Create(tree, TextSpan.FromBounds(0, 6));

        var context1 = new HashesShouldHaveUnpredictableSalt.LocationContext(location, "message");
        var context2 = new HashesShouldHaveUnpredictableSalt.LocationContext(location, "2nd message");
        var context3 = new HashesShouldHaveUnpredictableSalt.LocationContext(null, "message");
        var context4 = new HashesShouldHaveUnpredictableSalt.LocationContext(location, "message");

        context1.Equals(null).Should().BeFalse();
        context1.Equals(new object()).Should().BeFalse();
        context1.Equals(context2).Should().BeFalse();
        context1.Equals(context3).Should().BeFalse();
        context1.Equals(context1).Should().BeTrue();
        context1.Equals(context4).Should().BeTrue();
    }
}
