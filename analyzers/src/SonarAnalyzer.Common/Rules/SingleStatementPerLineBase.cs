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

namespace SonarAnalyzer.Rules
{
    public abstract class SingleStatementPerLineBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S122";
        protected const string MessageFormat = "Reformat the code to have only one statement per line.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    }

    public abstract class SingleStatementPerLineBase<TStatementSyntax> : SingleStatementPerLineBase
        where TStatementSyntax : SyntaxNode
    {
        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterTreeAction(
                GeneratedCodeRecognizer,
                c =>
                {
                    var statements = c.Tree.GetRoot()
                        .DescendantNodesAndSelf()
                        .OfType<TStatementSyntax>()
                        .Where(st => !StatementShouldBeExcluded(st));

                    var statementsByLines = MultiValueDictionary<int, TStatementSyntax>.Create<HashSet<TStatementSyntax>>();
                    foreach (var statement in statements)
                    {
                        AddStatementToLineCache(statement, statementsByLines);
                    }

                    var lines = c.Tree.GetText().Lines;
                    foreach (var statementsByLine in statementsByLines.Where(pair => pair.Value.Count > 1))
                    {
                        var location = CalculateLocationForLine(lines[statementsByLine.Key], c.Tree, statementsByLine.Value);
                        c.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], location));
                    }
                });
        }
        protected abstract bool StatementShouldBeExcluded(TStatementSyntax statement);

        private static Location CalculateLocationForLine(TextLine line, SyntaxTree tree,
            ICollection<TStatementSyntax> statements)
        {
            var lineSpan = line.Span;

            var min = statements.Min(st => lineSpan.Intersection(st.Span).Value.Start);
            var max = statements.Max(st => lineSpan.Intersection(st.Span).Value.End);

            return Location.Create(tree, TextSpan.FromBounds(min, max));
        }

        private void AddStatementToLineCache(TStatementSyntax statement, MultiValueDictionary<int, TStatementSyntax> statementsByLines)
        {
            var startLine = statement.GetLocation().GetLineSpan().StartLinePosition.Line;
            statementsByLines.AddWithKey(startLine, statement);

            var lastToken = statement.GetLastToken();
            var tokenBelonsTo = GetContainingStatement(lastToken);
            if (tokenBelonsTo == statement)
            {
                var endLine = statement.GetLocation().GetLineSpan().EndLinePosition.Line;
                statementsByLines.AddWithKey(endLine, statement);
            }
        }

        private TStatementSyntax GetContainingStatement(SyntaxToken token)
        {
            var node = token.Parent;
            var statement = node as TStatementSyntax;
            while (node != null &&
                (statement == null || !StatementShouldBeExcluded(statement)))
            {
                node = node.Parent;
                statement = node as TStatementSyntax;
            }
            return statement;
        }
    }
}
