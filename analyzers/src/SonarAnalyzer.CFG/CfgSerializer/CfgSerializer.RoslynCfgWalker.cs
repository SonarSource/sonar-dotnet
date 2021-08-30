/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using SonarAnalyzer.CFG.Roslyn;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.CFG
{
    public partial class CfgSerializer
    {
        private class RoslynCfgWalker
        {
            private readonly DotWriter writer;
            private readonly HashSet<BasicBlock> visited = new HashSet<BasicBlock>();
            private readonly RoslynCfgIdProvider cfgIdProvider;
            private readonly int cfgId;

            public RoslynCfgWalker(DotWriter writer, RoslynCfgIdProvider cfgIdProvider)
            {
                this.writer = writer;
                this.cfgIdProvider = cfgIdProvider;
                cfgId = cfgIdProvider.Next();
            }

            public void Visit(string title, ControlFlowGraph cfg, bool subgraph)
            {
                writer.WriteGraphStart(title, subgraph);
                //foreach (var region in cfg.Root.NestedRegions)
                //{
                //    Visit(cfg, region);
                //}
                foreach (var block in cfg.Blocks.Where(x => !visited.Contains(x)).ToArray())
                {
                    Visit(block);
                }
                //foreach (var localFunction in cfg.LocalFunctions)
                //{
                //    var localFunctionCfg = cfg.GetLocalFunctionControlFlowGraph(localFunction);
                //    new RoslynCfgWalker(writer, blockPrefixProvider).Visit($"{methodName}.{localFunction.Name}", localFunctionCfg, true);
                //}
                //foreach (var anonymousFunction in GetAnonymousFunctions(cfg))
                //{
                //    var anonymousFunctionCfg = cfg.GetAnonymousFunctionControlFlowGraph(anonymousFunction);
                //    new RoslynCfgWalker(writer, blockPrefixProvider).Visit($"{methodName}.anonymous", anonymousFunctionCfg, true);
                //}
                writer.WriteGraphEnd();
            }

            private void Visit(ControlFlowGraph cfg, ControlFlowRegion region)
            {
                writer.WriteGraphStart(region.Kind + " region" + (region.ExceptionType == null ? null : " " + region.ExceptionType), true);
                foreach (var nested in region.NestedRegions)
                {
                    Visit(cfg, nested);
                }
                foreach (var block in cfg.Blocks.Where(x => x.EnclosingRegion == region))
                {
                    Visit(block);
                }
                writer.WriteGraphEnd();
            }

            private void Visit(BasicBlock block)
            {
                visited.Add(block);
                WriteNode(block);
                WriteEdges(block);
            }

            private void WriteNode(BasicBlock block)
            {
                var header = block.Kind.ToString().ToUpperInvariant() + " #" + block.Ordinal;
                writer.WriteNode(BlockId(block), header, block.Operations.SelectMany(SerializeOperation).Concat(SerializeBranchValue(block.BranchValue)).ToArray());
            }

            private static IEnumerable<string> SerializeBranchValue(IOperation operation) =>
                operation == null ? Enumerable.Empty<string>() : new[] { "## BranchValue ##" }.Concat(SerializeOperation(operation));

            private static IEnumerable<string> SerializeOperation(IOperation operation) =>
                SerializeOperation(0, operation).Concat(new[] { "##########" });

            private static IEnumerable<string> SerializeOperation(int level, IOperation operation) =>
                new[] { $"{level}# {operation.GetType().Name}{OperationSuffix(operation)} / {operation.Syntax.GetType().Name}: {operation.Syntax}" }
                .Concat(new IOperationWrapperSonar(operation).Children.SelectMany(x => SerializeOperation(level + 1, x)));

            private static string OperationSuffix(IOperation op) =>
                IInvocationOperationWrapper.IsInstance(op) ? ": " + IInvocationOperationWrapper.FromOperation(op).TargetMethod.Name : null;

            private void WriteEdges(BasicBlock block)
            {
                foreach (var predecessor in block.Predecessors)
                {
                    var condition = string.Empty;
                    if (predecessor.Source.ConditionKind != ControlFlowConditionKind.None)
                    {
                        condition = predecessor == predecessor.Source.ConditionalSuccessor ? predecessor.Source.ConditionKind.ToString() : "Else";
                    }
                    var semantics = predecessor.Semantics == ControlFlowBranchSemantics.Regular ? null : predecessor.Semantics.ToString();
                    writer.WriteEdge(BlockId(predecessor.Source), BlockId(block), $"{semantics} {condition}".Trim());
                }
                if (block.FallThroughSuccessor != null && block.FallThroughSuccessor.Destination == null)
                {
                    writer.WriteEdge(BlockId(block), "NoDestination_" + BlockId(block), block.FallThroughSuccessor.Semantics.ToString());
                }
            }

            private string BlockId(BasicBlock block) =>
                $"cfg{cfgId}_block{block.Ordinal}";

            //private static IEnumerable<IFlowAnonymousFunctionOperationWrapper> GetAnonymousFunctions(ControlFlowGraph cfg) =>
            //    cfg.Blocks
            //       .SelectMany(block => block.Operations)
            //       .Concat(cfg.Blocks.Select(block => block.BranchValue).Where(op => op != null))
            //       .SelectMany(operation => operation.DescendantsAndSelf())
            //       .OfType<IFlowAnonymousFunctionOperationWrapper>();
        }

        private class RoslynCfgIdProvider
        {
            private int value;

            public int Next() => value++;
        }
    }
}
