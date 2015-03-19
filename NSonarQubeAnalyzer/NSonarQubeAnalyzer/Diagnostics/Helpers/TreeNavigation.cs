using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace NSonarQubeAnalyzer.Diagnostics.Helpers
{
    public static class TreeNavigation
    {
        public static IEnumerable<SyntaxNode> NextSiblings(this SyntaxNode self)
        {
            if (self.Parent == null)
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
    }
}
