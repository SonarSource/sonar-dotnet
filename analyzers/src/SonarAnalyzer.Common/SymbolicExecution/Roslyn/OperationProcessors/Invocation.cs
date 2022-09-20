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

using System.Diagnostics;
using Microsoft.CodeAnalysis;
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
            _ when invocation.TargetMethod.IsAny(KnownType.System_String, nameof(string.IsNullOrEmpty), nameof(string.IsNullOrWhiteSpace)) => ProcessStringIsNullOrEmpty(context, invocation),
            _ when invocation.TargetMethod.Is(KnownType.System_Diagnostics_Debug, nameof(Debug.Assert)) => ProcessDebugAssert(context, invocation),
            _ => new[] { state }
        };
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

    private static ProgramState ProcessThrowHelper(ProgramState state, IInvocationOperationWrapper invocation)
    {
        if (KnownMethods.IsDebugFail(invocation.TargetMethod))
        {
            return ProgramState.Empty;
        }
        return state;
    }
}
