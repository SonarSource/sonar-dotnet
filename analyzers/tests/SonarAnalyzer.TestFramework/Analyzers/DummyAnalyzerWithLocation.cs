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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.AnalysisContext;

namespace SonarAnalyzer.TestFramework.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DummyAnalyzerWithLocation : SonarDiagnosticAnalyzer
{
    private readonly DiagnosticDescriptor rule;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

    public DummyAnalyzerWithLocation() : this("DummyWithLocation", DiagnosticDescriptorFactory.MainSourceScopeTag) { }

    public DummyAnalyzerWithLocation(string id, params string[] customTags) =>
        rule = AnalysisScaffolding.CreateDescriptor(id, customTags);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(TestGeneratedCodeRecognizer.Instance, ReportIssue, SyntaxKind.InvocationExpression);

    private void ReportIssue(SonarSyntaxNodeReportingContext context)
    {
        if (context.Node is InvocationExpressionSyntax invocation
            && invocation.Expression is IdentifierNameSyntax { Identifier.ValueText: "RaiseHere" } identifier)
        {
            context.ReportIssue(rule, identifier.GetLocation(), invocation.ArgumentList.Arguments.Select(arg => arg.ToSecondaryLocation()));
        }
    }
}
