/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Linq;
using SonarAnalyzer.SymbolicExecution.Constraints;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

internal sealed partial class Invocation
{
    private static ProgramState[] ProcessLinqEnumerableAndQueryable(SymbolicContext context, IInvocationOperationWrapper invocation)
    {
        switch (invocation.TargetMethod.Name)
        {
            case "Append":
            case nameof(Enumerable.AsEnumerable):
            case nameof(Queryable.AsQueryable):
            case nameof(Enumerable.Cast):
            case "Chunk":
            case nameof(Enumerable.Concat):
            case nameof(Enumerable.DefaultIfEmpty):
            case nameof(Enumerable.Distinct):
            case "DistinctBy":
            case nameof(Enumerable.Empty):
            case nameof(Enumerable.Except):
            case "ExceptBy":
            case nameof(Enumerable.GroupBy):
            case nameof(Enumerable.GroupJoin):
            case nameof(Enumerable.Intersect):
            case "IntersectBy":
            case nameof(Enumerable.Join):
            case nameof(Enumerable.OfType):
            case nameof(Enumerable.OrderBy):
            case nameof(Enumerable.OrderByDescending):
            case "Prepend":
            case nameof(Enumerable.Range):
            case nameof(Enumerable.Repeat):
            case nameof(Enumerable.Reverse):
            case nameof(Enumerable.Select):
            case nameof(Enumerable.SelectMany):
            case nameof(Enumerable.Skip):
            case "SkipLast":
            case nameof(Enumerable.SkipWhile):
            case nameof(Enumerable.Take):
            case "TakeLast":
            case nameof(Enumerable.TakeWhile):
            case nameof(Enumerable.ThenBy):
            case nameof(Enumerable.ThenByDescending):
            case nameof(Enumerable.ToArray):
            case nameof(Enumerable.ToDictionary):
            case "ToHashSet":
            case nameof(Enumerable.ToList):
            case nameof(Enumerable.ToLookup):
            case nameof(Enumerable.Union):
            case "UnionBy":
            case nameof(Enumerable.Where):
            case nameof(Enumerable.Zip):
                return new[] { context.SetOperationConstraint(ObjectConstraint.NotNull) };

            // ElementAtOrDefault is intentionally not supported. It's causing many FPs
            case nameof(Enumerable.FirstOrDefault):
            case nameof(Enumerable.LastOrDefault):
            case nameof(Enumerable.SingleOrDefault):
                return invocation.TargetMethod.ReturnType.IsReferenceType
                    ? new[]
                    {
                        context.SetOperationConstraint(ObjectConstraint.Null),
                        context.SetOperationConstraint(ObjectConstraint.NotNull),
                    }
                    : new[] { context.State };

            default:
                return new[] { context.State };
        }
    }
}
