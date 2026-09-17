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

using Microsoft.CodeAnalysis.CSharp.Extensions;
using SonarAnalyzer.Core.Common;

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidDiscardingSafeVisitResult : StylingAnalyzer
{
    public AvoidDiscardingSafeVisitResult() : base("T0049", "Check the return value of 'SafeVisit': 'false' means the walk was aborted and the collected data can't be trusted.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                if (c.Node is InvocationExpressionSyntax invocation
                    && invocation.Expression.NameIs(nameof(ISafeSyntaxWalker.SafeVisit))
                    && IsResultDiscarded(c.Model, invocation)
                    && c.Model.GetSymbolInfo(invocation) is { Symbol: IMethodSymbol { ContainingType: { } container } }
                    && IsSafeSyntaxWalker(container))
                {
                    c.ReportIssue(Rule, invocation);
                }
            },
            SyntaxKind.InvocationExpression);

    private static bool IsResultDiscarded(SemanticModel model, ExpressionSyntax expression)
    {
        var effective = expression.GetParentConditionalAccessExpression() is { } conditional
            ? conditional.GetRootConditionalAccessExpression()
            : expression;
        return effective.Parent switch
        {
            ExpressionStatementSyntax => true,
            LambdaExpressionSyntax lambda => model.GetTypeInfo(lambda).ConvertedType is INamedTypeSymbol { DelegateInvokeMethod.ReturnsVoid: true },
            ArrowExpressionClauseSyntax arrow => model.GetDeclaredSymbol(arrow.Parent) is IMethodSymbol { ReturnsVoid: true },
            _ => false,
        };
    }

    private static bool IsSafeSyntaxWalker(ITypeSymbol type) =>
        type.Name == nameof(ISafeSyntaxWalker) || type.AllInterfaces.Any(x => x.Name == nameof(ISafeSyntaxWalker));
}
