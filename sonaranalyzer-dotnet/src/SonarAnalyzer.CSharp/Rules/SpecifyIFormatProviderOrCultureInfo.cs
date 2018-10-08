/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class SpecifyIFormatProviderOrCultureInfo : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4056";
        private const string MessageFormat = "Use the overload that takes a 'CultureInfo' or 'IFormatProvider' parameter.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ImmutableArray<KnownType> formatAndClutureTypes =
            ImmutableArray.Create(
                KnownType.System_IFormatProvider,
                KnownType.System_Globalization_CultureInfo
            );

        private static readonly ImmutableArray<KnownType> formattableTypes =
            ImmutableArray.Create(
                KnownType.System_String,
                KnownType.System_Object
            );

        private static readonly ISet<MethodSignature> whitelistMethods = new HashSet<MethodSignature>
        {
            new MethodSignature(KnownType.System_Activator, "CreateInstance"),
            new MethodSignature(KnownType.Sytem_Resources_ResourceManager, "GetObject"),
            new MethodSignature(KnownType.Sytem_Resources_ResourceManager, "GetString"),
        };

        private static readonly ISet<MethodSignature> blacklistMethods = new HashSet<MethodSignature>
        {
            new MethodSignature(KnownType.System_Char, "ToUpper"),
            new MethodSignature(KnownType.System_Char, "ToLower"),
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    if (invocation.Expression != null &&
                        c.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol is IMethodSymbol methodSymbol &&
                        !IsIgnored(methodSymbol) &&
                        CanPotentiallyRaise(methodSymbol) &&
                        HasOverloadWithFormatOrCulture(invocation, c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, invocation.GetLocation(), invocation.Expression));
                    }
                }, SyntaxKind.InvocationExpression);
        }

        private static bool IsIgnored(IMethodSymbol methodSymbol) =>
            SpecifyStringComparison.HasAnyStringComparisonParameter(methodSymbol) ||
            HasAnyFormatOrCultureParameter(methodSymbol) ||
            whitelistMethods.Any(x => Matches(x, methodSymbol));

        private bool CanPotentiallyRaise(IMethodSymbol methodSymbol) =>
            ReturnsOrAcceptsFormattableType(methodSymbol) ||
            blacklistMethods.Any(x => Matches(x, methodSymbol));

        private static bool HasOverloadWithFormatOrCulture(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            semanticModel.GetMemberGroup(invocation.Expression)
                .OfType<IMethodSymbol>()
                .Where(m => !m.GetAttributes(KnownType.System_ObsoleteAttribute).Any())
                .Any(HasAnyFormatOrCultureParameter);

        private bool ReturnsOrAcceptsFormattableType(IMethodSymbol methodSymbol) =>
            methodSymbol.ReturnType.IsAny(formattableTypes) ||
            methodSymbol.GetParameters().Any(p => p.Type.IsAny(formattableTypes));

        public static bool HasAnyFormatOrCultureParameter(ISymbol method) =>
            method.GetParameters().Any(p => p.Type.IsAny(formatAndClutureTypes));

        private static bool Matches(MethodSignature methodSignature, IMethodSymbol methodSymbol) =>
            methodSymbol != null &&
            methodSymbol.ContainingType.Is(methodSignature.ContainingType) &&
            methodSymbol.Name == methodSignature.Name;
    }
}
