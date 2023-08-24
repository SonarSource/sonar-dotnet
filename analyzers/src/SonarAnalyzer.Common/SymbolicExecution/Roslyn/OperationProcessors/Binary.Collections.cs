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

internal sealed partial class Binary
{
    private ProgramState LearnBranchingCollectionConstraint(ProgramState state, IBinaryOperationWrapper binary, bool falseBranch)
    {
        ISymbol collection;
        bool countIsLeft;
        if (CountInstance(binary.LeftOperand) is { } symbol)
        {
            collection = symbol;
            countIsLeft = true;
        }
        else
        {
            collection = CountInstance(binary.RightOperand);
            countIsLeft = false;
        }

        return collection is not null
            && state.Constraint<NumberConstraint>(OtherOperator(binary, countIsLeft)) is { } number
            && BranchingCollectionConstraint(binary.OperatorKind, falseBranch, countIsLeft, number) is { } constraint
                ? state.SetSymbolConstraint(collection, constraint)
                : state;

        ISymbol CountInstance(IOperation operation) =>
            operation.AsPropertyReference() is { Instance: { } instance, Property.Name: nameof(Array.Length) or nameof(List<int>.Count) }
            && instance.TrackedSymbol(state) is { } symbol
                ? symbol
                : null;

        static IOperation OtherOperator(IBinaryOperationWrapper binary, bool countIsLeft) =>
            countIsLeft ? binary.RightOperand : binary.LeftOperand;
    }

    private static SymbolicConstraint BranchingCollectionConstraint(BinaryOperatorKind operatorKind, bool falseBranch, bool countIsLeft, NumberConstraint number) =>
        BranchingCollectionConstraintFromEquals(operatorKind, number, falseBranch)
        ?? BranchingCollectionConstraintFromRelational(operatorKind, number, falseBranch, countIsLeft);

    private static SymbolicConstraint BranchingCollectionConstraintFromEquals(BinaryOperatorKind operatorKind, NumberConstraint number, bool falseBranch)
    {
        if (operatorKind.IsAnyEquality())
        {
            var isNotEquals = falseBranch ^ operatorKind.IsNotEquals();
            if (number.Min > 0 && !isNotEquals)
            {
                return CollectionConstraint.NotEmpty;                            // list.Count == 5
            }
            else if (number.Max == 0)
            {
                return CollectionConstraint.Empty.ApplyOpposite(isNotEquals);    // list.Count == 0
            }
        }
        return null;
    }

    private static SymbolicConstraint BranchingCollectionConstraintFromRelational(BinaryOperatorKind operatorKind, NumberConstraint number, bool falseBranch, bool countIsLeft)
    {
        if (operatorKind.IsAnyRelational())
        {
            if (CountBiggerNumber())
            {
                // list.Count > 0  true
                // list.Count >= 1 true
                // list.Count < 1  false
                // list.Count <= 0 false
                if (number.Min >= Threshold(true))
                {
                    return CollectionConstraint.NotEmpty;
                }
            }
            else if (number.Max == Threshold(false))
            {
                // list.Count > 0  false
                // list.Count >= 1 false
                // list.Count < 1  true
                // list.Count <= 0 true
                return CollectionConstraint.Empty;
            }
        }
        return null;

        bool CountBiggerNumber() =>
            // one of the three, or all of them
            countIsLeft ^ operatorKind is BinaryOperatorKind.LessThan or BinaryOperatorKind.LessThanOrEqual ^ falseBranch;

        int Threshold(bool countBiggerNumber) =>
            countBiggerNumber ^ falseBranch ^ operatorKind is BinaryOperatorKind.GreaterThanOrEqual or BinaryOperatorKind.LessThanOrEqual ? 0 : 1;
    }
}
