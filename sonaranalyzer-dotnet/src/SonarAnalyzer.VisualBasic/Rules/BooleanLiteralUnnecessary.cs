/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class BooleanLiteralUnnecessary : BooleanLiteralUnnecessaryBase<BinaryExpressionSyntax>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override bool IsBooleanLiteral(SyntaxNode node) => IsTrueLiteralKind(node) || IsFalseLiteralKind(node);

        protected override SyntaxNode GetLeftNode(BinaryExpressionSyntax binaryExpression) => binaryExpression.Left;

        protected override SyntaxNode GetRightNode(BinaryExpressionSyntax binaryExpression) => binaryExpression.Right;

        protected override SyntaxToken GetOperatorToken(BinaryExpressionSyntax binaryExpression) => binaryExpression.OperatorToken;

        protected override bool IsTrueLiteralKind(SyntaxNode syntaxNode) => syntaxNode.IsKind(SyntaxKind.TrueLiteralExpression);

        protected override bool IsFalseLiteralKind(SyntaxNode syntaxNode) => syntaxNode.IsKind(SyntaxKind.FalseLiteralExpression);

        protected override SyntaxNode RemoveParentheses(SyntaxNode syntaxNode) => syntaxNode.RemoveParentheses();

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckLogicalNot,
                SyntaxKind.NotExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckAndExpression,
                SyntaxKind.AndAlsoExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckAndExpression,
                SyntaxKind.AndExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckOrExpression,
                SyntaxKind.OrElseExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckOrExpression,
                SyntaxKind.OrExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckEquals,
                SyntaxKind.EqualsExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckNotEquals,
                SyntaxKind.NotEqualsExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckConditional,
                SyntaxKind.TernaryConditionalExpression);
        }

        private void CheckLogicalNot(SyntaxNodeAnalysisContext context)
        {
            var logicalNot = (UnaryExpressionSyntax)context.Node;
            var logicalNotOperand = logicalNot.Operand.RemoveParentheses();
            if (IsBooleanLiteral(logicalNotOperand))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, logicalNot.Operand.GetLocation()));
            }
        }

        private void CheckConditional(SyntaxNodeAnalysisContext context)
        {
            var conditional = (TernaryConditionalExpressionSyntax)context.Node;
            var whenTrue = conditional.WhenTrue;
            var whenFalse = conditional.WhenFalse;
            var typeLeft = context.SemanticModel.GetTypeInfo(whenTrue).Type;
            var typeRight = context.SemanticModel.GetTypeInfo(whenFalse).Type;
            if (typeLeft.IsNullableBoolean() || typeRight.IsNullableBoolean())
            {
                return;
            }
            CheckTernaryExpressionBranches(context, conditional.SyntaxTree, whenTrue, whenFalse);
        }
    }
}
