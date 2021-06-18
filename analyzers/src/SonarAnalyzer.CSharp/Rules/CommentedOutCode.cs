/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
        private const string DiagnosticId = "S125";
        private const string MessageFormat = "Remove this commented out code.";
        private const int CommentMarkLength = 2;

        private static readonly string[] CodeEndings = { ";", "{", ";}", "{}" };
        private static readonly string[] CodeParts = { "++", "catch(", "switch(", "try{", "else{" };
        private static readonly string[] CodePartsWithRelationalOperator = { "for(", "if(", "while(" };
        private static readonly string[] RelationalOperators = { "<", ">", "<=", ">=", "==", "!=" };

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxTreeActionInNonGenerated(c =>
                {
                    foreach (var token in c.Tree.GetRoot().DescendantTokens())
                    {
                        CheckTrivias(c, token.LeadingTrivia);
                        CheckTrivias(c, token.TrailingTrivia);
                    }
                });

        private static void CheckTrivias(SyntaxTreeAnalysisContext context, IEnumerable<SyntaxTrivia> trivias)
        {
            var shouldReport = true;
            foreach (var trivia in trivias)
            {
                // comment start is checked because of  https://github.com/dotnet/roslyn/issues/10003
                if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) && !trivia.ToFullString().TrimStart().StartsWith("/**", StringComparison.Ordinal))
                {
                    CheckMultilineComment(context, trivia);
                    shouldReport = true;
                }
                else if (shouldReport
                    && trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                    && !trivia.ToFullString().TrimStart().StartsWith("///", StringComparison.Ordinal)
                    && IsCode(trivia.ToString().Substring(CommentMarkLength)))
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, trivia.GetLocation()));
                    shouldReport = false;
                }
            }
        }

        private static void CheckMultilineComment(SyntaxTreeAnalysisContext context, SyntaxTrivia trivia)
        {
            var triviaLines = TriviaContent().Split(MetricsBase.LineTerminators, StringSplitOptions.None);

            for (var triviaLineNumber = 0; triviaLineNumber < triviaLines.Length; triviaLineNumber++)
            {
                if (IsCode(triviaLines[triviaLineNumber]))
                {
                    var triviaStartingLineNumber = trivia.GetLocation().GetLineSpan().StartLinePosition.Line;
                    var lineNumber = triviaStartingLineNumber + triviaLineNumber;
                    var lineSpan = context.Tree.GetText().Lines[lineNumber].Span;
                    var commentLineSpan = lineSpan.Intersection(trivia.GetLocation().SourceSpan);
                    var location = Location.Create(context.Tree, commentLineSpan ?? lineSpan);
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, location));
                    return;
                }
            }

            string TriviaContent()
            {
                var ret = trivia.ToString();
                if (ret.StartsWith("/*", StringComparison.Ordinal))
                {
                    ret = ret.Substring(CommentMarkLength);
                }
                if (ret.EndsWith("*/", StringComparison.Ordinal))
                {
                    ret = ret.Substring(0, ret.Length - CommentMarkLength);
                }
                return ret;
            }
        }

        private static bool IsCode(string line)
        {
            var checkedLine = line.Replace(" ", string.Empty).Replace("\t", string.Empty);

            var isPossiblyCode = EndsWithCode(checkedLine)
                || ContainsCodeParts(checkedLine)
                || ContainsMultipleLogicalOperators(checkedLine)
                || ContainsCodePartsWithRelationalOperator(checkedLine);

            return isPossiblyCode
                && !checkedLine.Contains("License")
                && !checkedLine.Contains("c++")
                && !checkedLine.Contains("C++");
        }

        private static bool ContainsMultipleLogicalOperators(string checkedLine)
        {
            const int lengthOfOperator = 2;
            const int operatorCountLimit = 3;
            var lineLengthWithoutLogicalOperators = checkedLine.Replace("&&", string.Empty).Replace("||", string.Empty).Length;

            return checkedLine.Length - lineLengthWithoutLogicalOperators >= operatorCountLimit * lengthOfOperator;
        }

        private static bool ContainsCodeParts(string checkedLine) =>
            CodeParts.Any(checkedLine.Contains);

        private static bool ContainsCodePartsWithRelationalOperator(string checkedLine)
        {
            return CodePartsWithRelationalOperator.Any(ContainsRelationalOperator);

            bool ContainsRelationalOperator(string codePart)
            {
                var index = checkedLine.IndexOf(codePart, StringComparison.Ordinal);
                return index >= 0 && RelationalOperators.Any(op => checkedLine.IndexOf(op, index, StringComparison.Ordinal) >= 0);
            }
        }

        private static bool EndsWithCode(string checkedLine) =>
            checkedLine == "}" || CodeEndings.Any(ending => checkedLine.EndsWith(ending, StringComparison.Ordinal));
    }
}
