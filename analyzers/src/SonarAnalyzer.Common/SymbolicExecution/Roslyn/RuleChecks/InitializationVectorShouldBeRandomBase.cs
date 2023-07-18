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

public abstract class InitializationVectorShouldBeRandomBase : CryptographySymbolicRuleCheck
{
    protected const string DiagnosticId = "S3329";
    protected const string MessageFormat = "Use a dynamically-generated, random IV.";

    protected override ProgramState PreProcessSimple(SymbolicContext context)
    {
        var state = base.PreProcessSimple(context);
        var operation = context.Operation.Instance;

        if (operation.AsAssignment() is { } assignment)
        {
            return ProcessAssignmentToIVProperty(state, assignment) ?? state;
        }
        else if (operation.AsPropertyReference() is { } property
            && state.ResolveCaptureAndUnwrapConversion(property.Instance).TrackedSymbol() is { } propertyInstance
            && IsIVProperty(property, propertyInstance)
            && state[property.Instance]?.Constraint<ByteCollectionConstraint>() is { } constraint)
        {
            return state.SetOperationConstraint(property, constraint);
        }
        else
        {
            return state;
        }
    }

    protected override ProgramState ProcessInvocation(ProgramState state, IInvocationOperationWrapper invocation) =>
        base.ProcessInvocation(state, invocation)
        ?? ProcessGenerateIV(state, invocation)
        ?? ProcessCreateEncryptorMethodInvocation(state, invocation)
        ?? state;

    private static ProgramState ProcessAssignmentToIVProperty(ProgramState state, IAssignmentOperationWrapper assignment) =>
        assignment.Target?.AsPropertyReference() is { } property
        && state.ResolveCaptureAndUnwrapConversion(property.Instance).TrackedSymbol() is { } propertyInstance
        && IsIVProperty(property, propertyInstance)
        && state[assignment.Value].HasConstraint(ByteCollectionConstraint.CryptographicallyWeak)
            ? state.SetSymbolConstraint(propertyInstance, ByteCollectionConstraint.CryptographicallyWeak)
            : null;

    private static ProgramState ProcessGenerateIV(ProgramState state, IInvocationOperationWrapper invocation) =>
        invocation.TargetMethod.Name.Equals(nameof(SymmetricAlgorithm.GenerateIV))
        && invocation.TargetMethod.ContainingType.DerivesFrom(KnownType.System_Security_Cryptography_SymmetricAlgorithm)
            ? state.SetSymbolConstraint(invocation.Instance.TrackedSymbol(), ByteCollectionConstraint.CryptographicallyStrong)
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

    private static bool IsIVProperty(IPropertyReferenceOperationWrapper property, ISymbol propertyInstance) =>
        property.Property.Name.Equals("IV")
        && propertyInstance.GetSymbolType().DerivesFrom(KnownType.System_Security_Cryptography_SymmetricAlgorithm);
}
