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

using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace SonarAnalyzer.Rules
{
    public abstract class ClassNotInstantiatableBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S3453";
        protected const string MessageFormat = "This class can't be instantiated; make {0} 'public'.";

        protected static bool HasNonPrivateConstructor(IEnumerable<IMethodSymbol> constructors)
        {
            return constructors.Any(method => method.DeclaredAccessibility != Accessibility.Private);
        }

        protected static bool IsAnyConstructorCalled
            <TBaseTypeSyntax, TObjectCreationSyntax, TClassDeclarationSyntax>
            (INamedTypeSymbol namedType, IEnumerable<SyntaxNodeAndSemanticModel<TBaseTypeSyntax>> typeDeclarations)
            where TBaseTypeSyntax : SyntaxNode
            where TObjectCreationSyntax : SyntaxNode
            where TClassDeclarationSyntax : SyntaxNode
        {
            return typeDeclarations
                .Select(classDeclaration => new
                {
                    classDeclaration.SemanticModel,
                    DescendantNodes = classDeclaration.SyntaxNode.DescendantNodes().ToList()
                })
                .Any(descendants =>
                    IsAnyConstructorToCurrentType<TObjectCreationSyntax>(descendants.DescendantNodes, namedType, descendants.SemanticModel) ||
                    IsAnyNestedTypeExtendingCurrentType<TClassDeclarationSyntax>(descendants.DescendantNodes, namedType, descendants.SemanticModel));
        }

        protected static bool IsAnyNestedTypeExtendingCurrentType<TClassDeclarationSyntax>(
            IEnumerable<SyntaxNode> descendantNodes, INamedTypeSymbol namedType, SemanticModel semanticModel)
            where TClassDeclarationSyntax : SyntaxNode
        {
            return descendantNodes
                .OfType<TClassDeclarationSyntax>()
                .Select(c => (semanticModel.GetDeclaredSymbol(c) as ITypeSymbol)?.BaseType)
                .Any(baseType => baseType != null && baseType.OriginalDefinition.DerivesFrom(namedType));
        }

        protected static bool IsAnyConstructorToCurrentType<TObjectCreationSyntax>(
            IEnumerable<SyntaxNode> descendantNodes, INamedTypeSymbol namedType, SemanticModel semanticModel)
            where TObjectCreationSyntax : SyntaxNode =>
            descendantNodes
                .OfType<TObjectCreationSyntax>()
                .Select(ctor => semanticModel.GetSymbolInfo(ctor).Symbol as IMethodSymbol)
                .WhereNotNull()
                .Any(ctor => Equals(ctor.ContainingType?.OriginalDefinition, namedType));

        protected static IEnumerable<IMethodSymbol> GetConstructors(IEnumerable<ISymbol> members) =>
            members
                .OfType<IMethodSymbol>()
                .Where(method => method.MethodKind == MethodKind.Constructor);

        protected static bool HasOnlyStaticMembers(ICollection<ISymbol> members) =>
            members.Any()
            && members.All(member => member.IsStatic);

        protected static bool IsNonStaticClassWithNoAttributes(INamedTypeSymbol namedType) =>
            namedType.IsClass()
            && !namedType.IsStatic
            && !namedType.GetAttributes().Any();

        protected static bool HasOnlyCandidateConstructors(ICollection<IMethodSymbol> constructors) =>
            constructors.Any()
            && !HasNonPrivateConstructor(constructors)
            && constructors.All(c => !c.GetAttributes().Any());
    }
}
