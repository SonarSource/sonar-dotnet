/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules
{
    public abstract class ShiftDynamicNotIntegerBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind> where TSyntaxKind : struct
    {
        private const string DiagnosticId = "S3449";

        protected abstract bool CanBeConvertedTo(SyntaxNode expression, ITypeSymbol type, SemanticModel semanticModel);

        protected abstract bool ShouldRaise(SemanticModel semanticModel, SyntaxNode left, SyntaxNode right);

        protected override string MessageFormat => "Remove this erroneous shift, it will fail because '{0}' can't be implicitly converted to 'int'.";

        protected ShiftDynamicNotIntegerBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                Language.GeneratedCodeRecognizer,
                c => CheckExpressionWithTwoParts(c, b => Language.Syntax.BinaryExpressionLeft(b), b => Language.Syntax.BinaryExpressionRight(b)),
                Language.SyntaxKind.LeftShiftExpression,
                Language.SyntaxKind.RightShiftExpression);

            context.RegisterNodeAction(
                Language.GeneratedCodeRecognizer,
                c => CheckExpressionWithTwoParts(c, b => Language.Syntax.AssignmentLeft(b), b => Language.Syntax.AssignmentRight(b)),
                Language.SyntaxKind.LeftShiftAssignmentStatement,
                Language.SyntaxKind.RightShiftAssignmentStatement);
        }

        protected void CheckExpressionWithTwoParts(SonarSyntaxNodeReportingContext context, Func<SyntaxNode, SyntaxNode> getLeft, Func<SyntaxNode, SyntaxNode> getRight)
        {
            var expression = context.Node;
            var left = getLeft(expression);
            var right = getRight(expression);

            if (!IsErrorType(right, context.SemanticModel, out var typeOfRight)
                && ShouldRaise(context.SemanticModel, left, right))
            {
                var typeInMessage = GetTypeNameForMessage(right, typeOfRight, context.SemanticModel);
                context.ReportIssue(CreateDiagnostic(Rule, right.GetLocation(), typeInMessage));
            }
        }

        private static string GetTypeNameForMessage(SyntaxNode expression, ITypeSymbol typeOfRight, SemanticModel semanticModel) =>
            semanticModel.GetConstantValue(expression) is { HasValue: true, Value: null }
            ? "null"
            : typeOfRight.ToMinimalDisplayString(semanticModel, expression.SpanStart);

        private static bool IsErrorType(SyntaxNode expression, SemanticModel semanticModel, out ITypeSymbol type)
        {
            type = semanticModel.GetTypeInfo(expression).Type;
            return type.Is(TypeKind.Error);
        }

        protected bool IsConvertibleToInt(SyntaxNode expression, SemanticModel semanticModel) =>
            semanticModel.Compilation.GetTypeByMetadataName(KnownType.System_Int32) is { } intType
            && CanBeConvertedTo(expression, intType, semanticModel);
    }
}
