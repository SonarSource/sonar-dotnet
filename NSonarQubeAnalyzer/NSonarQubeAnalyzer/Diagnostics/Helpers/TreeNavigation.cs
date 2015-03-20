using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace NSonarQubeAnalyzer.Diagnostics.Helpers
{
    public static class TreeNavigation
    {
        public static IEnumerable<SyntaxNode> NextSiblings(this SyntaxNode self)
        {
            if (self == null || self.Parent == null)
            {
                yield break; 
            }

            var siblingsAndSelf = self.Parent.ChildNodes().ToList();
            var childIndex = siblingsAndSelf.IndexOf(self);
            var childCount = siblingsAndSelf.Count;
            
            for (var i = childIndex + 1; i < childCount; i++)
            {
                yield return siblingsAndSelf[i];
            }
        }

        public static IEnumerable<SyntaxNode> FollowingSyntaxNodes(this SyntaxNode self)
        {
            var currentSyntaxNode = self;

            while (currentSyntaxNode != null)
            {
                var lastToken = currentSyntaxNode.GetLastToken();
                var nextToken = lastToken.GetNextToken();

                var nextSyntaxNode = nextToken.Parent;
                yield return nextSyntaxNode;

                currentSyntaxNode = nextSyntaxNode;
            }
        }
    }
}
