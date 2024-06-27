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

namespace SonarAnalyzer.Extensions;

public static class IOperationExtensions
{
    public static IOperationWrapperSonar ToSonar(this IOperation operation) =>
        new(operation);

    public static IOperationWrapperSonar ToSonar(this IOperationWrapper operation) =>
        new(operation.WrappedOperation);

    public static bool IsOutArgumentReference(this IOperation operation) =>
        operation.ToSonar() is var wrapped
        && IArgumentOperationWrapper.IsInstance(wrapped.Parent)
        && IArgumentOperationWrapper.FromOperation(wrapped.Parent).Parameter.RefKind == RefKind.Out;

    public static bool IsAssignmentTarget(this IOperationWrapper operation) =>
        operation.ToSonar().Parent is { } parent
        && ISimpleAssignmentOperationWrapper.IsInstance(parent)
        && ISimpleAssignmentOperationWrapper.FromOperation(parent).Target == operation.WrappedOperation;

    public static bool IsCompoundAssignmentTarget(this IOperationWrapper operation) =>
        operation.ToSonar().Parent is { } parent
        && ICompoundAssignmentOperationWrapper.IsInstance(parent)
        && ICompoundAssignmentOperationWrapper.FromOperation(parent).Target == operation.WrappedOperation;

    public static bool IsOutArgument(this IOperationWrapper operation) =>
        operation.ToSonar().Parent is { } parent
        && IArgumentOperationWrapper.IsInstance(parent)
        && IArgumentOperationWrapper.FromOperation(parent).Parameter.RefKind == RefKind.Out;

    public static bool IsAnyKind(this IOperation operation, params OperationKind[] kinds) =>
        kinds.Contains(operation.Kind);

    public static IOperation RootOperation(this IOperation operation)
    {
        var wrapper = operation.ToSonar();
        while (wrapper.Parent is not null)
        {
            wrapper = wrapper.Parent.ToSonar();
        }
        return wrapper.Instance;
    }

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

    public static OperationExecutionOrder ToExecutionOrder(this IEnumerable<IOperation> operations) =>
        new(operations, false);

    public static OperationExecutionOrder ToReversedExecutionOrder(this IEnumerable<IOperation> operations) =>
        new(operations, true);

    public static string Serialize(this IOperation operation) =>
        $"{OperationPrefix(operation)}{OperationSuffix(operation)}: {operation.Syntax}";

    // This method is taken from Roslyn implementation
    public static IEnumerable<IOperation> DescendantsAndSelf(this IOperation operation) =>
        Descendants(operation, true);

    public static IArgumentOperationWrapper? AsArgument(this IOperation operation) =>
   operation.As(OperationKindEx.Argument, IArgumentOperationWrapper.FromOperation);

    public static IAssignmentOperationWrapper? AsAssignment(this IOperation operation) =>
        operation.As(OperationKindEx.SimpleAssignment, IAssignmentOperationWrapper.FromOperation);

    public static IArrayCreationOperationWrapper? AsArrayCreation(this IOperation operation) =>
        operation.As(OperationKindEx.ArrayCreation, IArrayCreationOperationWrapper.FromOperation);

    public static IConversionOperationWrapper? AsConversion(this IOperation operation) =>
        operation.As(OperationKindEx.Conversion, IConversionOperationWrapper.FromOperation);

    public static IDeclarationExpressionOperationWrapper? AsDeclarationExpression(this IOperation operation) =>
        operation.As(OperationKindEx.DeclarationExpression, IDeclarationExpressionOperationWrapper.FromOperation);

    public static IDeclarationPatternOperationWrapper? AsDeclarationPattern(this IOperation operation) =>
        operation.As(OperationKindEx.DeclarationPattern, IDeclarationPatternOperationWrapper.FromOperation);

    public static IInvocationOperationWrapper? AsInvocation(this IOperation operation) =>
        operation.As(OperationKindEx.Invocation, IInvocationOperationWrapper.FromOperation);

    public static IIsPatternOperationWrapper? AsIsPattern(this IOperation operation) =>
        operation.As(OperationKindEx.IsPattern, IIsPatternOperationWrapper.FromOperation);

    public static IMethodReferenceOperationWrapper? AsMethodReference(this IOperation operation) =>
        operation.As(OperationKindEx.MethodReference, IMethodReferenceOperationWrapper.FromOperation);

    public static IObjectCreationOperationWrapper? AsObjectCreation(this IOperation operation) =>
        operation.As(OperationKindEx.ObjectCreation, IObjectCreationOperationWrapper.FromOperation);

    public static IPropertyReferenceOperationWrapper? AsPropertyReference(this IOperation operation) =>
        operation.As(OperationKindEx.PropertyReference, IPropertyReferenceOperationWrapper.FromOperation);

    public static IRecursivePatternOperationWrapper? AsRecursivePattern(this IOperation operation) =>
        operation.As(OperationKindEx.RecursivePattern, IRecursivePatternOperationWrapper.FromOperation);

    public static IArrayElementReferenceOperationWrapper? AsArrayElementReference(this IOperation operation) =>
        operation.As(OperationKindEx.ArrayElementReference, IArrayElementReferenceOperationWrapper.FromOperation);

    public static ITupleOperationWrapper? AsTuple(this IOperation operation) =>
        operation.As(OperationKindEx.Tuple, ITupleOperationWrapper.FromOperation);

    public static IAwaitOperationWrapper ToAwait(this IOperation operation) =>
        IAwaitOperationWrapper.FromOperation(operation);

    public static IArgumentOperationWrapper ToArgument(this IOperation operation) =>
        IArgumentOperationWrapper.FromOperation(operation);

    public static IAssignmentOperationWrapper ToAssignment(this IOperation operation) =>
        IAssignmentOperationWrapper.FromOperation(operation);

    public static IArrayElementReferenceOperationWrapper ToArrayElementReference(this IOperation operation) =>
        IArrayElementReferenceOperationWrapper.FromOperation(operation);

    public static IBinaryOperationWrapper ToBinary(this IOperation operation) =>
        IBinaryOperationWrapper.FromOperation(operation);

    public static IBinaryPatternOperationWrapper ToBinaryPattern(this IOperation operation) =>
        IBinaryPatternOperationWrapper.FromOperation(operation);

    public static ICompoundAssignmentOperationWrapper ToCompoundAssignment(this IOperation operation) =>
        ICompoundAssignmentOperationWrapper.FromOperation(operation);

    public static IConstantPatternOperationWrapper ToConstantPattern(this IOperation operation) =>
        IConstantPatternOperationWrapper.FromOperation(operation);

    public static IConversionOperationWrapper ToConversion(this IOperation operation) =>
        IConversionOperationWrapper.FromOperation(operation);

    public static IDeclarationPatternOperationWrapper ToDeclarationPattern(this IOperation operation) =>
        IDeclarationPatternOperationWrapper.FromOperation(operation);

    public static IEventReferenceOperationWrapper ToEventReference(this IOperation operation) =>
        IEventReferenceOperationWrapper.FromOperation(operation);

    public static IFieldReferenceOperationWrapper ToFieldReference(this IOperation operation) =>
        IFieldReferenceOperationWrapper.FromOperation(operation);

    public static IFlowCaptureReferenceOperationWrapper ToFlowCaptureReference(this IOperation operation) =>
        IFlowCaptureReferenceOperationWrapper.FromOperation(operation);

    public static IIncrementOrDecrementOperationWrapper ToIncrementOrDecrement(this IOperation operation) =>
        IIncrementOrDecrementOperationWrapper.FromOperation(operation);

    public static IInvocationOperationWrapper ToInvocation(this IOperation operation) =>
        IInvocationOperationWrapper.FromOperation(operation);

    public static ILocalReferenceOperationWrapper ToLocalReference(this IOperation operation) =>
        ILocalReferenceOperationWrapper.FromOperation(operation);

    public static IMemberReferenceOperationWrapper ToMemberReference(this IOperation operation) =>
        IMemberReferenceOperationWrapper.FromOperation(operation);

    public static IMethodReferenceOperationWrapper ToMethodReference(this IOperation operation) =>
        IMethodReferenceOperationWrapper.FromOperation(operation);

    public static INegatedPatternOperationWrapper ToNegatedPattern(this IOperation operation) =>
        INegatedPatternOperationWrapper.FromOperation(operation);

    public static IObjectCreationOperationWrapper ToObjectCreation(this IOperation operation) =>
        IObjectCreationOperationWrapper.FromOperation(operation);

    public static IParameterReferenceOperationWrapper ToParameterReference(this IOperation operation) =>
        IParameterReferenceOperationWrapper.FromOperation(operation);

    public static IPropertyReferenceOperationWrapper ToPropertyReference(this IOperation operation) =>
        IPropertyReferenceOperationWrapper.FromOperation(operation);

    public static IRecursivePatternOperationWrapper ToRecursivePattern(this IOperation operation) =>
        IRecursivePatternOperationWrapper.FromOperation(operation);

    public static IRelationalPatternOperationWrapper ToRelationalPattern(this IOperation operation) =>
        IRelationalPatternOperationWrapper.FromOperation(operation);

    public static ITypePatternOperationWrapper ToTypePattern(this IOperation operation) =>
        ITypePatternOperationWrapper.FromOperation(operation);

    public static ITupleOperationWrapper ToTuple(this IOperation operation) =>
        ITupleOperationWrapper.FromOperation(operation);

    public static IUnaryOperationWrapper ToUnary(this IOperation operation) =>
        IUnaryOperationWrapper.FromOperation(operation);

    public static IOperation UnwrapConversion(this IOperation operation)
    {
        while (operation?.Kind == OperationKindEx.Conversion)
        {
            operation = operation.ToConversion().Operand;
        }
        return operation;
    }

    // This method is taken from Roslyn implementation
    private static IEnumerable<IOperation> Descendants(IOperation operation, bool includeSelf)
    {
        if (operation is null)
        {
            yield break;
        }
        if (includeSelf)
        {
            yield return operation;
        }
        var stack = new Stack<IEnumerator<IOperation>>();
        stack.Push(operation.ToSonar().Children.GetEnumerator());
        while (stack.Any())
        {
            var iterator = stack.Pop();
            if (!iterator.MoveNext())
            {
                continue;
            }

            stack.Push(iterator);
            if (iterator.Current is { } current)
            {
                yield return current;
                stack.Push(current.ToSonar().Children.GetEnumerator());
            }
        }
    }

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

    private static string OperationPrefix(IOperation op) =>
        op.Kind == OperationKindEx.Invalid ? "INVALID" : op.GetType().Name;

    private static string OperationSuffix(IOperation op) =>
        op switch
        {
            var _ when IInvocationOperationWrapper.IsInstance(op) => ": " + IInvocationOperationWrapper.FromOperation(op).TargetMethod.Name,
            var _ when IFlowCaptureOperationWrapper.IsInstance(op) => ": " + IFlowCaptureOperationWrapper.FromOperation(op).Id.Serialize(),
            var _ when IFlowCaptureReferenceOperationWrapper.IsInstance(op) => ": " + IFlowCaptureReferenceOperationWrapper.FromOperation(op).Id.Serialize(),
            _ => null
        };

    private static T? As<T>(this IOperation operation, OperationKind kind, Func<IOperation, T> fromOperation) where T : struct =>
        operation.Kind == kind ? fromOperation(operation) : null;
}
