using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

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
    }
}
