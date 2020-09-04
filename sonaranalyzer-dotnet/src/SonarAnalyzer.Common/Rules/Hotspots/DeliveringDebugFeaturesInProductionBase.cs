/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class DeliveringDebugFeaturesInProductionBase<TSyntaxKind> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S4507";
        protected const string MessageFormat = "Make sure this debug feature is deactivated before delivering the code in production.";

        private static readonly ImmutableArray<MemberDescriptor> isDevelopmentMethods = ImmutableArray.Create(
            new MemberDescriptor(KnownType.Microsoft_AspNetCore_Hosting_HostingEnvironmentExtensions, "IsDevelopment"),
            new MemberDescriptor(KnownType.Microsoft_Extensions_Hosting_HostEnvironmentEnvExtensions, "IsDevelopment")
            );

        protected InvocationTracker<TSyntaxKind> InvocationTracker { get; set; }

        protected override void Initialize(SonarAnalysisContext context)
        {
            InvocationTracker.Track(context,
                InvocationTracker.MatchMethod(
                    new MemberDescriptor(KnownType.Microsoft_AspNetCore_Builder_DeveloperExceptionPageExtensions, "UseDeveloperExceptionPage"),
                    new MemberDescriptor(KnownType.Microsoft_AspNetCore_Builder_DatabaseErrorPageExtensions, "UseDatabaseErrorPage")),
                Conditions.ExceptWhen(IsInvokedConditionally()));
        }

        protected abstract InvocationCondition IsInvokedConditionally();

        protected static bool IsValidationMethod(SemanticModel semanticModel, SyntaxNode condition, string methodName, bool caseInsensitiveComparison = false)
        {
            var methodSymbol = new Lazy<IMethodSymbol>(() => semanticModel.GetSymbolInfo(condition).Symbol as IMethodSymbol);
            return isDevelopmentMethods.Any(x => x.IsMatch(methodName, methodSymbol, caseInsensitiveComparison));
        }
    }
}
