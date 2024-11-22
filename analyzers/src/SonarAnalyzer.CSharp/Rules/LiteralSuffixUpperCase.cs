/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
                static c =>
                {
                    var literal = (LiteralExpressionSyntax)c.Node;
                    var text = literal.Token.Text;

                    if (text[text.Length - 1] == 'l' && !ShouldIgnore(text))
                    {
                        c.ReportIssue(Rule, Location.Create(literal.SyntaxTree, new TextSpan(literal.Span.End - 1, 1)));
                    }
                },
                SyntaxKind.NumericLiteralExpression);

        // We know that @text is a number that ends with 'l'. Being a number, it has at least one digit (thus 2 characters).
        // If it has 3 characters or more, it could be `2ul` or `2Ul` and we ignore this, because 'l' is easier to read.
        private static bool ShouldIgnore(string text) =>
            text.Length > 2 && text[text.Length - 2] is 'U' or 'u';
    }
}
