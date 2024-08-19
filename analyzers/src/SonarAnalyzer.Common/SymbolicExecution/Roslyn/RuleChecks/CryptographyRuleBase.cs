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

using System.Security.Cryptography;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks;

public abstract class CryptographyRuleBase : SymbolicRuleCheck
{
    protected override ProgramState PreProcessSimple(SymbolicContext context)
    {
        if (context.Operation.Instance.AsArrayCreation() is { } arrayCreation)
        {
            return ProcessArrayCreation(context.State, arrayCreation) ?? context.State;
        }
        else if (context.Operation.Instance.AsInvocation() is { } invocation)
        {
            return ProcessInvocation(context.State, invocation);
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

    protected virtual ProgramState ProcessInvocation(ProgramState state, IInvocationOperationWrapper invocation) => IsCryptographicallyStrongRandomNumberGenerator(invocation)
        && FindInvocationArgument(state, invocation.Arguments, KnownType.System_Byte_Array) is { } dataArgument
        && dataArgument.TrackedSymbol(state) is { } trackedSymbol
            ? state.SetSymbolConstraint(trackedSymbol, ByteCollectionConstraint.CryptographicallyStrong)
            : state;

    protected static IOperation FindInvocationArgument(ProgramState state, ImmutableArray<IOperation> arguments, KnownType argumentType, string[] nameCandidates = null) =>
        arguments.FirstOrDefault(x => IsArgumentMatch(state, x, argumentType, nameCandidates))?.AsArgument() is { } argument
            ? state.ResolveCaptureAndUnwrapConversion(argument.Value)
            : null;

    private static bool IsArgumentMatch(ProgramState state, IOperation operation, KnownType argumentType, string[] nameCandidates = null) =>
        operation.AsArgument() is { } argument
        && (nameCandidates is null || nameCandidates.Contains(argument.Parameter.Name))
        && state.ResolveCaptureAndUnwrapConversion(argument.Value) is { } argumentValue
        && argumentValue.Type.Is(argumentType);

    private static bool IsCryptographicallyStrongRandomNumberGenerator(IInvocationOperationWrapper invocation) =>
        invocation.TargetMethod.Name is nameof(RandomNumberGenerator.GetBytes) or nameof(RandomNumberGenerator.GetNonZeroBytes)
        && invocation.TargetMethod.ContainingType.DerivesFrom(KnownType.System_Security_Cryptography_RandomNumberGenerator);
}
