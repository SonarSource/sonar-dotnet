/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

        private static readonly ImmutableArray<KnownType> formatAndCultureType =
            ImmutableArray.Create(
                KnownType.System_IFormatProvider,
                KnownType.System_Globalization_CultureInfo
            );

        private static readonly ImmutableArray<KnownType> formattableTypes =
            ImmutableArray.Create(
                KnownType.System_String,
                KnownType.System_Object
            );

        private static readonly ISet<MemberDescriptor> whitelistMethods = new HashSet<MemberDescriptor>
        {
            new MemberDescriptor(KnownType.System_Activator, "CreateInstance"),
            new MemberDescriptor(KnownType.Sytem_Resources_ResourceManager, "GetObject"),
            new MemberDescriptor(KnownType.Sytem_Resources_ResourceManager, "GetString"),
        };

        private static readonly ISet<MemberDescriptor> blacklistMethods = new HashSet<MemberDescriptor>
        {
            new MemberDescriptor(KnownType.System_Char, "ToUpper"),
            new MemberDescriptor(KnownType.System_Char, "ToLower"),
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

        private static bool HasOverloadWithFormatOrCulture(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            return semanticModel.GetMemberGroup(invocation.Expression)
                .OfType<IMethodSymbol>()
                .Where(m => !m.GetAttributes(KnownType.System_ObsoleteAttribute).Any())
                .Where(IsCompatibleOverload)
                .Any(m => SameParametersExceptFormatOrCulture(m, GetParameters()));

            IEnumerable<IParameterSymbol> GetParameters() =>
                semanticModel.GetSymbolInfo(invocation.Expression).Symbol?.GetParameters();

            // must have same number of arguments + 1 (the format or culture argument) OR is params argument
            bool IsCompatibleOverload(IMethodSymbol m) =>
                (m.GetParameters().Count() - invocation.ArgumentList.Arguments.Count == 1) ||
                m.GetParameters().Last().IsParams;
        }

        private static bool SameParametersExceptFormatOrCulture(IMethodSymbol possibleOverload, IEnumerable<IParameterSymbol> parameters)
        {
            var overloadParametersWithoutFormatCulture = possibleOverload.GetParameters().Where(p => !p.Type.IsAny(formatAndCultureType));
            // once we filter out the (format or culture) argument, the number of parameters must be the same
            if (parameters.Count() != overloadParametersWithoutFormatCulture.Count())
            {
                return false;
            }

            var possibleOverloadParameters = overloadParametersWithoutFormatCulture.ToList();
            var invocationParameters = parameters.ToList();
            for (var i = 0; i < invocationParameters.Count; i++)
            {
                // invocation parameter can be sub-type of overload parameter
                if (!invocationParameters[i].Type.DerivesOrImplements(possibleOverloadParameters[i].Type))
                {
                    return false;
                }
            }
            return true;
        }

        private bool ReturnsOrAcceptsFormattableType(IMethodSymbol methodSymbol) =>
            methodSymbol.ReturnType.IsAny(formattableTypes) ||
            methodSymbol.GetParameters().Any(p => p.Type.IsAny(formattableTypes));

        public static bool HasAnyFormatOrCultureParameter(ISymbol method) =>
            method.GetParameters().Any(p => p.Type.IsAny(formatAndCultureType));

        private static bool Matches(MemberDescriptor memberDescriptor, IMethodSymbol methodSymbol) =>
            methodSymbol != null &&
            methodSymbol.ContainingType.Is(memberDescriptor.ContainingType) &&
            methodSymbol.Name == memberDescriptor.Name;
    }
}
