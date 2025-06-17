/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.CSharp.Rules
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
                        && invocationExpression.ArgumentList?.Arguments.FirstOrDefault() is { } firstArgument
                        && memberAccessExpression.IsMemberAccessOnKnownType(EqualsName, KnownType.System_String, c.Model))
                    {
                        // x.Equals(value), where x is string.Empty, "" or const "", and value is some string
                        if (IsStringIdentifier(firstArgument.Expression, c.Model)
                            && IsConstantEmptyString(memberAccessExpression.Expression, c.Model))
                        {
                            c.ReportIssue(rule, invocationExpression, MessageFormat);
                            return;
                        }

                        // value.Equals(x), where x is string.Empty, "" or const "", and value is some string
                        if (IsStringIdentifier(memberAccessExpression.Expression, c.Model)
                            && IsConstantEmptyString(firstArgument.Expression, c.Model))
                        {
                            c.ReportIssue(rule, invocationExpression, MessageFormat);
                        }
                    }
                },
                SyntaxKind.InvocationExpression);
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
