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
            _ => ProcessArgumentAttributes(state, invocation),
        };
    }

    private static ProgramState[] ProcessArgumentAttributes(ProgramState state, IInvocationOperationWrapper invocation)
    {
        var states = state.ToArray();
        foreach (var argument in invocation.Arguments.Select(x => x.ToArgument()).Where(x => x.Parameter is not null))
        {
            foreach (var attribute in argument.Parameter.GetAttributes())
            {
                states = ProcessArgumentAttribute(states, invocation, argument, attribute);
            }
        }
        return states;
    }

    private static ProgramState[] ProcessArgumentAttribute(ProgramState[] states, IInvocationOperationWrapper invocation, IArgumentOperationWrapper argument, AttributeData attribute)
    {
        if (AttributeValue("NotNullWhenAttribute", "returnValue") is { } notNullWhenValue)
        {
            return states.SelectMany(x => ProcessIsNotNullWhen(x, invocation.WrappedOperation, argument, notNullWhenValue, false)).ToArray();
        }
        else if (AttributeValue("DoesNotReturnIfAttribute", "parameterValue") is { } doesNotReturnIfValue)
        {
            return states.SelectMany(x => ProcessDoesNotReturnIf(x, argument, doesNotReturnIfValue)).ToArray();
        }
        else
        {
            return states;
        }

        bool? AttributeValue(string attributeName, string valueName) =>
            attribute.HasName(attributeName) && attribute.TryGetAttributeValue<bool>(valueName, out var value) ? value : null;
    }

    private static ProgramState[] ProcessIsNotNullWhen(ProgramState state, IOperation invocation, IArgumentOperationWrapper argument, bool when, bool learnNull)
    {
        var whenBoolConstraint = BoolConstraint.From(when);
        return state[invocation]?.Constraint<BoolConstraint>() is { } existingBoolConstraint
            ? DefineConstraintsFromKnownResult().ToArray()
            : DefineAllConstraints();

        // There's a lot of room for improvement here to properly support cases with more than one attribute like TimeOnly.TryParseExact
        ProgramState DefineConstraintsFromKnownResult() =>
            existingBoolConstraint.Equals(when) && argument.WrappedOperation.TrackedSymbol() is { } argumentSymbol
                ? state.SetSymbolConstraint(argumentSymbol, ObjectConstraint.NotNull)
                : state;

        ProgramState[] DefineAllConstraints() =>
            state[argument.Value]?.Constraint<ObjectConstraint>() switch
            {
                ObjectConstraint constraint when constraint == ObjectConstraint.NotNull && argument.Parameter.RefKind == RefKind.None =>
                    state.ToArray(),                                                                 // The "normal" state handling reflects already what is going on.
                ObjectConstraint constraint when constraint == ObjectConstraint.Null && argument.Parameter.RefKind == RefKind.None =>
                    state.SetOperationConstraint(invocation, whenBoolConstraint.Opposite).ToArray(), // IsNullOrEmpty([NotNullWhen(false)] arg) returns true if arg is null
                _ when argument.WrappedOperation.TrackedSymbol() is { } argumentSymbol =>
                    ExplodeStates(argumentSymbol),
                _ => state.ToArray()
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
            : ProcessAssertedBoolSymbol(state, argument.Value, !when).ToArray();

    private static ProgramState[] ProcessDebugAssert(SymbolicContext context, IInvocationOperationWrapper invocation)
    {
        if (invocation.Arguments.IsEmpty)   // Defensive: User-defined useless method
        {
            return context.State.ToArray();
        }
        else
        {
            return invocation.Arguments[0].ToArgument().Value is var argumentValue
                && context.State[argumentValue] is { } value
                && value.HasConstraint(BoolConstraint.False)
                    ? EmptyStates
                    : ProcessAssertedBoolSymbol(context.State, argumentValue, false).ToArray();
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
                return context.SetOperationConstraint(BoolConstraint.True).ToArray();
            }
            else if (leftConstraint is not null && rightConstraint is not null)
            {
                return context.SetOperationConstraint(BoolConstraint.False).ToArray();
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
        return context.State.ToArray();
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
            ObjectConstraint constraint when constraint == ObjectConstraint.Null => context.SetOperationConstraint(BoolConstraint.True).ToArray(),
            ObjectConstraint constraint when constraint == ObjectConstraint.NotNull => context.SetOperationConstraint(BoolConstraint.False).ToArray(),
            _ when invocation.Arguments[0].ToArgument().Value.UnwrapConversion().Type is { } type && !type.CanBeNull() => context.SetOperationConstraint(BoolConstraint.False).ToArray(),
            _ when invocation.Arguments[0].TrackedSymbol() is { } argumentSymbol => new[]
            {
                        context.SetOperationConstraint(BoolConstraint.True).SetSymbolConstraint(argumentSymbol, ObjectConstraint.Null),
                        context.SetOperationConstraint(BoolConstraint.False).SetSymbolConstraint(argumentSymbol, ObjectConstraint.NotNull),
            },
            _ => context.State.ToArray()
        };
}
