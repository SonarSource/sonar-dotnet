/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class UnconditionalJumpStatementBase<TStatementSyntax, TLanguageKindEnum> : SonarDiagnosticAnalyzer
        where TStatementSyntax : SyntaxNode
        where TLanguageKindEnum : struct
    {
        protected const string DiagnosticId = "S1751";
        protected const string MessageFormat = "Refactor the containing loop to do more than one iteration.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        protected abstract LoopWalkerBase<TStatementSyntax, TLanguageKindEnum> GetWalker(SyntaxNodeAnalysisContext context);

        protected abstract ISet<TLanguageKindEnum> LoopStatements { get; }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(GeneratedCodeRecognizer,
                c =>
                {
                    var walker = GetWalker(c);
                    walker.Visit();
                    foreach (var node in walker.GetRuleViolations())
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], node.GetLocation()));
                    }
                },
                LoopStatements.ToArray());
        }
    }

    public abstract class LoopWalkerBase<TStatementSyntax, TLanguageKindEnum>
        where TStatementSyntax : SyntaxNode
        where TLanguageKindEnum : struct
    {
        protected readonly SyntaxNode rootExpression;

        protected List<TStatementSyntax> ConditionalContinues { get; } = new List<TStatementSyntax>();
        protected List<TStatementSyntax> ConditionalTerminates { get; } = new List<TStatementSyntax>();

        protected List<TStatementSyntax> UnconditionalContinues { get; } = new List<TStatementSyntax>();
        protected List<TStatementSyntax> UnconditionalTerminates { get; } = new List<TStatementSyntax>();

        protected abstract ISet<TLanguageKindEnum> ConditionalStatements { get; }
        protected abstract ISet<TLanguageKindEnum> StatementsThatCanThrow { get; }
        protected abstract ISet<TLanguageKindEnum> LambdaSyntaxes { get; }

        private readonly ISet<TLanguageKindEnum> lambdaOrLoopStatements;

        protected LoopWalkerBase(SyntaxNodeAnalysisContext context, ISet<TLanguageKindEnum> loopStatements)
        {
            this.rootExpression = context.Node;
            this.lambdaOrLoopStatements = LambdaSyntaxes.Union(loopStatements).ToHashSet();
        }

        public abstract void Visit();

        public List<TStatementSyntax> GetRuleViolations()
        {
            var ruleViolations = new List<TStatementSyntax>(UnconditionalContinues);
            if (!ConditionalContinues.Any())
            {
                ruleViolations.AddRange(UnconditionalTerminates);
            }

            return ruleViolations;
        }

        protected void StoreVisitData(TStatementSyntax node, List<TStatementSyntax> conditionalCollection,
            List<TStatementSyntax> unconditionalCollection)
        {
            var ancestors = node
                .Ancestors()
                .TakeWhile(n => !this.rootExpression.Equals(n))
                .ToList();

            if (ancestors.Any(n => IsAnyKind(n, this.lambdaOrLoopStatements)))
            {
                return;
            }

            if (ancestors.Any(n => IsAnyKind(n, ConditionalStatements)) ||
                IsInTryCatchWithMethodInvocation(node, ancestors))
            {
                conditionalCollection.Add(node);
            }
            else
            {
                unconditionalCollection.Add(node);
            }
        }

        protected abstract bool IsAnyKind(SyntaxNode node, ISet<TLanguageKindEnum> syntaxKinds);

        protected abstract bool IsReturnStatement(SyntaxNode node);

        protected abstract bool TryGetTryAncestorStatements(TStatementSyntax node, List<SyntaxNode> ancestors,
            out IEnumerable<TStatementSyntax> tryAncestorStatements);

        private bool IsInTryCatchWithMethodInvocation(TStatementSyntax node, List<SyntaxNode> ancestors)
        {
            if (!TryGetTryAncestorStatements(node, ancestors, out var tryAncestorStatements))
            {
                return false;
            }

            if (IsReturnStatement(node) &&
                node.DescendantNodes().Any(n => IsAnyKind(n, StatementsThatCanThrow)))
            {
                return true;
            }

            return tryAncestorStatements
                .TakeWhile(statement => !statement.Equals(node))
                .SelectMany(statement => statement.DescendantNodes())
                .Any(statement => IsAnyKind(statement, StatementsThatCanThrow));
        }
    }
}
