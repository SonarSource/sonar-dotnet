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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Protobuf.Ucfg;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    internal class UcfgBlockFactory
    {
        private readonly UcfgInstructionFactory instructionFactory;
        private readonly BlockIdProvider blockIdProvider;
        private readonly UcfgExpressionService expressionService;

        public UcfgBlockFactory(SemanticModel semanticModel, BlockIdProvider blockIdProvider)
        {
            this.blockIdProvider = blockIdProvider;
            this.expressionService = new UcfgExpressionService();
            this.instructionFactory = new UcfgInstructionFactory(semanticModel, expressionService);
        }

        public BasicBlock CreateBasicBlock(Block block)
        {
            var ucfgBlock = CreateBlockWithId(blockIdProvider.Get(block));

            ucfgBlock.Instructions.AddRange(block.Instructions.SelectMany(instructionFactory.CreateFrom));

            if (block is JumpBlock jumpBlock &&
                jumpBlock.JumpNode is ReturnStatementSyntax returnStatement)
            {
                ucfgBlock.Ret = new Return
                {
                    Location = returnStatement.GetUcfgLocation(),
                    ReturnedExpression = returnStatement.Expression != null
                        ? expressionService.GetExpression(returnStatement.Expression).Expression
                        : expressionService.CreateConstant().Expression
                };
            }

            if (block is ExitBlock exit)
            {
                ucfgBlock.Ret = new Return
                {
                    Location = null,
                    ReturnedExpression = expressionService.CreateConstant().Expression
                };
            }

            if (ucfgBlock.TerminatorCase == BasicBlock.TerminatorOneofCase.None)
            {
                // No return was created from JumpBlock or ExitBlock, wire up the successor blocks
                ucfgBlock.Jump = CreateJump(block.SuccessorBlocks.Select(blockIdProvider.Get).ToArray());
            }

            return ucfgBlock;
        }

        public BasicBlock CreateEntryPointBlock(BaseMethodDeclarationSyntax methodDeclaration,
            IMethodSymbol methodSymbol, string currentEntryBlockId)
        {
            var ucfgBlock = CreateBlockWithId(blockIdProvider.Get(new TemporaryBlock()));

            ucfgBlock.Jump = CreateJump(currentEntryBlockId);

            ucfgBlock.Instructions.Add(instructionFactory.CreateFrom(methodDeclaration));

            foreach (var parameter in methodSymbol.Parameters)
            {
                var parameterInstructions = parameter.GetAttributes()
                    .Where(a => a.AttributeConstructor != null)
                    .SelectMany(a => instructionFactory.CreateFromAttributeSyntax(
                        (AttributeSyntax)a.ApplicationSyntaxReference.GetSyntax(),
                        a.AttributeConstructor,
                        parameter.Name));
                ucfgBlock.Instructions.AddRange(parameterInstructions);
            }

            return ucfgBlock;
        }

        private BasicBlock CreateBlockWithId(string id) =>
            new BasicBlock { Id = id };

        private Jump CreateJump(params string[] blockIds)
        {
            var jump = new Jump();
            jump.Destinations.AddRange(blockIds);
            return jump;
        }
    }
}
