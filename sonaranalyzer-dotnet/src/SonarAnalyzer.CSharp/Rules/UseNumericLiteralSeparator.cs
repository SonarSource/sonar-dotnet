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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class UseNumericLiteralSeparator : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2148";
        private const string MessageFormat = "Add underscores to this numeric value for readability.";

        private const int BinaryMinimumDigits = 9 + 2; // +2 for 0b
        private const int HexadecimalMinimumDigits = 9 + 2; // +2 for 0x
        private const int DecimalMinimumDigits = 6;

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
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
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, numericLiteral.GetLocation()));
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
