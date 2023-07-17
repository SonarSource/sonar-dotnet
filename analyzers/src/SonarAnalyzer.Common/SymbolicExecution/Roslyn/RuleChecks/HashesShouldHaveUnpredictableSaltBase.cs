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

public abstract class HashesShouldHaveUnpredictableSaltBase : SymbolicRuleCheck
{
    protected const string DiagnosticId = "S2053";
    protected const string MessageFormat = "{0}";

    private const int SafeSaltSize = 16;
    private const string MakeSaltUnpredictableMessage = "Make this salt unpredictable.";
    private const string MakeThisSaltLongerMessage = "Make this salt at least 16 bytes.";

    protected override ProgramState PreProcessSimple(SymbolicContext context)
    {
        var state = context.State;
        var instance = context.Operation.Instance;
        if (instance.AsObjectCreation() is { } objectCreation)
        {
            ProcessObjectCreation(state, objectCreation);
        }
        else if (instance.AsInvocation() is { } invocation)
        {
            return ProcessInvocation(state, invocation);
        }
        else if (instance.AsArrayCreation() is { } arrayCreation)
        {
            return ProcessArrayCreation(state, arrayCreation);
        }
        return state;
    }

    private void ProcessObjectCreation(ProgramState state, IObjectCreationOperationWrapper objectCreation)
    {
        if (objectCreation.Type.DerivesFrom(KnownType.System_Security_Cryptography_DeriveBytes)
            && objectCreation.Arguments.FirstOrDefault(x => x.AsArgument() is { Parameter.Name: "salt" or "rgbSalt" }) is { } saltArgument)
        {
            if (state[saltArgument]?.HasConstraint(ByteCollectionConstraint.CryptographicallyWeak) is true)
            {
                ReportIssue(saltArgument, MakeSaltUnpredictableMessage);
            }
            else if (state[saltArgument]?.HasConstraint(SaltSizeConstraint.Short) is true)
            {
                ReportIssue(saltArgument, MakeThisSaltLongerMessage);
            }
        }
    }

    private static ProgramState ProcessInvocation(ProgramState state, IInvocationOperationWrapper invocation) =>
        IsCryptographicallyStrongRandomNumberGenerator(invocation)
        && invocation.ArgumentValue("data") is { } dataArgument
        && dataArgument.TrackedSymbol() is { } trackedSymbol
            ? state.SetSymbolConstraint(trackedSymbol, ByteCollectionConstraint.CryptographicallyStrong)
            : state;

    private static ProgramState ProcessArrayCreation(ProgramState state, IArrayCreationOperationWrapper arrayCreation)
    {
        if (arrayCreation.Type.Is(KnownType.System_Byte_Array) && arrayCreation.DimensionSizes.Length == 1)
        {
            state = state.SetOperationConstraint(arrayCreation, ByteCollectionConstraint.CryptographicallyWeak);

            if (arrayCreation.DimensionSizes.Single().ConstantValue.Value is int arraySize && arraySize < SafeSaltSize)
            {
                state = state.SetOperationConstraint(arrayCreation, SaltSizeConstraint.Short);
            }
        }
        return state;
    }

    private static bool IsCryptographicallyStrongRandomNumberGenerator(IInvocationOperationWrapper invocation) =>
      (invocation.TargetMethod.Name.Equals(nameof(RandomNumberGenerator.GetBytes)) || invocation.TargetMethod.Name.Equals(nameof(RandomNumberGenerator.GetNonZeroBytes)))
      && invocation.TargetMethod.ContainingType.DerivesFrom(KnownType.System_Security_Cryptography_RandomNumberGenerator);
}
