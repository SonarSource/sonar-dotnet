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

    private static readonly ImmutableArray<MemberDescriptor> CryptographicallyStrongRandomNumberGenerators = ImmutableArray.Create(
        new MemberDescriptor(KnownType.System_Security_Cryptography_RandomNumberGenerator, nameof(RandomNumberGenerator.GetBytes)),
        new MemberDescriptor(KnownType.System_Security_Cryptography_RandomNumberGenerator, nameof(RandomNumberGenerator.GetNonZeroBytes)));

    protected override ProgramState PreProcessSimple(SymbolicContext context)
    {
        var state = context.State;
        var instance = context.Operation.Instance;
        if (instance.AsObjectCreation() is { } objectCreation)
        {
            return ProcessObjectCreation(objectCreation, state);
        }
        else if (instance.AsInvocation() is { } invocation)
        {
            return ProcessInvocation(invocation, state);
        }
        else if (instance.AsArrayCreation() is { } arrayCreation)
        {
            return ProcessArrayCreation(arrayCreation, state);
        }
        else
        {
            return state;
        }
    }

    private ProgramState ProcessObjectCreation(IObjectCreationOperationWrapper objectCreation, ProgramState state)
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
        return state;
    }

    private static ProgramState ProcessInvocation(IInvocationOperationWrapper invocation, ProgramState state)
    {
        if (CryptographicallyStrongRandomNumberGenerators.Any(x => IsInvocationToRandomNumberGenerator(x, invocation))
            && invocation.ArgumentValue("data") is { } dataArgument
            && dataArgument.TrackedSymbol() is { } trackedSymbol)
        {
            state = state.SetSymbolConstraint(trackedSymbol, ByteCollectionConstraint.CryptographicallyStrong);
        }
        return state;
    }

    private static ProgramState ProcessArrayCreation(IArrayCreationOperationWrapper arrayCreation, ProgramState state)
    {
        if (arrayCreation.Type.Is(KnownType.System_Byte_Array) && arrayCreation.DimensionSizes.Length == 1)
        {
            state = state.SetOperationConstraint(arrayCreation.WrappedOperation, ByteCollectionConstraint.CryptographicallyWeak);

            if (arrayCreation.DimensionSizes.Single().ConstantValue.Value is int arraySize && arraySize < SafeSaltSize)
            {
                state = state.SetOperationConstraint(arrayCreation.WrappedOperation, SaltSizeConstraint.Short);
            }
        }
        return state;
    }

    private static bool IsInvocationToRandomNumberGenerator(MemberDescriptor methodDescriptor, IInvocationOperationWrapper invocation) =>
        invocation.TargetMethod.Name == methodDescriptor.Name
        && invocation.TargetMethod.ContainingType.DerivesFrom(methodDescriptor.ContainingType);
}
