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

namespace SonarAnalyzer.Rules.CSharp;

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
                    && c.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
                    && methodSymbol.ContainingType.Is(KnownType.System_Diagnostics_Trace)
                    && UsesTraceSwitchAsCondition(c.SemanticModel, methodSymbol, invocation) is { } traceSwitchProperty)
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
