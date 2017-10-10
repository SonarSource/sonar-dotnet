/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
    public class StringOperationWithoutCulture : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1449";
        private const string MessageFormat = "{0}";
        internal const string MessageDefineLocale = "Define the locale to be used in this string operation.";
        internal const string MessageChangeCompareTo = "Use 'CompareOrdinal' or 'Compare' with the locale specified instead of 'CompareTo'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;

                    var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                    if(memberAccess == null)
                    {
                        return;
                    }

                    var calledMethod = c.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                    if (calledMethod == null)
                    {
                        return;
                    }

                    if (calledMethod.IsInType(KnownType.System_String) &&
                        CommonCultureSpecificMethodNames.Contains(calledMethod.Name) &&
                        !calledMethod.Parameters.Any(param => param.Type.IsAny(StringCultureSpecifierNames)))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, memberAccess.Name.GetLocation(), MessageDefineLocale));
                        return;
                    }

                    if (calledMethod.IsInType(KnownType.System_String) &&
                        IndexLookupMethodNames.Contains(calledMethod.Name) &&
                        calledMethod.Parameters.Any(param => param.Type.SpecialType == SpecialType.System_String) &&
                        !calledMethod.Parameters.Any(param => param.Type.IsAny(StringCultureSpecifierNames)))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, memberAccess.Name.GetLocation(), MessageDefineLocale));
                        return;
                    }

                    if (IsMethodOnNonIntegralOrDateTime(calledMethod) &&
                        calledMethod.Name == ToStringMethodName &&
                        calledMethod.Parameters.Length == 0)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, memberAccess.Name.GetLocation(), MessageDefineLocale));
                        return;
                    }

                    if (calledMethod.IsInType(KnownType.System_String) &&
                        calledMethod.Name == CompareToMethodName)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, memberAccess.Name.GetLocation(), MessageChangeCompareTo));
                    }
                },
                SyntaxKind.InvocationExpression);
        }

        private static bool IsMethodOnNonIntegralOrDateTime(IMethodSymbol methodSymbol)
        {
            return methodSymbol.IsInType(KnownType.NonIntegralNumbers) ||
                methodSymbol.IsInType(KnownType.System_DateTime);
        }

        private static readonly ISet<string> CommonCultureSpecificMethodNames = ImmutableHashSet.Create(
            "ToLower",
            "ToUpper",
            "Compare");

        private static readonly ISet<string> IndexLookupMethodNames = ImmutableHashSet.Create(
            "IndexOf",
            "LastIndexOf");

        private const string CompareToMethodName = "CompareTo";
        private const string ToStringMethodName = "ToString";

        private static readonly ISet<KnownType> StringCultureSpecifierNames = ImmutableHashSet.Create(
            KnownType.System_Globalization_CultureInfo,
            KnownType.System_Globalization_CompareOptions,
            KnownType.System_StringComparison);
    }
}
