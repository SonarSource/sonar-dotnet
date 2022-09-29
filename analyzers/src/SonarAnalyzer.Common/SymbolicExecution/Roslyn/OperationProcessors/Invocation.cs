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

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Constraints;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

internal sealed class Invocation : MultiProcessor<IInvocationOperationWrapper>
{
    protected override IInvocationOperationWrapper Convert(IOperation operation) =>
        IInvocationOperationWrapper.FromOperation(operation);

    protected override ProgramState[] Process(SymbolicContext context, IInvocationOperationWrapper invocation)
    {
        if (IsThrowHelper(invocation.TargetMethod))
        {
            return EmptyStates;
        }
        var state = context.State;
        if (!invocation.TargetMethod.IsStatic             // Also applies to C# extensions
            && !invocation.TargetMethod.IsExtensionMethod // VB extensions in modules are not marked as static
            && invocation.Instance.TrackedSymbol() is { } symbol)
        {
            state = state.SetSymbolConstraint(symbol, ObjectConstraint.NotNull);
        }
        if (invocation.HasThisReceiver())
        {
            state = state.ResetFieldConstraints();
        }
        return invocation switch
        {
            _ when invocation.TargetMethod.Is(KnownType.Microsoft_VisualBasic_Information, "IsNothing") => ProcessInformationIsNothing(context, invocation),
            _ when invocation.TargetMethod.Is(KnownType.System_Diagnostics_Debug, nameof(Debug.Assert)) => ProcessDebugAssert(context, invocation),
            _ when invocation.TargetMethod.ContainingType.IsAny(KnownType.System_Linq_Enumerable, KnownType.System_Linq_Queryable) => ProcessLinqEnumerableAndQueryable(context, invocation),
            _ when invocation.TargetMethod.IsAny(KnownType.System_String, nameof(string.IsNullOrEmpty), nameof(string.IsNullOrWhiteSpace)) => ProcessStringIsNullOrEmpty(context, invocation),
            _ => new[] { state }
        };
    }

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

    private static ProgramState[] ProcessStringIsNullOrEmpty(SymbolicContext context, IInvocationOperationWrapper invocation) =>
        context.State[invocation.Arguments[0].ToArgument().Value]?.Constraint<ObjectConstraint>() switch
        {
            ObjectConstraint constraint when constraint == ObjectConstraint.NotNull => new[] { context.State },    // The "normal" state handling reflects already what is going on.
            ObjectConstraint constraint when constraint == ObjectConstraint.Null => new[] { context.SetOperationConstraint(BoolConstraint.True) }, // IsNullOrEmpty(arg) returns true if arg is null
            _ when invocation.Arguments[0].TrackedSymbol() is { } argumentSymbol => new[]       // Explode the known states, these methods can create.
            {
                        context.SetOperationConstraint(BoolConstraint.True).SetSymbolConstraint(argumentSymbol, ObjectConstraint.Null),
                        context.SetOperationConstraint(BoolConstraint.True).SetSymbolConstraint(argumentSymbol, ObjectConstraint.NotNull),
                        context.SetOperationConstraint(BoolConstraint.False).SetSymbolConstraint(argumentSymbol, ObjectConstraint.NotNull),
            },
            _ => new[] { context.State }
        };

    private ProgramState[] ProcessDebugAssert(SymbolicContext context, IInvocationOperationWrapper invocation)
    {
        if (invocation.Arguments.IsEmpty)   // Defensive: User-defined useless method
        {
            return new[] { context.State };
        }
        else
        {
            return invocation.Arguments[0].ToArgument().Value is var argumentValue
                && context.State[argumentValue] is { } value
                && value.HasConstraint(BoolConstraint.False)
                    ? EmptyStates
                    : new[] { ProcessDebugAssertBoolSymbol(context.State, argumentValue, false) };
        }
    }

    private ProgramState ProcessDebugAssertBoolSymbol(ProgramState state, IOperation operation, bool isNegated)
    {
        if (operation.Kind == OperationKindEx.Unary && IUnaryOperationWrapper.FromOperation(operation) is { OperatorKind: UnaryOperatorKind.Not } unaryNot)
        {
            return ProcessDebugAssertBoolSymbol(state, unaryNot.Operand, !isNegated);
        }
        else
        {
            return operation.TrackedSymbol() is { } symbol
                ? state.SetSymbolConstraint(symbol, BoolConstraint.From(!isNegated))
                : state;
        }
    }

    private static bool IsThrowHelper(IMethodSymbol method) =>
        method.Is(KnownType.System_Diagnostics_Debug, nameof(Debug.Fail))
        || method.IsAny(KnownType.System_Environment, nameof(Environment.FailFast), nameof(Environment.Exit))
        || method.GetAttributes().Any(x => x.HasAnyName(
                                                "DoesNotReturnAttribute",       // https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.doesnotreturnattribute
                                                "TerminatesProgramAttribute")); // https://www.jetbrains.com/help/resharper/Reference__Code_Annotation_Attributes.html#TerminatesProgramAttribute

    private static ProgramState[] ProcessInformationIsNothing(SymbolicContext context, IInvocationOperationWrapper invocation) =>
        context.State[invocation.Arguments[0].ToArgument().Value]?.Constraint<ObjectConstraint>() switch
        {
            ObjectConstraint constraint when constraint == ObjectConstraint.Null => new[] { context.SetOperationConstraint(BoolConstraint.True) },
            ObjectConstraint constraint when constraint == ObjectConstraint.NotNull => new[] { context.SetOperationConstraint(BoolConstraint.False) },
            _ when invocation.Arguments[0].ToArgument().Value.UnwrapConversion().Type is { } type && !type.CanBeNull() => new[] { context.SetOperationConstraint(BoolConstraint.False) },
            _ when invocation.Arguments[0].TrackedSymbol() is { } argumentSymbol => new[]
            {
                        context.SetOperationConstraint(BoolConstraint.True).SetSymbolConstraint(argumentSymbol, ObjectConstraint.Null),
                        context.SetOperationConstraint(BoolConstraint.False).SetSymbolConstraint(argumentSymbol, ObjectConstraint.NotNull),
            },
            _ => new[] { context.State }
        };
}
