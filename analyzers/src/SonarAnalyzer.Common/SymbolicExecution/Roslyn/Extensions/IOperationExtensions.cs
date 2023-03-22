/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    internal static class IOperationExtensions
    {
        internal static ISymbol TrackedSymbol(this IOperation operation)
        {
            return operation?.Kind switch
            {
                OperationKindEx.Conversion when operation.ToConversion() is var conversion && !IsTryDownCast(conversion) => TrackedSymbol(conversion.Operand),
                OperationKindEx.FieldReference when operation.ToFieldReference() is var fieldReference && IsStaticOrThis(fieldReference) => fieldReference.Field,
                OperationKindEx.LocalReference => operation.ToLocalReference().Local,
                OperationKindEx.ParameterReference => operation.ToParameterReference().Parameter,
                OperationKindEx.Argument => operation.ToArgument().Value.TrackedSymbol(),
                OperationKindEx.DeclarationExpression => IDeclarationExpressionOperationWrapper.FromOperation(operation).Expression.TrackedSymbol(),
                _ => null
            };

            static bool IsTryDownCast(IConversionOperationWrapper conversion) =>
                conversion.IsTryCast && !conversion.Operand.Type.DerivesOrImplements(conversion.Type);
        }

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

        internal static IConversionOperationWrapper ToConversion(this IOperation operation) =>
            IConversionOperationWrapper.FromOperation(operation);

        internal static IInvocationOperationWrapper ToInvocation(this IOperation operation) =>
            IInvocationOperationWrapper.FromOperation(operation);

        internal static IFieldReferenceOperationWrapper ToFieldReference(this IOperation operation) =>
            IFieldReferenceOperationWrapper.FromOperation(operation);

        internal static ILocalReferenceOperationWrapper ToLocalReference(this IOperation operation) =>
            ILocalReferenceOperationWrapper.FromOperation(operation);

        internal static IMethodReferenceOperationWrapper ToMethodReference(this IOperation operation) =>
            IMethodReferenceOperationWrapper.FromOperation(operation);

        internal static IPropertyReferenceOperationWrapper ToPropertyReference(this IOperation operation) =>
            IPropertyReferenceOperationWrapper.FromOperation(operation);

        internal static IParameterReferenceOperationWrapper ToParameterReference(this IOperation operation) =>
            IParameterReferenceOperationWrapper.FromOperation(operation);

        internal static IEventReferenceOperationWrapper ToEventReference(this IOperation operation) =>
            IEventReferenceOperationWrapper.FromOperation(operation);

        internal static IUnaryOperationWrapper ToUnary(this IOperation operation) =>
            IUnaryOperationWrapper.FromOperation(operation);

        public static bool IsStaticOrThis(this IMemberReferenceOperationWrapper reference) =>
            reference.Instance == null // static fields
            || reference.Instance.Kind == OperationKindEx.InstanceReference;

        public static IOperation UnwrapConversion(this IOperation operation)
        {
            while (operation?.Kind == OperationKindEx.Conversion)
            {
                operation = operation.ToConversion().Operand;
            }
            return operation;
        }

        private static T? As<T>(this IOperation operation, OperationKind kind, Func<IOperation, T> fromOperation) where T : struct =>
            operation.Kind == kind ? fromOperation(operation) : null;
    }
}
