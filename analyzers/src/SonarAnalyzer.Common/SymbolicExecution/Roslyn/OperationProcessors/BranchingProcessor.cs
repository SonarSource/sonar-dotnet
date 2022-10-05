/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

/// <summary>
/// Base class for operation processors - used when operation takes a branching decision.
/// See <see cref="SimpleProcessor{T}"/> if you need to return a single ProgramStates.
/// See <see cref="BranchingProcessor{T}"/> if you just need to return multiple ProgramStates.
/// </summary>
internal abstract class BranchingProcessor<T> : MultiProcessor<T>
    where T : IOperationWrapper
{
    protected abstract SymbolicConstraint BoolConstraintFromOperation(SymbolicContext context, T operation);
    protected abstract ProgramState LearnBranchingConstraint(ProgramState state, T operation, bool falseBranch);

    protected override ProgramState[] Process(SymbolicContext context, T operation)
    {
        if (BoolConstraintFromOperation(context, operation) is { } constraint)
        {
            return context.SetOperationConstraint(constraint).ToArray();    // We already know the answer from existing constraints
        }
        else
        {
            var positive = LearnBranchingConstraint(context.State, operation, false);
            var negative = LearnBranchingConstraint(context.State, operation, true);
            return positive == context.State && negative == context.State
                ? context.State.ToArray()   // We can't learn anything, just move on
                : new[]
                {
                    positive.SetOperationConstraint(context.Operation, BoolConstraint.True),
                    negative.SetOperationConstraint(context.Operation, BoolConstraint.False)
                };
        }
    }
}
