/*
 * Copyright (C) 2018-2019 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    public static class CSharpControlFlowGraph
    {
        public static IControlFlowGraph Create(CSharpSyntaxNode node, SemanticModel semanticModel)
        {
            return new CSharpControlFlowGraphBuilder(node, semanticModel).Build();
        }

        public static bool TryGet(CSharpSyntaxNode node, SemanticModel semanticModel, out IControlFlowGraph cfg)
        {
            cfg = null;
            try
            {
                if (node != null)
                {
                    cfg = Create(node, semanticModel);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exc) when (exc is InvalidOperationException ||
                                        exc is ArgumentException ||
                                        exc is NotSupportedException)
            {
                // These are expected
            }
            catch (Exception exc) when (exc is NotImplementedException)
            {
                Debug.Fail(exc.ToString());
            }

            return cfg != null;
        }
    }
}
