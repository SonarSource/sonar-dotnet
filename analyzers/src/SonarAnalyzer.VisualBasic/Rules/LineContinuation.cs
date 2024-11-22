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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class LineContinuation : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2354";
        private const string MessageFormat = "Reformat the code to remove this use of the line continuation character.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterTreeAction(
                c =>
                {
                    var lineContinuations = c.Tree.GetRoot().DescendantTokens()
                        .SelectMany(token => token.TrailingTrivia)
                        .Where(trivia => trivia.IsKind(SyntaxKind.LineContinuationTrivia));

                    foreach (var lineContinuation in lineContinuations)
                    {
                        c.ReportIssue(rule, lineContinuation.GetLocation());
                    }
                });
        }
    }
}
