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
    public sealed class DontMixIncrementOrDecrementWithOtherOperators : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S881";
        private const string MessageFormat = "Extract this {0} operation into a dedicated statement.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<SyntaxKind> arithmeticOperator =
            new HashSet<SyntaxKind>
            {
                SyntaxKind.AddExpression,
                SyntaxKind.SubtractExpression,
                SyntaxKind.MultiplyExpression,
                SyntaxKind.DivideExpression,
                SyntaxKind.ModuloExpression
            };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var operatorToken = (c.Node as PrefixUnaryExpressionSyntax)?.OperatorToken
                            ?? (c.Node as PostfixUnaryExpressionSyntax)?.OperatorToken;

                    if (operatorToken != null &&
                        c.Node.Ancestors().FirstOrDefault(node => node.IsAnyKind(arithmeticOperator)) is BinaryExpressionSyntax)
                    {
                        c.ReportIssue(rule, operatorToken.Value, operatorToken.Value.IsKind(SyntaxKind.PlusPlusToken) ? "increment" : "decrement");
                    }
                },
                SyntaxKind.PreDecrementExpression,
                SyntaxKind.PreIncrementExpression,
                SyntaxKind.PostDecrementExpression,
                SyntaxKind.PostIncrementExpression);
        }
    }
}
