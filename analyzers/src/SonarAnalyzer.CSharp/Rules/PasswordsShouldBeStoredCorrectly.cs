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

using SonarAnalyzer.Helpers.Trackers;

namespace SonarAnalyzer.Rules.CSharp;

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
        NetCore(context);
        Rfc2898DeriveBytes(context);
        BouncyCastle(context);
    }

    private static void NetCore(SonarAnalysisContext context)
    {
        var propertyTracker = CSharpFacade.Instance.Tracker.PropertyAccess;
        Track(
            propertyTracker,
            context,
            UseMoreIterationsMessageFormat,
            propertyTracker.MatchSetter(),
            propertyTracker.MatchProperty(new MemberDescriptor(KnownType.Microsoft_AspNetCore_Identity_PasswordsHasherOptions, "IterationCount")),
            x => HasFewIterations(x, propertyTracker));
        Track(
            propertyTracker,
            context,
            "Identity v2 uses only 1000 iterations. Consider changing to identity V3.",
            propertyTracker.MatchSetter(),
            propertyTracker.MatchProperty(new MemberDescriptor(KnownType.Microsoft_AspNetCore_Identity_PasswordsHasherOptions, "CompatibilityMode")),
            x => propertyTracker.AssignedValue(x) is int mode && mode == 0); // PasswordHasherCompatibilityMode.IdentityV2 results to 0

        var argumentTracker = CSharpFacade.Instance.Tracker.Argument;
        Track(
            argumentTracker,
            context,
            UseMoreIterationsMessageFormat,
            argumentTracker.MatchArgument(ArgumentDescriptor.MethodInvocation(KnownType.Microsoft_AspNetCore_Cryptography_KeyDerivation_KeyDerivation, "Pbkdf2", "iterationCount", 3)),
            x => ArgumentLessThan(x, IterationCountThreshold));
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
    }

    private static void BouncyCastle(SonarAnalysisContext context)
    {
        var tracker = CSharpFacade.Instance.Tracker.Argument;

        Track(
            tracker,
            context,
            "Use a cost factor of at least 12 here.",
            tracker.Or(
                tracker.MatchArgument(ArgumentDescriptor.MethodInvocation(KnownType.Org_BouncyCastle_Crypto_Generators_OpenBsdBCrypt, "Generate", "cost", x => x is 2 or 3)),
                tracker.MatchArgument(ArgumentDescriptor.MethodInvocation(KnownType.Org_BouncyCastle_Crypto_Generators_BCrypt, "Generate", "cost", 2))),
            x => ArgumentLessThan(x, 12));

        Track(
            tracker,
            context,
            UseMoreIterationsMessageFormat,
            tracker.MatchArgument(ArgumentDescriptor.MethodInvocation(KnownType.Org_BouncyCastle_Crypto_PbeParametersGenerator, "Init", "iterationCount", 2)),
            x => ArgumentLessThan(x, IterationCountThreshold));
    }

    private static bool HasFewIterations(PropertyAccessContext context, PropertyAccessTracker<SyntaxKind> tracker) =>
        tracker.AssignedValue(context) is int iterationCount
        && iterationCount < IterationCountThreshold;

    private static bool ArgumentLessThan(ArgumentContext context, int threshold) =>
        context.SemanticModel.GetConstantValue(((ArgumentSyntax)context.Node).Expression) is { HasValue: true, Value: int value }
        && value < threshold;

    private static void Track<TContext>(SyntaxTrackerBase<SyntaxKind, TContext> tracker,
                                        SonarAnalysisContext context,
                                        string message,
                                        params SyntaxTrackerBase<SyntaxKind, TContext>.Condition[] conditions) where TContext : SyntaxBaseContext =>
        tracker.Track(new(context, AnalyzerConfiguration.AlwaysEnabled, Rule), [message], conditions);
}
