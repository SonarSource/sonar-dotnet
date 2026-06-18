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

public static class CompilationUnitSyntaxExtensions
{
    extension(CompilationUnitSyntax compilationUnit)
    {
        public IEnumerable<SyntaxNode> TopLevelMainBody =>
            compilationUnit.ChildNodes()
                           .SkipWhile(x => x.Kind() is SyntaxKind.UsingDirective or SyntaxKind.NamespaceDeclaration)
                           .TakeWhile(x => x.IsKind(SyntaxKind.GlobalStatement));

        public IEnumerable<IMethodDeclaration> MethodDeclarations =>
            compilationUnit.TopLevelMainBody
                                 .Select(x => x.ChildNodes().FirstOrDefault(y => y.IsKind(SyntaxKindEx.LocalFunctionStatement)))
                                 .Where(x => x != null)
                                 .Select(x => MethodDeclarationFactory.Create(x));

        public bool IsTopLevelMain =>
            compilationUnit.Members.Any(x => x.IsKind(SyntaxKind.GlobalStatement));
    }
}
