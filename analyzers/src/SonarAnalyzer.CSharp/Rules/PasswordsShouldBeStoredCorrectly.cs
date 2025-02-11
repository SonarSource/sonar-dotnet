/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PasswordsShouldBeStoredCorrectly : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S5344";
    private const string MessageFormat = "{0}";
    private const string UseMoreIterationsMessageFormat = "Use at least 100,000 iterations here.";
    private const int IterationCountThreshold = 100_000;

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        AspNetCore(context);
        AspNetFramework(context);
        Rfc2898DeriveBytes(context);
        BouncyCastle(context);
    }

    private static void AspNetCore(SonarAnalysisContext context)
    {
        var propertyTracker = CSharpFacade.Instance.Tracker.PropertyAccess;
        Track(
            propertyTracker,
            context,
            UseMoreIterationsMessageFormat,
            propertyTracker.MatchSetter(),
            propertyTracker.MatchProperty(new MemberDescriptor(KnownType.Microsoft_AspNetCore_Identity_PasswordHasherOptions, "IterationCount")),
            x => HasFewIterations(x, propertyTracker));
        Track(
            propertyTracker,
            context,
            "Identity v2 uses only 1000 iterations. Consider changing to identity V3.",
            propertyTracker.MatchSetter(),
            propertyTracker.MatchProperty(new MemberDescriptor(KnownType.Microsoft_AspNetCore_Identity_PasswordHasherOptions, "CompatibilityMode")),
            x => propertyTracker.AssignedValue(x) is 0); // PasswordHasherCompatibilityMode.IdentityV2 results to 0

        var argumentTracker = CSharpFacade.Instance.Tracker.Argument;
        Track(
            argumentTracker,
            context,
            UseMoreIterationsMessageFormat,
            argumentTracker.MatchArgument(ArgumentDescriptor.MethodInvocation(KnownType.Microsoft_AspNetCore_Cryptography_KeyDerivation_KeyDerivation, "Pbkdf2", "iterationCount", 3)),
            x => ArgumentLessThan(x, IterationCountThreshold));
    }

    private static void AspNetFramework(SonarAnalysisContext context)
    {
        var tracker = CSharpFacade.Instance.Tracker.ObjectCreation;
        Track(
            tracker,
            context,
            "PasswordHasher does not support state-of-the-art parameters. Use Rfc2898DeriveBytes instead.",
            tracker.WhenDerivesFrom(KnownType.Microsoft_AspNet_Identity_PasswordHasherOptions));
    }

    private static void Rfc2898DeriveBytes(SonarAnalysisContext context)
    {
        // Raise when hashAlgorithm is present
        var argumentTracker = CSharpFacade.Instance.Tracker.Argument;
        // Exclude the constructors that have a hashAlgorithm parameter
        var constructorArgument = ArgumentDescriptor.ConstructorInvocation(
                        ctor => ctor.ContainingType.Is(KnownType.System_Security_Cryptography_Rfc2898DeriveBytes) && ctor.Parameters.Any(x => x.Name == "hashAlgorithm"),
                        (methodName, comparison) => string.Compare(methodName, "Rfc2898DeriveBytes", comparison) == 0,
                        null,
                        x => x.Name == "iterations",
                        null,
                        null);
        var invocationArgument = ArgumentDescriptor.MethodInvocation(KnownType.System_Security_Cryptography_Rfc2898DeriveBytes, "Pbkdf2", "iterations", x => x is 2 or 3);
        Track(
            argumentTracker,
            context,
            UseMoreIterationsMessageFormat,
            argumentTracker.Or(
                argumentTracker.MatchArgument(constructorArgument),
                argumentTracker.MatchArgument(invocationArgument)),
            x => ArgumentLessThan(x, IterationCountThreshold));

        // Raise when hashAlgorithm is NOT present
        var objectCreationTracker = CSharpFacade.Instance.Tracker.ObjectCreation;
        Track(
            objectCreationTracker,
            context,
            "Use at least 100,000 iterations and a state-of-the-art digest algorithm here.",
            objectCreationTracker.MatchConstructor(KnownType.System_Security_Cryptography_Rfc2898DeriveBytes),
            x => x.InvokedConstructorSymbol.Value.Parameters.All(x => x.Name != "hashAlgorithm"));

        var propertyTracker = CSharpFacade.Instance.Tracker.PropertyAccess;
        Track(
            propertyTracker,
            context,
            UseMoreIterationsMessageFormat,
            propertyTracker.MatchSetter(),
            propertyTracker.MatchProperty(new MemberDescriptor(KnownType.System_Security_Cryptography_Rfc2898DeriveBytes, "IterationCount")),
            x => HasFewIterations(x, propertyTracker));
    }

    private static void BouncyCastle(SonarAnalysisContext context)
    {
        var tracker = CSharpFacade.Instance.Tracker.Argument;

        Track(
            tracker,
            context,
            "Use a cost factor of at least 12 here.",
            tracker.Or(
                tracker.MatchArgument(
                    ArgumentDescriptor.MethodInvocation(KnownType.Org_BouncyCastle_Crypto_Generators_OpenBsdBCrypt, "Generate", "cost", x => x is 2 or 3)),
                tracker.MatchArgument(
                    ArgumentDescriptor.MethodInvocation(KnownType.Org_BouncyCastle_Crypto_Generators_BCrypt, "Generate", "cost", 2))),
            x => ArgumentLessThan(x, 12));

        Track(
            tracker,
            context,
            UseMoreIterationsMessageFormat,
            tracker.MatchArgument(
                ArgumentDescriptor.MethodInvocation(KnownType.Org_BouncyCastle_Crypto_PbeParametersGenerator, "Init", "iterationCount", 2)),
            x => ArgumentLessThan(x, IterationCountThreshold));

        TrackSCrypt("N", 2, "Use a cost factor of at least 2 ^ 12 for N here.", 1 << 12);
        TrackSCrypt("r", 3, "Use a memory factor of at least 8 for r here.", 8);
        TrackSCrypt("dkLen", 5, "Use an output length of at least 32 for dkLen here.", 32);

        void TrackSCrypt(string argumentName, int argumentPosition, string diagnosticMessage, int threshold) =>
            Track(
                tracker,
                context,
                diagnosticMessage,
                tracker.MatchArgument(ArgumentDescriptor.MethodInvocation(
                    KnownType.Org_BouncyCastle_Crypto_Generators_SCrypt, "Generate", argumentName, argumentPosition)),
                x => ArgumentLessThan(x, threshold));
    }

    private static bool HasFewIterations(PropertyAccessContext context, PropertyAccessTracker<SyntaxKind> tracker) =>
        tracker.AssignedValue(context) is < IterationCountThreshold;

    private static bool ArgumentLessThan(ArgumentContext context, int threshold) =>
        context.Model.GetConstantValue(((ArgumentSyntax)context.Node).Expression) is { HasValue: true, Value: int value }
        && value < threshold;

    private static void Track<TContext>(SyntaxTrackerBase<SyntaxKind, TContext> tracker,
                                        SonarAnalysisContext context,
                                        string message,
                                        params SyntaxTrackerBase<SyntaxKind, TContext>.Condition[] conditions) where TContext : SyntaxBaseContext =>
        tracker.Track(new(context, AnalyzerConfiguration.AlwaysEnabled, Rule), [message], conditions);
}
