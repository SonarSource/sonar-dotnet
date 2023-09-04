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
        bool countIsLeftOperand;
        if (InstanceOfCountPropertyReference(binary.LeftOperand) is { } symbol)
        {
            collection = symbol;
            countIsLeftOperand = true;
        }
        else
        {
            collection = InstanceOfCountPropertyReference(binary.RightOperand);
            countIsLeftOperand = false;
        }

        return collection is not null
            && state.Constraint<NumberConstraint>(OtherOperator(binary, countIsLeftOperand)) is { } number
            && BranchingCollectionConstraint(binary.OperatorKind, falseBranch, countIsLeftOperand, number) is { } constraint
                ? state.SetSymbolConstraint(collection, constraint)
                : state;

        ISymbol InstanceOfCountPropertyReference(IOperation operation) =>
            operation.AsPropertyReference() is { Instance: { } instance, Property.Name: nameof(Array.Length) or nameof(List<int>.Count) }
            && instance.TrackedSymbol(state) is { } symbol
                ? symbol
                : null;

        static IOperation OtherOperator(IBinaryOperationWrapper binary, bool countIsLeftOperand) =>
            countIsLeftOperand ? binary.RightOperand : binary.LeftOperand;
    }

    private static SymbolicConstraint BranchingCollectionConstraint(BinaryOperatorKind operatorKind, bool falseBranch, bool countIsLeftOperand, NumberConstraint number) =>
        BranchingCollectionConstraintFromEquals(operatorKind, number, falseBranch)
        ?? BranchingCollectionConstraintFromRelational(operatorKind, number, falseBranch, countIsLeftOperand);

    private static SymbolicConstraint BranchingCollectionConstraintFromEquals(BinaryOperatorKind operatorKind, NumberConstraint number, bool falseBranch)
    {
        if (operatorKind.IsAnyEquality())
        {
            var operandsAreEqual = operatorKind.IsEquals() ^ falseBranch;
            if (number.Min > 0 && operandsAreEqual)                                 // list.Count == 5
            {
                return CollectionConstraint.NotEmpty;
            }
            else if (number.Max == 0)                                               // list.Count == 0
            {
                return CollectionConstraint.Empty.ApplyOpposite(!operandsAreEqual);
            }
        }
        return null;
    }

    private static SymbolicConstraint BranchingCollectionConstraintFromRelational(BinaryOperatorKind operatorKind, NumberConstraint number, bool falseBranch, bool countIsLeftOperand)
    {
        if ((CountIsGreaterThanOtherOperand() && number.Min >= 0)
            || (CountIsGreaterThanOrEqualsOtherOperand() && number.Min >= 1))
        {
            return CollectionConstraint.NotEmpty;
        }
        else if ((CountIsLessThanOtherOperand() && number.Max == 1)
            || (CountIsLessThanOrEqualsOtherOperand() && number.Max == 0))
        {
            return CollectionConstraint.Empty;
        }
        return null;

        bool CountIsGreaterThanOtherOperand() =>
            operatorKind switch
            {
                BinaryOperatorKind.GreaterThan => countIsLeftOperand && !falseBranch,          // list.Count > x  is true
                BinaryOperatorKind.LessThan => !countIsLeftOperand && !falseBranch,            // x < list.Count  is true
                BinaryOperatorKind.GreaterThanOrEqual => !countIsLeftOperand && falseBranch,   // x >= list.Count is false
                BinaryOperatorKind.LessThanOrEqual => countIsLeftOperand && falseBranch,       // list.Count <= x is false
                _ => false
            };

        bool CountIsGreaterThanOrEqualsOtherOperand() =>
            operatorKind switch
            {
                BinaryOperatorKind.GreaterThan => !countIsLeftOperand && falseBranch,          // x > list.Count  is false
                BinaryOperatorKind.LessThan => countIsLeftOperand && falseBranch,              // list.Count < x  is false
                BinaryOperatorKind.GreaterThanOrEqual => countIsLeftOperand && !falseBranch,   // list.Count >= x is true
                BinaryOperatorKind.LessThanOrEqual => !countIsLeftOperand && !falseBranch,     // x <= list.Count is true
                _ => false
            };

        bool CountIsLessThanOtherOperand() =>
            operatorKind switch
            {
                BinaryOperatorKind.GreaterThan => !countIsLeftOperand && !falseBranch,         // x > list.Count  is true
                BinaryOperatorKind.LessThan => countIsLeftOperand && !falseBranch,             // list.Count < x  is true
                BinaryOperatorKind.GreaterThanOrEqual => countIsLeftOperand && falseBranch,    // list.Count >= x is false
                BinaryOperatorKind.LessThanOrEqual => !countIsLeftOperand && falseBranch,      // x <= list.Count is false
                _ => false
            };

        bool CountIsLessThanOrEqualsOtherOperand() =>
            operatorKind switch
            {
                BinaryOperatorKind.GreaterThan => countIsLeftOperand && falseBranch,           // list.Count > x  is false
                BinaryOperatorKind.LessThan => !countIsLeftOperand && falseBranch,             // x < list.Count  is false
                BinaryOperatorKind.GreaterThanOrEqual => !countIsLeftOperand && !falseBranch,  // x >= list.Count is true
                BinaryOperatorKind.LessThanOrEqual => countIsLeftOperand && !falseBranch,      // list.Count <= x is true
                _ => false
            };
    }
}
