/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class UnaryPrefixOperatorRepeated : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2761";
        private const string MessageFormat = "Use the '{0}' operator just once or not at all.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var topLevelUnary = (PrefixUnaryExpressionSyntax)c.Node;

                    if (!TopLevelUnaryInChain(topLevelUnary))
                    {
                        return;
                    }

                    var repeatedCount = 0U;
                    var currentUnary = topLevelUnary;
                    var lastUnary = currentUnary;
                    while (currentUnary != null &&
                           SameOperators(currentUnary, topLevelUnary))
                    {
                        lastUnary = currentUnary;
                        repeatedCount++;
                        currentUnary = currentUnary.Operand as PrefixUnaryExpressionSyntax;
                    }

                    if (repeatedCount < 2)
                    {
                        return;
                    }

                    var errorLocation = new TextSpan(topLevelUnary.SpanStart, lastUnary.OperatorToken.Span.End - topLevelUnary.SpanStart);
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule,
                            Location.Create(c.Node.SyntaxTree, errorLocation),
                            topLevelUnary.OperatorToken.ToString()));
                },
                SyntaxKind.LogicalNotExpression,
                SyntaxKind.BitwiseNotExpression);
        }

        private static bool TopLevelUnaryInChain(PrefixUnaryExpressionSyntax unary)
        {
            return !(unary.Parent is PrefixUnaryExpressionSyntax parent) || !SameOperators(parent, unary);
        }

        private static bool SameOperators(PrefixUnaryExpressionSyntax expression1, PrefixUnaryExpressionSyntax expression2)
        {
            return expression1.OperatorToken.IsKind(expression2.OperatorToken.Kind());
        }
    }
}
