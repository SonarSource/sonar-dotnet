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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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
                .Any(methodSignature => SameParametersExceptFormatOrCulture(methodSignature, GetInvocationParameters()));

            IEnumerable<IParameterSymbol> GetInvocationParameters() =>
                semanticModel.GetSymbolInfo(invocation.Expression).Symbol?.GetParameters();

            // must have same number of arguments + 1 (the format or culture argument) OR is params argument
            bool IsCompatibleOverload(IMethodSymbol m) =>
                (m.GetParameters().Count() - invocation.ArgumentList.Arguments.Count == 1) ||
                (m.GetParameters().Any() && m.GetParameters().Last().IsParams);
        }

        private static bool SameParametersExceptFormatOrCulture(IMethodSymbol possibleOverload, IEnumerable<IParameterSymbol> invocationParameters)
        {
            var parametersWithoutFormatCulture = possibleOverload.GetParameters().Where(p => !p.Type.IsAny(formatAndCultureType));
            // no FormatOrCulture argument found
            if (parametersWithoutFormatCulture.Count() == possibleOverload.GetParameters().Count())
            {
                return false;
            }

            // once we filter out the FormatOrCulture argument, we have two possibilities:
            // - possibleOverload has a 'params' argument which matches the invocationParameters
            // - the number of parameters is the same and of same type

            var invocationParametersNumber = invocationParameters.Count();
            var parametersWithoutFormatCultureNumber = parametersWithoutFormatCulture.Count();

            if (parametersWithoutFormatCultureNumber <= invocationParametersNumber &&
                parametersWithoutFormatCultureNumber > 0 &&
                parametersWithoutFormatCulture.Last() is IParameterSymbol lastParameter &&
                lastParameter.IsParams)
            {
                return VerifyCompatibility(invocationParameters.ToList(), parametersWithoutFormatCulture.ToList(), lastParameter);
            }
            else if (invocationParametersNumber == parametersWithoutFormatCultureNumber)
            {
                return VerifyCompatibility(invocationParameters.ToList(), parametersWithoutFormatCulture.ToList());
            }
            return false;
        }

        /**
         * Verifies the compatibility between the invocation parameters and the parameters of a possible overload that
         * has the last parameter of 'params' type (variable length parameter).
         */ 
        private static bool VerifyCompatibility(IList<IParameterSymbol> invocationParameters,
            IList<IParameterSymbol> overloadCandidateParameters,
            IParameterSymbol paramsParameter)
        {
            var i = 0;
            // check parameters before the last parameter
            for (; i < overloadCandidateParameters.Count - 1; i++)
            {
                if (!invocationParameters[i].Type.DerivesOrImplements(overloadCandidateParameters[i].Type))
                {
                    return false;
                }
            }
            // make sure the rest of the invocation parameters match with the 'params' type
            var paramsType = GetParamsType(paramsParameter.Type);
            for (; i < invocationParameters.Count; i++)
            {
                if (!invocationParameters[i].Type.DerivesOrImplements(paramsType))
                {
                    return false;
                }
            }
            return true;

            ITypeSymbol GetParamsType(ITypeSymbol typeSymbol) => (typeSymbol as IArrayTypeSymbol)?.ElementType;
        }

        /**
         * Checks that the invocation parameters are of the same type (or a subtype) with the overload parameters
         * Assumption: the two lists have the same number of elements.
         */
        private static bool VerifyCompatibility(IList<IParameterSymbol> invocationParameters, IList<IParameterSymbol> overloadCandidateParameters) =>
             invocationParameters
                .Select((p, index) => p.Type.DerivesOrImplements(overloadCandidateParameters[index].Type))
                .All(isCompatible => isCompatible);

        private static bool ReturnsOrAcceptsFormattableType(IMethodSymbol methodSymbol) =>
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
