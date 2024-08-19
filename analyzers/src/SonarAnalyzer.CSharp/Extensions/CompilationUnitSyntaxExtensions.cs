/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
    public static class CompilationUnitSyntaxExtensions
    {
        public static IEnumerable<SyntaxNode> GetTopLevelMainBody(this CompilationUnitSyntax compilationUnit) =>
            compilationUnit.ChildNodes()
                           .SkipWhile(x => x.IsAnyKind(SyntaxKind.UsingDirective, SyntaxKind.NamespaceDeclaration))
                           .TakeWhile(x => x.IsKind(SyntaxKind.GlobalStatement));

        public static IEnumerable<IMethodDeclaration> GetMethodDeclarations(this CompilationUnitSyntax compilationUnitSyntax) =>
            compilationUnitSyntax.GetTopLevelMainBody()
                                 .Select(x => x.ChildNodes().FirstOrDefault(y => y.IsKind(SyntaxKindEx.LocalFunctionStatement)))
                                 .Where(x => x != null)
                                 .Select(x => MethodDeclarationFactory.Create(x));

        public static bool IsTopLevelMain(this CompilationUnitSyntax compilationUnit) =>
            compilationUnit.Members.Any(x => x.IsKind(SyntaxKind.GlobalStatement));
    }
}
