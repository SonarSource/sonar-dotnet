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

namespace SonarAnalyzer.SymbolicExecution.Roslyn.Checks;

internal sealed class NonNullableValueTypeCheck : SymbolicCheck
{
    protected override ProgramState PostProcessSimple(SymbolicContext context)
    {
        var state = context.State;
        var operation = context.Operation.Instance;
        if (operation.Type is { } type && (type.IsNonNullableValueType() || type.IsEnum()))
        {
            state = context.SetOperationConstraint(ObjectConstraint.NotNull);
        }
        if (operation.TrackedSymbol(state) is { } trackedSymbol
            && trackedSymbol.GetSymbolType() is { } symbol
            && (symbol.IsNonNullableValueType() || symbol.IsEnum()))
        {
            state = state.SetSymbolConstraint(trackedSymbol, ObjectConstraint.NotNull);
        }
        return state;
    }
}
