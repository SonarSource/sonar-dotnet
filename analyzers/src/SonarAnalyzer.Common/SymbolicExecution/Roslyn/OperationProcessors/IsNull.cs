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

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors
{
    internal static class IsNull
    {
        public static ProgramState[] Process(SymbolicContext context, IIsNullOperationWrapper isNull)
        {
            if (context.State[isNull.Operand] is { } value && value.HasConstraint<ObjectConstraint>())
            {
                return new[] { context.SetOperationConstraint(BoolConstraint.From(value.HasConstraint(ObjectConstraint.Null))) };
            }
            else
            {
                var positive = LearnBranchingConstraint(context.State, isNull, false);
                var negative = LearnBranchingConstraint(context.State, isNull, true);
                return positive == context.State && negative == context.State
                    ? new[] { context.State }   // We can't learn anything, just move on
                    : new[]
                    {
                        positive.SetOperationConstraint(context.Operation, BoolConstraint.True),
                        negative.SetOperationConstraint(context.Operation, BoolConstraint.False)
                    };
            }
        }

        private static ProgramState LearnBranchingConstraint(ProgramState state, IIsNullOperationWrapper isNull, bool useOpposite) =>
            state.ResolveCapture(isNull.Operand).TrackedSymbol() is { } testedSymbol
                // Can't use ObjectConstraint.ApplyOpposite() because here, we are sure that it is either Null or NotNull
                ? state.SetSymbolConstraint(testedSymbol, useOpposite ? ObjectConstraint.NotNull : ObjectConstraint.Null)
                : state;
    }
}
