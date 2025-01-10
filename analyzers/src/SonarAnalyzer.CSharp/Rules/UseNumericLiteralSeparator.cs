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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseNumericLiteralSeparator : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2148";
        private const string MessageFormat = "Add underscores to this numeric value for readability.";

        private const int BinaryMinimumDigits = 9 + 2; // +2 for 0b
        private const int HexadecimalMinimumDigits = 9 + 2; // +2 for 0x
        private const int DecimalMinimumDigits = 6;

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    if (!c.Compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp7))
                    {
                        return;
                    }

                    var numericLiteral = (LiteralExpressionSyntax)c.Node;

                    if (numericLiteral.Token.Text.IndexOf('_') < 0 &&
                        ShouldHaveSeparator(numericLiteral))
                    {
                        c.ReportIssue(rule, numericLiteral);
                    }
                },
                SyntaxKind.NumericLiteralExpression);
        }

        private static bool ShouldHaveSeparator(LiteralExpressionSyntax numericLiteral)
        {
            if (numericLiteral.Token.Text.StartsWith("0x"))
            {
                return numericLiteral.Token.Text.Length > HexadecimalMinimumDigits;
            }

            if (numericLiteral.Token.Text.StartsWith("0b"))
            {
                return numericLiteral.Token.Text.Length > BinaryMinimumDigits;
            }

            var indexOfDot = numericLiteral.Token.Text.IndexOf('.');
            var beforeDotPartLength = indexOfDot < 0
                ? numericLiteral.Token.Text.Length
                : numericLiteral.Token.Text.Remove(indexOfDot).Length;

            return beforeDotPartLength > DecimalMinimumDigits;
        }
    }
}
