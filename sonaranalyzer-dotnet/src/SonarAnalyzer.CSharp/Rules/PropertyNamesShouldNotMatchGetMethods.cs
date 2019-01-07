/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
        private const string MessageFormat = "Change either the name of property '{0}' or the name of " +
            "method '{1}' to make them distinguishable.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    if (!(c.SemanticModel.GetDeclaredSymbol(c.Node) is INamedTypeSymbol classSymbol))
                    {
                        return;
                    }

                    var classMembers = classSymbol.GetMembers().Where(SymbolHelper.IsPubliclyAccessible);
                    var properties = classMembers.OfType<IPropertySymbol>().Where(property => !property.IsOverride);
                    var methods = classMembers.OfType<IMethodSymbol>().ToList();

                    foreach (var collidingMembers in GetCollidingMembers(properties, methods))
                    {
                        var propertyIdentifier = collidingMembers.Item1;
                        var methodIdentifier = collidingMembers.Item2;

                        c.ReportDiagnosticWhenActive(Diagnostic.Create(
                            rule,
                            propertyIdentifier.GetLocation(),
                            additionalLocations: new[] { methodIdentifier.GetLocation() },
                            messageArgs: new[] { propertyIdentifier.ValueText, methodIdentifier.ValueText }));
                    }
                }, SyntaxKind.ClassDeclaration);
        }

        private static IEnumerable<Tuple<SyntaxToken, SyntaxToken>> GetCollidingMembers(
            IEnumerable<IPropertySymbol> properties, IEnumerable<IMethodSymbol> methods)
        {
            foreach (var property in properties)
            {
                var collidingMethod = methods.FirstOrDefault(method => AreCollidingNames(property.Name, method.Name));
                if (collidingMethod == null)
                {
                    continue;
                }

                var methodSyntax = collidingMethod.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()
                    as MethodDeclarationSyntax;

                if (!(property.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is PropertyDeclarationSyntax propertySyntax) || methodSyntax == null)
                {
                    continue;
                }

                yield return new Tuple<SyntaxToken, SyntaxToken>(propertySyntax.Identifier, methodSyntax.Identifier);
            }
        }

        private static bool AreCollidingNames(string propertyName, string methodName)
        {
            return methodName.Equals(propertyName, StringComparison.OrdinalIgnoreCase) ||
                methodName.Equals("Get" + propertyName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
