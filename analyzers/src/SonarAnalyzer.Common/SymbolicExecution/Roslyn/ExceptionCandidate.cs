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

using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    internal class ExceptionCandidate
    {
        private readonly TypeCatalog typeCatalog;

        public ExceptionCandidate(Compilation compilation) =>
            typeCatalog = new TypeCatalog(compilation);

        public ExceptionState FromOperation(IOperationWrapperSonar operation) =>
            operation.Instance.Kind switch
            {
                OperationKindEx.ArrayElementReference => FromOperation(IArrayElementReferenceOperationWrapper.FromOperation(operation.Instance)),
                OperationKindEx.Conversion => ConversionExceptionCandidate(operation),
                OperationKindEx.DynamicIndexerAccess => new ExceptionState(typeCatalog.SystemIndexOutOfRangeException),
                OperationKindEx.DynamicInvocation => ExceptionState.UnknownException,      // This raises is Microsoft.CSharp.RuntimeBinder.RuntimeBinderException that we can't access.
                OperationKindEx.DynamicMemberReference => ExceptionState.UnknownException, // This raises is Microsoft.CSharp.RuntimeBinder.RuntimeBinderException that we can't access.
                OperationKindEx.DynamicObjectCreation => ExceptionState.UnknownException,  // This raises is Microsoft.CSharp.RuntimeBinder.RuntimeBinderException that we can't access.
                OperationKindEx.EventReference => FromOperation(IMemberReferenceOperationWrapper.FromOperation(operation.Instance)),
                OperationKindEx.FieldReference => FromOperation(IMemberReferenceOperationWrapper.FromOperation(operation.Instance)),
                OperationKindEx.Invocation => ExceptionState.UnknownException,
                OperationKindEx.MethodReference => FromOperation(IMemberReferenceOperationWrapper.FromOperation(operation.Instance)),
                OperationKindEx.ObjectCreation => operation.Instance.Type.DerivesFrom(KnownType.System_Exception) ? null : ExceptionState.UnknownException,
                OperationKindEx.PropertyReference => FromOperation(IMemberReferenceOperationWrapper.FromOperation(operation.Instance)),
                _ => null
            };

        private ExceptionState FromOperation(IArrayElementReferenceOperationWrapper reference) =>
            reference.Indices.Any(x => x.Kind == OperationKindEx.Range) // In case of Range, ArgumentOutOfRangeException is raised
                ? new ExceptionState(typeCatalog.SystemArgumentOutOfRangeException)
                : new ExceptionState(typeCatalog.SystemIndexOutOfRangeException);

        private ExceptionState FromOperation(IMemberReferenceOperationWrapper reference) =>
            reference.IsStaticOrThis() ? null : new ExceptionState(typeCatalog.SystemNullReferenceException);

        private ExceptionState ConversionExceptionCandidate(IOperationWrapperSonar operation)
        {
            if (operation.IsImplicit)
            {
                return null;
            }

            var conversion = IConversionOperationWrapper.FromOperation(operation.Instance);
            return conversion.Operand.Type.DerivesOrImplements(conversion.Type)
                       ? null
                       : new ExceptionState(typeCatalog.SystemInvalidCastException);
        }
    }
}
