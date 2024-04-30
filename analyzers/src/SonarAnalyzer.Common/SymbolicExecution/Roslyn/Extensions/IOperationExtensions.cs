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

internal static class IOperationExtensions
{
    internal static ISymbol TrackedSymbol(this IOperation operation, ProgramState state) =>
        operation?.Kind switch
        {
            OperationKindEx.FlowCaptureReference when state.ResolveCapture(operation) is var resolved && resolved != operation => resolved.TrackedSymbol(state),
            OperationKindEx.Conversion when operation.ToConversion() is var conversion && (!conversion.IsTryCast || conversion.IsUpcast()) => TrackedSymbol(operation.ToConversion().Operand, state),
            OperationKindEx.FieldReference when operation.ToFieldReference() is var fieldReference && IsStaticOrThis(fieldReference, state) && !fieldReference.Type.IsEnum() => fieldReference.Field,
            OperationKindEx.LocalReference => operation.ToLocalReference().Local,
            OperationKindEx.ParameterReference => operation.ToParameterReference().Parameter,
            OperationKindEx.Argument => operation.ToArgument().Value.TrackedSymbol(state),
            OperationKindEx.DeclarationExpression => IDeclarationExpressionOperationWrapper.FromOperation(operation).Expression.TrackedSymbol(state),
            OperationKindEx.PropertyReference when operation.ToPropertyReference() is { Property: { IsVirtual: false } property } propertyReference
                && IsStaticOrThis(propertyReference, state)
                && property.IsAutoProperty() => property,
            _ => null
        };

    internal static IArgumentOperationWrapper? AsArgument(this IOperation operation) =>
        operation.As(OperationKindEx.Argument, IArgumentOperationWrapper.FromOperation);

    internal static IAssignmentOperationWrapper? AsAssignment(this IOperation operation) =>
        operation.As(OperationKindEx.SimpleAssignment, IAssignmentOperationWrapper.FromOperation);

    internal static IArrayCreationOperationWrapper? AsArrayCreation(this IOperation operation) =>
        operation.As(OperationKindEx.ArrayCreation, IArrayCreationOperationWrapper.FromOperation);

    internal static IConversionOperationWrapper? AsConversion(this IOperation operation) =>
        operation.As(OperationKindEx.Conversion, IConversionOperationWrapper.FromOperation);

    internal static IDeclarationExpressionOperationWrapper? AsDeclarationExpression(this IOperation operation) =>
        operation.As(OperationKindEx.DeclarationExpression, IDeclarationExpressionOperationWrapper.FromOperation);

    internal static IDeclarationPatternOperationWrapper? AsDeclarationPattern(this IOperation operation) =>
        operation.As(OperationKindEx.DeclarationPattern, IDeclarationPatternOperationWrapper.FromOperation);

    internal static IInvocationOperationWrapper? AsInvocation(this IOperation operation) =>
        operation.As(OperationKindEx.Invocation, IInvocationOperationWrapper.FromOperation);

    internal static IIsPatternOperationWrapper? AsIsPattern(this IOperation operation) =>
        operation.As(OperationKindEx.IsPattern, IIsPatternOperationWrapper.FromOperation);

    internal static IMethodReferenceOperationWrapper? AsMethodReference(this IOperation operation) =>
        operation.As(OperationKindEx.MethodReference, IMethodReferenceOperationWrapper.FromOperation);

    internal static IObjectCreationOperationWrapper? AsObjectCreation(this IOperation operation) =>
        operation.As(OperationKindEx.ObjectCreation, IObjectCreationOperationWrapper.FromOperation);

    internal static IPropertyReferenceOperationWrapper? AsPropertyReference(this IOperation operation) =>
        operation.As(OperationKindEx.PropertyReference, IPropertyReferenceOperationWrapper.FromOperation);

    internal static IRecursivePatternOperationWrapper? AsRecursivePattern(this IOperation operation) =>
        operation.As(OperationKindEx.RecursivePattern, IRecursivePatternOperationWrapper.FromOperation);

    internal static ITupleOperationWrapper? AsTuple(this IOperation operation) =>
        operation.As(OperationKindEx.Tuple, ITupleOperationWrapper.FromOperation);

    internal static IAwaitOperationWrapper ToAwait(this IOperation operation) =>
        IAwaitOperationWrapper.FromOperation(operation);

    internal static IArgumentOperationWrapper ToArgument(this IOperation operation) =>
        IArgumentOperationWrapper.FromOperation(operation);

    internal static IAssignmentOperationWrapper ToAssignment(this IOperation operation) =>
        IAssignmentOperationWrapper.FromOperation(operation);

    internal static IArrayElementReferenceOperationWrapper ToArrayElementReference(this IOperation operation) =>
        IArrayElementReferenceOperationWrapper.FromOperation(operation);

    internal static IBinaryOperationWrapper ToBinary(this IOperation operation) =>
        IBinaryOperationWrapper.FromOperation(operation);

    internal static IBinaryPatternOperationWrapper ToBinaryPattern(this IOperation operation) =>
        IBinaryPatternOperationWrapper.FromOperation(operation);

    internal static ICompoundAssignmentOperationWrapper ToCompoundAssignment(this IOperation operation) =>
        ICompoundAssignmentOperationWrapper.FromOperation(operation);

    internal static IConstantPatternOperationWrapper ToConstantPattern(this IOperation operation) =>
        IConstantPatternOperationWrapper.FromOperation(operation);

    internal static IConversionOperationWrapper ToConversion(this IOperation operation) =>
        IConversionOperationWrapper.FromOperation(operation);

    internal static IDeclarationPatternOperationWrapper ToDeclarationPattern(this IOperation operation) =>
        IDeclarationPatternOperationWrapper.FromOperation(operation);

    internal static IEventReferenceOperationWrapper ToEventReference(this IOperation operation) =>
        IEventReferenceOperationWrapper.FromOperation(operation);

    internal static IFieldReferenceOperationWrapper ToFieldReference(this IOperation operation) =>
        IFieldReferenceOperationWrapper.FromOperation(operation);

    internal static IFlowCaptureReferenceOperationWrapper ToFlowCaptureReference(this IOperation operation) =>
        IFlowCaptureReferenceOperationWrapper.FromOperation(operation);

    internal static IIncrementOrDecrementOperationWrapper ToIncrementOrDecrement(this IOperation operation) =>
        IIncrementOrDecrementOperationWrapper.FromOperation(operation);

    internal static IInvocationOperationWrapper ToInvocation(this IOperation operation) =>
        IInvocationOperationWrapper.FromOperation(operation);

    internal static ILocalReferenceOperationWrapper ToLocalReference(this IOperation operation) =>
        ILocalReferenceOperationWrapper.FromOperation(operation);

    internal static IMemberReferenceOperationWrapper ToMemberReference(this IOperation operation) =>
        IMemberReferenceOperationWrapper.FromOperation(operation);

    internal static IMethodReferenceOperationWrapper ToMethodReference(this IOperation operation) =>
        IMethodReferenceOperationWrapper.FromOperation(operation);

    internal static INegatedPatternOperationWrapper ToNegatedPattern(this IOperation operation) =>
        INegatedPatternOperationWrapper.FromOperation(operation);

    internal static IObjectCreationOperationWrapper ToObjectCreation(this IOperation operation) =>
        IObjectCreationOperationWrapper.FromOperation(operation);

    internal static IParameterReferenceOperationWrapper ToParameterReference(this IOperation operation) =>
        IParameterReferenceOperationWrapper.FromOperation(operation);

    internal static IPropertyReferenceOperationWrapper ToPropertyReference(this IOperation operation) =>
        IPropertyReferenceOperationWrapper.FromOperation(operation);

    internal static IRecursivePatternOperationWrapper ToRecursivePattern(this IOperation operation) =>
        IRecursivePatternOperationWrapper.FromOperation(operation);

    internal static IRelationalPatternOperationWrapper ToRelationalPattern(this IOperation operation) =>
        IRelationalPatternOperationWrapper.FromOperation(operation);

    internal static ITypePatternOperationWrapper ToTypePattern(this IOperation operation) =>
        ITypePatternOperationWrapper.FromOperation(operation);

    internal static ITupleOperationWrapper ToTuple(this IOperation operation) =>
        ITupleOperationWrapper.FromOperation(operation);

    internal static IUnaryOperationWrapper ToUnary(this IOperation operation) =>
        IUnaryOperationWrapper.FromOperation(operation);

    public static bool IsStaticOrThis(this IMemberReferenceOperationWrapper reference, ProgramState state) =>
        reference.Instance is null // static fields
        || state.ResolveCaptureAndUnwrapConversion(reference.Instance).Kind == OperationKindEx.InstanceReference;

    public static IOperation UnwrapConversion(this IOperation operation)
    {
        while (operation?.Kind == OperationKindEx.Conversion)
        {
            operation = operation.ToConversion().Operand;
        }
        return operation;
    }

    public static bool IsUpcast(this IConversionOperationWrapper conversion) =>
        conversion.Operand.Type.DerivesOrImplements(conversion.Type)
        || (conversion.Operand.Type.IsNonNullableValueType() && conversion.Type.IsNullableValueType());

    /// <inheritdoc cref="ArgumentValue(ImmutableArray{IOperation}, string)"/>
    public static IOperation ArgumentValue(this IInvocationOperationWrapper invocation, string parameterName) =>
        ArgumentValue(invocation.Arguments, parameterName);

    /// <inheritdoc cref="ArgumentValue(ImmutableArray{IOperation}, string)"/>
    public static IOperation ArgumentValue(this IObjectCreationOperationWrapper objectCreation, string parameterName) =>
        ArgumentValue(objectCreation.Arguments, parameterName);

    /// <inheritdoc cref="ArgumentValue(ImmutableArray{IOperation}, string)"/>
    public static IOperation ArgumentValue(this IPropertyReferenceOperationWrapper propertyReference, string parameterName) =>
        ArgumentValue(propertyReference.Arguments, parameterName);

    /// <inheritdoc cref="ArgumentValue(ImmutableArray{IOperation}, string)"/>
    public static IOperation ArgumentValue(this IRaiseEventOperationWrapper raiseEvent, string parameterName) =>
        ArgumentValue(raiseEvent.Arguments, parameterName);

    /// <summary>
    /// Returns the argument value corresponding to <paramref name="parameterName"/>. For <see langword="params"/> parameter an IArrayCreationOperation is returned.
    /// </summary>
    private static IOperation ArgumentValue(ImmutableArray<IOperation> arguments, string parameterName)
    {
        foreach (var operation in arguments)
        {
            var argument = operation.ToArgument();
            if (argument.Parameter.Name == parameterName)
            {
                return argument.Value;
            }
        }
        return null;
    }

    private static T? As<T>(this IOperation operation, OperationKind kind, Func<IOperation, T> fromOperation) where T : struct =>
        operation.Kind == kind ? fromOperation(operation) : null;
}
