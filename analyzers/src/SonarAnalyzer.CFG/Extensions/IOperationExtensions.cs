/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CFG.Operations.Utilities;

namespace SonarAnalyzer.CFG.Extensions;

public static class IOperationExtensions
{
    public static bool IsOutArgumentReference(this IOperation operation) =>
        IArgumentOperationWrapper.IsInstance(operation.Parent)
        && IArgumentOperationWrapper.From(operation.Parent).Parameter.RefKind == RefKind.Out;

    public static bool IsAssignmentTarget(this IOperationWrapper operation) =>
        operation.WrappedOperation.Parent is { } parent
        && ISimpleAssignmentOperationWrapper.IsInstance(parent)
        && ISimpleAssignmentOperationWrapper.From(parent).Target == operation.WrappedOperation;

    public static bool IsCompoundAssignmentTarget(this IOperationWrapper operation) =>
        operation.WrappedOperation.Parent is { } parent
        && ICompoundAssignmentOperationWrapper.IsInstance(parent)
        && ICompoundAssignmentOperationWrapper.From(parent).Target == operation.WrappedOperation;

    public static bool IsOutArgument(this IOperationWrapper operation) =>
        operation.WrappedOperation.Parent is { } parent
        && IArgumentOperationWrapper.IsInstance(parent)
        && IArgumentOperationWrapper.From(parent).Parameter.RefKind == RefKind.Out;

    public static bool IsAnyKind(this IOperation operation, params OperationKind[] kinds) =>
        kinds.Contains(operation.Kind);

    public static IOperation RootOperation(this IOperation operation)
    {
        while (operation.Parent is not null)
        {
            operation = operation.Parent;
        }
        return operation;
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

    public static IAnonymousFunctionOperationWrapper? AsAnonymousFunction(this IOperation operation) =>
        operation.As(OperationKindEx.AnonymousFunction, IAnonymousFunctionOperationWrapper.From);

    public static IArgumentOperationWrapper? AsArgument(this IOperation operation) =>
        operation.As(OperationKindEx.Argument, IArgumentOperationWrapper.From);

    public static IAssignmentOperationWrapper? AsAssignment(this IOperation operation) =>
        operation.As(OperationKindEx.SimpleAssignment, IAssignmentOperationWrapper.From);

    public static ISimpleAssignmentOperationWrapper? AsSimpleAssignment(this IOperation operation) =>
        operation.As(OperationKindEx.SimpleAssignment, ISimpleAssignmentOperationWrapper.From);

    public static IArrayCreationOperationWrapper? AsArrayCreation(this IOperation operation) =>
        operation.As(OperationKindEx.ArrayCreation, IArrayCreationOperationWrapper.From);

    public static IArrayElementReferenceOperationWrapper? AsArrayElementReference(this IOperation operation) =>
        operation.As(OperationKindEx.ArrayElementReference, IArrayElementReferenceOperationWrapper.From);

    public static IConversionOperationWrapper? AsConversion(this IOperation operation) =>
        operation.As(OperationKindEx.Conversion, IConversionOperationWrapper.From);

    public static IDeclarationExpressionOperationWrapper? AsDeclarationExpression(this IOperation operation) =>
        operation.As(OperationKindEx.DeclarationExpression, IDeclarationExpressionOperationWrapper.From);

    public static IDeclarationPatternOperationWrapper? AsDeclarationPattern(this IOperation operation) =>
        operation.As(OperationKindEx.DeclarationPattern, IDeclarationPatternOperationWrapper.From);

    public static IFlowAnonymousFunctionOperationWrapper? AsFlowAnonymousFunction(this IOperation operation) =>
        operation.As(OperationKindEx.FlowAnonymousFunction, IFlowAnonymousFunctionOperationWrapper.From);

    public static IFlowCaptureOperationWrapper? AsFlowCapture(this IOperation operation) =>
        operation.As(OperationKindEx.FlowCapture, IFlowCaptureOperationWrapper.From);

    public static IFlowCaptureReferenceOperationWrapper? AsFlowCaptureReference(this IOperation operation) =>
        operation.As(OperationKindEx.FlowCaptureReference, IFlowCaptureReferenceOperationWrapper.From);

    public static IForEachLoopOperationWrapper? AsForEachLoop(this IOperation operation)
    {
        if (operation is null)  // null check to be consistent with other the other As methods
        {
            throw new NullReferenceException(nameof(operation));
        }
        // Other LoopKinds (e.g. For, While) are still OperationKindEx.Loop, but cannot be cast to IForEachLoopOperationWrapper so we need an additional check
        return IForEachLoopOperationWrapper.IsInstance(operation) ? IForEachLoopOperationWrapper.From(operation) : null;
    }

    public static IInvocationOperationWrapper? AsInvocation(this IOperation operation) =>
        operation.As(OperationKindEx.Invocation, IInvocationOperationWrapper.From);

    public static ILocalFunctionOperationWrapper? AsLocalFunction(this IOperation operation) =>
        operation.As(OperationKindEx.LocalFunction, ILocalFunctionOperationWrapper.From);

    public static ILocalReferenceOperationWrapper? AsLocalReference(this IOperation operation) =>
        operation.As(OperationKindEx.LocalReference, ILocalReferenceOperationWrapper.From);

    public static IIsNullOperationWrapper? AsIsNull(this IOperation operation) =>
        operation.As(OperationKindEx.IsNull, IIsNullOperationWrapper.From);

    public static IIsPatternOperationWrapper? AsIsPattern(this IOperation operation) =>
        operation.As(OperationKindEx.IsPattern, IIsPatternOperationWrapper.From);

    public static IParameterReferenceOperationWrapper? AsParameterReference(this IOperation operation) =>
        operation.As(OperationKindEx.ParameterReference, IParameterReferenceOperationWrapper.From);

    public static IMethodReferenceOperationWrapper? AsMethodReference(this IOperation operation) =>
        operation.As(OperationKindEx.MethodReference, IMethodReferenceOperationWrapper.From);

    public static IObjectCreationOperationWrapper? AsObjectCreation(this IOperation operation) =>
        operation.As(OperationKindEx.ObjectCreation, IObjectCreationOperationWrapper.From);

    public static IPropertyReferenceOperationWrapper? AsPropertyReference(this IOperation operation) =>
        operation.As(OperationKindEx.PropertyReference, IPropertyReferenceOperationWrapper.From);

    public static IRecursivePatternOperationWrapper? AsRecursivePattern(this IOperation operation) =>
        operation.As(OperationKindEx.RecursivePattern, IRecursivePatternOperationWrapper.From);

    public static ISpreadOperationWrapper? AsSpread(this IOperation operation) =>
        operation.As(OperationKindEx.Spread, ISpreadOperationWrapper.From);

    public static ITupleOperationWrapper? AsTuple(this IOperation operation) =>
        operation.As(OperationKindEx.Tuple, ITupleOperationWrapper.From);

    public static IVariableDeclaratorOperationWrapper? AsVariableDeclarator(this IOperation operation) =>
        operation.As(OperationKindEx.VariableDeclarator, IVariableDeclaratorOperationWrapper.From);

    public static IAddressOfOperationWrapper ToAddressOf(this IOperation operation) =>
        IAddressOfOperationWrapper.From(operation);

    public static IAwaitOperationWrapper ToAwait(this IOperation operation) =>
        IAwaitOperationWrapper.From(operation);

    public static IArgumentOperationWrapper ToArgument(this IOperation operation) =>
        IArgumentOperationWrapper.From(operation);

    public static IArrayCreationOperationWrapper ToArrayCreation(this IOperation operation) =>
        IArrayCreationOperationWrapper.From(operation);

    public static IAssignmentOperationWrapper ToAssignment(this IOperation operation) =>
        IAssignmentOperationWrapper.From(operation);

    public static IArrayElementReferenceOperationWrapper ToArrayElementReference(this IOperation operation) =>
        IArrayElementReferenceOperationWrapper.From(operation);

    public static IBinaryOperationWrapper ToBinary(this IOperation operation) =>
        IBinaryOperationWrapper.From(operation);

    public static IBinaryPatternOperationWrapper ToBinaryPattern(this IOperation operation) =>
        IBinaryPatternOperationWrapper.From(operation);

    public static ICatchClauseOperationWrapper ToCatchClause(this IOperation operation) =>
        ICatchClauseOperationWrapper.From(operation);

    public static ICompoundAssignmentOperationWrapper ToCompoundAssignment(this IOperation operation) =>
        ICompoundAssignmentOperationWrapper.From(operation);

    public static IConstantPatternOperationWrapper ToConstantPattern(this IOperation operation) =>
        IConstantPatternOperationWrapper.From(operation);

    public static IConversionOperationWrapper ToConversion(this IOperation operation) =>
        IConversionOperationWrapper.From(operation);

    public static IDeclarationPatternOperationWrapper ToDeclarationPattern(this IOperation operation) =>
        IDeclarationPatternOperationWrapper.From(operation);

    public static IEventReferenceOperationWrapper ToEventReference(this IOperation operation) =>
        IEventReferenceOperationWrapper.From(operation);

    public static IFieldReferenceOperationWrapper ToFieldReference(this IOperation operation) =>
        IFieldReferenceOperationWrapper.From(operation);

    public static IFlowCaptureOperationWrapper ToFlowCapture(this IOperation operation) =>
        IFlowCaptureOperationWrapper.From(operation);

    public static IFlowCaptureReferenceOperationWrapper ToFlowCaptureReference(this IOperation operation) =>
        IFlowCaptureReferenceOperationWrapper.From(operation);

    public static IIncrementOrDecrementOperationWrapper ToIncrementOrDecrement(this IOperation operation) =>
        IIncrementOrDecrementOperationWrapper.From(operation);

    public static IInvocationOperationWrapper ToInvocation(this IOperation operation) =>
        IInvocationOperationWrapper.From(operation);

    public static IIsTypeOperationWrapper ToIsType(this IOperation operation) =>
        IIsTypeOperationWrapper.From(operation);

    public static ILocalFunctionOperationWrapper ToLocalFunction(this IOperation operation) =>
        ILocalFunctionOperationWrapper.From(operation);

    public static ILocalReferenceOperationWrapper ToLocalReference(this IOperation operation) =>
        ILocalReferenceOperationWrapper.From(operation);

    public static IMemberReferenceOperationWrapper ToMemberReference(this IOperation operation) =>
        IMemberReferenceOperationWrapper.From(operation);

    public static IMethodReferenceOperationWrapper ToMethodReference(this IOperation operation) =>
        IMethodReferenceOperationWrapper.From(operation);

    public static INegatedPatternOperationWrapper ToNegatedPattern(this IOperation operation) =>
        INegatedPatternOperationWrapper.From(operation);

    public static IObjectCreationOperationWrapper ToObjectCreation(this IOperation operation) =>
        IObjectCreationOperationWrapper.From(operation);

    public static IPatternOperationWrapper ToPattern(this IOperation operation) =>
        IPatternOperationWrapper.From(operation);

    public static IParameterReferenceOperationWrapper ToParameterReference(this IOperation operation) =>
        IParameterReferenceOperationWrapper.From(operation);

    public static IPropertyReferenceOperationWrapper ToPropertyReference(this IOperation operation) =>
        IPropertyReferenceOperationWrapper.From(operation);

    public static IRecursivePatternOperationWrapper ToRecursivePattern(this IOperation operation) =>
        IRecursivePatternOperationWrapper.From(operation);

    public static IRelationalPatternOperationWrapper ToRelationalPattern(this IOperation operation) =>
        IRelationalPatternOperationWrapper.From(operation);

    public static ITypePatternOperationWrapper ToTypePattern(this IOperation operation) =>
        ITypePatternOperationWrapper.From(operation);

    public static ITupleOperationWrapper ToTuple(this IOperation operation) =>
        ITupleOperationWrapper.From(operation);

    public static IUnaryOperationWrapper ToUnary(this IOperation operation) =>
        IUnaryOperationWrapper.From(operation);

    public static IVariableDeclarationOperationWrapper ToVariableDeclaration(this IOperation operation) =>
        IVariableDeclarationOperationWrapper.From(operation);

    public static IVariableDeclaratorOperationWrapper ToVariableDeclarator(this IOperation operation) =>
        IVariableDeclaratorOperationWrapper.From(operation);

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
        stack.Push(operation.Children.GetEnumerator());
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
                stack.Push(current.Children.GetEnumerator());
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
            var _ when IInvocationOperationWrapper.IsInstance(op) => ": " + IInvocationOperationWrapper.From(op).TargetMethod.Name,
            var _ when IFlowCaptureOperationWrapper.IsInstance(op) => ": " + IFlowCaptureOperationWrapper.From(op).Id.Serialize(),
            var _ when IFlowCaptureReferenceOperationWrapper.IsInstance(op) => ": " + IFlowCaptureReferenceOperationWrapper.From(op).Id.Serialize(),
            _ => null
        };

    private static T? As<T>(this IOperation operation, OperationKind kind, Func<IOperation, T> from) where T : struct =>
        operation.Kind == kind ? from(operation) : null;
}
