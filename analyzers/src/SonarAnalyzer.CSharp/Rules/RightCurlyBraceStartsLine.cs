/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RightCurlyBraceStartsLine : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1109";
        private const string MessageFormat = "Move this closing curly brace to the next line.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterTreeAction(
                c =>
                {
                    var root = c.Tree.GetRoot();
                    foreach (var closeBraceToken in GetDescendantCloseBraceTokens(root)
                        .Where(closeBraceToken =>
                            !StartsLine(closeBraceToken) &&
                            !IsOnSameLineAsOpenBrace(closeBraceToken) &&
                            !IsInitializer(closeBraceToken.Parent)))
                    {
                        c.ReportIssue(rule, closeBraceToken);
                    }
                });
        }

        private static bool StartsLine(SyntaxToken token)
        {
            return token.GetPreviousToken().GetLocation().EndLine() != token.GetLocation().StartLine();
        }

        private static bool IsOnSameLineAsOpenBrace(SyntaxToken closeBraceToken)
        {
            var openBraceToken = closeBraceToken.Parent.ChildTokens().Single(token => token.IsKind(SyntaxKind.OpenBraceToken));
            return openBraceToken.GetLocation().StartLine() == closeBraceToken.GetLocation().StartLine();
        }

        private static bool IsInitializer(SyntaxNode node)
        {
            return node.IsKind(SyntaxKind.ArrayInitializerExpression) ||
                node.IsKind(SyntaxKind.CollectionInitializerExpression) ||
                node.IsKind(SyntaxKind.AnonymousObjectCreationExpression) ||
                node.IsKind(SyntaxKind.ObjectInitializerExpression);
        }

        private static IEnumerable<SyntaxToken> GetDescendantCloseBraceTokens(SyntaxNode node)
        {
            return node.DescendantTokens().Where(token => token.IsKind(SyntaxKind.CloseBraceToken));
        }
    }
}
