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
                    if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess) ||
                        !IsDisposableField(memberAccess.Expression, c.SemanticModel, out var fieldSymbol) ||
                        !IsDisposeMethodCalled(invocation, c.SemanticModel))
                    {
                        return;
                    }

                    var containingType = fieldSymbol.ContainingType;
                    if (!ImplementsDisposable(containingType) &&
                        !IsDisposableRefStruct(c.Compilation, containingType))
                    {
                        return;
                    }

                    var enclosingSymbol = c.SemanticModel.GetEnclosingSymbol(invocation.SpanStart);
                    if (enclosingSymbol == null)
                    {
                        return;
                    }

                    if (!(enclosingSymbol is IMethodSymbol enclosingMethodSymbol) ||
                        !IsMethodMatchingDisposeMethodName(enclosingMethodSymbol))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, memberAccess.Name.GetLocation()));
                    }
                },
                SyntaxKind.InvocationExpression);
        }

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

        private static bool IsDisposableField(ExpressionSyntax expression, SemanticModel semanticModel, out IFieldSymbol fieldSymbol)
        {
            var symbol = semanticModel.GetSymbolInfo(expression).Symbol;
            if (symbol is IFieldSymbol fs)
            {
                fieldSymbol = fs;
                return DisposableMemberInNonDisposableClass.IsNonStaticNonPublicDisposableField(fieldSymbol);
            }
            fieldSymbol = null;
            return false;
        }

        private static bool IsMethodMatchingDisposeMethodName(IMethodSymbol enclosingMethodSymbol) =>
            enclosingMethodSymbol.Name == DisposeMethodName ||
            enclosingMethodSymbol.ExplicitInterfaceImplementations.Any() && enclosingMethodSymbol.Name == DisposeMethodExplicitName;

        private static bool ImplementsDisposable(INamedTypeSymbol containingType) =>
            containingType.Implements(KnownType.System_IDisposable);

        private static bool IsDisposableRefStruct(Compilation compilation, INamedTypeSymbol containingType) =>
            compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp8) &&
            containingType.GetMembers("Dispose").Any(
                s => s is IMethodSymbol disposeMethod &&
                disposeMethod.Arity == 0 &&
                disposeMethod.DeclaredAccessibility == Accessibility.Public);
    }
}
