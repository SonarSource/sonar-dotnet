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

using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.Test.TestFramework.SymbolicExecution;

public class PreserveTestCheck : SymbolicCheck
{
    private readonly HashSet<string> symbolNames;

    public PreserveTestCheck(params string[] symbolNames)
    {
        if (symbolNames.Length == 0)
        {
            throw new ArgumentException("Value cannot be empty", nameof(symbolNames));
        }
        this.symbolNames = new(symbolNames);
    }

    protected override ProgramState PreProcessSimple(SymbolicContext context) =>
        context.Operation.Instance.TrackedSymbol(context.State) is { } symbol && symbolNames.Contains(symbol.Name)
            ? context.State.Preserve(symbol)
            : context.State;
}
