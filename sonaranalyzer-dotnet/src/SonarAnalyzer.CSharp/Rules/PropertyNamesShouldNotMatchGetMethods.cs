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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class PropertyNamesShouldNotMatchGetMethods : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4059";
        private const string MessageFormat = "Change either the name of the property '{0}' or the name of " +
            "the method '{1}' to make them distinguishable.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var classSymbol = c.SemanticModel.GetDeclaredSymbol(c.Node) as INamedTypeSymbol;
                    if (classSymbol == null)
                    {
                        return;
                    }

                    var classMembers = classSymbol.GetMembers().Where(IsPublicOrProtectedMember);
                    var properties = classMembers.OfType<IPropertySymbol>();
                    var methods = classMembers.OfType<IMethodSymbol>().ToList();

                    properties.Select(p => methods.FirstOrDefault(m => AreCollidingNames(p.Name, m.Name)))

                    foreach (var collidingMembers in GetCollidingMembers(properties, methods))
                    {
                        var propertyIdentifier = collidingMembers.Item1.Identifier;
                        var methodIdentifier = collidingMembers.Item2.Identifier;

                        c.ReportDiagnostic(Diagnostic.Create(
                            rule,
                            propertyIdentifier.GetLocation(),
                            additionalLocations: new[] { methodIdentifier.GetLocation() },
                            messageArgs: new[] { propertyIdentifier.ValueText, methodIdentifier.ValueText }));
                    }
                }, SyntaxKind.ClassDeclaration);
        }

        private static bool IsPublicOrProtectedMember(ISymbol symbol)
        {
            var accessibility = symbol.GetEffectiveAccessibility();

            return accessibility == Accessibility.Public || accessibility == Accessibility.Protected;
        }

        private static IEnumerable<Tuple<PropertyDeclarationSyntax, MethodDeclarationSyntax>> GetCollidingMembers(
            IEnumerable<IPropertySymbol> properties, IEnumerable<IMethodSymbol> methods)
        {

            foreach (var property in properties)
            {
                var collidingMethod = methods.FirstOrDefault(method => AreCollidingNames(property.Name, method.Name));
                if (collidingMethod != null)
                {
                    yield return new Tuple<PropertyDeclarationSyntax, MethodDeclarationSyntax>(
                        property.DeclaringSyntaxReferences.First().GetSyntax() as PropertyDeclarationSyntax,
                        collidingMethod.DeclaringSyntaxReferences.First().GetSyntax() as MethodDeclarationSyntax);
                }
            }
        }

        private static bool AreCollidingNames(string propertyName, string methodName)
        {
            return methodName.Equals(propertyName, StringComparison.OrdinalIgnoreCase) ||
                methodName.Equals("Get" + propertyName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
