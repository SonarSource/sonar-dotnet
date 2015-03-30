using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

namespace NSonarQubeAnalyzer.Diagnostics.Helpers
{
    public class EquivalenceChecker : IDisposable
    {
        private readonly SemanticModel semanticModel;
        private Workspace workspace;

        public EquivalenceChecker(SemanticModel semanticModel)
        {
            this.semanticModel = semanticModel;
            workspace = new AdhocWorkspace();
        }

        public bool AreEquivalent(SyntaxNode node1, SyntaxNode node2)
        {
            return AreEquivalent(node1, node2, true, true);
        }
        public bool AreEquivalent(SyntaxNode node1, SyntaxNode node2, bool shouldExpandFirst)
        {
            return AreEquivalent(node1, node2, shouldExpandFirst, true);
        }
        public bool AreEquivalent(SyntaxNode node1, SyntaxNode node2, bool shouldExpandFirst, bool shouldExpandSecond)
        {
            var nodeToCompare1 = node1;
            var nodeToCompare2 = node2;

            if (shouldExpandFirst && IsExpandable(node1))
            {
                nodeToCompare1 = Simplifier.Expand(node1, semanticModel, workspace);
            }

            if (shouldExpandSecond && IsExpandable(node2))
            {
                nodeToCompare2 = Simplifier.Expand(node2, semanticModel, workspace);
            }

            var equivalent = SyntaxFactory.AreEquivalent(nodeToCompare1, nodeToCompare2);

            return equivalent;
        }

        private static bool IsExpandable(SyntaxNode node1)
        {
            return node1 is ExpressionSyntax ||
                   node1 is StatementSyntax ||
                   node1 is AttributeSyntax ||
                   node1 is ConstructorInitializerSyntax;
        }

        public bool AreEquivalent(SyntaxList<SyntaxNode> nodeList1, SyntaxList<SyntaxNode> nodeList2)
        {
            return AreEquivalent(nodeList1, nodeList2, true, true);
        }
        public bool AreEquivalent(SyntaxList<SyntaxNode> nodeList1, SyntaxList<SyntaxNode> nodeList2, bool shouldExpandFirst)
        {
            return AreEquivalent(nodeList1, nodeList2, shouldExpandFirst, true);
        }
        public bool AreEquivalent(SyntaxList<SyntaxNode> nodeList1, SyntaxList<SyntaxNode> nodeList2, bool shouldExpandFirst, bool shouldExpandSecond)
        {
            if (nodeList1.Count != nodeList2.Count)
            {
                return false;
            }

            var nodeListToCompare1 = nodeList1;
            if (shouldExpandFirst)
            {
                nodeListToCompare1 = GetExpandedList(nodeList1);
            }

            var nodeListToCompare2 = nodeList2;
            if (shouldExpandSecond)
            {
                nodeListToCompare2 = GetExpandedList(nodeList2);
            }

            for (var i = 0; i < nodeListToCompare1.Count; i++)
            {
                if (!SyntaxFactory.AreEquivalent(nodeListToCompare1[i], nodeListToCompare2[i]))
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

                if (IsExpandable(syntaxNode))
                {
                    simplifiedNode = Simplifier.Expand(syntaxNode, semanticModel, workspace);
                }
                
                newNodeList1 = newNodeList1.Add(simplifiedNode);
            }
            return newNodeList1;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (workspace != null)
            {
                workspace.Dispose();
                workspace = null;
            }
        }
    }
}
