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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class CommentedOutCode : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S125";
        private const string MessageFormat = "Remove this commented out code.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxTreeActionInNonGenerated(
                c =>
                {
                    foreach (var token in c.Tree.GetRoot().DescendantTokens())
                    {
                        CheckTrivias(token.LeadingTrivia, c);
                        CheckTrivias(token.TrailingTrivia, c);
                    }
                });
        }

        private static void CheckTrivias(IEnumerable<SyntaxTrivia> trivias, SyntaxTreeAnalysisContext context)
        {
            var shouldReport = true;
            foreach (var trivia in trivias)
            {
                // comment start is checked because of  https://github.com/dotnet/roslyn/issues/10003

                if (!trivia.ToFullString().TrimStart().StartsWith("/**", StringComparison.Ordinal) &&
                    trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                {
                    CheckMultilineComment(context, trivia);
                    shouldReport = true;
                    continue;
                }

                if (!trivia.ToFullString().TrimStart().StartsWith("///", StringComparison.Ordinal) &&
                    trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) &&
                    shouldReport)
                {
                    var triviaContent = GetTriviaContent(trivia);
                    if (!IsCode(triviaContent))
                    {
                        continue;
                    }

                    context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, trivia.GetLocation()));
                    shouldReport = false;
                }
            }
        }

        private static void CheckMultilineComment(SyntaxTreeAnalysisContext context, SyntaxTrivia comment)
        {
            var triviaContent = GetTriviaContent(comment);
            var triviaLines = triviaContent.Split(MetricsBase.LineTerminators, StringSplitOptions.None);

            for (var triviaLineNumber = 0; triviaLineNumber < triviaLines.Length; triviaLineNumber++)
            {
                if (!IsCode(triviaLines[triviaLineNumber]))
                {
                    continue;
                }

                var triviaStartingLineNumber = comment.GetLocation().GetLineSpan().StartLinePosition.Line;
                var lineNumber = triviaStartingLineNumber + triviaLineNumber;
                var lineSpan = context.Tree.GetText().Lines[lineNumber].Span;
                var commentLineSpan = lineSpan.Intersection(comment.GetLocation().SourceSpan);

                var location = Location.Create(context.Tree, commentLineSpan ?? lineSpan);
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location));
                return;
            }
        }

        private static string GetTriviaContent(SyntaxTrivia trivia)
        {
            var triviaContent = trivia.ToString();
            if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
            {
                if (triviaContent.StartsWith("/*", StringComparison.Ordinal))
                {
                    triviaContent = triviaContent.Substring(2);
                }

                if (triviaContent.EndsWith("*/", StringComparison.Ordinal))
                {
                    triviaContent = triviaContent.Substring(0, triviaContent.Length-2);
                }
                return triviaContent;
            }

            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                if (triviaContent.StartsWith("//", StringComparison.Ordinal))
                {
                    triviaContent = triviaContent.Substring(2);
                }

                return triviaContent;
            }

            return string.Empty;
        }

        private static bool IsCode(string line)
        {
            var checkedLine = line
                .Replace(" ", string.Empty)
                .Replace("\t", string.Empty);

            var isPossiblyCode = EndsWithCode(checkedLine) ||
                    ContainsCodeParts(checkedLine) ||
                    ContainsMultipleLogicalOperators(checkedLine) ||
                    ContainsCodePartsWithRelationalOperator(checkedLine);

            return isPossiblyCode &&
                !checkedLine.Contains("License") &&
                !checkedLine.Contains("c++") &&
                !checkedLine.Contains("C++");
        }

        private static bool ContainsMultipleLogicalOperators(string checkedLine)
        {
            var lineLength = checkedLine.Length;
            var lineLengthWithoutLogicalOperators = checkedLine
                .Replace("&&", string.Empty)
                .Replace("||", string.Empty)
                .Length;

            const int lengthOfOperator = 2;

            return lineLength - lineLengthWithoutLogicalOperators >= 3 * lengthOfOperator;
        }

        private static bool ContainsCodeParts(string checkedLine)
        {
            return CodeParts.Any(checkedLine.Contains);
        }

        private static bool ContainsCodePartsWithRelationalOperator(string checkedLine)
        {
            return CodePartsWithRelationalOperator.Any(codePart =>
            {
                var index = checkedLine.IndexOf(codePart, StringComparison.Ordinal);
                return index >= 0 && RelationalOperators.Any(op => checkedLine.IndexOf(op, index, StringComparison.Ordinal) >= 0);
            });
        }

        private static bool EndsWithCode(string checkedLine)
        {
            return CodeEndings.Any(ending => checkedLine.EndsWith(ending, StringComparison.Ordinal));
        }

        private static readonly string[] CodeEndings = { ";", "{", "}" };
        private static readonly string[] CodeParts = { "++", "catch(", "switch(", "try{", "else{" };
        private static readonly string[] CodePartsWithRelationalOperator = { "for(", "if(", "while(" };
        private static readonly string[] RelationalOperators = { "<", ">", "<=", ">=", "==", "!=" };
    }
}
