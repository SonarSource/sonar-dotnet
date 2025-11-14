/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SwitchDefaultClauseEmpty : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3532";
        private const string MessageFormat = "Remove this empty 'default' clause.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var section = (SwitchSectionSyntax)c.Node;

                    if (!section.Labels.Any(SyntaxKind.DefaultSwitchLabel) ||
                        section.Statements.Count != 1)
                    {
                        return;
                    }

                    if (section.Statements[0].IsKind(SyntaxKind.BreakStatement) &&
                        !HasAnyComment(section))
                    {
                        c.ReportIssue(rule, section);
                    }
                },
                SyntaxKind.SwitchSection);
        }

        private static bool HasAnyComment(SwitchSectionSyntax section) =>
            section.Labels.Last()
                .GetTrailingTrivia()                                // handle comments after last label, which will normally be default:
                .Union(section.Statements[0].GetLeadingTrivia())    // handle comments before break
                .Union(section.Statements[0].GetTrailingTrivia())   // handle comments after break
                .Any(trivia =>
                    trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                    trivia.IsKind(SyntaxKind.MultiLineCommentTrivia));
    }
}
