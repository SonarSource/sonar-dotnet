/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.CSharp.Core.Common;

public static class SyntaxConstants
{
    public const string Discard = "_";
    public const string Private = "private";
    public const string Protected = "protected";
    public const string Internal = "internal";
    public const string NameOfKeywordText = "nameof";

    public static readonly ExpressionSyntax NullLiteralExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
    public static readonly ExpressionSyntax FalseLiteralExpression = SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
    public static readonly ExpressionSyntax TrueLiteralExpression = SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
}
