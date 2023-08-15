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

    protected const string MessageFormat = "{0}{1}";
    private const string MessageBool = "Change this condition so that it does not always evaluate to '{0}'.";
    private const string MessageNull = "Change this expression which always evaluates to the same result.";
    private const string S2583MessageSuffix = " Some code paths are unreachable.";

    protected abstract DiagnosticDescriptor Rule2583 { get; }
    protected abstract DiagnosticDescriptor Rule2589 { get; }

    private readonly Dictionary<IOperation, BasicBlock> trueOperations = new();
    private readonly Dictionary<IOperation, BasicBlock> falseOperations = new();
    private readonly HashSet<IOperation> reached = new();

    protected abstract bool IsLeftCoalesceExpression(SyntaxNode syntax);
    protected abstract bool IsConditionalAccessExpression(SyntaxNode syntax);
    protected abstract bool IsForLoopIncrementor(SyntaxNode syntax);
    protected abstract bool IsUsing(SyntaxNode syntax);
    protected abstract bool IsLockStatement(SyntaxNode syntax);

    public override ProgramState[] PreProcess(SymbolicContext context)
    {
        reached.Add(context.Operation.Instance);
        return context.State.ToArray();
    }

    public override ProgramState ConditionEvaluated(SymbolicContext context)
    {
        var operation = context.Operation.Instance;
        if (operation.Kind is not OperationKindEx.Literal
            && !operation.Syntax.Ancestors().Any(x => IsUsing(x) || IsLockStatement(x))
            && operation.TrackedSymbol(context.State) is not IFieldSymbol { IsConst: true }
            && !IsDiscardPattern(operation))
        {
            if (context.State.HasConstraint(operation, BoolConstraint.True))
            {
                trueOperations[operation] = context.Block;
            }
            else
            {
                falseOperations[operation] = context.Block;
            }
        }
        return context.State;

        static bool IsDiscardPattern(IOperation operation) =>
            operation.AsIsPattern() is { } pattern
            && pattern.Pattern.WrappedOperation.Kind is OperationKindEx.DiscardPattern;
    }

    public override void ExecutionCompleted()
    {
        var alwaysTrue = trueOperations.Except(falseOperations);
        var alwaysFalse = falseOperations.Except(trueOperations);

        foreach (var pair in alwaysTrue)
        {
            ReportIssue(pair.Key, pair.Value, true);
        }
        foreach (var pair in alwaysFalse)
        {
            ReportIssue(pair.Key, pair.Value, false);
        }
    }

    private void ReportIssue(IOperation operation, BasicBlock block, bool conditionValue)
    {
        var issueMessage = operation.Kind == OperationKindEx.IsNull ? MessageNull : string.Format(MessageBool, conditionValue);
        var secondaryLocations = SecondaryLocations(block, conditionValue);
        if (secondaryLocations.Any())
        {
            ReportIssue(Rule2583, operation, secondaryLocations, issueMessage, S2583MessageSuffix);
        }
        else
        {
            ReportIssue(Rule2589, operation, null, issueMessage, string.Empty);
        }
    }

    private List<Location> SecondaryLocations(BasicBlock block, bool conditionValue)
    {
        List<Location> locations = new();
        IOperation currentStart = null;
        IOperation currentEnd = null;
        var unreachable = UnreachableOperations(block, conditionValue).Where(x => !IsIgnoredLocation(x.Syntax)).ToHashSet();
        foreach (var operation in unreachable.Concat(reached).OrderBy(x => x.Syntax.SpanStart))
        {
            if (unreachable.Contains(operation))
            {
                currentStart ??= operation;
                currentEnd = operation;
            }
            else
            {
                AddCurrent();
            }
        }
        AddCurrent();
        return locations;

        void AddCurrent()
        {
            if (currentStart is not null)
            {
                locations.Add(currentStart.Syntax.CreateLocation(currentEnd.Syntax));
                currentStart = null;
            }
        }
    }

    private IEnumerable<IOperation> UnreachableOperations(BasicBlock block, bool conditionValue)
    {
        if (block.SuccessorBlocks.Distinct().Count() != 2)
        {
            return Enumerable.Empty<IOperation>();
        }
        HashSet<BasicBlock> reachable = new() { block };
        HashSet<BasicBlock> unreachable = new();

        var conditionalIsRechable = (block.ConditionKind == ControlFlowConditionKind.WhenTrue) == conditionValue;
        Traverse(conditionalIsRechable ? block.ConditionalSuccessor : block.FallThroughSuccessor, reachable, new List<BasicBlock>());
        Traverse(conditionalIsRechable ? block.FallThroughSuccessor : block.ConditionalSuccessor, unreachable, reachable);
        return unreachable.SelectMany(x => x.OperationsAndBranchValue).Except(reached);

        static void Traverse(ControlFlowBranch branch, HashSet<BasicBlock> result, ICollection<BasicBlock> excluded)
        {
            var queue = new Queue<BasicBlock>();
            queue.Enqueue(branch.Destination);
            do
            {
                var current = queue.Dequeue();
                if (!excluded.Contains(current) && result.Add(current))
                {
                    foreach (var successor in current.SuccessorBlocks)
                    {
                        queue.Enqueue(successor);
                    }
                }
            }
            while (queue.Any());
        }
    }

    private bool IsIgnoredLocation(SyntaxNode x) =>
        IsForLoopIncrementor(x)
        || IsConditionalAccessExpression(x)
        || IsLeftCoalesceExpression(x);
}
