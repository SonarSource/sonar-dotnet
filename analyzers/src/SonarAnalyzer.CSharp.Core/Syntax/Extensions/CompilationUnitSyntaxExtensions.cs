/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
    public static IEnumerable<SyntaxNode> GetTopLevelMainBody(this CompilationUnitSyntax compilationUnit) =>
        compilationUnit.ChildNodes()
                       .SkipWhile(x => x.Kind() is SyntaxKind.UsingDirective or SyntaxKind.NamespaceDeclaration)
                       .TakeWhile(x => x.IsKind(SyntaxKind.GlobalStatement));

    public static IEnumerable<IMethodDeclaration> GetMethodDeclarations(this CompilationUnitSyntax compilationUnitSyntax) =>
        compilationUnitSyntax.GetTopLevelMainBody()
                             .Select(x => x.ChildNodes().FirstOrDefault(y => y.IsKind(SyntaxKindEx.LocalFunctionStatement)))
                             .Where(x => x != null)
                             .Select(x => MethodDeclarationFactory.Create(x));

    public static bool IsTopLevelMain(this CompilationUnitSyntax compilationUnit) =>
        compilationUnit.Members.Any(x => x.IsKind(SyntaxKind.GlobalStatement));
}
