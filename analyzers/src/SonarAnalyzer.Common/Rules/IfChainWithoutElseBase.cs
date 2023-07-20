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

namespace SonarAnalyzer.Rules
{
    public abstract class IfChainWithoutElseBase<TSyntaxKind, TIfSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
        where TIfSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S126";

        protected abstract TSyntaxKind SyntaxKind { get; }
        protected abstract string ElseClause { get; }

        protected abstract bool IsElseIfWithoutElse(TIfSyntax ifSyntax);
        protected abstract Location IssueLocation(SonarSyntaxNodeReportingContext context, TIfSyntax ifSyntax);

        protected override string MessageFormat => "Add the missing '{0}' clause with either the appropriate action or a suitable comment as to why no action is taken.";

        protected IfChainWithoutElseBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer,
                c =>
                {
                    var ifNode = (TIfSyntax)c.Node;
                    if (!IsElseIfWithoutElse(ifNode))
                    {
                        return;
                    }

                    c.ReportIssue(CreateDiagnostic(Rule, IssueLocation(c, ifNode), ElseClause));
                },
                SyntaxKind);
    }
}
