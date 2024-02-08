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

using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.TestFramework.Analyzers;

internal sealed class TestGeneratedCodeRecognizer : GeneratedCodeRecognizer
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
