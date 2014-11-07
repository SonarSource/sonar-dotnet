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
    public class Metrics
    {
        private SyntaxTree tree;

        public Metrics(SyntaxTree tree)
        {
            this.tree = tree;
        }

        public int Lines()
        {
            return tree.GetLineSpan(TextSpan.FromBounds(tree.Length, tree.Length)).EndLinePosition.Line + 1;
        }

        public ISet<int> Comments()
        {
            var builder = ImmutableHashSet.CreateBuilder<int>();
            foreach (SyntaxTrivia trivia in tree.GetRoot().DescendantTrivia())
            {
                if (IsComment(trivia))
                {
                    int startLine = tree.GetLineSpan(trivia.FullSpan).StartLinePosition.Line + 1;
                    int endLine = tree.GetLineSpan(trivia.FullSpan).EndLinePosition.Line + 1;
                    for (int line = startLine; line <= endLine; line++)
                    {
                        builder.Add(line);
                    }
                }
            }
            return builder.ToImmutable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trivia"></param>
        /// <returns></returns>
        private static bool IsComment(SyntaxTrivia trivia)
        {
            return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
        }
    }
}
