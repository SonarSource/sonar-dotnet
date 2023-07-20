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
    public abstract class UnconditionalJumpStatementBase<TStatementSyntax, TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TStatementSyntax : SyntaxNode
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S1751";

        protected abstract ISet<TSyntaxKind> LoopStatements { get; }

        protected abstract LoopWalkerBase<TStatementSyntax, TSyntaxKind> GetWalker(SonarSyntaxNodeReportingContext context);

        protected override string MessageFormat => "Refactor the containing loop to do more than one iteration.";

        protected UnconditionalJumpStatementBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer,
                c =>
                {
                    var walker = GetWalker(c);
                    walker.Visit();
                    foreach (var node in walker.GetRuleViolations())
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, node.GetLocation()));
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

        protected abstract ILanguageFacade<TLanguageKindEnum> Language { get; }
        protected abstract ISet<TLanguageKindEnum> ConditionalStatements { get; }
        protected abstract ISet<TLanguageKindEnum> StatementsThatCanThrow { get; }
        protected abstract ISet<TLanguageKindEnum> LambdaSyntaxes { get; }
        protected abstract ISet<TLanguageKindEnum> LocalFunctionSyntaxes { get; }

        public abstract void Visit();
        protected abstract bool TryGetTryAncestorStatements(TStatementSyntax node, List<SyntaxNode> ancestors, out IEnumerable<TStatementSyntax> tryAncestorStatements);
        protected abstract bool IsPropertyAccess(TStatementSyntax node);

        protected List<TStatementSyntax> ConditionalContinues { get; } = new List<TStatementSyntax>();
        protected List<TStatementSyntax> ConditionalTerminates { get; } = new List<TStatementSyntax>();

        protected List<TStatementSyntax> UnconditionalContinues { get; } = new List<TStatementSyntax>();
        protected List<TStatementSyntax> UnconditionalTerminates { get; } = new List<TStatementSyntax>();

        protected LoopWalkerBase(SonarSyntaxNodeReportingContext context, ISet<TLanguageKindEnum> loopStatements)
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

        protected void StoreVisitData(TStatementSyntax node, List<TStatementSyntax> conditionalCollection, List<TStatementSyntax> unconditionalCollection)
        {
            var ancestors = node
                .Ancestors()
                .TakeWhile(n => !rootExpression.Equals(n))
                .ToList();

            if (ancestors.Any(n => Language.Syntax.IsAnyKind(n, ignoredSyntaxesKind)))
            {
                return;
            }

            if (ancestors.Any(n => Language.Syntax.IsAnyKind(n, ConditionalStatements))
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

            if (Language.Syntax.IsKind(node, Language.SyntaxKind.ReturnStatement)
                && (node.DescendantNodes().Any(n => Language.Syntax.IsAnyKind(n, StatementsThatCanThrow))
                    || IsPropertyAccess(node)))
            {
                return true;
            }

            return tryAncestorStatements
                .TakeWhile(statement => !statement.Equals(node))
                .SelectMany(statement => statement.DescendantNodes())
                .Any(statement => Language.Syntax.IsAnyKind(statement, StatementsThatCanThrow));
        }
    }
}
