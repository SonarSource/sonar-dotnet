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
    extension(IOperation operation)
    {
        public bool IsOutArgumentReference =>
            IArgumentOperationWrapper.IsInstance(operation.Parent)
            && IArgumentOperationWrapper.From(operation.Parent).Parameter.RefKind == RefKind.Out;

        public bool IsAnyKind(params OperationKind[] kinds) =>
            kinds.Contains(operation.Kind);

        public IOperation RootOperation
        {
            get
            {
                while (operation.Parent is not null)
                {
                    operation = operation.Parent;
                }
                return operation;
            }
        }

        public string Serialize() =>
            $"{OperationPrefix(operation)}{OperationSuffix(operation)}: {operation.Syntax}";

        public IOperation WithoutEnclosingConversions
        {
            get
            {
                while (operation?.Kind == OperationKindEx.Conversion)
                {
                    operation = operation.ToConversion().Operand;
                }
                return operation;
            }
        }
    }

    extension(IOperationWrapper operation)
    {
        public bool IsAssignmentTarget =>
            operation.WrappedInstance.Parent is { } parent
            && ISimpleAssignmentOperationWrapper.IsInstance(parent)
            && ISimpleAssignmentOperationWrapper.From(parent).Target == operation.WrappedInstance;

        public bool IsCompoundAssignmentTarget =>
            operation.WrappedInstance.Parent is { } parent
            && ICompoundAssignmentOperationWrapper.IsInstance(parent)
            && ICompoundAssignmentOperationWrapper.From(parent).Target == operation.WrappedInstance;

        public bool IsOutArgument =>
            operation.WrappedInstance.Parent is { } parent
            && IArgumentOperationWrapper.IsInstance(parent)
            && IArgumentOperationWrapper.From(parent).Parameter.RefKind == RefKind.Out;
    }

    extension(IInvocationOperationWrapper invocation)
    {
        /// <inheritdoc cref="ArgumentValue(ImmutableArray{IOperation}, string)"/>
        public IOperation ArgumentValue(string parameterName) =>
            ArgumentValue(invocation.Arguments, parameterName);
    }

    extension(IObjectCreationOperationWrapper objectCreation)
    {
        /// <inheritdoc cref="ArgumentValue(ImmutableArray{IOperation}, string)"/>
        public IOperation ArgumentValue(string parameterName) =>
            ArgumentValue(objectCreation.Arguments, parameterName);
    }

    extension(IPropertyReferenceOperationWrapper propertyReference)
    {
        /// <inheritdoc cref="ArgumentValue(ImmutableArray{IOperation}, string)"/>
        public IOperation ArgumentValue(string parameterName) =>
            ArgumentValue(propertyReference.Arguments, parameterName);
    }

    extension(IRaiseEventOperationWrapper raiseEvent)
    {
        /// <inheritdoc cref="ArgumentValue(ImmutableArray{IOperation}, string)"/>
        public IOperation ArgumentValue(string parameterName) =>
            ArgumentValue(raiseEvent.Arguments, parameterName);
    }

    extension(IEnumerable<IOperation> operations)
    {
        public OperationExecutionOrder ToExecutionOrder() =>
            new(operations, false);

        public OperationExecutionOrder ToReversedExecutionOrder() =>
            new(operations, true);
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
