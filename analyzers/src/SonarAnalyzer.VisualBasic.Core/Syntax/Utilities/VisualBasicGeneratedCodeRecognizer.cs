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

namespace SonarAnalyzer.VisualBasic.Core.Syntax.Utilities;

public sealed class VisualBasicGeneratedCodeRecognizer : GeneratedCodeRecognizer
{
    #region Singleton implementation

    private VisualBasicGeneratedCodeRecognizer()
    {
    }

    private static readonly Lazy<VisualBasicGeneratedCodeRecognizer> Lazy = new Lazy<VisualBasicGeneratedCodeRecognizer>(() => new VisualBasicGeneratedCodeRecognizer());
    public static VisualBasicGeneratedCodeRecognizer Instance => Lazy.Value;

    #endregion Singleton implementation

    protected override bool IsTriviaComment(SyntaxTrivia trivia) =>
        trivia.IsKind(SyntaxKind.CommentTrivia);

    protected override string GetAttributeName(SyntaxNode node) =>
        node.IsKind(SyntaxKind.Attribute)
            ? ((AttributeSyntax)node).Name.ToString()
            : string.Empty;
}
