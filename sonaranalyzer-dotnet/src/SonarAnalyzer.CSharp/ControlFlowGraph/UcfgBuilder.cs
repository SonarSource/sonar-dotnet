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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Protobuf.Ucfg;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    public class UcfgBuilder
    {
        private readonly UcfgObjectFactory objectFactory;
        private readonly UcfgBlockIdProvider blockIdProvider;
        private readonly UcfgInstructionFactory instructionFactory;

        public UcfgBuilder(SemanticModel semanticModel)
        {
            objectFactory = new UcfgObjectFactory();
            blockIdProvider = new UcfgBlockIdProvider();
            instructionFactory = new UcfgInstructionFactory(semanticModel, objectFactory);
        }

        public UCFG Build(SyntaxNode syntaxNode, IMethodSymbol methodSymbol, IControlFlowGraph cfg)
        {
            var ucfg = objectFactory.CreateUcfg(syntaxNode, UcfgMethod.Create(methodSymbol));

            ucfg.BasicBlocks.AddRange(cfg.Blocks.Select(CreateBasicBlock));

            ucfg.Parameters.AddRange(methodSymbol.GetParameters().Select(p => p.Name));

            if (syntaxNode is BaseMethodDeclarationSyntax methodDeclaration &&
                TaintAnalysisEntryPointDetector.IsEntryPoint(methodSymbol))
            {
                var entryPointBlock = CreateEntryPointBlock(methodDeclaration, methodSymbol, blockIdProvider.Get(cfg.EntryBlock));
                ucfg.BasicBlocks.Add(entryPointBlock);
                ucfg.Entries.Add(entryPointBlock.Id);
            }
            else
            {
                ucfg.Entries.Add(blockIdProvider.Get(cfg.EntryBlock));
            }

            return ucfg;
        }

        private BasicBlock CreateBasicBlock(Block block)
        {
            var ucfgBlock = objectFactory.CreateUcfgBlock(blockIdProvider.Get(block));

            ucfgBlock.Instructions.AddRange(
                block.Instructions.Select(instructionFactory.Create).WhereNotNull());

            if (block is JumpBlock jumpBlock &&
                jumpBlock.JumpNode is ReturnStatementSyntax returnStatement)
            {
                ucfgBlock.Ret = objectFactory.CreateReturnExpression(returnStatement, returnStatement.Expression);
            }

            if (block is ExitBlock exit)
            {
                ucfgBlock.Ret = objectFactory.CreateReturnExpression();
            }

            if (ucfgBlock.TerminatorCase == BasicBlock.TerminatorOneofCase.None)
            {
                // No return was created from JumpBlock or ExitBlock, wire up the successor blocks
                ucfgBlock.Jump = objectFactory.CreateJump(block.SuccessorBlocks.Select(blockIdProvider.Get).ToArray());
            }

            return ucfgBlock;
        }

        private BasicBlock CreateEntryPointBlock(BaseMethodDeclarationSyntax methodDeclaration,
            IMethodSymbol methodSymbol, string currentEntryBlockId)
        {
            var ucfgBlock = objectFactory.CreateUcfgBlock(blockIdProvider.Get(new TemporaryBlock()));

            ucfgBlock.Jump = objectFactory.CreateJump(currentEntryBlockId);

            ucfgBlock.Instructions.Add(instructionFactory.Create(methodDeclaration));

            foreach (var parameter in methodSymbol.Parameters)
            {
                ucfgBlock.Instructions.AddRange(CreateParameterInstructions(parameter));
            }

            return ucfgBlock;
        }

        private IEnumerable<Instruction> CreateParameterInstructions(IParameterSymbol parameter) =>
            parameter.GetAttributes()
                .Where(a => a.AttributeConstructor != null)
                .SelectMany(a =>
                    CreateAttributeInstructions(
                        a.ApplicationSyntaxReference.GetSyntax(),
                        UcfgMethod.Create(a.AttributeConstructor),
                        parameter.Name));

        private IEnumerable<Instruction> CreateAttributeInstructions(SyntaxNode attributeSyntax, UcfgMethod method, string parameterName) =>
            new[]
            {
                objectFactory.CreateMethodCall(attributeSyntax, method),
                objectFactory.CreateParameterAnnotation(parameterName, attributeSyntax),
            };
    }
}
