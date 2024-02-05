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

namespace SonarAnalyzer.SymbolicExecution.Roslyn;

/// <summary>
/// Lifespan of this class is one method analyzed with SE.
/// </summary>
public class SymbolicCheck
{
    /// <summary>
    /// Stop processing this branch of the exploded graph. There will be no follow up states.
    /// </summary>
    protected static readonly ProgramStates EmptyStates = new();

    protected SymbolicCheck() { } // Avoid abstract class, fixes S1694

    public virtual ProgramState ConditionEvaluated(SymbolicContext context) =>
        context.State;

    /// <summary>
    /// Override this if you need to return multiple states.
    /// </summary>
    public virtual ProgramStates PreProcess(SymbolicContext context) =>
        PreProcessSimple(context) is { } newState ? new(newState) : EmptyStates;

    /// <summary>
    /// Override this if you need to return multiple states.
    /// </summary>
    public virtual ProgramStates PostProcess(SymbolicContext context) =>
        PostProcessSimple(context) is { } newState ? new(newState) : EmptyStates;

    /// <summary>
    /// Method is invoked for each execution flow that reaches exit block. Once for each unique state after LVA cleanup.
    /// </summary>
    public virtual void ExitReached(SymbolicContext context) { }

    /// <summary>
    /// Method is invoked once for analyzed CFG.
    /// </summary>
    public virtual void ExecutionCompleted() { }

    /// <summary>
    /// Override this if you need to return a single state or null to stop the execution.
    /// </summary>
    protected virtual ProgramState PreProcessSimple(SymbolicContext context) =>
        context.State;

    /// <summary>
    /// Override this if you need to return a single state or null to stop the execution.
    /// </summary>
    protected virtual ProgramState PostProcessSimple(SymbolicContext context) =>
        context.State;
}
