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
        var state = context.State;
        SymbolicValue value;
        if (context.State.Constraint<FuzzyConstraint>(assignment.Target) is { } fuzzy && fuzzy.Source == assignment.WrappedOperation)
        {
            value = SymbolicValue.Empty;
        }
        else
        {
            value = (ProcessNumericalCompoundAssignment(state, assignment) ?? ProcessCompoundAssignment(state, assignment)) is { } constraint
                ? (state[assignment.Target] ?? SymbolicValue.Empty).WithConstraint(constraint).WithConstraint(new FuzzyConstraint(assignment.WrappedOperation))
                : SymbolicValue.Empty;
        }
        if (assignment.Target.TrackedSymbol(state) is { } symbol)
        {
            state = state.SetSymbolValue(symbol, value);
        }
        return state.SetOperationValue(assignment, value);
    }

    private static SymbolicConstraint ProcessNumericalCompoundAssignment(ProgramState state, ICompoundAssignmentOperationWrapper assignment) =>
        state.Constraint<NumberConstraint>(assignment.Target) is { } leftNumber
        && state.Constraint<NumberConstraint>(assignment.Value) is { } rightNumber
            ? ArithmeticCalculator.Calculate(assignment.OperatorKind, leftNumber, rightNumber)
            : null;

    private static SymbolicConstraint ProcessCompoundAssignment(ProgramState state, ICompoundAssignmentOperationWrapper assignment) =>
        (state.HasConstraint(assignment.Target, ObjectConstraint.NotNull) && state.HasConstraint(assignment.Value, ObjectConstraint.NotNull))
        || assignment.Target.Type.Is(KnownType.System_String)
        || assignment.Target.Type.IsNonNullableValueType()
            ? ObjectConstraint.NotNull
            : null;
}
