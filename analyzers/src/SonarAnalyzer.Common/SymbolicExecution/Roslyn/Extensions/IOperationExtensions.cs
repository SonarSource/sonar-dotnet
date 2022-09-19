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
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Extensions;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    internal static class IOperationExtensions
    {
        internal static ISymbol TrackedSymbol(this IOperation operation) =>
            operation?.Kind switch
            {
                OperationKindEx.Conversion => TrackedSymbol(IConversionOperationWrapper.FromOperation(operation).Operand),
                OperationKindEx.FieldReference when IFieldReferenceOperationWrapper.FromOperation(operation) is var fieldReference && IsStaticOrThis(fieldReference) => fieldReference.Field,
                OperationKindEx.LocalReference => ILocalReferenceOperationWrapper.FromOperation(operation).Local,
                OperationKindEx.ParameterReference => IParameterReferenceOperationWrapper.FromOperation(operation).Parameter,
                OperationKindEx.Argument => IArgumentOperationWrapper.FromOperation(operation).Value.TrackedSymbol(),
                _ => null
            };

        internal static ISymbol ResolveTrackedSymbol(this IOperation operation, ProgramState state) =>
            operation?.Kind switch
            {
                OperationKindEx.FlowCaptureReference => state.ResolveCapture(operation) is { } capture && capture != operation ? capture.ResolveTrackedSymbol(state) : null,
                OperationKindEx.PropertyReference => IPropertyReferenceOperationWrapper.FromOperation(operation).Instance.ResolveTrackedSymbol(state),
                OperationKindEx.Conversion => IConversionOperationWrapper.FromOperation(operation).Operand.ResolveTrackedSymbol(state),
                _ => TrackedSymbol(operation),
            };

        internal static IInvocationOperationWrapper? AsInvocation(this IOperation operation) =>
            operation.As(OperationKindEx.Invocation, IInvocationOperationWrapper.FromOperation);

        internal static IIsPatternOperationWrapper? AsIsPattern(this IOperation operation) =>
            operation.As(OperationKindEx.IsPattern, IIsPatternOperationWrapper.FromOperation);

        internal static IObjectCreationOperationWrapper? AsObjectCreation(this IOperation operation) =>
            operation.As(OperationKindEx.ObjectCreation, IObjectCreationOperationWrapper.FromOperation);

        internal static IAssignmentOperationWrapper? AsAssignment(this IOperation operation) =>
            operation.As(OperationKindEx.SimpleAssignment, IAssignmentOperationWrapper.FromOperation);

        internal static IPropertyReferenceOperationWrapper? AsPropertyReference(this IOperation operation) =>
            operation.As(OperationKindEx.PropertyReference, IPropertyReferenceOperationWrapper.FromOperation);

        internal static IAwaitOperationWrapper ToAwait(this IOperation operation) =>
            IAwaitOperationWrapper.FromOperation(operation);

        internal static IArgumentOperationWrapper ToArgument(this IOperation operation) =>
            IArgumentOperationWrapper.FromOperation(operation);

        internal static IArrayElementReferenceOperationWrapper ToArrayElementReference(this IOperation operation) =>
            IArrayElementReferenceOperationWrapper.FromOperation(operation);

        internal static IInvocationOperationWrapper ToInvocation(this IOperation operation) =>
            IInvocationOperationWrapper.FromOperation(operation);

        internal static IPropertyReferenceOperationWrapper ToPropertyReference(this IOperation operation) =>
            IPropertyReferenceOperationWrapper.FromOperation(operation);

        internal static IUnaryOperationWrapper ToUnary(this IOperation operation) =>
            IUnaryOperationWrapper.FromOperation(operation);

        public static bool IsStaticOrThis(this IMemberReferenceOperationWrapper reference) =>
            reference.Instance == null // static fields
            || reference.Instance.IsAnyKind(OperationKindEx.InstanceReference);

        public static IOperation UnwrapConversion(this IOperation operation)
        {
            while (operation?.Kind == OperationKindEx.Conversion)
            {
                operation = IConversionOperationWrapper.FromOperation(operation).Operand;
            }
            return operation;
        }

        private static T? As<T>(this IOperation operation, OperationKind kind, Func<IOperation, T> fromOperation) where T : struct =>
            operation.Kind == kind ? fromOperation(operation) : null;
    }
}
