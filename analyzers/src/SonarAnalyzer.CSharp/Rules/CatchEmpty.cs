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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CatchEmpty : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2486";
        private const string MessageFormat = "Handle the exception or explain in a comment why it can be ignored.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var catchClause = (CatchClauseSyntax)c.Node;

                    if (!HasStatements(catchClause) &&
                        !HasComments(catchClause) &&
                        IsGenericCatch(catchClause, c.SemanticModel))
                    {
                        c.ReportIssue(CreateDiagnostic(rule, c.Node.GetLocation()));
                    }
                },
                SyntaxKind.CatchClause);
        }

        private static bool IsGenericCatch(CatchClauseSyntax catchClause, SemanticModel semanticModel)
        {
            if (catchClause.Declaration == null)
            {
                return true;
            }

            if (catchClause.Filter != null)
            {
                return false;
            }

            var type = semanticModel.GetTypeInfo(catchClause.Declaration.Type).Type;
            return type.Is(KnownType.System_Exception);
        }

        private static bool HasComments(CatchClauseSyntax catchClause)
        {
            return catchClause.Block.OpenBraceToken.TrailingTrivia.Any(IsCommentTrivia) ||
                catchClause.Block.CloseBraceToken.LeadingTrivia.Any(IsCommentTrivia);
        }

        private static bool IsCommentTrivia(SyntaxTrivia trivia)
        {
            return trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) || trivia.IsKind(SyntaxKind.SingleLineCommentTrivia);
        }

        private static bool HasStatements(CatchClauseSyntax catchClause)
        {
            return catchClause.Block.Statements.Any();
        }
    }
}
