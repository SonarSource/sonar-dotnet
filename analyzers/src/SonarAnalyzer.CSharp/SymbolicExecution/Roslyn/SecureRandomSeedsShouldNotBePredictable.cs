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
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;

public sealed class SecureRandomSeedsShouldNotBePredictable : SymbolicRuleCheck
{
    private const string DiagnosticId = "S4347";
    private const string MessageFormat = "Set an unpredictable seed before generating random values.";
    private static readonly ImmutableHashSet<string> SecureRandomNextMethods = ImmutableHashSet.Create(
               "Next",
               "NextInt",
               "NextLong",
               "NextDouble",
               "NextBytes");

    public static readonly DiagnosticDescriptor S4347 = DescriptorFactory.Create(DiagnosticId, MessageFormat);
    protected override DiagnosticDescriptor Rule => S4347;

    public override bool ShouldExecute() => true;

    protected override ProgramState PreProcessSimple(SymbolicContext context)
    {
        var state = context.State;
        var operation = context.Operation.Instance;

        if (operation.AsArrayCreation() is { } arrayCreation)
        {
            return ProcessArrayCreation(state, arrayCreation);
        }
        else if (operation.AsArrayElementReference() is { } arrayElementReference)
        {
            return ProcessArrayElementReference(state, arrayElementReference);
        }
        else if (operation.AsInvocation() is { } invocation)
        {
            return ProcessArraySetValue(state, invocation)
                ?? ProcessStringToBytes(state, invocation)
                ?? ProcessSecureRandomGetInstance(state, invocation)
                ?? ProcessSecureRandomSetSeed(state, invocation)
                ?? ProcessSecureRandomNext(state, invocation)
                ?? state;
        }
        return state;
    }

    private static ProgramState ProcessArrayCreation(ProgramState state, IArrayCreationOperationWrapper arrayCreation)
    {
        if (arrayCreation.Type.IsAny(KnownType.System_Byte_Array, KnownType.System_Char_Array))
        {
            var isConstant = arrayCreation.Initializer.WrappedOperation is null || arrayCreation.Initializer.ElementValues.All(x => x.ConstantValue.HasValue);
            return state.SetOperationConstraint(
                arrayCreation,
                isConstant ? CryptographicSeedConstraint.Predictable : CryptographicSeedConstraint.Unpredictable);
        }
        return state;
    }

    private static ProgramState ProcessArrayElementReference(ProgramState state, IArrayElementReferenceOperationWrapper arrayElementReference) =>
        (arrayElementReference.IsAssignmentTarget() || arrayElementReference.IsCompoundAssignmentTarget())
        && arrayElementReference.ArrayReference.TrackedSymbol(state) is { } array
            ? state.SetSymbolConstraint(array, CryptographicSeedConstraint.Unpredictable)
            : state;

    private static ProgramState ProcessArraySetValue(ProgramState state, IInvocationOperationWrapper invocation)
    {
        if (invocation.TargetMethod.Name == nameof(Array.SetValue)
            && invocation.TargetMethod.ContainingType.Is(KnownType.System_Array))
        {
            return invocation.ArgumentValue("value") is { ConstantValue.HasValue: true }
                ? state
                : state.SetSymbolConstraint(invocation.Instance.TrackedSymbol(state), CryptographicSeedConstraint.Unpredictable);
        }
        return null;
    }

    private static ProgramState ProcessSecureRandomGetInstance(ProgramState state, IInvocationOperationWrapper invocation) =>
        invocation.TargetMethod.Name == "GetInstance"
        && invocation.TargetMethod.ContainingType.DerivesFrom(KnownType.Org_BouncyCastle_Security_SecureRandom)
        && invocation.Arguments.Length == 2
        && invocation.ArgumentValue("autoSeed") is { ConstantValue: { HasValue: true, Value: false } }
            ? state.SetOperationConstraint(invocation, CryptographicSeedConstraint.Predictable)
            : null;

    private ProgramState ProcessStringToBytes(ProgramState state, IInvocationOperationWrapper invocation)
    {
        return (IsEncodingGetBytes() || IsConvertFromBase64String() || IsConvertFromBase64CharArray())
            ? state.SetOperationConstraint(invocation, CryptographicSeedConstraint.Predictable)
            : null;

        bool IsEncodingGetBytes() =>
            invocation.TargetMethod.Name == nameof(Encoding.UTF8.GetBytes)
            && invocation.TargetMethod.ContainingType.DerivesFrom(KnownType.System_Text_Encoding)
            && (invocation.ArgumentValue("s") is { ConstantValue.HasValue: true } || ArgumentIsPredictable("chars"));

        bool IsConvertFromBase64CharArray() =>
            invocation.TargetMethod.Name == nameof(Convert.FromBase64CharArray)
            && invocation.TargetMethod.ContainingType.Is(KnownType.System_Convert)
            && ArgumentIsPredictable("inArray");

        bool IsConvertFromBase64String() =>
            invocation.TargetMethod.Name == nameof(Convert.FromBase64String)
            && invocation.TargetMethod.ContainingType.Is(KnownType.System_Convert)
            && invocation.ArgumentValue("s") is { ConstantValue.HasValue: true };

        bool ArgumentIsPredictable(string parameterName) =>
            invocation.ArgumentValue(parameterName) is { } value
            && state[value]?.HasConstraint(CryptographicSeedConstraint.Predictable) is true;
    }

    private static ProgramState ProcessSecureRandomSetSeed(ProgramState state, IInvocationOperationWrapper invocation)
    {
        if (invocation.TargetMethod.Name == "SetSeed"
        && invocation.TargetMethod.ContainingType.DerivesFrom(KnownType.Org_BouncyCastle_Security_SecureRandom)
        // If it is already unpredictable, do nothing. SecureRandom.SetSeed() does not overwrite the state, but _mixes_ it with the new value.
        && state[invocation.Instance]?.HasConstraint(CryptographicSeedConstraint.Unpredictable) is false
        && invocation.ArgumentValue("seed") is { } argumentValue)
        {
            var constraint = CryptographicSeedConstraint.Unpredictable;
            if (argumentValue.ConstantValue.HasValue)
            {
                constraint = CryptographicSeedConstraint.Predictable;
            }
            else if (state[argumentValue]?.Constraint<CryptographicSeedConstraint>() is { } value)
            {
                constraint = value;
            }
            return state.SetSymbolConstraint(invocation.Instance.TrackedSymbol(state), constraint);
        }
        return null;
    }

    private ProgramState ProcessSecureRandomNext(ProgramState state, IInvocationOperationWrapper invocation)
    {
        if (SecureRandomNextMethods.Contains(invocation.TargetMethod.Name)
            && invocation.TargetMethod.ContainingType.DerivesFrom(KnownType.Org_BouncyCastle_Security_SecureRandom)
            && state[invocation.Instance]?.HasConstraint(CryptographicSeedConstraint.Predictable) is true)
        {
            ReportIssue(invocation.WrappedOperation);
        }
        return state;
    }
}
