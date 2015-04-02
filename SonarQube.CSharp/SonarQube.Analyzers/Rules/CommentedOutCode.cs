using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarQube.Analyzers.Helpers;
using SonarQube.Analyzers.SonarQube.Settings;
using SonarQube.Analyzers.SonarQube.Settings.Sqale;

namespace SonarQube.Analyzers.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [SqaleConstantRemediation("5min")]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.Understandability)]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    public class CommentedOutCode : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CommentedCode";
        internal const string Description = "Comment should not include code";
        internal const string MessageFormat = "Remove this commented out code or move it into XML documentation.";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Minor;
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, RuleSeverity.ToDiagnosticSeverity(), true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(
                c =>
                {
                    foreach (var token in c.Tree.GetRoot().DescendantTokens())
                    {
                        Action<IEnumerable<SyntaxTrivia>> check =
                            trivias =>
                            {
                                var lastCommentedCodeLine = int.MinValue;

                                foreach (var trivia in trivias)
                                {
                                    if (!trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) &&
                                        !trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                                    {
                                        continue;
                                    }

                                    var triviaContent = trivia.ToString().Substring(2);
                                    if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                                    {
                                        triviaContent = triviaContent.Substring(0, triviaContent.Length - 2);
                                    }

                                    var triviaStartingLineNumber = trivia.GetLocation().GetLineSpan().StartLinePosition.Line;
                                    // TODO Do not duplicate line terminators here
                                    var triviaLines = triviaContent.Split(new [] { "\r\n", "\n" }, StringSplitOptions.None);

                                    for (var triviaLineNumber = 0; triviaLineNumber < triviaLines.Length; triviaLineNumber++)
                                    {
                                        if (!IsCode(triviaLines[triviaLineNumber]))
                                        {
                                            continue;
                                        }

                                        var lineNumber = triviaStartingLineNumber + triviaLineNumber;
                                        var previousLastCommentedCodeLine = lastCommentedCodeLine;
                                        lastCommentedCodeLine = lineNumber;

                                        if (lineNumber == previousLastCommentedCodeLine + 1)
                                        {
                                            continue;
                                        }

                                        var location = Location.Create(c.Tree, c.Tree.GetText().Lines[lineNumber].Span);
                                        c.ReportDiagnostic(Diagnostic.Create(Rule, location));
                                        break;
                                    }
                                }
                            };

                        check(token.LeadingTrivia);
                        check(token.TrailingTrivia);
                    }
                });
        }

        private static bool IsCode(string line)
        {
            var checkedLine = line.Replace(" ", "").Replace("\t", "");

            return
                CodeEndings.Any(ending => checkedLine.EndsWith(ending, StringComparison.Ordinal)) ||
                CodeParts.Any(part => checkedLine.Contains(part)) ||
                (checkedLine.Length - checkedLine.Replace("&&", "").Replace("||", "").Length)/2 >= 3;
        }

        private static IEnumerable<string> CodeEndings
        {
            get { return new[] {";", "{", "}"}; }
        }

        private static IEnumerable<string> CodeParts
        {
            get { return new[] { "++", "for(", "if(", "while(", "catch(", "switch(", "try(", "else(" }; }
        }
    }
}
