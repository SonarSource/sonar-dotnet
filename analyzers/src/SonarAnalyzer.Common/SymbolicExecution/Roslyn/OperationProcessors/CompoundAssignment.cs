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

using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

internal sealed class CompoundAssignment : SimpleProcessor<ICompoundAssignmentOperationWrapper>
{
    protected override ICompoundAssignmentOperationWrapper Convert(IOperation operation) =>
        ICompoundAssignmentOperationWrapper.FromOperation(operation);

    protected override ProgramState Process(SymbolicContext context, ICompoundAssignmentOperationWrapper assignment)
    {
        var state = context.SetOperationValue(SymbolicValue.NotNull);
        if (assignment.Target.TrackedSymbol(state) is { } symbol)
        {
            state = state.SetSymbolValue(symbol, SymbolicValue.NotNull);
            state = CheckForNumberConstraints(state, assignment, symbol);
        }
        return state;
    }

    private ProgramState CheckForNumberConstraints(ProgramState state, ICompoundAssignmentOperationWrapper assignment, ISymbol targetSymbol)
    {
        if (state[assignment.Target]?.Constraint<NumberConstraint>() is { } leftNumber
            && state[assignment.Value]?.Constraint<NumberConstraint>() is { } rightNumber
            && Calculate(assignment.OperatorKind, leftNumber, rightNumber) is { } constraint)
        {
            state =  state.SetSymbolConstraint(targetSymbol, constraint);
            return state.SetOperationConstraint(assignment, constraint);
        }
        return state;
    }

    private static NumberConstraint Calculate(BinaryOperatorKind kind, NumberConstraint left, NumberConstraint right) => kind switch
    {
        BinaryOperatorKind.Add => NumberConstraint.From(left.Min + right.Min, left.Max + right.Max),
        BinaryOperatorKind.Subtract => NumberConstraint.From(left.Min - right.Max, left.Max - right.Min),
        _ => null
    };
}
