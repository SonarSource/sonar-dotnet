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

using System;
using System.Collections.Immutable;
using System.Linq;
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
    public sealed class NumberPatternShouldBeRegular : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3937";
        private const string MessageFormat = "Review this number; its irregular pattern indicates an error.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ImmutableHashSet<char> NumericTypeSuffix =
            ImmutableHashSet.Create('L', 'D', 'F', 'U', 'M');

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    if (!c.Compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp7))
                    {
                        return;
                    }

                    var literal = (LiteralExpressionSyntax)c.Node;

                    var numberWithoutSuffix = ClearNumberTypeSuffix(literal.Token.Text);

                    var decimalParts = numberWithoutSuffix.Split('.');
                    if (decimalParts.Length > 2)
                    {
                        return;
                    }

                    var hasIrregularPattern = decimalParts
                        .SelectMany(part => part.Split('_'))
                        .Select(x => x.Length)
                        .Skip(1) // skip the first part (1_234 => 234)
                        .Reverse().Skip(decimalParts.Length == 2 ? 1 : 0) // skip the last if there is a decimal (.234_5 => 234)
                        .Distinct().Skip(1).Any(); // we expect to have only 1 size of pattern

                    if (hasIrregularPattern)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, literal.GetLocation()));
                    }
                },
                SyntaxKind.NumericLiteralExpression);
        }

        private static string ClearNumberTypeSuffix(string numberLiteral)
        {
            if (numberLiteral.EndsWith("UL", StringComparison.OrdinalIgnoreCase))
            {
                return numberLiteral.Substring(0, numberLiteral.Length - 2);
            }

            if (NumericTypeSuffix.Contains(char.ToUpperInvariant(numberLiteral[numberLiteral.Length - 1])))
            {
                return numberLiteral.Substring(0, numberLiteral.Length - 1);
            }

            return numberLiteral;
        }
    }
}
