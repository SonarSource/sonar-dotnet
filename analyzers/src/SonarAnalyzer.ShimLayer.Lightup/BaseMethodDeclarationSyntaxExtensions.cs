/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StyleCop.Analyzers.Lightup;

public static class BaseMethodDeclarationSyntaxExtensions
{
    public static ArrowExpressionClauseSyntax ExpressionBody(this BaseMethodDeclarationSyntax syntax) =>
        // Prior to C# 7, the ExpressionBody properties did not have ExpressionBody on BaseMethodDeclarationSyntax but on
        // some of the derived types only. Therefore we need to special case the derived types here.
        syntax.Kind() switch
        {
            SyntaxKind.MethodDeclaration => ((MethodDeclarationSyntax)syntax).ExpressionBody,
            SyntaxKind.OperatorDeclaration => ((OperatorDeclarationSyntax)syntax).ExpressionBody,
            SyntaxKind.ConversionOperatorDeclaration => ((ConversionOperatorDeclarationSyntax)syntax).ExpressionBody,
            _ => BaseMethodDeclarationSyntaxShimExtensions.get_ExpressionBody(syntax),
        };
}
