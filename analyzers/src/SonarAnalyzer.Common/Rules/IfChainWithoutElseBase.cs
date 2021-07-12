/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class IfChainWithoutElseBase<TSyntaxKind, TIfSyntax> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TIfSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S126";
        private const string MessageFormat = "Add the missing '{0}' clause with either the appropriate action or a suitable comment as to why no action is taken.";
        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
        protected abstract TSyntaxKind SyntaxKind { get; }
        protected abstract string ElseClause { get; }

        protected abstract bool IsElseIfWithoutElse(TIfSyntax ifSyntax);
        protected abstract Location IssueLocation(SyntaxNodeAnalysisContext context, TIfSyntax ifSyntax);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected IfChainWithoutElseBase() =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, Language.RspecResources);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer,
                c =>
                {
                    var ifNode = (TIfSyntax)c.Node;
                    if (!IsElseIfWithoutElse(ifNode))
                    {
                        return;
                    }

                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, IssueLocation(c, ifNode), ElseClause));
                },
                SyntaxKind);
    }
}
