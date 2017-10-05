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

using System.Collections.Immutable;
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
    public class OrderByRepeated : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3169";
        private const string MessageFormat = "Use 'ThenBy' instead.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var outerInvocation = (InvocationExpressionSyntax)c.Node;
                    if (!IsMethodOrderByExtension(outerInvocation, c.SemanticModel))
                    {
                        return;
                    }

                    var memberAccess = outerInvocation.Expression as MemberAccessExpressionSyntax;
                    if (memberAccess == null)
                    {
                        return;
                    }

                    var innerInvocation = memberAccess.Expression as InvocationExpressionSyntax;
                    if (!IsMethodOrderByExtension(innerInvocation, c.SemanticModel) &&
                        !IsMethodThenByExtension(innerInvocation, c.SemanticModel))
                    {
                        return;
                    }

                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, memberAccess.Name.GetLocation()));
                },
                SyntaxKind.InvocationExpression);
        }
        private static bool IsMethodOrderByExtension(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            if (invocation == null)
            {
                return false;
            }

            var methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

            return methodSymbol != null &&
                   methodSymbol.Name == "OrderBy" &&
                   methodSymbol.MethodKind == MethodKind.ReducedExtension &&
                   methodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T);
        }
        private static bool IsMethodThenByExtension(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            if (invocation == null)
            {
                return false;
            }

            var methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

            return methodSymbol != null &&
                   methodSymbol.Name == "ThenBy" &&
                   methodSymbol.MethodKind == MethodKind.ReducedExtension &&
                   MethodIsOnIOrderedEnumerable(methodSymbol);
        }

        private static bool MethodIsOnIOrderedEnumerable(IMethodSymbol methodSymbol)
        {
            var receiverType = methodSymbol.ReceiverType as INamedTypeSymbol;

            return receiverType != null &&
                   receiverType.ConstructedFrom.ContainingNamespace.ToString() == "System.Linq" &&
                   receiverType.ConstructedFrom.MetadataName == "IOrderedEnumerable`1";
        }
    }
}
