using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
            return SyntaxFactory.AreEquivalent(node1, node2);
        }

        public bool AreEquivalent(SyntaxList<SyntaxNode> nodeList1, SyntaxList<SyntaxNode> nodeList2)
        {
            if (nodeList1.Count != nodeList2.Count)
            {
                return false;
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
