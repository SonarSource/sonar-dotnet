/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
    private const string S2583MessageSuffix = " Some code paths are unreachable.";

    protected abstract DiagnosticDescriptor Rule2583 { get; }
    protected abstract DiagnosticDescriptor Rule2589 { get; }
    protected abstract string NullName { get; }
    private string MessageNull => $"Remove this unnecessary check for {NullName}.";

    private readonly Dictionary<IOperation, BasicBlock> trueOperations = new();
    private readonly Dictionary<IOperation, BasicBlock> falseOperations = new();
    private readonly Dictionary<IOperation, BasicBlock> unknownOperations = new();
    private readonly List<IOperation> reachedOperations = new();

    protected abstract bool IsInsideUsingDeclaration(SyntaxNode node);
    protected abstract bool IsLockStatement(SyntaxNode syntax);

    public override ProgramStates PreProcess(SymbolicContext context)
    {
        reachedOperations.Add(context.Operation.Instance);
        return new(context.State);
    }

    public override ProgramState ConditionEvaluated(SymbolicContext context)
    {
        var operation = context.Operation.Instance;
        if (!IsIgnored(context.State, operation))
        {
            var constraint = context.State.Constraint<BoolConstraint>(operation);
            if (constraint == BoolConstraint.True)
            {
                trueOperations[operation] = context.Block;
            }
            else if (constraint == BoolConstraint.False)
            {
                falseOperations[operation] = context.Block;
            }
            else
            {
                unknownOperations[operation] = context.Block;
            }
        }
        return context.State;
    }

    private bool IsIgnored(ProgramState state, IOperation operation) =>
        operation.Kind is OperationKindEx.Literal
        || IsVarPattern(operation)
        || IsDiscardPattern(state, operation)
        || operation.TrackedSymbol(state) is IFieldSymbol { IsConst: true }
        || operation.Syntax.Ancestors().Any(x => IsInsideUsingDeclaration(x) || IsLockStatement(x));

    private static bool IsVarPattern(IOperation operation) =>
        operation.AsIsPattern()?.Pattern.WrappedOperation.AsDeclarationPattern()?.MatchesNull is true;

    private static bool IsDiscardPattern(ProgramState state, IOperation operation) =>
        operation.Kind is OperationKindEx.DiscardPattern
        || (operation.AsIsPattern() is { } isPattern
            && (isPattern.Pattern.WrappedOperation.Kind is OperationKindEx.DiscardPattern
                || (isPattern.Pattern.WrappedOperation.AsRecursivePattern() is {} recursivePattern && recursivePattern.DeconstructionSubpatterns.All(x => IsDiscardPattern(state, x)))));

    public override void ExecutionCompleted()
    {
        var alwaysTrue = trueOperations.Except(falseOperations).Except(unknownOperations);
        var alwaysFalse = falseOperations.Except(trueOperations).Except(unknownOperations);

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
        // For SwitchExpressionArms like `true => 5` we are only interested in the left part (`true`).
        var syntax = operation.Syntax.IsKind(SyntaxKindEx.SwitchExpressionArm) ? ((SwitchExpressionArmSyntaxWrapper)operation.Syntax).Pattern : operation.Syntax;
        var secondaryLocations = SecondaryLocations(block, conditionValue, syntax.Span.End);
        if (secondaryLocations.Any())
        {
            ReportIssue(Rule2583, syntax, secondaryLocations, properties: null, issueMessage, S2583MessageSuffix);
        }
        else
        {
            ReportIssue(Rule2589, syntax, additionalLocations: null, properties: null, issueMessage, string.Empty);
        }
    }

    private List<Location> SecondaryLocations(BasicBlock block, bool conditionValue, int spanStart)
    {
        List<Location> locations = new();
        var unreachable = UnreachableOperations(block, conditionValue);
        var currentStart = spanStart;

        foreach (var node in reachedOperations.Select(x => x.Syntax).Where(x => x.SpanStart > spanStart).OrderBy(x => x.SpanStart))
        {
            if (AddUnreachableLocation(currentStart, node.SpanStart))
            {
                currentStart = node.Span.End;
            }
        }
        AddUnreachableLocation(currentStart, int.MaxValue);  // Get all unreachable operations from the very last reached one until the end of the method.
        return locations;

        bool AddUnreachableLocation(int from, int to)
        {
            var nodes = unreachable.Where(x => x.SpanStart > from && x.Span.End < to);
            if (nodes.Any())
            {
                var firstNode = nodes.OrderBy(x => x.SpanStart).First();
                var lastNode = nodes.OrderBy(x => x.Span.End).Last();
                locations.Add(firstNode.CreateLocation(lastNode));
                return true;
            }
            return false;
        }
    }

    private List<SyntaxNode> UnreachableOperations(BasicBlock block, bool conditionValue)
    {
        if (block.SuccessorBlocks.Distinct().Count() != 2)
        {
            return new List<SyntaxNode>();
        }
        HashSet<BasicBlock> reachable = new() { block };
        HashSet<BasicBlock> unreachable = new();

        var conditionalIsRechable = (block.ConditionKind == ControlFlowConditionKind.WhenTrue) == conditionValue;
        Traverse(conditionalIsRechable ? block.ConditionalSuccessor : block.FallThroughSuccessor, reachable, new List<BasicBlock>());
        Traverse(conditionalIsRechable ? block.FallThroughSuccessor : block.ConditionalSuccessor, unreachable, reachable);
        return unreachable
            .SelectMany(x => x.OperationsAndBranchValue)
            .Except(reachedOperations)
            .SelectMany(x => x.DescendantsAndSelf().Select(x => x.Syntax))
            .ToList();

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
}
