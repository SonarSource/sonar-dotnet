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

internal sealed class TupleBinary : BinaryBase<ITupleBinaryOperationWrapper>
{
    protected override ITupleBinaryOperationWrapper Convert(IOperation operation) =>
        ITupleBinaryOperationWrapper.FromOperation(operation);

    protected override SymbolicConstraint BoolConstraintFromOperation(ProgramState state, ITupleBinaryOperationWrapper operation)
    {
        var boolConstraints = TupleElements(operation.LeftOperand, operation.RightOperand)
            .Select(x => BoolConstraintFromBinaryOperation(BinaryOperatorKind.Equals, state[x.Left], state[x.Right]))
            .ToArray();
        if (boolConstraints.Contains(BoolConstraint.False))
        {
            return operation.OperatorKind is BinaryOperatorKind.Equals
                ? BoolConstraint.False
                : BoolConstraint.True;
        }
        else if (Array.TrueForAll(boolConstraints, x => x == BoolConstraint.True))
        {
            return operation.OperatorKind == BinaryOperatorKind.Equals
                ? BoolConstraint.True
                : BoolConstraint.False;
        }
        else
        {
            return null;
        }
    }

    protected override ProgramState LearnBranchingConstraint(ProgramState state, ITupleBinaryOperationWrapper operation, bool falseBranch)
    {
        if ((operation.OperatorKind is BinaryOperatorKind.Equals) ^ falseBranch)
        {
            foreach (var pair in TupleElements(operation.LeftOperand, operation.RightOperand))
            {
                state = LearnBranchingEqualityConstraint(state, pair.Left, pair.Right, operation.OperatorKind, falseBranch);
            }
        }
        return state;
    }

    private static IEnumerable<Operands> TupleElements(IOperation leftSide, IOperation rightSide) =>
        leftSide.AsTuple() is { } leftTuple && rightSide.AsTuple() is { } rightTuple
            ? Enumerable.Zip(leftTuple.Elements, rightTuple.Elements, (left, right) => (left, right))
                .SelectMany(x => TupleElements(x.left, x.right))
            : [new Operands(leftSide, rightSide)];

    private sealed record Operands(IOperation Left, IOperation Right);
}
