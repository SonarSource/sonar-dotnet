/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CFG;
using SonarAnalyzer.CFG.LiveVariableAnalysis;
using SonarAnalyzer.CSharp.Core.Syntax.Utilities;

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
        dot.Should().BeIgnoringLineEndings("""
            digraph "RoslynCfgLva" {
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
            cfg0_block1 -> cfg0_block3 [label="LVA" fontcolor="blue" penwidth="2" color="blue"]
            cfg0_block2 -> cfg0_block3 [label="LVA" fontcolor="blue" penwidth="2" color="blue"]
            cfg0_block2 -> cfg0_block4 [label="LVA" fontcolor="blue" penwidth="2" color="blue"]
            cfg0_block3 -> cfg0_block4 [label="LVA" fontcolor="blue" penwidth="2" color="blue"]
            cfg0_block4 -> NoDestination_cfg0_block4 [label="StructuredExceptionHandling"]
            cfg0_block0 -> cfg0_block1
            cfg0_block4 -> cfg0_block5 [label="LVA" fontcolor="blue" penwidth="2" color="blue"]
            cfg0_block4 -> cfg0_block5 [label="LVA" fontcolor="blue" penwidth="2" color="blue"]
            cfg0_block2 -> cfg0_block5
            cfg0_block3 -> cfg0_block5
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
        dot.Should().BeIgnoringLineEndings("""
            digraph "RoslynCfgLva" {
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
            cfg0_block1 -> cfg0_block3 [label="LVA" fontcolor="blue" penwidth="2" color="blue"]
            cfg0_block2 -> cfg0_block3 [label="LVA" fontcolor="blue" penwidth="2" color="blue"]
            cfg0_block3 -> NoDestination_cfg0_block3 [label="Rethrow"]
            cfg0_block2 -> cfg0_block4 [label="LVA" fontcolor="blue" penwidth="2" color="blue"]
            cfg0_block3 -> cfg0_block4 [label="LVA" fontcolor="blue" penwidth="2" color="blue"]
            cfg0_block4 -> NoDestination_cfg0_block4 [label="StructuredExceptionHandling"]
            cfg0_block0 -> cfg0_block1
            cfg0_block4 -> cfg0_block5 [label="LVA" fontcolor="blue" penwidth="2" color="blue"]
            cfg0_block2 -> cfg0_block5
            }

            """);
    }

    [TestMethod]
    public void Serialize_While()
    {
        const string code = """
            class Sample
            {
                void Method()
                {
                    var value = 0;
                    while (value < 10)
                    {
                        Use(value);
                        value++;
                    }
                }

                void Use(int v) {}
            }
            """;
        var dot = CfgSerializer.Serialize(CreateLva(code));
        dot.Should().BeIgnoringLineEndings("""
            digraph "RoslynCfgLva" {
            subgraph "cluster_1" {
            label = "LocalLifetime region, Locals: value"
            cfg0_block1 [shape=record label="{BLOCK #1|0#: SimpleAssignmentOperation: value = 0|1#: 0#.Target: LocalReferenceOperation: value = 0|1#: 0#.Value: LiteralOperation: 0|##########}"]
            cfg0_block2 [shape=record label="{BLOCK #2|## BranchValue ##|0#: BinaryOperation: value \< 10|1#: 0#.LeftOperand: LocalReferenceOperation: value|1#: 0#.RightOperand: LiteralOperation: 10|##########}"]
            cfg0_block3 [shape=record label="{BLOCK #3|0#: ExpressionStatementOperation: Use(value);|1#: 0#.Operation: InvocationOperation: Use: Use(value)|2#: 1#.Instance: InstanceReferenceOperation: Use|2#: ArgumentOperation: value|3#: 2#.Value: LocalReferenceOperation: value|##########|0#: ExpressionStatementOperation: value++;|1#: 0#.Operation: IncrementOrDecrementOperation: value++|2#: 1#.Target: LocalReferenceOperation: value|##########}"]
            }
            cfg0_block0 [shape=record label="{ENTRY #0}"]
            cfg0_block4 [shape=record label="{EXIT #4}"]
            cfg0_block0 -> cfg0_block1
            cfg0_block1 -> cfg0_block2
            cfg0_block3 -> cfg0_block2
            cfg0_block2 -> cfg0_block3 [label="Else"]
            cfg0_block2 -> cfg0_block4 [label="WhenFalse"]
            }

            """);
    }

    [TestMethod]
    public void Serialize_Foreach()
    {
        const string code = """
            class Sample
            {
                void Method(int[] values)
                {
                    var value = 0;
                    foreach (var v in values)
                    {
                        Use(value);
                        value = v;
                    }
                }

                void Use(int v) {}
            }
            """;
        var dot = CfgSerializer.Serialize(CreateLva(code));
        dot.Should().BeIgnoringLineEndings("""
            digraph "RoslynCfgLva" {
            subgraph "cluster_1" {
            label = "LocalLifetime region, Locals: value"
            subgraph "cluster_2" {
            label = "LocalLifetime region, Captures: #Capture-0"
            subgraph "cluster_3" {
            label = "TryAndFinally region"
            subgraph "cluster_4" {
            label = "Try region"
            subgraph "cluster_5" {
            label = "LocalLifetime region, Locals: v"
            cfg0_block4 [shape=record label="{BLOCK #4|0#: SimpleAssignmentOperation: var|1#: 0#.Target: LocalReferenceOperation: var|1#: 0#.Value: ConversionOperation: var|2#: 1#.Operand: PropertyReferenceOperation: var|3#: 2#.Instance: FlowCaptureReferenceOperation: #Capture-0: values|##########|0#: ExpressionStatementOperation: Use(value);|1#: 0#.Operation: InvocationOperation: Use: Use(value)|2#: 1#.Instance: InstanceReferenceOperation: Use|2#: ArgumentOperation: value|3#: 2#.Value: LocalReferenceOperation: value|##########|0#: ExpressionStatementOperation: value = v;|1#: 0#.Operation: SimpleAssignmentOperation: value = v|2#: 1#.Target: LocalReferenceOperation: value|2#: 1#.Value: LocalReferenceOperation: v|##########}"]
            }
            cfg0_block3 [shape=record label="{BLOCK #3|## BranchValue ##|0#: InvocationOperation: MoveNext: values|1#: 0#.Instance: FlowCaptureReferenceOperation: #Capture-0: values|##########}"]
            }
            subgraph "cluster_6" {
            label = "Finally region, Captures: #Capture-1"
            cfg0_block5 [shape=record label="{BLOCK #5|0#: FlowCaptureOperation: #Capture-1: values|1#: 0#.Value: ConversionOperation: values|2#: 1#.Operand: FlowCaptureReferenceOperation: #Capture-0: values|##########|## BranchValue ##|0#: IsNullOperation: values|1#: 0#.Operand: FlowCaptureReferenceOperation: #Capture-1: values|##########}"]
            cfg0_block6 [shape=record label="{BLOCK #6|0#: InvocationOperation: Dispose: values|1#: 0#.Instance: FlowCaptureReferenceOperation: #Capture-1: values|##########}"]
            cfg0_block7 [shape=record label="{BLOCK #7}"]
            }
            }
            cfg0_block2 [shape=record label="{BLOCK #2|0#: FlowCaptureOperation: #Capture-0: values|1#: 0#.Value: InvocationOperation: GetEnumerator: values|2#: 1#.Instance: ConversionOperation: values|3#: 2#.Operand: ParameterReferenceOperation: values|##########}"]
            }
            cfg0_block1 [shape=record label="{BLOCK #1|0#: SimpleAssignmentOperation: value = 0|1#: 0#.Target: LocalReferenceOperation: value = 0|1#: 0#.Value: LiteralOperation: 0|##########}"]
            }
            cfg0_block0 [shape=record label="{ENTRY #0}"]
            cfg0_block8 [shape=record label="{EXIT #8}"]
            cfg0_block3 -> cfg0_block4 [label="Else"]
            cfg0_block2 -> cfg0_block3
            cfg0_block4 -> cfg0_block3
            cfg0_block3 -> cfg0_block5 [label="LVA" fontcolor="blue" penwidth="2" color="blue"]
            cfg0_block3 -> cfg0_block5 [label="LVA" fontcolor="blue" penwidth="2" color="blue"]
            cfg0_block2 -> cfg0_block5 [label="LVA" fontcolor="blue" penwidth="2" color="blue"]
            cfg0_block4 -> cfg0_block5 [label="LVA" fontcolor="blue" penwidth="2" color="blue"]
            cfg0_block5 -> cfg0_block6 [label="Else"]
            cfg0_block5 -> cfg0_block7 [label="WhenTrue"]
            cfg0_block6 -> cfg0_block7
            cfg0_block7 -> NoDestination_cfg0_block7 [label="StructuredExceptionHandling"]
            cfg0_block1 -> cfg0_block2
            cfg0_block0 -> cfg0_block1
            cfg0_block7 -> cfg0_block8 [label="LVA" fontcolor="blue" penwidth="2" color="blue"]
            cfg0_block3 -> cfg0_block8 [label="WhenFalse"]
            }

            """);
    }

    [TestMethod]
    public void Serialize_IfElse()
    {
        const string code = """
            class Sample
            {
                void Method(int value)
                {
                    if (value % 2 == 0)
                    {
                        Use(value);
                    }
                    else
                    {
                        Use(value);
                    }
                }

                void Use(int v) {}
            }
            """;
        var dot = CfgSerializer.Serialize(CreateLva(code));
        dot.Should().BeIgnoringLineEndings("""
            digraph "RoslynCfgLva" {
            cfg0_block0 [shape=record label="{ENTRY #0}"]
            cfg0_block1 [shape=record label="{BLOCK #1|## BranchValue ##|0#: BinaryOperation: value % 2 == 0|1#: 0#.LeftOperand: BinaryOperation: value % 2|2#: 1#.LeftOperand: ParameterReferenceOperation: value|2#: 1#.RightOperand: LiteralOperation: 2|1#: 0#.RightOperand: LiteralOperation: 0|##########}"]
            cfg0_block2 [shape=record label="{BLOCK #2|0#: ExpressionStatementOperation: Use(value);|1#: 0#.Operation: InvocationOperation: Use: Use(value)|2#: 1#.Instance: InstanceReferenceOperation: Use|2#: ArgumentOperation: value|3#: 2#.Value: ParameterReferenceOperation: value|##########}"]
            cfg0_block3 [shape=record label="{BLOCK #3|0#: ExpressionStatementOperation: Use(value);|1#: 0#.Operation: InvocationOperation: Use: Use(value)|2#: 1#.Instance: InstanceReferenceOperation: Use|2#: ArgumentOperation: value|3#: 2#.Value: ParameterReferenceOperation: value|##########}"]
            cfg0_block4 [shape=record label="{EXIT #4}"]
            cfg0_block0 -> cfg0_block1
            cfg0_block1 -> cfg0_block2 [label="Else"]
            cfg0_block1 -> cfg0_block3 [label="WhenFalse"]
            cfg0_block2 -> cfg0_block4
            cfg0_block3 -> cfg0_block4
            }

            """);
    }

    private static RoslynLiveVariableAnalysis CreateLva(string code)
    {
        var cfg = TestCompiler.CompileCfgCS(code);
        return new RoslynLiveVariableAnalysis(cfg, CSharpSyntaxClassifier.Instance, CancellationToken.None);
    }
}
