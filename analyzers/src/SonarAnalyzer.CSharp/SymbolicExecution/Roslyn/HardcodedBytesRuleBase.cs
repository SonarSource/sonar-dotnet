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

using System.Text;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;

public abstract class HardcodedBytesRuleBase : SymbolicRuleCheck
{
    protected abstract SymbolicConstraint Hardcoded { get; }
    protected abstract SymbolicConstraint NotHardcoded { get; }

    // new byte/char[] { ... }
    // new byte/char[42]
    protected ProgramState ProcessArrayCreation(ProgramState state, IArrayCreationOperationWrapper arrayCreation)
    {
        if (arrayCreation.Type.IsAny(KnownType.System_Byte_Array, KnownType.System_Char_Array))
        {
            var isConstant = arrayCreation.Initializer.WrappedOperation is null || arrayCreation.Initializer.ElementValues.All(x => x.ConstantValue.HasValue);
            return state.SetOperationConstraint(arrayCreation, isConstant ? Hardcoded : NotHardcoded);
        }
        return state;
    }

    // array[42] = ...
    protected ProgramState ProcessArrayElementReference(ProgramState state, IArrayElementReferenceOperationWrapper arrayElementReference)
    {
        return (IsAssignedToNonConstant() || IsCompoundAssignedToNonConstant())
        && arrayElementReference.ArrayReference.TrackedSymbol(state) is { } array
            ? state.SetSymbolConstraint(array, NotHardcoded)
            : state;

        bool IsAssignedToNonConstant() =>
            arrayElementReference.IsAssignmentTarget()
            && !ISimpleAssignmentOperationWrapper.FromOperation(arrayElementReference.ToSonar().Parent).Value.ConstantValue.HasValue;

        bool IsCompoundAssignedToNonConstant() =>
            arrayElementReference.IsCompoundAssignmentTarget()
            && !ICompoundAssignmentOperationWrapper.FromOperation(arrayElementReference.ToSonar().Parent).Value.ConstantValue.HasValue;
    }

    // array.SetValue(value, index)
    protected ProgramState ProcessArraySetValue(ProgramState state, IInvocationOperationWrapper invocation)
    {
        if (invocation.TargetMethod.Name == nameof(Array.SetValue)
            && invocation.TargetMethod.ContainingType.Is(KnownType.System_Array)
            && invocation.Instance.TrackedSymbol(state) is { } array)
        {
            return invocation.ArgumentValue("value") is { ConstantValue.HasValue: true }
                       ? state
                       : state.SetSymbolConstraint(array, NotHardcoded);
        }
        return null;
    }

    // array.Initialize()
    protected ProgramState ProcessArrayInitialize(ProgramState state, IInvocationOperationWrapper invocation) =>
        invocation.TargetMethod.Name == nameof(Array.Initialize)
        && invocation.TargetMethod.ContainingType.Is(KnownType.System_Array)
        && invocation.Instance.TrackedSymbol(state) is { } array
            ? state.SetSymbolConstraint(array, Hardcoded)
            : null;

    // Encoding.UTF8.GetBytes(s)
    // Convert.FromBase64CharArray(chars, ...)
    // Convert.FromBase64String(s)
    protected ProgramState ProcessStringToBytes(ProgramState state, IInvocationOperationWrapper invocation)
    {
        return (IsEncodingGetBytes() || IsConvertFromBase64String() || IsConvertFromBase64CharArray())
                   ? state.SetOperationConstraint(invocation, Hardcoded)
                   : null;

        bool IsEncodingGetBytes() =>
            invocation.TargetMethod.Name == nameof(Encoding.UTF8.GetBytes)
            && invocation.TargetMethod.ContainingType.DerivesFrom(KnownType.System_Text_Encoding)
            && (ArgumentIsHardcoded("s") || ArgumentIsHardcoded("chars"));

        bool IsConvertFromBase64CharArray() =>
            invocation.TargetMethod.Name == nameof(Convert.FromBase64CharArray)
            && invocation.TargetMethod.ContainingType.Is(KnownType.System_Convert)
            && ArgumentIsHardcoded("inArray");

        bool IsConvertFromBase64String() =>
            invocation.TargetMethod.Name == nameof(Convert.FromBase64String)
            && invocation.TargetMethod.ContainingType.Is(KnownType.System_Convert)
            && ArgumentIsHardcoded("s");

        bool ArgumentIsHardcoded(string parameterName) =>
            invocation.ArgumentValue(parameterName) is { } value
            && (value.ConstantValue.HasValue || state[value]?.HasConstraint(Hardcoded) is true);
    }
}
