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

using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;

public sealed class SecureRandomSeedsShouldNotBePredictable : HardcodedBytesRuleBase
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
    protected override SymbolicConstraint Hardcoded => CryptographicSeedConstraint.Predictable;
    protected override SymbolicConstraint NotHardcoded => CryptographicSeedConstraint.Unpredictable;

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
        else if (operation.AsObjectCreation() is { } objectCreation)
        {
            return ProcessObjectCreation(state, objectCreation);
        }
        else if (operation.AsInvocation() is { } invocation)
        {
            return ProcessArraySetValue(state, invocation)
                ?? ProcessArrayInitialize(state, invocation)
                ?? ProcessStringToBytes(state, invocation)
                ?? ProcessSecureRandomGetInstance(state, invocation)
                ?? ProcessSeedingMethods(state, invocation)
                ?? ProcessNextMethods(state, invocation)
                ?? state;
        }
        return state;
    }

    // new VmpcRandomGenerator()
    // new DigestRandomGenerator(digest)
    private static ProgramState ProcessObjectCreation(ProgramState state, IObjectCreationOperationWrapper objectCreation) =>
        objectCreation.Type.IsAny(KnownType.Org_BouncyCastle_Crypto_Prng_DigestRandomGenerator, KnownType.Org_BouncyCastle_Crypto_Prng_VmpcRandomGenerator)
            ? state.SetOperationConstraint(objectCreation, CryptographicSeedConstraint.Predictable)
            : state;

    // SecureRandom.GetInstance("algorithm", false)
    private static ProgramState ProcessSecureRandomGetInstance(ProgramState state, IInvocationOperationWrapper invocation) =>
        invocation.TargetMethod.Name == "GetInstance"
        && IsSecureRandom(invocation)
        && invocation.Arguments.Length == 2
        && invocation.ArgumentValue("autoSeed") is { ConstantValue: { HasValue: true, Value: false } }
            ? state.SetOperationConstraint(invocation, CryptographicSeedConstraint.Predictable)
            : null;

    // secureRandom.SetSeed(bytes/number)
    // randomGenerator.AddSeedMaterial(bytes/number)
    private static ProgramState ProcessSeedingMethods(ProgramState state, IInvocationOperationWrapper invocation)
    {
        if ((IsSetSeed() || IsAddSeedMaterial())
            && invocation.Instance is { } instance
            // If it is already unpredictable, do nothing.
            // Seeding methods do not overwrite the state, but _mix_ it with the new value.
            && state[instance]?.HasConstraint(CryptographicSeedConstraint.Unpredictable) is false
            && invocation.ArgumentValue("seed") is { } argumentValue
            && invocation.Instance.TrackedSymbol(state) is { } instanceSymbol)
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
            return state.SetSymbolConstraint(instanceSymbol, constraint);
        }
        return null;

        bool IsSetSeed() =>
            invocation.TargetMethod.Name == "SetSeed"
            && IsSecureRandom(invocation);

        bool IsAddSeedMaterial() =>
            invocation.TargetMethod.Name == "AddSeedMaterial"
            && IsIRandomGenerator(invocation);
    }

    // secureRandom.NextXXX()
    // randomGenerator.NextBytes()
    private ProgramState ProcessNextMethods(ProgramState state, IInvocationOperationWrapper invocation)
    {
        if ((IsSecureRandomMethod() || IsRandomGeneratorMethod())
            && invocation.Instance is { } instance
            && state[instance]?.HasConstraint(CryptographicSeedConstraint.Predictable) is true)
        {
            ReportIssue(invocation.WrappedOperation);
        }
        return null;

        bool IsSecureRandomMethod() =>
            SecureRandomNextMethods.Contains(invocation.TargetMethod.Name)
            && IsSecureRandom(invocation);

        bool IsRandomGeneratorMethod() =>
            invocation.TargetMethod.Name == "NextBytes"
            && IsIRandomGenerator(invocation);
    }

    private static bool IsSecureRandom(IInvocationOperationWrapper invocation) =>
        invocation.TargetMethod.ContainingType.Is(KnownType.Org_BouncyCastle_Security_SecureRandom);

    private static bool IsIRandomGenerator(IInvocationOperationWrapper invocation) =>
        invocation.Instance is { } instance
        && instance.Type.Is(KnownType.Org_BouncyCastle_Crypto_Prng_IRandomGenerator);
}
