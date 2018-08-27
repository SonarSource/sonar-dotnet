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
    public sealed class DisposeNotImplementingDispose : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2953";
        private const string MessageFormat = "Either implement 'IDisposable.Dispose', or totally rename this method to prevent confusion.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private const string DisposeMethodName = "Dispose";

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSymbolAction(
                c =>
                {
                    var declaredSymbol = (INamedTypeSymbol)c.Symbol;

                    if (declaredSymbol.DeclaringSyntaxReferences.Length > 1)
                    {
                        // Partial classes are not processed.
                        // See https://github.com/dotnet/roslyn/issues/3748
                        return;
                    }

                    var disposeMethod = GetDisposeMethod(c.Compilation);
                    if (disposeMethod == null)
                    {
                        return;
                    }

                    var mightImplementDispose = new HashSet<IMethodSymbol>();
                    var namedDispose = new HashSet<IMethodSymbol>();

                    var methods = declaredSymbol.GetMembers(DisposeMethodName).OfType<IMethodSymbol>();
                    foreach (var method in methods)
                    {
                        CollectMethodsNamedAndImplementingDispose(method, disposeMethod, namedDispose, mightImplementDispose);
                    }

                    var disposeMethodsCalledFromDispose = new HashSet<IMethodSymbol>();
                    CollectInvocationsFromDisposeImplementation(disposeMethod, c.Compilation, mightImplementDispose, disposeMethodsCalledFromDispose);

                    ReportDisposeMethods(
                        namedDispose.Except(mightImplementDispose).Where(m => !disposeMethodsCalledFromDispose.Contains(m)),
                        c);
                },
                SymbolKind.NamedType);
        }

        private static void CollectInvocationsFromDisposeImplementation(IMethodSymbol disposeMethod, Compilation compilation,
            HashSet<IMethodSymbol> mightImplementDispose,
            HashSet<IMethodSymbol> disposeMethodsCalledFromDispose)
        {
            foreach (var method in mightImplementDispose
                .Where(method => MethodIsDisposeImplementation(method, disposeMethod)))
            {
                var methodDeclarations = method.DeclaringSyntaxReferences
                    .Select(r => new SyntaxNodeSemanticModelTuple<MethodDeclarationSyntax>
                    {
                        SyntaxNode = r.GetSyntax() as MethodDeclarationSyntax,
                        SemanticModel = compilation.GetSemanticModel(r.SyntaxTree)
                    })
                    .Where(m => m.SyntaxNode != null);

                var methodDeclaration = methodDeclarations
                    .FirstOrDefault(m => m.SyntaxNode.HasBodyOrExpressionBody());

                if (methodDeclaration == null)
                {
                    continue;
                }

                var invocations = methodDeclaration.SyntaxNode.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>();

                foreach (var invocation in invocations)
                {
                    CollectDisposeMethodsCalledFromDispose(invocation, methodDeclaration.SemanticModel,
                        disposeMethodsCalledFromDispose);
                }
            }
        }

        private static void CollectDisposeMethodsCalledFromDispose(InvocationExpressionSyntax invocationExpression,
            SemanticModel semanticModel,
            HashSet<IMethodSymbol> disposeMethodsCalledFromDispose)
        {
            if (!invocationExpression.IsOnThis())
            {
                return;
            }

            if (!(semanticModel.GetSymbolInfo(invocationExpression).Symbol is IMethodSymbol invokedMethod) ||
                invokedMethod.Name != DisposeMethodName)
            {
                return;
            }

            disposeMethodsCalledFromDispose.Add(invokedMethod);
        }

        private static void ReportDisposeMethods(IEnumerable<IMethodSymbol> disposeMethods,
            SymbolAnalysisContext context)
        {
            foreach (var location in disposeMethods.SelectMany(m => m.Locations))
            {
                context.ReportDiagnosticIfNonGenerated(Diagnostic.Create(
                    rule, location));
            }
        }

        private static void CollectMethodsNamedAndImplementingDispose(IMethodSymbol methodSymbol, IMethodSymbol disposeMethod,
            HashSet<IMethodSymbol> namedDispose, HashSet<IMethodSymbol> mightImplementDispose)
        {
            if (methodSymbol.Name != DisposeMethodName)
            {
                return;
            }

            namedDispose.Add(methodSymbol);

            if (methodSymbol.IsOverride ||
                MethodIsDisposeImplementation(methodSymbol, disposeMethod) ||
                MethodMightImplementDispose(methodSymbol))
            {
                mightImplementDispose.Add(methodSymbol);
            }
        }

        private static bool MethodIsDisposeImplementation(IMethodSymbol methodSymbol, IMethodSymbol disposeMethod)
        {
            return methodSymbol.Equals(methodSymbol.ContainingType.FindImplementationForInterfaceMember(disposeMethod));
        }

        private static bool MethodMightImplementDispose(IMethodSymbol declaredMethodSymbol)
        {
            var containingType = declaredMethodSymbol.ContainingType;

            if (containingType.BaseType != null && containingType.BaseType.Kind == SymbolKind.ErrorType)
            {
                return true;
            }

            var interfaces = containingType.AllInterfaces;
            foreach (var @interface in interfaces)
            {
                if (@interface.Kind == SymbolKind.ErrorType)
                {
                    return true;
                }

                var interfaceMethods = @interface.GetMembers().OfType<IMethodSymbol>();
                if (interfaceMethods.Any(interfaceMethod => declaredMethodSymbol.Equals(containingType.FindImplementationForInterfaceMember(interfaceMethod))))
                {
                    return true;
                }
            }
            return false;
        }

        internal static IMethodSymbol GetDisposeMethod(Compilation compilation)
        {
            return (IMethodSymbol)compilation.GetSpecialType(SpecialType.System_IDisposable)
                .GetMembers("Dispose")
                .SingleOrDefault();
        }
    }
}
