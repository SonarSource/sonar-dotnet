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

using SonarAnalyzer.Core.Syntax.Utilities;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.TestFramework.Analyzers;

public sealed class TestGeneratedCodeRecognizer : GeneratedCodeRecognizer
{
    public static TestGeneratedCodeRecognizer Instance { get; } = new();

    protected override bool IsTriviaComment(SyntaxTrivia trivia) =>
        trivia.IsKind(CS.SyntaxKind.SingleLineCommentTrivia)
        || trivia.IsKind(CS.SyntaxKind.MultiLineCommentTrivia)
        || trivia.IsKind(VB.SyntaxKind.CommentTrivia);

    protected override string GetAttributeName(SyntaxNode node)
    {
        if (node.IsKind(CS.SyntaxKind.Attribute))
        {
            return ((CS.Syntax.AttributeSyntax)node).Name.ToString();
        }
        else if (node.IsKind(VB.SyntaxKind.Attribute))
        {
            return ((VB.Syntax.AttributeSyntax)node).Name.ToString();
        }
        else
        {
            return string.Empty;
        }
    }
}
