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
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ClassNotInstantiatableBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S3453";
        protected const string MessageFormat = "This {0} can't be instantiated; make {1} 'public'.";

        protected abstract bool IsTypeDeclaration(SyntaxNode node);

        protected bool IsAnyConstructorCalled
            <TBaseTypeSyntax, TObjectCreationSyntax>
            (INamedTypeSymbol namedType, IEnumerable<SyntaxNodeAndSemanticModel<TBaseTypeSyntax>> typeDeclarations)
            where TBaseTypeSyntax : SyntaxNode
            where TObjectCreationSyntax : SyntaxNode =>
            typeDeclarations
                .Select(typeDeclaration => new
                {
                    typeDeclaration.SemanticModel,
                    DescendantNodes = typeDeclaration.SyntaxNode.DescendantNodes().ToList()
                })
                .Any(descendants =>
                    IsAnyConstructorToCurrentType<TObjectCreationSyntax>(descendants.DescendantNodes, namedType, descendants.SemanticModel) ||
                    IsAnyNestedTypeExtendingCurrentType(descendants.DescendantNodes, namedType, descendants.SemanticModel));

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

        protected static bool DerivesFromSafeHandle(ITypeSymbol typeSymbol) =>
            typeSymbol.DerivesFrom(KnownType.System_Runtime_InteropServices_SafeHandle);

        private bool IsAnyNestedTypeExtendingCurrentType(IEnumerable<SyntaxNode> descendantNodes, INamedTypeSymbol namedType, SemanticModel semanticModel) =>
            descendantNodes
                .Where(IsTypeDeclaration)
                .Select(x => (semanticModel.GetDeclaredSymbol(x) as ITypeSymbol)?.BaseType)
                .WhereNotNull()
                .Any(baseType => baseType.OriginalDefinition.DerivesFrom(namedType));

        private static bool HasNonPrivateConstructor(IEnumerable<IMethodSymbol> constructors) =>
            constructors.Any(method => method.DeclaredAccessibility != Accessibility.Private);

        private static bool IsAnyConstructorToCurrentType<TObjectCreationSyntax>(IEnumerable<SyntaxNode> descendantNodes, INamedTypeSymbol namedType, SemanticModel semanticModel)
            where TObjectCreationSyntax : SyntaxNode =>
            descendantNodes
                .OfType<TObjectCreationSyntax>()
                .Select(ctor => semanticModel.GetSymbolInfo(ctor).Symbol as IMethodSymbol)
                .WhereNotNull()
                .Any(ctor => Equals(ctor.ContainingType.OriginalDefinition, namedType));
    }
}
