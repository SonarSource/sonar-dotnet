/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Sonar;
using SonarAnalyzer.SymbolicExecution.Sonar.Constraints;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Extensions
{
    internal static class ISymbolExtensions
    {
        public static bool HasConstraint(this ISymbol symbol, SymbolicConstraint constraint, ProgramState programState)
        {
            var symbolicValue = programState.GetSymbolValue(symbol);
            if (symbolicValue == null)
            {
                return false;
            }

            return programState.HasConstraint(symbolicValue, constraint);
        }

        public static ProgramState SetConstraint(this ISymbol symbol, SymbolicConstraint constraint,
            ProgramState programState)
        {
            var symbolicValue = programState.GetSymbolValue(symbol);
            if (symbolicValue == null ||
                programState.HasConstraint(symbolicValue, constraint))
            {
                return programState;
            }

            return programState.SetConstraint(symbolicValue, constraint);
        }

        public static ProgramState RemoveConstraint(this ISymbol symbol, SymbolicConstraint constraint, ProgramState programState)
        {
            var symbolicValue = programState.GetSymbolValue(symbol);
            if (symbolicValue == null ||
                !programState.HasConstraint(symbolicValue, constraint))
            {
                return programState;
            }

            return programState.RemoveConstraint(symbolicValue, constraint);
        }

        public static IEnumerable<SyntaxNode> GetLocationNodes(this ISymbol symbol, SyntaxNode node) =>
            symbol.Locations.SelectMany(location => GetDescendantNodes(location, node));

        private static IEnumerable<SyntaxNode> GetDescendantNodes(Location location, SyntaxNode invocation)
        {
            var locationRootNode = location.SourceTree?.GetRoot();
            var invocationRootNode = invocation.SyntaxTree.GetRoot();

            // We don't look for descendants when the location is outside the current context root
            if (locationRootNode != null && locationRootNode != invocationRootNode)
            {
                return Enumerable.Empty<SyntaxNode>();
            }

            // To optimise, we search first for the class constructor, then for the method declaration.
            // If these cannot be found (e.g. fields), we get the root of the syntax tree and search from there.
            var root = locationRootNode?.FindNode(location.SourceSpan)
                       ?? invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>()
                       ?? invocationRootNode;

            return root.DescendantNodes();
        }

        /// <summary>
        /// Returns the Identifier token of the first declaring <see cref="SyntaxReference"/>.
        /// For e.g. an <see cref="IPropertySymbol"/> this is <see cref="PropertyDeclarationSyntax.Identifier"/>.
        /// </summary>
        /// <returns>The idenifier token or <see langword="null"/> if there is no DeclaringSyntaxReference or the <see cref="SyntaxNode"/> does not have an Identifier.</returns>
        public static SyntaxToken? GetFirstIdentifier(this ISymbol symbol) =>
            symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().NodeIdentifier();

        /// <summary>
        /// Returns the Identifier tokens of all declaring <see cref="SyntaxReference"/>s.
        /// For e.g. an <see cref="INamedTypeSymbol"/> this can be a list of partial <see cref="ClassDeclarationSyntax.Identifier"/>.
        /// </summary>
        public static ImmutableArray<SyntaxToken> GetIdentifiers(this ISymbol symbol) =>
            symbol.DeclaringSyntaxReferences
            .Select(r => r.GetSyntax().NodeIdentifier())
            .WhereNotNull()
            .ToImmutableArray();

        /// <inheritdoc cref="Helpers.Facade.SyntaxFacade{TSyntaxKind}.NodeIdentifier"/>
        public static SyntaxToken? NodeIdentifier(this SyntaxNode node)
            => node.RemoveParentheses() switch
            {
                AttributeArgumentSyntax attribute => attribute.NameColon?.Name.Identifier,
                BaseTypeDeclarationSyntax baseType => baseType.Identifier,
                DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Identifier,
                EnumMemberDeclarationSyntax enumMember => enumMember.Identifier,
                EventDeclarationSyntax @event => @event.Identifier,
                InvocationExpressionSyntax invocation => NodeIdentifier(invocation.Expression),
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier,
                MemberBindingExpressionSyntax memberBinding => memberBinding.Name.Identifier,
                MethodDeclarationSyntax method => method.Identifier,
                ParameterSyntax parameter => parameter.Identifier,
                PropertyDeclarationSyntax property => property.Identifier,
                SimpleNameSyntax simpleName => simpleName.Identifier,
                VariableDeclaratorSyntax variable => variable.Identifier,
                _ => null,
            };
    }
}
