/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Protobuf.Ucfg;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    internal class UcfgFactory
    {
        private readonly BlockIdProvider blockIdProvider = new BlockIdProvider();
        private readonly UcfgBlockFactory blockBuilder;

        public UcfgFactory(SemanticModel semanticModel)
        {
            this.blockBuilder = new UcfgBlockFactory(semanticModel, blockIdProvider);
        }

        public UCFG Create(SyntaxNode syntaxNode, IMethodSymbol methodSymbol, IControlFlowGraph cfg)
        {
            try
            {
                var ucfg = new UCFG
                {
                    Location = syntaxNode.GetUcfgLocation(),
                    MethodId = methodSymbol.ToUcfgMethodId()
                };

                ucfg.BasicBlocks.AddRange(cfg.Blocks.Select(blockBuilder.CreateBasicBlock));
                ucfg.Parameters.AddRange(methodSymbol.GetParameters().Select(p => p.Name));

                if (syntaxNode is BaseMethodDeclarationSyntax methodDeclaration &&
                    TaintAnalysisEntryPointDetector.IsEntryPoint(methodSymbol))
                {
                    var entryPointBlock = blockBuilder.CreateEntryPointBlock(methodDeclaration, methodSymbol, blockIdProvider.Get(cfg.EntryBlock));
                    ucfg.BasicBlocks.Add(entryPointBlock);
                    ucfg.Entries.Add(entryPointBlock.Id);
                }
                else
                {
                    ucfg.Entries.Add(blockIdProvider.Get(cfg.EntryBlock));
                }

                return ucfg;
            }
            catch (System.Exception e)
            {
                var message = "Exception during creation of UCFG: " +
                    $"Method name: {methodSymbol?.Name ?? "{unknown}"}  " +
                    $"Syntax node kind: {syntaxNode.Kind()} " +
                    $"File: {syntaxNode.GetLocation()?.GetLineSpan().Path ?? "{unknown}"}  " +
                    $"Line: {syntaxNode.GetLocation()?.GetLineSpan().StartLinePosition.ToString() ?? "{unknown}"}  ##  " +
                    // Note: only the first line of the message appears in the Jenkins log so make sure
                    // there are no line breaks.
                    e.ToString().Replace("\r\n", " ## ");

                throw new UcfgException(message);
            }
        }
    }
}
