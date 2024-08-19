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

using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn;

internal class ExceptionCandidate
{
    private readonly TypeCatalog typeCatalog;

    public ExceptionCandidate(Compilation compilation) =>
        typeCatalog = new TypeCatalog(compilation);

    public ExceptionState FromOperation(ProgramState state, IOperationWrapperSonar operation) =>
        operation.Instance.Kind switch
        {
            OperationKindEx.ArrayElementReference => FromOperation(operation.Instance.ToArrayElementReference()),
            OperationKindEx.Conversion => FromConversion(operation),
            OperationKindEx.DynamicIndexerAccess => new ExceptionState(typeCatalog.SystemIndexOutOfRangeException),
            OperationKindEx.DynamicInvocation => ExceptionState.UnknownException,      // This raises is Microsoft.CSharp.RuntimeBinder.RuntimeBinderException that we can't access.
            OperationKindEx.DynamicMemberReference => ExceptionState.UnknownException, // This raises is Microsoft.CSharp.RuntimeBinder.RuntimeBinderException that we can't access.
            OperationKindEx.DynamicObjectCreation => ExceptionState.UnknownException,  // This raises is Microsoft.CSharp.RuntimeBinder.RuntimeBinderException that we can't access.
            OperationKindEx.EventReference => FromOperation(state, operation.Instance.ToMemberReference()),
            OperationKindEx.FieldReference => FromOperation(state, operation.Instance.ToMemberReference()),
            OperationKindEx.Invocation => FromOperation(operation.Instance.ToInvocation()),
            OperationKindEx.MethodReference => FromOperation(state, operation.Instance.ToMemberReference()),
            OperationKindEx.ObjectCreation => operation.Instance.Type.DerivesFrom(KnownType.System_Exception) ? null : ExceptionState.UnknownException,
            OperationKindEx.PropertyReference => FromOperation(state, operation.Instance.ToMemberReference()),
            OperationKindEx.Binary => FromOperation(operation.Instance.ToBinary()),
            OperationKindEx.CompoundAssignment => FromOperation(operation.Instance.ToCompoundAssignment()),
            _ => null
        };

    private ExceptionState FromConversion(IOperationWrapperSonar operation)
    {
        if (operation.IsImplicit)
        {
            return null;
        }

        var conversion = operation.Instance.ToConversion();
        return conversion.Operand.Type.DerivesOrImplements(conversion.Type)
                   ? null
                   : new ExceptionState(typeCatalog.SystemInvalidCastException);
    }

    private ExceptionState FromOperation(IArrayElementReferenceOperationWrapper reference) =>
        reference.Indices.Any(x => x.Kind == OperationKindEx.Range) // In case of Range, ArgumentOutOfRangeException is raised
            ? new ExceptionState(typeCatalog.SystemArgumentOutOfRangeException)
            : new ExceptionState(typeCatalog.SystemIndexOutOfRangeException);

    private ExceptionState FromOperation(ProgramState state, IMemberReferenceOperationWrapper reference) =>
        reference.IsStaticOrThis(state)
        || state[reference.Instance]?.HasConstraint(ObjectConstraint.NotNull) is true
        || reference.IsOnReaderWriterLockOrSlim()   // Needed by S2222
            ? null
            : new ExceptionState(typeCatalog.SystemNullReferenceException);

    private static ExceptionState FromOperation(IInvocationOperationWrapper invocation) =>
        // These methods are declared as well-known methods that (usually) do not throw.
        // Otherwise, we would have FPs because engine would split the flow to happy path with constraints and possible exception path.
        invocation.IsMonitorExit()          // Needed by S2222
        || invocation.IsMonitorIsEntered()  // Needed by S2222
        || invocation.IsLockRelease()       // Needed by S2222
        ? null
        : ExceptionState.UnknownException;

    private ExceptionState FromOperation(IBinaryOperationWrapper binary) =>
        IsDivision(binary.OperatorKind)
            ? new ExceptionState(typeCatalog.SystemDivideByZeroException)
            : null;

    private ExceptionState FromOperation(ICompoundAssignmentOperationWrapper compoundAssignment) =>
        IsDivision(compoundAssignment.OperatorKind)
            ? new ExceptionState(typeCatalog.SystemDivideByZeroException)
            : null;

    private static bool IsDivision(BinaryOperatorKind operatorKind) =>
        operatorKind is BinaryOperatorKind.Divide or BinaryOperatorKind.Remainder or BinaryOperatorKind.IntegerDivide;
}
