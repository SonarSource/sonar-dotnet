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

internal sealed partial class Invocation
{
    private static readonly HashSet<string> StringMethodReturningNotNull = new()
    {
        nameof(string.Clone),
        nameof(string.Concat),
        nameof(string.Copy),
        "Create",
        nameof(string.Format),
        nameof(string.GetEnumerator),
        "GetPinnableReference",
        nameof(string.Insert),
        nameof(string.Intern),
        nameof(string.Join),
        nameof(string.Normalize),
        nameof(string.PadLeft),
        nameof(string.PadRight),
        nameof(string.Remove),
        nameof(string.Replace),
        "ReplaceLineEndings",
        nameof(string.Split),
        nameof(string.Substring),
        nameof(string.ToCharArray),
        nameof(string.ToLower),
        nameof(string.ToLowerInvariant),
        nameof(string.ToString),
        nameof(string.ToUpper),
        nameof(string.ToUpperInvariant),
        nameof(string.Trim),
        nameof(string.TrimEnd),
        nameof(string.TrimStart)
    };

    private static ProgramStates ProcessSystemStringInvocation(ProgramState state, IInvocationOperationWrapper invocation)
    {
        if (invocation.TargetMethod.Name is nameof(string.IsNullOrEmpty) or nameof(string.IsNullOrWhiteSpace))
        {
            return ProcessIsNotNullWhen(state, invocation.WrappedOperation, invocation.Arguments[0].ToArgument(), false, true);
        }
        else if (StringMethodReturningNotNull.Contains(invocation.TargetMethod.Name))
        {
            return new(state.SetOperationConstraint(invocation, ObjectConstraint.NotNull));
        }
        else
        {
            return new(state);
        }
    }
}
