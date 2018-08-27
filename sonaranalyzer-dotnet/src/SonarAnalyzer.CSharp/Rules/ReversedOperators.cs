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
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ReversedOperators : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2757";
        private const string MessageFormat = "Was '{0}' meant instead?";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var unaryExpression = (PrefixUnaryExpressionSyntax) c.Node;

                    var operatorToken = unaryExpression.OperatorToken;
                    var previousToken = operatorToken.GetPreviousToken();
                    var nextToken = operatorToken.GetNextToken();

                    var operatorLocation = operatorToken.GetLocation();

                    var operatorSpan = operatorLocation.GetLineSpan();
                    var previousSpan = previousToken.GetLocation().GetLineSpan();
                    var nextSpan = nextToken.GetLocation().GetLineSpan();

                    if (previousToken.IsKind(SyntaxKind.EqualsToken) &&
                        TiedTogether(previousSpan, operatorSpan) &&
                        !(operatorToken.IsKind(SyntaxKind.MinusToken) && TiedTogether(operatorSpan, nextSpan)))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, operatorLocation, $"{operatorToken.Text}="));
                    }
                },
                SyntaxKind.UnaryMinusExpression,
                SyntaxKind.UnaryPlusExpression,
                SyntaxKind.LogicalNotExpression);
        }

        private static bool TiedTogether(FileLinePositionSpan span, FileLinePositionSpan nextSpan)
        {
            return span.EndLinePosition == nextSpan.StartLinePosition;
        }
    }
}
