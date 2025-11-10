/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.VisualBasic.Rules
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class CommentLineEnd : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S139";
        private const string MessageFormat = "Move this trailing comment on the previous empty line.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private const string DefaultPattern = @"^'\s*\S+\s*$";

        [RuleParameter("legalCommentPattern", PropertyType.String,
            "Pattern for text of trailing comments that are allowed.", DefaultPattern)]
        public string LegalCommentPattern { get; set; } = DefaultPattern;

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterTreeAction(
                c =>
                {
                    foreach (var token in c.Tree.GetRoot().DescendantTokens())
                    {
                        CheckTokenComments(c, token);
                    }
                });
        }

        private void CheckTokenComments(SonarSyntaxTreeReportingContext context, SyntaxToken token)
        {
            var tokenLine = token.GetLocation().StartLine();

            var comments = token.TrailingTrivia
                .Where(tr => tr.IsKind(SyntaxKind.CommentTrivia));

            foreach (var comment in comments)
            {
                var location = comment.GetLocation();
                if (location.StartLine() == tokenLine && !SafeRegex.IsMatch(comment.ToString(), LegalCommentPattern))
                {
                    context.ReportIssue(rule, location);
                }
            }
        }
    }
}
