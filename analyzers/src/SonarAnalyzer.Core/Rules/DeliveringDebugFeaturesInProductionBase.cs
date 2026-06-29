/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

namespace SonarAnalyzer.Core.Rules;

public abstract class DeliveringDebugFeaturesInProductionBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    protected const string StartupDevelopment = "StartupDevelopment";
    private const string DiagnosticId = "S4507";

    private readonly ImmutableArray<MemberDescriptor> isDevelopmentMethods = ImmutableArray.Create(
        new MemberDescriptor(KnownType.Microsoft_AspNetCore_Hosting_HostingEnvironmentExtensions, "IsDevelopment"),
        new MemberDescriptor(KnownType.Microsoft_Extensions_Hosting_HostEnvironmentEnvExtensions, "IsDevelopment"));

    protected abstract bool IsDevelopmentCheckInvoked(SyntaxNode node, SemanticModel model);

    protected abstract bool IsInDevelopmentContext(SyntaxNode node);

    protected override string MessageFormat => "Disable this debug feature in production.";

    protected DeliveringDebugFeaturesInProductionBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        var input = new TrackerInput(context, AnalyzerConfiguration.AlwaysEnabled, Rule);
        var t = Language.Tracker.Invocation;
        t.Track(
            input,
            t.MatchMethod(
                new MemberDescriptor(KnownType.Microsoft_AspNetCore_Builder_DeveloperExceptionPageExtensions, "UseDeveloperExceptionPage"),
                new MemberDescriptor(KnownType.Microsoft_AspNetCore_Builder_DatabaseErrorPageExtensions, "UseDatabaseErrorPage")),
            t.ExceptWhen(x => IsDevelopmentCheckInvoked(x.Node, x.Model)),
            t.ExceptWhen(x => IsInDevelopmentContext(x.Node)));
    }

    protected bool IsValidationMethod(SemanticModel model, SyntaxNode condition, string methodName) =>
        new Lazy<IMethodSymbol>(() => model.GetSymbolInfo(condition).Symbol as IMethodSymbol) is var lazySymbol
        && isDevelopmentMethods.Any(x => x.IsMatch(methodName, lazySymbol, Language.NameComparison));
}
