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

using SonarAnalyzer.CFG;
using SonarAnalyzer.CFG.LiveVariableAnalysis;

namespace SonarAnalyzer.Test.CFG.Roslyn;

[TestClass]
public class RoslynLvaSerializerTest
{
    [TestMethod]
    public void Serialize_TryCatchFinally()
    {
        const string code = """
            class Sample
            {
                void Method()
                {
                    var value = 0;
                    try
                    {
                        Use(0);
                        value = 42;
                    }
                    catch
                    {
                        Use(value);
                        value = 1;
                    }
                    finally
                    {
                        Use(value);
                    }
                }

                void Use(int v) {}
            }
            """;
        var dot = CfgSerializer.Serialize(CreateLva(code));
        dot.Should().BeIgnoringLineEndingsAndEmptyLines("""
            digraph "RoslynCfg" {
            subgraph "cluster_1" {
            label = "LocalLifetime region, Locals: value"
            subgraph "cluster_2" {
            label = "TryAndFinally region"
            subgraph "cluster_3" {
            label = "Try region"
            subgraph "cluster_4" {
            label = "TryAndCatch region"
            subgraph "cluster_5" {
            label = "Try region"
            cfg0_block2 [shape=record label="{BLOCK #2|0#: ExpressionStatementOperation: Use(0);|1#: 0#.Operation: InvocationOperation: Use: Use(0)|2#: 1#.Instance: InstanceReferenceOperation: Use|2#: ArgumentOperation: 0|3#: 2#.Value: LiteralOperation: 0|##########|0#: ExpressionStatementOperation: value = 42;|1#: 0#.Operation: SimpleAssignmentOperation: value = 42|2#: 1#.Target: LocalReferenceOperation: value|2#: 1#.Value: LiteralOperation: 42|##########}"]
            }
            subgraph "cluster_6" {
            label = "Catch region: object"
            cfg0_block3 [shape=record label="{BLOCK #3|0#: ExpressionStatementOperation: Use(value);|1#: 0#.Operation: InvocationOperation: Use: Use(value)|2#: 1#.Instance: InstanceReferenceOperation: Use|2#: ArgumentOperation: value|3#: 2#.Value: LocalReferenceOperation: value|##########|0#: ExpressionStatementOperation: value = 1;|1#: 0#.Operation: SimpleAssignmentOperation: value = 1|2#: 1#.Target: LocalReferenceOperation: value|2#: 1#.Value: LiteralOperation: 1|##########}"]
            }
            }
            }
            subgraph "cluster_7" {
            label = "Finally region"
            cfg0_block4 [shape=record label="{BLOCK #4|0#: ExpressionStatementOperation: Use(value);|1#: 0#.Operation: InvocationOperation: Use: Use(value)|2#: 1#.Instance: InstanceReferenceOperation: Use|2#: ArgumentOperation: value|3#: 2#.Value: LocalReferenceOperation: value|##########}"]
            }
            }
            cfg0_block1 [shape=record label="{BLOCK #1|0#: SimpleAssignmentOperation: value = 0|1#: 0#.Target: LocalReferenceOperation: value = 0|1#: 0#.Value: LiteralOperation: 0|##########}"]
            }
            cfg0_block0 [shape=record label="{ENTRY #0}"]
            cfg0_block5 [shape=record label="{EXIT #5}"]
            cfg0_block1 -> cfg0_block2
            cfg0_block2 -> cfg0_block3
            cfg0_block1 -> cfg0_block3
            cfg0_block2 -> cfg0_block4
            cfg0_block3 -> cfg0_block4
            cfg0_block4 -> NoDestination_cfg0_block4 [label="StructuredExceptionHandling"]
            cfg0_block0 -> cfg0_block1
            cfg0_block4 -> cfg0_block5
            cfg0_block4 -> cfg0_block5
            }
            """);
        }

    [TestMethod]
    public void Serialize_TryCatchFinallyRethrow()
    {
        const string code = """
            class Sample
            {
                void Method()
                {
                    var value = 0;
                    try
                    {
                        Use(0);
                        value = 42;
                    }
                    catch
                    {
                        Use(value);
                        value = 1;
                        throw;
                    }
                    finally
                    {
                        Use(value);
                    }
                }

                void Use(int v) {}
            }
            """;
        var dot = CfgSerializer.Serialize(CreateLva(code));
        dot.Should().BeIgnoringLineEndingsAndEmptyLines("""
            digraph "RoslynCfg" {
            subgraph "cluster_1" {
            label = "LocalLifetime region, Locals: value"
            subgraph "cluster_2" {
            label = "TryAndFinally region"
            subgraph "cluster_3" {
            label = "Try region"
            subgraph "cluster_4" {
            label = "TryAndCatch region"
            subgraph "cluster_5" {
            label = "Try region"
            cfg0_block2 [shape=record label="{BLOCK #2|0#: ExpressionStatementOperation: Use(0);|1#: 0#.Operation: InvocationOperation: Use: Use(0)|2#: 1#.Instance: InstanceReferenceOperation: Use|2#: ArgumentOperation: 0|3#: 2#.Value: LiteralOperation: 0|##########|0#: ExpressionStatementOperation: value = 42;|1#: 0#.Operation: SimpleAssignmentOperation: value = 42|2#: 1#.Target: LocalReferenceOperation: value|2#: 1#.Value: LiteralOperation: 42|##########}"]
            }
            subgraph "cluster_6" {
            label = "Catch region: object"
            cfg0_block3 [shape=record label="{BLOCK #3|0#: ExpressionStatementOperation: Use(value);|1#: 0#.Operation: InvocationOperation: Use: Use(value)|2#: 1#.Instance: InstanceReferenceOperation: Use|2#: ArgumentOperation: value|3#: 2#.Value: LocalReferenceOperation: value|##########|0#: ExpressionStatementOperation: value = 1;|1#: 0#.Operation: SimpleAssignmentOperation: value = 1|2#: 1#.Target: LocalReferenceOperation: value|2#: 1#.Value: LiteralOperation: 1|##########}"]
            }
            }
            }
            subgraph "cluster_7" {
            label = "Finally region"
            cfg0_block4 [shape=record label="{BLOCK #4|0#: ExpressionStatementOperation: Use(value);|1#: 0#.Operation: InvocationOperation: Use: Use(value)|2#: 1#.Instance: InstanceReferenceOperation: Use|2#: ArgumentOperation: value|3#: 2#.Value: LocalReferenceOperation: value|##########}"]
            }
            }
            cfg0_block1 [shape=record label="{BLOCK #1|0#: SimpleAssignmentOperation: value = 0|1#: 0#.Target: LocalReferenceOperation: value = 0|1#: 0#.Value: LiteralOperation: 0|##########}"]
            }
            cfg0_block0 [shape=record label="{ENTRY #0}"]
            cfg0_block5 [shape=record label="{EXIT #5}"]
            cfg0_block1 -> cfg0_block2
            cfg0_block2 -> cfg0_block3
            cfg0_block1 -> cfg0_block3
            cfg0_block3 -> NoDestination_cfg0_block3 [label="Rethrow"]
            cfg0_block2 -> cfg0_block4
            cfg0_block4 -> NoDestination_cfg0_block4 [label="StructuredExceptionHandling"]
            cfg0_block0 -> cfg0_block1
            cfg0_block4 -> cfg0_block5
            }
            """);
        }

    private static RoslynLiveVariableAnalysis CreateLva(string code)
    {
        var cfg = TestHelper.CompileCfgCS(code);
        return new RoslynLiveVariableAnalysis(cfg, CancellationToken.None);
    }
}
