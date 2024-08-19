/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class UnaryPrefixOperatorRepeatedCodeFix : SonarCodeFix
    {
        internal const string Title = "Remove repeated prefix operator(s)";
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(UnaryPrefixOperatorRepeated.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            if (!(root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) is PrefixUnaryExpressionSyntax prefix))
            {
                return Task.CompletedTask;
            }

            context.RegisterCodeFix(
                Title,
                c =>
                {
                    GetExpression(prefix, out var expression, out var count);

                    if (count%2 == 1)
                    {
                        expression = SyntaxFactory.PrefixUnaryExpression(
                            prefix.Kind(),
                            expression);
                    }

                    var newRoot = root.ReplaceNode(prefix, expression
                        .WithAdditionalAnnotations(Formatter.Annotation));
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

            return Task.CompletedTask;
        }

        private static void GetExpression(PrefixUnaryExpressionSyntax prefix, out ExpressionSyntax expression, out uint count)
        {
            count = 0;
            var currentUnary = prefix;
            do
            {
                count++;
                expression = currentUnary.Operand;
                currentUnary = currentUnary.Operand as PrefixUnaryExpressionSyntax;
            }
            while (currentUnary != null);
        }
    }
}
