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

    protected override ProgramState Process(SymbolicContext context, ICompoundAssignmentOperationWrapper assignment) =>
        ProcessNumericalCompoundAssignment(context, assignment)
        ?? LearnNotNullFromCompoundAssignment(context.State, assignment)
        ?? context.State;

    private ProgramState ProcessNumericalCompoundAssignment(SymbolicContext context, ICompoundAssignmentOperationWrapper assignment)
    {
        var state = context.State;
        var leftNumber = state[assignment.Target]?.Constraint<NumberConstraint>();
        var rightNumber = state[assignment.Value]?.Constraint<NumberConstraint>();
        if (leftNumber is { } && rightNumber is { } && Calculate(assignment.OperatorKind, leftNumber, rightNumber) is { } constraint)
        {
            if (assignment.Target.TrackedSymbol(state) is { } targetSymbol)
            {
                state = state.SetSymbolConstraint(targetSymbol, constraint);
            }
            return state.SetOperationConstraint(assignment, constraint);
        }
        else if (leftNumber is { }
            && assignment.Target.TrackedSymbol(state) is { } targetSymbol
            && state[assignment.Value]?.Constraint<ObjectConstraint>() is { Kind: ConstraintKind.NotNull })
        {
            return state.SetSymbolValue(targetSymbol, SymbolicValue.NotNull);
        }
        return null;
    }

    private ProgramState LearnNotNullFromCompoundAssignment(ProgramState state, ICompoundAssignmentOperationWrapper assignment) =>
        assignment.Target.TrackedSymbol(state) is { } targetSymbol && targetSymbol.GetSymbolType().IsAny(KnownType.System_String, KnownType.System_Boolean)
            ? state.SetSymbolValue(targetSymbol, SymbolicValue.NotNull)
            : null;

    private static NumberConstraint Calculate(BinaryOperatorKind kind, NumberConstraint left, NumberConstraint right) => kind switch
    {
        BinaryOperatorKind.Add => NumberConstraint.From(left.Min + right.Min, left.Max + right.Max),
        BinaryOperatorKind.Subtract => NumberConstraint.From(left.Min - right.Max, left.Max - right.Min),
        _ => null
    };
}
