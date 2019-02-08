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

using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace SonarAnalyzer.Rules
{
    public abstract class RedundantParenthesesBase<TParenthesizedExpression, TSyntaxKind>
        : SonarDiagnosticAnalyzer
        where TParenthesizedExpression : SyntaxNode
        where TSyntaxKind : struct
    {
        internal const string DiagnosticId = "S1110";
        protected const string MessageFormat = "Remove these redundant parentheses.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
        protected abstract TSyntaxKind ParenthesizedExpressionSyntaxKind { get; }

        protected abstract SyntaxNode GetExpression(TParenthesizedExpression parenthesizedExpression);
        protected abstract SyntaxToken GetOpenParenToken(TParenthesizedExpression parenthesizedExpression);
        protected abstract SyntaxToken GetCloseParenToken(TParenthesizedExpression parenthesizedExpression);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    var expression = (TParenthesizedExpression)c.Node;

                    if (!(expression.Parent is TParenthesizedExpression) &&
                        (GetExpression(expression) is TParenthesizedExpression))
                    {
                        var innermostExpression = GetSelfAndDescendantParenthesizedExpressions(expression)
                            .Reverse()
                            .Skip(1)
                            .First(); // There are always at least two parenthesized expressions

                        var location = GetOpenParenToken(expression).CreateLocation(GetOpenParenToken(innermostExpression));

                        var secondaryLocation = GetCloseParenToken(innermostExpression).CreateLocation(GetCloseParenToken(expression));

                        c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0],
                            location, additionalLocations: new[] { secondaryLocation }));
                    }
                },
                ParenthesizedExpressionSyntaxKind);
        }

        protected IEnumerable<TParenthesizedExpression> GetSelfAndDescendantParenthesizedExpressions(TParenthesizedExpression expression)
        {
            var descendant = expression;
            while (descendant != null)
            {
                yield return descendant;
                descendant = GetExpression(descendant) as TParenthesizedExpression;
            }
        }
    }
}

