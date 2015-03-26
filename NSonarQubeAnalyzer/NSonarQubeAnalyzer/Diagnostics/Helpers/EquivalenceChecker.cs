using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Simplification;

namespace NSonarQubeAnalyzer.Diagnostics.Helpers
{
    public class EquivalenceChecker
    {
        private readonly SemanticModel semanticModel;
        private readonly Workspace workspace;

        public EquivalenceChecker(SemanticModel semanticModel)
        {
            this.semanticModel = semanticModel;
            workspace = new AdhocWorkspace();
        }

        public bool AreEquivalent(SyntaxNode node1, SyntaxNode node2, bool shouldExpandFirst = true, bool shouldExpandSecond = true)
        {
            try
            {
                if (shouldExpandFirst)
                {
                    node1 = Simplifier.Expand(node1, semanticModel, workspace);
                }

                if (shouldExpandSecond)
                {
                    node2 = Simplifier.Expand(node2, semanticModel, workspace);
                }
            }
            catch
            {
                //couldn't expand node
            }

            var equivalent = SyntaxFactory.AreEquivalent(node1, node2);

            return equivalent;
        }

        public bool AreEquivalent(SyntaxList<SyntaxNode> nodeList1, SyntaxList<SyntaxNode> nodeList2, bool shouldExpandFirst = true, bool shouldExpandSecond = true)
        {
            if (nodeList1.Count != nodeList2.Count)
            {
                return false;
            }

            if (shouldExpandFirst)
            {
                nodeList1 = GetExpandedList(nodeList1);
            }

            if (shouldExpandSecond)
            {
                nodeList2 = GetExpandedList(nodeList2);
            }

            for (var i = 0; i < nodeList1.Count; i++)
            {
                if (!SyntaxFactory.AreEquivalent(nodeList1[i], nodeList2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public SyntaxList<SyntaxNode> GetExpandedList(SyntaxList<SyntaxNode> nodeList1)
        {
            var newNodeList1 = new SyntaxList<SyntaxNode>();
            foreach (var syntaxNode in nodeList1)
            {
                var simplifiedNode = syntaxNode;
                try
                {
                    simplifiedNode = Simplifier.Expand(syntaxNode, semanticModel, workspace);
                }
                catch
                {
                    //couldn't expand node
                }
                
                newNodeList1 = newNodeList1.Add(simplifiedNode);
            }
            return newNodeList1;
        }
    }
}
