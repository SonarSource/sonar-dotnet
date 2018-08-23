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
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ImplementIDisposableCorrectly : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3881";
        private const string MessageFormat = "Fix this implementation of 'IDisposable' to conform to the dispose pattern.";

        private static readonly ISet<SyntaxKind> notAllowedDisposeModifiers = new HashSet<SyntaxKind>
        {
            SyntaxKind.VirtualKeyword,
            SyntaxKind.AbstractKeyword
        };

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var classDeclaration = (ClassDeclarationSyntax)c.Node;

                var checker = new DisposableChecker(classDeclaration, c.SemanticModel);
                var locations = checker.GetIssueLocations();
                if (locations.Any())
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, classDeclaration.Identifier.GetLocation(),
                        locations.ToAdditionalLocations(),
                        locations.ToProperties()));
                }
            },
            SyntaxKind.ClassDeclaration);
        }

        private class DisposableChecker
        {
            private readonly ClassDeclarationSyntax classDeclaration;
            private readonly SemanticModel semanticModel;
            private readonly List<SecondaryLocation> secondaryLocations = new List<SecondaryLocation>();
            private readonly INamedTypeSymbol classSymbol;

            public DisposableChecker(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
            {
                this.classDeclaration = classDeclaration;
                this.semanticModel = semanticModel;
                classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
            }

            public IEnumerable<SecondaryLocation> GetIssueLocations()
            {
                if (classSymbol == null || classSymbol.IsSealed)
                {
                    return Enumerable.Empty<SecondaryLocation>();
                }

                if (classSymbol.BaseType.Implements(KnownType.System_IDisposable))
                {
                    var idisposableInterfaceSyntax = classDeclaration.BaseList?.Types
                        .FirstOrDefault(IsOrImplementsIDisposable);

                    if (idisposableInterfaceSyntax != null)
                    {
                        AddSecondaryLocation(idisposableInterfaceSyntax.GetLocation(),
                            $"Remove 'IDisposable' from the list of interfaces implemented by '{classSymbol.Name}'"
                                + " and override the base class 'Dispose' implementation instead.");
                    }

                    if (HasVirtualDisposeBool(classSymbol.BaseType))
                    {
                        VerifyDisposeOverrideCallsBase(FindMethodDeclarations(classSymbol, IsDisposeBool)
                            .OfType<MethodDeclarationSyntax>()
                            .FirstOrDefault());
                    }

                    return secondaryLocations;
                }

                if (classSymbol.Implements(KnownType.System_IDisposable))
                {
                    if (!FindMethodDeclarations(classSymbol, IsDisposeBool).Any())
                    {
                        AddSecondaryLocation(classDeclaration.Identifier.GetLocation(),
                            $"Provide 'protected' overridable implementation of 'Dispose(bool)' on "
                                + $"'{classSymbol.Name}' or mark the type as 'sealed'.");
                    }

                    var destructor = FindMethodDeclarations(classSymbol, SymbolHelper.IsDestructor)
                        .OfType<DestructorDeclarationSyntax>()
                        .FirstOrDefault();

                    VerifyDestructor(destructor);

                    var disposeMethod = FindMethodDeclarations(classSymbol, KnownMethods.IsIDisposableDispose)
                        .OfType<MethodDeclarationSyntax>()
                        .FirstOrDefault();

                    VerifyDispose(disposeMethod, classSymbol.IsSealed);
                }

                return secondaryLocations;
            }

            private void AddSecondaryLocation(Location location, string message)
            {
                secondaryLocations.Add(new SecondaryLocation(location, message));
            }

            private void VerifyDestructor(DestructorDeclarationSyntax destructorSyntax)
            {
                if (!destructorSyntax.HasBodyOrExpressionBody())
                {
                    return;
                }

                if (!HasStatementsCount(destructorSyntax, 1) ||
                    !CallsVirtualDispose(destructorSyntax, argumentValue: "false"))
                {
                    AddSecondaryLocation(destructorSyntax.Identifier.GetLocation(),
                        $"Modify '{classSymbol.Name}.~{classSymbol.Name}()' so that it calls 'Dispose(false)' and " +
                        "then returns.");
                }
            }

            private void VerifyDisposeOverrideCallsBase(MethodDeclarationSyntax disposeMethod)
            {
                if (!disposeMethod.HasBodyOrExpressionBody())
                {
                    return;
                }

                var parameterName = disposeMethod.ParameterList.Parameters.Single().Identifier.ToString();

                if (!CallsVirtualDispose(disposeMethod, argumentValue: parameterName))
                {
                    AddSecondaryLocation(disposeMethod.Identifier.GetLocation(),
                        $"Modify 'Dispose({parameterName})' so that it calls 'base.Dispose({parameterName})'.");
                }
            }

            private void VerifyDispose(MethodDeclarationSyntax disposeMethod, bool isSealedClass)
            {
                if (disposeMethod == null)
                {
                    return;
                }

                if (disposeMethod.HasBodyOrExpressionBody() &&
                    !isSealedClass &&
                        (
                            !HasStatementsCount(disposeMethod, 2) ||
                            !CallsVirtualDispose(disposeMethod, argumentValue: "true") ||
                            !CallsSuppressFinalize(disposeMethod)
                        ))
                {
                    AddSecondaryLocation(disposeMethod.Identifier.GetLocation(),
                        $"'{classSymbol.Name}.Dispose()' should only invoke 'Dispose(true)' and " +
                        "'GC.SuppressFinalize(this)'.");
                }

                // Because of partial classes we cannot always rely on the current semantic model.
                // See issue: https://github.com/SonarSource/sonar-csharp/issues/690
                var disposeMethodSymbol = semanticModel.GetSyntaxTreeSemanticModel(disposeMethod)
                    .GetDeclaredSymbol(disposeMethod);
                if (disposeMethodSymbol == null)
                {
                    return;
                }

                if (disposeMethodSymbol.IsAbstract ||
                    disposeMethodSymbol.IsVirtual)
                {
                    var modifier = disposeMethod.Modifiers
                        .FirstOrDefault(m => m.IsAnyKind(notAllowedDisposeModifiers));

                    AddSecondaryLocation(modifier.GetLocation(),
                        $"'{classSymbol.Name}.Dispose()' should not be 'virtual' or 'abstract'.");
                }

                if (disposeMethodSymbol.ExplicitInterfaceImplementations.Any())
                {
                    AddSecondaryLocation(disposeMethod.Identifier.GetLocation(),
                        $"'{classSymbol.Name}.Dispose()' should be 'public'.");
                }
            }

            private static bool IsDisposeBool(IMethodSymbol method)
            {
                return method.Name == nameof(IDisposable.Dispose) &&
                    (method.IsVirtual || method.IsAbstract || method.IsOverride) &&
                    method.DeclaredAccessibility == Accessibility.Protected &&
                    method.Parameters.Length == 1 &&
                    method.Parameters.Any(p => p.Type.Is(KnownType.System_Boolean));
            }

            private bool IsOrImplementsIDisposable(BaseTypeSyntax baseType)
            {
                return baseType?.Type != null &&
                    (semanticModel.GetSymbolInfo(baseType.Type).Symbol as INamedTypeSymbol)
                        .Is(KnownType.System_IDisposable);
            }

            private static bool HasArgumentValues(InvocationExpressionSyntax invocation, params string[] arguments)
            {
                return invocation.HasExactlyNArguments(arguments.Length) &&
                       invocation.ArgumentList.Arguments
                            .Select((a, index) => a.Expression.ToString() == arguments[index])
                            .All(matching => matching);
            }

            private static bool HasStatementsCount(BaseMethodDeclarationSyntax methodDeclaration, int expectedStatementsCount) =>
                methodDeclaration.Body?.Statements.Count == expectedStatementsCount ||
                (methodDeclaration.ExpressionBody() != null &&
                    expectedStatementsCount == 1); // Expression body has only one statement

            private bool CallsSuppressFinalize(BaseMethodDeclarationSyntax methodDeclaration)
            {
                return CSharpSyntaxHelper.ContainsMethodInvocation(methodDeclaration,
                    semanticModel,
                    method => HasArgumentValues(method, "this"),
                    KnownMethods.IsGcSuppressFinalize);
            }

            private bool CallsVirtualDispose(BaseMethodDeclarationSyntax methodDeclaration, string argumentValue)
            {
                return CSharpSyntaxHelper.ContainsMethodInvocation(methodDeclaration,
                    semanticModel,
                    method => HasArgumentValues(method, argumentValue),
                    IsDisposeBool);
            }

            private static IEnumerable<SyntaxNode> FindMethodDeclarations(INamedTypeSymbol typeSymbol,
                Func<IMethodSymbol, bool> predicate)
            {
                return typeSymbol.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(predicate)
                    .SelectMany(m => m.DeclaringSyntaxReferences)
                    .Select(r => r.GetSyntax());
            }

            private bool HasVirtualDisposeBool(INamedTypeSymbol typeSymbol)
            {
                return typeSymbol.GetSelfAndBaseTypes()
                    .SelectMany(t => t.GetMembers())
                    .OfType<IMethodSymbol>()
                    .Where(IsDisposeBool)
                    .SelectMany(m => m.DeclaringSyntaxReferences)
                    .Select(r => r.GetSyntax())
                    // Because of partial classes we cannot always rely on the current semantic model.
                    .Select(r => semanticModel.GetSyntaxTreeSemanticModel(r).GetDeclaredSymbol(r))
                    .WhereNotNull()
                    .Any(methodSym => !methodSym.IsAbstract);
            }
        }
    }
}
