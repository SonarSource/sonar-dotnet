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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.AnalysisContext;
using SonarAnalyzer.Core.Syntax.Extensions;

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
            context.ReportIssue(rule, identifier.GetLocation(), invocation.ArgumentList.Arguments.Select(x => x.ToSecondaryLocation()));
        }
    }
}
