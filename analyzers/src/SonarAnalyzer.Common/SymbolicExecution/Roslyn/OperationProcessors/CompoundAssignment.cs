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

internal sealed class CompoundAssignment : SimpleProcessor<ICompoundAssignmentOperationWrapper>
{
    protected override ICompoundAssignmentOperationWrapper Convert(IOperation operation) =>
        ICompoundAssignmentOperationWrapper.FromOperation(operation);

    protected override ProgramState Process(SymbolicContext context, ICompoundAssignmentOperationWrapper assignment) =>
        ProcessNumericalCompoundAssignment(context.State, assignment, context.IsInLoop)
        ?? ProcessDelegateCompoundAssignment(context.State, assignment)
        ?? ProcessCompoundAssignment(context.State, assignment)
        ?? context.State;

    private static ProgramState ProcessNumericalCompoundAssignment(ProgramState state, ICompoundAssignmentOperationWrapper assignment, bool isInLoop)
    {
        if (state.Constraint<NumberConstraint>(assignment.Target) is { } leftNumber
            && state.Constraint<NumberConstraint>(assignment.Value) is { } rightNumber
            && ArithmeticCalculator.Calculate(assignment.OperatorKind, leftNumber, rightNumber, isInLoop) is { } constraint)
        {
            state = state.SetOperationConstraint(assignment, constraint);
            if (assignment.Target.TrackedSymbol(state) is { } symbol)
            {
                state = state.SetSymbolConstraint(symbol, constraint);
            }
            return state;
        }
        else
        {
            return null;
        }
    }

    private static ProgramState ProcessDelegateCompoundAssignment(ProgramState state, ICompoundAssignmentOperationWrapper assignment)
    {
        // When the -= operator is used on a delegate instance (for unsubscribing),
        // it can leave the invocation list associated with the delegate empty. In that case the delegate instance will become null.
        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/subtraction-operator#delegate-removal
        // For this reason, we remove the NotNull constraint from the delegate instance.
        if (assignment.Target.Type.TypeKind == TypeKind.Delegate
            && assignment.OperatorKind == BinaryOperatorKind.Subtract
            && state.HasConstraint(assignment.Target, ObjectConstraint.NotNull))
        {
            var value = (state[assignment.Target] ?? SymbolicValue.Empty).WithoutConstraint(ObjectConstraint.NotNull);
            return state
                .SetOperationValue(assignment, value)
                .SetOperationValue(assignment.Target, value)
                .SetSymbolValue(assignment.Target.TrackedSymbol(state), value);
        }
        else
        {
            return null;
        }
    }

    private static ProgramState ProcessCompoundAssignment(ProgramState state, ICompoundAssignmentOperationWrapper assignment)
    {
        if ((state.HasConstraint(assignment.Target, ObjectConstraint.NotNull) && state.HasConstraint(assignment.Value, ObjectConstraint.NotNull))
            || assignment.Target.Type.Is(KnownType.System_String)
            || assignment.Target.Type.IsNonNullableValueType())
        {
            state = state.SetOperationConstraint(assignment, ObjectConstraint.NotNull);
            if (assignment.Target.TrackedSymbol(state) is { } symbol)
            {
                state = state.SetSymbolValue(symbol, SymbolicValue.NotNull);
            }
            return state;
        }
        else
        {
            return assignment.Target.TrackedSymbol(state) is { } symbol
                ? state.SetSymbolValue(symbol, SymbolicValue.Empty)
                : state;
        }
    }
}
