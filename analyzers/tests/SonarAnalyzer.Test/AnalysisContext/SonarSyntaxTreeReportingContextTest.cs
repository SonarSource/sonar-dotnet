﻿/*
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

using System.Threading.Channels;
using NSubstitute;
using SonarAnalyzer.AnalysisContext;

namespace SonarAnalyzer.Test.AnalysisContext;

[TestClass]
public class SonarSyntaxTreeReportingContextTest
{
    [TestMethod]
    public void Constructor_Null_Throws()
    {
        var (tree, model) = TestHelper.CompileCS("// Nothing to see here");
        var treeContext = new SyntaxTreeAnalysisContext(tree, null, _ => { }, _ => true, default);
        var analysisContext = AnalysisScaffolding.CreateSonarAnalysisContext();

        ((Func<SonarSyntaxTreeReportingContext>)(() => new(null, treeContext, model.Compilation))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("analysisContext");
        ((Func<SonarSyntaxTreeReportingContext>)(() => new(analysisContext, treeContext, null))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("compilation");
    }

    [TestMethod]
    public void Properties_ArePropagated()
    {
        var cancel = new CancellationToken(true);
        var (tree, model) = TestHelper.CompileCS("// Nothing to see here");
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
        var (tree, model) = TestHelper.CompileCS("using System;");
        var secondaryTree = TestHelper.CompileCS("namespace Nothing;").Tree; // This tree is not in the analyzed compilation
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
        var compilation = TestHelper.CompileCS("// Nothing to see here").Model.Compilation;
        var sut = new SonarSyntaxTreeReportingContext(AnalysisScaffolding.CreateSonarAnalysisContext(), default, compilation);
        var rule = AnalysisScaffolding.CreateDescriptor("Sxxxx", DiagnosticDescriptorFactory.MainSourceScopeTag);

        sut.Invoking(x => x.ReportIssue(null, primaryLocation: null,  secondaryLocations: [])).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("rule");
        sut.Invoking(x => x.ReportIssue(rule, primaryLocation: null,  secondaryLocations: null)).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("secondaryLocations");
    }
}
