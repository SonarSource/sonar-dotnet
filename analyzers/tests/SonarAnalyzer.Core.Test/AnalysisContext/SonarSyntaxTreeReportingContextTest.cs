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

using SonarAnalyzer.AnalysisContext;

namespace SonarAnalyzer.Core.Test.AnalysisContext;

[TestClass]
public class SonarSyntaxTreeReportingContextTest
{
    [TestMethod]
    public void Properties_ArePropagated()
    {
        var cancel = new CancellationToken(true);
        var (tree, model) = TestCompiler.CompileCS("// Nothing to see here");
        var options = AnalysisScaffolding.CreateOptions();
        var context = new SyntaxTreeAnalysisContext(tree, options, _ => { }, _ => true, cancel);
        var sut = new SonarSyntaxTreeReportingContext(AnalysisScaffolding.CreateSonarAnalysisContext(), context, model.Compilation);

        sut.Tree.Should().BeSameAs(tree);
        sut.Compilation.Should().BeSameAs(model.Compilation);
        sut.Options.Should().BeSameAs(options);
        sut.Cancel.Should().Be(cancel);
    }

    [TestMethod]
    public void ReportIssue_IgnoreSecondaryLocationOutsideCompilation()
    {
        Diagnostic lastDiagnostic = null;
        var (tree, model) = TestCompiler.CompileCS("using System;");
        var secondaryTree = TestCompiler.CompileCS("namespace Nothing;").Tree; // This tree is not in the analyzed compilation
        var context = new SyntaxTreeAnalysisContext(tree, AnalysisScaffolding.CreateOptions(), x => lastDiagnostic = x, _ => true, default);
        var sut = new SonarSyntaxTreeReportingContext(AnalysisScaffolding.CreateSonarAnalysisContext(), context, model.Compilation);
        var rule = AnalysisScaffolding.CreateDescriptor("Sxxxx", DiagnosticDescriptorFactory.MainSourceScopeTag);

        sut.ReportIssue(rule, tree.GetRoot(), [new SecondaryLocation(tree.GetRoot().GetLocation(), null)]);
        lastDiagnostic.Should().NotBeNull();
        lastDiagnostic.Id.Should().Be("Sxxxx");
        lastDiagnostic.AdditionalLocations.Should().ContainSingle("This secondary location is in compilation");

        sut.ReportIssue(rule, tree.GetRoot(), [new SecondaryLocation(secondaryTree.GetRoot().GetLocation(), null)]);
        lastDiagnostic.Should().NotBeNull();
        lastDiagnostic.Id.Should().Be("Sxxxx");
        lastDiagnostic.AdditionalLocations.Should().BeEmpty("This secondary location is outside the compilation");
    }

    [TestMethod]
    public void ReportIssue_NullArguments_Throws()
    {
        var compilation = TestCompiler.CompileCS("// Nothing to see here").Model.Compilation;
        var context = new SyntaxTreeAnalysisContext(compilation.SyntaxTrees.First(), new AnalyzerOptions([]), _ => { }, _ => true, CancellationToken.None);
        var sut = new SonarSyntaxTreeReportingContext(AnalysisScaffolding.CreateSonarAnalysisContext(), context, compilation);
        var rule = AnalysisScaffolding.CreateDescriptor("Sxxxx", DiagnosticDescriptorFactory.MainSourceScopeTag);

        sut.Invoking(x => x.ReportIssue(null, primaryLocation: null, secondaryLocations: [])).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("rule");
        sut.Invoking(x => x.ReportIssue(rule, primaryLocation: null, secondaryLocations: null)).Should().NotThrow();
        sut.Invoking(x => x.ReportIssue(rule, primaryLocation: null, secondaryLocations: [], properties: null)).Should().NotThrow();
    }

    [TestMethod]
    public void ReportIssue_PropertiesAndSecondaryLocations_Combine()
    {
        Diagnostic lastDiagnostic = null;
        var (tree, model) = TestCompiler.CompileCS("using System;");
        var context = new SyntaxTreeAnalysisContext(tree, AnalysisScaffolding.CreateOptions(), x => lastDiagnostic = x, _ => true, default);
        var sut = new SonarSyntaxTreeReportingContext(AnalysisScaffolding.CreateSonarAnalysisContext(), context, model.Compilation);
        var rule = AnalysisScaffolding.CreateDescriptor("Sxxxx", DiagnosticDescriptorFactory.MainSourceScopeTag);
        var secondaryLocation = new SecondaryLocation(tree.GetRoot().GetLocation(), "secondary");

        sut.ReportIssue(rule, tree.GetRoot().GetLocation(), [secondaryLocation], properties: new[] { "custom property"}.ToImmutableDictionary(x => x));
        lastDiagnostic.Should().NotBeNull();
        lastDiagnostic.Id.Should().Be("Sxxxx");
        lastDiagnostic.Properties.Should().HaveCount(2)
            .And.Contain(new KeyValuePair<string, string>("custom property", "custom property")).And.Contain(new KeyValuePair<string, string>("0", "secondary"));
        lastDiagnostic.AdditionalLocations.Should().ContainSingle("secondary");
    }
}
