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
    private static readonly HashSet<string> ReturningNotNull = new()
    {
        nameof(Enumerable.Append),
        nameof(Enumerable.AsEnumerable),
        nameof(Queryable.AsQueryable),
        nameof(Enumerable.Cast),
        "Chunk",
        nameof(Enumerable.Concat),
        nameof(Enumerable.DefaultIfEmpty),
        nameof(Enumerable.Distinct),
        "DistinctBy",
        nameof(Enumerable.Empty),
        nameof(Enumerable.Except),
        "ExceptBy",
        nameof(Enumerable.GroupBy),
        nameof(Enumerable.GroupJoin),
        nameof(Enumerable.Intersect),
        "IntersectBy",
        nameof(Enumerable.Join),
        nameof(Enumerable.OfType),
        nameof(Enumerable.OrderBy),
        nameof(Enumerable.OrderByDescending),
        nameof(Enumerable.Prepend),
        nameof(Enumerable.Range),
        nameof(Enumerable.Repeat),
        nameof(Enumerable.Reverse),
        nameof(Enumerable.Select),
        nameof(Enumerable.SelectMany),
        nameof(Enumerable.Skip),
        "SkipLast",
        nameof(Enumerable.SkipWhile),
        nameof(Enumerable.Take),
        "TakeLast",
        nameof(Enumerable.TakeWhile),
        nameof(Enumerable.ThenBy),
        nameof(Enumerable.ThenByDescending),
        nameof(Enumerable.ToArray),
        nameof(Enumerable.ToDictionary),
        "ToHashSet",
        nameof(Enumerable.ToList),
        nameof(Enumerable.ToLookup),
        nameof(Enumerable.Union),
        "UnionBy",
        nameof(Enumerable.Where),
        nameof(Enumerable.Zip),
    };

    private static ProgramStates ProcessLinqEnumerableAndQueryable(ProgramState state, IInvocationOperationWrapper invocation)
    {
        var name = invocation.TargetMethod.Name;
        if (ReturningNotNull.Contains(name))
        {
            return new(state.SetOperationConstraint(invocation, ObjectConstraint.NotNull));
        }
        // ElementAtOrDefault is intentionally not supported. It's causing many FPs
        else if (name is nameof(Enumerable.FirstOrDefault) or nameof(Enumerable.LastOrDefault) or nameof(Enumerable.SingleOrDefault))
        {
            return invocation.TargetMethod.ReturnType.IsReferenceType
                ? new(state.SetOperationConstraint(invocation, ObjectConstraint.Null), state.SetOperationConstraint(invocation, ObjectConstraint.NotNull))
                : new(state);
        }
        else
        {
            return new(state);
        }
    }
}
