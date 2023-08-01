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

using Microsoft.CodeAnalysis.Text;
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

        foreach (var isTrue in alwaysTrue)
        {
            ReportIssue(isTrue.Key, isTrue.Value, true);
        }
        foreach (var isFalse in alwaysFalse)
        {
            ReportIssue(isFalse.Key, isFalse.Value, false);
        }

        base.ExecutionCompleted();
    }

    private void ReportIssue(IOperation operation, BasicBlock block, bool conditionEvaluation)
    {
        var issueMessage = operation.Kind == OperationKindEx.IsNull ? MessageNullCoalescing : string.Format(MessageBool, conditionEvaluation);
        var unreachable = Unreachable(conditionEvaluation, block).Where(x => IsNotIgnored(x.Syntax));
        if (unreachable.Any())
        {
            ReportIssue(Rule2583, operation, SecondaryLocations(unreachable), string.Format(MessageUnreachable, issueMessage));
        }
        else
        {
            ReportIssue(Rule2589, operation, null, string.Format(issueMessage, conditionEvaluation));
        }
    }
    private static IEnumerable<IOperation> Unreachable(bool conditionIsTrue, BasicBlock block)
    {
        if (block.SuccessorBlocks.Distinct().Count() != 2)
        {
            return Enumerable.Empty<IOperation>();
        }
        HashSet<BasicBlock> reachable = new() { block };
        HashSet<BasicBlock> unreachable = new();
        var successor = conditionIsTrue ^ block.ConditionKind == ControlFlowConditionKind.WhenFalse
            ? block.ConditionalSuccessor.Destination
            : block.SuccessorBlocks.Single(x => x != block.ConditionalSuccessor.Destination);
        TraverseReachable(successor);
        var unreachableSuccessor = block.SuccessorBlocks.Single(x => x != successor);
        TraverseUnreachable(unreachableSuccessor);
        return unreachable.SelectMany(x => x.OperationsAndBranchValue);

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

    private bool IsNotIgnored(SyntaxNode x) =>
        !IsForLoopIncrementor(x)
        && !IsConditionalAccessExpression(x)
        && !IsLeftCoalesceExpression(x);

    private List<Location> SecondaryLocations(IEnumerable<IOperation> unreachable)
    {
        List<Location> locations = new();
        IOperation currentStart = null;
        IOperation currentEnd = null;
        foreach (var operation in unreachable.Union(reached).OrderBy(x => x.Syntax.SpanStart))
        {
            if (reached.Contains(operation))
            {
                if (currentStart is not null)
                {
                    locations.Add(currentStart.Syntax.CreateLocation(currentEnd.Syntax));
                    currentStart = null;
                }
            }
            else
            {
                currentStart ??= operation;
                currentEnd = operation;
            }
        }
        if (currentStart != null)
        {
            locations.Add(currentStart.Syntax.CreateLocation(currentEnd.Syntax));
        }
        return locations;
    }
}
