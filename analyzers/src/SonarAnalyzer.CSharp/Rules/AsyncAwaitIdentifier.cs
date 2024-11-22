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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AsyncAwaitIdentifier : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2306";
        private const string MessageFormat = "Rename '{0}' to not use a contextual keyword as an identifier.";

        private static readonly ISet<string> AsyncOrAwait = new HashSet<string> { "async", "await" };

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterTreeAction(
                c =>
                {
                    foreach (var asyncOrAwaitToken in GetAsyncOrAwaitTokens(c.Tree.GetRoot())
                        .Where(token => !token.Parent.AncestorsAndSelf().OfType<IdentifierNameSyntax>().Any()))
                    {
                        c.ReportIssue(rule, asyncOrAwaitToken, asyncOrAwaitToken.ToString());
                    }
                });
        }

        private static IEnumerable<SyntaxToken> GetAsyncOrAwaitTokens(SyntaxNode node)
        {
            return node.DescendantTokens()
                .Where(token =>
                    token.IsKind(SyntaxKind.IdentifierToken) &&
                    AsyncOrAwait.Contains(token.ToString()));
        }
    }
}
