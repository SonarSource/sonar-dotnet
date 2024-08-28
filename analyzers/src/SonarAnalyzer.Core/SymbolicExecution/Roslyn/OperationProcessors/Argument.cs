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

internal sealed class Argument : SimpleProcessor<IArgumentOperationWrapper>
{
    protected override IArgumentOperationWrapper Convert(IOperation operation) =>
        IArgumentOperationWrapper.FromOperation(operation);

    protected override ProgramState Process(SymbolicContext context, IArgumentOperationWrapper argument) =>
        ProcessArgument(context.State, argument) ?? context.State;

    private static ProgramState ProcessArgument(ProgramState state, IArgumentOperationWrapper argument)
    {
        if (argument.Parameter is null)
        {
            return null; // __arglist is not assigned to a parameter
        }
        if (argument is { Parameter.RefKind: RefKind.Out or RefKind.Ref } && argument.Value.TrackedSymbol(state) is { } refOutSymbol)
        {
            state = state.SetSymbolValue(refOutSymbol, null); // Forget state for "out" or "ref" arguments
        }
        if (argument.Parameter.HasNotNullAttribute() && argument.Value.TrackedSymbol(state) is { } notNullSymbol)
        {
            state = state.SetSymbolConstraint(notNullSymbol, ObjectConstraint.NotNull);
        }
        // Enumerable is static class with extensions, so the instance is passed as argument, but it does not get mutated
        if (!argument.Parameter.ContainingType.Is(KnownType.System_Linq_Enumerable)
            && argument.WrappedOperation.TrackedSymbol(state) is { } symbol
            && state[symbol] is { } symbolValue)
        {
            state = state.SetSymbolValue(symbol, symbolValue.WithoutConstraint<CollectionConstraint>());
        }
        return state.SetOperationValue(argument, state[argument.Value]);
    }
}
