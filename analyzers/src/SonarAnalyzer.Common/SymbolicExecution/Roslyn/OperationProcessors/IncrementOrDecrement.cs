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

internal sealed class IncrementOrDecrement : SimpleProcessor<IIncrementOrDecrementOperationWrapper>
{
    protected override IIncrementOrDecrementOperationWrapper Convert(IOperation operation) =>
        IIncrementOrDecrementOperationWrapper.FromOperation(operation);

    protected override ProgramState Process(SymbolicContext context, IIncrementOrDecrementOperationWrapper incrementOrDecrement)
    {
        if (context.State[incrementOrDecrement.Target]?.Constraint<NumberConstraint>() is { } oldNumber)
        {
            NumberConstraint newNumber;
            var state = context.State;
            if (state.Constraint<FuzzyConstraint>(incrementOrDecrement.Target) is { } fuzzy && fuzzy.Source == incrementOrDecrement.WrappedOperation)
            {
                newNumber = incrementOrDecrement.WrappedOperation.Kind == OperationKindEx.Increment
                    ? NumberConstraint.From(oldNumber.Min + 1, null)
                    : NumberConstraint.From(null, oldNumber.Max - 1);
            }
            else
            {
                newNumber = incrementOrDecrement.WrappedOperation.Kind == OperationKindEx.Increment
                    ? NumberConstraint.From(oldNumber.Min + 1, oldNumber.Max + 1)
                    : NumberConstraint.From(oldNumber.Min - 1, oldNumber.Max - 1);
                if (incrementOrDecrement.Target.TrackedSymbol(state) is { } symbol)
                {
                    state = state.SetSymbolConstraint(symbol, new FuzzyConstraint(incrementOrDecrement.WrappedOperation));
                }
            }
            if (newNumber is not null)
            {
                state = incrementOrDecrement.Target.TrackedSymbol(state) is { } symbol
                    ? state.SetSymbolConstraint(symbol, newNumber)
                    : state;
                return incrementOrDecrement.IsPostfix
                    ? state.SetOperationConstraint(incrementOrDecrement, oldNumber)
                    : state.SetOperationConstraint(incrementOrDecrement, newNumber);
            }
            else
            {
                state = incrementOrDecrement.Target.TrackedSymbol(state) is { } symbol
                    && state[symbol] is { } symbolValue
                        ? state.SetSymbolValue(symbol, symbolValue.WithoutConstraint<NumberConstraint>())
                        : state;
                return incrementOrDecrement.IsPostfix
                    ? state.SetOperationConstraint(incrementOrDecrement, oldNumber)
                    : state.SetOperationValue(incrementOrDecrement, state[incrementOrDecrement].WithoutConstraint<NumberConstraint>());
            }
        }
        return context.State;
    }
}
