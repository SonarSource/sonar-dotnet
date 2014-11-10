using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace NSonarQubeAnalyzer
{
    public class FileComments
    {
        public readonly ISet<int> NoSonar;
        public readonly ISet<int> NonBlanks;

        public FileComments(ISet<int> noSonar, ISet<int> nonBlanks)
        {
            this.NoSonar = noSonar;
            this.NonBlanks = nonBlanks;
        }
    }

    public class Metrics
    {
        private readonly string[] LINE_TERMINATORS = { "\r\n", "\n", "\r" };
        private SyntaxTree tree;

        public Metrics(SyntaxTree tree)
        {
            this.tree = tree;
        }

        public int Lines()
        {
            return tree.GetLineSpan(TextSpan.FromBounds(tree.Length, tree.Length)).EndLinePosition.Line + 1;
        }

        public FileComments Comments(bool ignoreHeaderComments)
        {
            var noSonar = ImmutableHashSet.CreateBuilder<int>();
            var nonBlanks = ImmutableHashSet.CreateBuilder<int>();

            foreach (SyntaxTrivia trivia in tree.GetRoot().DescendantTrivia())
            {
                if (!(ignoreHeaderComments && trivia.Token.GetPreviousToken().IsKind(SyntaxKind.None)) && IsComment(trivia))
                {
                    int lineNumber = tree.GetLineSpan(trivia.FullSpan).StartLinePosition.Line + 1;

                    foreach (string line in trivia.ToFullString().Split(LINE_TERMINATORS, StringSplitOptions.None))
                    {
                        if (line.Contains("NOSONAR"))
                        {
                            nonBlanks.Remove(lineNumber);
                            noSonar.Add(lineNumber);
                        }
                        else if (line.Any(char.IsLetter) && !noSonar.Contains(lineNumber))
                        {
                            nonBlanks.Add(lineNumber);
                        }

                        lineNumber++;
                    }
                }
            }

            return new FileComments(noSonar.ToImmutableHashSet(), nonBlanks.ToImmutableHashSet());
        }

        private static bool IsComment(SyntaxTrivia trivia)
        {
            return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
        }
    }
}
