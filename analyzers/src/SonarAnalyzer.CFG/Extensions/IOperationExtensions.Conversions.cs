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

public static partial class IOperationExtensions
{
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

    private static T? As<T>(this IOperation operation, OperationKind kind, Func<IOperation, T> fromOperation) where T : struct =>
        operation.Kind == kind ? fromOperation(operation) : null;
}
