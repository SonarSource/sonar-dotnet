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

internal sealed class IsType : BranchingProcessor<IIsTypeOperationWrapper>
{
    protected override IIsTypeOperationWrapper Convert(IOperation operation) =>
        IIsTypeOperationWrapper.FromOperation(operation);

    protected override SymbolicConstraint BoolConstraintFromOperation(ProgramState state, IIsTypeOperationWrapper operation) =>
        state[operation.ValueOperand]?.HasConstraint(ObjectConstraint.Null) is true ? BoolConstraint.False : null;

    protected override ProgramState LearnBranchingConstraint(ProgramState state, IIsTypeOperationWrapper operation, bool falseBranch) =>
        operation.ValueOperand.TrackedSymbol(state) is { } testedSymbol
        && ObjectConstraint.NotNull.ApplyOpposite(falseBranch ^ operation.IsNegated) is { } constraint
            ? state.SetSymbolConstraint(testedSymbol, constraint)
            : state;
}
