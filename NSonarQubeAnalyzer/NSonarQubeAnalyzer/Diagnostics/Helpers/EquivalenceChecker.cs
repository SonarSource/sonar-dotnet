using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Simplification;

namespace NSonarQubeAnalyzer.Diagnostics.Helpers
{
    public static class EquivalenceChecker
    {
        public static bool AreEquivalent(SyntaxNode node1, SyntaxNode node2, SemanticModel semanticModel, bool shouldExpandFirst = true, bool shouldExpandSecond = true)
        {
            var equivalent = false;
            
            if ((shouldExpandFirst || shouldExpandSecond) && semanticModel == null)
            {
                throw new ArgumentNullException("semanticModel");
            }

            try
            {
                if (shouldExpandFirst)
                {
                    node1 = Simplifier.Expand(node1, semanticModel, new AdhocWorkspace());
                }

                if (shouldExpandSecond)
                {
                    node2 = Simplifier.Expand(node2, semanticModel, new AdhocWorkspace());
                }
            }
            catch
            {
                //couldn't expand node
            }

            equivalent = SyntaxFactory.AreEquivalent(node1, node2);

            return equivalent;
        }

        public static bool AreEquivalent(SyntaxList<SyntaxNode> nodeList1, SyntaxList<SyntaxNode> nodeList2, SemanticModel semanticModel, bool shouldExpandFirst = true, bool shouldExpandSecond = true)
        {
            if ((shouldExpandFirst || shouldExpandSecond) && semanticModel == null)
            {
                throw new ArgumentNullException("semanticModel");
            }

            if (nodeList1.Count != nodeList2.Count)
            {
                return false;
            }

            if (shouldExpandFirst)
            {
                nodeList1 = GetExpandedList(nodeList1, semanticModel);
            }

            if (shouldExpandSecond)
            {
                nodeList2 = GetExpandedList(nodeList2, semanticModel);
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

        public static SyntaxList<SyntaxNode> GetExpandedList(SyntaxList<SyntaxNode> nodeList1, SemanticModel semanticModel)
        {
            var newNodeList1 = new SyntaxList<SyntaxNode>();
            var workspace = new AdhocWorkspace();
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
