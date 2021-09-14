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

using System;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.CFG;
using SonarAnalyzer.CFG.Sonar;
using SonarAnalyzer.LiveVariableAnalysis.CSharp;
using SonarAnalyzer.UnitTest.CFG.Sonar;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.LiveVariableAnalysis
{
    [TestClass]
    public class SonarLiveVariableAnalysisTest
    {
        [TestMethod]
        public void WriteOnly()
        {
            var code = @"
int a = 1;
var b = Method();
var c = 2 + 3;";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock);
        }

        [TestMethod]
        public void UsedBeforeAssigned_LiveIn()
        {
            var code = @"
Method(intParameter);
IsMethod(boolParameter);";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveIn("intParameter", "boolParameter"));
        }

        [TestMethod]
        public void UsedAfterBranch_LiveOut()
        {
            /*       Binary
             *       /   \
             *    Jump   Simple
             *   return  Method()
             *       \   /
             *        Exit
             */
            var code = @"
if (boolParameter)
    return;
Method(intParameter);";
            var context = new Context(code);
            var binary = context.Cfg.EntryBlock;
            var jump = context.Block<JumpBlock>();
            var simple = context.Block<SimpleBlock>();
            var exit = context.Cfg.ExitBlock;
            context.Validate(binary, new LiveIn("boolParameter", "intParameter"), new LiveOut("intParameter"));
            context.Validate(jump);
            context.Validate(simple, new LiveIn("intParameter"));
            context.Validate(exit);
        }

        [TestMethod]
        public void Captured_NotLiveIn_NotLiveOut()
        {
            /*       Binary
             *       /   \
             *    Jump   Simple
             *   return  Method()
             *       \   /
             *        Exit
             */
            var code = @"
Capturing(() => intParameter);
if (boolParameter)
    return;
Method(intParameter);";
            var context = new Context(code);
            var binary = context.Cfg.EntryBlock;
            var jump = context.Block<JumpBlock>();
            var simple = context.Block<SimpleBlock>();
            var exit = context.Cfg.ExitBlock;
            context.Validate(binary, new Captured("intParameter"), new LiveIn("boolParameter"));
            context.Validate(jump, new Captured("intParameter"));
            context.Validate(simple, new Captured("intParameter"));
            context.Validate(exit, new Captured("intParameter"));
        }

        [TestMethod]
        public void Assigned_NotLiveIn_NotLiveOut()
        {
            /*       Binary
             *       /   \
             *    Jump   Simple
             *   return  intParameter=0
             *       \   /
             *        Exit
             */
            var code = @"
if (boolParameter)
    return;
intParameter = 0;";
            var context = new Context(code);
            var binary = context.Cfg.EntryBlock;
            var jump = context.Block<JumpBlock>();
            var simple = context.Block<SimpleBlock>();
            var exit = context.Cfg.ExitBlock;
            context.Validate(binary, new LiveIn("boolParameter"));
            context.Validate(jump);
            context.Validate(simple);
            context.Validate(exit);
        }

        [TestMethod]
        public void LongPropagationChain_LiveIn_LiveOut()
        {
            /*    Binary -> Jump (return) ----+
             *    declare                     |
             *      |                         |
             *    Binary -> Jump (return) ---+|
             *    use & assign               ||
             *      |                        ||
             *    Binary -> Jump (return) --+||
             *    assign                    |||
             *      |                       vvv
             *    Simple -----------------> Exit
             *    use
             */
            var code = @"
var value = 0;
if (boolParameter)
    return;
Method(value);
value = 42;
if (boolParameter)
    return;
value = 42;
if (boolParameter)
    return;
Method(intParameter, value);";
            var context = new Context(code);
            var allBinary = context.Cfg.Blocks.OfType<BinaryBranchBlock>().ToArray();
            var simple = context.Block<SimpleBlock>();
            context.Validate(allBinary[0], new LiveIn("boolParameter", "intParameter"), new LiveOut("boolParameter", "intParameter", "value"));
            context.Validate(allBinary[1], new LiveIn("boolParameter", "intParameter", "value"), new LiveOut("boolParameter", "intParameter"));
            context.Validate(allBinary[2], new LiveIn("boolParameter", "intParameter"), new LiveOut("value", "intParameter"));
            context.Validate(simple, new LiveIn("value", "intParameter"));
            context.Validate(context.Cfg.ExitBlock);
        }

        [TestMethod]
        public void BranchedPropagationChain_LiveIn_LiveOut()
        {
            /*              Binary
             *              boolParameter
             *              /           \
             *             /             \
             *            /               \
             *       Binary             Binary
             *       firstBranch        secondBranch
             *       /      \              /      \
             *      /        \            /        \
             *  Simple     Simple      Simple      Simple
             *  firstTrue  firstFalse  secondTrue  secondFalse
             *      \        /            \        /
             *       \      /              \      /
             *        Simple                Simple
             *        first                 second
             *             \               /
             *              \             /
             *                  Simple
             *                  reassigned
             *                  everywhere
             */
            var code = @"
var everywhere = 42;
var reassigned = 42;
var first = 42;
var firstTrue = 42;
var firstFalse = 42;
var second = 42;
var secondTrue = 42;
var secondFalse = 42;
var firstCondition = boolParameter;
var secondCondition = boolParameter;
if (boolParameter)
{
    if (firstCondition)
    {
        Method(firstTrue);
    }
    else
    {
        Method(firstFalse);
    }
    Method(first);
}
else
{
    if (secondCondition)
    {
        Method(secondTrue);
    }
    else
    {
        Method(secondFalse);
    }
    Method(second);
}
reassigned = 0;
Method(everywhere, reassigned);";
            var context = new Context(code);
            context.Validate(
                context.Block<BinaryBranchBlock>("boolParameter"),
                new LiveIn("boolParameter"),
                new LiveOut("everywhere", "firstCondition", "firstTrue", "firstFalse", "first", "secondCondition", "secondTrue", "secondFalse", "second"));
            // First block
            context.Validate(
                context.Block<BinaryBranchBlock>("firstCondition"),
                new LiveIn("everywhere", "firstCondition", "firstTrue", "firstFalse", "first"),
                new LiveOut("everywhere", "firstTrue", "firstFalse", "first"));
            context.Validate(
                context.Block<SimpleBlock>("Method(firstTrue)"),
                new LiveIn("everywhere", "firstTrue", "first"),
                new LiveOut("everywhere", "first"));
            context.Validate(
                context.Block<SimpleBlock>("Method(firstFalse)"),
                new LiveIn("everywhere", "firstFalse", "first"),
                new LiveOut("everywhere", "first"));
            context.Validate(
                context.Block<SimpleBlock>("Method(first)"),
                new LiveIn("everywhere", "first"),
                new LiveOut("everywhere"));
            // Second block
            context.Validate(
                context.Block<BinaryBranchBlock>("secondCondition"),
                new LiveIn("everywhere", "secondCondition", "secondTrue", "secondFalse", "second"),
                new LiveOut("everywhere", "secondTrue", "secondFalse", "second"));
            context.Validate(
                context.Block<SimpleBlock>("Method(secondTrue)"),
                new LiveIn("everywhere", "secondTrue", "second"),
                new LiveOut("everywhere", "second"));
            context.Validate(
                context.Block<SimpleBlock>("Method(secondFalse)"),
                new LiveIn("everywhere", "secondFalse", "second"),
                new LiveOut("everywhere", "second"));
            context.Validate(
                context.Block<SimpleBlock>("Method(second)"),
                new LiveIn("everywhere", "second"),
                new LiveOut("everywhere"));
            // Common end
            context.Validate(context.Block<SimpleBlock>("Method(everywhere, reassigned)"), new LiveIn("everywhere"));
        }

        [TestMethod]
        public void ProcessIdentifier_InNameOf_NotLiveIn_NotLiveOut()
        {
            var code = @"Method(nameof(intParameter));";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock);
        }

        [TestMethod]
        public void ProcessIdentifier_LocalScopeSymbol_LiveIn()
        {
            var code = @"
var variable = 42;
if (boolParameter)
    return;
Method(intParameter, variable);";
            var context = new Context(code);
            context.Validate(context.Block<SimpleBlock>(), new LiveIn("intParameter", "variable"));
        }

        [TestMethod]
        public void ProcessIdentifier_GlobalScopeSymbol_NotLiveIn_NotLiveOut()
        {
            var code = @"
var s = new Sample();
Method(field, s.Property);";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock);
        }

        [TestMethod]
        public void ProcessIdentifier_UndefinedSymbol_NotLiveIn_NotLiveOut()
        {
            var code = @"Method(undefined);";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock);
        }

        [TestMethod]
        public void ProcessIdentifier_RefOutArgument_NotLiveIn_LiveOut()
        {
            var code = @"
outParameter = true;
Method(outParameter, refParameter);";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock);
        }

        [TestMethod]
        public void ProcessIdentifier_Assigned_NotLiveIn_LiveOut()
        {
            // Jump (intParamter=42; goto) --> Jump (label) --> Simple (Method) --> Exit
            var code = @"
intParameter = 42;
goto A;
A:
Method(intParameter);";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveOut("intParameter"));
            context.Validate(context.Block<SimpleBlock>(), new LiveIn("intParameter"));
        }

        [TestMethod]
        public void ProcessIdentifier_NotAssigned_LiveIn_LiveOut()
        {
            // Jump (intParamter=42; goto) --> Jump (label) --> Simple (Method) --> Exit
            var code = @"
var value = intParameter;
goto A;
A:
Method(value, intParameter);";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveIn("intParameter"), new LiveOut("intParameter", "value"));
            context.Validate(context.Block<SimpleBlock>(), new LiveIn("value", "intParameter"));
        }

        [TestMethod]
        public void ProcessSimpleAssignment_Discard_NotLiveIn_NotLiveOut()
        {
            var code = @"_ = intParameter;";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveIn("intParameter"));
        }

        [TestMethod]
        public void ProcessSimpleAssignment_UndefinedSymbol_NotLiveIn_NotLiveOut()
        {
            var code = @"
undefined = intParameter;
if (undefined == 0)
    Method(undefined);";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveIn("intParameter"));
        }

        [TestMethod]
        public void ProcessSimpleAssignment_GlobalScoped_NotLiveIn_NotLiveOut()
        {
            var code = @"
field = intParameter;
if (field == 0)
    Method(field);";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveIn("intParameter"));
        }

        [TestMethod]
        public void ProcessSimpleAssignment_LocalScoped_NotLiveIn_LiveOut()
        {
            var code = @"
int value;
value = 42;
if (value == 0)
    Method(value);";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveOut("value"));
        }

        [TestMethod]
        public void ProcessVariableDeclarator_NotLiveIn_LiveOut()
        {
            var code = @"
int intValue = 42;
var varValue = 42;
if (intValue == 0)
    Method(intValue, varValue);";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveOut("intValue", "varValue"));
        }

        [TestMethod]
        public void ProcessVariableInForeach_Declared_LiveIn_LiveOut()
        {
            /*
             * ForEach
             *    |
             * Binary <-----------+
             *  |   \ true        |
             *  v    \            |
             * Exit  Simple       |
             *       Method(i) -->+
             */
            var code = @"
foreach(var i in new int[] {1, 2, 3})
{
    Method(i, intParameter);
}";
            var context = new Context(code);
            context.Validate(context.Block<ForeachCollectionProducerBlock>(), new LiveIn("intParameter"), new LiveOut("intParameter"));
            context.Validate(context.Block<BinaryBranchBlock>(), new LiveIn("intParameter"), new LiveOut("intParameter", "i"));
            context.Validate(context.Block<SimpleBlock>(), new LiveIn("intParameter", "i"), new LiveOut("intParameter"));
        }

        [TestMethod]
        public void ProcessVariableInForeach_Reused_LiveIn_LiveOut()
        {
            var code = @"
int i = 42;
foreach(i in new int[] {1, 2, 3})
{
    Method(i, intParameter);
}";
            var context = new Context(code);
            context.Validate(context.Block<ForeachCollectionProducerBlock>(), new LiveIn("intParameter"), new LiveOut("intParameter", "i"));
            context.Validate(context.Block<BinaryBranchBlock>(), new LiveIn("intParameter", "i"), new LiveOut("intParameter", "i"));
            context.Validate(context.Block<SimpleBlock>(), new LiveIn("intParameter", "i"), new LiveOut("intParameter", "i"));
        }

        [TestMethod]
        public void StaticLocalFunction_ExpressionLiveIn()
        {
            var code = @"
outParameter = LocalFunction(intParameter);
static int LocalFunction(int a) => a + 1;";
            var context = new Context(code, "LocalFunction");
            context.Validate(context.Cfg.EntryBlock, new LiveIn("a"));
        }

        [TestMethod]
        public void StaticLocalFunction_ExpressionNotLiveIn()
        {
            var code = @"
outParameter = LocalFunction(0);
static int LocalFunction(int a) => 42;";
            var context = new Context(code, "LocalFunction");
            context.Validate(context.Cfg.EntryBlock);
        }

        [TestMethod]
        public void StaticLocalFunction_LiveIn()
        {
            var code = @"
outParameter = LocalFunction(intParameter);
static int LocalFunction(int a)
{
    return a + 1;
}";
            var context = new Context(code, "LocalFunction");
            context.Validate(context.Cfg.EntryBlock, new LiveIn("a"));
        }

        [TestMethod]
        public void StaticLocalFunction_NotLiveIn()
        {
            var code = @"
outParameter = LocalFunction(0);
static int LocalFunction(int a)
{
    return 42;
}";
            var context = new Context(code, "LocalFunction");
            context.Validate(context.Cfg.EntryBlock);
        }

        [TestMethod]
        public void StaticLocalFunction_Recursive()
        {
            var code = @"
outParameter = LocalFunction(intParameter);
static int LocalFunction(int a)
{
    if(a <= 0)
        return 0;
    else
        return LocalFunction(a - 1);
}";
            var context = new Context(code, "LocalFunction");
            context.Validate(context.Cfg.EntryBlock, new LiveIn("a"), new LiveOut("a"));
        }

        [TestMethod]
        public void UsingBlock_LiveInUntilTheEnd()
        {
            /*     Jump
             *     new ms
             *       |
             *     Binary
             *     Method(ms.Length)
             *      /  \
             * Simple   \
             * Method(0) |
             *      \   /
             *     Using
             *       |
             *     Exit
             */
            var code = @"
using (var ms = new System.IO.MemoryStream())
{
    Method(ms.Length);
    if (boolParameter)
        Method(0);
}";
            var context = new Context(code);
            context.Validate(context.Block<JumpBlock>(), new LiveIn("boolParameter"), new LiveOut("boolParameter", "ms"));
            context.Validate(context.Block<BinaryBranchBlock>(), new LiveIn("boolParameter", "ms"), new LiveOut("ms"));
            context.Validate(context.Block<SimpleBlock>(), new LiveIn("ms"), new LiveOut("ms"));
            context.Validate(context.Block<UsingEndBlock>(), new LiveIn("ms"));
            context.Validate(context.Block<ExitBlock>());
        }

        [TestMethod]
        public void UsingVar_NotSupported()
        {
            /* Bad CFG Shape
             *
             *     Binary
             *     ms = new
             *     Method(ms.Length)
             *      /  \
             * Simple   \
             * Method(0) |
             *      \   /
             *      Exit
             */
            var code = @"
using var ms = new System.IO.MemoryStream();
Method(ms.Length);
if (boolParameter)
    Method(0);";
            var context = new Context(code);
            context.Validate(context.Block<BinaryBranchBlock>(), new LiveIn("boolParameter"));
            context.Validate(context.Block<SimpleBlock>());
            context.Validate(context.Block<ExitBlock>());
        }

        [TestMethod]
        public void LocalFunctionInvocation_LiveIn()
        {
            var code = @"
var variable = 42;
if (boolParameter)
    return;
LocalFunction();

int LocalFunction() => variable;";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveIn("boolParameter"), new LiveOut("variable"));
            context.Validate(context.Block<BinaryBranchBlock>(), new LiveIn("boolParameter"), new LiveOut("variable")); // "variable" should also LiveIn
            context.Validate(context.Block<SimpleBlock>(), new LiveIn("variable"));
        }

        [TestMethod]
        public void LocalFunctionInvocation_Generic_LiveIn()
        {
            var code = @"
var variable = 42;
if (boolParameter)
    return;
LocalFunction<int>();

T LocalFunction<T>() => variable;";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveIn("boolParameter"), new LiveOut("variable"));
            context.Validate(context.Block<BinaryBranchBlock>(), new LiveIn("boolParameter"), new LiveOut("variable"));
            context.Validate(context.Block<SimpleBlock>(), new LiveIn("variable"));
        }

        [TestMethod]
        public void LocalFunctionInvocation_NotLiveIn()
        {
            var code = @"
var variable = 42;
if (boolParameter)
    return;
LocalFunction();
Method(variable);

void LocalFunction()
{
    variable = 0;
}";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveIn("boolParameter"));
            context.Validate(context.Block<BinaryBranchBlock>(), new LiveIn("boolParameter"));
            context.Validate(context.Block<SimpleBlock>());
        }

        private class Context
        {
            public readonly SonarCSharpLiveVariableAnalysis Lva;
            public readonly IControlFlowGraph Cfg;

            public Context(string methodBody, string localFunctionName = null)
            {
                var code = @$"
public class Sample
{{
    private int field;
    public int Property {{ get; set; }}

    public void Main(bool boolParameter, int intParameter, out int outParameter, ref int refParameter)
    {{
        {methodBody}
    }}

    private int Method(params int[] args) => 42;
    private string Method(params string[] args) => null;
    private bool IsMethod(params bool[] args) => true;
    private void Capturing(System.Func<int> f) {{ }}
}}";
                var method = SonarControlFlowGraphTest.CompileWithMethodBody(code, "Main", out var semanticModel);
                IMethodSymbol symbol;
                CSharpSyntaxNode body;
                if (localFunctionName == null)
                {
                    symbol = semanticModel.GetDeclaredSymbol(method);
                    body = method.Body;
                }
                else
                {
                    var function = (LocalFunctionStatementSyntaxWrapper)method.DescendantNodes()
                        .Single(x => x.Kind() == SyntaxKindEx.LocalFunctionStatement && ((LocalFunctionStatementSyntaxWrapper)x).Identifier.Text == localFunctionName);
                    symbol = semanticModel.GetDeclaredSymbol(function) as IMethodSymbol;
                    body = (CSharpSyntaxNode)function.Body ?? function.ExpressionBody;
                }
                Cfg = CSharpControlFlowGraph.Create(body, semanticModel);
                Console.WriteLine(CfgSerializer.Serialize(Cfg));
                Lva = new SonarCSharpLiveVariableAnalysis(Cfg, symbol, semanticModel);
            }

            public Block Block<TBlock>(string withInstruction = null) where TBlock : Block =>
                Cfg.Blocks.Single(x => x.GetType().Equals(typeof(TBlock)) && (withInstruction == null || x.Instructions.Any(instruction => instruction.ToString() == withInstruction)));

            public void Validate(Block block, params Expected[] expected)
            {
                // This is not very nice from OOP perspective, but it makes UTs above easy to read.
                var expectedLiveIn = expected.OfType<LiveIn>().SingleOrDefault() ?? new LiveIn();
                var expectedLiveOut = expected.OfType<LiveOut>().SingleOrDefault() ?? new LiveOut();
                var expectedCaptured = expected.OfType<Captured>().SingleOrDefault() ?? new Captured();
                Lva.LiveIn(block).Select(x => x.Name).Should().BeEquivalentTo(expectedLiveIn.Names, block.GetType().Name);
                Lva.LiveOut(block).Select(x => x.Name).Should().BeEquivalentTo(expectedLiveOut.Names, block.GetType().Name);
                Lva.CapturedVariables.Select(x => x.Name).Should().BeEquivalentTo(expectedCaptured.Names, block.GetType().Name);
            }
        }

        private abstract class Expected
        {
            public readonly string[] Names;

            protected Expected(string[] names) =>
                Names = names;
        }

        private class LiveIn : Expected
        {
            public LiveIn(params string[] names) : base(names) { }
        }

        private class LiveOut : Expected
        {
            public LiveOut(params string[] names) : base(names) { }
        }

        private class Captured : Expected
        {
            public Captured(params string[] names) : base(names) { }
        }
    }
}
