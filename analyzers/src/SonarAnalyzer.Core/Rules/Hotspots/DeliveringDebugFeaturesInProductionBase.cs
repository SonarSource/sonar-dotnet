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

namespace SonarAnalyzer.Rules
{
    public abstract class DeliveringDebugFeaturesInProductionBase<TSyntaxKind> : TrackerHotspotDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        private const string DiagnosticId = "S4507";
        protected const string StartupDevelopment = "StartupDevelopment";
        private const string MessageFormat = "Make sure this debug feature is deactivated before delivering the code in production.";

        private readonly ImmutableArray<MemberDescriptor> isDevelopmentMethods = ImmutableArray.Create(
            new MemberDescriptor(KnownType.Microsoft_AspNetCore_Hosting_HostingEnvironmentExtensions, "IsDevelopment"),
            new MemberDescriptor(KnownType.Microsoft_Extensions_Hosting_HostEnvironmentEnvExtensions, "IsDevelopment"));

        protected abstract bool IsDevelopmentCheckInvoked(SyntaxNode node, SemanticModel semanticModel);

        protected abstract bool IsInDevelopmentContext(SyntaxNode node);

        protected DeliveringDebugFeaturesInProductionBase(IAnalyzerConfiguration configuration)
            : base(configuration, DiagnosticId, MessageFormat) { }

        protected override void Initialize(TrackerInput input)
        {
            var t = Language.Tracker.Invocation;
            t.Track(input,
                    t.MatchMethod(new MemberDescriptor(KnownType.Microsoft_AspNetCore_Builder_DeveloperExceptionPageExtensions, "UseDeveloperExceptionPage"),
                                  new MemberDescriptor(KnownType.Microsoft_AspNetCore_Builder_DatabaseErrorPageExtensions, "UseDatabaseErrorPage")),
                    t.ExceptWhen(c => IsDevelopmentCheckInvoked(c.Node, c.Model)),
                    t.ExceptWhen(c => IsInDevelopmentContext(c.Node)));
        }

        protected bool IsValidationMethod(SemanticModel semanticModel, SyntaxNode condition, string methodName) =>
            new Lazy<IMethodSymbol>(() => semanticModel.GetSymbolInfo(condition).Symbol as IMethodSymbol) is var lazySymbol
            && isDevelopmentMethods.Any(x => x.IsMatch(methodName, lazySymbol, Language.NameComparison));
    }
}
