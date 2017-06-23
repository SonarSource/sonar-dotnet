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
    public sealed class InterfaceMethodsShouldBeCallableByChildTypes : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4039";
        private const string MessageFormat = "Make '{0}' sealed, change to a non explicit declaration or provide a " +
            "new method exposing the functionality of '{1}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)c.Node;
                    var classSymbol = c.SemanticModel.GetDeclaredSymbol(classDeclaration);

                    if (classSymbol == null ||
                        classSymbol.IsSealed ||
                        classSymbol.IsStatic ||
                        !classSymbol.IsPublicApi() ||
                        classDeclaration.Identifier.IsMissing)
                    {
                        return;
                    }

                    var allTrackedClassMembers = classSymbol.GetMembers().Where(IsTrackedMember);
                    var explicitMembers = allTrackedClassMembers.Where(IsExplicit);
                    var nonExplicitMembers = allTrackedClassMembers.Except(explicitMembers);

                    explicitMembers.Where(member => !HasPublicEquivalentMember(member, nonExplicitMembers))
                        .Select(faultyMember => CreateDiagnostic(classDeclaration, faultyMember))
                        .WhereNotNull()
                        .ToList()
                        .ForEach(diagnostic => c.ReportDiagnostic(diagnostic));
                }, SyntaxKind.ClassDeclaration);
        }

        private static bool IsTrackedMember(ISymbol symbol)
        {
            return symbol.Kind == SymbolKind.Event ||
                symbol.Kind == SymbolKind.Property ||
                (symbol.Kind == SymbolKind.Method && symbol.Name != ".ctor");
        }

        private static bool IsExplicit(ISymbol symbol)
        {
            var methodSymbol = symbol as IMethodSymbol;
            if (methodSymbol != null)
            {
                return methodSymbol.ExplicitInterfaceImplementations.Length > 0;
            }

            var propertySymbol = symbol as IPropertySymbol;
            if (propertySymbol != null)
            {
                return propertySymbol.ExplicitInterfaceImplementations.Length > 0;
            }

            var eventSymbol = symbol as IEventSymbol;
            if (eventSymbol != null)
            {
                return eventSymbol.ExplicitInterfaceImplementations.Length > 0;
            }

            return false;
        }

        private static bool HasPublicEquivalentMember(ISymbol memberSymbolToMatch, IEnumerable<ISymbol> allMembers)
        {
            return allMembers.Any(
                symbol =>
                    symbol.Name == memberSymbolToMatch.Name &&
                    symbol.Kind.Equals(memberSymbolToMatch.Kind) &&
                    symbol.IsPublicApi());
        }

        private static Diagnostic CreateDiagnostic(ClassDeclarationSyntax classDeclaration, ISymbol faultyMember)
        {
            var syntaxNode = faultyMember.DeclaringSyntaxReferences.First().GetSyntax();

            Location location;
            switch (syntaxNode.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                    location = ((MethodDeclarationSyntax)syntaxNode).Identifier.GetLocation();
                    break;
                case SyntaxKind.PropertyDeclaration:
                    location = ((PropertyDeclarationSyntax)syntaxNode).Identifier.GetLocation();
                    break;
                case SyntaxKind.EventDeclaration:
                    location = ((EventDeclarationSyntax)syntaxNode).Identifier.GetLocation();
                    break;
                default:
                    location = null; // Should not happen but let's play defensive
                    break;
            }
            if (location == null)
            {
                return null;
            }

            return Diagnostic.Create(rule, location, classDeclaration.Identifier.ValueText, faultyMember.Name);
        }
    }
}
