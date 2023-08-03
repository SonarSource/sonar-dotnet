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

using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks;

public abstract class ConditionEvaluatesToConstantBase : SymbolicRuleCheck
{
    protected const string DiagnosticId2583 = "S2583"; // Bug
    protected const string DiagnosticId2589 = "S2589"; // Code smell

    protected const string MessageFormat = "{0}";
    private const string MessageBool = "Change this condition so that it does not always evaluate to '{0}'.";
    private const string MessageNullCoalescing = "Change this expression which always evaluates to the same result.";
    private const string MessageUnreachable = "{0} Some code paths are unreachable.";

    protected abstract DiagnosticDescriptor Rule2583 { get; }
    protected abstract DiagnosticDescriptor Rule2589 { get; }

    private readonly Dictionary<IOperation, BasicBlock> trueOperations = new();
    private readonly Dictionary<IOperation, BasicBlock> falseOperations = new();
    private readonly HashSet<IOperation> reached = new();

    protected abstract bool IsLeftCoalesceExpression(SyntaxNode syntax);
    protected abstract bool IsConditionalAccessExpression(SyntaxNode syntax);
    protected abstract bool IsForLoopIncrementor(SyntaxNode syntax);
    protected abstract bool IsUsing(SyntaxNode syntax);

    public override ProgramState[] PreProcess(SymbolicContext context)
    {
        reached.Add(context.Operation.Instance);
        return base.PreProcess(context);
    }

    public override ProgramState ConditionEvaluated(SymbolicContext context)
    {
        var operation = context.Operation.Instance;
        if (operation.Kind is not OperationKindEx.Literal
            && operation.Syntax.Ancestors().Any(IsUsing) is false
            && operation?.TrackedSymbol() is not IFieldSymbol { IsConst: true }
            && !IsDiscardPattern(operation))
        {
            if (context.State[operation].Constraint<BoolConstraint>().Kind == ConstraintKind.True)
            {
                trueOperations[operation] = context.Block;
            }
            else
            {
                falseOperations[operation] = context.Block;
            }
        }
        return base.ConditionEvaluated(context);

        static bool IsDiscardPattern(IOperation operation) =>
            operation.AsIsPattern() is { } pattern
            && pattern.Pattern.WrappedOperation.Kind is OperationKindEx.DiscardPattern;
}

    public override void ExecutionCompleted()
    {
        var alwaysTrue = trueOperations.Except(falseOperations);
        var alwaysFalse = falseOperations.Except(trueOperations);

        foreach (var (operation, block) in alwaysTrue.Select(x => (x.Key, x.Value)))
        {
            ReportIssue(operation, block, true);
        }
        foreach (var (operation, block) in alwaysFalse.Select(x => (x.Key, x.Value)))
        {
            ReportIssue(operation, block, false);
        }

        base.ExecutionCompleted();
    }

    private void ReportIssue(IOperation operation, BasicBlock block, bool conditionIsTrue)
    {
        var issueMessage = operation.Kind == OperationKindEx.IsNull ? MessageNullCoalescing : string.Format(MessageBool, conditionIsTrue);
        var secondaryLocations = SecondaryLocations(block, conditionIsTrue);
        if (secondaryLocations.Any())
        {
            ReportIssue(Rule2583, operation, secondaryLocations, string.Format(MessageUnreachable, issueMessage));
        }
        else
        {
            ReportIssue(Rule2589, operation, null, string.Format(issueMessage, conditionIsTrue));
        }
    }

    private List<Location> SecondaryLocations(BasicBlock block, bool conditionIsTrue)
    {
        List<Location> locations = new();
        IOperation currentStart = null;
        IOperation currentEnd = null;
        var unreachable = UnreachableOperations(block, conditionIsTrue).Where(x => !IsIgnoredLocation(x.Syntax));
        foreach (var operation in unreachable.Concat(reached).OrderBy(x => x.Syntax.SpanStart))
        {
            if (unreachable.Contains(operation))
            {
                currentStart ??= operation;
                currentEnd = operation;
            }
            else
            {
                if (currentStart is not null)
                {
                    locations.Add(currentStart.Syntax.CreateLocation(currentEnd.Syntax));
                    currentStart = null;
                }
            }
        }
        if (currentStart != null)
        {
            locations.Add(currentStart.Syntax.CreateLocation(currentEnd.Syntax));
        }
        return locations;
    }

    private IEnumerable<IOperation> UnreachableOperations(BasicBlock block, bool conditionIsTrue)
    {
        if (block.SuccessorBlocks.Distinct().Count() != 2)
        {
            return Enumerable.Empty<IOperation>();
        }
        HashSet<BasicBlock> reachable = new() { block };
        HashSet<BasicBlock> unreachable = new();

        var reachableSuccessor = conditionIsTrue ^ block.ConditionKind == ControlFlowConditionKind.WhenFalse
            ? block.ConditionalSuccessor.Destination
            : block.SuccessorBlocks.Single(x => x != block.ConditionalSuccessor.Destination);
        TraverseReachable(reachableSuccessor);
        var unreachableSuccessor = block.SuccessorBlocks.Single(x => x != reachableSuccessor);
        TraverseUnreachable(unreachableSuccessor);
        return unreachable.SelectMany(x => x.OperationsAndBranchValue).Except(reached);

        void TraverseReachable(BasicBlock block)
        {
            if (reachable.Add(block))
            {
                foreach (var successor in block.SuccessorBlocks)
                {
                    TraverseReachable(successor);
                }
            }
        }

        void TraverseUnreachable(BasicBlock block)
        {
            if (reachable.Contains(block) is false && unreachable.Add(block))
            {
                foreach (var successor in block.SuccessorBlocks)
                {
                    TraverseUnreachable(successor);
                }
            }
        }
    }

    private bool IsIgnoredLocation(SyntaxNode x) =>
        IsForLoopIncrementor(x)
        || IsConditionalAccessExpression(x)
        || IsLeftCoalesceExpression(x);
}
