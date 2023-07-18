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

    protected override ProgramState PreProcessSimple(SymbolicContext context)
    {
        var state = context.State;
        var operation = context.Operation.Instance;

        if (operation.AsArrayCreation() is { } arrayCreation
            && arrayCreation.Type.Is(KnownType.System_Byte_Array)
            && arrayCreation.DimensionSizes.Length == 1)
        {
            return state.SetOperationConstraint(arrayCreation, ByteCollectionConstraint.CryptographicallyWeak);
        }
        else if (operation.AsAssignment() is { } assignment)
        {
            return ProcessAssignmentToIVProperty(state, assignment)
                   ?? state;
        }
        else if (operation.AsPropertyReference() is { } property
                 && property.Instance is { } propertyInstance
                 && state.ResolveCaptureAndUnwrapConversion(propertyInstance).TrackedSymbol() is { } propertyInstanceSymbol
                 && IsIVProperty(property, propertyInstanceSymbol)
                 && state[propertyInstance]?.Constraint<ByteCollectionConstraint>() is { } constraint)
        {
            return state.SetOperationConstraint(property, constraint);
        }
        else if (operation.AsInvocation() is { } invocation)
        {
            return ProcessStrongRandomGeneratorMethodInvocation(state, invocation)
                   ?? ProcessGenerateIV(state, invocation)
                   ?? ProcessCreateEncryptorMethodInvocation(state, invocation)
                   ?? state;
        }
        else
        {
            return state;
        }
    }

    private static ProgramState ProcessAssignmentToIVProperty(ProgramState state, IAssignmentOperationWrapper assignment) =>
        assignment.Target?.AsPropertyReference() is { } property
        && property.Instance is { } propertyInstance
        && state.ResolveCaptureAndUnwrapConversion(propertyInstance).TrackedSymbol() is { } propertyInstanceSymbol
        && IsIVProperty(property, propertyInstanceSymbol)
        && state[assignment.Value].HasConstraint(ByteCollectionConstraint.CryptographicallyWeak)
            ? state.SetSymbolConstraint(propertyInstanceSymbol, ByteCollectionConstraint.CryptographicallyWeak)
            : null;

    private static ProgramState ProcessStrongRandomGeneratorMethodInvocation(ProgramState state, IInvocationOperationWrapper invocation) =>
        IsCryptographicallyStrongRandomNumberGenerator(invocation)
        && invocation.ArgumentValue("data") is { } byteArray
        && state.ResolveCaptureAndUnwrapConversion(byteArray).TrackedSymbol() is { } byteArraySymbol
            ? state.SetSymbolConstraint(byteArraySymbol, ByteCollectionConstraint.CryptographicallyStrong)
            : null;

    private static ProgramState ProcessGenerateIV(ProgramState state, IInvocationOperationWrapper invocation) =>
        invocation.TargetMethod.Name.Equals(nameof(SymmetricAlgorithm.GenerateIV))
        && invocation.TargetMethod.ContainingType.DerivesFrom(KnownType.System_Security_Cryptography_SymmetricAlgorithm)
            ? state.SetSymbolConstraint(state.ResolveCaptureAndUnwrapConversion(invocation.Instance).TrackedSymbol(), ByteCollectionConstraint.CryptographicallyStrong)
            : null;

    private ProgramState ProcessCreateEncryptorMethodInvocation(ProgramState state, IInvocationOperationWrapper invocation)
    {
        if (IsCreateEncryptorMethod(invocation)
            && (ArgumentIsCryptographicallyWeak(state, invocation) || UsesCryptographicallyWeakIVProperty(state, invocation)))
        {
            ReportIssue(invocation.WrappedOperation, invocation.Instance.Syntax.ToString());
        }
        return state;

        bool ArgumentIsCryptographicallyWeak(ProgramState state, IInvocationOperationWrapper invocation) =>
            invocation.Arguments.Any(x => state[x]?.HasConstraint(ByteCollectionConstraint.CryptographicallyWeak) is true);

        bool UsesCryptographicallyWeakIVProperty(ProgramState state, IInvocationOperationWrapper invocation) =>
            invocation.Arguments.Length == 0  && state[invocation.Instance]?.HasConstraint(ByteCollectionConstraint.CryptographicallyWeak) is true;
    }

    private static bool IsCreateEncryptorMethod(IInvocationOperationWrapper invocation) =>
        invocation.TargetMethod.Name.Equals(nameof(SymmetricAlgorithm.CreateEncryptor))
        && invocation.TargetMethod.ContainingType.DerivesFrom(KnownType.System_Security_Cryptography_SymmetricAlgorithm);

    private static bool IsCryptographicallyStrongRandomNumberGenerator(IInvocationOperationWrapper invocation) =>
        (invocation.TargetMethod.Name.Equals(nameof(RandomNumberGenerator.GetBytes)) || invocation.TargetMethod.Name.Equals(nameof(RandomNumberGenerator.GetNonZeroBytes)))
        && invocation.TargetMethod.ContainingType.DerivesFrom(KnownType.System_Security_Cryptography_RandomNumberGenerator);

    private static bool IsIVProperty(IPropertyReferenceOperationWrapper property, ISymbol propertyInstance) =>
        property.Property.Name.Equals("IV")
        && propertyInstance.GetSymbolType().DerivesFrom(KnownType.System_Security_Cryptography_SymmetricAlgorithm);
}
