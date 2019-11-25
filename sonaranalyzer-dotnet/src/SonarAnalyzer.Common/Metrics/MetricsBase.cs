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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Common
{
    public abstract class MetricsBase
    {
        protected readonly SyntaxTree tree;
        protected const string InitalizationErrorTextPattern = "The input tree is not of the expected language.";

        protected MetricsBase(SyntaxTree tree)
        {
            this.tree = tree;
        }

        #region LinesOfCode

        public int LineCount => this.tree.GetText().Lines.Count;

        public abstract ImmutableArray<int> ExecutableLines { get; }

        public ISet<int> CodeLines
        {
            get
            {
                return this.tree.GetRoot()
                    .DescendantTokens()
                    .Where(token => !IsEndOfFile(token))
                    .SelectMany(
                        t =>
                        {
                            var start = t.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                            var end = t.GetLocation().GetLineSpan().EndLinePosition.Line + 1;
                            return Enumerable.Range(start, end - start + 1);
                        })
                    .ToHashSet();
            }
        }

        protected abstract bool IsEndOfFile(SyntaxToken token);

        #endregion LinesOfCode

        #region Comments

        internal static readonly string[] LineTerminators = { "\r\n", "\n", "\r" };

        public FileComments GetComments(bool ignoreHeaderComments)
        {
            var noSonar = new HashSet<int>();
            var nonBlank = new HashSet<int>();

            var trivias = this.tree.GetRoot().DescendantTrivia();

            foreach (var trivia in trivias)
            {
                if (!IsCommentTrivia(trivia) ||
                    (ignoreHeaderComments && IsNoneToken(trivia.Token.GetPreviousToken())))
                {
                    continue;
                }

                var lineNumber = this.tree
                    .GetLineSpan(trivia.FullSpan)
                    .StartLinePosition
                    .Line + 1;

                var triviaLines = trivia
                    .ToFullString()
                    .Split(LineTerminators, StringSplitOptions.None);

                foreach (var line in triviaLines)
                {
                    CategorizeLines(line, lineNumber, noSonar, nonBlank);

                    lineNumber++;
                }
            }

            return new FileComments(noSonar.ToHashSet(), nonBlank.ToHashSet());
        }

        private static void CategorizeLines(string line, int lineNumber, HashSet<int> noSonar, HashSet<int> nonBlank)
        {
            if (line.Contains("NOSONAR"))
            {
                nonBlank.Remove(lineNumber);
                noSonar.Add(lineNumber);
            }
            else
            {
                if (HasValidCommentContent(line) &&
                    !noSonar.Contains(lineNumber))
                {
                    nonBlank.Add(lineNumber);
                }
            }
        }

        private static bool HasValidCommentContent(string content) => content.Any(char.IsLetter) || content.Any(char.IsDigit);

        protected abstract bool IsNoneToken(SyntaxToken token);

        protected abstract bool IsCommentTrivia(SyntaxTrivia trivia);

        protected abstract bool IsDocumentationCommentTrivia(SyntaxTrivia trivia);

        #endregion Comments

        #region Classes, Accessors, Functions, Statements

        protected abstract bool IsClass(SyntaxNode node);

        protected abstract bool IsStatement(SyntaxNode node);

        protected abstract bool IsFunction(SyntaxNode node);

        public int ClassCount => ClassNodes.Count();

        public IEnumerable<SyntaxNode> ClassNodes => this.tree.GetRoot().DescendantNodes().Where(IsClass);

        public int StatementCount => this.tree.GetRoot().DescendantNodes().Count(IsStatement);

        public int FunctionCount => FunctionNodes.Count();

        public IEnumerable<SyntaxNode> FunctionNodes => this.tree.GetRoot().DescendantNodes().Where(IsFunction);

        #endregion Classes, Accessors, Functions, Statements

        #region Complexity

        public int Complexity => GetCyclomaticComplexity(this.tree.GetRoot());

        public int CognitiveComplexity => GetCognitiveComplexity(this.tree.GetRoot());

        public abstract int GetCyclomaticComplexity(SyntaxNode node);

        public abstract int GetCognitiveComplexity(SyntaxNode node);

        #endregion Complexity
    }
}
