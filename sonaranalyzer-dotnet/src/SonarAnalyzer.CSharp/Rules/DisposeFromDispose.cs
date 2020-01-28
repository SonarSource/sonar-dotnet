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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DisposeFromDispose : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2952";
        private const string MessageFormat = "Move this 'Dispose' call into this class' own 'Dispose' method.";

        private const string DisposeMethodName = "Dispose";
        private const string DisposeMethodExplicitName = "System.IDisposable.Dispose";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    var languageVersion = c.Compilation.GetLanguageVersion();
                    if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                        c.SemanticModel.GetSymbolInfo(memberAccess.Expression).Symbol is IFieldSymbol invocationTarget &&
                        invocationTarget.IsNonStaticNonPublicDisposableField(languageVersion) &&
                        IsDisposeMethodCalled(invocation, c.SemanticModel, languageVersion) &&
                        IsDisposableClassOrStruct(invocationTarget.ContainingType, languageVersion) &&
                        !IsCalledInsideDispose(invocation, c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, GetLocation(invocation)));
                    }
                },
                SyntaxKind.InvocationExpression);
        }

        /// <summary>
        /// Classes and structs are disposable if they implement the IDisposable interface.
        /// Starting C# 8, "ref structs" (which cannot implement an interface) can also be disposable.
        /// </summary>
        private static bool IsDisposableClassOrStruct(INamedTypeSymbol type, LanguageVersion languageVersion) =>
            ImplementsDisposable(type) ||
            type.IsDisposableRefStruct(languageVersion);

        private static bool IsCalledInsideDispose(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            semanticModel.GetEnclosingSymbol(invocation.SpanStart) is IMethodSymbol enclosingMethodSymbol &&
            IsMethodMatchingDisposeMethodName(enclosingMethodSymbol);

        private Location GetLocation(InvocationExpressionSyntax invocation) =>
            // We already did the type check before
            ((MemberAccessExpressionSyntax)invocation.Expression).Name.GetLocation();

        /// <summary>
        /// Note: disposable ref structs do not implement the IDisposable interface
        /// </summary>
        private static bool IsDisposeMethodCalled(InvocationExpressionSyntax invocation, SemanticModel semanticModel, LanguageVersion languageVersion)
        {
            if (!(semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol))
            {
                return false;
            }

            var disposeMethod = semanticModel.Compilation.GetTypeMethod(SpecialType.System_IDisposable, "Dispose");
            return disposeMethod != null &&
            (
                methodSymbol.Equals(methodSymbol.ContainingType.FindImplementationForInterfaceMember(disposeMethod)) ||
                methodSymbol.ContainingType.IsDisposableRefStruct(languageVersion)
            );
        }

        private static bool IsMethodMatchingDisposeMethodName(IMethodSymbol enclosingMethodSymbol) =>
            enclosingMethodSymbol.Name == DisposeMethodName ||
            enclosingMethodSymbol.ExplicitInterfaceImplementations.Any() && enclosingMethodSymbol.Name == DisposeMethodExplicitName;

        private static bool ImplementsDisposable(INamedTypeSymbol containingType) =>
            containingType.Implements(KnownType.System_IDisposable);
    }
}
