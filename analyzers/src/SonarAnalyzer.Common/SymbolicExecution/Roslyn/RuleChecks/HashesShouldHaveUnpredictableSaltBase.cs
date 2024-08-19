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

using System.Numerics;
using System.Text;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks;

public abstract class HashesShouldHaveUnpredictableSaltBase : CryptographyRuleBase
{
    protected const string DiagnosticId = "S2053";
    protected const string MessageFormat = "{0}";

    private const string MakeSaltUnpredictableMessage = "Make this salt unpredictable.";
    private const string MakeThisSaltLongerMessage = "Make this salt at least 16 bytes.";
    private static readonly BigInteger SafeSaltSize = new(16);
    private static readonly string[] DeriveBytesSaltParameterNames = new[] { "salt", "rgbSalt" };

    protected override ProgramState PreProcessSimple(SymbolicContext context)
    {
        var state = base.PreProcessSimple(context);
        return context.Operation.Instance.AsObjectCreation() is { } objectCreation
            ? ProcessObjectCreation(state, objectCreation)
            : state;
    }

    protected override ProgramState ProcessArrayCreation(ProgramState state, IArrayCreationOperationWrapper arrayCreation)
    {
        if (base.ProcessArrayCreation(state, arrayCreation) is { } newState)
        {
            return newState[arrayCreation.DimensionSizes.Single()].Constraint<NumberConstraint>() is { } arraySizeConstraint
                && arraySizeConstraint.Max < SafeSaltSize
                    ? newState.SetOperationConstraint(arrayCreation, SaltSizeConstraint.Short)
                    : newState;
        }
        return state;
    }

    private ProgramState ProcessObjectCreation(ProgramState state, IObjectCreationOperationWrapper objectCreation)
    {
        if (objectCreation.Type.DerivesFrom(KnownType.System_Security_Cryptography_DeriveBytes)
            && FindInvocationArgument(state, objectCreation.Arguments, KnownType.System_Byte_Array, DeriveBytesSaltParameterNames) is { } saltArgument)
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

    protected override ProgramState ProcessInvocation(ProgramState state, IInvocationOperationWrapper invocation)
    {
        state = base.ProcessInvocation(state, invocation);
        return invocation.TargetMethod.Is(KnownType.System_Text_Encoding, nameof(Encoding.GetBytes))
            && FindInvocationArgument(state, invocation.Arguments, KnownType.System_String)?.Kind == OperationKindEx.Literal
                ? state.SetOperationConstraint(invocation, ByteCollectionConstraint.CryptographicallyWeak)
                : state;
    }
}
