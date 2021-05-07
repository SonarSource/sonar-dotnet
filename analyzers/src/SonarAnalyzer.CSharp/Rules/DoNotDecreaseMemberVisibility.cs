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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DoNotDecreaseMemberVisibility : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4015";
        private const string MessageFormat = "This member hides '{0}'. Make it non-private or seal the class.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var classDeclaration = (TypeDeclarationSyntax)c.Node;
                    var classSymbol = c.SemanticModel.GetDeclaredSymbol(classDeclaration);

                    if (classSymbol == null
                        || classDeclaration.Identifier.IsMissing
                        || classSymbol.IsSealed)
                    {
                        return;
                    }

                    var issueFinder = new IssueFinder(classSymbol, c.SemanticModel);

                    classDeclaration
                        .Members
                        .Select(issueFinder.FindIssue)
                        .WhereNotNull()
                        .ToList()
                        .ForEach(d => c.ReportDiagnosticWhenActive(d));
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKindEx.RecordDeclaration);

        private class IssueFinder
        {
            private readonly IList<IMethodSymbol> allBaseClassMethods;
            private readonly IList<IPropertySymbol> allBaseClassProperties;
            private readonly SemanticModel semanticModel;

            public IssueFinder(ITypeSymbol classSymbol, SemanticModel semanticModel)
            {
                this.semanticModel = semanticModel;
                var allBaseClassMembers = classSymbol.BaseType
                        .GetSelfAndBaseTypes()
                        .SelectMany(t => t.GetMembers())
                        .Where(symbol => IsSymbolVisibleFromNamespace(symbol, classSymbol.ContainingNamespace))
                        .ToList();

                allBaseClassMethods = allBaseClassMembers.OfType<IMethodSymbol>().ToList();
                allBaseClassProperties = allBaseClassMembers.OfType<IPropertySymbol>().ToList();
            }

            public Diagnostic FindIssue(MemberDeclarationSyntax memberDeclaration)
            {
                var memberSymbol = semanticModel.GetDeclaredSymbol(memberDeclaration);

                if (memberSymbol is IMethodSymbol methodSymbol)
                {
                    return FindMethodIssue(memberDeclaration, methodSymbol);
                }

                if (memberSymbol is IPropertySymbol propertySymbol)
                {
                    return FindPropertyIssue(memberDeclaration, propertySymbol);
                }

                return null;
            }

            private Diagnostic FindMethodIssue(MemberDeclarationSyntax memberDeclaration, IMethodSymbol methodSymbol)
            {
                if (!(memberDeclaration is MethodDeclarationSyntax methodDeclaration)
                    || methodDeclaration.Modifiers.Any(SyntaxKind.NewKeyword))
                {
                    return null;
                }

                var hidingMethod = allBaseClassMethods.FirstOrDefault(m => IsDecreasingAccess(m.DeclaredAccessibility, methodSymbol.DeclaredAccessibility, false)
                                                                           && IsMatchingSignature(m, methodSymbol));

                if (hidingMethod != null)
                {
                    var location = methodDeclaration.Identifier.GetLocation();
                    if (location != null)
                    {
                        return Diagnostic.Create(Rule, location, hidingMethod);
                    }
                }

                return null;
            }

            private Diagnostic FindPropertyIssue(MemberDeclarationSyntax memberDeclaration, IPropertySymbol propertySymbol)
            {
                if (!(memberDeclaration is PropertyDeclarationSyntax propertyDeclaration)
                    || propertyDeclaration.Modifiers.Any(SyntaxKind.NewKeyword))
                {
                    return null;
                }

                var hidingProperty = allBaseClassProperties.FirstOrDefault(p => IsDecreasingPropertyAccess(p, propertySymbol, propertySymbol.IsOverride));
                if (hidingProperty != null)
                {
                    var location = propertyDeclaration.Identifier.GetLocation();
                    return Diagnostic.Create(Rule, location, hidingProperty);
                }

                return null;
            }

            private static bool IsSymbolVisibleFromNamespace(ISymbol symbol, INamespaceSymbol ns) =>
                symbol.DeclaredAccessibility != Accessibility.Private
                && (symbol.DeclaredAccessibility != Accessibility.Internal || ns.Equals(symbol.ContainingNamespace));

            private static bool IsDecreasingPropertyAccess(IPropertySymbol baseProperty, IPropertySymbol propertySymbol, bool isOverride)
            {
                if (baseProperty.Name != propertySymbol.Name
                    || !AreParameterTypesEqual(baseProperty.Parameters, propertySymbol.Parameters))
                {
                    return false;
                }

                var baseGetAccess = GetEffectiveDeclaredAccess(baseProperty.GetMethod, baseProperty.DeclaredAccessibility);
                var baseSetAccess = GetEffectiveDeclaredAccess(baseProperty.SetMethod, baseProperty.DeclaredAccessibility);

                var propertyGetAccess = GetEffectiveDeclaredAccess(propertySymbol.GetMethod, baseProperty.DeclaredAccessibility);
                var propertySetAccess = GetEffectiveDeclaredAccess(propertySymbol.SetMethod, baseProperty.DeclaredAccessibility);

                return IsDecreasingAccess(baseGetAccess, propertyGetAccess, isOverride)
                       || IsDecreasingAccess(baseSetAccess, propertySetAccess, isOverride);
            }

            private static Accessibility GetEffectiveDeclaredAccess(ISymbol symbol, Accessibility defaultAccessibility)
            {
                if (symbol == null)
                {
                    return Accessibility.NotApplicable;
                }

                return symbol.DeclaredAccessibility == Accessibility.NotApplicable
                    ? defaultAccessibility
                    : symbol.DeclaredAccessibility;
            }

            private static bool IsMatchingSignature(IMethodSymbol baseMethod, IMethodSymbol methodSymbol) =>
                baseMethod.Name == methodSymbol.Name
                && baseMethod.TypeParameters.Length == methodSymbol.TypeParameters.Length
                && AreParameterTypesEqual(baseMethod.Parameters, methodSymbol.Parameters);

            private static bool AreParameterTypesEqual(IEnumerable<IParameterSymbol> first, IEnumerable<IParameterSymbol> second) =>
                first.Equals(second, AreParameterTypesEqual);

            private static bool AreParameterTypesEqual(IParameterSymbol first, IParameterSymbol second)
            {
                if (first.RefKind != second.RefKind)
                {
                    return false;
                }

                return first.Type.TypeKind == TypeKind.TypeParameter
                    ? second.Type.TypeKind == TypeKind.TypeParameter
                    : Equals(first.Type.OriginalDefinition, second.Type.OriginalDefinition);
            }

            private static bool IsDecreasingAccess(Accessibility baseAccess, Accessibility memberAccess, bool isOverride)
            {
                if (memberAccess == Accessibility.NotApplicable && isOverride)
                {
                    return false;
                }

                return (baseAccess != Accessibility.NotApplicable && memberAccess == Accessibility.Private)
                       || (baseAccess == Accessibility.Public && memberAccess != Accessibility.Public);
            }
        }
    }
}
