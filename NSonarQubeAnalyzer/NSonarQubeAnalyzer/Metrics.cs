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
        public readonly IImmutableSet<int> NoSonar;
        public readonly IImmutableSet<int> NonBlank;

        public FileComments(IImmutableSet<int> noSonar, IImmutableSet<int> nonBlank)
        {
            this.NoSonar = noSonar;
            this.NonBlank = nonBlank;
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
            var nonBlank = ImmutableHashSet.CreateBuilder<int>();

            foreach (SyntaxTrivia trivia in tree.GetRoot().DescendantTrivia())
            {
                if (!(ignoreHeaderComments && trivia.Token.GetPreviousToken().IsKind(SyntaxKind.None)) && IsComment(trivia))
                {
                    int lineNumber = tree.GetLineSpan(trivia.FullSpan).StartLinePosition.Line + 1;

                    foreach (string line in trivia.ToFullString().Split(LINE_TERMINATORS, StringSplitOptions.None))
                    {
                        if (line.Contains("NOSONAR"))
                        {
                            nonBlank.Remove(lineNumber);
                            noSonar.Add(lineNumber);
                        }
                        else if (line.Any(char.IsLetter) && !noSonar.Contains(lineNumber))
                        {
                            nonBlank.Add(lineNumber);
                        }

                        lineNumber++;
                    }
                }
            }

            return new FileComments(noSonar.ToImmutableHashSet(), nonBlank.ToImmutableHashSet());
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
