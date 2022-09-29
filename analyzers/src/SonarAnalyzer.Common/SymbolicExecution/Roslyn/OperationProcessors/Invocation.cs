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

internal sealed partial class Invocation : MultiProcessor<IInvocationOperationWrapper>
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
            _ when invocation.TargetMethod.Name == nameof(object.Equals) => ProcessEquals(context, invocation),
            _ when invocation.TargetMethod.IsAny(KnownType.System_String, nameof(string.IsNullOrEmpty), nameof(string.IsNullOrWhiteSpace)) =>
                ProcessIsNotNullWhen(state, invocation.WrappedOperation, invocation.Arguments[0].ToArgument(), false, true),
            _ => ProcessIsNotNullWhen(state, invocation),
        };
    }

    private static ProgramState[] ProcessIsNotNullWhen(ProgramState state, IInvocationOperationWrapper invocation)
    {
        foreach (var argument in invocation.Arguments.Select(x => x.ToArgument())) // TODO: support attributes on more than one argument
        {
            if (argument.Parameter?.GetAttributes() is { } attributes)
            {
                if (attributes.FirstOrDefault(x => x.HasName("NotNullWhenAttribute")) is { } notNullWhenAttribute
                    && notNullWhenAttribute.TryGetAttributeValue<bool>("returnValue", out var returnValue))
                {
                    return ProcessIsNotNullWhen(state, invocation.WrappedOperation, argument, returnValue, false);
                }
                else if(attributes.FirstOrDefault(x => x.HasName("DoesNotReturnIfAttribute")) is { } doesNotReturnIfAttribute
                    && doesNotReturnIfAttribute.TryGetAttributeValue<bool>("condition", out var condition))
                {
                    // FIXME: Rebase conflict, do not return but loop instead
                    return ProcessDoesNotReturnIf(state, argument, condition);
                }
            }
        }
        return new[] { state };
    }

    private static ProgramState[] ProcessIsNotNullWhen(ProgramState state, IOperation invocation, IArgumentOperationWrapper argument, bool when, bool learnNull)
    {
        var whenBoolConstraint = BoolConstraint.From(when);
        return state[argument.Value]?.Constraint<ObjectConstraint>() switch
        {
            ObjectConstraint constraint when constraint == ObjectConstraint.NotNull && argument.Parameter.RefKind == RefKind.None =>
                new[] { state },                                                                 // The "normal" state handling reflects already what is going on.
            ObjectConstraint constraint when constraint == ObjectConstraint.Null && argument.Parameter.RefKind == RefKind.None =>
                new[] { state.SetOperationConstraint(invocation, whenBoolConstraint.Opposite) }, // IsNullOrEmpty([NotNullWhen(false)] arg) returns true if arg is null
            _ when argument.WrappedOperation.TrackedSymbol() is { } argumentSymbol =>
                ExplodeStates(argumentSymbol),
            _ => new[] { state }
        };

        ProgramState[] ExplodeStates(ISymbol argumentSymbol) =>
            learnNull
                ? new[]
                    {
                        state.SetOperationConstraint(invocation, whenBoolConstraint).SetSymbolConstraint(argumentSymbol, ObjectConstraint.NotNull),
                        state.SetOperationConstraint(invocation, whenBoolConstraint.Opposite).SetSymbolConstraint(argumentSymbol, ObjectConstraint.Null),
                        state.SetOperationConstraint(invocation, whenBoolConstraint.Opposite).SetSymbolConstraint(argumentSymbol, ObjectConstraint.NotNull),
                    }
                : new[]
                    {
                        state.SetOperationConstraint(invocation, whenBoolConstraint).SetSymbolConstraint(argumentSymbol, ObjectConstraint.NotNull),
                        state.SetOperationConstraint(invocation, whenBoolConstraint.Opposite),
                    };
    }

    private static ProgramState[] ProcessDoesNotReturnIf(ProgramState state, IArgumentOperationWrapper argument, bool when) =>
        state[argument.Value] is { } argumentValue && argumentValue.HasConstraint(BoolConstraint.From(when))
            ? EmptyStates
            : new[] { ProcessAssertedBoolSymbol(state, argument.Value, !when) };

    private static ProgramState[] ProcessDebugAssert(SymbolicContext context, IInvocationOperationWrapper invocation)
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
                    : new[] { ProcessAssertedBoolSymbol(context.State, argumentValue, false) };
        }
    }

    private static ProgramState ProcessAssertedBoolSymbol(ProgramState state, IOperation operation, bool isNegated)
    {
        if (operation.Kind == OperationKindEx.Unary && IUnaryOperationWrapper.FromOperation(operation) is { OperatorKind: UnaryOperatorKind.Not } unaryNot)
        {
            return ProcessAssertedBoolSymbol(state, unaryNot.Operand, !isNegated);
        }
        else
        {
            return operation.TrackedSymbol() is { } symbol
                ? state.SetSymbolConstraint(symbol, BoolConstraint.From(!isNegated))
                : state;
        }
    }

    private static ProgramState[] ProcessEquals(SymbolicContext context, IInvocationOperationWrapper invocation)
    {
        if (invocation.TargetMethod.IsStatic && invocation.Arguments.Length == 2
            && invocation.Arguments[0].ToArgument().Value is var leftOperation
            && invocation.Arguments[1].ToArgument().Value is var rightOperation
            && context.State[leftOperation]?.Constraint<ObjectConstraint>() is var leftConstraint
            && context.State[rightOperation]?.Constraint<ObjectConstraint>() is var rightConstraint
            && (leftConstraint == ObjectConstraint.Null || rightConstraint == ObjectConstraint.Null))
        {
            if (leftConstraint == ObjectConstraint.Null && rightConstraint == ObjectConstraint.Null)
            {
                return new[] { context.SetOperationConstraint(BoolConstraint.True) };
            }
            else if (leftConstraint is not null && rightConstraint is not null)
            {
                return new[] { context.SetOperationConstraint(BoolConstraint.False) };
            }
            else if ((leftConstraint == ObjectConstraint.Null ? rightOperation : leftOperation).TrackedSymbol() is { } symbol)
            {
                return new[]
                {
                    context.SetOperationConstraint(BoolConstraint.True).SetSymbolConstraint(symbol, ObjectConstraint.Null),
                    context.SetOperationConstraint(BoolConstraint.False).SetSymbolConstraint(symbol, ObjectConstraint.NotNull)
                };
            }
        }
        return new[] { context.State };
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
