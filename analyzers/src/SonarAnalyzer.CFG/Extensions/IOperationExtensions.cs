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
        operation.WrappedInstance.Parent is { } parent
        && ISimpleAssignmentOperationWrapper.IsInstance(parent)
        && ISimpleAssignmentOperationWrapper.From(parent).Target == operation.WrappedInstance;

    public static bool IsCompoundAssignmentTarget(this IOperationWrapper operation) =>
        operation.WrappedInstance.Parent is { } parent
        && ICompoundAssignmentOperationWrapper.IsInstance(parent)
        && ICompoundAssignmentOperationWrapper.From(parent).Target == operation.WrappedInstance;

    public static bool IsOutArgument(this IOperationWrapper operation) =>
        operation.WrappedInstance.Parent is { } parent
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
    private static IOperation ArgumentValue(ImmutableArray<IArgumentOperationWrapper> arguments, string parameterName)
    {
        foreach (var argument in arguments)
        {
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
}
