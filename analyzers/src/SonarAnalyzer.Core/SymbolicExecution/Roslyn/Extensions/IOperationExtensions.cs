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
    public static bool IsStaticOrThis(this IMemberReferenceOperationWrapper reference, ProgramState state) =>
        reference.Instance is null // static fields
        || state.ResolveCaptureAndUnwrapConversion(reference.Instance).Kind == OperationKindEx.InstanceReference;

    public static bool IsUpcast(this IConversionOperationWrapper conversion) =>
        conversion.Operand.Type.DerivesOrImplements(conversion.Type)
        || (conversion.Operand.Type.IsNonNullableValueType() && conversion.Type.IsNullableValueType());

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
}
