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

using Microsoft.CodeAnalysis.Operations;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.Test.TestFramework.SymbolicExecution;

internal class AddConstraintOnInvocationCheck : SymbolicCheck
{
    private static readonly SymbolicConstraint[] AvailableConstraints = { TestConstraint.First, BoolConstraint.True, DummyConstraint.Dummy, LockConstraint.Held };

    protected override ProgramState PostProcessSimple(SymbolicContext context) =>
        context.Operation.Instance is IInvocationOperation invocation
        && invocation.Instance.TrackedSymbol(context.State) is { } symbol
        && AvailableConstraints.FirstOrDefault(x => context.State[symbol] is null || !context.State[symbol].HasConstraint(x)) is { } nextConstraint
            ? context.SetSymbolConstraint(symbol, nextConstraint)
            : context.State;
}
