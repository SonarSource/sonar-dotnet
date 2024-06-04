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
public sealed class DelegateSubtraction : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S3172";
    private const string MessageFormat = "Review this subtraction of a chain of delegates: it may not work as you expect.";

    private static readonly DiagnosticDescriptor Rule =
        DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            c =>
            {
                var assignment = (AssignmentExpressionSyntax)c.Node;
                if (!ExpressionIsSimple(assignment.Right) && IsDelegateSubtraction(assignment, c.SemanticModel))
                {
                    c.ReportIssue(Rule, assignment);
                }
            },
            SyntaxKind.SubtractAssignmentExpression);

        context.RegisterNodeAction(
            c =>
            {
                var binary = (BinaryExpressionSyntax)c.Node;
                if (IsTopLevelSubtraction(binary)
                    && !BinaryIsValidSubstraction(binary)
                    && IsDelegateSubtraction(binary, c.SemanticModel))
                {
                    c.ReportIssue(Rule, binary);
                }
            },
            SyntaxKind.SubtractExpression);
    }

    private static bool BinaryIsValidSubstraction(BinaryExpressionSyntax subtraction)
    {
        var currentSubtraction = subtraction;

        while (currentSubtraction is not null && currentSubtraction.IsKind(SyntaxKind.SubtractExpression))
        {
            if (!ExpressionIsSimple(currentSubtraction.Right))
            {
                return false;
            }

            currentSubtraction = currentSubtraction.Left as BinaryExpressionSyntax;
        }
        return true;
    }

    private static bool IsTopLevelSubtraction(BinaryExpressionSyntax subtraction) =>
        subtraction.Parent is not BinaryExpressionSyntax parent || !parent.IsKind(SyntaxKind.SubtractExpression);

    private static bool IsDelegateSubtraction(SyntaxNode node, SemanticModel semanticModel) =>
        semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol subtractMethod
        && subtractMethod.ReceiverType.Is(TypeKind.Delegate);

    private static bool ExpressionIsSimple(ExpressionSyntax expression) =>
        expression.RemoveParentheses() is IdentifierNameSyntax or MemberAccessExpressionSyntax;
}
