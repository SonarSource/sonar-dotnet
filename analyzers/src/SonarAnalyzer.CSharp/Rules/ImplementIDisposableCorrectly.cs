/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ImplementIDisposableCorrectly : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3881";
        private const string MessageFormat = "Fix this implementation of 'IDisposable' to conform to the dispose pattern.";

        private static readonly ISet<SyntaxKind> NotAllowedDisposeModifiers = new HashSet<SyntaxKind>
        {
            SyntaxKind.VirtualKeyword,
            SyntaxKind.AbstractKeyword
        };

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    if (c.ContainingSymbol.Kind != SymbolKind.NamedType)
                    {
                        return;
                    }

                    var typeDeclarationSyntax = (TypeDeclarationSyntax)c.Node;
                    var declarationIdentifier = typeDeclarationSyntax.Identifier;
                    var checker = new DisposableChecker(typeDeclarationSyntax.BaseList,
                                                        declarationIdentifier,
                                                        c.SemanticModel.GetDeclaredSymbol(typeDeclarationSyntax),
                                                        c.Node.GetDeclarationTypeName(),
                                                        c.SemanticModel);

                    var locations = checker.GetIssueLocations(typeDeclarationSyntax);
                    if (locations.Any())
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, declarationIdentifier.GetLocation(),
                                                                       locations.ToAdditionalLocations(),
                                                                       locations.ToProperties()));
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKindEx.RecordDeclaration);

        private class DisposableChecker
        {
            private readonly SemanticModel semanticModel;
            private readonly List<SecondaryLocation> secondaryLocations = new List<SecondaryLocation>();
            private readonly BaseListSyntax baseTypes;
            private readonly SyntaxToken typeIdentifier;
            private readonly INamedTypeSymbol typeSymbol;
            private readonly string nodeType;

            public DisposableChecker(BaseListSyntax baseTypes, SyntaxToken typeIdentifier, INamedTypeSymbol typeSymbol, string nodeType, SemanticModel semanticModel)
            {
                this.baseTypes = baseTypes;
                this.typeIdentifier = typeIdentifier;
                this.typeSymbol = typeSymbol;
                this.nodeType = nodeType;
                this.semanticModel = semanticModel;
            }

            public List<SecondaryLocation> GetIssueLocations(TypeDeclarationSyntax typeDeclarationSyntax)
            {
                if (typeSymbol == null || typeSymbol.IsSealed)
                {
                    return new List<SecondaryLocation>();
                }

                if (typeSymbol.BaseType.Implements(KnownType.System_IDisposable))
                {
                    var iDisposableInterfaceSyntax = baseTypes?.Types.FirstOrDefault(IsOrImplementsIDisposable);
                    if (iDisposableInterfaceSyntax != null)
                    {
                        AddSecondaryLocation(iDisposableInterfaceSyntax.GetLocation(),
                                             $"Remove 'IDisposable' from the list of interfaces implemented by '{typeSymbol.Name}'"
                                             + $" and override the base {nodeType} 'Dispose' implementation instead.");
                    }

                    if (HasVirtualDisposeBool(typeSymbol.BaseType))
                    {
                        VerifyDisposeOverrideCallsBase(FindMethodImplementation(typeSymbol, IsDisposeBool, typeDeclarationSyntax)
                                                       .OfType<MethodDeclarationSyntax>()
                                                       .FirstOrDefault());
                    }

                    return secondaryLocations;
                }

                if (typeSymbol.Implements(KnownType.System_IDisposable))
                {
                    if (!FindMethodDeclarations(typeSymbol, IsDisposeBool).Any())
                    {
                        AddSecondaryLocation(typeIdentifier.GetLocation(),
                                             $"Provide 'protected' overridable implementation of 'Dispose(bool)' on "
                                             + $"'{typeSymbol.Name}' or mark the type as 'sealed'.");
                    }

                    var destructor = FindMethodImplementation(typeSymbol, SymbolHelper.IsDestructor, typeDeclarationSyntax)
                                     .OfType<DestructorDeclarationSyntax>()
                                     .FirstOrDefault();

                    VerifyDestructor(destructor);

                    var disposeMethod = FindMethodImplementation(typeSymbol, KnownMethods.IsIDisposableDispose, typeDeclarationSyntax)
                                        .OfType<MethodDeclarationSyntax>()
                                        .FirstOrDefault();

                    VerifyDispose(disposeMethod, typeSymbol.IsSealed);
                }

                return secondaryLocations;
            }

            private void AddSecondaryLocation(Location location, string message) =>
                secondaryLocations.Add(new SecondaryLocation(location, message));

            private void VerifyDestructor(DestructorDeclarationSyntax destructorSyntax)
            {
                if (!destructorSyntax.HasBodyOrExpressionBody())
                {
                    return;
                }

                if (!HasStatementsCount(destructorSyntax, 1) || !CallsVirtualDispose(destructorSyntax, argumentValue: "false"))
                {
                    AddSecondaryLocation(destructorSyntax.Identifier.GetLocation(),
                                         $"Modify '{typeSymbol.Name}.~{typeSymbol.Name}()' so that it calls 'Dispose(false)' and "
                                         + "then returns.");
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
                    AddSecondaryLocation(disposeMethod.Identifier.GetLocation(), $"Modify 'Dispose({parameterName})' so that it calls 'base.Dispose({parameterName})'.");
                }
            }

            private void VerifyDispose(MethodDeclarationSyntax disposeMethod, bool isSealedClass)
            {
                if (disposeMethod == null)
                {
                    return;
                }

                if (disposeMethod.HasBodyOrExpressionBody() && !isSealedClass)
                {
                    var missingVirtualDispose = !CallsVirtualDispose(disposeMethod, argumentValue: "true");
                    var missingSuppressFinalize = !CallsSuppressFinalize(disposeMethod);
                    string remediation = null;

                    if (missingVirtualDispose && missingSuppressFinalize)
                    {
                        remediation = "should call 'Dispose(true)' and 'GC.SuppressFinalize(this)'.";
                    }
                    else if (missingVirtualDispose)
                    {
                        remediation = "should also call 'Dispose(true)'.";
                    }
                    else if (missingSuppressFinalize)
                    {
                        remediation = "should also call 'GC.SuppressFinalize(this)'.";
                    }
                    else if (!HasStatementsCount(disposeMethod, 2))
                    {
                        remediation = "should call 'Dispose(true)', 'GC.SuppressFinalize(this)' and nothing else.";
                    }

                    if (remediation != null)
                    {
                        AddSecondaryLocation(disposeMethod.Identifier.GetLocation(), $"'{typeSymbol.Name}.Dispose()' {remediation}");
                    }
                }

                // Because of partial classes we cannot always rely on the current semantic model.
                // See issue: https://github.com/SonarSource/sonar-dotnet/issues/690
                var disposeMethodSymbol = disposeMethod.SyntaxTree.GetSemanticModelOrDefault(semanticModel)?.GetDeclaredSymbol(disposeMethod);
                if (disposeMethodSymbol == null)
                {
                    return;
                }

                if (disposeMethodSymbol.IsAbstract || disposeMethodSymbol.IsVirtual)
                {
                    var modifier = disposeMethod.Modifiers
                                                .FirstOrDefault(m => m.IsAnyKind(NotAllowedDisposeModifiers));

                    AddSecondaryLocation(modifier.GetLocation(), $"'{typeSymbol.Name}.Dispose()' should not be 'virtual' or 'abstract'.");
                }

                if (disposeMethodSymbol.ExplicitInterfaceImplementations.Any())
                {
                    AddSecondaryLocation(disposeMethod.Identifier.GetLocation(), $"'{typeSymbol.Name}.Dispose()' should be 'public'.");
                }
            }

            private bool IsOrImplementsIDisposable(BaseTypeSyntax baseType) =>
                (semanticModel.GetSymbolInfo(baseType.Type).Symbol as INamedTypeSymbol).Is(KnownType.System_IDisposable);

            private bool CallsSuppressFinalize(BaseMethodDeclarationSyntax methodDeclaration) =>
                methodDeclaration.ContainsMethodInvocation(semanticModel, method => HasArgumentValues(method, "this"), KnownMethods.IsGcSuppressFinalize);

            private bool CallsVirtualDispose(BaseMethodDeclarationSyntax methodDeclaration, string argumentValue) =>
                methodDeclaration.ContainsMethodInvocation(semanticModel, method => HasArgumentValues(method, argumentValue), IsDisposeBool);

            private static bool IsDisposeBool(IMethodSymbol method) =>
                method.Name == nameof(IDisposable.Dispose)
                && (method.IsVirtual || method.IsAbstract || method.IsOverride)
                && method.DeclaredAccessibility == Accessibility.Protected
                && method.Parameters.Length == 1
                && method.Parameters.Any(p => p.Type.Is(KnownType.System_Boolean));

            private static bool HasArgumentValues(InvocationExpressionSyntax invocation, params string[] arguments) =>
                invocation.HasExactlyNArguments(arguments.Length)
                && invocation.ArgumentList.Arguments
                             .Select((a, index) => a.Expression.ToString() == arguments[index])
                             .All(matching => matching);

            private static bool HasStatementsCount(BaseMethodDeclarationSyntax methodDeclaration, int expectedStatementsCount) =>
                methodDeclaration.Body?.Statements.Count == expectedStatementsCount
                || (methodDeclaration.ExpressionBody() != null && expectedStatementsCount == 1); // Expression body has only one statement

            private static IEnumerable<SyntaxNode> FindMethodDeclarations(INamedTypeSymbol typeSymbol, Func<IMethodSymbol, bool> predicate) =>
                typeSymbol.GetMembers()
                          .OfType<IMethodSymbol>()
                          .Where(predicate)
                          .SelectMany(symbol => symbol.PartialImplementationPart?.DeclaringSyntaxReferences ?? symbol.DeclaringSyntaxReferences)
                          .Select(reference => reference.GetSyntax());

            private static IEnumerable<SyntaxNode> FindMethodImplementation(INamedTypeSymbol typeSymbol, Func<IMethodSymbol, bool> predicate, TypeDeclarationSyntax typeDeclarationSyntax) =>
                FindMethodDeclarations(typeSymbol, predicate)
                    .OfType<BaseMethodDeclarationSyntax>()
                    .Where(syntax => typeDeclarationSyntax.Contains(syntax) && (syntax.HasBodyOrExpressionBody() || syntax.Modifiers.AnyOfKind(SyntaxKind.AbstractKeyword)));

            private static bool HasVirtualDisposeBool(ITypeSymbol typeSymbol) =>
                typeSymbol.GetSelfAndBaseTypes()
                          .SelectMany(type => type.GetMembers())
                          .OfType<IMethodSymbol>()
                          .Where(IsDisposeBool)
                          .Any(symbol => !symbol.IsAbstract);
        }
    }
}
