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

using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

/// <summary>
/// Base class for operation processors - used when operation takes a branching decision.
/// See <see cref="SimpleProcessor{T}"/> if you need to return a single ProgramStates.
/// See <see cref="BranchingProcessor{T}"/> if you just need to return multiple ProgramStates.
/// </summary>
internal abstract class BranchingProcessor<T> : MultiProcessor<T>
    where T : IOperationWrapper
{
    protected abstract SymbolicConstraint BoolConstraintFromOperation(ProgramState state, T operation);
    protected abstract ProgramState LearnBranchingConstraint(ProgramState state, T operation, bool falseBranch);

    protected virtual ProgramState PreProcess(ProgramState state, T operation, bool isInLoop) =>
        state;

    protected override ProgramStates Process(SymbolicContext context, T operation)
    {
        var state = PreProcess(context.State, operation, context.IsInLoop);
        if (BoolConstraintFromOperation(state, operation) is { } constraint)
        {
            return new(state.SetOperationConstraint(context.Operation, constraint));    // We already know the answer from existing constraints
        }
        else
        {
            var beforeLearningState = state;
            var positive = LearnBranchingConstraint(state, operation, false);
            var negative = LearnBranchingConstraint(state, operation, true);
            return positive == beforeLearningState && negative == beforeLearningState
                ? new(beforeLearningState)   // We can't learn anything, just move on
                : new(
                    positive.SetOperationConstraint(context.Operation, BoolConstraint.True),
                    negative.SetOperationConstraint(context.Operation, BoolConstraint.False));
        }
    }
}
