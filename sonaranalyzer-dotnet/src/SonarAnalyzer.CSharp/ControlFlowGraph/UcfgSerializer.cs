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

using System;
using System.IO;
using System.Linq;
using System.Text;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Protobuf.Ucfg;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    internal static class UcfgSerializer
    {
        private const string EntryBlockId = "ENTRY";
        private const string ExitBlockId = "END";

        public static string Serialize(UCFG ucfg)
        {
            var stringBuilder = new StringBuilder();
            using (var writer = new StringWriter(stringBuilder))
            {
                Serialize(ucfg, writer);
            }
            return stringBuilder.ToString();
        }

        public static void Serialize(UCFG ucfg, TextWriter writer)
        {
            new UcfgWalker(new DotWriter(writer)).Visit(ucfg.MethodId, ucfg);
        }

        private class UcfgWalker
        {
            private readonly DotWriter writer;

            public UcfgWalker(DotWriter writer)
            {
                this.writer = writer;
            }

            public void Visit(string methodName, UCFG ucfg)
            {
                writer.WriteGraphStart(methodName);

                writer.WriteNode(EntryBlockId, EntryBlockId, ucfg.Parameters.ToArray());

                foreach (var entry in ucfg.Entries)
                {
                    writer.WriteEdge(EntryBlockId, entry, string.Empty);
                }

                foreach (var block in ucfg.BasicBlocks)
                {
                    Visit(block);
                }

                writer.WriteNode(ExitBlockId, ExitBlockId);

                writer.WriteGraphEnd();
            }

            private void Visit(BasicBlock block)
            {
                var instructions = block.Instructions.Select(SerializeInstruction);

                var terminator = block.TerminatorCase == BasicBlock.TerminatorOneofCase.Jump
                    ? $"JUMP: {string.Join(", ", block.Jump.Destinations)}"
                    : $"RET: {SerializeExpression(block.Ret.ReturnedExpression)}";

                var jumps = new[] { $"TERMINATOR {terminator}" };

                writer.WriteNode(block.Id, $"BLOCK:{block.Id}", instructions.Union(jumps).ToArray());
                if (block.TerminatorCase == BasicBlock.TerminatorOneofCase.Jump)
                {
                    foreach (var destination in block.Jump.Destinations)
                    {
                        writer.WriteEdge(block.Id, destination, string.Empty);
                    }
                }
                else
                {
                    writer.WriteEdge(block.Id, ExitBlockId, string.Empty);
                }
            }

            private string SerializeInstruction(Instruction instruction)
            {
                switch (instruction.InstrCase)
                {
                    case Instruction.InstrOneofCase.Assigncall:
                        return $"{instruction.Assigncall.Variable} {instruction.Assigncall.MethodId} " +
                            $"{string.Join(",", instruction.Assigncall.Args.Select(SerializeExpression))}";

                    case Instruction.InstrOneofCase.NewObject:
                        return $"{instruction.NewObject.Variable} {instruction.NewObject.Type}";

                    default:
                        throw new ArgumentOutOfRangeException(nameof(instruction));
                }
            }

            private string SerializeExpression(Expression expression) =>
                expression.ExprCase == Expression.ExprOneofCase.Const
                    ? "CONST"
                    : expression.Var.Name;
        }
    }
}
