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
                        c.ReportIssue(CreateDiagnostic(rule, lineContinuation.GetLocation()));
                    }
                });
        }
    }
}
