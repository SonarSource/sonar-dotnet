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

using System.IO;
using System.Linq;
using System.Text;
using SonarAnalyzer.Protobuf.Ucfg;

namespace SonarAnalyzer.Helpers
{
    public class UcfgSerializer
    {
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

                writer.WriteNode("ENTRY", "ENTRY", ucfg.Parameters.ToArray());

                foreach (var entry in ucfg.Entries)
                {
                    writer.WriteEdge("ENTRY", entry, string.Empty);
                }

                foreach (var block in ucfg.BasicBlocks)
                {
                    Visit(block);
                }

                writer.WriteNode("EXIT", "EXIT");

                writer.WriteGraphEnd();
            }

            private void Visit(BasicBlock block)
            {
                writer.WriteNode(block.Id, "BLOCK", block.Instructions.Select(SerializeInstruction).ToArray());
                if (block.TerminatorCase == BasicBlock.TerminatorOneofCase.Jump)
                {
                    foreach (var destination in block.Jump.Destinations)
                    {
                        writer.WriteEdge(block.Id, destination, string.Empty);
                    }
                }
                else
                {
                    writer.WriteEdge(block.Id, "EXIT", string.Empty);
                }
            }

            private string SerializeInstruction(Instruction instruction) =>
                $"{instruction.Variable} {instruction.MethodId} {string.Join(",", instruction.Args.Select(SerializeExpression))}";

            private string SerializeExpression(Expression expression) =>
                expression.ExprCase == Expression.ExprOneofCase.Const
                    ? "CONST"
                    : expression.Var.Name;
        }
    }
}
