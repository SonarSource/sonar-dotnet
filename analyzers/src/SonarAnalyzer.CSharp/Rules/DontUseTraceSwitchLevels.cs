/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DontUseTraceSwitchLevels : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6675";
    private const string MessageFormat = "'Trace.{0}' should not be used with 'TraceSwitch' levels.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly ImmutableArray<string> TraceSwitchProperties = ImmutableArray.Create(
        "Level",
        "TraceError",
        "TraceInfo",
        "TraceVerbose",
        "TraceWarning");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var invocation = (InvocationExpressionSyntax)c.Node;
                if (invocation.GetName() is "WriteIf" or "WriteLineIf"
                    && invocation.ArgumentList.Arguments.Count > 1
                    && c.Model.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
                    && methodSymbol.ContainingType.Is(KnownType.System_Diagnostics_Trace)
                    && UsesTraceSwitchAsCondition(c.Model, methodSymbol, invocation) is { } traceSwitchProperty)
                {
                    c.ReportIssue(Rule, traceSwitchProperty, invocation.GetName());
                }
            },
            SyntaxKind.InvocationExpression);

    private static SyntaxNode UsesTraceSwitchAsCondition(SemanticModel model, IMethodSymbol methodSymbol, InvocationExpressionSyntax invocation)
    {
        var lookup = new CSharpMethodParameterLookup(invocation.ArgumentList, methodSymbol);
        lookup.TryGetSyntax("condition", out var expressions);
        var conditionArgument = expressions[0];
        return conditionArgument.DescendantNodesAndSelf().FirstOrDefault(x => x is MemberAccessExpressionSyntax memberAccess
            && TraceSwitchProperties.Contains(memberAccess.GetName())
            && model.GetTypeInfo(memberAccess.Expression).Type.DerivesFrom(KnownType.System_Diagnostics_TraceSwitch));
    }
}
