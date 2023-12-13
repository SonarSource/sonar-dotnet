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

using OperationValueList = System.Collections.Generic.List<(Microsoft.CodeAnalysis.IOperation TupleMember, SonarAnalyzer.SymbolicExecution.Roslyn.SymbolicValue Value)>;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

internal sealed class DeconstructionAssignment : SimpleProcessor<IDeconstructionAssignmentOperationWrapper>
{
    protected override IDeconstructionAssignmentOperationWrapper Convert(IOperation operation) =>
        IDeconstructionAssignmentOperationWrapper.FromOperation(operation);

    protected override ProgramState Process(SymbolicContext context, IDeconstructionAssignmentOperationWrapper assignment)
    {
        var list = new OperationValueList();
        CollectOperationValues(context.State, assignment.Target, assignment.Value, list);
        var newState = context.State;
        foreach (var (tupleMember, value) in list)
        {
            newState = newState.SetOperationAndSymbolValue(tupleMember, value);
        }
        return newState;
    }

    private static void CollectOperationValues(ProgramState state, IOperation target, IOperation value, OperationValueList operationValues)
    {
        var leftTupleElements = TupleElements(target);
        // If the right side is a tuple, then every symbol/constraint is copied to the left side.
        if (Unwrap(value).AsTuple() is { } rightSideTuple)
        {
            var rightTupleElements = TupleElements(rightSideTuple.WrappedOperation);
            for (var i = 0; i < leftTupleElements.Length; i++)
            {
                var leftTupleMember = leftTupleElements[i];
                var rightTupleMember = rightTupleElements[i];
                if (leftTupleMember.Kind == OperationKindEx.Discard)
                {
                    continue;
                }
                if (leftTupleMember.AsTuple() is { } nestedTuple)
                {
                    var rightSideMember = rightTupleMember.AsTuple()?.WrappedOperation ?? rightTupleMember;
                    CollectOperationValues(state, nestedTuple.WrappedOperation, rightSideMember, operationValues);
                }
                else
                {
                    operationValues.Add((leftTupleMember, state[rightTupleMember]));
                }
            }
        }
        // If the right side is not a tuple, then every member of the left side tuple is set to empty.
        else
        {
            foreach (var tupleMember in leftTupleElements.Where(x => x.Kind != OperationKindEx.Discard))
            {
                operationValues.Add((tupleMember, SymbolicValue.Empty));
            }
        }
    }

    private static IOperation[] TupleElements(IOperation operation) =>
        Unwrap(operation)
            .ToTuple()
            .Elements
            .Select(Unwrap)
            .ToArray();

    private static IOperation Unwrap(IOperation operation)
    {
        var unwrapped = operation.UnwrapConversion();
        return unwrapped.AsDeclarationExpression()?.Expression ?? unwrapped;
    }
}
