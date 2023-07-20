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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseStringIsNullOrEmpty : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3256";
        private const string MessageFormat =
            "Use 'string.IsNullOrEmpty()' instead of comparing to empty string.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private const string EqualsName = nameof(string.Equals);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var invocationExpression = (InvocationExpressionSyntax)c.Node;

                    if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression
                        && memberAccessExpression.Name.Identifier.ValueText == EqualsName
                        && TryGetFirstArgument(invocationExpression, out var firstArgument)
                        && IsStringEqualsMethod(memberAccessExpression, c.SemanticModel))
                    {
                        // x.Equals(value), where x is string.Empty, "" or const "", and value is some string
                        if (IsStringIdentifier(firstArgument.Expression, c.SemanticModel)
                            && IsConstantEmptyString(memberAccessExpression.Expression, c.SemanticModel))
                        {
                            c.ReportIssue(CreateDiagnostic(rule, invocationExpression.GetLocation(), MessageFormat));
                            return;
                        }

                        // value.Equals(x), where x is string.Empty, "" or const "", and value is some string
                        if (IsStringIdentifier(memberAccessExpression.Expression, c.SemanticModel)
                            && IsConstantEmptyString(firstArgument.Expression, c.SemanticModel))
                        {
                            c.ReportIssue(CreateDiagnostic(rule, invocationExpression.GetLocation(), MessageFormat));
                        }
                    }
                },
                SyntaxKind.InvocationExpression);
        }

        private static bool TryGetFirstArgument(InvocationExpressionSyntax invocationExpression, out ArgumentSyntax firstArgument)
        {
            firstArgument = invocationExpression?.ArgumentList?.Arguments.FirstOrDefault();

            return firstArgument != null;
        }

        private static bool IsStringEqualsMethod(MemberAccessExpressionSyntax memberAccessExpression, SemanticModel semanticModel)
        {
            var methodName = semanticModel.GetSymbolInfo(memberAccessExpression.Name);

            return methodName.Symbol.IsInType(KnownType.System_String)
                && methodName.Symbol.Name == EqualsName;
        }

        private static bool IsStringIdentifier(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            if (!(expression is IdentifierNameSyntax identifierNameExpression))
            {
                return false;
            }

            var expressionType = semanticModel.GetTypeInfo(identifierNameExpression).Type;

            return expressionType != null && expressionType.Is(KnownType.System_String);
        }

        private static bool IsConstantEmptyString(ExpressionSyntax expression, SemanticModel semanticModel) =>
            IsStringEmptyLiteral(expression)
            || IsStringEmptyConst(expression, semanticModel)
            || expression.IsStringEmpty(semanticModel);

        private static bool IsStringEmptyConst(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var constValue = semanticModel.GetConstantValue(expression);
            return constValue.HasValue
                && constValue.Value is string stringConstValue && stringConstValue == string.Empty;
        }

        private static bool IsStringEmptyLiteral(ExpressionSyntax expression)
        {
            var literalExpression = expression as LiteralExpressionSyntax;
            return literalExpression?.Token.ValueText == string.Empty;
        }
    }
}
