/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.Helpers.VisualBasic
{
    public sealed class VisualBasicGeneratedCodeRecognizer : Helpers.GeneratedCodeRecognizer
    {
        #region Singleton implementation

        private VisualBasicGeneratedCodeRecognizer()
        {
        }

        private static readonly Lazy<VisualBasicGeneratedCodeRecognizer> lazy = new Lazy<VisualBasicGeneratedCodeRecognizer>(() => new VisualBasicGeneratedCodeRecognizer());
        public static VisualBasicGeneratedCodeRecognizer Instance => lazy.Value;

        #endregion Singleton implementation

        protected override bool IsTriviaComment(SyntaxTrivia trivia) =>
            trivia.IsKind(SyntaxKind.CommentTrivia);

        protected override string GetAttributeName(SyntaxNode node) =>
            node.IsKind(SyntaxKind.Attribute)
                ? ((AttributeSyntax)node).Name.ToString()
                : string.Empty;
    }
}
