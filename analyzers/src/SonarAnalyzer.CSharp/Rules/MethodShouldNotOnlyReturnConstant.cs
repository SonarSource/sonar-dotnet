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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MethodShouldNotOnlyReturnConstant : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S3400";
    private const string MessageFormat = "Remove this method and declare a constant for this value.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            c =>
            {
                var method = (MethodDeclarationSyntax)c.Node;
                if (method.ParameterList?.Parameters.Count is 0
                    && !IsVirtual(method)
                    && IsConstantExpression(SingleExpressionOrDefault(method), c.Model)
                    && !ContainsConditionalCompilation((SyntaxNode)method.ExpressionBody ?? method.Body)
                    && c.Model.GetDeclaredSymbol(method) is { } methodSymbol
                    && !methodSymbol.ContainingType.IsInterface()
                    && methodSymbol.InterfaceMembers().IsEmpty()
                    && methodSymbol.GetOverriddenMember() is null)
                {
                    c.ReportIssue(Rule, method.Identifier);
                }
            },
            SyntaxKind.MethodDeclaration);

    private static bool IsVirtual(BaseMethodDeclarationSyntax methodDeclaration) =>
        methodDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.VirtualKeyword));

    private static ExpressionSyntax SingleExpressionOrDefault(MethodDeclarationSyntax method) =>
        method switch
        {
            { ExpressionBody: { } body } => body.Expression,
            { Body.Statements: { Count: 1 } statements } when statements.Single() is ReturnStatementSyntax returnStatement => returnStatement.Expression,
            _ => null
        };

    private static bool IsConstantExpression(ExpressionSyntax expression, SemanticModel model) =>
        expression.RemoveParentheses() is LiteralExpressionSyntax literal
        && !literal.IsNullLiteral()
        && model.GetConstantValue(literal).HasValue;

    private static bool ContainsConditionalCompilation(SyntaxNode node) =>
        node.DescendantNodes(descendIntoTrivia: true).Any(x => x.IsKind(SyntaxKind.IfDirectiveTrivia));
}
