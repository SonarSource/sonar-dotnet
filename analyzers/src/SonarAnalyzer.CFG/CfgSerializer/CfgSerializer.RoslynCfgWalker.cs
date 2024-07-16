/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using System.Text;
using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.CFG;

public static partial class CfgSerializer
{
    private class RoslynCfgWalker
    {
        private readonly DotWriter writer;
        private readonly HashSet<BasicBlock> visited = new();
        private readonly RoslynCfgIdProvider cfgIdProvider;
        private readonly int cfgId;

        public RoslynCfgWalker(DotWriter writer, RoslynCfgIdProvider cfgIdProvider)
        {
            this.writer = writer;
            this.cfgIdProvider = cfgIdProvider;
            cfgId = cfgIdProvider.Next();
        }

        public void Visit(ControlFlowGraph cfg, string title)
        {
            writer.WriteGraphStart(title);
            VisitContent(cfg, title);
            writer.WriteGraphEnd();
        }

        private void VisitSubGraph(ControlFlowGraph cfg, string title)
        {
            writer.WriteSubGraphStart(cfgIdProvider.Next(), title);
            VisitContent(cfg, title);
            writer.WriteSubGraphEnd();
        }

        private void VisitContent(ControlFlowGraph cfg, string titlePrefix)
        {
            foreach (var region in cfg.Root.NestedRegions)
            {
                Visit(cfg, region);
            }
            foreach (var block in cfg.Blocks.Where(x => !visited.Contains(x)).ToArray())
            {
                Visit(block);
            }
            foreach (var localFunction in cfg.LocalFunctions)
            {
                var localFunctionCfg = cfg.GetLocalFunctionControlFlowGraph(localFunction, default);
                new RoslynCfgWalker(writer, cfgIdProvider).VisitSubGraph(localFunctionCfg, $"{titlePrefix}.{localFunction.Name}");
            }
            foreach (var anonymousFunction in AnonymousFunctions(cfg))
            {
                var anonymousFunctionCfg = cfg.GetAnonymousFunctionControlFlowGraph(anonymousFunction, default);
                new RoslynCfgWalker(writer, cfgIdProvider).VisitSubGraph(anonymousFunctionCfg, $"{titlePrefix}.anonymous");
            }
        }

        private void Visit(ControlFlowGraph cfg, ControlFlowRegion region)
        {
            writer.WriteSubGraphStart(cfgIdProvider.Next(), SerializeRegion(region));
            foreach (var nested in region.NestedRegions)
            {
                Visit(cfg, nested);
            }
            foreach (var block in cfg.Blocks.Where(x => x.EnclosingRegion == region))
            {
                Visit(block);
            }
            writer.WriteSubGraphEnd();
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
            operation is null ? [] : new[] { "## BranchValue ##" }.Concat(SerializeOperation(operation));

        private static IEnumerable<string> SerializeOperation(IOperation operation) =>
            SerializeOperation(0, null, operation).Concat(new[] { "##########" });

        private static IEnumerable<string> SerializeOperation(int level, string prefix, IOperation operation)
        {
            var prefixes = operation.GetType().GetProperties()
                .GroupBy(x => x.GetValue(operation) as IOperation, x => x.Name)
                .Where(x => x.Key is not null)
                .ToDictionary(x => x.Key, x => string.Join(", ", x));
            var ret = new List<string> { $"{level}#: {prefix}{operation.Serialize()}" };
            foreach (var child in operation.ToSonar().Children)
            {
                ret.AddRange(SerializeOperation(level + 1, prefixes.TryGetValue(child, out var childPrefix) ? $"{level}#.{childPrefix}: " : null, child));
            }
            return ret;
        }

        private static string SerializeRegion(ControlFlowRegion region)
        {
            var sb = new StringBuilder();
            sb.Append(region.Kind.ToString()).Append(" region");
            if (region.ExceptionType is not null)
            {
                sb.Append(": ").Append(region.ExceptionType);
            }
            if (region.Locals.Any())
            {
                sb.Append(", Locals: ").Append(string.Join(", ", region.Locals.Select(x => x.Name ?? "N/A")));
            }
            if (region.CaptureIds.Any())
            {
                sb.Append(", Captures: ").Append(string.Join(", ", region.CaptureIds.Select(x => x.Serialize())));  // Same as IOperationExtension.SerializeSuffix
            }
            return sb.ToString();
        }

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
            if (block.FallThroughSuccessor is { Destination: null })
            {
                writer.WriteEdge(BlockId(block), "NoDestination_" + BlockId(block), block.FallThroughSuccessor.Semantics.ToString());
            }
        }

        private string BlockId(BasicBlock block) =>
            $"cfg{cfgId}_block{block.Ordinal}";

        private static IEnumerable<IFlowAnonymousFunctionOperationWrapper> AnonymousFunctions(ControlFlowGraph cfg) =>
            cfg.Blocks
                .SelectMany(x => x.Operations)
                .Concat(cfg.Blocks.Select(x => x.BranchValue).Where(x => x is not null))
                .SelectMany(x => x.DescendantsAndSelf())
                .Where(IFlowAnonymousFunctionOperationWrapper.IsInstance)
                .Select(IFlowAnonymousFunctionOperationWrapper.FromOperation);
    }

    private sealed class RoslynCfgIdProvider
    {
        private int value;

        public int Next() => value++;
    }
}
