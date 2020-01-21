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
                    if (IsDisposableFieldInDisposableClassOrStruct(invocation, c.SemanticModel, c.Compilation) &&
                        !IsCalledInsideDispose(invocation, c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, GetLocation(invocation)));
                    }
                },
                SyntaxKind.InvocationExpression);
        }

        /// <summary>
        /// Returns true if:
        /// - the invocation is done on a non-static, non-public disposable field
        /// AND
        /// - the containing type is either a class implementing IDisposable, or a disposable ref struct (C# 8 feature)
        /// </summary>
        private static bool IsDisposableFieldInDisposableClassOrStruct(InvocationExpressionSyntax invocation, SemanticModel semanticModel, Compilation compilation) =>
            invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
            semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol is IFieldSymbol fieldSymbol &&
            DisposableMemberInNonDisposableClass.IsNonStaticNonPublicDisposableField(fieldSymbol) &&
            IsDisposeMethodCalled(invocation, semanticModel) &&
            (
                ImplementsDisposable(fieldSymbol.ContainingType) ||
                IsDisposableRefStruct(compilation, fieldSymbol.ContainingType)
            );

        private static bool IsCalledInsideDispose(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            semanticModel.GetEnclosingSymbol(invocation.SpanStart) is IMethodSymbol enclosingMethodSymbol &&
            IsMethodMatchingDisposeMethodName(enclosingMethodSymbol);

        private Location GetLocation(InvocationExpressionSyntax invocation) =>
            // We already did the type check before
            ((MemberAccessExpressionSyntax)invocation.Expression).Name.GetLocation();

        private static bool IsDisposeMethodCalled(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            if (!(semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol))
            {
                return false;
            }

            var disposeMethod = DisposeNotImplementingDispose.GetDisposeMethod(semanticModel.Compilation);
            return disposeMethod != null &&
                methodSymbol.Equals(methodSymbol.ContainingType.FindImplementationForInterfaceMember(disposeMethod));
        }

        private static bool IsMethodMatchingDisposeMethodName(IMethodSymbol enclosingMethodSymbol) =>
            enclosingMethodSymbol.Name == DisposeMethodName ||
            enclosingMethodSymbol.ExplicitInterfaceImplementations.Any() && enclosingMethodSymbol.Name == DisposeMethodExplicitName;

        private static bool ImplementsDisposable(INamedTypeSymbol containingType) =>
            containingType.Implements(KnownType.System_IDisposable);

        // The disposable ref struct feature has been introduced in C# 8
        private static bool IsDisposableRefStruct(Compilation compilation, INamedTypeSymbol containingType) =>
            compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp8) &&
            IsRefStruct(containingType) &&
            containingType.GetMembers("Dispose").Any(
                s => s is IMethodSymbol disposeMethod &&
                disposeMethod.Arity == 0 &&
                disposeMethod.DeclaredAccessibility == Accessibility.Public);

        private static bool IsRefStruct(INamedTypeSymbol symbol) =>
            symbol.IsStruct() &&
            symbol.DeclaringSyntaxReferences.Length == 1 &&
            symbol.DeclaringSyntaxReferences[0].GetSyntax() is StructDeclarationSyntax structDeclaration &&
            structDeclaration.Modifiers.Any(SyntaxKind.RefKeyword);
    }
}
