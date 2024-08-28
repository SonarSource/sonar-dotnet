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

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

internal sealed class DeconstructionAssignment : SimpleProcessor<IDeconstructionAssignmentOperationWrapper>
{
    protected override IDeconstructionAssignmentOperationWrapper Convert(IOperation operation) =>
        IDeconstructionAssignmentOperationWrapper.FromOperation(operation);

    protected override ProgramState Process(SymbolicContext context, IDeconstructionAssignmentOperationWrapper assignment)
    {
        var operationValues = TupleElementValues(context.State, assignment.Target, assignment.Value);
        var newState = context.State;
        foreach (var (tupleMember, value) in operationValues)
        {
            newState = newState.SetOperationAndSymbolValue(tupleMember, value);
        }
        return newState;
    }

    private static IEnumerable<OperationValue> TupleElementValues(ProgramState state, IOperation target, IOperation value)
    {
        var operationValues = new List<OperationValue>();
        var leftTupleElements = TupleElements(Unwrap(target, state).ToTuple(), state);
        // If the right side is a tuple, then every symbol/constraint is copied to the left side.
        if (Unwrap(value, state).AsTuple() is { } rightSideTuple)
        {
            var rightTupleElements = TupleElements(rightSideTuple, state);
            for (var i = 0; i < leftTupleElements.Length; i++)
            {
                var leftTupleMember = leftTupleElements[i];
                var rightTupleMember = rightTupleElements[i];
                if (leftTupleMember.Kind == OperationKindEx.Discard)
                {
                    continue;
                }
                else if (leftTupleMember.AsTuple() is { } nestedTuple)
                {
                    operationValues.AddRange(TupleElementValues(state, nestedTuple.WrappedOperation, rightTupleMember));
                }
                else
                {
                    operationValues.Add(new(leftTupleMember, state[rightTupleMember]));
                }
            }
        }
        // If the right side is not a tuple, then every member of the left side tuple is set to empty.
        else
        {
            operationValues.AddRange(leftTupleElements
                .Where(x => x.Kind != OperationKindEx.Discard)
                .Select(x => new OperationValue(x, SymbolicValue.Empty)));
        }
        return operationValues;
    }

    private static IOperation[] TupleElements(ITupleOperationWrapper operation, ProgramState state) =>
        operation.Elements.Select(x => Unwrap(x, state)).ToArray();

    private static IOperation Unwrap(IOperation operation, ProgramState state)
    {
        var unwrapped = state.ResolveCaptureAndUnwrapConversion(operation);
        return unwrapped.AsDeclarationExpression()?.Expression ?? unwrapped;
    }

    private sealed record OperationValue(IOperation Operation, SymbolicValue Value);
}
