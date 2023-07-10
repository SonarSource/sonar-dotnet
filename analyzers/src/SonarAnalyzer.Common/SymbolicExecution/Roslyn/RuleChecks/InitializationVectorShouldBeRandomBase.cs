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

public abstract class InitializationVectorShouldBeRandomBase : SymbolicRuleCheck
{
    protected const string DiagnosticId = "S3329";
    protected const string MessageFormat = "Use a dynamically-generated, random IV.";

    private static readonly ImmutableArray<MemberDescriptor> CryptographicallyStrongRandomNumberGenerators =
        ImmutableArray.Create(
            new MemberDescriptor(KnownType.System_Security_Cryptography_RandomNumberGenerator, nameof(RandomNumberGenerator.GetBytes)),
            new MemberDescriptor(KnownType.System_Security_Cryptography_RandomNumberGenerator, nameof(RandomNumberGenerator.GetNonZeroBytes)));

    private static bool IsCreateEncryptorMethod(IInvocationOperationWrapper invocation) =>
        invocation.TargetMethod.Is(KnownType.System_Security_Cryptography_SymmetricAlgorithm, nameof(SymmetricAlgorithm.CreateEncryptor));

    private static bool IsGenerateIVMethod(IInvocationOperationWrapper invocation) =>
        invocation.TargetMethod.Is(KnownType.System_Security_Cryptography_SymmetricAlgorithm, nameof(SymmetricAlgorithm.GenerateIV));

    protected override ProgramState PreProcessSimple(SymbolicContext context)
    {
        var state = context.State;
        if (context.Operation.Instance.AsAssignment() is { } assignment)
        {
            state = ProcessAssignment(assignment, state);
        }
        if (context.Operation.Instance.AsInvocation() is { } invocation)
        {
            state = ProcessInvocation(invocation, state);

            if (invocation.Instance is { } inv && state[inv]?.HasConstraint(InitializationVectorConstraint.NotInitialized) is true)
            {
                ReportIssue(context.Operation.Instance, invocation.Instance.Syntax.ToString());
            }
        }
        return state;
    }

    private static ProgramState ProcessAssignment(IAssignmentOperationWrapper assignment, ProgramState state)
    {
        var assignmentValueIsCryptographicallyStrong = assignment.Value.AsArrayCreation() is { } arrayCreation && !IsByteCollectionInitializedEmptyOrWithConstants(arrayCreation);

        if (!assignmentValueIsCryptographicallyStrong && assignment.Target.TrackedSymbol() is { } assignmentTargetSymbol)
        {
            state = state.SetSymbolConstraint(assignmentTargetSymbol, ByteArrayConstraint.Constant);
        }
        if (assignment.Target?.AsPropertyReference() is { } property
            && property.Property.Name.Equals("IV")
            && (!assignmentValueIsCryptographicallyStrong || state[assignment.Value.TrackedSymbol()].HasConstraint(ByteArrayConstraint.Constant)))
        {
            state = state.SetSymbolConstraint(property.Instance.TrackedSymbol(), InitializationVectorConstraint.NotInitialized);
        }

        return state;
    }

    private static ProgramState ProcessInvocation(IInvocationOperationWrapper invocation, ProgramState state)
    {
        if (CryptographicallyStrongRandomNumberGenerators.Any(x => IsStrongRandomGeneratorInvocation(x, invocation))
            && invocation.ArgumentValue("data") is { } byteArray
            && byteArray.TrackedSymbol() is { } byteArraySymbol)
        {
            state = state.SetSymbolConstraint(byteArraySymbol, ByteArrayConstraint.Modified);
        }
        if (IsGenerateIVMethod(invocation))
        {
            state = state.SetSymbolConstraint(invocation.Instance.TrackedSymbol(), InitializationVectorConstraint.Initialized);
        }
        if (IsCreateEncryptorMethod(invocation))
        {
            if (invocation.Arguments.Length == 2)
            {
                var ivArgument = invocation.ArgumentValue("rgbIV");
                if (ivArgument.AsArrayCreation() is { } IVArray && IsByteCollectionInitializedEmptyOrWithConstants(IVArray))
                {
                    state = state.SetSymbolConstraint(invocation.Instance.TrackedSymbol(), InitializationVectorConstraint.NotInitialized);
                }
                else if (ivArgument.AsArgument() is { } argument && state[argument]?.HasConstraint(ByteArrayConstraint.Constant) is true)
                {
                    state = state.SetSymbolConstraint(invocation.Instance.TrackedSymbol(), InitializationVectorConstraint.NotInitialized);
                }
            }
        }
        return state;

        static bool IsStrongRandomGeneratorInvocation(MemberDescriptor method, IInvocationOperationWrapper invocation) =>
            invocation.TargetMethod.Name == method.Name
            && invocation.TargetMethod.ContainingType.DerivesFrom(method.ContainingType);
    }

    private static bool IsByteCollectionInitializedEmptyOrWithConstants(IArrayCreationOperationWrapper arrayCreation) =>
            !arrayCreation.WrappedOperation.ConstantValue.HasValue || arrayCreation.Initializer.ElementValues.All(x => x.ConstantValue.HasValue);
}
