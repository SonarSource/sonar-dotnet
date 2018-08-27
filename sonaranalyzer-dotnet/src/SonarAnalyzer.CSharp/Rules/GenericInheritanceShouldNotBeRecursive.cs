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
    public sealed class GenericInheritanceShouldNotBeRecursive : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3464";
        private const string MessageFormat = "Refactor this {0} so that the generic inheritance chain is not recursive.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var typeDeclaration = (TypeDeclarationSyntax)c.Node;
                    var typeSymbol = c.SemanticModel.GetDeclaredSymbol(typeDeclaration);

                    if (!IsGenericType(typeSymbol))
                    {
                        return;
                    }

                    var baseTypes = GetBaseTypes(typeSymbol);

                    if (baseTypes.Any(t => IsGenericType(t) && HasRecursiveGenericSubstitution(t, typeSymbol)))
                    {
                        c.ReportDiagnosticWhenActive(
                            Diagnostic.Create(rule, typeDeclaration.Identifier.GetLocation(), typeDeclaration.Keyword));
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.InterfaceDeclaration);
        }

        private static IEnumerable<INamedTypeSymbol> GetBaseTypes(INamedTypeSymbol typeSymbol)
        {
            var interfaces = typeSymbol.Interfaces.Where(IsGenericType);
            return typeSymbol.IsClass()
                ? interfaces.Concat(new[] { typeSymbol.BaseType })
                : interfaces;
        }

        private static bool HasRecursiveGenericSubstitution(INamedTypeSymbol typeSymbol, INamedTypeSymbol declaredType)
        {
            bool IsSameAsDeclaredType(INamedTypeSymbol type) =>
                type.OriginalDefinition.Equals(declaredType) && HasSubstitutedTypeArguments(type);

            bool ContainsRecursiveGenericSubstitution(IEnumerable<ITypeSymbol> types) =>
                types.OfType<INamedTypeSymbol>()
                    .Any(type => IsSameAsDeclaredType(type) || ContainsRecursiveGenericSubstitution(type.TypeArguments));

            return ContainsRecursiveGenericSubstitution(typeSymbol.TypeArguments);
        }

        private static bool IsGenericType(INamedTypeSymbol type) =>
            type != null && type.IsGenericType;

        private static bool HasSubstitutedTypeArguments(INamedTypeSymbol type) =>
            type.TypeArguments.OfType<INamedTypeSymbol>().Any();
    }
}
