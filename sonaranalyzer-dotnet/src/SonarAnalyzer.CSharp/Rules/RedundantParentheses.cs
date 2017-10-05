/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
    public class RedundantParentheses : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3235";
        private const string MessageFormat = "Remove these redundant parentheses.";
        private const IdeVisibility ideVisibility = IdeVisibility.Hidden;

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, ideVisibility, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var argumentList = (AttributeArgumentListSyntax)c.Node;
                    if (!argumentList.Arguments.Any())
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, argumentList.GetLocation()));
                    }
                },
                SyntaxKind.AttributeArgumentList);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var objectCreation = (ObjectCreationExpressionSyntax)c.Node;
                    var argumentList = objectCreation.ArgumentList;
                    if (argumentList != null &&
                        objectCreation.Initializer != null &&
                        !argumentList.Arguments.Any())
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, argumentList.GetLocation()));
                    }
                },
                SyntaxKind.ObjectCreationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var expression = (ParenthesizedExpressionSyntax)c.Node;

                    if (!(expression.Parent is ParenthesizedExpressionSyntax) &&
                        (expression.Expression is ParenthesizedExpressionSyntax))
                    {
                        var innermostExpression = GetSelfAndDescendantParenthesizedExpressions(expression)
                            .Reverse()
                            .Skip(1)
                            .First(); // There are always at least two parenthesized expressions

                        var location = Location.Create(expression.SyntaxTree,
                            GetSpan(expression.OpenParenToken, innermostExpression.OpenParenToken));

                        var secondaryLocation = Location.Create(expression.SyntaxTree,
                            GetSpan(innermostExpression.CloseParenToken, expression.CloseParenToken));

                        c.ReportDiagnosticWhenActive(
                            Diagnostic.Create(rule, location, additionalLocations: new[] { secondaryLocation }));
                    }
                },
                SyntaxKind.ParenthesizedExpression);
        }

        private static TextSpan GetSpan(SyntaxToken startToken, SyntaxToken endToken)
        {
            return TextSpan.FromBounds(startToken.Span.Start, endToken.Span.End);
        }

        private IEnumerable<ParenthesizedExpressionSyntax> GetSelfAndDescendantParenthesizedExpressions(ParenthesizedExpressionSyntax expression)
        {
            var descendant = expression;
            while (descendant != null)
            {
                yield return descendant;
                descendant = descendant.Expression as ParenthesizedExpressionSyntax;
            }
        }
    }
}
