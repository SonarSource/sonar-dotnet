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

using System.Security.Cryptography;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks;

public abstract class CryptographySymbolicRuleCheck : SymbolicRuleCheck
{
    protected override ProgramState PreProcessSimple(SymbolicContext context)
    {
        var state = context.State;
        var operation = context.Operation.Instance;

        if (operation.AsArrayCreation() is { } arrayCreation)
        {
            return ProcessArrayCreation(state, arrayCreation) ?? state;
        }
        else if (operation.AsInvocation() is { } invocation)
        {
            return ProcessInvocation(state, invocation) ?? state;
        }
        else
        {
            return context.State;
        }
    }

    protected virtual ProgramState ProcessArrayCreation(ProgramState state, IArrayCreationOperationWrapper arrayCreation) =>
        arrayCreation.Type.Is(KnownType.System_Byte_Array) && arrayCreation.DimensionSizes.Length == 1
            ? state.SetOperationConstraint(arrayCreation, ByteCollectionConstraint.CryptographicallyWeak)
            : null;

    protected virtual ProgramState ProcessInvocation(ProgramState state, IInvocationOperationWrapper invocation) =>
        IsCryptographicallyStrongRandomNumberGenerator(invocation)
        && invocation.ArgumentValue("data") is { } byteArray
        && state.ResolveCaptureAndUnwrapConversion(byteArray).TrackedSymbol() is { } byteArraySymbol
            ? state.SetSymbolConstraint(byteArraySymbol, ByteCollectionConstraint.CryptographicallyStrong)
            : null;

    private static bool IsCryptographicallyStrongRandomNumberGenerator(IInvocationOperationWrapper invocation) =>
        (invocation.TargetMethod.Name.Equals(nameof(RandomNumberGenerator.GetBytes)) || invocation.TargetMethod.Name.Equals(nameof(RandomNumberGenerator.GetNonZeroBytes)))
        && invocation.TargetMethod.ContainingType.DerivesFrom(KnownType.System_Security_Cryptography_RandomNumberGenerator);
}
