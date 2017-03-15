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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class ImplementIDisposableCorrectly : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3881";
        private const string MessageFormat = "Fix this implementation of IDisposable to conform to the dispose pattern.";

        private const string DisposeName = nameof(IDisposable.Dispose);
        private const string SuppressFinalizeName = nameof(GC.SuppressFinalize);

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected sealed override DiagnosticDescriptor Rule => rule;

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(CheckIDisposableImplementation, SyntaxKind.ClassDeclaration);
        }

        private void CheckIDisposableImplementation(SyntaxNodeAnalysisContext c)
        {
            var classDeclaration = (ClassDeclarationSyntax)c.Node;
            var classSymbol = c.SemanticModel.GetDeclaredSymbol(classDeclaration);

            if (classSymbol == null || classSymbol.IsSealed)
            {
                return;
            }

            var idisposableInterface = classSymbol.Interfaces.FirstOrDefault(IsOrImplementsIDisposable);
            if (idisposableInterface == null)
            {
                return;
            }

            var implementationErrors = new List<SecondaryLocation>();

            if (classSymbol.BaseType.Implements(KnownType.System_IDisposable))
            {
                var idisposableInterfaceSyntax = classDeclaration.BaseList.Types
                    .FirstOrDefault(t => IsOrImplementsIDisposable(t, c.SemanticModel));
                if (idisposableInterfaceSyntax != null)
                {
                    implementationErrors.Add(new SecondaryLocation(idisposableInterfaceSyntax.GetLocation(),
                        $"Remove IDisposable from the list of interfaces implemented by {classSymbol.Name} and override the base class Dispose implementation instead."));
                }
            }
            else
            {
                if (!FindMethods(classSymbol, IsProtectedVirtualDisposeBool).Any())
                {
                    implementationErrors.Add(new SecondaryLocation(classDeclaration.Identifier.GetLocation(),
                        $"Provide protected overridable implementation of Dispose(bool) on {classSymbol.Name} or mark the type as sealed."));
                }

                var finalizer = FindMethods(classSymbol, IsFinalizer)
                    .OfType<DestructorDeclarationSyntax>()
                    .FirstOrDefault();
                implementationErrors.AddRange(VerifyFinalizer(finalizer, c.SemanticModel, classSymbol));

                var hasFinalizer = finalizer != null;

                var disposeMethods = FindMethods(classSymbol, KnownMethods.IsIDisposableDispose)
                    .OfType<MethodDeclarationSyntax>();
                implementationErrors.AddRange(
                    disposeMethods.SelectMany(disposeMethod => VerifyDispose(disposeMethod, c.SemanticModel, classSymbol, hasFinalizer)));
            }

            if (implementationErrors.Any())
            {
                c.ReportDiagnostic(Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(),
                    implementationErrors.ToAdditionalLocations(),
                    implementationErrors.ToProperties()));
            }
        }

        private IEnumerable<SecondaryLocation> VerifyFinalizer(DestructorDeclarationSyntax finalizer, SemanticModel semanticModel, INamedTypeSymbol classSymbol)
        {
            if (finalizer?.Body == null)
            {
                yield break;
            }

            var finalizerStatementsCount = finalizer.Body.ChildNodes().Count();

            if (finalizerStatementsCount != 1 ||
                !ContainsMethodInvocation(finalizer.Body, semanticModel,
                    method => HasArgumentValues(method, "false"), method => IsOverridableDispose(method, classSymbol)))
            {
                yield return new SecondaryLocation(finalizer.Identifier.GetLocation(),
                    $"Modify {classSymbol.Name}.~{classSymbol.Name}() so that it calls Dispose(false) and then returns.");
            }
        }

        private IEnumerable<SecondaryLocation> VerifyDispose(MethodDeclarationSyntax disposeMethod, SemanticModel semanticModel, INamedTypeSymbol classSymbol, bool hasFinalizer)
        {
            if (disposeMethod?.Body == null)
            {
                yield break;
            }

            var expectedStatementsCount = hasFinalizer ? 2 /*Dispose(true);GC.SuppressFinalize(this)*/: 1 /*Dispose(true)*/;

            if (disposeMethod.Body.ChildNodes().Count() != expectedStatementsCount ||
                !CallsVirtualVoidDispose(disposeMethod, semanticModel, classSymbol) ||
                (hasFinalizer && !CallsSuppressFinalize(disposeMethod, semanticModel)))
            {
                yield return new SecondaryLocation(disposeMethod.Identifier.GetLocation(),
                    $"{classSymbol.Name}.Dispose() should contain only a call to {classSymbol.Name}.Dispose(true) and if the class contains a finalizer, call to GC.SuppressFinalize(this).");
            }

            var disposeMethodSymbol = semanticModel.GetDeclaredSymbol(disposeMethod);
            if (disposeMethodSymbol == null)
            {
                yield break;
            }

            if (disposeMethodSymbol.IsAbstract || disposeMethodSymbol.IsVirtual)
            {
                var modifier = disposeMethod.Modifiers.FirstOrDefault(t => t.IsKind(SyntaxKind.VirtualKeyword) || t.IsKind(SyntaxKind.AbstractKeyword));
                yield return new SecondaryLocation(modifier.GetLocation(),
                    $"{classSymbol.Name}.Dispose() should be sealed.");
            }

            if (disposeMethodSymbol.ExplicitInterfaceImplementations.Any())
            {
                yield return new SecondaryLocation(disposeMethod.Identifier.GetLocation(),
                    $"{classSymbol.Name}.Dispose() should be public.");
            }
        }

        private static bool CallsSuppressFinalize(MethodDeclarationSyntax disposeMethod, SemanticModel semanticModel)
        {
            return ContainsMethodInvocation(disposeMethod.Body, semanticModel,
                method => HasArgumentValues(method, "this"), method => IsSuppressFinalize(method));
        }

        private static bool CallsVirtualVoidDispose(MethodDeclarationSyntax disposeMethod, SemanticModel semanticModel, INamedTypeSymbol containingType)
        {
            return ContainsMethodInvocation(disposeMethod.Body, semanticModel,
                method => HasArgumentValues(method, "true"), method => IsOverridableDispose(method, containingType));
        }

        private static bool IsOrImplementsIDisposable(INamedTypeSymbol typeSymbol)
        {
            return typeSymbol != null &&
                (typeSymbol.Is(KnownType.System_IDisposable) || typeSymbol.Implements(KnownType.System_IDisposable));
        }

        private static bool IsOrImplementsIDisposable(BaseTypeSyntax baseType, SemanticModel semanticModel)
        {
            if (baseType?.Type == null)
            {
                return false;
            }

            var typeSymbol = semanticModel.GetSymbolInfo(baseType.Type).Symbol as INamedTypeSymbol;

            return typeSymbol != null && typeSymbol.IsInterface() && IsOrImplementsIDisposable(typeSymbol);
        }

        private static bool IsFinalizer(IMethodSymbol method)
        {
            return method.MethodKind == MethodKind.Destructor;
        }

        private static bool IsSuppressFinalize(IMethodSymbol method)
        {
            return method != null && method.Name == SuppressFinalizeName && method.ContainingType.Is(KnownType.System_GC);
        }

        private static bool IsOverridableDispose(IMethodSymbol method, INamedTypeSymbol containingType)
        {
            return method != null && method.Name == DisposeName && method.IsVirtual && containingType.Equals(method.ContainingType);
        }

        private static bool IsProtectedVirtualDisposeBool(IMethodSymbol method)
        {
            return method.Name == DisposeName &&
                method.IsVirtual &&
                method.DeclaredAccessibility == Accessibility.Protected &&
                method.Parameters.Length == 1 &&
                method.Parameters.Any(p => p.Type.Is(KnownType.System_Boolean));
        }

        private static IEnumerable<SyntaxNode> FindMethods(INamedTypeSymbol typeSymbol, Func<IMethodSymbol, bool> predicate)
        {
            return typeSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(predicate)
                .SelectMany(m => m.DeclaringSyntaxReferences)
                .Select(r => r.GetSyntax());
        }

        private static bool ContainsMethodInvocation(BlockSyntax block, SemanticModel semanticModel,
            Func<InvocationExpressionSyntax, bool> syntaxPredicate, Func<IMethodSymbol, bool> symbolPredicate)
        {
            return block.ChildNodes()
                .OfType<ExpressionStatementSyntax>()
                .Select(e => e.Expression)
                .OfType<InvocationExpressionSyntax>()
                .Where(syntaxPredicate)
                .Select(e => semanticModel.GetSymbolInfo(e.Expression).Symbol)
                .OfType<IMethodSymbol>()
                .Any(symbolPredicate);
        }

        private static bool HasArgumentValues(InvocationExpressionSyntax invocation, params string[] values)
        {
            return invocation.ArgumentList.Arguments.Count == values.Length &&
                   invocation.ArgumentList.Arguments
                        .Select((a, index) => a.Expression.ToString() == values[index])
                        .All(matching => matching);
        }
    }
}
