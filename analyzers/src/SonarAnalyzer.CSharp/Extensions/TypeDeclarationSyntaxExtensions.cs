/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Extensions
{
    public static class TypeDeclarationSyntaxExtensions
    {
        /// <summary>
        /// Returns a union of all the methods and local functions from a given type declaration.
        /// </summary>
        public static IEnumerable<IMethodDeclaration> GetMethodDeclarations(this TypeDeclarationSyntax typeDeclaration) =>
            typeDeclaration.Members
                           .OfType<MethodDeclarationSyntax>()
                           .SelectMany(method => GetLocalFunctions(method).Union(new List<IMethodDeclaration> { MethodDeclarationFactory.Create(method) }));

        private static IEnumerable<IMethodDeclaration> GetLocalFunctions(MethodDeclarationSyntax methodDeclaration) =>
            methodDeclaration.DescendantNodes()
                             .Where(member => member.IsKind(SyntaxKindEx.LocalFunctionStatement))
                             .Select(member => MethodDeclarationFactory.Create(member));

        public static IMethodSymbol PrimaryConstructor(this TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel)
        {
            if (((TypeDeclarationSyntaxWrapper)typeDeclaration) is { ParameterList: { } parameterList })
            {
                return parameterList is { Parameters: { Count: > 0 } parameters }
                    ? semanticModel.GetDeclaredSymbol(parameters[0])?.ContainingSymbol as IMethodSymbol
                    : semanticModel.GetDeclaredSymbol(typeDeclaration).GetMembers(".ctor").OfType<IMethodSymbol>().FirstOrDefault(m => m is
                    {
                        MethodKind: MethodKind.Constructor,
                        Parameters.Length: 0,
                    });
            }

            return null;
        }
    }
}
