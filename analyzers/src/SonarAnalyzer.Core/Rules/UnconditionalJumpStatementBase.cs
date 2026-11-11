/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Rules;

public abstract class UnconditionalJumpStatementBase<TStatementSyntax, TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TStatementSyntax : SyntaxNode
    where TSyntaxKind : struct
{
    protected const string DiagnosticId = "S1751";

    protected abstract ISet<TSyntaxKind> LoopStatements { get; }

    protected abstract LoopWalkerBase<TStatementSyntax, TSyntaxKind> Walker(SonarSyntaxNodeReportingContext context);

    protected override string MessageFormat => "Refactor the containing loop to do more than one iteration.";

    protected UnconditionalJumpStatementBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                var walker = Walker(c);
                if (walker.Visit())
                {
                    foreach (var node in walker.RuleViolations())
                    {
                        c.ReportIssue(Rule, node);
                    }
                }
            },
            LoopStatements.ToArray());
}

public abstract class LoopWalkerBase<TStatementSyntax, TSyntaxKind>
    where TStatementSyntax : SyntaxNode
    where TSyntaxKind : struct
{
    protected readonly SyntaxNode rootExpression;
    protected readonly SemanticModel model;
    protected readonly ICollection<TStatementSyntax> conditionalContinues = [];
    protected readonly ICollection<TStatementSyntax> conditionalTerminates = [];
    protected readonly ICollection<TStatementSyntax> unconditionalContinues = [];
    protected readonly ICollection<TStatementSyntax> unconditionalTerminates = [];

    private readonly ISet<TSyntaxKind> loopStatements;

    public abstract bool Visit();

    protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
    protected abstract ISet<TSyntaxKind> ConditionalStatements { get; }
    protected abstract ISet<TSyntaxKind> StatementsThatCanThrow { get; }
    protected abstract ISet<TSyntaxKind> LambdaSyntaxes { get; }
    protected abstract ISet<TSyntaxKind> LocalFunctionSyntaxes { get; }

    protected abstract IEnumerable<TStatementSyntax> TryBlockStatements(IReadOnlyList<SyntaxNode> ancestors);
    protected abstract bool IsPropertyAccess(TStatementSyntax node);
    protected abstract bool IsJumpToRootLoop(TStatementSyntax node);

    protected LoopWalkerBase(SonarSyntaxNodeReportingContext context, ISet<TSyntaxKind> loopStatements)
    {
        rootExpression = context.Node;
        model = context.Model;
        this.loopStatements = loopStatements;
    }

    public IEnumerable<TStatementSyntax> RuleViolations() =>
        conditionalContinues.Count == 0
            ? unconditionalContinues.Concat(unconditionalTerminates)
            : unconditionalContinues;

    protected void StoreVisitData(TStatementSyntax node, ICollection<TStatementSyntax> conditionalCollection, ICollection<TStatementSyntax> unconditionalCollection)
    {
        var ancestors = node
            .Ancestors()
            .TakeWhile(x => !rootExpression.Equals(x))
            .ToList();

        if (ancestors.Any(x => Language.Syntax.IsAnyKind(x, LambdaSyntaxes) || Language.Syntax.IsAnyKind(x, LocalFunctionSyntaxes)))
        {
            return;
        }

        var isInNestedLoop = ancestors.Any(x => Language.Syntax.IsAnyKind(x, loopStatements));
        if (isInNestedLoop && !IsJumpToRootLoop(node))
        {
            return;
        }

        if (isInNestedLoop
            || ancestors.Any(x => Language.Syntax.IsAnyKind(x, ConditionalStatements))
            || IsInTryCatchWithStatementThatCanThrow(node, ancestors))
        {
            conditionalCollection.Add(node);
        }
        else
        {
            unconditionalCollection.Add(node);
        }
    }

    private bool IsInTryCatchWithStatementThatCanThrow(TStatementSyntax node, IReadOnlyList<SyntaxNode> ancestors)
    {
        var tryBlockStatements = TryBlockStatements(ancestors);
        return tryBlockStatements.Any()
            && (IsReturnThatCanThrow(node)
                || tryBlockStatements
                    .TakeWhile(x => !x.Equals(node))
                    .SelectMany(x => x.DescendantNodes())
                    .Any(x => Language.Syntax.IsAnyKind(x, StatementsThatCanThrow)));
    }

    private bool IsReturnThatCanThrow(TStatementSyntax node) =>
        Language.Syntax.IsKind(node, Language.SyntaxKind.ReturnStatement)
        && (node.DescendantNodes().Any(x => Language.Syntax.IsAnyKind(x, StatementsThatCanThrow)) || IsPropertyAccess(node));
}
