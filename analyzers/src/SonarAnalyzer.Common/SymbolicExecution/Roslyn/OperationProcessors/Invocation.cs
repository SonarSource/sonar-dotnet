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
using SonarAnalyzer.SymbolicExecution.Roslyn.Checks;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

internal sealed partial class Invocation : MultiProcessor<IInvocationOperationWrapper>
{
    protected override IInvocationOperationWrapper Convert(IOperation operation) =>
        IInvocationOperationWrapper.FromOperation(operation);

    protected override ProgramStates Process(SymbolicContext context, IInvocationOperationWrapper invocation)
    {
        if (IsThrowHelper(invocation.TargetMethod))
        {
            return new();
        }
        var state = context.State;
        if (invocation.TargetMethod.IsStatic                // Also applies to C# extensions
            || invocation.TargetMethod.IsExtensionMethod)   // VB extensions in modules are not marked as static
        {
            state = state.ResetStaticFieldConstraints(invocation.TargetMethod.ContainingType);
        }
        else if (invocation.Instance.TrackedSymbol(state) is { } symbol && !IsNullableGetValueOrDefault(invocation))
        {
            state = state.SetSymbolConstraint(symbol, ObjectConstraint.NotNull)
                .SetOperationConstraint(invocation.Instance, ObjectConstraint.NotNull);
        }
        if (invocation.HasThisReceiver(state))
        {
            state = state.ResetFieldConstraints();
        }
        if (invocation.TargetMethod.IsExtensionMethod
            && invocation.TargetMethod.ReducedFrom is { } reducedFrom   // VB reduces method symbol to 'instance.Extension()' without annotated ArgumentOperation
            && reducedFrom.Parameters.First().HasNotNullAttribute()
            && invocation.Instance.TrackedSymbol(state) is { } instanceSymbol)
        {
            state = state.SetSymbolConstraint(instanceSymbol, ObjectConstraint.NotNull);
        }
        state = CollectionTracker.LearnFrom(state, invocation);

        return invocation switch
        {
            _ when IsNullableGetValueOrDefault(invocation) => new(ProcessNullableGetValueOrDefault(context, invocation)),
            _ when invocation.TargetMethod.Is(KnownType.Microsoft_VisualBasic_Information, "IsNothing") => ProcessInformationIsNothing(context, invocation),
            _ when invocation.TargetMethod.Is(KnownType.System_Diagnostics_Debug, nameof(Debug.Assert)) => ProcessDebugAssert(context, invocation),
            _ when invocation.TargetMethod.Is(KnownType.System_Object, nameof(ReferenceEquals)) => ProcessReferenceEquals(context, invocation),
            _ when invocation.TargetMethod.Is(KnownType.System_Nullable_T, "get_HasValue") => ProcessNullableHasValue(state, invocation),
            _ when invocation.TargetMethod.ContainingType.IsAny(KnownType.System_Linq_Enumerable, KnownType.System_Linq_Queryable) => ProcessLinqEnumerableAndQueryable(state, invocation),
            _ when invocation.TargetMethod.Name == nameof(Equals) => ProcessEquals(context, invocation),
            _ when invocation.TargetMethod.ContainingType.Is(KnownType.System_String) => ProcessSystemStringInvocation(state, invocation),
            _ => ProcessArgumentAttributes(state, invocation),
        };
    }

    private static ProgramStates ProcessArgumentAttributes(ProgramState state, IInvocationOperationWrapper invocation)
    {
        var states = new ProgramStates(state);
        foreach (var argument in invocation.Arguments.Select(x => x.ToArgument()).Where(x => x.Parameter is not null))
        {
            foreach (var attribute in argument.Parameter.GetAttributes())
            {
                states = ProcessArgumentAttribute(states, invocation, argument, attribute);
            }
        }
        return states;
    }

    private static ProgramStates ProcessArgumentAttribute(ProgramStates states, IInvocationOperationWrapper invocation, IArgumentOperationWrapper argument, AttributeData attribute)
    {
        if (AttributeValue("NotNullWhenAttribute", "returnValue") is { } notNullWhenValue)
        {
            ProgramStates result = new();
            foreach (var state in states)
            {
                result += ProcessIsNotNullWhen(state, invocation.WrappedOperation, argument, notNullWhenValue, false);
            }
            return result;
        }
        else if (AttributeValue("DoesNotReturnIfAttribute", "parameterValue") is { } doesNotReturnIfValue)
        {
            ProgramStates result = new();
            foreach (var state in states)
            {
                result += ProcessDoesNotReturnIf(state, argument, doesNotReturnIfValue);
            }
            return result;
        }
        else
        {
            return states;
        }

        bool? AttributeValue(string attributeName, string valueName) =>
            attribute.HasName(attributeName) && attribute.TryGetAttributeValue<bool>(valueName, out var value) ? value : null;
    }

    private static ProgramStates ProcessIsNotNullWhen(ProgramState state, IOperation invocation, IArgumentOperationWrapper argument, bool when, bool learnNull)
    {
        var whenBoolConstraint = BoolConstraint.From(when);
        return state[invocation]?.Constraint<BoolConstraint>() is { } existingBoolConstraint
            ? new(DefineConstraintsFromKnownResult())
            : DefineAllConstraints();

        // There's a lot of room for improvement here to properly support cases with more than one attribute like TimeOnly.TryParseExact
        ProgramState DefineConstraintsFromKnownResult() =>
            existingBoolConstraint.Equals(when) && argument.WrappedOperation.TrackedSymbol(state) is { } argumentSymbol
                ? state.SetSymbolConstraint(argumentSymbol, ObjectConstraint.NotNull)
                : state;

        ProgramStates DefineAllConstraints() =>
            state[argument.Value]?.Constraint<ObjectConstraint>() switch
            {
                ObjectConstraint constraint when constraint == ObjectConstraint.NotNull && argument.Parameter.RefKind == RefKind.None =>
                    new(state),                                                                 // The "normal" state handling reflects already what is going on.
                ObjectConstraint constraint when constraint == ObjectConstraint.Null && argument.Parameter.RefKind == RefKind.None =>
                    new(state.SetOperationConstraint(invocation, whenBoolConstraint.Opposite)), // IsNullOrEmpty([NotNullWhen(false)] arg) returns true if arg is null
                _ when argument.WrappedOperation.TrackedSymbol(state) is { } argumentSymbol =>
                    ExplodeStates(argumentSymbol),
                _ => new(state)
            };

        ProgramStates ExplodeStates(ISymbol argumentSymbol) =>
            learnNull
                ? new(
                    state.SetOperationConstraint(invocation, whenBoolConstraint).SetSymbolConstraint(argumentSymbol, ObjectConstraint.NotNull),
                    state.SetOperationConstraint(invocation, whenBoolConstraint.Opposite).SetSymbolConstraint(argumentSymbol, ObjectConstraint.Null),
                    state.SetOperationConstraint(invocation, whenBoolConstraint.Opposite).SetSymbolConstraint(argumentSymbol, ObjectConstraint.NotNull))
                : new(
                    state.SetOperationConstraint(invocation, whenBoolConstraint).SetSymbolConstraint(argumentSymbol, ObjectConstraint.NotNull),
                    state.SetOperationConstraint(invocation, whenBoolConstraint.Opposite));
    }

    private static ProgramStates ProcessDoesNotReturnIf(ProgramState state, IArgumentOperationWrapper argument, bool when) =>
        state[argument.Value] is { } argumentValue && argumentValue.HasConstraint(BoolConstraint.From(when))
            ? new()
            : new(ProcessAssertedBoolSymbol(state, argument.Value, !when));

    private static ProgramStates ProcessDebugAssert(SymbolicContext context, IInvocationOperationWrapper invocation)
    {
        if (invocation.Arguments.IsEmpty)   // Defensive: User-defined useless method
        {
            return new(context.State);
        }
        else
        {
            return invocation.Arguments[0].ToArgument().Value is var argumentValue
                && context.State[argumentValue] is { } value
                && value.HasConstraint(BoolConstraint.False)
                    ? new()
                    : new(ProcessAssertedBoolSymbol(context.State, argumentValue, false));
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
            return operation.TrackedSymbol(state) is { } symbol
                ? state.SetSymbolConstraint(symbol, BoolConstraint.From(!isNegated)).SetSymbolConstraint(symbol, ObjectConstraint.NotNull)
                : state;
        }
    }

    private static ProgramStates ProcessReferenceEquals(SymbolicContext context, IInvocationOperationWrapper invocation) =>
        invocation.Arguments.Length == 2
            ? ProcessEqualsObject(context, invocation.Arguments[0].ToArgument().Value, invocation.Arguments[1].ToArgument().Value)
            : new(context.State);

    private static ProgramStates ProcessEquals(SymbolicContext context, IInvocationOperationWrapper invocation) =>
        invocation switch
        {
            { Arguments.Length: 2, TargetMethod.IsStatic: true } => ProcessEquals(context, invocation.Arguments[0].ToArgument().Value, invocation.Arguments[1].ToArgument().Value),
            { Arguments.Length: 1 } when IsSupportedEqualsType(invocation.TargetMethod.ContainingType) =>
                ProcessEquals(context, invocation.Instance, invocation.Arguments[0].ToArgument().Value),
            _ => new(context.State)
        };

    private static ProgramStates ProcessEquals(SymbolicContext context, IOperation leftOperation, IOperation rightOperation)
    {
        if (context.State.Constraint<BoolConstraint>(leftOperation) is { } leftBool
            && context.State.Constraint<BoolConstraint>(rightOperation) is { } rightBool)
        {
            return new(context.SetOperationConstraint(BoolConstraint.From(leftBool == rightBool)));
        }
        else if (context.State.Constraint<NumberConstraint>(leftOperation) is { } leftNumber
            && context.State.Constraint<NumberConstraint>(rightOperation) is { } rightNumber)
        {
            return new(ProcessNumberConstraints(leftNumber, rightNumber));
        }
        else
        {
            return ProcessEqualsObject(context, leftOperation, rightOperation);
        }

        ProgramState ProcessNumberConstraints(NumberConstraint left, NumberConstraint right)
        {
            if (left.IsSingleValue && right.IsSingleValue)
            {
                return context.SetOperationConstraint(BoolConstraint.From(left.Equals(right)));
            }
            else if (!left.Overlaps(right))
            {
                return context.SetOperationConstraint(BoolConstraint.False);
            }
            else
            {
                return context.State;
            }
        }
    }

    private static ProgramStates ProcessEqualsObject(SymbolicContext context, IOperation leftOperation, IOperation rightOperation)
    {
        if (context.State[leftOperation]?.Constraint<ObjectConstraint>() is var leftConstraint
            && context.State[rightOperation]?.Constraint<ObjectConstraint>() is var rightConstraint
            && (leftConstraint == ObjectConstraint.Null || rightConstraint == ObjectConstraint.Null))
        {
            if (leftConstraint == ObjectConstraint.Null && rightConstraint == ObjectConstraint.Null)
            {
                return new(context.SetOperationConstraint(BoolConstraint.True));
            }
            else if (leftConstraint is not null && rightConstraint is not null)
            {
                return new(context.SetOperationConstraint(BoolConstraint.False));
            }
            else if ((leftConstraint == ObjectConstraint.Null ? rightOperation : leftOperation).TrackedSymbol(context.State) is { } symbol)
            {
                return new(
                    context.SetOperationConstraint(BoolConstraint.True).SetSymbolConstraint(symbol, ObjectConstraint.Null),
                    context.SetOperationConstraint(BoolConstraint.False).SetSymbolConstraint(symbol, ObjectConstraint.NotNull));
            }
        }
        return new(context.State);
    }

    private static ProgramState ProcessNullableGetValueOrDefault(SymbolicContext context, IInvocationOperationWrapper invocation)
    {
        return context.State[invocation.Instance] switch
        {
            { } instanceValue when instanceValue.HasConstraint(ObjectConstraint.Null) => NullableDefaultState(),
            { } instanceValue => context.SetOperationValue(instanceValue),
            _ => context.State
        };

        ProgramState NullableDefaultState()
        {
            var valueType = ((INamedTypeSymbol)invocation.Instance.Type).TypeArguments.Single();
            return ConstantCheck.ConstraintFromType(valueType) is { } orDefaultConstraint
                ? context.SetOperationConstraint(orDefaultConstraint)
                : context.State;
        }
    }

    private static ProgramStates ProcessNullableHasValue(ProgramState state, IInvocationOperationWrapper invocation)
    {
        if (state[invocation.Instance]?.Constraint<ObjectConstraint>() is { } objectConstraint)
        {
            return new(state.SetOperationConstraint(invocation, BoolConstraint.From(objectConstraint == ObjectConstraint.NotNull)));
        }
        else if (invocation.Instance.TrackedSymbol(state) is { } symbol)
        {
            return new(
                state.SetSymbolConstraint(symbol, ObjectConstraint.Null).SetOperationConstraint(invocation, BoolConstraint.False),
                state.SetSymbolConstraint(symbol, ObjectConstraint.NotNull).SetOperationConstraint(invocation, BoolConstraint.True));
        }
        else
        {
            return new(state);
        }
    }

    private static bool IsThrowHelper(IMethodSymbol method) =>
        method.Is(KnownType.System_Diagnostics_Debug, nameof(Debug.Fail))
        || method.IsAny(KnownType.System_Environment, nameof(Environment.FailFast), nameof(Environment.Exit))
        || method.GetAttributes().Any(x => x.HasAnyName(
                                                "DoesNotReturnAttribute",       // https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.doesnotreturnattribute
                                                "TerminatesProgramAttribute")); // https://www.jetbrains.com/help/resharper/Reference__Code_Annotation_Attributes.html#TerminatesProgramAttribute

    private static ProgramStates ProcessInformationIsNothing(SymbolicContext context, IInvocationOperationWrapper invocation) =>
        context.State[invocation.Arguments[0].ToArgument().Value]?.Constraint<ObjectConstraint>() switch
        {
            ObjectConstraint constraint when constraint == ObjectConstraint.Null => new(context.SetOperationConstraint(BoolConstraint.True)),
            ObjectConstraint constraint when constraint == ObjectConstraint.NotNull => new(context.SetOperationConstraint(BoolConstraint.False)),
            _ when invocation.Arguments[0].ToArgument().Value.UnwrapConversion().Type is { } type && !type.CanBeNull() => new(context.SetOperationConstraint(BoolConstraint.False)),
            _ when invocation.Arguments[0].TrackedSymbol(context.State) is { } argumentSymbol => new(
                context.SetOperationConstraint(BoolConstraint.True).SetSymbolConstraint(argumentSymbol, ObjectConstraint.Null),
                context.SetOperationConstraint(BoolConstraint.False).SetSymbolConstraint(argumentSymbol, ObjectConstraint.NotNull)),
            _ => new(context.State),
        };

    private static bool IsNullableGetValueOrDefault(IInvocationOperationWrapper invocation) =>
        invocation.TargetMethod.Is(KnownType.System_Nullable_T, nameof(Nullable<int>.GetValueOrDefault));

    private static bool IsSupportedEqualsType(ITypeSymbol type) =>
        type.IsNullableValueType()                                      // int?.Equals
        || (type.IsStruct() && type.SpecialType != SpecialType.None)    // int.Equals and similar build-in basic value types
        || type.SpecialType == SpecialType.System_ValueType;           // struct.Equals that was not overriden
}
