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

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

internal interface IMultiProcessor
{
    public ProgramStates Process(SymbolicContext context);
}

/// <summary>
/// Base class for operation processors - used when operation returns multiple ProgramStates.
/// See <see cref="SimpleProcessor{T}"/> if you need to return a single ProgramStates.
/// See <see cref="BranchingProcessor{T}"/> if you need to take a branching decision.
/// </summary>
internal abstract class MultiProcessor<T> : Processor<T>, IMultiProcessor
    where T : IOperationWrapper
{
    protected abstract ProgramStates Process(SymbolicContext context, T operation);

    public ProgramStates Process(SymbolicContext context) =>
        Process(context, Convert(context.Operation.Instance));
}
