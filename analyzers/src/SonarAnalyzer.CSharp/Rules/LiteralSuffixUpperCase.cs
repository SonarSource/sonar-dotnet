/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class LiteralSuffixUpperCase : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S818";
        private const string MessageFormat = "Upper case this literal suffix.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var literal = (LiteralExpressionSyntax)c.Node;
                    var text = literal.Token.Text;

                    if (text[text.Length - 1] == 'l' && !ShouldIgnore(text))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, Location.Create(literal.SyntaxTree, new TextSpan(literal.Span.End - 1, 1))));
                    }
                },
                SyntaxKind.NumericLiteralExpression);

        // We know that @text is a number that ends with 'l'. Being a number, it has at least one digit (thus 2 characters).
        // If it has 3 characters or more, it could be `2ul` or `2Ul` and we ignore this, because 'l' is easier to read.
        private static bool ShouldIgnore(string text) =>
            text.Length > 2 && char.ToUpperInvariant(text[text.Length - 2]) == 'U';
    }
}
