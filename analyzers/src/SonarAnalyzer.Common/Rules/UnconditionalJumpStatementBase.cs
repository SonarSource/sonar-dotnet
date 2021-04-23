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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class UnconditionalJumpStatementBase<TStatementSyntax, TSyntaxKind> : SonarDiagnosticAnalyzer
        where TStatementSyntax : SyntaxNode
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S1751";
        private const string MessageFormat = "Refactor the containing loop to do more than one iteration.";

        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        protected abstract LoopWalkerBase<TStatementSyntax, TSyntaxKind> GetWalker(SyntaxNodeAnalysisContext context);

        protected abstract ISet<TSyntaxKind> LoopStatements { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected UnconditionalJumpStatementBase() =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, Language.RspecResources);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer,
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

    public abstract class LoopWalkerBase<TStatementSyntax, TLanguageKindEnum>
        where TStatementSyntax : SyntaxNode
        where TLanguageKindEnum : struct
    {
        protected readonly SyntaxNode rootExpression;
        protected readonly SemanticModel semanticModel;

        private readonly ISet<TLanguageKindEnum> ignoredSyntaxesKind;

        protected abstract ISet<TLanguageKindEnum> ConditionalStatements { get; }
        protected abstract ISet<TLanguageKindEnum> StatementsThatCanThrow { get; }
        protected abstract ISet<TLanguageKindEnum> LambdaSyntaxes { get; }
        protected abstract ISet<TLanguageKindEnum> LocalFunctionSyntaxes { get; }

        public abstract void Visit();
        protected abstract bool IsAnyKind(SyntaxNode node, ISet<TLanguageKindEnum> syntaxKinds);
        protected abstract bool IsReturnStatement(SyntaxNode node);
        protected abstract bool TryGetTryAncestorStatements(TStatementSyntax node, List<SyntaxNode> ancestors, out IEnumerable<TStatementSyntax> tryAncestorStatements);
        protected abstract bool IsAccessToClassMember(TStatementSyntax node);

        protected List<TStatementSyntax> ConditionalContinues { get; } = new List<TStatementSyntax>();
        protected List<TStatementSyntax> ConditionalTerminates { get; } = new List<TStatementSyntax>();

        protected List<TStatementSyntax> UnconditionalContinues { get; } = new List<TStatementSyntax>();
        protected List<TStatementSyntax> UnconditionalTerminates { get; } = new List<TStatementSyntax>();

        protected LoopWalkerBase(SyntaxNodeAnalysisContext context, ISet<TLanguageKindEnum> loopStatements)
        {
            rootExpression = context.Node;
            semanticModel = context.SemanticModel;
            ignoredSyntaxesKind = LambdaSyntaxes.Union(LocalFunctionSyntaxes).Union(loopStatements).ToHashSet();
        }

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
                .TakeWhile(n => !rootExpression.Equals(n))
                .ToList();

            if (ancestors.Any(n => IsAnyKind(n, ignoredSyntaxesKind)))
            {
                return;
            }

            if (ancestors.Any(n => IsAnyKind(n, ConditionalStatements))
                || IsInTryCatchWithStatementThatCanThrow(node, ancestors))
            {
                conditionalCollection.Add(node);
            }
            else
            {
                unconditionalCollection.Add(node);
            }
        }

        private bool IsInTryCatchWithStatementThatCanThrow(TStatementSyntax node, List<SyntaxNode> ancestors)
        {
            if (!TryGetTryAncestorStatements(node, ancestors, out var tryAncestorStatements))
            {
                return false;
            }

            if (IsReturnStatement(node)
                && (node.DescendantNodes().Any(n => IsAnyKind(n, StatementsThatCanThrow))
                    || IsAccessToClassMember(node)))
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
