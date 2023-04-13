/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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
using SonarAnalyzer.UnitTest.Helpers;

namespace SonarAnalyzer.UnitTest.CFG.Sonar
{
    [TestClass]
    public class RoslynCfgSerializerTest
    {
        [TestMethod]
        public void Serialize_MethodNameUsedInTitle()
        {
            var code = @"
class Sample
{
    void Method() { }
}";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfgCS(code), "GraphTitle");
            dot.Should().BeIgnoringLineEndings(
@"digraph ""GraphTitle"" {
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block1 [shape=record label=""{EXIT #1}""]
cfg0_block0 -> cfg0_block1
}
");
        }

        [TestMethod]
        public void Serialize_EmptyMethod()
        {
            var code = @"
class Sample
{
    void Method() { }
}";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfgCS(code));

            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block1 [shape=record label=""{EXIT #1}""]
cfg0_block0 -> cfg0_block1
}
");
        }

        [TestMethod]
        public void Serialize_OperationSequence()
        {
            var code = @"
class Sample
{
    void Method()
    {
        A();
        B();
        var c = C();
    }

    private void A() { }
    private void B() { }
    private int C() => 42;
}";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfgCS(code));
            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
subgraph ""cluster_1"" {
label = ""LocalLifetime region, Locals: c""
cfg0_block1 [shape=record label=""{BLOCK #1|0# ExpressionStatementOperation: A();|1# InvocationOperation: A: A()|2# InstanceReferenceOperation: A|##########|0# ExpressionStatementOperation: B();|1# InvocationOperation: B: B()|2# InstanceReferenceOperation: B|##########|0# SimpleAssignmentOperation: c = C()|1# LocalReferenceOperation: c = C()|1# InvocationOperation: C: C()|2# InstanceReferenceOperation: C|##########}""]
}
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block2 [shape=record label=""{EXIT #2}""]
cfg0_block0 -> cfg0_block1
cfg0_block1 -> cfg0_block2
}
");
        }

        [TestMethod]
        public void Serialize_Switch()
        {
            var code = @"
class Sample
{
    void Foo(int a)
    {
        switch (a)
        {
            case 1:
                c1();
                break;

            case 2:
                c2();
                break;
        }
    }
    private void c1() { }
    private void c2() { }
}
";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfgCS(code));
            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
subgraph ""cluster_1"" {
label = ""LocalLifetime region, Captures: #Capture-0""
cfg0_block1 [shape=record label=""{BLOCK #1|0# FlowCaptureOperation: #Capture-0: a|1# ParameterReferenceOperation: a|##########|## BranchValue ##|0# BinaryOperation: 1|1# FlowCaptureReferenceOperation: #Capture-0: a|1# LiteralOperation: 1|##########}""]
cfg0_block2 [shape=record label=""{BLOCK #2|0# ExpressionStatementOperation: c1();|1# InvocationOperation: c1: c1()|2# InstanceReferenceOperation: c1|##########}""]
cfg0_block3 [shape=record label=""{BLOCK #3|## BranchValue ##|0# BinaryOperation: 2|1# FlowCaptureReferenceOperation: #Capture-0: a|1# LiteralOperation: 2|##########}""]
cfg0_block4 [shape=record label=""{BLOCK #4|0# ExpressionStatementOperation: c2();|1# InvocationOperation: c2: c2()|2# InstanceReferenceOperation: c2|##########}""]
}
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block5 [shape=record label=""{EXIT #5}""]
cfg0_block0 -> cfg0_block1
cfg0_block1 -> cfg0_block2 [label=""Else""]
cfg0_block1 -> cfg0_block3 [label=""WhenFalse""]
cfg0_block3 -> cfg0_block4 [label=""Else""]
cfg0_block2 -> cfg0_block5
cfg0_block3 -> cfg0_block5 [label=""WhenFalse""]
cfg0_block4 -> cfg0_block5
}
");
        }

        [TestMethod]
        public void Serialize_If()
        {
            var code = @"
class Sample
{
    void Method()
    {
        if (true)
        {
            Bar();
        }
    }
    void Bar() { }
}
";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfgCS(code));

            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block1 [shape=record label=""{BLOCK #1|## BranchValue ##|0# LiteralOperation: true|##########}""]
cfg0_block2 [shape=record label=""{BLOCK #2|0# ExpressionStatementOperation: Bar();|1# InvocationOperation: Bar: Bar()|2# InstanceReferenceOperation: Bar|##########}""]
cfg0_block3 [shape=record label=""{EXIT #3}""]
cfg0_block0 -> cfg0_block1
cfg0_block1 -> cfg0_block2 [label=""Else""]
cfg0_block1 -> cfg0_block3 [label=""WhenFalse""]
cfg0_block2 -> cfg0_block3
}
");
        }

        [TestMethod]
        public void Serialize_Foreach_Simple()
        {
            var code = @"
class Sample
{
    void Method(int[] items)
    {
        foreach (var i in items)
        {
            Bar(i);
        }
    }
    private void Bar(int i) { }
}";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfgCS(code));
            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
subgraph ""cluster_1"" {
label = ""LocalLifetime region, Captures: #Capture-0""
subgraph ""cluster_2"" {
label = ""TryAndFinally region""
subgraph ""cluster_3"" {
label = ""Try region""
subgraph ""cluster_4"" {
label = ""LocalLifetime region, Locals: i""
cfg0_block3 [shape=record label=""{BLOCK #3|0# SimpleAssignmentOperation: var|1# LocalReferenceOperation: var|1# ConversionOperation: var|2# PropertyReferenceOperation: var|3# FlowCaptureReferenceOperation: #Capture-0: items|##########|0# ExpressionStatementOperation: Bar(i);|1# InvocationOperation: Bar: Bar(i)|2# InstanceReferenceOperation: Bar|2# ArgumentOperation: i|3# LocalReferenceOperation: i|##########}""]
}
cfg0_block2 [shape=record label=""{BLOCK #2|## BranchValue ##|0# InvocationOperation: MoveNext: items|1# FlowCaptureReferenceOperation: #Capture-0: items|##########}""]
}
subgraph ""cluster_5"" {
label = ""Finally region, Captures: #Capture-1""
cfg0_block4 [shape=record label=""{BLOCK #4|0# FlowCaptureOperation: #Capture-1: items|1# ConversionOperation: items|2# FlowCaptureReferenceOperation: #Capture-0: items|##########|## BranchValue ##|0# IsNullOperation: items|1# FlowCaptureReferenceOperation: #Capture-1: items|##########}""]
cfg0_block5 [shape=record label=""{BLOCK #5|0# InvocationOperation: Dispose: items|1# FlowCaptureReferenceOperation: #Capture-1: items|##########}""]
cfg0_block6 [shape=record label=""{BLOCK #6}""]
}
}
cfg0_block1 [shape=record label=""{BLOCK #1|0# FlowCaptureOperation: #Capture-0: items|1# InvocationOperation: GetEnumerator: items|2# ConversionOperation: items|3# ParameterReferenceOperation: items|##########}""]
}
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block7 [shape=record label=""{EXIT #7}""]
cfg0_block2 -> cfg0_block3 [label=""Else""]
cfg0_block1 -> cfg0_block2
cfg0_block3 -> cfg0_block2
cfg0_block4 -> cfg0_block5 [label=""Else""]
cfg0_block4 -> cfg0_block6 [label=""WhenTrue""]
cfg0_block5 -> cfg0_block6
cfg0_block6 -> NoDestination_cfg0_block6 [label=""StructuredExceptionHandling""]
cfg0_block0 -> cfg0_block1
cfg0_block2 -> cfg0_block7 [label=""WhenFalse""]
}
");
        }

        [TestMethod]
        public void Serialize_Foreach_TupleVarDeclaration()
        {
            var code = @"
public class Sample
{
    public void Method((string key, string value)[] values)
    {
        foreach (var (key, value) in values)
        {
            string i = key;
            string j = value;
        }
    }
}";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfgCS(code));
            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
subgraph ""cluster_1"" {
label = ""LocalLifetime region, Captures: #Capture-0""
subgraph ""cluster_2"" {
label = ""TryAndFinally region""
subgraph ""cluster_3"" {
label = ""Try region""
subgraph ""cluster_4"" {
label = ""LocalLifetime region, Locals: key, value""
subgraph ""cluster_5"" {
label = ""LocalLifetime region, Locals: i, j""
cfg0_block4 [shape=record label=""{BLOCK #4|0# SimpleAssignmentOperation: i = key|1# LocalReferenceOperation: i = key|1# LocalReferenceOperation: key|##########|0# SimpleAssignmentOperation: j = value|1# LocalReferenceOperation: j = value|1# LocalReferenceOperation: value|##########}""]
}
cfg0_block3 [shape=record label=""{BLOCK #3|0# DeconstructionAssignmentOperation: var (key, value)|1# DeclarationExpressionOperation: var (key, value)|2# TupleOperation: (key, value)|3# LocalReferenceOperation: key|3# LocalReferenceOperation: value|1# ConversionOperation: var (key, value)|2# PropertyReferenceOperation: var (key, value)|3# FlowCaptureReferenceOperation: #Capture-0: values|##########}""]
}
cfg0_block2 [shape=record label=""{BLOCK #2|## BranchValue ##|0# InvocationOperation: MoveNext: values|1# FlowCaptureReferenceOperation: #Capture-0: values|##########}""]
}
subgraph ""cluster_6"" {
label = ""Finally region, Captures: #Capture-1""
cfg0_block5 [shape=record label=""{BLOCK #5|0# FlowCaptureOperation: #Capture-1: values|1# ConversionOperation: values|2# FlowCaptureReferenceOperation: #Capture-0: values|##########|## BranchValue ##|0# IsNullOperation: values|1# FlowCaptureReferenceOperation: #Capture-1: values|##########}""]
cfg0_block6 [shape=record label=""{BLOCK #6|0# InvocationOperation: Dispose: values|1# FlowCaptureReferenceOperation: #Capture-1: values|##########}""]
cfg0_block7 [shape=record label=""{BLOCK #7}""]
}
}
cfg0_block1 [shape=record label=""{BLOCK #1|0# FlowCaptureOperation: #Capture-0: values|1# InvocationOperation: GetEnumerator: values|2# ConversionOperation: values|3# ParameterReferenceOperation: values|##########}""]
}
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block8 [shape=record label=""{EXIT #8}""]
cfg0_block3 -> cfg0_block4
cfg0_block2 -> cfg0_block3 [label=""Else""]
cfg0_block1 -> cfg0_block2
cfg0_block4 -> cfg0_block2
cfg0_block5 -> cfg0_block6 [label=""Else""]
cfg0_block5 -> cfg0_block7 [label=""WhenTrue""]
cfg0_block6 -> cfg0_block7
cfg0_block7 -> NoDestination_cfg0_block7 [label=""StructuredExceptionHandling""]
cfg0_block0 -> cfg0_block1
cfg0_block2 -> cfg0_block8 [label=""WhenFalse""]
}
");
        }

        [TestMethod]
        public void Serialize_For()
        {
            var code = @"
class Sample
{
    void Method()
    {
        for (var i = 0; i < 10; i++)
        {
            Bar(i);
        }
    }
    private void Bar(int i) { }
}
";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfgCS(code));
            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
subgraph ""cluster_1"" {
label = ""LocalLifetime region, Locals: i""
cfg0_block1 [shape=record label=""{BLOCK #1|0# SimpleAssignmentOperation: i = 0|1# LocalReferenceOperation: i = 0|1# LiteralOperation: 0|##########}""]
cfg0_block2 [shape=record label=""{BLOCK #2|## BranchValue ##|0# BinaryOperation: i \< 10|1# LocalReferenceOperation: i|1# LiteralOperation: 10|##########}""]
cfg0_block3 [shape=record label=""{BLOCK #3|0# ExpressionStatementOperation: Bar(i);|1# InvocationOperation: Bar: Bar(i)|2# InstanceReferenceOperation: Bar|2# ArgumentOperation: i|3# LocalReferenceOperation: i|##########|0# ExpressionStatementOperation: i++|1# IncrementOrDecrementOperation: i++|2# LocalReferenceOperation: i|##########}""]
}
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block4 [shape=record label=""{EXIT #4}""]
cfg0_block0 -> cfg0_block1
cfg0_block1 -> cfg0_block2
cfg0_block3 -> cfg0_block2
cfg0_block2 -> cfg0_block3 [label=""Else""]
cfg0_block2 -> cfg0_block4 [label=""WhenFalse""]
}
");
        }

        [TestMethod]
        public void Serialize_Using()
        {
            var code = @"
class Sample
{
    void Method()
    {
        using (var x = new System.IO.MemoryStream())
        {
            Bar();
        }
    }
    private void Bar() { }
}";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfgCS(code));
            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
subgraph ""cluster_1"" {
label = ""LocalLifetime region, Locals: x""
subgraph ""cluster_2"" {
label = ""TryAndFinally region""
subgraph ""cluster_3"" {
label = ""Try region""
cfg0_block2 [shape=record label=""{BLOCK #2|0# ExpressionStatementOperation: Bar();|1# InvocationOperation: Bar: Bar()|2# InstanceReferenceOperation: Bar|##########}""]
}
subgraph ""cluster_4"" {
label = ""Finally region""
cfg0_block3 [shape=record label=""{BLOCK #3|## BranchValue ##|0# IsNullOperation: x = new System.IO.MemoryStream()|1# LocalReferenceOperation: x = new System.IO.MemoryStream()|##########}""]
cfg0_block4 [shape=record label=""{BLOCK #4|0# InvocationOperation: Dispose: x = new System.IO.MemoryStream()|1# ConversionOperation: x = new System.IO.MemoryStream()|2# LocalReferenceOperation: x = new System.IO.MemoryStream()|##########}""]
cfg0_block5 [shape=record label=""{BLOCK #5}""]
}
}
cfg0_block1 [shape=record label=""{BLOCK #1|0# SimpleAssignmentOperation: x = new System.IO.MemoryStream()|1# LocalReferenceOperation: x = new System.IO.MemoryStream()|1# ObjectCreationOperation: new System.IO.MemoryStream()|##########}""]
}
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block6 [shape=record label=""{EXIT #6}""]
cfg0_block1 -> cfg0_block2
cfg0_block3 -> cfg0_block4 [label=""Else""]
cfg0_block3 -> cfg0_block5 [label=""WhenTrue""]
cfg0_block4 -> cfg0_block5
cfg0_block5 -> NoDestination_cfg0_block5 [label=""StructuredExceptionHandling""]
cfg0_block0 -> cfg0_block1
cfg0_block2 -> cfg0_block6
}
");
        }

        [TestMethod]
        public void Serialize_Lock()
        {
            var code = @"
class Sample
{
    object x;

    void Method()
    {
        lock (x)
        {
            Bar();
        }
    }
    private void Bar() { }
}";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfgCS(code));
            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
subgraph ""cluster_1"" {
label = ""LocalLifetime region, Locals: N/A, Captures: #Capture-0""
subgraph ""cluster_2"" {
label = ""TryAndFinally region""
subgraph ""cluster_3"" {
label = ""Try region""
cfg0_block2 [shape=record label=""{BLOCK #2|0# InvocationOperation: Enter: x|1# ArgumentOperation: x|2# FlowCaptureReferenceOperation: #Capture-0: x|1# ArgumentOperation: x|2# LocalReferenceOperation: x|##########|0# ExpressionStatementOperation: Bar();|1# InvocationOperation: Bar: Bar()|2# InstanceReferenceOperation: Bar|##########}""]
}
subgraph ""cluster_4"" {
label = ""Finally region""
cfg0_block3 [shape=record label=""{BLOCK #3|## BranchValue ##|0# LocalReferenceOperation: x|##########}""]
cfg0_block4 [shape=record label=""{BLOCK #4|0# InvocationOperation: Exit: x|1# ArgumentOperation: x|2# FlowCaptureReferenceOperation: #Capture-0: x|##########}""]
cfg0_block5 [shape=record label=""{BLOCK #5}""]
}
}
cfg0_block1 [shape=record label=""{BLOCK #1|0# FlowCaptureOperation: #Capture-0: x|1# FieldReferenceOperation: x|2# InstanceReferenceOperation: x|##########}""]
}
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block6 [shape=record label=""{EXIT #6}""]
cfg0_block1 -> cfg0_block2
cfg0_block3 -> cfg0_block4 [label=""Else""]
cfg0_block3 -> cfg0_block5 [label=""WhenFalse""]
cfg0_block4 -> cfg0_block5
cfg0_block5 -> NoDestination_cfg0_block5 [label=""StructuredExceptionHandling""]
cfg0_block0 -> cfg0_block1
cfg0_block2 -> cfg0_block6
}
");
        }

        [TestMethod]
        public void Serialize_Regions()
        {
            var code = @"
class Sample
{
    void Method()
    {
        Before();
        try
        {
            InTry();
        }
        finally
        {
            InFinally();
        }
        After();
    }
    private void Before() { }
    private void InTry() { }
    private void InFinally() { }
    private void After() { }
}";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfgCS(code));
            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
subgraph ""cluster_1"" {
label = ""TryAndFinally region""
subgraph ""cluster_2"" {
label = ""Try region""
cfg0_block2 [shape=record label=""{BLOCK #2|0# ExpressionStatementOperation: InTry();|1# InvocationOperation: InTry: InTry()|2# InstanceReferenceOperation: InTry|##########}""]
}
subgraph ""cluster_3"" {
label = ""Finally region""
cfg0_block3 [shape=record label=""{BLOCK #3|0# ExpressionStatementOperation: InFinally();|1# InvocationOperation: InFinally: InFinally()|2# InstanceReferenceOperation: InFinally|##########}""]
}
}
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block1 [shape=record label=""{BLOCK #1|0# ExpressionStatementOperation: Before();|1# InvocationOperation: Before: Before()|2# InstanceReferenceOperation: Before|##########}""]
cfg0_block4 [shape=record label=""{BLOCK #4|0# ExpressionStatementOperation: After();|1# InvocationOperation: After: After()|2# InstanceReferenceOperation: After|##########}""]
cfg0_block5 [shape=record label=""{EXIT #5}""]
cfg0_block1 -> cfg0_block2
cfg0_block3 -> NoDestination_cfg0_block3 [label=""StructuredExceptionHandling""]
cfg0_block0 -> cfg0_block1
cfg0_block2 -> cfg0_block4
cfg0_block4 -> cfg0_block5
}
");
        }

        [TestMethod]
        public void Serialize_Region_ExceptionType()
        {
            var code = @"
class Sample
{
    void Method()
    {
        try { }
        catch(System.InvalidOperationException ex) { }
    }
}";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfgCS(code));
            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
subgraph ""cluster_1"" {
label = ""TryAndCatch region""
subgraph ""cluster_2"" {
label = ""Try region""
cfg0_block1 [shape=record label=""{BLOCK #1}""]
}
subgraph ""cluster_3"" {
label = ""Catch region: System.InvalidOperationException, Locals: ex""
cfg0_block2 [shape=record label=""{BLOCK #2|0# SimpleAssignmentOperation: (System.InvalidOperationException ex)|1# LocalReferenceOperation: (System.InvalidOperationException ex)|1# CaughtExceptionOperation: (System.InvalidOperationException ex)|##########}""]
}
}
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block3 [shape=record label=""{EXIT #3}""]
cfg0_block0 -> cfg0_block1
cfg0_block1 -> cfg0_block3
cfg0_block2 -> cfg0_block3
}
");
        }

        [TestMethod]
        public void Serialize_LocalFunctions()
        {
            var code = @"
class Sample
{
    void Method()
    {
        var fourty = 40;
        Local();

        int Local() => fourty + LocalStatic();

        static int LocalStatic() => LocalStaticArg(1);
        static int LocalStaticArg(int one) => one + 1; // Overloaded
    }
}";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfgCS(code));
            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
subgraph ""cluster_1"" {
label = ""LocalLifetime region, Locals: fourty""
cfg0_block1 [shape=record label=""{BLOCK #1|0# SimpleAssignmentOperation: fourty = 40|1# LocalReferenceOperation: fourty = 40|1# LiteralOperation: 40|##########|0# ExpressionStatementOperation: Local();|1# InvocationOperation: Local: Local()|##########}""]
}
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block2 [shape=record label=""{EXIT #2}""]
subgraph ""cluster_3"" {
label = ""RoslynCfg.Local""
cfg2_block0 [shape=record label=""{ENTRY #0}""]
cfg2_block1 [shape=record label=""{BLOCK #1|## BranchValue ##|0# BinaryOperation: fourty + LocalStatic()|1# LocalReferenceOperation: fourty|1# InvocationOperation: LocalStatic: LocalStatic()|##########}""]
cfg2_block2 [shape=record label=""{EXIT #2}""]
}
subgraph ""cluster_5"" {
label = ""RoslynCfg.LocalStatic""
cfg4_block0 [shape=record label=""{ENTRY #0}""]
cfg4_block1 [shape=record label=""{BLOCK #1|## BranchValue ##|0# InvocationOperation: LocalStaticArg: LocalStaticArg(1)|1# ArgumentOperation: 1|2# LiteralOperation: 1|##########}""]
cfg4_block2 [shape=record label=""{EXIT #2}""]
}
subgraph ""cluster_7"" {
label = ""RoslynCfg.LocalStaticArg""
cfg6_block0 [shape=record label=""{ENTRY #0}""]
cfg6_block1 [shape=record label=""{BLOCK #1|## BranchValue ##|0# BinaryOperation: one + 1|1# ParameterReferenceOperation: one|1# LiteralOperation: 1|##########}""]
cfg6_block2 [shape=record label=""{EXIT #2}""]
}
cfg0_block0 -> cfg0_block1
cfg0_block1 -> cfg0_block2
cfg2_block0 -> cfg2_block1
cfg2_block1 -> cfg2_block2 [label=""Return""]
cfg4_block0 -> cfg4_block1
cfg4_block1 -> cfg4_block2 [label=""Return""]
cfg6_block0 -> cfg6_block1
cfg6_block1 -> cfg6_block2 [label=""Return""]
}
");
        }

        [TestMethod]
        public void Serialize_Lambdas()
        {
            var code = @"
class Sample
{
    void Method(int arg)
    {
        Bar(x => { return arg + 1; });
        Bar(x => arg - 1);
    }
    private void Bar(System.Func<int, int> f) { }
}";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfgCS(code));
            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block1 [shape=record label=""{BLOCK #1|0# ExpressionStatementOperation: Bar(x =\> \{ return arg + 1; \});|1# InvocationOperation: Bar: Bar(x =\> \{ return arg + 1; \})|2# InstanceReferenceOperation: Bar|2# ArgumentOperation: x =\> \{ return arg + 1; \}|3# DelegateCreationOperation: x =\> \{ return arg + 1; \}|4# FlowAnonymousFunctionOperation: x =\> \{ return arg + 1; \}|##########|0# ExpressionStatementOperation: Bar(x =\> arg - 1);|1# InvocationOperation: Bar: Bar(x =\> arg - 1)|2# InstanceReferenceOperation: Bar|2# ArgumentOperation: x =\> arg - 1|3# DelegateCreationOperation: x =\> arg - 1|4# FlowAnonymousFunctionOperation: x =\> arg - 1|##########}""]
cfg0_block2 [shape=record label=""{EXIT #2}""]
subgraph ""cluster_2"" {
label = ""RoslynCfg.anonymous""
cfg1_block0 [shape=record label=""{ENTRY #0}""]
cfg1_block1 [shape=record label=""{BLOCK #1|## BranchValue ##|0# BinaryOperation: arg + 1|1# ParameterReferenceOperation: arg|1# LiteralOperation: 1|##########}""]
cfg1_block2 [shape=record label=""{EXIT #2}""]
}
subgraph ""cluster_4"" {
label = ""RoslynCfg.anonymous""
cfg3_block0 [shape=record label=""{ENTRY #0}""]
cfg3_block1 [shape=record label=""{BLOCK #1|## BranchValue ##|0# BinaryOperation: arg - 1|1# ParameterReferenceOperation: arg|1# LiteralOperation: 1|##########}""]
cfg3_block2 [shape=record label=""{EXIT #2}""]
}
cfg0_block0 -> cfg0_block1
cfg0_block1 -> cfg0_block2
cfg1_block0 -> cfg1_block1
cfg1_block1 -> cfg1_block2 [label=""Return""]
cfg3_block0 -> cfg3_block1
cfg3_block1 -> cfg3_block2 [label=""Return""]
}
");
        }

        [TestMethod]
        public void Serialize_CaptureId()
        {
            var code = @"
class Sample
{
    void Method(bool arg)
    {
        bool b = arg && false;
        b = arg || true;
    }
}";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfgCS(code));
            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
subgraph ""cluster_1"" {
label = ""LocalLifetime region, Locals: b""
subgraph ""cluster_2"" {
label = ""LocalLifetime region, Captures: #Capture-0""
cfg0_block1 [shape=record label=""{BLOCK #1|## BranchValue ##|0# ParameterReferenceOperation: arg|##########}""]
cfg0_block2 [shape=record label=""{BLOCK #2|0# FlowCaptureOperation: #Capture-0: false|1# LiteralOperation: false|##########}""]
cfg0_block3 [shape=record label=""{BLOCK #3|0# FlowCaptureOperation: #Capture-0: arg|1# LiteralOperation: arg|##########}""]
cfg0_block4 [shape=record label=""{BLOCK #4|0# SimpleAssignmentOperation: b = arg && false|1# LocalReferenceOperation: b = arg && false|1# FlowCaptureReferenceOperation: #Capture-0: arg && false|##########}""]
}
subgraph ""cluster_3"" {
label = ""LocalLifetime region, Captures: #Capture-1, #Capture-2""
cfg0_block5 [shape=record label=""{BLOCK #5|0# FlowCaptureOperation: #Capture-1: b|1# LocalReferenceOperation: b|##########|## BranchValue ##|0# ParameterReferenceOperation: arg|##########}""]
cfg0_block6 [shape=record label=""{BLOCK #6|0# FlowCaptureOperation: #Capture-2: true|1# LiteralOperation: true|##########}""]
cfg0_block7 [shape=record label=""{BLOCK #7|0# FlowCaptureOperation: #Capture-2: arg|1# LiteralOperation: arg|##########}""]
cfg0_block8 [shape=record label=""{BLOCK #8|0# ExpressionStatementOperation: b = arg \|\| true;|1# SimpleAssignmentOperation: b = arg \|\| true|2# FlowCaptureReferenceOperation: #Capture-1: b|2# FlowCaptureReferenceOperation: #Capture-2: arg \|\| true|##########}""]
}
}
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block9 [shape=record label=""{EXIT #9}""]
cfg0_block0 -> cfg0_block1
cfg0_block1 -> cfg0_block2 [label=""Else""]
cfg0_block1 -> cfg0_block3 [label=""WhenFalse""]
cfg0_block2 -> cfg0_block4
cfg0_block3 -> cfg0_block4
cfg0_block4 -> cfg0_block5
cfg0_block5 -> cfg0_block6 [label=""Else""]
cfg0_block5 -> cfg0_block7 [label=""WhenTrue""]
cfg0_block6 -> cfg0_block8
cfg0_block7 -> cfg0_block8
cfg0_block8 -> cfg0_block9
}
");
        }

        [TestMethod]
        public void Serialize_InvalidOperation()
        {
            var code = @"
class Sample
{
    void Method()
    {
        undefined();
    }
}";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfgCS(code, ignoreErrors: true));
            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block1 [shape=record label=""{BLOCK #1|0# ExpressionStatementOperation: undefined();|1# INVALID: undefined()|2# INVALID: undefined|##########}""]
cfg0_block2 [shape=record label=""{EXIT #2}""]
cfg0_block0 -> cfg0_block1
cfg0_block1 -> cfg0_block2
}
");
        }

        [TestMethod]
        public void Serialize_TryCatchChain()
        {
            var code = @"
class Sample
{
    void Method()
    {
        try { }
        catch { }

        try { }
        catch { }
    }
}";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfgCS(code));
            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
subgraph ""cluster_1"" {
label = ""TryAndCatch region""
subgraph ""cluster_2"" {
label = ""Try region""
cfg0_block1 [shape=record label=""{BLOCK #1}""]
}
subgraph ""cluster_3"" {
label = ""Catch region: object""
cfg0_block2 [shape=record label=""{BLOCK #2}""]
}
}
subgraph ""cluster_4"" {
label = ""TryAndCatch region""
subgraph ""cluster_5"" {
label = ""Try region""
cfg0_block3 [shape=record label=""{BLOCK #3}""]
}
subgraph ""cluster_6"" {
label = ""Catch region: object""
cfg0_block4 [shape=record label=""{BLOCK #4}""]
}
}
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block5 [shape=record label=""{EXIT #5}""]
cfg0_block0 -> cfg0_block1
cfg0_block1 -> cfg0_block3
cfg0_block2 -> cfg0_block3
cfg0_block3 -> cfg0_block5
cfg0_block4 -> cfg0_block5
}
");
        }
    }
}
