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

internal sealed partial class Binary
{
    private ProgramState LearnBranchingCollectionConstraint(ProgramState state, IBinaryOperationWrapper binary, bool falseBranch)
    {
        var operatorKind = falseBranch ? Opposite(binary.OperatorKind) : binary.OperatorKind;
        IOperation otherOperand;
        if (InstanceOfCountProperty(binary.LeftOperand) is { } collection)
        {
            otherOperand = binary.RightOperand;
        }
        else
        {
            otherOperand = binary.LeftOperand;
            operatorKind = Flip(operatorKind);
            collection = InstanceOfCountProperty(binary.RightOperand);
        }

        return collection is not null
            && state.Constraint<NumberConstraint>(otherOperand) is { } number
            && CollectionConstraintFromOperator(operatorKind, number) is { } constraint
                ? state.SetSymbolConstraint(collection, constraint)
                : state;

        ISymbol InstanceOfCountProperty(IOperation operation) =>
            operation.AsPropertyReference() is { Instance: { } instance, Property.Name: nameof(Array.Length) or nameof(List<int>.Count) }
            && instance.Type.DerivesOrImplementsAny(CollectionTracker.CollectionTypes)
            && instance.TrackedSymbol(state) is { } symbol
                ? symbol
                : null;
    }

    private static SymbolicConstraint CollectionConstraintFromOperator(BinaryOperatorKind operatorKind, NumberConstraint number) =>
        // consider count to be the left operand and the comparison to resolve to true:
        operatorKind switch
        {
            _ when operatorKind.IsEquals() && number.Min > 0 => CollectionConstraint.NotEmpty,
            _ when operatorKind.IsEquals() && number.Max == 0 => CollectionConstraint.Empty,
            _ when operatorKind.IsNotEquals() && number.Max == 0 => CollectionConstraint.NotEmpty,
            BinaryOperatorKind.GreaterThan when number.Min >= 0 => CollectionConstraint.NotEmpty,
            BinaryOperatorKind.GreaterThanOrEqual when number.Min >= 1 => CollectionConstraint.NotEmpty,
            BinaryOperatorKind.LessThan when number.Max == 1 => CollectionConstraint.Empty,
            BinaryOperatorKind.LessThanOrEqual when number.Max == 0 => CollectionConstraint.Empty,
            _ => null
        };
}
