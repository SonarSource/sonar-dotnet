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
                this.writer.WriteGraphStart(methodName);

                this.writer.WriteNode(EntryBlockId, EntryBlockId, ucfg.Parameters.ToArray());

                foreach (var entry in ucfg.Entries)
                {
                    this.writer.WriteEdge(EntryBlockId, entry, string.Empty);
                }

                foreach (var block in ucfg.BasicBlocks)
                {
                    Visit(block);
                }

                this.writer.WriteNode(ExitBlockId, ExitBlockId);

                this.writer.WriteGraphEnd();
            }

            private void Visit(BasicBlock block)
            {
                var instructions = block.Instructions.Select(SerializeInstruction);

                var terminator = block.TerminatorCase == BasicBlock.TerminatorOneofCase.Jump
                    ? $"JUMP: #{string.Join(", #", block.Jump.Destinations)}"
                    : $"RET: {Serialize(block.Ret.ReturnedExpression)}";

                var jumps = new[] { $"TERMINATOR: {terminator}" };

                this.writer.WriteNode(block.Id, $"BLOCK:#{block.Id}", instructions.Union(jumps).ToArray());
                if (block.TerminatorCase == BasicBlock.TerminatorOneofCase.Jump)
                {
                    foreach (var destination in block.Jump.Destinations)
                    {
                        this.writer.WriteEdge(block.Id, destination, string.Empty);
                    }
                }
                else
                {
                    this.writer.WriteEdge(block.Id, ExitBlockId, string.Empty);
                }
            }

            private string SerializeInstruction(Instruction instruction)
            {
                switch (instruction.InstrCase)
                {
                    case Instruction.InstrOneofCase.Assigncall:
                        return Serialize(instruction.Assigncall);

                    case Instruction.InstrOneofCase.NewObject:
                        return Serialize(instruction.NewObject);

                    default:
                        throw new ArgumentOutOfRangeException(nameof(instruction));
                }
            }

            private string Serialize(NewObject newObject)
            {
                string target;
                switch (newObject.TargetCase)
                {
                    case NewObject.TargetOneofCase.Variable:
                        target = Serialize(newObject.Variable);
                        break;
                    case NewObject.TargetOneofCase.FieldAccess:
                        target = Serialize(newObject.FieldAccess);
                        break;
                    default:
                        throw new NotSupportedException(newObject.TargetCase.ToString());
                }

                return $"{target} := new {newObject.Type}";
            }

            private string Serialize(AssignCall assignCall)
            {
                string target;
                switch (assignCall.TargetCase)
                {
                    case AssignCall.TargetOneofCase.Variable:
                        target = Serialize(assignCall.Variable);
                        break;
                    case AssignCall.TargetOneofCase.FieldAccess:
                        target = Serialize(assignCall.FieldAccess);
                        break;
                    default:
                        throw new NotSupportedException(assignCall.TargetCase.ToString());
                }

                var arguments = string.Join(", ", assignCall.Args.Select(Serialize));

                return $"{target} := {assignCall.MethodId} [ {arguments} ]";
            }

            private string Serialize(FieldAccess fieldAccess)
            {
                switch (fieldAccess.ExprObjCase)
                {
                    case FieldAccess.ExprObjOneofCase.Object:
                        return $"{Serialize(fieldAccess.Object)}.{Serialize(fieldAccess.Field)}";
                    case FieldAccess.ExprObjOneofCase.This:
                        return $"{Serialize(fieldAccess.This)}.{Serialize(fieldAccess.Field)}";
                    case FieldAccess.ExprObjOneofCase.Classname:
                        return $"{Serialize(fieldAccess.Classname)}.{Serialize(fieldAccess.Field)}";
                    default:
                        throw new NotSupportedException(fieldAccess.ExprObjCase.ToString());
                }
            }

            private string Serialize(ClassName classname)
            {
                return classname.Classname;
            }

            private string Serialize(This @this)
            {
                return "this";
            }

            private string Serialize(Variable variable)
            {
                return variable.Name;
            }

            private string Serialize(Expression expression)
            {
                switch (expression.ExprCase)
                {
                    case Expression.ExprOneofCase.Var:
                        return Serialize(expression.Var);

                    case Expression.ExprOneofCase.Const:
                        return "CONST";

                    case Expression.ExprOneofCase.This:
                        return Serialize(expression.This);

                    case Expression.ExprOneofCase.Classname:
                        return Serialize(expression.Classname);

                    case Expression.ExprOneofCase.FieldAccess:
                        return Serialize(expression.FieldAccess);

                    default:
                        throw new ArgumentOutOfRangeException(nameof(expression));
                }
            }
        }
    }
}
