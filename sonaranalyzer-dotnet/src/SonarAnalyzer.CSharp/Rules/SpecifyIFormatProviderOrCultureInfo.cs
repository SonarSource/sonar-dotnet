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
    public sealed class SpecifyIFormatProviderOrCultureInfo : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4056";
        private const string MessageFormat = "Use the overload that takes a 'CultureInfo' or 'IFormatProvider' parameter.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<KnownType> checkedTypes = new HashSet<KnownType>
        {
            KnownType.System_IFormatProvider,
            KnownType.System_Globalization_CultureInfo
        };

        private static readonly ISet<MethodSignature> allowedMethods = new HashSet<MethodSignature>
        {
            new MethodSignature(KnownType.System_Activator, "CreateInstance"),
            new MethodSignature(KnownType.Sytem_Resources_ResourceManager, "GetObject"),
            new MethodSignature(KnownType.Sytem_Resources_ResourceManager, "GetString")
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;

                    if (invocation.Expression != null &&
                        IsInvalidCall(invocation.Expression, c.SemanticModel) &&
                        HasOverloadWithStringComparison(invocation.Expression, c.SemanticModel))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule, invocation.GetLocation(), invocation.Expression));
                    }
                }, SyntaxKind.InvocationExpression);
        }

        private static bool HasOverloadWithStringComparison(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            return semanticModel.GetMemberGroup(expression)
                .OfType<IMethodSymbol>()
                .Any(HasAnyCheckedTypeParameter);
        }

        private static bool IsInvalidCall(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var methodSymbol = semanticModel.GetSymbolInfo(expression).Symbol as IMethodSymbol;

            return methodSymbol != null &&
                !allowedMethods.Any(x => methodSymbol.ContainingType.Is(x.ContainingType) &&
                                         methodSymbol.Name == x.Name) &&
                !HasAnyCheckedTypeParameter(methodSymbol);
        }

        private static bool HasAnyCheckedTypeParameter(IMethodSymbol method)
        {
            return method.GetParameters().Any(parameter => parameter.Type.IsAny(checkedTypes));
        }
    }
}
