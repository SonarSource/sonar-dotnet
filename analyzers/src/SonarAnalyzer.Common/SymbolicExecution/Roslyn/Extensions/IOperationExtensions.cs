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
            operation switch
            {
                _ when IParameterReferenceOperationWrapper.IsInstance(operation) => IParameterReferenceOperationWrapper.FromOperation(operation).Parameter,
                _ when ILocalReferenceOperationWrapper.IsInstance(operation) => ILocalReferenceOperationWrapper.FromOperation(operation).Local,
                _ when IFieldReferenceOperationWrapper.IsInstance(operation)
                    && IFieldReferenceOperationWrapper.FromOperation(operation) is var fieldReference
                    && IsStaticOrThis(fieldReference.Instance) => fieldReference.Field,
                _ => null
            };

        internal static IInvocationOperationWrapper? AsInvocation(this IOperation operation) =>
            operation.As(OperationKindEx.Invocation, IInvocationOperationWrapper.FromOperation);

        internal static IObjectCreationOperationWrapper? AsObjectCreation(this IOperation operation) =>
            operation.As(OperationKindEx.ObjectCreation, IObjectCreationOperationWrapper.FromOperation);

        internal static IAssignmentOperationWrapper? AsAssignment(this IOperation operation) =>
            operation.As(OperationKindEx.SimpleAssignment, IAssignmentOperationWrapper.FromOperation);

        private static T? As<T>(this IOperation operation, OperationKind kind, Func<IOperation, T> fromOperation) where T : struct =>
            operation.Kind == kind ? fromOperation(operation) : null;

        private static bool IsStaticOrThis(IOperation operation) =>
            operation == null // static fields
            || operation.IsAnyKind(OperationKindEx.InstanceReference);
    }
}
