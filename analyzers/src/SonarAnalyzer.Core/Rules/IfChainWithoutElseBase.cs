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

namespace SonarAnalyzer.Core.Rules
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

                    c.ReportIssue(Rule, IssueLocation(c, ifNode), ElseClause);
                },
                SyntaxKind);
    }
}
