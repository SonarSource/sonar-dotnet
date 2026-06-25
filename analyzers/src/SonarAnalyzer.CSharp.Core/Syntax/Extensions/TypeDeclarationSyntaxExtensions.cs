/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class TypeDeclarationSyntaxExtensions
{
    extension(TypeDeclarationSyntax typeDeclaration)
    {
        /// <summary>
        /// Returns a union of all the methods and local functions from a given type declaration.
        /// </summary>
        public IEnumerable<IMethodDeclaration> GetMethodDeclarations() =>
            typeDeclaration.Members
                           .OfType<MethodDeclarationSyntax>()
                           .SelectMany(method => GetLocalFunctions(method).Union(new List<IMethodDeclaration> { MethodDeclarationFactory.Create(method) }));

        public IMethodSymbol PrimaryConstructor(SemanticModel semanticModel)
        {
            if (typeDeclaration.ParameterList() is { } parameterList)
            {
                return parameterList is { Parameters: { Count: > 0 } parameters } && parameters[0] is { Identifier.RawKind: not (int)SyntaxKind.ArgListKeyword } parameter0
                    ? semanticModel.GetDeclaredSymbol(parameter0)?.ContainingSymbol as IMethodSymbol
                    : semanticModel.GetDeclaredSymbol(typeDeclaration).GetMembers(".ctor").OfType<IMethodSymbol>().FirstOrDefault(m => m is
                    {
                        MethodKind: MethodKind.Constructor,
                        Parameters.Length: 0,
                    });
            }

            return null;
        }

        /// <summary>
        /// Returns the parameter list of the type declaration, or <see langword="null"/> if it has none.
        /// Call this method instead of accessing the <see cref="TypeDeclarationSyntaxShimExtensions.ParameterList"/>
        /// property: it resolves the parameter list correctly across all supported Roslyn versions.
        /// </summary>
        /// <remarks>
        /// The ShimLayer also exposes a <see cref="TypeDeclarationSyntaxShimExtensions.ParameterList"/> extension property on <see cref="TypeDeclarationSyntax"/>,
        /// but it only reads the property declared on the base type, which is <see langword="null"/> on Roslyn 3.7 to 4.6.
        /// On those versions the parameter list lives only on the derived <see cref="ClassDeclarationSyntax"/>,
        /// <see cref="StructDeclarationSyntax"/> and record declarations, so this method special-cases each kind to read
        /// it from the right place. It is deliberately a method and not a property: a same-named extension property here
        /// would be an ambiguous duplicate of the shim property and fail to compile (CS9339). See NET-3482.
        /// </remarks>
        public ParameterListSyntax ParameterList() =>
            typeDeclaration.Kind() switch
            {
                SyntaxKind.ClassDeclaration => ClassDeclarationSyntaxShimExtensions.get_ParameterList((ClassDeclarationSyntax)typeDeclaration),
                SyntaxKind.StructDeclaration => StructDeclarationSyntaxShimExtensions.get_ParameterList((StructDeclarationSyntax)typeDeclaration),
                SyntaxKindEx.RecordDeclaration or SyntaxKindEx.RecordStructDeclaration => ((RecordDeclarationSyntaxWrapper)typeDeclaration).ParameterList,
                SyntaxKindEx.ExtensionBlockDeclaration => ((ExtensionBlockDeclarationSyntaxWrapper)typeDeclaration).ParameterList,
                _ => TypeDeclarationSyntaxShimExtensions.get_ParameterList(typeDeclaration),
            };
    }

    private static IEnumerable<IMethodDeclaration> GetLocalFunctions(MethodDeclarationSyntax methodDeclaration) =>
        methodDeclaration.DescendantNodes()
                         .Where(member => member.IsKind(SyntaxKindEx.LocalFunctionStatement))
                         .Select(member => MethodDeclarationFactory.Create(member));
}
