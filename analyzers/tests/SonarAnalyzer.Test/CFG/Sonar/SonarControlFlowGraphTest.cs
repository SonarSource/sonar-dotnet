﻿/*
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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.CFG;
using SonarAnalyzer.CFG.Sonar;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Test.CFG.Sonar
{
    [TestClass]
    public class SonarControlFlowGraphTest
    {
        private const string SimpleReturn = "return";
        private const string SimpleThrow = "throw";
        private const string SimpleYieldBreak = "yield break";
        private const string ExpressionReturn = "return ii";
        private const string ExpressionThrow = "throw ii";

        #region Top level - build CFG expression body / body

        [TestMethod]
        public void Cfg_Constructed_for_Body()
        {
            var cfg = Build("i = 4 + 5;");
            VerifyMinimalCfg(cfg);
        }

        [TestMethod]
        public void Cfg_Constructed_for_ExpressionBody()
        {
            const string input = @"
namespace NS
{
  public class Foo
  {
    public int Bar() => 4 + 5;
  }
}";
            var (method, model) = CompileWithMethodBody(input);
            var expression = method.ExpressionBody.Expression;
            var cfg = CSharpControlFlowGraph.Create(expression, model);
            VerifyMinimalCfg(cfg);
        }

        [TestMethod]
        public void Cfg_Constructed_for_Ctor_ComplexArguments()
        {
            const string input = @"
namespace NS
{
    public class Foo
    {
        private static int num = 10;

        public Foo() : this(true ? 5 : num + num)
        {
            var x = num;
        }

        public Foo(int i) {}
    }
}";
            var (tree, semanticModel) = TestHelper.CompileCS(input);
            var cfg = CSharpControlFlowGraph.Create(FirstConstructorBody(tree), semanticModel);

            VerifyCfg(cfg, 5);

            var blocks = cfg.Blocks.ToArray();

            var conditionalBlock = (BinaryBranchBlock)blocks[0];
            var trueCondition = blocks[1];
            var falseCondition = blocks[2];
            var constructorBody = blocks[3];
            var exit = cfg.ExitBlock;

            conditionalBlock.TrueSuccessorBlock.Should().Be(trueCondition);
            conditionalBlock.FalseSuccessorBlock.Should().Be(falseCondition);
            trueCondition.SuccessorBlocks.Should().Equal(constructorBody);
            falseCondition.SuccessorBlocks.Should().Equal(constructorBody);
            constructorBody.SuccessorBlocks.Should().Equal(exit);
            exit.PredecessorBlocks.Should().Equal(constructorBody);

            VerifyAllInstructions(conditionalBlock, "true");
            VerifyAllInstructions(trueCondition, "5");
            VerifyAllInstructions(falseCondition, "num", "num", "num + num");
            VerifyAllInstructions(constructorBody, ": this(true ? 5 : num + num)", "num", "x = num");
            VerifyAllInstructions(exit);
        }

        [TestMethod]
        public void Cfg_Constructed_for_Ctor_This()
        {
            const string input = @"
namespace NS
{
    public class Foo
    {
        public Foo() : this(5) {}

        public Foo(int i) {}
    }
}";
            var (tree, semanticModel) = TestHelper.CompileCS(input);
            var cfg = CSharpControlFlowGraph.Create(FirstConstructorBody(tree), semanticModel);

            VerifyCfg(cfg, 2);

            var blocks = cfg.Blocks.ToArray();

            var constructorBody = blocks[0];
            var exit = cfg.ExitBlock;

            constructorBody.SuccessorBlocks.Should().Equal(exit);
            exit.PredecessorBlocks.Should().Equal(constructorBody);

            VerifyAllInstructions(constructorBody, "5", ": this(5)");
            VerifyAllInstructions(exit);
        }

        [TestMethod]
        public void Cfg_Constructed_for_Ctor_Base()
        {
            const string input = @"
namespace NS
{
    public class Foo : Bar
    {
        public Foo() : base(5) {}
    }
    public class Bar
    {
        public Bar(int i) {}
    }
}";
            var (tree, semanticModel) = TestHelper.CompileCS(input);
            var cfg = CSharpControlFlowGraph.Create(FirstConstructorBody(tree), semanticModel);

            VerifyCfg(cfg, 2);

            var blocks = cfg.Blocks.ToArray();

            var constructorBody = blocks[0];
            var exit = cfg.ExitBlock;

            constructorBody.SuccessorBlocks.Should().Equal(exit);
            exit.PredecessorBlocks.Should().Equal(constructorBody);

            VerifyAllInstructions(constructorBody, "5", ": base(5)");
            VerifyAllInstructions(exit);
        }

        [TestMethod]
        public void Cfg_ExtremelyNestedExpression_NotSupported_FromExpression()
        {
            var (method, model) = CompileWithMethodBody(string.Format(TestInput, $"var x = {ExtremelyNestedExpression()};"));
            var equalsValueSyntax = method.DescendantNodes(x => !(x is ExpressionSyntax)).OfType<EqualsValueClauseSyntax>().Single();
            Action a = () => CSharpControlFlowGraph.Create(equalsValueSyntax.Value, model);

            a.Should().Throw<NotSupportedException>().WithMessage("Too complex expression");
            CSharpControlFlowGraph.TryGet(equalsValueSyntax.Value, model, out _).Should().BeFalse();
        }

        [TestMethod]
        public void Cfg_ExtremelyNestedExpression_NotSupported_FromBodyMethod()
        {
            var input = @$"
public class Sample
{{
    public string Main()
    {{
        return {ExtremelyNestedExpression()};
    }}
}}";
            var (tree, semanticModel) = TestHelper.CompileCS(input);
            var method = FirstMethod(tree);
            Action a = () => CSharpControlFlowGraph.Create(method.Body, semanticModel);

            a.Should().Throw<NotSupportedException>().WithMessage("Too complex expression");
            CSharpControlFlowGraph.TryGet(method.Body, semanticModel, out _).Should().BeFalse();
        }

        [TestMethod]
        public void Cfg_ExtremelyNestedExpression_NotSupported_FromArrowMethod()
        {
            var input = @$"
public class Sample
{{
    public string Main() =>{ExtremelyNestedExpression()};
}}";
            var (tree, semanticModel) = TestHelper.CompileCS(input);
            var method = FirstMethod(tree);
            Action a = () => CSharpControlFlowGraph.Create(method.ExpressionBody, semanticModel);

            a.Should().Throw<NotSupportedException>().WithMessage("Too complex expression");
            CSharpControlFlowGraph.TryGet(method.ExpressionBody, semanticModel, out _).Should().BeFalse();
        }

        [TestMethod]
        public void Cfg_ExtremelyNestedExpression_IsSupported_InSimpleLambda()
        {
            var input = @$"
public class Sample
{{
    public void Main() => Go(x => x + {ExtremelyNestedExpression()});

    public void Go(System.Func<string, string> arg) {{ }}
}}";
            var (tree, semanticModel) = TestHelper.CompileCS(input);
            var method = FirstMethod(tree);
            CSharpControlFlowGraph.Create(method.ExpressionBody, semanticModel).Should().NotBeNull();
            CSharpControlFlowGraph.TryGet(method, semanticModel, out _).Should().BeTrue();
        }

        [TestMethod]
        public void Cfg_ExtremelyNestedExpression_IsSupported_InParenthesizedLambda()
        {
            var input = @$"
public class Sample
{{
    public void Main() => Go(() => {ExtremelyNestedExpression()});

    public void Go(System.Func<string> arg) {{ }}
}}";
            var (tree, semanticModel) = TestHelper.CompileCS(input);
            var method = FirstMethod(tree);
            CSharpControlFlowGraph.Create(method.ExpressionBody, semanticModel).Should().NotBeNull();
            CSharpControlFlowGraph.TryGet(method, semanticModel, out _).Should().BeTrue();
        }

        #endregion

        #region Empty statement

        [TestMethod]
        public void Cfg_EmptyStatement()
        {
            var cfg = Build(";;;;;");
            VerifyEmptyCfg(cfg);
        }

        #endregion

        #region Variable declaration

        [TestMethod]
        public void Cfg_VariableDeclaration()
        {
            var cfg = Build("var x = 10, y = 11; var z = 12;");
            VerifyMinimalCfg(cfg);

            VerifyAllInstructions(cfg.EntryBlock, "10", "x = 10", "11", "y = 11", "12", "z = 12");
        }

        #endregion

        #region If statement

        [TestMethod]
        public void Cfg_If()
        {
            var cfg = Build("if (true) { var x = 10; }");

            VerifyCfg(cfg, 3);
            var branchBlock = cfg.EntryBlock as BinaryBranchBlock;
            var trueBlock = cfg.Blocks.ToList()[1];
            var exitBlock = cfg.ExitBlock;

            branchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { trueBlock, exitBlock });
            branchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            trueBlock.SuccessorBlocks.Should().Equal(exitBlock);
            exitBlock.PredecessorBlocks.Should().BeEquivalentTo(new[] { branchBlock, trueBlock });

            VerifyAllInstructions(branchBlock, "true");
            VerifyAllInstructions(trueBlock, "10", "x = 10");
        }

        [TestMethod]
        public void Cfg_If_Branch_Or_Condition()
        {
            var cfg = Build("if (a || b) { var x = 10; }");

            VerifyCfg(cfg, 4);

            var blocks = cfg.Blocks.ToList();

            var branchBlockA = (BinaryBranchBlock)blocks[0];
            var branchBlockB = (BinaryBranchBlock)blocks[1];
            var trueBlock = blocks[2];
            var exitBlock = cfg.ExitBlock;

            branchBlockA.TrueSuccessorBlock.Should().Be(trueBlock);
            branchBlockA.FalseSuccessorBlock.Should().Be(branchBlockB);
            branchBlockB.TrueSuccessorBlock.Should().Be(trueBlock);
            branchBlockB.FalseSuccessorBlock.Should().Be(exitBlock);
        }

        [TestMethod]
        public void Cfg_If_Branch_And_Condition()
        {
            var cfg = Build("if (a && b) { var x = 10; }");

            VerifyCfg(cfg, 4);

            var blocks = cfg.Blocks.ToList();

            var branchBlockA = (BinaryBranchBlock)blocks[0];
            var branchBlockB = (BinaryBranchBlock)blocks[1];
            var trueBlock = blocks[2];
            var exitBlock = cfg.ExitBlock;

            branchBlockA.TrueSuccessorBlock.Should().Be(branchBlockB);
            branchBlockA.FalseSuccessorBlock.Should().Be(exitBlock);
            branchBlockB.TrueSuccessorBlock.Should().Be(trueBlock);
            branchBlockB.FalseSuccessorBlock.Should().Be(exitBlock);
        }

        [TestMethod]
        public void Cfg_If_Branch_And_Condition_Parentheses()
        {
            var cfg = Build("if (((a && b))) { var x = 10; }");

            VerifyCfg(cfg, 4);

            var blocks = cfg.Blocks.ToList();

            var branchBlockA = (BinaryBranchBlock)blocks[0];
            var branchBlockB = (BinaryBranchBlock)blocks[1];
            var trueBlock = blocks[2];
            var exitBlock = cfg.ExitBlock;

            branchBlockA.TrueSuccessorBlock.Should().Be(branchBlockB);
            branchBlockA.FalseSuccessorBlock.Should().Be(exitBlock);
            branchBlockB.TrueSuccessorBlock.Should().Be(trueBlock);
            branchBlockB.FalseSuccessorBlock.Should().Be(exitBlock);
        }

        [TestMethod]
        public void Cfg_If_Else()
        {
            var cfg = Build("if (true) { var x = 10; } else { var y = 11; }");
            VerifyCfg(cfg, 4);
            var branchBlock = cfg.EntryBlock as BinaryBranchBlock;
            var trueBlock = cfg.Blocks.ToList()[1];
            var falseBlock = cfg.Blocks.ToList()[2];
            var exitBlock = cfg.ExitBlock;

            branchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { trueBlock, falseBlock });
            branchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            trueBlock.SuccessorBlocks.Should().Equal(exitBlock);
            falseBlock.SuccessorBlocks.Should().Equal(exitBlock);
            exitBlock.PredecessorBlocks.Should().BeEquivalentTo(new[] { trueBlock, falseBlock });
        }

        [TestMethod]
        public void Cfg_If_ElseIf()
        {
            var cfg = Build("if (true) { var x = 10; } else if (false) { var y = 11; }");
            VerifyCfg(cfg, 5);
            var firstCondition = cfg.EntryBlock as BinaryBranchBlock;
            var trueBlockX = cfg.Blocks.ToList()[1];
            var secondCondition = cfg.Blocks.ToList()[2] as BinaryBranchBlock;
            var trueBlockY = cfg.Blocks.ToList()[3];
            var exitBlock = cfg.ExitBlock;

            firstCondition.SuccessorBlocks.Should().BeEquivalentTo(new[] { trueBlockX, secondCondition });
            firstCondition.BranchingNode.Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            trueBlockX.SuccessorBlocks.Should().Equal(exitBlock);

            secondCondition.SuccessorBlocks.Should().BeEquivalentTo(new[] { trueBlockY, exitBlock });
            secondCondition.BranchingNode.Kind().Should().Be(SyntaxKind.FalseLiteralExpression);

            exitBlock.PredecessorBlocks.Should().BeEquivalentTo(new[] { trueBlockX, trueBlockY, secondCondition }, x => x.IgnoringCyclicReferences());
        }

        [TestMethod]
        public void Cfg_If_ElseIf_Else()
        {
            var cfg = Build("if (true) { var x = 10; } else if (false) { var y = 11; } else { var z = 12; }");
            VerifyCfg(cfg, 6);
            var firstCondition = cfg.EntryBlock as BinaryBranchBlock;
            var trueBlockX = cfg.Blocks.ToList()[1];
            var secondCondition = cfg.Blocks.ToList()[2] as BinaryBranchBlock;
            var trueBlockY = cfg.Blocks.ToList()[3];
            var falseBlockZ = cfg.Blocks.ToList()[4];
            var exitBlock = cfg.ExitBlock;

            firstCondition.SuccessorBlocks.Should().BeEquivalentTo(new[] { trueBlockX, secondCondition });
            firstCondition.BranchingNode.Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            trueBlockX.SuccessorBlocks.Should().Equal(exitBlock);

            secondCondition.SuccessorBlocks.Should().BeEquivalentTo(new[] { trueBlockY, falseBlockZ });
            secondCondition.BranchingNode.Kind().Should().Be(SyntaxKind.FalseLiteralExpression);

            trueBlockY.SuccessorBlocks.Should().Equal(exitBlock);
            falseBlockZ.SuccessorBlocks.Should().Equal(exitBlock);

            exitBlock.PredecessorBlocks.Should().BeEquivalentTo(new[] { trueBlockX, trueBlockY, falseBlockZ }, x => x.IgnoringCyclicReferences());
        }

        [TestMethod]
        public void Cfg_NestedIf()
        {
            var cfg = Build("if (true) { if (false) { var x = 10; } else { var y = 10; } }");
            VerifyCfg(cfg, 5);
            var firstCondition = cfg.EntryBlock as BinaryBranchBlock;
            firstCondition.Instructions.Should().Contain(n => n.IsKind(SyntaxKind.TrueLiteralExpression));

            var secondCondition = cfg.Blocks.ToList()[1] as BinaryBranchBlock;
            secondCondition.Instructions.Should().Contain(n => n.IsKind(SyntaxKind.FalseLiteralExpression));

            var trueBlockX = cfg.Blocks.ToList()[2];
            var falseBlockY = cfg.Blocks.ToList()[3];
            var exitBlock = cfg.ExitBlock;

            firstCondition.SuccessorBlocks.Should().BeEquivalentTo(new Block[] { secondCondition, exitBlock });
            firstCondition.BranchingNode.Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            secondCondition.SuccessorBlocks.Should().BeEquivalentTo(new[] { trueBlockX, falseBlockY });
            secondCondition.BranchingNode.Kind().Should().Be(SyntaxKind.FalseLiteralExpression);

            trueBlockX.SuccessorBlocks.Should().Equal(exitBlock);
            falseBlockY.SuccessorBlocks.Should().Equal(exitBlock);

            exitBlock.PredecessorBlocks.Should().BeEquivalentTo(new[] { trueBlockX, falseBlockY, firstCondition });
        }

        [TestMethod]
        public void Cfg_If_Coalesce()
        {
            var cfg = Build("if (a ?? b) { var y = 10; }");

            VerifyCfg(cfg, 5);

            var blocks = cfg.Blocks.ToList();

            var branchBlockA = (BinaryBranchBlock)blocks[0];
            var branchBlockALeft = (BranchBlock)blocks[1];
            var branchBlockB = (BinaryBranchBlock)blocks[2];
            var trueBlock = blocks[3];
            var exit = blocks[4];

            branchBlockA.TrueSuccessorBlock.Should().Be(branchBlockB);
            branchBlockA.FalseSuccessorBlock.Should().Be(branchBlockALeft);
            branchBlockALeft.SuccessorBlocks.Should().BeEquivalentTo(new[] { trueBlock, exit });
            branchBlockB.TrueSuccessorBlock.Should().Be(trueBlock);
            branchBlockB.FalseSuccessorBlock.Should().Be(exit);
            trueBlock.SuccessorBlocks.Should().Equal(exit);
        }

        [TestMethod]
        public void Cfg_Conditional_ComplexCondition_Coalesce()
        {
            var cfg = Build("var a = (x ?? y) ? b : c;");
            VerifyCfg(cfg, 7);

            var blocks = cfg.Blocks.ToList();
            var branchBlockX = (BinaryBranchBlock)blocks[0];
            var branchBlockXLeft = (BranchBlock)blocks[1];
            var branchBlockY = (BinaryBranchBlock)blocks[2];
            var condTrue = blocks[3];
            var condFalse = blocks[4];
            var after = blocks[5];
            var exitBlock = cfg.ExitBlock;

            branchBlockX.TrueSuccessorBlock.Should().Be(branchBlockY);
            branchBlockX.FalseSuccessorBlock.Should().Be(branchBlockXLeft);
            branchBlockXLeft.SuccessorBlocks.Should().BeEquivalentTo(new[] { condTrue, condFalse });
            branchBlockY.TrueSuccessorBlock.Should().Be(condTrue);
            branchBlockY.FalseSuccessorBlock.Should().Be(condFalse);

            condFalse.SuccessorBlocks.Should().Equal(after);
            condTrue.SuccessorBlocks.Should().Equal(after);
            after.SuccessorBlocks.Should().Equal(exitBlock);

            VerifyAllInstructions(branchBlockX, "x");
            VerifyAllInstructions(branchBlockXLeft);
            VerifyAllInstructions(branchBlockY, "y");
            VerifyAllInstructions(condTrue, "b");
            VerifyAllInstructions(condFalse, "c");
            VerifyAllInstructions(after, "a = (x ?? y) ? b : c");
        }

        [TestMethod]
        public void Cfg_If_Patterns_Constant_Complex_Condition()
        {
            var cfg = Build("cw0(); if (x is 10 && o is null) { cw1(); } cw2()");

            VerifyCfg(cfg, 5);
            var xBranchBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(0);
            var oBranchBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(1);
            var trueBlock = cfg.Blocks.ElementAt(2);
            var falseBlock = cfg.Blocks.ElementAt(3);
            var exitBlock = cfg.ExitBlock;

            xBranchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { oBranchBlock, falseBlock });
            xBranchBlock.BranchingNode.Kind().Should().Be(SyntaxKindEx.IsPatternExpression);
            VerifyAllInstructions(xBranchBlock, "cw0", "cw0()", "x", "10", "x is 10");

            oBranchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { trueBlock, falseBlock });
            oBranchBlock.BranchingNode.Kind().Should().Be(SyntaxKindEx.IsPatternExpression);
            VerifyAllInstructions(oBranchBlock, "o", "null", "o is null");

            trueBlock.SuccessorBlocks.Should().Equal(falseBlock);
            VerifyAllInstructions(trueBlock, "cw1", "cw1()");

            falseBlock.SuccessorBlocks.Should().Equal(exitBlock);
            VerifyAllInstructions(falseBlock, "cw2", "cw2()");

            exitBlock.PredecessorBlocks.Should().Equal(falseBlock);
        }

        [TestMethod]
        public void Cfg_If_Patterns_Single_Var_Complex_Condition()
        {
            var cfg = Build("cw0(); if (x is int i && o is string s) { cw1(); } cw2()");

            VerifyCfg(cfg, 5);
            var xBranchBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(0);
            var oBranchBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(1);
            var trueBlock = cfg.Blocks.ElementAt(2);
            var falseBlock = cfg.Blocks.ElementAt(3);
            var exitBlock = cfg.ExitBlock;

            xBranchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { oBranchBlock, falseBlock });
            xBranchBlock.BranchingNode.Kind().Should().Be(SyntaxKindEx.IsPatternExpression);
            VerifyAllInstructions(xBranchBlock, "cw0", "cw0()", "x", "x is int i");

            oBranchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { trueBlock, falseBlock });
            oBranchBlock.BranchingNode.Kind().Should().Be(SyntaxKindEx.IsPatternExpression);
            VerifyAllInstructions(oBranchBlock, "o", "o is string s");

            trueBlock.SuccessorBlocks.Should().Equal(falseBlock);
            VerifyAllInstructions(trueBlock, "cw1", "cw1()");

            falseBlock.SuccessorBlocks.Should().Equal(exitBlock);
            VerifyAllInstructions(falseBlock, "cw2", "cw2()");

            exitBlock.PredecessorBlocks.Should().Equal(falseBlock);
        }

        [TestMethod]
        public void Cfg_If_Patterns_Not_Null()
        {
            var cfg = Build("cw0(); if (!(x is null)) { cw1(); } cw2()");

            VerifyCfg(cfg, 4);
            var xBranchBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(0);
            var trueBlock = cfg.Blocks.ElementAt(1);
            var falseBlock = cfg.Blocks.ElementAt(2);
            var exitBlock = cfg.ExitBlock;

            xBranchBlock.TrueSuccessorBlock.Should().Be(trueBlock);
            xBranchBlock.FalseSuccessorBlock.Should().Be(falseBlock);
            xBranchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.LogicalNotExpression);
            VerifyAllInstructions(xBranchBlock, "cw0", "cw0()", "x", "null", "x is null", "!(x is null)");

            trueBlock.SuccessorBlocks.Should().Equal(falseBlock);
            VerifyAllInstructions(trueBlock, "cw1", "cw1()");

            falseBlock.SuccessorBlocks.Should().Equal(exitBlock);
            VerifyAllInstructions(falseBlock, "cw2", "cw2()");

            exitBlock.PredecessorBlocks.Should().Equal(falseBlock);
        }

        #endregion

        #region While statement

        [TestMethod]
        public void Cfg_While()
        {
            var cfg = Build("while (true) { var x = 10; }");
            VerifyCfg(cfg, 3);
            var branchBlock = cfg.EntryBlock as BinaryBranchBlock;
            var loopBodyBlock = cfg.Blocks
                .First(b => b.Instructions.Any(n => n.ToString() == "x = 10"));
            var exitBlock = cfg.ExitBlock;

            branchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { loopBodyBlock, exitBlock });
            branchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            loopBodyBlock.SuccessorBlocks.Should().Equal(branchBlock);
            branchBlock.PredecessorBlocks.Should().Equal(loopBodyBlock);
            exitBlock.PredecessorBlocks.Should().Equal(branchBlock);

            VerifyAllInstructions(branchBlock, "true");
            VerifyAllInstructions(loopBodyBlock, "10", "x = 10");
        }

        [TestMethod]
        public void Cfg_While_ComplexCondition_Or()
        {
            var cfg = Build("while (a || b) { var x = 10; }");
            VerifyCfg(cfg, 4);

            var blocks = cfg.Blocks.ToList();

            var branchBlockA = blocks[0] as BinaryBranchBlock;
            var branchBlockB = blocks[1] as BinaryBranchBlock;
            var loopBodyBlock = blocks[2];
            var exitBlock = cfg.ExitBlock;

            branchBlockA.TrueSuccessorBlock.Should().Be(loopBodyBlock);
            branchBlockA.FalseSuccessorBlock.Should().Be(branchBlockB);

            branchBlockB.TrueSuccessorBlock.Should().Be(loopBodyBlock);
            branchBlockB.FalseSuccessorBlock.Should().Be(exitBlock);

            loopBodyBlock.SuccessorBlocks.Should().Equal(branchBlockA);

            exitBlock.PredecessorBlocks.Should().Equal(branchBlockB);

            VerifyAllInstructions(branchBlockA, "a");
            VerifyAllInstructions(branchBlockB, "b");
            VerifyAllInstructions(loopBodyBlock, "10", "x = 10");
        }

        [TestMethod]
        public void Cfg_While_ComplexCondition_And()
        {
            var cfg = Build("while (a && b) { var x = 10; }");

            VerifyCfg(cfg, 4);

            var blocks = cfg.Blocks.ToList();

            var branchBlockA = blocks[0] as BinaryBranchBlock;
            var branchBlockB = blocks[1] as BinaryBranchBlock;
            var loopBodyBlock = blocks[2];
            var exitBlock = cfg.ExitBlock;

            branchBlockA.TrueSuccessorBlock.Should().Be(branchBlockB);
            branchBlockA.FalseSuccessorBlock.Should().Be(exitBlock);

            branchBlockB.TrueSuccessorBlock.Should().Be(loopBodyBlock);
            branchBlockB.FalseSuccessorBlock.Should().Be(exitBlock);

            loopBodyBlock.SuccessorBlocks.Should().Equal(branchBlockA);

            exitBlock.PredecessorBlocks.Should().BeEquivalentTo(new[] { branchBlockB, branchBlockA });

            VerifyAllInstructions(branchBlockA, "a");
            VerifyAllInstructions(branchBlockB, "b");
            VerifyAllInstructions(loopBodyBlock, "10", "x = 10");
        }

        [TestMethod]
        public void Cfg_NestedWhile()
        {
            var cfg = Build("while (true) while(false) { var x = 10; }");
            VerifyCfg(cfg, 4);
            var firstBranchBlock = cfg.EntryBlock as BinaryBranchBlock;
            firstBranchBlock.Instructions.Should().Contain(n => n.IsKind(SyntaxKind.TrueLiteralExpression));
            var blocks = cfg.Blocks.ToList();
            var loopBodyBlock = blocks
                .First(b => b.Instructions.Any(n => n.ToString() == "x = 10"));
            var secondBranchBlock = blocks[1] as BinaryBranchBlock;
            secondBranchBlock.Instructions.Should().Contain(n => n.IsKind(SyntaxKind.FalseLiteralExpression));
            var exitBlock = cfg.ExitBlock;

            firstBranchBlock.SuccessorBlocks.Should().BeEquivalentTo(new Block[] { secondBranchBlock, exitBlock });
            firstBranchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            secondBranchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { loopBodyBlock, firstBranchBlock });
            secondBranchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.FalseLiteralExpression);

            loopBodyBlock.SuccessorBlocks.Should().Equal(secondBranchBlock);
            firstBranchBlock.PredecessorBlocks.Should().Equal(secondBranchBlock);
            secondBranchBlock.PredecessorBlocks.Should().BeEquivalentTo(new[] { firstBranchBlock, loopBodyBlock });
            exitBlock.PredecessorBlocks.Should().Equal(firstBranchBlock);
        }

        #endregion

        #region Do statement

        [TestMethod]
        public void Cfg_DoWhile_ComplexCondition()
        {
            var cfg = Build("do { var x = 10; } while (a || b);");
            VerifyCfg(cfg, 4);

            var blocks = cfg.Blocks.ToList();

            var loopBodyBlock = blocks[0];
            var branchBlockA = (BinaryBranchBlock)blocks[1];
            var branchBlockB = (BinaryBranchBlock)blocks[2];
            var exitBlock = cfg.ExitBlock;

            loopBodyBlock.SuccessorBlocks.Should().Equal(branchBlockA);
            branchBlockA.TrueSuccessorBlock.Should().Be(loopBodyBlock);
            branchBlockA.FalseSuccessorBlock.Should().Be(branchBlockB);
            branchBlockB.TrueSuccessorBlock.Should().Be(loopBodyBlock);
            branchBlockB.FalseSuccessorBlock.Should().Be(exitBlock);

            exitBlock.PredecessorBlocks.Should().Equal(branchBlockB);

            VerifyAllInstructions(loopBodyBlock, "10", "x = 10");
        }

        [TestMethod]
        public void Cfg_DoWhile()
        {
            var cfg = Build("do { var x = 10; } while (true);");
            VerifyCfg(cfg, 3);
            var branchBlock = cfg.Blocks.ToList()[1] as BinaryBranchBlock;
            var loopBodyBlock = cfg.EntryBlock;
            var exitBlock = cfg.ExitBlock;

            loopBodyBlock.SuccessorBlocks.Should().Equal(branchBlock);

            branchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { loopBodyBlock, exitBlock });
            branchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            branchBlock.PredecessorBlocks.Should().Equal(loopBodyBlock);
            exitBlock.PredecessorBlocks.Should().Equal(new[] { branchBlock });

            VerifyAllInstructions(loopBodyBlock, "10", "x = 10");
            VerifyAllInstructions(branchBlock, "true");
        }

        [TestMethod]
        public void Cfg_NestedDoWhile()
        {
            var cfg = Build("do { do { var x = 10; } while (false); } while (true);");
            VerifyCfg(cfg, 4);
            var blocks = cfg.Blocks.ToList();
            var loopBodyBlock = cfg.EntryBlock;
            var falseBranchBlock = blocks[1] as BinaryBranchBlock;
            falseBranchBlock.Instructions.Should().Contain(n => n.IsKind(SyntaxKind.FalseLiteralExpression));
            var trueBranchBlock = blocks[2] as BinaryBranchBlock;
            trueBranchBlock.Instructions.Should().Contain(n => n.IsKind(SyntaxKind.TrueLiteralExpression));
            var exitBlock = cfg.ExitBlock;

            loopBodyBlock.SuccessorBlocks.Should().Equal(falseBranchBlock);

            falseBranchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { loopBodyBlock, trueBranchBlock });
            falseBranchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.FalseLiteralExpression);

            trueBranchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { loopBodyBlock, exitBlock });
            trueBranchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            falseBranchBlock.PredecessorBlocks.Should().Equal(loopBodyBlock);
            trueBranchBlock.PredecessorBlocks.Should().Equal(falseBranchBlock);
            exitBlock.PredecessorBlocks.Should().Equal(trueBranchBlock);
        }

        [TestMethod]
        public void Cfg_DoWhile_Continue()
        {
            var cfg = Build(@"
                int p;
                do
                {
                    p = unknown();
                    if (unknown())
                    {
                        p = 0;
                        continue;
                    }
                } while (!p);");

            VerifyCfg(cfg, 5);

            var blocks = cfg.Blocks.ToArray();

            var defBlock = blocks[0];
            var ifBlock = blocks[1] as BinaryBranchBlock;
            var continueJump = blocks[2] as JumpBlock;
            var doCondition = blocks[3] as BinaryBranchBlock;
            var exitBlock = blocks[4];

            defBlock.Should().Be(cfg.EntryBlock);
            defBlock.SuccessorBlocks.Should().Equal(ifBlock);
            VerifyAllInstructions(defBlock, "p");

            ifBlock.SuccessorBlocks.Should().BeEquivalentTo(new Block[] { continueJump, doCondition });
            ifBlock.BranchingNode.Kind().Should().Be(SyntaxKind.InvocationExpression);
            VerifyAllInstructions(ifBlock, "unknown", "unknown()", "p = unknown()", "unknown", "unknown()");

            continueJump.SuccessorBlocks.Should().Equal(doCondition);
            continueJump.JumpNode.Kind().Should().Be(SyntaxKind.ContinueStatement);
            VerifyAllInstructions(continueJump, "0", "p = 0");

            doCondition.SuccessorBlocks.Should().BeEquivalentTo(new[] { ifBlock, exitBlock });
            doCondition.BranchingNode.Kind().Should().Be(SyntaxKind.LogicalNotExpression);
            VerifyAllInstructions(doCondition, "p", "!p");

            exitBlock.Should().Be(cfg.ExitBlock);
        }

        #endregion

        #region Foreach statement

        [TestMethod]
        public void Cfg_Foreach()
        {
            var cfg = Build("foreach (var item in collection) { var x = 10; }");
            VerifyCfg(cfg, 4);
            var collectionBlock = cfg.EntryBlock as ForeachCollectionProducerBlock;
            collectionBlock.Should().NotBeNull();
            var blocks = cfg.Blocks.ToList();
            var foreachBlock = blocks[1] as BinaryBranchBlock;
            var loopBodyBlock = blocks
                .First(b => b.Instructions.Any(n => n.ToString() == "x = 10"));
            var exitBlock = cfg.ExitBlock;

            collectionBlock.SuccessorBlocks.Should().Contain(foreachBlock);

            foreachBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { loopBodyBlock, exitBlock });
            foreachBlock.BranchingNode.Kind().Should().Be(SyntaxKind.ForEachStatement);

            loopBodyBlock.SuccessorBlocks.Should().Equal(foreachBlock);
            exitBlock.PredecessorBlocks.Should().Equal(foreachBlock);

            VerifyAllInstructions(collectionBlock, "collection");
            VerifyNoInstruction(foreachBlock);
        }

        [TestMethod]
        public void Cfg_NestedForeach()
        {
            var cfg = Build("foreach (var item1 in collection1) { foreach (var item2 in collection2) { var x = 10; } }");
            VerifyCfg(cfg, 6);

            var blocks = cfg.Blocks.ToList();

            var collection1Block = cfg.EntryBlock;
            var foreach1Block = blocks[1] as BinaryBranchBlock;

            var collection2Block = blocks[2];
            var foreach2Block = blocks[3] as BinaryBranchBlock;

            var loopBodyBlock = blocks
                .First(b => b.Instructions.Any(n => n.ToString() == "x = 10"));

            var exitBlock = cfg.ExitBlock;

            collection1Block.Instructions.Should().Contain(n => n.ToString() == "collection1");
            collection2Block.Instructions.Should().Contain(n => n.ToString() == "collection2");

            collection1Block.SuccessorBlocks.Should().Contain(foreach1Block);

            foreach1Block.SuccessorBlocks.Should().BeEquivalentTo(new[] { collection2Block, exitBlock });
            foreach1Block.BranchingNode.Kind().Should().Be(SyntaxKind.ForEachStatement);

            collection2Block.SuccessorBlocks.Should().Contain(foreach2Block);

            foreach2Block.SuccessorBlocks.Should().BeEquivalentTo(new[] { loopBodyBlock, foreach1Block });
            foreach2Block.BranchingNode.Kind().Should().Be(SyntaxKind.ForEachStatement);

            loopBodyBlock.SuccessorBlocks.Should().Equal(foreach2Block);
            exitBlock.PredecessorBlocks.Should().Equal(foreach1Block);
        }

        [TestMethod]
        public void Cfg_Foreach_VarDeclaration()
        {
            var cfg = Build("foreach (var (key, value) in collection) { string j = value; } ");
            VerifyCfg(cfg, 4);

            var allBlocks = cfg.Blocks.ToList();

            var forEachCollectionBlock = cfg.EntryBlock as ForeachCollectionProducerBlock;

            forEachCollectionBlock.Should().NotBeNull();
            VerifyAllInstructions(forEachCollectionBlock, "collection");

            var foreachBlock = allBlocks[1] as BinaryBranchBlock;
            VerifyNoInstruction(foreachBlock);

            var loopBodyBlock = allBlocks[2] as SimpleBlock;
            VerifyAllInstructions(loopBodyBlock, "value", "j = value");

            var exitBlock = cfg.ExitBlock;

            forEachCollectionBlock.SuccessorBlocks.Should().Contain(foreachBlock);

            foreachBlock.SuccessorBlocks.Should().BeEquivalentTo(new Block[] { loopBodyBlock, exitBlock });
            foreachBlock.BranchingNode.Kind().Should().Be(SyntaxKind.ForEachVariableStatement);

            loopBodyBlock.SuccessorBlocks.Should().Equal(foreachBlock);

            exitBlock.PredecessorBlocks.Should().Equal(foreachBlock);
        }

        [TestMethod]
        public void Cfg_Foreach_AsyncStream()
        {
            var cfg = Build("await foreach (var number in GetAsync()) { var x = 10; }");

            VerifyCfg(cfg, 4);
            var collectionBlock = cfg.EntryBlock as ForeachCollectionProducerBlock;
            var blocks = cfg.Blocks.ToList();
            var foreachBlock = (BinaryBranchBlock)blocks[1];
            var loopBodyBlock = blocks[2];
            var exitBlock = cfg.ExitBlock;

            collectionBlock.SuccessorBlocks.Should().Contain(foreachBlock);
            VerifyAllInstructions(collectionBlock, "GetAsync", "GetAsync()");

            foreachBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { loopBodyBlock, exitBlock });
            foreachBlock.BranchingNode.Kind().Should().Be(SyntaxKind.ForEachStatement);
            VerifyNoInstruction(foreachBlock);

            loopBodyBlock.SuccessorBlocks.Should().Equal(foreachBlock);
            VerifyAllInstructions(loopBodyBlock, "10", "x = 10");

            exitBlock.PredecessorBlocks.Should().Equal(foreachBlock);
        }

        #endregion

        #region For statement

        [TestMethod]
        public void Cfg_For()
        {
            var cfg = Build("for (var i = 0; true; i++) { var x = 10; }");
            VerifyForStatement(cfg);
            VerifyAllInstructions(cfg.EntryBlock, "0", "i = 0");
            var condition = cfg.EntryBlock.SuccessorBlocks[0];
            VerifyAllInstructions(condition, "true");
            var body = condition.SuccessorBlocks[0];
            VerifyAllInstructions(condition.SuccessorBlocks[0], "10", "x = 10");
            VerifyAllInstructions(body.SuccessorBlocks[0], "i", "i++");

            cfg = Build("var y = 11; for (var i = 0; true; i++) { var x = 10; }");
            VerifyForStatement(cfg);

            cfg = Build("for (var i = 0; ; i++) { var x = 10; }");
            VerifyForStatement(cfg);

            cfg = Build("for (i = 0, j = 11; ; i++) { var x = 10; }");
            VerifyForStatement(cfg);
            cfg.EntryBlock.Should().BeAssignableTo<ForInitializerBlock>();
        }

        [TestMethod]
        public void Cfg_For_NoInitializer()
        {
            var cfg = Build("for (; true; i++) { var x = 10; }");
            VerifyForStatementNoInitializer(cfg);

            cfg = Build("for (; ; i++) { var x = 10; }");
            VerifyForStatementNoInitializer(cfg);
        }

        [TestMethod]
        public void Cfg_For_NoIncrementor()
        {
            var cfg = Build("for (var i = 0; true;) { var x = 10; }");
            VerifyForStatementNoIncrementor(cfg);

            cfg = Build("for (var i = 0; ;) { var x = 10; }");
            VerifyForStatementNoIncrementor(cfg);
        }

        [TestMethod]
        public void Cfg_For_Empty()
        {
            var cfg = Build("for (; true;) { var x = 10; }");
            VerifyForStatementEmpty(cfg);

            cfg = Build("for (;;) { var x = 10; }");
            VerifyForStatementEmpty(cfg);
        }

        [TestMethod]
        public void Cfg_NestedFor()
        {
            var cfg = Build("for (var i = 0; true; i++) { for (var j = 0; false; j++) { var x = 10; } }");
            VerifyCfg(cfg, 8);
            var initBlockI = cfg.EntryBlock;
            var blocks = cfg.Blocks.ToList();
            var branchBlockTrue = blocks
                .First(b => b.Instructions.Any(n => n.ToString() == "true")) as BinaryBranchBlock;
            var incrementorBlockI = blocks
                .First(b => b.Instructions.Any(n => n.ToString() == "i++"));

            var branchBlockFalse = blocks
                .First(b => b.Instructions.Any(n => n.ToString() == "false")) as BinaryBranchBlock;
            var incrementorBlockJ = blocks
                .First(b => b.Instructions.Any(n => n.ToString() == "j++"));
            var initBlockJ = blocks
                .First(b => b.Instructions.Any(n => n.ToString() == "j = 0"));

            var loopBodyBlock = blocks
                .First(b => b.Instructions.Any(n => n.ToString() == "x = 10"));
            var exitBlock = cfg.ExitBlock;

            initBlockI.SuccessorBlocks.Should().Equal(branchBlockTrue);

            branchBlockTrue.SuccessorBlocks.Should().BeEquivalentTo(new[] { initBlockJ, exitBlock });
            branchBlockTrue.BranchingNode.Kind().Should().Be(SyntaxKind.ForStatement);

            initBlockJ.SuccessorBlocks.Should().Equal(branchBlockFalse);

            branchBlockFalse.SuccessorBlocks.Should().BeEquivalentTo(new[] { loopBodyBlock, incrementorBlockI });
            branchBlockFalse.BranchingNode.Kind().Should().Be(SyntaxKind.ForStatement);

            loopBodyBlock.SuccessorBlocks.Should().Equal(incrementorBlockJ);
            incrementorBlockJ.SuccessorBlocks.Should().Equal(branchBlockFalse);
            incrementorBlockI.SuccessorBlocks.Should().Equal(branchBlockTrue);

            exitBlock.PredecessorBlocks.Should().Equal(branchBlockTrue);
        }

        #endregion

        #region Return, throw, yield break statement

        [TestMethod]
        public void Cfg_Return()
        {
            var cfg = Build($"if (true) {{ var y = 12; {SimpleReturn}; }} var x = 11;");
            VerifyJumpWithNoExpression(cfg, SyntaxKind.ReturnStatement);

            cfg = Build($"if (true) {{ {SimpleReturn}; }} var x = 11;");
            VerifyJumpWithNoExpression(cfg, SyntaxKind.ReturnStatement);
        }

        [TestMethod]
        public void Cfg_Throw_Statement_InvalidThrow()
        {
            var cfg = Build($"if (true) {{ var y = 12; {SimpleThrow}; }} var x = 11;");
            VerifyJumpWithNoExpression(cfg, SyntaxKind.ThrowStatement);

            cfg = Build($"if (true) {{ {SimpleThrow}; }} var x = 11;");
            VerifyJumpWithNoExpression(cfg, SyntaxKind.ThrowStatement);
        }

        [TestMethod]
        public void Cfg_Throw_Statement()
        {
            var throwException = "throw new InvalidOperationException(\"\")";
            var cfg = Build($"if (true) {{ var y = 12; {throwException}; }} var x = 11;");
            VerifyJumpWithNoExpression(cfg, SyntaxKind.ThrowStatement);

            cfg = Build($"if (true) {{ {throwException}; }} var x = 11;");
            VerifyJumpWithNoExpression(cfg, SyntaxKind.ThrowStatement);
        }

        [TestMethod]
        public void Cfg_YieldBreak()
        {
            var cfg = Build($"if (true) {{ var y = 12; {SimpleYieldBreak}; }} var x = 11;");
            VerifyJumpWithNoExpression(cfg, SyntaxKind.YieldBreakStatement);

            cfg = Build($"if (true) {{ {SimpleYieldBreak}; }} var x = 11;");
            VerifyJumpWithNoExpression(cfg, SyntaxKind.YieldBreakStatement);
        }

        [TestMethod]
        public void Cfg_Return_Value()
        {
            var cfg = Build($"if (true) {{ var y = 12; {ExpressionReturn}; }} var x = 11;");
            VerifyJumpWithExpression(cfg, SyntaxKind.ReturnStatement);
        }

        [TestMethod]
        public void Cfg_Return_JustBeforeExit()
        {
            var cfg = Build(@"
            return;
            cw0();
            return;");

            VerifyCfg(cfg, 3);

            var blocks = cfg.Blocks.ToList();

            var block1 = (JumpBlock)blocks[0];
            var block2 = (SimpleBlock)blocks[1];
            var exit = (ExitBlock)blocks.Last();

            block1.Instructions.Should().BeEmpty();
            block1.SuccessorBlocks.Should().Equal(exit);

            VerifyAllInstructions(block2, "cw0", "cw0()");
            block2.SuccessorBlocks.Should().Equal(exit);
        }

        [TestMethod]
        public void Cfg_Throw_Value()
        {
            var cfg = Build($"if (true) {{ var y = 12; {ExpressionThrow}; }} var x = 11;");
            VerifyJumpWithExpression(cfg, SyntaxKind.ThrowStatement);
        }

        #endregion

        #region Lock statement

        [TestMethod]
        public void Cfg_Lock()
        {
            var cfg = Build("lock(this) { var x = 10; }");

            VerifyCfg(cfg, 3);
            var lockBlock = cfg.EntryBlock as LockBlock;
            var bodyBlock = cfg.Blocks.Skip(1).First();
            var exitBlock = cfg.ExitBlock;

            lockBlock.SuccessorBlocks.Should().Equal(bodyBlock);
            bodyBlock.SuccessorBlocks.Should().Equal(exitBlock);

            lockBlock.LockNode.Kind().Should().Be(SyntaxKind.LockStatement);

            VerifyAllInstructions(cfg.EntryBlock, "this");
        }

        [TestMethod]
        public void Cfg_NestedLock()
        {
            var cfg = Build("lock(this) { lock(that) { var x = 10; }}");
            VerifyCfg(cfg, 4);
            var lockBlock = cfg.EntryBlock as LockBlock;
            var innerLockBlock = cfg.Blocks.Skip(1).First() as LockBlock;
            var bodyBlock = cfg.Blocks.Skip(2).First();
            var exitBlock = cfg.ExitBlock;

            lockBlock.SuccessorBlocks.Should().Equal(innerLockBlock);
            innerLockBlock.SuccessorBlocks.Should().Equal(bodyBlock);
            bodyBlock.SuccessorBlocks.Should().Equal(exitBlock);

            lockBlock.LockNode.Kind().Should().Be(SyntaxKind.LockStatement);
            lockBlock.Instructions.Should().Contain(n => n.IsKind(SyntaxKind.ThisExpression));

            innerLockBlock.LockNode.Kind().Should().Be(SyntaxKind.LockStatement);
            innerLockBlock.Instructions.Should().Contain(n => n.IsKind(SyntaxKind.IdentifierName) && n.ToString() == "that");
        }

        #endregion

        #region Using statement

        [TestMethod]
        public void Cfg_UsingDeclaration()
        {
            var cfg = Build("using(var stream = new MemoryStream()) { var x = 10; }");
            VerifySimpleJumpBlock(cfg, SyntaxKind.UsingStatement);

            VerifyAllInstructions(cfg.EntryBlock, "new MemoryStream()", "stream = new MemoryStream()");

            var usingBlock = cfg.Blocks.Skip(1).First() as UsingEndBlock;
            usingBlock.Should().NotBeNull();
            usingBlock.Identifiers.Select(n => n.ValueText).Should().Equal(new[] { "stream" });
            usingBlock.Should().BeOfType<UsingEndBlock>();
        }

        [TestMethod]
        public void Cfg_UsingAssignment()
        {
            var cfg = Build("Stream stream; using(stream = new MemoryStream()) { var x = 10; }");
            VerifySimpleJumpBlock(cfg, SyntaxKind.UsingStatement);

            VerifyAllInstructions(cfg.EntryBlock, "stream", "new MemoryStream()", "stream = new MemoryStream()");

            var usingBlock = cfg.Blocks.Skip(1).First() as UsingEndBlock;
            usingBlock.Should().NotBeNull();
            usingBlock.Identifiers.Select(n => n.ValueText).Should().Equal(new[] { "stream" });
            usingBlock.Should().BeOfType<UsingEndBlock>();
        }

        [TestMethod]
        public void Cfg_UsingExpression()
        {
            var cfg = Build("using(new MemoryStream()) { var x = 10; }");
            VerifySimpleJumpBlock(cfg, SyntaxKind.UsingStatement);

            VerifyAllInstructions(cfg.EntryBlock, "new MemoryStream()");
        }

        [TestMethod]
        public void Cfg_UsingLocalDeclaration()
        {
            var cfg = Build("using var stream = new MemoryStream();");

            VerifyCfg(cfg, 2);
            cfg.EntryBlock.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(cfg.EntryBlock, "new MemoryStream()", "stream = new MemoryStream()");
        }

        #endregion

        #region Fixed statement

        [TestMethod]
        public void Cfg_Fixed()
        {
            var cfg = Build("fixed (int* p = &pt.x) { *p = 1; }");
            VerifySimpleJumpBlock(cfg, SyntaxKind.FixedStatement);

            VerifyAllInstructions(cfg.EntryBlock, "pt", "pt.x", "&pt.x", "p = &pt.x");
        }

        #endregion

        #region Checked/unchecked statement

        [TestMethod]
        public void Cfg_Checked()
        {
            var cfg = Build("checked { var i = int.MaxValue + 1; }");
            VerifySimpleJumpBlock(cfg, SyntaxKind.CheckedStatement);

            VerifyNoInstruction(cfg.EntryBlock);
            VerifyInstructions(cfg.EntryBlock.SuccessorBlocks[0], 1, "int.MaxValue");

            cfg = Build("unchecked { var i = int.MaxValue + 1; }");
            VerifySimpleJumpBlock(cfg, SyntaxKind.UncheckedStatement);

            VerifyNoInstruction(cfg.EntryBlock);
            VerifyInstructions(cfg.EntryBlock.SuccessorBlocks[0], 1, "int.MaxValue");
        }

        #endregion

        #region Unsafe statement

        [TestMethod]
        public void Cfg_Unsafe()
        {
            var cfg = Build("unsafe { int* p = &i; *p *= *p; }");
            VerifySimpleJumpBlock(cfg, SyntaxKind.UnsafeStatement);

            VerifyNoInstruction(cfg.EntryBlock);
            VerifyInstructions(cfg.EntryBlock.SuccessorBlocks[0], 0, "i");
        }

        #endregion

        #region Logical && and ||

        [TestMethod]
        public void Cfg_LogicalAnd()
        {
            var cfg = Build("var b = a && c;");
            VerifyCfg(cfg, 4);

            var branchBlock = cfg.EntryBlock as BinaryBranchBlock;
            var blocks = cfg.Blocks.ToList();
            var trueABlock = blocks[1];
            var afterOp = blocks[2];
            var exitBlock = cfg.ExitBlock;

            branchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { trueABlock, afterOp });
            trueABlock.SuccessorBlocks.Should().Equal(afterOp);
            afterOp.SuccessorBlocks.Should().Equal(exitBlock);

            branchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.LogicalAndExpression);

            VerifyAllInstructions(branchBlock, "a");
            VerifyAllInstructions(trueABlock, "c");
            VerifyAllInstructions(afterOp, "b = a && c");
        }

        [TestMethod]
        public void Cfg_LogicalOr()
        {
            var cfg = Build("var b = a || c;");
            VerifyCfg(cfg, 4);

            var branchBlock = cfg.EntryBlock as BinaryBranchBlock;
            var blocks = cfg.Blocks.ToList();
            var falseABlock = blocks[1];
            var afterOp = blocks[2];
            var exitBlock = cfg.ExitBlock;

            branchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { afterOp, falseABlock });
            falseABlock.SuccessorBlocks.Should().Equal(afterOp);
            afterOp.SuccessorBlocks.Should().Equal(exitBlock);

            branchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.LogicalOrExpression);

            VerifyAllInstructions(branchBlock, "a");
            VerifyAllInstructions(falseABlock, "c");
            VerifyAllInstructions(afterOp, "b = a || c");
        }

        [TestMethod]
        public void Cfg_LogicalAnd_Multiple()
        {
            var cfg = Build("var b = a && c && d;");
            VerifyCfg(cfg, 6);

            var branchBlock = cfg.EntryBlock as BinaryBranchBlock;
            var blocks = cfg.Blocks.ToList();
            var trueABlock = blocks[1];
            var afterAC = blocks[2] as BinaryBranchBlock;
            var trueACBlock = blocks[3];
            var afterOp = blocks[4];
            var exitBlock = cfg.ExitBlock;

            branchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { trueABlock, afterAC });
            trueABlock.SuccessorBlocks.Should().Equal(afterAC);
            afterAC.SuccessorBlocks.Should().BeEquivalentTo(new[] { trueACBlock, afterOp });
            trueACBlock.SuccessorBlocks.Should().Equal(afterOp);
            afterOp.SuccessorBlocks.Should().Equal(exitBlock);
        }

        [TestMethod]
        public void Cfg_LogicalAnd_With_For()
        {
            var cfg = Build("for(x = 10; a && c; y++) { var z = 11; }");
            VerifyCfg(cfg, 7);

            var initBlock = cfg.EntryBlock;
            var blocks = cfg.Blocks.ToList();
            var aBlock = blocks
                .First(b => b.Instructions.Any(n => n.ToString() == "a")) as BinaryBranchBlock;
            var cBlock = blocks
                .First(b => b.Instructions.Any(n => n.ToString() == "c"));
            var acBlock = blocks[3] as BinaryBranchBlock;
            var bodyBlock = blocks
                .First(b => b.Instructions.Any(n => n.ToString() == "z = 11"));
            var incrementBlock = blocks
                .First(b => b.Instructions.Any(n => n.ToString() == "y++"));
            var exitBlock = cfg.ExitBlock;

            initBlock.SuccessorBlocks.Should().Equal(aBlock);
            aBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { cBlock, acBlock });
            cBlock.SuccessorBlocks.Should().Equal(acBlock);
            acBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { bodyBlock, exitBlock });
            bodyBlock.SuccessorBlocks.Should().Equal(incrementBlock);
            incrementBlock.SuccessorBlocks.Should().Equal(aBlock);

            acBlock.Instructions.Should().BeEmpty();
        }

        #endregion

        #region Coalesce expression

        [TestMethod]
        public void Cfg_Coalesce()
        {
            var cfg = Build("var a = b ?? c;");
            VerifyCfg(cfg, 4);
            var branchBlock = (BinaryBranchBlock)cfg.EntryBlock;
            var blocks = cfg.Blocks.ToList();
            var bNullBlock = blocks[1];
            var assignmentBlock = blocks[2];
            var exitBlock = cfg.ExitBlock;

            branchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { bNullBlock, assignmentBlock });
            bNullBlock.SuccessorBlocks.Should().Equal(assignmentBlock);
            assignmentBlock.SuccessorBlocks.Should().Equal(exitBlock);

            branchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.CoalesceExpression);

            VerifyAllInstructions(branchBlock, "b");
            VerifyAllInstructions(bNullBlock, "c");
            VerifyAllInstructions(assignmentBlock, "a = b ?? c");
        }

        [TestMethod]
        public void Cfg_Coalesce_Self()
        {
            var cfg = Build("a = a ?? c;");
            VerifyCfg(cfg, 4);

            var branchBlock = (BinaryBranchBlock)cfg.EntryBlock;
            var blocks = cfg.Blocks.ToList();
            var bNullBlock = blocks[1];
            var afterOp = blocks[2];
            var exitBlock = cfg.ExitBlock;

            branchBlock.TrueSuccessorBlock.Should().Be(bNullBlock);
            branchBlock.FalseSuccessorBlock.Should().Be(afterOp);
            branchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.CoalesceExpression);

            bNullBlock.SuccessorBlocks.Should().Equal(afterOp);

            afterOp.SuccessorBlocks.Should().Equal(exitBlock);

            VerifyAllInstructions(branchBlock, "a");
            VerifyAllInstructions(bNullBlock, "c");
            VerifyAllInstructions(afterOp, "a = a ?? c");
        }

        [TestMethod]
        public void Cfg_Coalesce_Multiple()
        {
            var cfg = Build("var a = b ?? c ?? d;");
            VerifyCfg(cfg, 5);

            var bBlock = (BinaryBranchBlock)cfg.EntryBlock;
            var blocks = cfg.Blocks.ToList();
            var cBlock = (BinaryBranchBlock)blocks[1];
            var dBlock = blocks[2];
            var bcdBlock = blocks[3];   // b ?? c ?? d
            var exitBlock = cfg.ExitBlock;

            bBlock.TrueSuccessorBlock.Should().Be(cBlock);
            bBlock.FalseSuccessorBlock.Should().Be(bcdBlock);
            bBlock.Instructions.Should().ContainSingle("b");
            bBlock.BranchingNode.Kind().Should().Be(SyntaxKind.CoalesceExpression);

            cBlock.TrueSuccessorBlock.Should().Be(dBlock);
            cBlock.FalseSuccessorBlock.Should().Be(bcdBlock);
            cBlock.Instructions.Should().ContainSingle("c");
            cBlock.BranchingNode.Kind().Should().Be(SyntaxKind.CoalesceExpression);

            dBlock.SuccessorBlocks.Should().Equal(bcdBlock);
            dBlock.Instructions.Should().ContainSingle("d");

            bcdBlock.SuccessorBlocks.Should().Equal(exitBlock);
            bcdBlock.Instructions.Should().ContainSingle("a = b ?? c ?? d");
        }

        [TestMethod]
        public void Cfg_Coalesce_MultipleAssignments()
        {
            var cfg = Build("a = a ?? (b = b ?? c);");
            VerifyCfg(cfg, 6);

            var firstBranchBlockWithA = (BinaryBranchBlock)cfg.EntryBlock;
            var blocks = cfg.Blocks.ToList();
            var secondBranchBlockWithB = (BinaryBranchBlock)blocks[1];
            var simpleBlockWithC = (SimpleBlock)blocks[2];
            var simpleBlockWithAssignmentToB = (SimpleBlock)blocks[3];
            var simpleBlockWithFullExpression = (SimpleBlock)blocks[4];
            var exitBlock = cfg.ExitBlock;

            firstBranchBlockWithA.TrueSuccessorBlock.Should().Be(secondBranchBlockWithB);
            firstBranchBlockWithA.FalseSuccessorBlock.Should().Be(simpleBlockWithFullExpression);
            firstBranchBlockWithA.Instructions.Should().ContainSingle("a");
            firstBranchBlockWithA.BranchingNode.Kind().Should().Be(SyntaxKind.CoalesceExpression);

            secondBranchBlockWithB.TrueSuccessorBlock.Should().Be(simpleBlockWithC);
            secondBranchBlockWithB.FalseSuccessorBlock.Should().Be(simpleBlockWithAssignmentToB);
            secondBranchBlockWithB.Instructions.Should().ContainSingle("b");
            secondBranchBlockWithB.BranchingNode.Kind().Should().Be(SyntaxKind.CoalesceExpression);

            simpleBlockWithC.SuccessorBlock.Should().Be(simpleBlockWithAssignmentToB);
            simpleBlockWithC.Instructions.Should().ContainSingle("c");

            simpleBlockWithAssignmentToB.SuccessorBlock.Should().Be(simpleBlockWithFullExpression);
            simpleBlockWithAssignmentToB.Instructions.Should().ContainSingle("b = b ?? c");

            simpleBlockWithFullExpression.SuccessorBlock.Should().Be(exitBlock);
            simpleBlockWithFullExpression.Instructions.Should().ContainSingle("a = a ?? (b = b ?? c)");
        }

        [TestMethod]
        public void Cfg_Coalesce_Throw()
        {
            var cfg = Build(@"var a = b ?? throw new Exception(""Test"");");
            VerifyCfg(cfg, 4);
            var branchBlock = (BinaryBranchBlock)cfg.EntryBlock;
            var blocks = cfg.Blocks.ToList();
            var throwBlock = blocks[1];
            var assignmentBlock = blocks[2];
            var exitBlock = cfg.ExitBlock;

            branchBlock.TrueSuccessorBlock.Should().Be(throwBlock);
            branchBlock.FalseSuccessorBlock.Should().Be(assignmentBlock);
            throwBlock.SuccessorBlocks.Should().Equal(exitBlock);
            assignmentBlock.SuccessorBlocks.Should().Equal(exitBlock);

            branchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.CoalesceExpression);

            VerifyAllInstructions(branchBlock, "b");
            VerifyAllInstructions(throwBlock, @"""Test""", @"new Exception(""Test"")");
            VerifyAllInstructions(assignmentBlock, @"a = b ?? throw new Exception(""Test"")");
        }

        [TestMethod]
        public void Cfg_Coalesce_ThrowCoalesce()
        {
            var cfg = Build(@"var a = b ?? throw ex ?? new Exception(""Test"");");
            VerifyCfg(cfg, 6);
            var blocks = cfg.Blocks.ToList();
            var firstBranchBlockWithB = (BinaryBranchBlock)cfg.EntryBlock;
            var secondBranchBlockWithEx = (BinaryBranchBlock)blocks[1];
            var simpleBlockWithException = (SimpleBlock)blocks[2];
            var jumpBlockThrow = (JumpBlock)blocks[3];
            var simpleBlockWithFullExpression = (SimpleBlock)blocks[4];
            var exitBlock = cfg.ExitBlock;

            firstBranchBlockWithB.TrueSuccessorBlock.Should().Be(secondBranchBlockWithEx);
            firstBranchBlockWithB.FalseSuccessorBlock.Should().Be(simpleBlockWithFullExpression);
            firstBranchBlockWithB.Instructions.Should().ContainSingle("b");
            firstBranchBlockWithB.BranchingNode.Kind().Should().Be(SyntaxKind.CoalesceExpression);

            secondBranchBlockWithEx.TrueSuccessorBlock.Should().Be(simpleBlockWithException);
            secondBranchBlockWithEx.FalseSuccessorBlock.Should().Be(jumpBlockThrow);
            secondBranchBlockWithEx.Instructions.Should().ContainSingle("ex");
            secondBranchBlockWithEx.BranchingNode.Kind().Should().Be(SyntaxKind.CoalesceExpression);

            simpleBlockWithException.SuccessorBlock.Should().Be(jumpBlockThrow);
            VerifyAllInstructions(simpleBlockWithException, @"""Test""", @"new Exception(""Test"")");

            jumpBlockThrow.SuccessorBlock.Should().Be(exitBlock);

            simpleBlockWithFullExpression.SuccessorBlock.Should().Be(exitBlock);
            simpleBlockWithFullExpression.Instructions.Should().ContainSingle(@"a = b ?? throw ex ?? new Exception(""Test"")");
        }

        #endregion

        #region Null-coalescing assigment

        [TestMethod]
        public void Cfg_NullCoalescingAssignment()
        {
            // is similar with "a = a ?? b;"
            /// <see cref="Cfg_Coalesce_Self"/>
            var cfg = Build("a ??= b;");
            VerifyCfg(cfg, 4);

            var branchBlock = (BinaryBranchBlock)cfg.EntryBlock;
            var blocks = cfg.Blocks.ToList();
            var blockWithB = blocks[1];
            var assignmentBlock = blocks[2];
            var exitBlock = cfg.ExitBlock;

            branchBlock.TrueSuccessorBlock.Should().Be(blockWithB);
            branchBlock.FalseSuccessorBlock.Should().Be(assignmentBlock);
            branchBlock.BranchingNode.Kind().Should().Be(SyntaxKindEx.CoalesceAssignmentExpression);
            VerifyAllInstructions(branchBlock, "a");

            blockWithB.SuccessorBlocks.Should().Equal(assignmentBlock);
            VerifyAllInstructions(blockWithB, "b");

            assignmentBlock.SuccessorBlocks.Should().Equal(exitBlock);
            VerifyAllInstructions(assignmentBlock, "a ??= b");
        }

        [TestMethod]
        public void Cfg_NullCoalescingAssignment_Multiple()
        {
            // is similar with "a = a ?? (b = b ?? c);"
            /// <see cref="Cfg_Coalesce_MultipleAssignments"/>
            var cfg = Build("a ??= b ??= c;");
            VerifyCfg(cfg, 6);

            var firstBranchBlockWithA = (BinaryBranchBlock)cfg.EntryBlock;
            var blocks = cfg.Blocks.ToList();
            var secondBranchBlockWithB = (BinaryBranchBlock)blocks[1];
            var simpleBlockWithC = (SimpleBlock)blocks[2];
            var simpleBlockWithAssignmentToB = (SimpleBlock)blocks[3];
            var simpleBlockWithFullExpression = (SimpleBlock)blocks[4];
            var exitBlock = cfg.ExitBlock;

            firstBranchBlockWithA.TrueSuccessorBlock.Should().Be(secondBranchBlockWithB);
            firstBranchBlockWithA.FalseSuccessorBlock.Should().Be(simpleBlockWithFullExpression);
            firstBranchBlockWithA.Instructions.Should().ContainSingle("a");
            firstBranchBlockWithA.BranchingNode.Kind().Should().Be(SyntaxKindEx.CoalesceAssignmentExpression);

            secondBranchBlockWithB.TrueSuccessorBlock.Should().Be(simpleBlockWithC);
            secondBranchBlockWithB.FalseSuccessorBlock.Should().Be(simpleBlockWithAssignmentToB);
            secondBranchBlockWithB.Instructions.Should().ContainSingle("b");
            secondBranchBlockWithB.BranchingNode.Kind().Should().Be(SyntaxKindEx.CoalesceAssignmentExpression);

            simpleBlockWithC.SuccessorBlock.Should().Be(simpleBlockWithAssignmentToB);
            simpleBlockWithC.Instructions.Should().ContainSingle("c");

            simpleBlockWithAssignmentToB.SuccessorBlock.Should().Be(simpleBlockWithFullExpression);
            simpleBlockWithAssignmentToB.Instructions.Should().ContainSingle("b = b ?? c");

            simpleBlockWithFullExpression.SuccessorBlock.Should().Be(exitBlock);
            simpleBlockWithFullExpression.Instructions.Should().ContainSingle("a = a ?? (b = b ?? c)");
        }

        [TestMethod]
        public void Cfg_NullCoalescingAssignment_Coalesce()
        {
            // similar to a = a ?? b ?? c
            var cfg = Build("a ??= b ?? c;");
            VerifyCfg(cfg, 5);

            var coalesceAssignmentBranchBlock = (BinaryBranchBlock)cfg.EntryBlock;
            var blocks = cfg.Blocks.ToList();
            var coalesceBranchBlock = (BinaryBranchBlock)blocks[1];
            var blockWithC = (SimpleBlock)blocks[2];
            var assignmentBlock = blocks[3];
            var exitBlock = cfg.ExitBlock;

            coalesceAssignmentBranchBlock.TrueSuccessorBlock.Should().Be(coalesceBranchBlock);
            coalesceAssignmentBranchBlock.FalseSuccessorBlock.Should().Be(assignmentBlock);
            coalesceAssignmentBranchBlock.BranchingNode.Kind().Should().Be(SyntaxKindEx.CoalesceAssignmentExpression);
            coalesceAssignmentBranchBlock.Instructions.Should().ContainSingle("a");

            coalesceBranchBlock.TrueSuccessorBlock.Should().Be(blockWithC);
            coalesceBranchBlock.FalseSuccessorBlock.Should().Be(assignmentBlock);
            coalesceBranchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.CoalesceExpression);
            coalesceBranchBlock.Instructions.Should().ContainSingle("b");

            blockWithC.SuccessorBlocks.Should().Equal(assignmentBlock);
            blockWithC.Instructions.Should().ContainSingle("c");

            assignmentBlock.SuccessorBlocks.Should().Equal(exitBlock);
            assignmentBlock.Instructions.Should().ContainSingle("a ??= b ?? c");
        }

        [TestMethod]
        public void Cfg_NullCoalescingAssignment_Conditional()
        {
            var cfg = Build("a ??= b ? c : d;");
            VerifyCfg(cfg, 6);

            var nullCoalesceAssignmentBranchBlock = (BinaryBranchBlock)cfg.EntryBlock;
            var blocks = cfg.Blocks.ToList();
            var conditionalBranchBlock = (BinaryBranchBlock)blocks[1];
            var cBlock = (SimpleBlock)blocks[2];
            var dBlock = (SimpleBlock)blocks[3];
            var simpleBlockWithFullExpression = (SimpleBlock)blocks[4];
            var exitBlock = cfg.ExitBlock;

            nullCoalesceAssignmentBranchBlock.TrueSuccessorBlock.Should().Be(conditionalBranchBlock);
            nullCoalesceAssignmentBranchBlock.FalseSuccessorBlock.Should().Be(simpleBlockWithFullExpression);
            nullCoalesceAssignmentBranchBlock.Instructions.Should().ContainSingle("a");
            nullCoalesceAssignmentBranchBlock.BranchingNode.Kind().Should().Be(SyntaxKindEx.CoalesceAssignmentExpression);

            conditionalBranchBlock.TrueSuccessorBlock.Should().Be(cBlock);
            conditionalBranchBlock.FalseSuccessorBlock.Should().Be(dBlock);
            conditionalBranchBlock.Instructions.Should().ContainSingle("b");
            conditionalBranchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.IdentifierName);

            cBlock.SuccessorBlock.Should().Be(simpleBlockWithFullExpression);
            cBlock.Instructions.Should().ContainSingle("c");

            dBlock.SuccessorBlock.Should().Be(simpleBlockWithFullExpression);
            dBlock.Instructions.Should().ContainSingle("d");

            simpleBlockWithFullExpression.SuccessorBlock.Should().Be(exitBlock);
            simpleBlockWithFullExpression.Instructions.Should().HaveCount(1);
            simpleBlockWithFullExpression.Instructions.Should().ContainSingle("a ??= b ? c : d");
        }

        #endregion

        #region Conditional expression

        [TestMethod]
        public void Cfg_Conditional()
        {
            var cfg = Build("var a = cond ? b : c;");
            VerifyCfg(cfg, 5);

            var branchBlock = cfg.EntryBlock as BinaryBranchBlock;
            var blocks = cfg.Blocks.ToList();
            var condFalse = blocks[2];
            var condTrue = blocks[1];
            var after = blocks[3];
            var exitBlock = cfg.ExitBlock;

            branchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { condTrue, condFalse });
            condFalse.SuccessorBlocks.Should().Equal(after);
            condTrue.SuccessorBlocks.Should().Equal(after);
            after.SuccessorBlocks.Should().Equal(exitBlock);

            branchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.IdentifierName);

            VerifyAllInstructions(branchBlock, "cond");
            VerifyAllInstructions(condTrue, "b");
            VerifyAllInstructions(condFalse, "c");
            VerifyAllInstructions(after, "a = cond ? b : c");
        }

        [TestMethod]
        public void Cfg_Conditional_ComplexCondition_Or()
        {
            var cfg = Build("var a = x || y ? b : c;");
            VerifyCfg(cfg, 6);

            var blocks = cfg.Blocks.ToList();
            var branchBlockA = (BinaryBranchBlock)blocks[0];
            var branchBlockB = (BinaryBranchBlock)blocks[1];
            var condFalse = blocks[3];
            var condTrue = blocks[2];
            var after = blocks[4];
            var exitBlock = cfg.ExitBlock;

            branchBlockA.TrueSuccessorBlock.Should().Be(condTrue);
            branchBlockA.FalseSuccessorBlock.Should().Be(branchBlockB);
            branchBlockB.TrueSuccessorBlock.Should().Be(condTrue);
            branchBlockB.FalseSuccessorBlock.Should().Be(condFalse);

            condFalse.SuccessorBlocks.Should().Equal(after);
            condTrue.SuccessorBlocks.Should().Equal(after);
            after.SuccessorBlocks.Should().Equal(exitBlock);

            VerifyAllInstructions(branchBlockA, "x");
            VerifyAllInstructions(branchBlockB, "y");
            VerifyAllInstructions(condTrue, "b");
            VerifyAllInstructions(condFalse, "c");
            VerifyAllInstructions(after, "a = x || y ? b : c");
        }

        [TestMethod]
        public void Cfg_Conditional_ComplexCondition_And()
        {
            var cfg = Build("var a = x && y ? b : c;");
            VerifyCfg(cfg, 6);

            var blocks = cfg.Blocks.ToList();
            var branchBlockA = (BinaryBranchBlock)blocks[0];
            var branchBlockB = (BinaryBranchBlock)blocks[1];
            var condFalse = blocks[3];
            var condTrue = blocks[2];
            var after = blocks[4];
            var exitBlock = cfg.ExitBlock;

            branchBlockA.TrueSuccessorBlock.Should().Be(branchBlockB);
            branchBlockA.FalseSuccessorBlock.Should().Be(condFalse);
            branchBlockB.TrueSuccessorBlock.Should().Be(condTrue);
            branchBlockB.FalseSuccessorBlock.Should().Be(condFalse);

            condFalse.SuccessorBlocks.Should().Equal(after);
            condTrue.SuccessorBlocks.Should().Equal(after);
            after.SuccessorBlocks.Should().Equal(exitBlock);

            VerifyAllInstructions(branchBlockA, "x");
            VerifyAllInstructions(branchBlockB, "y");
            VerifyAllInstructions(condTrue, "b");
            VerifyAllInstructions(condFalse, "c");
            VerifyAllInstructions(after, "a = x && y ? b : c");
        }

        [TestMethod]
        public void Cfg_ConditionalMultiple()
        {
            var cfg = Build("var a = cond1 ? (cond2?x:y) : (cond3?p:q);");
            VerifyCfg(cfg, 9);

            var branchBlock = cfg.EntryBlock as BinaryBranchBlock;
            var blocks = cfg.Blocks.ToList();
            var cond2Block = blocks[1];
            VerifyAllInstructions(cond2Block, "cond2");
            var cond3Block = blocks[4];
            VerifyAllInstructions(cond3Block, "cond3");

            branchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { cond2Block, cond3Block });
            cond2Block.SuccessorBlocks.Should().HaveCount(2);
            cond3Block.SuccessorBlocks.Should().HaveCount(2);

            cond2Block
                .SuccessorBlocks[0]
                .SuccessorBlocks[0]
                .SuccessorBlocks[0].Should().Be(cfg.ExitBlock);

            var assignmentBlock = cfg.ExitBlock.PredecessorBlocks.First();
            assignmentBlock.Instructions.Should().HaveCount(1);
            assignmentBlock.Instructions.Should().Contain(i => i.ToString() == "a = cond1 ? (cond2?x:y) : (cond3?p:q)");
        }

        #endregion

        #region Throw expression

        [TestMethod]
        public void Cfg_Throw_Expression_NullCoalesce()
        {
            var throwException = "throw new InvalidOperationException(\"\")";
            var cfg = Build($"object x = null; var y = x ?? {throwException};");
            VerifyJumpWithNoExpression(cfg, SyntaxKind.ThrowExpression);
        }

        [TestMethod]
        public void Cfg_Throw_Expression_Ternary()
        {
            var throwException = "throw new InvalidOperationException(\"\")";
            var cfg = Build($"var x = true ? 1 : {throwException};");

            VerifyCfg(cfg, 5);
            var binaryBranch = cfg.EntryBlock as BinaryBranchBlock;
            var blocks = cfg.Blocks.ToList();
            var trueBlock = blocks[1] as SimpleBlock;
            var falseJumpBlock = blocks[2] as JumpBlock;
            var assignmentBlock = blocks[3] as SimpleBlock;
            var exitBlock = cfg.ExitBlock;

            binaryBranch.SuccessorBlocks.Should().BeEquivalentTo(new[] { trueBlock, falseJumpBlock });
            trueBlock.SuccessorBlocks.Should().Equal(assignmentBlock);
            falseJumpBlock.SuccessorBlock.Should().Be(exitBlock);
            falseJumpBlock.WouldBeSuccessor.Should().Be(assignmentBlock);
            assignmentBlock.SuccessorBlocks.Should().Equal(exitBlock);
            exitBlock.PredecessorBlocks.Should().BeEquivalentTo(new[] { falseJumpBlock, assignmentBlock });
        }

        [TestMethod]
        public void Cfg_Throw_Expression_MethodArgument()
        {
            var throwException = "throw new InvalidOperationException(\"\")";
            var cfg = Build($"Console.WriteLine({throwException});");

            VerifyCfg(cfg, 3);
            var jumpBlock = cfg.EntryBlock as JumpBlock;
            var blocks = cfg.Blocks.ToList();
            var methodCallBlock = blocks[1] as SimpleBlock;
            var exitBlock = cfg.ExitBlock;

            jumpBlock.SuccessorBlocks.Should().Equal(exitBlock);
            jumpBlock.WouldBeSuccessor.Should().Be(methodCallBlock);
            methodCallBlock.SuccessorBlocks.Should().Equal(exitBlock);
            exitBlock.PredecessorBlocks.Should().BeEquivalentTo(new[] { methodCallBlock, jumpBlock });
        }

        #endregion

        #region Ranges and Indices

        [TestMethod]
        public void Cfg_Range_Expression()
        {
            var cfg = Build($"Range r = 1..4;");

            VerifyCfg(cfg, 2);

            var rangeBlock = cfg.EntryBlock as SimpleBlock;
            var exitBlock = cfg.ExitBlock;

            VerifyAllInstructions(rangeBlock, "1..4", "r = 1..4");

            rangeBlock.SuccessorBlocks.Should().Equal(exitBlock);
            exitBlock.PredecessorBlocks.Should().Equal(rangeBlock);
        }

        [TestMethod]
        public void Cfg_Index_Expression()
        {
            var cfg = Build($"Index index = ^1;");

            VerifyCfg(cfg, 2);

            var rangeBlock = cfg.EntryBlock as SimpleBlock;
            var exitBlock = cfg.ExitBlock;

            VerifyAllInstructions(rangeBlock, "^1", "index = ^1");

            rangeBlock.SuccessorBlocks.Should().Equal(exitBlock);
            exitBlock.PredecessorBlocks.Should().Equal(rangeBlock);
        }

        #endregion

        #region Conditional access

        [TestMethod]
        public void Cfg_ConditionalAccess()
        {
            var cfg = Build("var a = o?.method(1);");
            VerifyCfg(cfg, 4);

            var blocks = cfg.Blocks.ToList();
            var oIsNullBranch = blocks[0] as BinaryBranchBlock;
            var oNotNull = blocks[1];
            var assignment = blocks[2];
            var exitBlock = cfg.ExitBlock;

            oIsNullBranch.TrueSuccessorBlock.Should().Be(assignment);
            oIsNullBranch.FalseSuccessorBlock.Should().Be(oNotNull);
            oNotNull.SuccessorBlocks.Should().Equal(assignment);
            assignment.SuccessorBlocks.Should().Equal(exitBlock);

            oIsNullBranch.BranchingNode.Kind().Should().Be(SyntaxKind.ConditionalAccessExpression);

            VerifyAllInstructions(oIsNullBranch, "o");
            VerifyAllInstructions(oNotNull, "method", ".method" /* This is equivalent to o.method */, "1", ".method(1)");
            VerifyAllInstructions(assignment, "a = o?.method(1)");
        }

        [TestMethod]
        public void Cfg_ConditionalAccessNested()
        {
            var cfg = Build("var a = o?.method()?[10];");
            VerifyCfg(cfg, 5);

            var blocks = cfg.Blocks.ToList();
            var oIsNullBranch = blocks[0] as BinaryBranchBlock;
            var methodCallIsNull = blocks[1] as BinaryBranchBlock;
            var arrayAccess = blocks[2];
            var assignment = blocks[3];
            var exitBlock = cfg.ExitBlock;

            VerifyAllInstructions(oIsNullBranch, "o");
            VerifyAllInstructions(methodCallIsNull, "method", ".method", ".method()");
            VerifyAllInstructions(arrayAccess, "10", "[10]");
            VerifyAllInstructions(assignment, "a = o?.method()?[10]");

            oIsNullBranch.TrueSuccessorBlock.Should().Be(assignment);
            oIsNullBranch.FalseSuccessorBlock.Should().Be(methodCallIsNull);
            methodCallIsNull.TrueSuccessorBlock.Should().Be(assignment);
            methodCallIsNull.FalseSuccessorBlock.Should().Be(arrayAccess);
            arrayAccess.SuccessorBlocks.Should().Equal(assignment);
            assignment.SuccessorBlocks.Should().Equal(exitBlock);
        }

        [TestMethod]
        public void Cfg_ConditionalAccess_Coalesce()
        {
            var cfg = Build("var a = aObj?.booleanVal ?? false");
            VerifyCfg(cfg, 6);

            var blocks = cfg.Blocks.ToList();
            var aObjIsNull = blocks[0] as BinaryBranchBlock;
            var boolFieldAccess = blocks[1];
            var coalesceIsNullBranch = blocks[2] as BinaryBranchBlock;
            var falseBlock = blocks[3];
            var assignment = blocks[4];
            var exitBlock = cfg.ExitBlock;

            VerifyAllInstructions(aObjIsNull, "aObj");
            VerifyAllInstructions(boolFieldAccess, "booleanVal", ".booleanVal");
            VerifyAllInstructions(coalesceIsNullBranch);
            VerifyAllInstructions(falseBlock, "false");
            VerifyAllInstructions(assignment, "a = aObj?.booleanVal ?? false");

            aObjIsNull.TrueSuccessorBlock.Should().Be(coalesceIsNullBranch);
            aObjIsNull.FalseSuccessorBlock.Should().Be(boolFieldAccess);
            boolFieldAccess.SuccessorBlocks.Should().Equal(coalesceIsNullBranch);
            coalesceIsNullBranch.TrueSuccessorBlock.Should().Be(falseBlock);
            coalesceIsNullBranch.FalseSuccessorBlock.Should().Be(assignment);
            falseBlock.SuccessorBlocks.Should().Equal(assignment);
            assignment.SuccessorBlocks.Should().Equal(exitBlock);
        }

        [TestMethod]
        public void Cfg_ConditionalAccess_Conditional()
        {
            var cfg = Build("a?.booleanVal == null ? true : false");
            VerifyCfg(cfg, 6);

            var blocks = cfg.Blocks.ToList();
            var aIsNullBranch = blocks[0] as BinaryBranchBlock;
            var boolFieldAccess = blocks[1];
            var nullCheckBranch = blocks[2] as BinaryBranchBlock;
            var trueBlock = blocks[3];
            var falseBlock = blocks[4];
            var exitBlock = cfg.ExitBlock;

            VerifyAllInstructions(aIsNullBranch, "a");
            VerifyAllInstructions(boolFieldAccess, "booleanVal", ".booleanVal");
            VerifyAllInstructions(nullCheckBranch, "null", "a?.booleanVal == null");
            VerifyAllInstructions(trueBlock, "true");
            VerifyAllInstructions(falseBlock, "false");

            aIsNullBranch.TrueSuccessorBlock.Should().Be(nullCheckBranch);
            aIsNullBranch.FalseSuccessorBlock.Should().Be(boolFieldAccess);
            boolFieldAccess.SuccessorBlocks.Should().Equal(nullCheckBranch);
            nullCheckBranch.TrueSuccessorBlock.Should().Be(trueBlock);
            nullCheckBranch.FalseSuccessorBlock.Should().Be(falseBlock);
            trueBlock.SuccessorBlocks.Should().Equal(exitBlock);
            falseBlock.SuccessorBlocks.Should().Equal(exitBlock);
        }

        [TestMethod]
        public void Cfg_ConditionalAccess_is()
        {
            var cfg = Build("if(a?.booleanVal is null) {return 1;}");
            VerifyCfg(cfg, 5);

            var blocks = cfg.Blocks.ToList();
            var aIsNullBranch = blocks[0] as BinaryBranchBlock;
            var boolFieldAccess = blocks[1];
            var isNullCheck = blocks[2] as BinaryBranchBlock;
            var returnBlock = blocks[3];
            var exitBlock = cfg.ExitBlock;

            VerifyAllInstructions(aIsNullBranch, "a");
            VerifyAllInstructions(boolFieldAccess, "booleanVal", ".booleanVal");
            VerifyAllInstructions(isNullCheck, "null", "a?.booleanVal is null");
            VerifyAllInstructions(returnBlock, "1");

            aIsNullBranch.TrueSuccessorBlock.Should().Be(isNullCheck);
            aIsNullBranch.FalseSuccessorBlock.Should().Be(boolFieldAccess);
            boolFieldAccess.SuccessorBlocks.Should().Equal(isNullCheck);
            isNullCheck.TrueSuccessorBlock.Should().Be(returnBlock);
            isNullCheck.FalseSuccessorBlock.Should().Be(exitBlock);
            returnBlock.SuccessorBlocks.Should().Equal(exitBlock);
        }

        #endregion

        #region Break

        [TestMethod]
        public void Cfg_For_Break()
        {
            var cfg = Build("cw0(); for (a; b && c; d) { if (e) { cw1(); break; } cw2(); } cw3();");
            VerifyCfg(cfg, 10);

            var blocks = cfg.Blocks.ToList();

            var cw0 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw0"));
            var cw1 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw1"));
            var cw2 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw2"));
            var cw3 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw3"));

            var b = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "b")) as BinaryBranchBlock;
            var c = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "c"));
            var bc = blocks[3] as BinaryBranchBlock;

            var d = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "d"));

            var e = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "e")) as BinaryBranchBlock;

            var exitBlock = cfg.ExitBlock;

            cw0.Should().BeSameAs(cfg.EntryBlock);

            cw0.SuccessorBlocks.Should().Equal(b);
            b.SuccessorBlocks.Should().BeEquivalentTo(new[] { c, bc });
            c.SuccessorBlocks.Should().Equal(bc);
            bc.SuccessorBlocks.Should().BeEquivalentTo(new[] { e, cw3 });
            e.SuccessorBlocks.Should().BeEquivalentTo(new[] { cw1, cw2 });
            cw1.SuccessorBlocks.Should().Equal(cw3);
            cw3.SuccessorBlocks.Should().Equal(exitBlock);
            cw2.SuccessorBlocks.Should().Equal(d);
            d.SuccessorBlocks.Should().Equal(b);

            bc.Instructions.Should().BeEmpty();
        }

        [TestMethod]
        public void Cfg_While_Break()
        {
            var cfg = Build("cw0(); while (b && c) { if (e) { cw1(); break; } cw2(); } cw3();");
            VerifyCfg(cfg, 8);

            var blocks = cfg.Blocks.ToList();

            var beforeWhile = blocks[0];
            var branchBlockB = (BinaryBranchBlock)blocks[1];
            var branchBlockC = (BinaryBranchBlock)blocks[2];
            var branchBlockE = (BinaryBranchBlock)blocks[3];
            var trueBlock = (JumpBlock)blocks[4];
            var afterIf = blocks[5];
            var afterWhile = blocks[6];
            var exit = blocks[7];

            beforeWhile.SuccessorBlocks.Should().Equal(branchBlockB);
            branchBlockB.TrueSuccessorBlock.Should().Be(branchBlockC);
            branchBlockB.FalseSuccessorBlock.Should().Be(afterWhile);
            branchBlockC.TrueSuccessorBlock.Should().Be(branchBlockE);
            branchBlockC.FalseSuccessorBlock.Should().Be(afterWhile);
            branchBlockE.TrueSuccessorBlock.Should().Be(trueBlock);
            branchBlockE.FalseSuccessorBlock.Should().Be(afterIf);
            trueBlock.SuccessorBlock.Should().Be(afterWhile);
            afterIf.SuccessorBlocks.Should().Equal(branchBlockB);
            afterWhile.SuccessorBlocks.Should().Equal(exit);
        }

        [TestMethod]
        public void Cfg_Foreach_Break()
        {
            var cfg = Build("cw0(); foreach (var x in xs) { if (e) { cw1(); break; } cw2(); } cw3();");
            VerifyCfg(cfg, 7);

            var blocks = cfg.Blocks.ToList();

            var cw0 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw0"));
            var cw1 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw1"));
            var cw2 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw2"));
            var cw3 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw3"));

            var xs = blocks.OfType<BinaryBranchBlock>().First(n => n.BranchingNode.IsKind(SyntaxKind.ForEachStatement));

            var e = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "e")) as BinaryBranchBlock;

            var exitBlock = cfg.ExitBlock;

            cw0.Should().BeSameAs(cfg.EntryBlock);

            cw0.SuccessorBlocks.Should().Equal(xs);
            xs.SuccessorBlocks.Should().BeEquivalentTo(new[] { e, cw3 });
            e.SuccessorBlocks.Should().BeEquivalentTo(new[] { cw1, cw2 });
            cw1.SuccessorBlocks.Should().Equal(cw3);
            cw3.SuccessorBlocks.Should().Equal(exitBlock);
            cw2.SuccessorBlocks.Should().Equal(xs);
        }

        [TestMethod]
        public void Cfg_Do_Break()
        {
            var cfg = Build("cw0(); do { if (e) { cw1(); break; } cw2(); } while (b && c); cw3();");

            VerifyCfg(cfg, 8);

            var blocks = cfg.Blocks.ToList();

            var beforeDo = blocks[0];
            var branchBlockE = (BinaryBranchBlock)blocks[1];
            var trueBlock = (JumpBlock)blocks[2];
            var afterIf = blocks[3];
            var branchBlockB = (BinaryBranchBlock)blocks[4];
            var branchBlockC = (BinaryBranchBlock)blocks[5];
            var afterWhile = blocks[6];
            var exit = blocks[7];

            beforeDo.SuccessorBlocks.Should().Equal(branchBlockE);
            branchBlockE.TrueSuccessorBlock.Should().Be(trueBlock);
            branchBlockE.FalseSuccessorBlock.Should().Be(afterIf);
            trueBlock.SuccessorBlock.Should().Be(afterWhile);
            afterIf.SuccessorBlocks.Should().Equal(branchBlockB);
            branchBlockB.TrueSuccessorBlock.Should().Be(branchBlockC);
            branchBlockB.FalseSuccessorBlock.Should().Be(afterWhile);
            branchBlockC.TrueSuccessorBlock.Should().Be(branchBlockE);
            branchBlockC.FalseSuccessorBlock.Should().Be(afterWhile);
            afterWhile.SuccessorBlocks.Should().Equal(exit);
        }

        [TestMethod]
        public void Cfg_Switch_Break()
        {
            var cfg = Build("cw0(); switch(a) { case 1: case 2: cw1(); break; } cw3();");
            VerifyCfg(cfg, 6);

            var blocks = cfg.Blocks.ToList();

            var cw0 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw0")) as BranchBlock;
            var cw1 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw1")) as JumpBlock;
            var cw3 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw3"));

            var case1Branch = blocks[1] as BinaryBranchBlock;
            var case2Branch = blocks[2] as BinaryBranchBlock;

            var exitBlock = cfg.ExitBlock;

            cw0.Should().BeSameAs(cfg.EntryBlock);

            cw0.SuccessorBlocks.Should().Equal(case1Branch);
            case1Branch.TrueSuccessorBlock.Should().Be(cw1);
            case1Branch.FalseSuccessorBlock.Should().Be(case2Branch);
            case2Branch.TrueSuccessorBlock.Should().Be(cw1);
            case2Branch.FalseSuccessorBlock.Should().Be(cw3);

            cw1.SuccessorBlocks.Should().Equal(cw3);
            cw3.SuccessorBlocks.Should().Equal(exitBlock);
        }

        #endregion

        #region Continue

        [TestMethod]
        public void Cfg_For_Continue()
        {
            var cfg = Build("cw0(); for (a; b && c; d) { if (e) { cw1(); continue; } cw2(); } cw3();");
            VerifyCfg(cfg, 10);

            var blocks = cfg.Blocks.ToList();

            var cw0 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw0"));
            var cw1 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw1"));
            var cw2 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw2"));
            var cw3 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw3"));

            var b = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "b")) as BinaryBranchBlock;
            var c = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "c"));
            var bc = blocks[3] as BinaryBranchBlock;

            var d = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "d"));

            var e = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "e")) as BinaryBranchBlock;

            var exitBlock = cfg.ExitBlock;

            cw0.Should().BeSameAs(cfg.EntryBlock);

            cw0.SuccessorBlocks.Should().Equal(b);
            b.SuccessorBlocks.Should().BeEquivalentTo(new[] { c, bc });
            c.SuccessorBlocks.Should().Equal(bc);
            bc.SuccessorBlocks.Should().BeEquivalentTo(new[] { e, cw3 });
            e.SuccessorBlocks.Should().BeEquivalentTo(new[] { cw1, cw2 });
            cw1.SuccessorBlocks.Should().Equal(d);
            cw3.SuccessorBlocks.Should().Equal(exitBlock);
            cw2.SuccessorBlocks.Should().Equal(d);
            d.SuccessorBlocks.Should().Equal(b);

            bc.Instructions.Should().BeEmpty();
        }

        [TestMethod]
        public void Cfg_While_Continue()
        {
            var cfg = Build("cw0(); while (b && c) { if (e) { cw1(); continue; } cw2(); } cw3();");
            VerifyCfg(cfg, 8);

            var blocks = cfg.Blocks.ToList();

            var beforeWhile = blocks[0];
            var branchBlockB = (BinaryBranchBlock)blocks[1];
            var branchBlockC = (BinaryBranchBlock)blocks[2];
            var branchBlockE = (BinaryBranchBlock)blocks[3];
            var trueBlock = (JumpBlock)blocks[4];
            var afterIf = blocks[5];
            var afterWhile = blocks[6];
            var exit = blocks[7];

            beforeWhile.SuccessorBlocks.Should().Equal(branchBlockB);
            branchBlockB.TrueSuccessorBlock.Should().Be(branchBlockC);
            branchBlockB.FalseSuccessorBlock.Should().Be(afterWhile);
            branchBlockC.TrueSuccessorBlock.Should().Be(branchBlockE);
            branchBlockC.FalseSuccessorBlock.Should().Be(afterWhile);
            branchBlockE.TrueSuccessorBlock.Should().Be(trueBlock);
            branchBlockE.FalseSuccessorBlock.Should().Be(afterIf);
            trueBlock.SuccessorBlock.Should().Be(branchBlockB);
            afterIf.SuccessorBlocks.Should().Equal(branchBlockB);
            afterWhile.SuccessorBlocks.Should().Equal(exit);
        }

        [TestMethod]
        public void Cfg_Foreach_Continue()
        {
            var cfg = Build("cw0(); foreach (var x in xs) { if (e) { cw1(); continue; } cw2(); } cw3();");
            VerifyCfg(cfg, 7);

            var blocks = cfg.Blocks.ToList();

            var cw0 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw0"));
            var cw1 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw1"));
            var cw2 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw2"));
            var cw3 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw3"));

            var foreachBlock = blocks.OfType<BinaryBranchBlock>().First(n => n.BranchingNode.IsKind(SyntaxKind.ForEachStatement));

            var e = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "e")) as BinaryBranchBlock;

            var exitBlock = cfg.ExitBlock;

            cw0.Should().BeSameAs(cfg.EntryBlock);

            cw0.SuccessorBlocks.Should().Equal(foreachBlock);
            foreachBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { e, cw3 });
            e.SuccessorBlocks.Should().BeEquivalentTo(new[] { cw1, cw2 });
            cw1.SuccessorBlocks.Should().Equal(foreachBlock);
            cw3.SuccessorBlocks.Should().Equal(exitBlock);
            cw2.SuccessorBlocks.Should().Equal(foreachBlock);

            foreachBlock.Instructions.Should().BeEmpty();
        }

        [TestMethod]
        public void Cfg_Foreach_Finally()
        {
            var cfg = Build(@"
                BeforeForeach();
                foreach (var item in collection)
                {
                    BeforeTry();
                    try
                    {
                        InsideTry();
                    }
                    finally
                    {
                        InsideFinally();
                    }
                    AfterFinally();
                }
                AfterForeach();
            ");

            VerifyCfg(cfg, 8);
            var blocks = cfg.Blocks.ToList();

            var beforeForeach = (ForeachCollectionProducerBlock)blocks[0];
            var foreachDecision = (BinaryBranchBlock)blocks[1];
            var beforeTry = (SimpleBlock)blocks[2];
            var insideTry = (BranchBlock)blocks[3];
            var insideFinally = (BranchBlock)blocks[4];
            var afterFinally = (SimpleBlock)blocks[5];
            var afterForeach = (SimpleBlock)blocks[6];
            var exit = (ExitBlock)blocks[7];

            beforeForeach.SuccessorBlock.Should().Be(foreachDecision);
            foreachDecision.TrueSuccessorBlock.Should().Be(beforeTry);
            foreachDecision.FalseSuccessorBlock.Should().Be(afterForeach);
            beforeTry.SuccessorBlock.Should().Be(insideTry);
            insideTry.SuccessorBlocks.Should().Equal(insideFinally);
            insideFinally.SuccessorBlocks.Should().BeEquivalentTo(new Block[] { afterFinally, exit });
            afterFinally.SuccessorBlock.Should().Be(foreachDecision);
            afterForeach.SuccessorBlock.Should().Be(exit);
        }

        [TestMethod]
        public void Cfg_Do_Continue()
        {
            var cfg = Build("cw0(); do { if (e) { cw1(); continue; } cw2(); } while (b && c); cw3();");

            VerifyCfg(cfg, 8);

            var blocks = cfg.Blocks.ToList();

            var beforeDo = blocks[0];
            var branchBlockE = (BinaryBranchBlock)blocks[1];
            var trueBlock = (JumpBlock)blocks[2];
            var afterIf = blocks[3];
            var branchBlockB = (BinaryBranchBlock)blocks[4];
            var branchBlockC = (BinaryBranchBlock)blocks[5];
            var afterWhile = blocks[6];
            var exit = blocks[7];

            beforeDo.SuccessorBlocks.Should().Equal(branchBlockE);
            branchBlockE.TrueSuccessorBlock.Should().Be(trueBlock);
            branchBlockE.FalseSuccessorBlock.Should().Be(afterIf);
            trueBlock.SuccessorBlock.Should().Be(branchBlockB);
            afterIf.SuccessorBlocks.Should().Equal(branchBlockB);
            branchBlockB.TrueSuccessorBlock.Should().Be(branchBlockC);
            branchBlockB.FalseSuccessorBlock.Should().Be(afterWhile);
            branchBlockC.TrueSuccessorBlock.Should().Be(branchBlockE);
            branchBlockC.FalseSuccessorBlock.Should().Be(afterWhile);
            afterWhile.SuccessorBlocks.Should().Equal(exit);
        }

        #endregion

        #region Try/Finally

        [TestMethod]
        public void Cfg_Try_Finally()
        {
            var cfg = Build(@"
            before();
            try
            {
                inside();
            }
            finally
            {
                fin();
            }
            after();");

            VerifyCfg(cfg, 5);

            var blocks = cfg.Blocks.ToList();

            var tryStartBlock = blocks[0];
            var insideTryBlock = blocks[1];
            var finallyBlock = blocks[2];
            var afterFinallyBlock = blocks[3];
            var exit = blocks[4];

            tryStartBlock.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(tryStartBlock, "before", "before()");
            tryStartBlock.SuccessorBlocks.Should().Equal(insideTryBlock);

            insideTryBlock.Should().BeOfType<BranchBlock>();
            VerifyAllInstructions(insideTryBlock, "inside", "inside()");
            insideTryBlock.SuccessorBlocks.Should().Equal(finallyBlock);

            finallyBlock.Should().BeOfType<BranchBlock>();
            VerifyAllInstructions(finallyBlock, "fin", "fin()");
            finallyBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { afterFinallyBlock, exit });

            afterFinallyBlock.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(afterFinallyBlock, "after", "after()");

            exit.Should().BeOfType<ExitBlock>();
        }

        [TestMethod]
        public void Cfg_Try_CatchSome()
        {
            var cfg = Build(@"
            before();
            try
            {
                inside();
            }
            catch(Exception1 e)
            {
                cat1();
            }
            catch(Exception2 e)
            {
                cat2();
            }
            after();");

            VerifyCfg(cfg, 6);

            var blocks = cfg.Blocks.ToList();

            var beforeTryBlock = blocks[0];
            var insideTryBlock = blocks[1];
            var catchBlock1 = blocks[2];
            var catchBlock2 = blocks[3];
            var afterFinallyBlock = blocks[4];
            var exit = blocks[5];

            beforeTryBlock.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(beforeTryBlock, "before", "before()");
            beforeTryBlock.SuccessorBlocks.Should().Equal(insideTryBlock);

            insideTryBlock.Should().BeOfType<BranchBlock>();
            VerifyAllInstructions(insideTryBlock, "inside", "inside()");
            insideTryBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { catchBlock1 /*caught ex*/, catchBlock2 /*caught ex*/, afterFinallyBlock /*no ex*/, exit /*uncaught ex*/});

            catchBlock1.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(catchBlock1, "cat1", "cat1()");
            catchBlock1.SuccessorBlocks.Should().Equal(afterFinallyBlock);

            catchBlock2.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(catchBlock2, "cat2", "cat2()");
            catchBlock2.SuccessorBlocks.Should().Equal(afterFinallyBlock);

            afterFinallyBlock.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(afterFinallyBlock, "after", "after()");

            exit.Should().BeOfType<ExitBlock>();
        }

        [TestMethod]
        public void Cfg_Try_CatchAll()
        {
            var cfg = Build(@"
            before();
            try
            {
                inside();
            }
            catch(Exception1 e)
            {
                cat1();
            }
            catch
            {
                cat2();
            }
            after();");

            VerifyCfg(cfg, 6);

            var blocks = cfg.Blocks.ToList();

            var tryStartBlock = blocks[0];
            var tryEndBlock = blocks[1];
            var catchBlock1 = blocks[2];
            var catchBlock2 = blocks[3];
            var afterFinallyBlock = blocks[4];
            var exit = blocks[5];

            tryStartBlock.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(tryStartBlock, "before", "before()");
            tryStartBlock.SuccessorBlocks.Should().Equal(tryEndBlock);

            tryEndBlock.Should().BeOfType<BranchBlock>();
            VerifyAllInstructions(tryEndBlock, "inside", "inside()");
            tryEndBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { catchBlock1 /*caught ex*/, catchBlock2 /*caught ex*/, afterFinallyBlock /*no ex*/});

            catchBlock1.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(catchBlock1, "cat1", "cat1()");
            catchBlock1.SuccessorBlocks.Should().Equal(afterFinallyBlock);

            catchBlock2.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(catchBlock2, "cat2", "cat2()");
            catchBlock2.SuccessorBlocks.Should().Equal(afterFinallyBlock);

            afterFinallyBlock.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(afterFinallyBlock, "after", "after()");

            exit.Should().BeOfType<ExitBlock>();
        }

        [TestMethod]
        public void Cfg_Try_CatchSome_Finally()
        {
            var cfg = Build(@"
            before();
            try
            {
                inside();
            }
            catch(Exception1 e)
            {
                cat1();
            }
            catch(Exception2 e)
            {
                cat2();
            }
            finally
            {
                fin();
            }
            after();");

            VerifyCfg(cfg, 7);

            var blocks = cfg.Blocks.ToList();

            var tryStartBlock = blocks[0];
            var insideTryBlock = blocks[1];
            var catchBlock1 = blocks[2];
            var catchBlock2 = blocks[3];
            var finallyBlock = blocks[4];
            var afterFinallyBlock = blocks[5];
            var exit = blocks[6];

            tryStartBlock.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(tryStartBlock, "before", "before()");
            tryStartBlock.SuccessorBlocks.Should().Equal(insideTryBlock);

            insideTryBlock.Should().BeOfType<BranchBlock>();
            VerifyAllInstructions(insideTryBlock, "inside", "inside()");
            insideTryBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { catchBlock1 /*caught ex*/, catchBlock2 /*caught ex*/, finallyBlock });

            catchBlock1.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(catchBlock1, "cat1", "cat1()");
            catchBlock1.SuccessorBlocks.Should().Equal(finallyBlock);

            catchBlock2.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(catchBlock2, "cat2", "cat2()");
            catchBlock2.SuccessorBlocks.Should().Equal(finallyBlock);

            finallyBlock.Should().BeOfType<BranchBlock>();
            VerifyAllInstructions(finallyBlock, "fin", "fin()");
            finallyBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { afterFinallyBlock, exit });

            afterFinallyBlock.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(afterFinallyBlock, "after", "after()");

            exit.Should().BeOfType<ExitBlock>();
        }

        [TestMethod]
        public void Cfg_Try_CatchAll_Finally()
        {
            var cfg = Build(@"
            before();
            try
            {
                inside();
            }
            catch(Exception1 e)
            {
                cat1();
            }
            catch
            {
                cat2();
            }
            finally
            {
                fin();
            }
            after();");

            VerifyCfg(cfg, 7);

            var blocks = cfg.Blocks.ToList();

            var tryStartBlock = blocks[0];
            var tryEndBlock = blocks[1];
            var catchBlock1 = blocks[2];
            var catchBlock2 = blocks[3];
            var finallyBlock = blocks[4];
            var afterFinallyBlock = blocks[5];
            var exit = blocks[6];

            tryStartBlock.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(tryStartBlock, "before", "before()");
            tryStartBlock.SuccessorBlocks.Should().Equal(tryEndBlock);

            tryEndBlock.Should().BeOfType<BranchBlock>();
            VerifyAllInstructions(tryEndBlock, "inside", "inside()");
            tryEndBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { catchBlock1 /*caught ex*/, catchBlock2 /*caught ex*/, finallyBlock /*no ex*/});

            catchBlock1.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(catchBlock1, "cat1", "cat1()");
            catchBlock1.SuccessorBlocks.Should().Equal(finallyBlock);

            catchBlock2.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(catchBlock2, "cat2", "cat2()");
            catchBlock2.SuccessorBlocks.Should().Equal(finallyBlock);

            finallyBlock.Should().BeOfType<BranchBlock>();
            VerifyAllInstructions(finallyBlock, "fin", "fin()");
            finallyBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { afterFinallyBlock, exit });

            afterFinallyBlock.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(afterFinallyBlock, "after", "after()");

            exit.Should().BeOfType<ExitBlock>();
        }

        [TestMethod]
        public void Cfg_Try_CatchAll_Finally_Conditional_Return()
        {
            var cfg = Build(@"
            before();
            try
            {
                if (true)
                {
                    return;
                }
                inside();
            }
            catch
            {
                cat();
            }
            finally
            {
                fin();
            }
            after();
            ");

            VerifyCfg(cfg, 8);

            var blocks = cfg.Blocks.ToList();

            var tryStartBlock = blocks[0];
            var binaryBlock = blocks[1];
            var returnBlock = blocks[2];
            var tryEndBlock = blocks[3];
            var catchBlock = blocks[4];
            var finallyBlock = blocks[5];
            var afterFinallyBlock = blocks[6];
            var exit = blocks[7];

            VerifyAllInstructions(tryStartBlock, "before", "before()");
            tryStartBlock.SuccessorBlocks.Should().Equal(binaryBlock);

            VerifyAllInstructions(binaryBlock, "true");
            binaryBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { tryEndBlock /*false*/, returnBlock /*true*/});

            VerifyAllInstructions(returnBlock);
            returnBlock.SuccessorBlocks.Should().Equal(finallyBlock);

            VerifyAllInstructions(tryEndBlock, "inside", "inside()");
            tryEndBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { catchBlock /*exception thrown*/, finallyBlock /*no exception*/});

            VerifyAllInstructions(catchBlock, "cat", "cat()");
            catchBlock.SuccessorBlocks.Should().Equal(finallyBlock);

            VerifyAllInstructions(finallyBlock, "fin", "fin()");
            finallyBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { afterFinallyBlock, exit });

            VerifyAllInstructions(afterFinallyBlock, "after", "after()");
            afterFinallyBlock.SuccessorBlocks.Should().Equal(exit);

            blocks.Last().Should().BeOfType<ExitBlock>();
        }

        [TestMethod]
        public void Cfg_Try_CatchSome_Finally_Conditional_Return()
        {
            var cfg = Build(@"
            before();
            try
            {
                if (true)
                {
                    return;
                }
                inside();
            }
            catch(SomeException)
            {
                cat();
            }
            finally
            {
                fin();
            }
            after();
            ");

            VerifyCfg(cfg, 8);

            var blocks = cfg.Blocks.ToList();

            var tryStartBlock = blocks[0];
            var binaryBlock = blocks[1];
            var returnBlock = blocks[2];
            var tryEndBlock = blocks[3];
            var catchBlock = blocks[4];
            var finallyBlock = blocks[5];
            var afterFinallyBlock = blocks[6];
            var exit = blocks[7];

            VerifyAllInstructions(tryStartBlock, "before", "before()");
            tryStartBlock.SuccessorBlocks.Should().Equal(binaryBlock);

            VerifyAllInstructions(binaryBlock, "true");
            binaryBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { tryEndBlock /*false*/, returnBlock /*true*/});

            VerifyAllInstructions(returnBlock);
            returnBlock.SuccessorBlocks.Should().Equal(finallyBlock);

            VerifyAllInstructions(tryEndBlock, "inside", "inside()");
            tryEndBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { catchBlock /*caught exception thrown*/, finallyBlock });

            VerifyAllInstructions(catchBlock, "cat", "cat()");
            catchBlock.SuccessorBlocks.Should().Equal(finallyBlock);

            VerifyAllInstructions(finallyBlock, "fin", "fin()");
            finallyBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { afterFinallyBlock, exit });

            VerifyAllInstructions(afterFinallyBlock, "after", "after()");
            afterFinallyBlock.SuccessorBlocks.Should().Equal(exit);

            blocks.Last().Should().BeOfType<ExitBlock>();
        }

        [TestMethod]
        public void Cfg_Try_CatchSome_Conditional_Return()
        {
            var cfg = Build(@"
            before();
            try
            {
                if (true)
                {
                    return;
                }
                inside();
            }
            catch(SomeException)
            {
                cat();
            }
            after();
            ");

            VerifyCfg(cfg, 7);

            var blocks = cfg.Blocks.ToList();

            var tryStartBlock = blocks[0];
            var binaryBlock = blocks[1];
            var returnBlock = blocks[2];
            var tryEndBlock = blocks[3];
            var catchBlock = blocks[4];
            var afterFinallyBlock = blocks[5];
            var exit = blocks[6];

            VerifyAllInstructions(tryStartBlock, "before", "before()");
            tryStartBlock.SuccessorBlocks.Should().Equal(binaryBlock);

            VerifyAllInstructions(binaryBlock, "true");
            binaryBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { tryEndBlock /*false*/, returnBlock /*true*/});

            VerifyAllInstructions(returnBlock);
            returnBlock.SuccessorBlocks.Should().Equal(exit);

            VerifyAllInstructions(tryEndBlock, "inside", "inside()");
            tryEndBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { catchBlock /*caught exception thrown*/, afterFinallyBlock /*no exception*/, exit /*uncaught exception*/});

            VerifyAllInstructions(catchBlock, "cat", "cat()");
            catchBlock.SuccessorBlocks.Should().Equal(afterFinallyBlock);

            VerifyAllInstructions(afterFinallyBlock, "after", "after()");
            afterFinallyBlock.SuccessorBlocks.Should().Equal(exit);

            blocks.Last().Should().BeOfType<ExitBlock>();
        }

        [TestMethod]
        public void Cfg_Try_CatchAll_Conditional_Return()
        {
            var cfg = Build(@"
            before();
            try
            {
                if (true)
                {
                    return;
                }
                inside();
            }
            catch
            {
                cat();
            }
            after();
            ");

            VerifyCfg(cfg, 7);

            var blocks = cfg.Blocks.ToList();

            var tryStartBlock = blocks[0];
            var binaryBlock = blocks[1];
            var returnBlock = blocks[2];
            var tryEndBlock = blocks[3];
            var catchBlock = blocks[4];
            var afterFinallyBlock = blocks[5];
            var exit = blocks[6];

            VerifyAllInstructions(tryStartBlock, "before", "before()");
            tryStartBlock.SuccessorBlocks.Should().Equal(binaryBlock);

            VerifyAllInstructions(binaryBlock, "true");
            binaryBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { tryEndBlock /*false*/, returnBlock /*true*/});

            VerifyAllInstructions(returnBlock);
            returnBlock.SuccessorBlocks.Should().Equal(exit);

            VerifyAllInstructions(tryEndBlock, "inside", "inside()");
            tryEndBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { catchBlock /*caught exception thrown*/, afterFinallyBlock /*no exception*/});

            VerifyAllInstructions(catchBlock, "cat", "cat()");
            catchBlock.SuccessorBlocks.Should().Equal(afterFinallyBlock);

            VerifyAllInstructions(afterFinallyBlock, "after", "after()");
            afterFinallyBlock.SuccessorBlocks.Should().Equal(exit);

            blocks.Last().Should().BeOfType<ExitBlock>();
        }

        [TestMethod]
        public void Cfg_TryCatch_Exception_Filter()
        {
            var cfg = Build(@"
            cw0();
            try
            {
                cw1();
            }
            catch(Exception e) when (e is InvalidOperationException)
            {
                cw2();
            }
            cw5();");

            VerifyCfg(cfg, 6);

            var blocks = cfg.Blocks.ToList();

            var tryStartBlock = blocks[0];
            var tryBodyBlock = blocks[1];
            var whenBlock = blocks[2];
            var catchBlock = blocks[3];
            var afterTryBlock = blocks[4];
            var exit = blocks.Last();

            tryStartBlock.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(tryStartBlock, "cw0", "cw0()");
            tryStartBlock.SuccessorBlocks.Should().Equal(tryBodyBlock);

            tryBodyBlock.Should().BeOfType<BranchBlock>();
            VerifyAllInstructions(tryBodyBlock, "cw1", "cw1()");
            tryBodyBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { whenBlock, afterTryBlock, exit });

            whenBlock.Should().BeOfType<BinaryBranchBlock>();
            VerifyAllInstructions(whenBlock, "e", "e is InvalidOperationException");
            whenBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { catchBlock, afterTryBlock });

            catchBlock.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(catchBlock, "cw2", "cw2()");
            catchBlock.SuccessorBlocks.Should().Equal(afterTryBlock);

            afterTryBlock.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(afterTryBlock, "cw5", "cw5()");
            afterTryBlock.SuccessorBlocks.Should().Equal(exit);

            exit.Should().BeOfType<ExitBlock>();
        }

        [TestMethod]
        public void Cfg_TryCatch_ThrowInsideTry()
        {
            var cfg = Build(@"
            bool shouldCatch = false;
            try
            {
                shouldCatch = true;
                throw new InvalidOperationException(""bar"");
            }
            catch(Exception e) when (shouldCatch)
            {
                cw2();
            }
            cw5();");

            VerifyCfg(cfg, 6);

            var blocks = cfg.Blocks.ToList();

            var tryStartBlock = blocks[0];
            var tryBodyBlock = blocks[1];
            var whenBlock = blocks[2];
            var catchBlock = blocks[3];
            var afterTryBlock = blocks[4];
            var exit = blocks.Last();

            tryStartBlock.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(tryStartBlock, "false", "shouldCatch = false");
            tryStartBlock.SuccessorBlocks.Should().Equal(tryBodyBlock);

            tryBodyBlock.Should().BeOfType<BranchBlock>();
            VerifyAllInstructions(tryBodyBlock, "true", "shouldCatch = true", "\"bar\"", "new InvalidOperationException(\"bar\")");
            tryBodyBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { whenBlock, afterTryBlock, exit });

            whenBlock.Should().BeOfType<BinaryBranchBlock>();
            VerifyAllInstructions(whenBlock, "shouldCatch");
            whenBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { catchBlock, afterTryBlock });

            catchBlock.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(catchBlock, "cw2", "cw2()");
            catchBlock.SuccessorBlocks.Should().Equal(afterTryBlock);

            afterTryBlock.Should().BeOfType<SimpleBlock>();
            VerifyAllInstructions(afterTryBlock, "cw5", "cw5()");
            afterTryBlock.SuccessorBlocks.Should().Equal(exit);

            exit.Should().BeOfType<ExitBlock>();
        }

        [TestMethod]
        public void Cfg_TryCatch_WithBreak_Inside_DoWhile()
        {
            var cfg = Build(@"
            var attempts = 0;
            do
            {
                cw0();
                try
                {
                    attempts++;
                    cw1();
                    break;
                }
                catch(Exception e)
                {
                    cw2();
                }
            } while (true);
            cw5();");

            VerifyCfg(cfg, 8);

            var blocks = cfg.Blocks.ToList();

            var beforeDoBlock = (SimpleBlock)blocks[0];
            var doBlock = (SimpleBlock)blocks[1];
            var tryBody = (BranchBlock)blocks[2];
            var tryStatementBranch = (BranchBlock)blocks[3];
            var catchBlock = (SimpleBlock)blocks[4];
            var whileStmt = (BinaryBranchBlock)blocks[5];
            var afterDoWhile = (SimpleBlock)blocks[6];
            var exit = (ExitBlock)blocks.Last();

            VerifyAllInstructions(beforeDoBlock, "0", "attempts = 0");
            beforeDoBlock.SuccessorBlocks.Should().Equal(doBlock);

            VerifyAllInstructions(doBlock, "cw0", "cw0()");
            doBlock.SuccessorBlocks.Should().Equal(tryBody);

            VerifyAllInstructions(tryBody, "attempts", "attempts++", "cw1", "cw1()");
            // this is wrong, the tryBody should not have a connection with whileStmt, it can lead to FNs
            tryBody.SuccessorBlocks.Should().BeEquivalentTo(new Block[] { catchBlock, whileStmt, afterDoWhile });

            tryStatementBranch.ReversedInstructions.Should().BeEmpty();
            tryStatementBranch.SuccessorBlocks.Should().BeEquivalentTo(new Block[] { catchBlock, whileStmt });

            VerifyAllInstructions(catchBlock, "cw2", "cw2()");
            catchBlock.SuccessorBlocks.Should().Equal(whileStmt);

            VerifyAllInstructions(whileStmt, "true");
            whileStmt.TrueSuccessorBlock.Should().Be(doBlock);
            whileStmt.FalseSuccessorBlock.Should().Be(afterDoWhile);

            VerifyAllInstructions(afterDoWhile, "cw5", "cw5()");
            afterDoWhile.SuccessorBlocks.Should().Equal(exit);

            exit.Should().BeOfType<ExitBlock>();
        }

        // This should be fixed in https://github.com/SonarSource/sonar-dotnet/issues/474
        [TestMethod]
        public void Cfg_TryCatchFinally_InsideLoop_WithBreakInsideTry_AndContinueInsideCatch()
        {
            var cfg = Build(@"
            do
            {
                cw0();
                try
                {
                    cw1();
                    break;
                }
                catch(ArgumentNullException e)
                {
                    cw2();
                    continue;
                }
                finally
                {
                    cw3();
                    // CS0157 control cannot leave the body of a finally, so we cannot have jumps here
                }
                // the below is not reachable
                cw4();
            } while (true);
            cw5();");

            VerifyCfg(cfg, 9);

            var blocks = cfg.Blocks.ToList();

            var doBeforeTry = (SimpleBlock)blocks[0];
            var tryStatement = (BranchBlock)blocks[1];
            var tryBody = (BranchBlock)blocks[2];
            var catchBody = (JumpBlock)blocks[3];
            var finallyBlock = (BranchBlock)blocks[4];
            var afterTry = (SimpleBlock)blocks[5];
            var whileStmt = (BinaryBranchBlock)blocks[6];
            var afterDoWhile = (SimpleBlock)blocks[7];
            var exit = (ExitBlock)blocks.Last();

            VerifyAllInstructions(doBeforeTry, "cw0", "cw0()");
            doBeforeTry.SuccessorBlocks.Should().Equal(tryStatement);

            VerifyAllInstructions(tryStatement, "cw1", "cw1()");
            tryStatement.SuccessorBlocks.Should().BeEquivalentTo(new Block[] { catchBody, finallyBlock, afterDoWhile });

            tryBody.SuccessorBlocks.Should().BeEquivalentTo(new Block[] { catchBody, finallyBlock });

            VerifyAllInstructions(catchBody, "cw2", "cw2()");
            catchBody.SuccessorBlock.Should().Be(whileStmt);

            // ToDo: this is wrong, `finally` should be connected to
            // - EXIT
            // - WHILE (because of `continue`)
            // - afterDoWhile (because of `break`)
            finallyBlock.SuccessorBlocks.Should().BeEquivalentTo(new Block[] { afterTry, exit });
            afterTry.SuccessorBlock.Should().Be(whileStmt);

            VerifyAllInstructions(whileStmt, "true");
            whileStmt.TrueSuccessorBlock.Should().Be(doBeforeTry);
            whileStmt.FalseSuccessorBlock.Should().Be(afterDoWhile);

            VerifyAllInstructions(afterDoWhile, "cw5", "cw5()");
            afterDoWhile.SuccessorBlocks.Should().Equal(exit);

            exit.Should().BeOfType<ExitBlock>();
        }

        // This should be fixed in https://github.com/SonarSource/sonar-dotnet/issues/474
        [TestMethod]
        public void Cfg_TryFinally_InsideLoop_WithBreakAndContinueInsideTry()
        {
            var cfg = Build(@"
            do
            {
                cw0();
                try
                {
                    if (cond)
                    {
                        cw1();
                        continue;
                    }
                    else
                    {
                        cw2();
                        break;
                    }
                }
                finally
                {
                    cw3();
                }
                // the below is not reachable
                cw4();
            } while (true);
            cw5();");

            VerifyCfg(cfg, 10);

            var blocks = cfg.Blocks.ToList();

            var doBeforeTry = (SimpleBlock)blocks[0];
            var ifInsideTry = (BinaryBranchBlock)blocks[1];
            var thenContinue = (JumpBlock)blocks[2];
            var elseIf = (JumpBlock)blocks[3];
            var tryStatement = (BranchBlock)blocks[4];
            var finallyBody = (BranchBlock)blocks[5];
            var afterTry = (SimpleBlock)blocks[6];
            var whileStmt = (BinaryBranchBlock)blocks[7];
            var afterDoWhile = (SimpleBlock)blocks[8];
            var exit = (ExitBlock)blocks.Last();

            VerifyAllInstructions(doBeforeTry, "cw0", "cw0()");
            doBeforeTry.SuccessorBlock.Should().Be(ifInsideTry);

            ifInsideTry.TrueSuccessorBlock.Should().Be(thenContinue);
            ifInsideTry.FalseSuccessorBlock.Should().Be(elseIf);

            VerifyAllInstructions(thenContinue, "cw1", "cw1()");
            // ToDo: it should lead to `finally` which should lead to `whileStmt`
            thenContinue.SuccessorBlock.Should().Be(whileStmt);

            VerifyAllInstructions(elseIf, "cw2", "cw2()");
            // ToDo: it should lead to `finally` which should lead to `afterDoWhile`
            elseIf.SuccessorBlock.Should().Be(afterDoWhile);

            // ToDo: this is weird and is basically skipped
            tryStatement.SuccessorBlocks.Should().Equal(finallyBody);

            finallyBody.SuccessorBlocks.Should().BeEquivalentTo(new Block[] { afterTry, exit });
            afterTry.SuccessorBlock.Should().Be(whileStmt);

            VerifyAllInstructions(whileStmt, "true");
            whileStmt.TrueSuccessorBlock.Should().Be(doBeforeTry);
            whileStmt.FalseSuccessorBlock.Should().Be(afterDoWhile);

            VerifyAllInstructions(afterDoWhile, "cw5", "cw5()");
            afterDoWhile.SuccessorBlocks.Should().Equal(exit);

            exit.Should().BeOfType<ExitBlock>();
        }

        [TestMethod]
        public void Cfg_TryCatch_Inside_DoWhile_WithThrow_InsideCatch()
        {
            var cfg = Build(@"
            var attempts = 0;
            do
            {
                cw0();
                try
                {
                    attempts++;
                    cw1();
                    break;
                }
                catch(Exception e)
                {
                    cw2();
                    if (attempts > retries)
                    {
                        cw3();
                        throw;
                    }
                    cw4();
                }
            } while (true);
            cw5();");

            VerifyCfg(cfg, 10);

            var blocks = cfg.Blocks.ToList();

            var beforeDoBlock = (SimpleBlock)blocks[0];
            var insideDoBeforeTry = (SimpleBlock)blocks[1];
            var insideTry = (BranchBlock)blocks[2];

            // this block is initially created for the `insideTry`,
            // and it gets replaced when seeing the `break;`
            var temporaryStrayBlock = (BranchBlock)blocks[3];

            var catchBodyWithIf = (BinaryBranchBlock)blocks[4];
            var insideIfInsideCatch = (JumpBlock)blocks[5];
            var afterIfInsideCatch = (SimpleBlock)blocks[6];
            var whileStmt = (BinaryBranchBlock)blocks[7];
            var afterDoWhile = (SimpleBlock)blocks[8];
            var exit = (ExitBlock)blocks.Last();

            VerifyAllInstructions(beforeDoBlock, "0", "attempts = 0");
            beforeDoBlock.SuccessorBlocks.Should().Equal(insideDoBeforeTry);

            VerifyAllInstructions(insideDoBeforeTry, "cw0", "cw0()");
            insideDoBeforeTry.SuccessorBlocks.Should().Equal(insideTry);

            VerifyAllInstructions(insideTry, "attempts", "attempts++", "cw1", "cw1()");
            insideTry.SuccessorBlocks.Should().BeEquivalentTo(new Block[] { catchBodyWithIf, whileStmt, afterDoWhile });

            temporaryStrayBlock.ReversedInstructions.Should().BeEmpty();
            temporaryStrayBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { catchBodyWithIf, whileStmt });

            VerifyAllInstructions(catchBodyWithIf, "cw2", "cw2()", "attempts", "retries", "attempts > retries");
            catchBodyWithIf.TrueSuccessorBlock.Should().Be(insideIfInsideCatch);
            catchBodyWithIf.FalseSuccessorBlock.Should().Be(afterIfInsideCatch);

            VerifyAllInstructions(insideIfInsideCatch, "cw3", "cw3()");
            insideIfInsideCatch.SuccessorBlocks.Should().Equal(exit);

            VerifyAllInstructions(afterIfInsideCatch, "cw4", "cw4()");
            afterIfInsideCatch.SuccessorBlocks.Should().Equal(whileStmt);

            VerifyAllInstructions(whileStmt, "true");
            whileStmt.TrueSuccessorBlock.Should().Be(insideDoBeforeTry);
            whileStmt.FalseSuccessorBlock.Should().Be(afterDoWhile);

            VerifyAllInstructions(afterDoWhile, "cw5", "cw5()");
            afterDoWhile.SuccessorBlocks.Should().Equal(exit);

            exit.Should().BeOfType<ExitBlock>();
        }

        [TestMethod]
        public void Cfg_TryFinally_Inside_DoWhile_WithThrow_InsideCatch()
        {
            var cfg = Build(@"
            var attempts = 0;
            do
            {
                cw0();
                try
                {
                    if (attempts)
                        cw1();
                }
                finally
                {
                    attempts = 1;
                }
            } while (true);
            cw5();");

            VerifyCfg(cfg, 9);

            var blocks = cfg.Blocks.ToList();

            var beforeDoBlock = (SimpleBlock)blocks[0];
            var insideDoBeforeTry = (SimpleBlock)blocks[1];
            var insideTryIfStatement = (BinaryBranchBlock)blocks[2];
            var insideIf = (SimpleBlock)blocks[3];
            var finallyBifurcation = (BranchBlock)blocks[4];
            var finallyBlock = (BranchBlock)blocks[5];
            var whileStmt = (BinaryBranchBlock)blocks[6];
            var afterDoWhile = (SimpleBlock)blocks[7];
            var exit = (ExitBlock)blocks.Last();

            VerifyAllInstructions(beforeDoBlock, "0", "attempts = 0");
            beforeDoBlock.SuccessorBlocks.Should().Equal(insideDoBeforeTry);

            VerifyAllInstructions(insideDoBeforeTry, "cw0", "cw0()");
            insideDoBeforeTry.SuccessorBlocks.Should().Equal(insideTryIfStatement);

            VerifyAllInstructions(insideTryIfStatement, "attempts");
            insideTryIfStatement.TrueSuccessorBlock.Should().Be(insideIf);
            insideTryIfStatement.FalseSuccessorBlock.Should().Be(finallyBifurcation);

            insideIf.SuccessorBlock.Should().Be(finallyBifurcation);

            finallyBifurcation.SuccessorBlocks.Should().Equal(finallyBlock);

            finallyBlock.SuccessorBlocks.Should().BeEquivalentTo(new Block[] { whileStmt, exit });

            whileStmt.TrueSuccessorBlock.Should().Be(insideDoBeforeTry);
            whileStmt.FalseSuccessorBlock.Should().Be(afterDoWhile);

            afterDoWhile.SuccessorBlock.Should().Be(exit);

            exit.Should().BeOfType<ExitBlock>();
        }

        [TestMethod]
        public void Cfg_TryCatchFinally_Return_Nested()
        {
            var cfg = Build(@"
            before_out();
            try
            {
                before_in();
                try
                {
                    foo();
                    return;
                }
                catch
                {
                    cat_in();
                }
                finally
                {
                    fin_in();
                }
                after_in();
            }
            catch
            {
                cat_out();
            }
            finally
            {
                fin_out();
            }
            after_out();");

            VerifyCfg(cfg, 11);

            var blocks = cfg.Blocks.ToList();

            var beforeOuterTry = blocks[0];
            var innerTryStartBlock = blocks[1];
            var innerReturnBlock = blocks[2];
            var innerTryEndBlock = blocks[3];
            var innerCatchBlock = blocks[4];
            var innerFinallyBlock = blocks[5];
            var outerTryBlock = blocks[6]; // innerAfterFinallyBlock is not generated, its instructions are in outerTryBlock
            var outerCatchBlock = blocks[7];
            var outerFinallyBlock = blocks[8];
            var afterFinallyBlock = blocks[9];
            var exit = blocks[10];

            exit.Should().BeOfType<ExitBlock>();

            beforeOuterTry.Should().BeOfType<SimpleBlock>();
            beforeOuterTry.SuccessorBlocks.Should().Equal(innerTryStartBlock);

            innerTryStartBlock.Should().BeOfType<BranchBlock>();
            innerTryStartBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { innerReturnBlock /*no ex*/, outerCatchBlock, outerFinallyBlock });

            innerReturnBlock.Should().BeOfType<BranchBlock>();
            innerReturnBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { innerTryEndBlock, innerCatchBlock });

            innerTryEndBlock.Should().BeOfType<JumpBlock>();
            VerifyAllInstructions(innerTryEndBlock, "foo", "foo()");
            innerTryEndBlock.SuccessorBlocks.Should().Equal(innerFinallyBlock);

            innerCatchBlock.Should().BeOfType<SimpleBlock>();
            innerCatchBlock.SuccessorBlocks.Should().Equal(innerFinallyBlock);

            innerFinallyBlock.Should().BeOfType<BranchBlock>();
            innerFinallyBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { outerTryBlock, outerFinallyBlock });

            outerTryBlock.Should().BeOfType<BranchBlock>();
            outerTryBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { outerCatchBlock /*ex*/, outerFinallyBlock /*no ex*/});

            outerCatchBlock.Should().BeOfType<SimpleBlock>();
            outerCatchBlock.SuccessorBlocks.Should().Equal(outerFinallyBlock);

            outerFinallyBlock.Should().BeOfType<BranchBlock>();
            outerFinallyBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { afterFinallyBlock, exit });

            afterFinallyBlock.Should().BeOfType<SimpleBlock>();
            afterFinallyBlock.SuccessorBlocks.Should().Equal(exit);
        }

        [TestMethod]
        public void Cfg_TryCatch_ReturnVariable_InCatch()
        {
            var cfg = Build(@"
                var number = 5;
                try
                {
                    bar();
                    return 0;
                }
                catch
                {
                    return number;
                }
                foo();");

            VerifyCfg(cfg, 6);

            var blocks = cfg.Blocks.ToList();

            var beforeOuterTry = (SimpleBlock)blocks[0];
            var tryStatementBlock = (BranchBlock)blocks[1];
            var tryReturn = (JumpBlock)blocks[2];
            var catchReturn = (JumpBlock)blocks[3];
            var afterTry = (SimpleBlock)blocks[4];
            var exit = blocks[5];

            VerifyAllInstructions(beforeOuterTry, "5", "number = 5");
            beforeOuterTry.SuccessorBlocks.Should().Equal(tryStatementBlock);

            VerifyNoInstruction(tryStatementBlock);
            tryStatementBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { tryReturn, catchReturn });

            VerifyAllInstructions(tryReturn, "bar", "bar()", "0");
            tryReturn.SuccessorBlocks.Should().Equal(exit);

            VerifyAllInstructions(catchReturn, "number");
            catchReturn.SuccessorBlocks.Should().Equal(exit);

            VerifyAllInstructions(afterTry, "foo", "foo()");
            afterTry.SuccessorBlocks.Should().Equal(exit);

            exit.Should().BeOfType<ExitBlock>();
        }

        [TestMethod]
        public void Cfg_TryCatch_NestedReturnVariable_InCatch()
        {
            var cfg = Build(@"
                var number = 5;
                try
                {
                    bar();
                    return 0;
                }
                catch
                {
                    if (cond) return number;
                }
                foo();");

            VerifyCfg(cfg, 7);

            var blocks = cfg.Blocks.ToList();

            var beforeOuterTry = (SimpleBlock)blocks[0];
            var tryStatementBlock = (BranchBlock)blocks[1];
            var tryReturn = (JumpBlock)blocks[2];
            var ifInsideCatch = (BinaryBranchBlock)blocks[3];
            var returnInCatch = (JumpBlock)blocks[4];
            var afterTry = (SimpleBlock)blocks[5];
            var exit = (ExitBlock)blocks[6];

            VerifyAllInstructions(beforeOuterTry, "5", "number = 5");
            beforeOuterTry.SuccessorBlocks.Should().Equal(tryStatementBlock);

            VerifyNoInstruction(tryStatementBlock);
            tryStatementBlock.SuccessorBlocks.Should().BeEquivalentTo(new Block[] { tryReturn, ifInsideCatch });

            VerifyAllInstructions(tryReturn, "bar", "bar()", "0");
            tryReturn.SuccessorBlocks.Should().Equal(exit);

            ifInsideCatch.TrueSuccessorBlock.Should().Be(returnInCatch);
            ifInsideCatch.FalseSuccessorBlock.Should().Be(afterTry);

            VerifyAllInstructions(returnInCatch, "number");
            returnInCatch.SuccessorBlocks.Should().Equal(exit);

            VerifyAllInstructions(afterTry, "foo", "foo()");
            afterTry.SuccessorBlocks.Should().Equal(exit);

            exit.Should().BeOfType<ExitBlock>();
        }

        [TestMethod]
        public void Cfg_TryCatch_MultipleReturnsInTry()
        {
            var cfg = Build(@"
                beforeTry();
                try
                {
                    if (cond) return;
                    insideOne();
                    if (cond) return;
                    insideTwo();
                }
                catch
                {
                    catchOne();
                }
                afterTry();");

            VerifyCfg(cfg, 9);

            var blocks = cfg.Blocks.ToList();

            var beforeOuterTry = (SimpleBlock)blocks[0];
            var firstIf = (BinaryBranchBlock)blocks[1];
            var firstIfReturn = (JumpBlock)blocks[2];
            var secondIf = (BinaryBranchBlock)blocks[3];
            var secondIfReturn = (JumpBlock)blocks[4];
            var tryStatementBranch = (BranchBlock)blocks[5];
            var insideCatch = (SimpleBlock)blocks[6];
            var afterTry = (SimpleBlock)blocks[7];
            var exit = (ExitBlock)blocks[8];

            beforeOuterTry.SuccessorBlocks.Should().Equal(firstIf);

            firstIf.TrueSuccessorBlock.Should().Be(firstIfReturn);
            firstIfReturn.SuccessorBlock.Should().Be(exit);
            firstIf.FalseSuccessorBlock.Should().Be(secondIf);

            secondIf.TrueSuccessorBlock.Should().Be(secondIfReturn);
            secondIfReturn.SuccessorBlock.Should().Be(exit);
            secondIf.FalseSuccessorBlock.Should().Be(tryStatementBranch);

            // ToDo: this tryStatementBranch is not always used as such, or is it?
            tryStatementBranch.SuccessorBlocks.Should().BeEquivalentTo(new[] { insideCatch, afterTry });

            insideCatch.SuccessorBlocks.Should().Equal(afterTry);
            afterTry.SuccessorBlocks.Should().Equal(exit);

            exit.Should().BeOfType<ExitBlock>();
        }

        #endregion

        #region Switch

        [TestMethod]
        public void Cfg_Switch()
        {
            var cfg = Build("cw0(); switch(a) { case 1: case 2: cw1(); break; case 3: default: case 4: cw2(); break; } cw3();");
            VerifyCfg(cfg, 9);

            var blocks = cfg.Blocks.ToList();

            var cw0 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw0")) as BranchBlock;
            var cw1 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw1")) as JumpBlock;
            var cw2 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw2")) as JumpBlock;
            var cw3 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw3"));

            var case1Jump = blocks[3] as JumpBlock;
            var defaultCaseJump = blocks[6] as JumpBlock;

            var branchCase1 = blocks[1] as BinaryBranchBlock;
            var branchCase2 = blocks[2] as BinaryBranchBlock;
            var branchCase3 = blocks[4] as BinaryBranchBlock;
            var branchDefault = blocks[5] as BinaryBranchBlock;

            var exitBlock = cfg.ExitBlock;

            cw0.Should().BeSameAs(cfg.EntryBlock);

            cw0.SuccessorBlocks.Should().Equal(branchCase1);
            branchCase1.TrueSuccessorBlock.Should().Be(case1Jump);
            branchCase1.FalseSuccessorBlock.Should().Be(branchCase2);

            branchCase2.TrueSuccessorBlock.Should().Be(case1Jump);
            branchCase2.FalseSuccessorBlock.Should().Be(branchCase3);

            case1Jump.SuccessorBlocks.Should().Equal(cw3);

            branchCase3.TrueSuccessorBlock.Should().Be(defaultCaseJump);
            branchCase3.FalseSuccessorBlock.Should().Be(branchDefault);

            branchDefault.TrueSuccessorBlock.Should().Be(defaultCaseJump);
            branchDefault.FalseSuccessorBlock.Should().Be(defaultCaseJump);

            defaultCaseJump.SuccessorBlocks.Should().Equal(cw3);

            cw1.SuccessorBlocks.Should().Equal(cw3);
            cw2.SuccessorBlocks.Should().Equal(cw3);
            cw3.SuccessorBlocks.Should().Equal(exitBlock);

            VerifyAllInstructions(cfg.EntryBlock, "cw0", "cw0()", "a");
            VerifyAllInstructions(cw1, "cw1", "cw1()");
        }

        [TestMethod]
        public void Cfg_Switch_NoDefault()
        {
            var cfg = Build("cw0(); switch(a) { case 1: case 2: cw1(); break; case 3: case 4: cw2(); break; } cw3();");
            VerifyCfg(cfg, 9);

            var blocks = cfg.Blocks.ToList();

            var cw0 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw0")) as BranchBlock;
            var cw1 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw1")) as JumpBlock;
            var cw2 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw2")) as JumpBlock;
            var cw3 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw3"));

            var case1Jump = blocks[3] as JumpBlock;
            var case3Jump = blocks[6] as JumpBlock;

            var branchCase1 = blocks[1] as BinaryBranchBlock;
            var branchCase2 = blocks[2] as BinaryBranchBlock;
            var branchCase3 = blocks[4] as BinaryBranchBlock;
            var branchDefault = blocks[5] as BinaryBranchBlock;

            var exitBlock = cfg.ExitBlock;

            cw0.Should().BeSameAs(cfg.EntryBlock);

            cw0.SuccessorBlocks.Should().Equal(branchCase1);
            case1Jump.SuccessorBlocks.Should().Equal(cw3);
            case3Jump.SuccessorBlocks.Should().Equal(cw3);

            cw1.SuccessorBlocks.Should().Equal(cw3);
            cw2.SuccessorBlocks.Should().Equal(cw3);
            cw3.SuccessorBlocks.Should().Equal(exitBlock);

            branchCase2.Should().NotBeNull();
            branchCase3.Should().NotBeNull();
            branchDefault.Should().NotBeNull();
        }

        [TestMethod]
        public void Cfg_Switch_GotoCase()
        {
            var cfg = Build("cw0(); switch(a) { case 1: case 2: cw1(); goto case 3; case 3: default: case 4: cw2(); break; } cw3();");
            VerifyCfg(cfg, 9);

            var blocks = cfg.Blocks.ToList();

            var cw0 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw0")) as BranchBlock;
            var cw1 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw1")) as JumpBlock;
            var cw2 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw2")) as JumpBlock;
            var cw3 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw3"));

            var case1Jump = blocks[3] as JumpBlock;
            var defaultCaseJump = blocks[6] as JumpBlock;

            var branchCase1 = blocks[1] as BinaryBranchBlock;
            var branchCase2 = blocks[2] as BinaryBranchBlock;
            var branchCase3 = blocks[4] as BinaryBranchBlock;
            var branchDefault = blocks[5] as BinaryBranchBlock;

            var exitBlock = cfg.ExitBlock;

            cw0.Should().BeSameAs(cfg.EntryBlock);

            cw0.SuccessorBlocks.Should().Equal(branchCase1);
            case1Jump.SuccessorBlocks.Should().Equal(defaultCaseJump);
            defaultCaseJump.SuccessorBlocks.Should().Equal(cw3);

            branchCase1.TrueSuccessorBlock.Should().Be(case1Jump);
            branchCase1.FalseSuccessorBlock.Should().Be(branchCase2);

            branchCase2.TrueSuccessorBlock.Should().Be(case1Jump);
            branchCase2.FalseSuccessorBlock.Should().Be(branchCase3);

            branchCase3.TrueSuccessorBlock.Should().Be(defaultCaseJump);
            branchCase3.FalseSuccessorBlock.Should().Be(branchDefault);

            branchDefault.TrueSuccessorBlock.Should().Be(defaultCaseJump);
            branchDefault.FalseSuccessorBlock.Should().Be(defaultCaseJump);

            cw1.SuccessorBlocks.Should().Equal(defaultCaseJump);
            cw2.SuccessorBlocks.Should().Equal(cw3);
            cw3.SuccessorBlocks.Should().Equal(exitBlock);
        }

        [TestMethod]
        public void Cfg_Switch_Null()
        {
            var cfg = Build("cw0(); switch(a) { case \"\": case null: cw1(); break; case \"a\": cw2(); goto case null; } cw3();");
            VerifyCfg(cfg, 8);

            var blocks = cfg.Blocks.ToList();

            var cw0 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw0")) as BranchBlock;
            var cw3 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw3"));

            var caseEmptyJump = blocks[3] as JumpBlock;
            var caseAJump = blocks[5] as JumpBlock;

            var branchEmpty = blocks[1] as BinaryBranchBlock;
            var branchNull = blocks[2] as BinaryBranchBlock;
            var branchA = blocks[4] as BinaryBranchBlock;

            cw0.Should().BeSameAs(cfg.EntryBlock);

            cw0.SuccessorBlocks.Should().Equal(branchEmpty);
            caseEmptyJump.SuccessorBlocks.Should().Equal(cw3);
            caseAJump.SuccessorBlocks.Should().Equal(caseEmptyJump);

            branchEmpty.TrueSuccessorBlock.Should().Be(caseEmptyJump);
            branchEmpty.FalseSuccessorBlock.Should().Be(branchNull);

            branchNull.TrueSuccessorBlock.Should().Be(caseEmptyJump);
            branchNull.FalseSuccessorBlock.Should().Be(branchA);

            branchA.TrueSuccessorBlock.Should().Be(caseAJump);
            branchA.FalseSuccessorBlock.Should().Be(cw3);
        }

        [TestMethod]
        public void Cfg_Switch_GotoDefault()
        {
            var cfg = Build("cw0(); switch(a) { case 1: case 2: cw1(); goto default; case 3: default: case 4: cw2(); break; } cw3();");
            VerifyCfg(cfg, 9);

            var blocks = cfg.Blocks.ToList();

            var cw0 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw0")) as BranchBlock;
            var cw1 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw1")) as JumpBlock;
            var cw2 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw2")) as JumpBlock;
            var cw3 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw3"));

            var case1Jump = blocks[3] as JumpBlock;
            var defaultCaseJump = blocks[6] as JumpBlock;

            var branchCase1 = blocks[1] as BinaryBranchBlock;
            var branchCase2 = blocks[2] as BinaryBranchBlock;
            var branchCase3 = blocks[4] as BinaryBranchBlock;
            var branchDefault = blocks[5] as BinaryBranchBlock;

            var exitBlock = cfg.ExitBlock;

            cw0.Should().BeSameAs(cfg.EntryBlock);

            cw0.SuccessorBlocks.Should().Equal(branchCase1);
            case1Jump.SuccessorBlocks.Should().Equal(defaultCaseJump);
            defaultCaseJump.SuccessorBlocks.Should().Equal(cw3);

            branchCase1.TrueSuccessorBlock.Should().Be(case1Jump);
            branchCase1.FalseSuccessorBlock.Should().Be(branchCase2);

            branchCase2.TrueSuccessorBlock.Should().Be(case1Jump);
            branchCase2.FalseSuccessorBlock.Should().Be(branchCase3);

            branchCase3.TrueSuccessorBlock.Should().Be(defaultCaseJump);
            branchCase3.FalseSuccessorBlock.Should().Be(branchDefault);

            branchDefault.TrueSuccessorBlock.Should().Be(defaultCaseJump);
            branchDefault.FalseSuccessorBlock.Should().Be(defaultCaseJump);

            cw1.SuccessorBlocks.Should().Equal(defaultCaseJump);
            cw2.SuccessorBlocks.Should().Equal(cw3);
            cw3.SuccessorBlocks.Should().Equal(exitBlock);
        }

        [TestMethod]
        public void Cfg_Switch_Patterns_Default()
        {
            var cfg = Build("cw0(); switch(o) { case int i: case string s: cw1(); break; default: case double d: cw2(); break; } cw3();");

            VerifyCfg(cfg, 8);

            var switchBlock = (BranchBlock)cfg.Blocks.ElementAt(0);
            var caseIntBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(1);
            var caseStringBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(2);
            var firstSectionBlock = (JumpBlock)cfg.Blocks.ElementAt(3);
            var caseDoubleBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(4);
            var secondSection_DefaultBlock = (JumpBlock)cfg.Blocks.ElementAt(5);
            var lastBlock = (SimpleBlock)cfg.Blocks.ElementAt(6);
            var exitBlock = (ExitBlock)cfg.Blocks.ElementAt(7);

            switchBlock.SuccessorBlocks.Should().Equal(caseIntBlock);
            VerifyAllInstructions(switchBlock, "cw0", "cw0()", "o");

            caseIntBlock.TrueSuccessorBlock.Should().Be(firstSectionBlock);
            caseIntBlock.FalseSuccessorBlock.Should().Be(caseStringBlock);
            VerifyAllInstructions(caseIntBlock, "int i");

            caseStringBlock.TrueSuccessorBlock.Should().Be(firstSectionBlock);
            caseStringBlock.FalseSuccessorBlock.Should().Be(caseDoubleBlock);
            VerifyAllInstructions(caseStringBlock, "string s");

            firstSectionBlock.SuccessorBlock.Should().Be(lastBlock);
            VerifyAllInstructions(firstSectionBlock, "cw1", "cw1()");

            caseDoubleBlock.TrueSuccessorBlock.Should().Be(secondSection_DefaultBlock);
            caseDoubleBlock.FalseSuccessorBlock.Should().Be(secondSection_DefaultBlock);
            VerifyAllInstructions(caseDoubleBlock, "double d");

            secondSection_DefaultBlock.SuccessorBlock.Should().Be(lastBlock);
            VerifyAllInstructions(secondSection_DefaultBlock, "cw2", "cw2()");

            lastBlock.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(lastBlock, "cw3", "cw3()");
        }

        [TestMethod]
        public void Cfg_Switch_Patterns_Two_Case_When()
        {
            var cfg = Build("cw0(); switch(o) { case int i when i > 0: case string s when s.Length > 0: cw1(); break; } cw2();");

            VerifyCfg(cfg, 8);

            var switchBlock = (BranchBlock)cfg.Blocks.ElementAt(0);
            var caseIntBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(1);
            var caseIntWhenBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(2);
            var caseStringBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(3);
            var caseStringWhenBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(4);
            var firstSectionBlock = (JumpBlock)cfg.Blocks.ElementAt(5);
            var lastBlock = (SimpleBlock)cfg.Blocks.ElementAt(6);
            var exitBlock = (ExitBlock)cfg.Blocks.ElementAt(7);

            switchBlock.SuccessorBlocks.Should().Equal(caseIntBlock);
            VerifyAllInstructions(switchBlock, "cw0", "cw0()", "o");

            caseIntBlock.TrueSuccessorBlock.Should().Be(caseIntWhenBlock);
            caseIntBlock.FalseSuccessorBlock.Should().Be(caseStringBlock);
            VerifyAllInstructions(caseIntBlock, "int i");

            caseIntWhenBlock.TrueSuccessorBlock.Should().Be(firstSectionBlock);
            caseIntWhenBlock.FalseSuccessorBlock.Should().Be(caseStringBlock);
            VerifyAllInstructions(caseIntWhenBlock, "i", "0", "i > 0");

            caseStringBlock.TrueSuccessorBlock.Should().Be(caseStringWhenBlock);
            caseStringBlock.FalseSuccessorBlock.Should().Be(lastBlock);
            VerifyAllInstructions(caseStringBlock, "string s");

            caseStringWhenBlock.TrueSuccessorBlock.Should().Be(firstSectionBlock);
            caseStringWhenBlock.FalseSuccessorBlock.Should().Be(lastBlock);
            VerifyAllInstructions(caseStringWhenBlock, "s", "s.Length", "0", "s.Length > 0");

            firstSectionBlock.SuccessorBlock.Should().Be(lastBlock);
            VerifyAllInstructions(firstSectionBlock, "cw1", "cw1()");

            lastBlock.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(lastBlock, "cw2", "cw2()");
        }

        [TestMethod]
        public void Cfg_Switch_One_Simple_Case_And_One_Case_With_When()
        {
            var cfg = Build("cw(); switch(o) { case 0 : cw0(); break; case 1 when s: cw1(); break; } cw2();");

            VerifyCfg(cfg, 8);

            var switchBlock = (BranchBlock)cfg.Blocks.ElementAt(0);
            var caseZero = (BinaryBranchBlock)cfg.Blocks.ElementAt(1);
            var caseZeroBlock = (JumpBlock)cfg.Blocks.ElementAt(2);
            var caseOne = (BinaryBranchBlock)cfg.Blocks.ElementAt(3);
            var caseOneWhenBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(4);
            var caseOneWhenBlockBody = (JumpBlock)cfg.Blocks.ElementAt(5);
            var afterSwitchBlock = (SimpleBlock)cfg.Blocks.ElementAt(6);
            var exitBlock = (ExitBlock)cfg.Blocks.ElementAt(7);

            switchBlock.SuccessorBlocks.Should().Equal(caseZero);
            VerifyAllInstructions(switchBlock, "cw", "cw()", "o");

            caseZero.TrueSuccessorBlock.Should().Be(caseZeroBlock);
            caseZero.FalseSuccessorBlock.Should().Be(caseOne);
            caseZero.BranchingNode.Kind().Should().Be(SyntaxKind.CaseSwitchLabel);

            caseZeroBlock.SuccessorBlock.Should().Be(afterSwitchBlock);
            VerifyAllInstructions(caseZeroBlock, "cw0", "cw0()");

            caseOne.TrueSuccessorBlock.Should().Be(caseOneWhenBlock);
            caseOne.FalseSuccessorBlock.Should().Be(afterSwitchBlock);
            caseOne.BranchingNode.Kind().Should().Be(SyntaxKind.CasePatternSwitchLabel);

            caseOneWhenBlock.TrueSuccessorBlock.Should().Be(caseOneWhenBlockBody);
            caseOneWhenBlock.FalseSuccessorBlock.Should().Be(afterSwitchBlock);
            VerifyAllInstructions(caseOneWhenBlock, "s");

            caseOneWhenBlockBody.SuccessorBlock.Should().Be(afterSwitchBlock);
            VerifyAllInstructions(caseOneWhenBlockBody, "cw1", "cw1()");

            afterSwitchBlock.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(afterSwitchBlock, "cw2", "cw2()");
        }

        [TestMethod]
        public void Cfg_Switch_Case_With_When_And_Default()
        {
            var cfg = Build("cw(); switch(o) { case 1 when i > 0: cw0(); break; default: cw1(); break; } cw2();");

            VerifyCfg(cfg, 7);

            var switchBlock = (BranchBlock)cfg.Blocks.ElementAt(0);
            var caseOne = (BranchBlock)cfg.Blocks.ElementAt(1);
            var caseOneWhenBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(2);
            var caseOneWhenBlockBody = (JumpBlock)cfg.Blocks.ElementAt(3);
            var defaultBlock = (SimpleBlock)cfg.Blocks.ElementAt(4);
            var afterSwitchBlock = (SimpleBlock)cfg.Blocks.ElementAt(5);
            var exitBlock = (ExitBlock)cfg.Blocks.ElementAt(6);

            switchBlock.SuccessorBlocks.Should().Equal(caseOne);
            VerifyAllInstructions(switchBlock, "cw", "cw()", "o");

            caseOne.SuccessorBlocks.Should().ContainInOrder(caseOneWhenBlock, defaultBlock);
            caseOne.BranchingNode.Kind().Should().Be(SyntaxKind.CasePatternSwitchLabel);

            caseOneWhenBlock.TrueSuccessorBlock.Should().Be(caseOneWhenBlockBody);
            caseOneWhenBlock.FalseSuccessorBlock.Should().Be(defaultBlock);
            VerifyAllInstructions(caseOneWhenBlock, "i", "0", "i > 0");

            caseOneWhenBlockBody.SuccessorBlock.Should().Be(afterSwitchBlock);
            VerifyAllInstructions(caseOneWhenBlockBody, "cw0", "cw0()");

            afterSwitchBlock.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(afterSwitchBlock, "cw2", "cw2()");
        }

        [TestMethod]
        public void Cfg_SwitchExpression_Return()
        {
            var cfg = Build(@"
var type = ""test"";
return type switch
{
    ""a"" => 1,
    ""b"" => 2,
    _ => 3
};");
            VerifyCfg(cfg, 7);

            // The generated CFG is very similar to the one generated for the following conditional expression:
            // return type == "a" ? 1 : (type == "b" ? 2 : 3);

            var aArm = (BinaryBranchBlock)cfg.Blocks.ElementAt(0);
            var aTrue = (SimpleBlock)cfg.Blocks.ElementAt(1);
            var bArm = (BinaryBranchBlock)cfg.Blocks.ElementAt(2);
            var bTrue = (SimpleBlock)cfg.Blocks.ElementAt(3);
            var discardArm = (SimpleBlock)cfg.Blocks.ElementAt(4);
            var returnStatement = (JumpBlock)cfg.Blocks.ElementAt(5);
            var exitBlock = cfg.ExitBlock;

            aArm.TrueSuccessorBlock.Should().Be(aTrue);
            aArm.FalseSuccessorBlock.Should().Be(bArm);
            VerifyAllInstructions(aArm, "\"test\"", "type = \"test\"", "type", "\"a\"");

            aTrue.SuccessorBlock.Should().Be(returnStatement);
            VerifyAllInstructions(aTrue, "1");

            bArm.TrueSuccessorBlock.Should().Be(bTrue);
            bArm.FalseSuccessorBlock.Should().Be(discardArm);
            VerifyAllInstructions(bArm, "type", "\"b\"");

            bTrue.SuccessorBlock.Should().Be(returnStatement);
            VerifyAllInstructions(bTrue, "2");

            discardArm.SuccessorBlock.Should().Be(returnStatement);
            VerifyAllInstructions(discardArm, "3");

            returnStatement.SuccessorBlock.Should().Be(exitBlock);
            VerifyNoInstruction(returnStatement);
        }

        [TestMethod]
        public void Cfg_SwitchExpression_Assignment()
        {
            var cfg = Build(@"var type = ""test""; var result = type switch {""a"" => 1, ""b"" => 2, _ => 3};");
            VerifyCfg(cfg, 7);

            var aArm = (BinaryBranchBlock)cfg.Blocks.ElementAt(0);
            var aTrue = (SimpleBlock)cfg.Blocks.ElementAt(1);
            var bArm = (BinaryBranchBlock)cfg.Blocks.ElementAt(2);
            var bTrue = (SimpleBlock)cfg.Blocks.ElementAt(3);
            var discardArm = (SimpleBlock)cfg.Blocks.ElementAt(4);
            var assignment = (SimpleBlock)cfg.Blocks.ElementAt(5);
            var exitBlock = cfg.ExitBlock;

            aArm.TrueSuccessorBlock.Should().Be(aTrue);
            aArm.FalseSuccessorBlock.Should().Be(bArm);
            VerifyAllInstructions(aArm, "\"test\"", "type = \"test\"", "type", "\"a\"");

            aTrue.SuccessorBlock.Should().Be(assignment);
            VerifyAllInstructions(aTrue, "1");

            bArm.TrueSuccessorBlock.Should().Be(bTrue);
            bArm.FalseSuccessorBlock.Should().Be(discardArm);
            VerifyAllInstructions(bArm, "type", "\"b\"");

            bTrue.SuccessorBlock.Should().Be(assignment);
            VerifyAllInstructions(bTrue, "2");

            discardArm.SuccessorBlock.Should().Be(assignment);
            VerifyAllInstructions(discardArm, "3");

            assignment.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(assignment, @"result = type switch {""a"" => 1, ""b"" => 2, _ => 3}");
        }

        [TestMethod]
        public void Cfg_SwitchExpression_InnerSwitch()
        {
            var cfg = Build(@"
string first = ""foo"", second = ""bar"";
var result = first switch {""a"" => second switch {""x"" => 1, _ => 2}, ""b"" => 3, _ => 4};");

            VerifyCfg(cfg, 9);

            var aArm = (BinaryBranchBlock)cfg.Blocks.ElementAt(0);
            var xArm = (BinaryBranchBlock)cfg.Blocks.ElementAt(1);
            var xTrue = (SimpleBlock)cfg.Blocks.ElementAt(2);
            var secondSwitchDiscardArm = (SimpleBlock)cfg.Blocks.ElementAt(3);
            var bArm = (BinaryBranchBlock)cfg.Blocks.ElementAt(4);
            var bTrue = (SimpleBlock)cfg.Blocks.ElementAt(5);
            var firstSwitchDiscard = (SimpleBlock)cfg.Blocks.ElementAt(6);
            var assignment = (SimpleBlock)cfg.Blocks.ElementAt(7);
            var exitBlock = cfg.ExitBlock;

            aArm.TrueSuccessorBlock.Should().Be(xArm);
            aArm.FalseSuccessorBlock.Should().Be(bArm);
            VerifyAllInstructions(aArm, "\"foo\"", "first = \"foo\"", "\"bar\"", "second = \"bar\"", "first", "\"a\"");

            xArm.TrueSuccessorBlock.Should().Be(xTrue);
            xArm.FalseSuccessorBlock.Should().Be(secondSwitchDiscardArm);
            VerifyAllInstructions(xArm, "second", "\"x\"");

            xTrue.SuccessorBlock.Should().Be(assignment);
            VerifyAllInstructions(xTrue, "1");

            secondSwitchDiscardArm.SuccessorBlock.Should().Be(assignment);
            VerifyAllInstructions(secondSwitchDiscardArm, "2");

            bArm.TrueSuccessorBlock.Should().Be(bTrue);
            bArm.FalseSuccessorBlock.Should().Be(firstSwitchDiscard);
            VerifyAllInstructions(bArm, "first", "\"b\"");

            bTrue.SuccessorBlock.Should().Be(assignment);
            VerifyAllInstructions(bTrue, "3");

            firstSwitchDiscard.SuccessorBlock.Should().Be(assignment);
            VerifyAllInstructions(firstSwitchDiscard, "4");

            assignment.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(assignment, @"result = first switch {""a"" => second switch {""x"" => 1, _ => 2}, ""b"" => 3, _ => 4}");
        }

        [TestMethod]
        public void Cfg_SwitchExpression_WhenClause()
        {
            var cfg = Build(@"string first = ""foo"", second = ""bar""; var result = first switch {""a"" when second == ""bar"" => 1, ""a"" => 2, ""b"" => 3, _ => 4};");

            VerifyCfg(cfg, 10);

            var aWithWhenClauseArm = (BinaryBranchBlock)cfg.EntryBlock;
            var whenClause = (BinaryBranchBlock)cfg.Blocks.ElementAt(1);
            var aWithWhenClauseArmTrue = (SimpleBlock)cfg.Blocks.ElementAt(2);
            var aArm = (BinaryBranchBlock)cfg.Blocks.ElementAt(3);
            var aTrue = (SimpleBlock)cfg.Blocks.ElementAt(4);
            var bArm = (BinaryBranchBlock)cfg.Blocks.ElementAt(5);
            var bTrue = (SimpleBlock)cfg.Blocks.ElementAt(6);
            var discardArm = (SimpleBlock)cfg.Blocks.ElementAt(7);
            var assignment = (SimpleBlock)cfg.Blocks.ElementAt(8);
            var exitBlock = cfg.ExitBlock;

            aWithWhenClauseArm.TrueSuccessorBlock.Should().Be(whenClause);
            aWithWhenClauseArm.FalseSuccessorBlock.Should().Be(aArm);
            VerifyAllInstructions(aWithWhenClauseArm, "\"foo\"", "first = \"foo\"", "\"bar\"", "second = \"bar\"", "first", "\"a\"");

            whenClause.TrueSuccessorBlock.Should().Be(aWithWhenClauseArmTrue);
            whenClause.FalseSuccessorBlock.Should().Be(aArm);
            VerifyAllInstructions(whenClause, "second", "\"bar\"", "second == \"bar\"");

            aWithWhenClauseArmTrue.SuccessorBlock.Should().Be(assignment);
            VerifyAllInstructions(aWithWhenClauseArmTrue, "1");

            aArm.TrueSuccessorBlock.Should().Be(aTrue);
            aArm.FalseSuccessorBlock.Should().Be(bArm);
            VerifyAllInstructions(aArm, "first", "\"a\"");

            aTrue.SuccessorBlock.Should().Be(assignment);
            VerifyAllInstructions(aTrue, "2");

            bArm.TrueSuccessorBlock.Should().Be(bTrue);
            bArm.FalseSuccessorBlock.Should().Be(discardArm);
            VerifyAllInstructions(bArm, "first", "\"b\"");

            bTrue.SuccessorBlock.Should().Be(assignment);
            VerifyAllInstructions(bTrue, "3");

            discardArm.SuccessorBlock.Should().Be(assignment);
            VerifyAllInstructions(discardArm, "4");

            assignment.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(assignment, @"result = first switch {""a"" when second == ""bar"" => 1, ""a"" => 2, ""b"" => 3, _ => 4}");
        }

        [TestMethod]
        public void Cfg_VarPattern_InSwitchExpression_IsNotSupported()
        {
            var exception = Assert.ThrowsException<NotSupportedException>(() => Build(@"string a = taintedString switch {var x => null};"));

            exception.Message.Should().Be("VarPattern");
        }

        [TestMethod]
        public void Cfg_VarPattern_InIf_IsNotSupported()
        {
            var exception = Assert.ThrowsException<NotSupportedException>(() => Build(@"if (tainted is var x) { }"));

            exception.Message.Should().Be("VarPattern");
        }

        [TestMethod]
        public void Cfg_NotPattern_InIf_IsNotSupported()
        {
            var exception = Assert.ThrowsException<NotSupportedException>(() => Build(@"if (tainted is not null) { }"));

            exception.Message.Should().Be("NotPattern");
        }

        [TestMethod]
        public void Cfg_AndPattern_InIf_IsNotSupported()
        {
            var exception = Assert.ThrowsException<NotSupportedException>(() => Build(@"if (tainted is int and > 0) { }"));

            exception.Message.Should().Be("AndPattern");
        }

        [TestMethod]
        public void Cfg_OrPattern_InIf_IsNotSupported()
        {
            var exception = Assert.ThrowsException<NotSupportedException>(() => Build(@"if (tainted is string or int) { }"));

            exception.Message.Should().Be("OrPattern");
        }

        [TestMethod]
        public void Cfg_ParenthesizedPattern_InIf_IsNotSupported()
        {
            var exception = Assert.ThrowsException<NotSupportedException>(() => Build(@"if (tainted is (string s)) { }"));

            exception.Message.Should().Be("ParenthesizedPattern");
        }

        [TestMethod]
        public void Cfg_ListPattern_InIf_IsNotSupported()
        {
            var exception = Assert.ThrowsException<NotSupportedException>(() => Build(@"if (tainted is []) { }"));

            exception.Message.Should().Be("ListPattern");
        }

        [TestMethod]
        public void Cfg_Switch_Patterns_NoDefault()
        {
            var cfg = Build("cw0(); switch(o) { case int i: case string s: cw1(); break; case double d: cw2(); break; } cw3();");

            VerifyCfg(cfg, 8);

            var switchBlock = (BranchBlock)cfg.Blocks.ElementAt(0);
            var caseIntBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(1);
            var caseStringBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(2);
            var firstSectionBlock = (JumpBlock)cfg.Blocks.ElementAt(3);
            var caseDoubleBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(4);
            var secondSectionBlock = (JumpBlock)cfg.Blocks.ElementAt(5);
            var lastBlock = (SimpleBlock)cfg.Blocks.ElementAt(6);
            var exitBlock = (ExitBlock)cfg.Blocks.ElementAt(7);

            switchBlock.SuccessorBlocks.Should().Equal(caseIntBlock);
            VerifyAllInstructions(switchBlock, "cw0", "cw0()", "o");

            caseIntBlock.TrueSuccessorBlock.Should().Be(firstSectionBlock);
            caseIntBlock.FalseSuccessorBlock.Should().Be(caseStringBlock);
            VerifyAllInstructions(caseIntBlock, "int i");

            caseStringBlock.TrueSuccessorBlock.Should().Be(firstSectionBlock);
            caseStringBlock.FalseSuccessorBlock.Should().Be(caseDoubleBlock);
            VerifyAllInstructions(caseStringBlock, "string s");

            firstSectionBlock.SuccessorBlock.Should().Be(lastBlock);
            VerifyAllInstructions(firstSectionBlock, "cw1", "cw1()");

            caseDoubleBlock.TrueSuccessorBlock.Should().Be(secondSectionBlock);
            caseDoubleBlock.FalseSuccessorBlock.Should().Be(lastBlock);
            VerifyAllInstructions(caseDoubleBlock, "double d");

            secondSectionBlock.SuccessorBlock.Should().Be(lastBlock);
            VerifyAllInstructions(secondSectionBlock, "cw2", "cw2()");

            lastBlock.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(lastBlock, "cw3", "cw3()");
        }

        [TestMethod]
        public void Cfg_Switch_Patterns_GotoDefault()
        {
            var cfg = Build("cw0(); switch(o) { case int i: case string s: cw1(); goto default; default: case double d: cw2(); break; } cw3();");

            VerifyCfg(cfg, 8);

            var switchBlock = (BranchBlock)cfg.Blocks.ElementAt(0);
            var caseIntBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(1);
            var caseStringBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(2);
            var firstSectionBlock = (JumpBlock)cfg.Blocks.ElementAt(3);
            var caseDoubleBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(4);
            var secondSection_DefaultBlock = (JumpBlock)cfg.Blocks.ElementAt(5);
            var lastBlock = (SimpleBlock)cfg.Blocks.ElementAt(6);
            var exitBlock = (ExitBlock)cfg.Blocks.ElementAt(7);

            switchBlock.SuccessorBlocks.Should().Equal(caseIntBlock);
            VerifyAllInstructions(switchBlock, "cw0", "cw0()", "o");

            caseIntBlock.TrueSuccessorBlock.Should().Be(firstSectionBlock);
            caseIntBlock.FalseSuccessorBlock.Should().Be(caseStringBlock);
            VerifyAllInstructions(caseIntBlock, "int i");

            caseStringBlock.TrueSuccessorBlock.Should().Be(firstSectionBlock);
            caseStringBlock.FalseSuccessorBlock.Should().Be(caseDoubleBlock);
            VerifyAllInstructions(caseStringBlock, "string s");

            firstSectionBlock.SuccessorBlock.Should().Be(secondSection_DefaultBlock);
            VerifyAllInstructions(firstSectionBlock, "cw1", "cw1()");

            caseDoubleBlock.TrueSuccessorBlock.Should().Be(secondSection_DefaultBlock);
            caseDoubleBlock.FalseSuccessorBlock.Should().Be(secondSection_DefaultBlock);
            VerifyAllInstructions(caseDoubleBlock, "double d");

            secondSection_DefaultBlock.SuccessorBlock.Should().Be(lastBlock);
            VerifyAllInstructions(secondSection_DefaultBlock, "cw2", "cw2()");

            lastBlock.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(lastBlock, "cw3", "cw3()");
        }

        [TestMethod]
        public void Cfg_Switch_Patterns_Null()
        {
            var cfg = Build("cw0(); switch(o) { case int i: case null: cw1(); break; } cw2();");

            VerifyCfg(cfg, 6);

            var switchBlock = (BranchBlock)cfg.Blocks.ElementAt(0);
            var caseIntBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(1);
            var caseNullBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(2);
            var firstSectionBlock = (JumpBlock)cfg.Blocks.ElementAt(3);
            var lastBlock = (SimpleBlock)cfg.Blocks.ElementAt(4);
            var exitBlock = (ExitBlock)cfg.Blocks.ElementAt(5);

            switchBlock.SuccessorBlocks.Should().Equal(caseIntBlock);
            VerifyAllInstructions(switchBlock, "cw0", "cw0()", "o");

            caseIntBlock.TrueSuccessorBlock.Should().Be(firstSectionBlock);
            caseIntBlock.FalseSuccessorBlock.Should().Be(caseNullBlock);
            VerifyAllInstructions(caseIntBlock, "int i");

            caseNullBlock.TrueSuccessorBlock.Should().Be(firstSectionBlock);
            caseNullBlock.FalseSuccessorBlock.Should().Be(lastBlock);
            VerifyAllInstructions(caseNullBlock, "o");

            firstSectionBlock.SuccessorBlock.Should().Be(lastBlock);
            VerifyAllInstructions(firstSectionBlock, "cw1", "cw1()");

            lastBlock.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(lastBlock, "cw2", "cw2()");
        }

        [TestMethod]
        public void Cfg_Switch_Patterns_Null_Default()
        {
            var cfg = Build(@"cw0(); switch(o) { case int i: cw1(); break; case null: cw2(); break; default: cw3(); break; } cw4();");

            VerifyCfg(cfg, 8);

            var switchBlock = (BranchBlock)cfg.Blocks.ElementAt(0);
            var caseIntBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(1);
            var intSectionBlock = (JumpBlock)cfg.Blocks.ElementAt(2);
            var caseNullBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(3);
            var nullSectionBlock = (JumpBlock)cfg.Blocks.ElementAt(4);
            var defaultSectionBlock = (JumpBlock)cfg.Blocks.ElementAt(5);
            var lastBlock = (SimpleBlock)cfg.Blocks.ElementAt(6);
            var exitBlock = (ExitBlock)cfg.Blocks.ElementAt(7);

            switchBlock.SuccessorBlocks.Should().Equal(caseIntBlock);
            VerifyAllInstructions(switchBlock, "cw0", "cw0()", "o");

            caseIntBlock.TrueSuccessorBlock.Should().Be(intSectionBlock);
            caseIntBlock.FalseSuccessorBlock.Should().Be(caseNullBlock);
            VerifyAllInstructions(caseIntBlock, "int i");

            intSectionBlock.SuccessorBlock.Should().Be(lastBlock);
            VerifyAllInstructions(intSectionBlock, "cw1", "cw1()");

            caseNullBlock.TrueSuccessorBlock.Should().Be(nullSectionBlock);
            caseNullBlock.FalseSuccessorBlock.Should().Be(defaultSectionBlock);
            VerifyAllInstructions(caseNullBlock, "o");

            nullSectionBlock.SuccessorBlock.Should().Be(lastBlock);
            VerifyAllInstructions(nullSectionBlock, "cw2", "cw2()");

            defaultSectionBlock.SuccessorBlock.Should().Be(lastBlock);
            VerifyAllInstructions(defaultSectionBlock, "cw3", "cw3()");

            lastBlock.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(lastBlock, "cw4", "cw4()");
        }

        [TestMethod]
        public void Cfg_String_And_Throw()
        {
            var cfg = Build(@"
cw();
switch(o) // switchBlock
{
   case ""0"": cw0(); break; // caseZero
   default: // defaultBlock
      throw new InvalidOperationException("""");
}
cw1(); // afterSwitchBlock
");
            VerifyCfg(cfg, 6);

            var switchBlock = (BranchBlock)cfg.Blocks.ElementAt(0);
            var branchBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(1);
            var caseZero = (JumpBlock)cfg.Blocks.ElementAt(2);
            var defaultBlock = (JumpBlock)cfg.Blocks.ElementAt(3);
            var afterSwitchBlock = (SimpleBlock)cfg.Blocks.ElementAt(4);
            var exitBlock = (ExitBlock)cfg.Blocks.ElementAt(5);

            switchBlock.SuccessorBlocks.Should().Equal(branchBlock);
            VerifyAllInstructions(switchBlock, "cw", "cw()", "o");

            branchBlock.TrueSuccessorBlock.Should().Be(caseZero);
            branchBlock.FalseSuccessorBlock.Should().Be(defaultBlock);
            VerifyAllInstructions(branchBlock, "o");

            caseZero.SuccessorBlock.Should().Be(afterSwitchBlock);
            VerifyAllInstructions(caseZero, "cw0", "cw0()");

            defaultBlock.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(defaultBlock, "\"\"", "new InvalidOperationException(\"\")");
        }

        [TestMethod]
        public void Cfg_Throws()
        {
            var cfg = Build(@"
cw();
switch(o) // switchBlock
{
   case 1:
    if (b) // firstCaseIfBlock
    {
        cw0(); // trueBranchBlock
    }
    else
    {
      throw new InvalidOperationException(""""); // falseBranchBlock
    }
    break;

   default: // defaultThrowBlock
      throw new InvalidOperationException(""a"");
}
cw1(); // afterSwitchBlock
");
            VerifyCfg(cfg, 9);

            var switchBlock = (BranchBlock)cfg.Blocks.ElementAt(0);
            var case1Block = (BinaryBranchBlock)cfg.Blocks.ElementAt(1);
            var firstCaseIfBlock = (BinaryBranchBlock)cfg.Blocks.ElementAt(2);
            var trueBranchBlock = (SimpleBlock)cfg.Blocks.ElementAt(3);
            var falseBranchBlock = (JumpBlock)cfg.Blocks.ElementAt(4);
            var breakJump = (JumpBlock)cfg.Blocks.ElementAt(5);
            var defaultBranchBlock = (JumpBlock)cfg.Blocks.ElementAt(6);
            var afterSwitchBlock = (SimpleBlock)cfg.Blocks.ElementAt(7);
            var exitBlock = (ExitBlock)cfg.Blocks.ElementAt(8);

            switchBlock.SuccessorBlocks.Should().Equal(case1Block);
            case1Block.TrueSuccessorBlock.Should().Be(firstCaseIfBlock);
            case1Block.FalseSuccessorBlock.Should().Be(defaultBranchBlock);
            firstCaseIfBlock.TrueSuccessorBlock.Should().Be(trueBranchBlock);
            firstCaseIfBlock.FalseSuccessorBlock.Should().Be(falseBranchBlock);
            trueBranchBlock.SuccessorBlocks.Should().Equal(breakJump);
            breakJump.SuccessorBlocks.Should().Equal(afterSwitchBlock);
            afterSwitchBlock.SuccessorBlocks.Should().Equal(exitBlock);
            falseBranchBlock.SuccessorBlocks.Should().Equal(exitBlock);
            defaultBranchBlock.SuccessorBlocks.Should().Equal(exitBlock);
        }

        [TestMethod]
        public void Cfg_Enumerable_Patterns()
        {
            var cfg = Build(@"
cw();
switch(o)
{
   case IEnumerable<object> subList when subList.Any() && b && sum > 0 : cw0(); break;
   default: cw2(); break;
}
cw3();
");
            VerifyCfg(cfg, 9);

            var switchBlock = (BranchBlock)cfg.Blocks.ElementAt(0);
            var patternCase = (BinaryBranchBlock)cfg.Blocks.ElementAt(1);
            var sublistAnyCondition = (BinaryBranchBlock)cfg.Blocks.ElementAt(2);
            var bCondition = (BinaryBranchBlock)cfg.Blocks.ElementAt(3);
            var sumGreaterCondition = (BinaryBranchBlock)cfg.Blocks.ElementAt(4);
            var caseInnerBlock = (JumpBlock)cfg.Blocks.ElementAt(5);
            var defaultBlock = (JumpBlock)cfg.Blocks.ElementAt(6);
            var afterSwitchBlock = (SimpleBlock)cfg.Blocks.ElementAt(7);
            var exitBlock = (ExitBlock)cfg.Blocks.ElementAt(8);

            switchBlock.SuccessorBlocks.Should().Equal(patternCase);

            patternCase.TrueSuccessorBlock.Should().Be(sublistAnyCondition);
            patternCase.FalseSuccessorBlock.Should().Be(defaultBlock);

            sublistAnyCondition.TrueSuccessorBlock.Should().Be(bCondition);
            sublistAnyCondition.FalseSuccessorBlock.Should().Be(defaultBlock);

            bCondition.TrueSuccessorBlock.Should().Be(sumGreaterCondition);
            bCondition.FalseSuccessorBlock.Should().Be(defaultBlock);

            sumGreaterCondition.TrueSuccessorBlock.Should().Be(caseInnerBlock);
            sumGreaterCondition.FalseSuccessorBlock.Should().Be(defaultBlock);

            caseInnerBlock.SuccessorBlock.Should().Be(afterSwitchBlock);
            defaultBlock.SuccessorBlock.Should().Be(afterSwitchBlock);
            afterSwitchBlock.SuccessorBlock.Should().Be(exitBlock);
        }

        [TestMethod]
        public void Cfg_Default_Statement_First()
        {
            var cfg = Build(@"
int index = 0;
Exception ex = null;
switch (index)
{
    default:
        break;

    case 0 when ex is InvalidOperationException:
        ex = null;
        break;
}
");
            VerifyCfg(cfg, 6);

            var switchBlock = (BranchBlock)cfg.Blocks.ElementAt(0);
            var caseZero = (BinaryBranchBlock)cfg.Blocks.ElementAt(1);
            var caseZeroWhenException = (BinaryBranchBlock)cfg.Blocks.ElementAt(2);
            var caseZeroWhenExceptionBlock = (JumpBlock)cfg.Blocks.ElementAt(3);
            var defaultBlock = (JumpBlock)cfg.Blocks.ElementAt(4);
            var exitBlock = (ExitBlock)cfg.Blocks.ElementAt(5);

            switchBlock.SuccessorBlocks.Should().Equal(caseZero);

            caseZero.TrueSuccessorBlock.Should().Be(caseZeroWhenException);
            caseZero.FalseSuccessorBlock.Should().Be(defaultBlock);

            caseZeroWhenException.TrueSuccessorBlock.Should().Be(caseZeroWhenExceptionBlock);
            caseZeroWhenException.FalseSuccessorBlock.Should().Be(defaultBlock);

            caseZeroWhenExceptionBlock.SuccessorBlock.Should().Be(exitBlock);
            defaultBlock.SuccessorBlock.Should().Be(exitBlock);
        }

        [TestMethod]
        public void Cfg_Mixed_Cases_With_The_Same_Action()
        {
            var cfg = Build(@"
object o = null;
Exception ex = null;
switch (o)
{
    case 0 when ex is ArgumentException:
    case 1:
    case string s when s.Length > 0:
    case object x:
        // do stuff
        break;
}
");
            VerifyCfg(cfg, 9);

            var switchBlock = (BranchBlock)cfg.Blocks.ElementAt(0);
            var caseZero = (BinaryBranchBlock)cfg.Blocks.ElementAt(1);
            var caseZeroWhenException = (BinaryBranchBlock)cfg.Blocks.ElementAt(2);
            var caseOne = (BinaryBranchBlock)cfg.Blocks.ElementAt(3);
            var caseStringS = (BinaryBranchBlock)cfg.Blocks.ElementAt(4);
            var caseStringSWhen = (BinaryBranchBlock)cfg.Blocks.ElementAt(5);
            var caseObjectX = (BinaryBranchBlock)cfg.Blocks.ElementAt(6);
            var breakBlock = (JumpBlock)cfg.Blocks.ElementAt(7);
            var exitBlock = (ExitBlock)cfg.Blocks.ElementAt(8);

            switchBlock.SuccessorBlocks.Should().Equal(caseZero);

            caseZero.TrueSuccessorBlock.Should().Be(caseZeroWhenException);
            caseZero.FalseSuccessorBlock.Should().Be(caseOne);

            caseZeroWhenException.TrueSuccessorBlock.Should().Be(breakBlock);
            caseZeroWhenException.FalseSuccessorBlock.Should().Be(caseOne);

            caseOne.TrueSuccessorBlock.Should().Be(breakBlock);
            caseOne.FalseSuccessorBlock.Should().Be(caseStringS);

            caseStringS.TrueSuccessorBlock.Should().Be(caseStringSWhen);
            caseStringS.FalseSuccessorBlock.Should().Be(caseObjectX);

            caseStringSWhen.TrueSuccessorBlock.Should().Be(breakBlock);
            caseStringSWhen.FalseSuccessorBlock.Should().Be(caseObjectX);

            caseObjectX.TrueSuccessorBlock.Should().Be(breakBlock);
            caseObjectX.FalseSuccessorBlock.Should().Be(exitBlock);

            breakBlock.SuccessorBlock.Should().Be(exitBlock);
        }

        #endregion

        #region Goto

        [TestMethod]
        public void Cfg_Goto_A()
        {
            var cfg = Build("var x = 1; a: b: x++; if (x < 42) { cw1(); goto a; } cw2();");
            VerifyCfg(cfg, 7);

            var blocks = cfg.Blocks.ToList();

            var cw1 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw1")) as JumpBlock;
            var cw2 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw2"));

            var cond = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "x < 42"));

            var a = blocks[1] as JumpBlock;
            var b = blocks[2] as JumpBlock;

            var entry = cfg.EntryBlock;

            (a.JumpNode as LabeledStatementSyntax).Identifier.ValueText.Should().Be("a");
            (b.JumpNode as LabeledStatementSyntax).Identifier.ValueText.Should().Be("b");

            entry.SuccessorBlocks.Should().Equal(a);
            a.SuccessorBlocks.Should().Equal(b);
            b.SuccessorBlocks.Should().Equal(cond);
            cond.SuccessorBlocks.Should().BeEquivalentTo(new[] { cw1, cw2 });
            cw1.SuccessorBlocks.Should().Equal(a);
            cw2.SuccessorBlocks.Should().Equal(cfg.ExitBlock);
        }

        [TestMethod]
        public void Cfg_Goto_B()
        {
            var cfg = Build("var x = 1; a: b: x++; if (x < 42) { cw1(); goto b; } cw2();");
            VerifyCfg(cfg, 7);

            var blocks = cfg.Blocks.ToList();

            var cw1 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw1")) as JumpBlock;
            var cw2 = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "cw2"));

            var cond = blocks
                .First(block => block.Instructions.Any(n => n.ToString() == "x < 42"));

            var a = blocks[1] as JumpBlock;
            var b = blocks[2] as JumpBlock;

            var entry = cfg.EntryBlock;

            (a.JumpNode as LabeledStatementSyntax).Identifier.ValueText.Should().Be("a");
            (b.JumpNode as LabeledStatementSyntax).Identifier.ValueText.Should().Be("b");

            entry.SuccessorBlocks.Should().Equal(a);
            a.SuccessorBlocks.Should().Equal(b);
            b.SuccessorBlocks.Should().Equal(cond);
            cond.SuccessorBlocks.Should().BeEquivalentTo(new[] { cw1, cw2 });
            cw1.SuccessorBlocks.Should().Equal(b);
            cw2.SuccessorBlocks.Should().Equal(cfg.ExitBlock);
        }

        #endregion

        #region Yield return

        [TestMethod]
        public void Cfg_YieldReturn()
        {
            var cfg = Build(@"yield return 5;");
            VerifyMinimalCfg(cfg);

            cfg.EntryBlock.Should().BeOfType<JumpBlock>();

            var jumpBlock = (JumpBlock)cfg.EntryBlock;
            jumpBlock.JumpNode.Kind().Should().Be(SyntaxKind.YieldReturnStatement);
            VerifyAllInstructions(jumpBlock, "5");
        }

        #endregion

        #region Non-branching expressions

        [TestMethod]
        public void Cfg_NonBranchingExpressions()
        {
            var cfg = Build(@"
x = a < 2;  x = a <= 2;  x = a > 2;  x = a >= 2;       x = a == 2;  x = a != 2;  s = c << 4;  s = c >> 4;
b = x | 2;  b = x & 2;   b = x ^ 2;  c = ""c"" + 'c';  c = a - b;   c = a * b;   c = a / b;   c = a % b;");

            VerifyMinimalCfg(cfg);
            VerifyInstructions(cfg.EntryBlock, 0, "a", "2", "a < 2", "x = a < 2");
            VerifyInstructions(cfg.EntryBlock, 15 * 4, "a", "b", "a % b", "c = a % b");

            cfg = Build("b |= 2;  b &= false;  b ^= 2;  c += b;  c -= b;  c *= b;  c /= b;  c %= b; s <<= 4;  s >>= 4;");

            VerifyMinimalCfg(cfg);
            VerifyInstructions(cfg.EntryBlock, 0, "b", "2", "b |= 2");
            VerifyInstructions(cfg.EntryBlock, 9 * 3, "s", "4", "s >>= 4");

            cfg = Build("p = c++;  p = c--;  p = ++c;  p = --c;  p = +c;  p = -c;  p = !true;  p = ~1;  p = &c;  p = *c;");

            VerifyMinimalCfg(cfg);
            VerifyInstructions(cfg.EntryBlock, 0, "c", "c++", "p = c++");
            VerifyInstructions(cfg.EntryBlock, 9 * 3, "c", "*c", "p = *c");

            cfg = Build("o = null;");
            VerifyMinimalCfg(cfg);
            VerifyAllInstructions(cfg.EntryBlock, "null", "o = null");

            cfg = Build("b = (b);");
            VerifyMinimalCfg(cfg);
            VerifyAllInstructions(cfg.EntryBlock, "b", "b = (b)");

            cfg = Build(@"var t = typeof(int); var s = sizeof(int); var v = default(int);");
            VerifyMinimalCfg(cfg);
            VerifyAllInstructions(cfg.EntryBlock, "typeof(int)", "t = typeof(int)", "sizeof(int)", "s = sizeof(int)", "default(int)", "v = default(int)");

            cfg = Build(@"v = checked(1+1); v = unchecked(1+1);");
            VerifyMinimalCfg(cfg);
            VerifyInstructions(cfg.EntryBlock, 2, "1+1", "checked(1+1)", "v = checked(1+1)");
            VerifyInstructions(cfg.EntryBlock, 7, "1+1", "unchecked(1+1)", "v = unchecked(1+1)");

            cfg = Build("v = (int)1; v = 1 as object; v = 1 is int;");
            VerifyMinimalCfg(cfg);
            VerifyAllInstructions(cfg.EntryBlock,
                "1", "(int)1", "v = (int)1",
                "1", "1 as object", "v = 1 as object",
                "1", "1 is int", "v = 1 is int");

            cfg = Build(@"var s = $""Some {text}"";");
            VerifyMinimalCfg(cfg);
            VerifyAllInstructions(cfg.EntryBlock,
                @"text",
                @"$""Some {text}""",
                @"s = $""Some {text}""");

            cfg = Build("this.Method(call, with, arguments);");
            VerifyMinimalCfg(cfg);
            VerifyAllInstructions(cfg.EntryBlock,
                "this",
                "this.Method",
                "call", "with", "arguments",
                "this.Method(call, with, arguments)");

            cfg = Build("x = array[1,2,3]; x = array2[1][2];");
            VerifyMinimalCfg(cfg);
            VerifyAllInstructions(cfg.EntryBlock,
                "array",
                "1", "2", "3",
                "array[1,2,3]",
                "x = array[1,2,3]",
                "array2",
                "1",
                "array2[1]",
                "2",
                "array2[1][2]",
                "x = array2[1][2]");

            cfg = Build(@"var dict = new Dictionary<string,int>{ [""one""] = 1 };");
            VerifyMinimalCfg(cfg);
            VerifyAllInstructions(cfg.EntryBlock,
                @"new Dictionary<string,int>{ [""one""] = 1 }",
                @"""one""",
                @"[""one""]",
                @"1",
                @"[""one""] = 1",
                @"{ [""one""] = 1 }",
                @"dict = new Dictionary<string,int>{ [""one""] = 1 }");

            cfg = Build("var x = new { Prop1 = 10, Prop2 = 20 };");
            VerifyMinimalCfg(cfg);
            VerifyAllInstructions(cfg.EntryBlock, "10", "20", "new { Prop1 = 10, Prop2 = 20 }", "x = new { Prop1 = 10, Prop2 = 20 }");

            cfg = Build("var x = new { Prop1 };");
            VerifyMinimalCfg(cfg);
            VerifyAllInstructions(cfg.EntryBlock, "Prop1", "new { Prop1 }", "x = new { Prop1 }");

            cfg = Build("var x = new MyClass(5) { Prop1 = 10 };");
            VerifyMinimalCfg(cfg);
            VerifyInstructions(cfg.EntryBlock, 0,
                "5",
                "new MyClass(5) { Prop1 = 10 }",
                "10",
                "Prop1 = 10",
                "{ Prop1 = 10 }");

            cfg = Build("var x = new List<int>{ 10, 20 };");
            VerifyMinimalCfg(cfg);
            VerifyInstructions(cfg.EntryBlock, 0,
                "new List<int>{ 10, 20 }",
                "10",
                "20",
                "{ 10, 20 }");

            cfg = Build("var x = new[,] { { 10, 20 }, { 10, 20 } };");
            VerifyMinimalCfg(cfg);
            VerifyInstructions(cfg.EntryBlock, 0,
                "new[,] { { 10, 20 }, { 10, 20 } }",
                "10",
                "20",
                "{ 10, 20 }");
            VerifyInstructions(cfg.EntryBlock, 7, "{ { 10, 20 }, { 10, 20 } }");

            cfg = Build("var x = new int[] { 1 };");
            VerifyMinimalCfg(cfg);
            VerifyAllInstructions(cfg.EntryBlock,
                "", "int[]", "new int[] { 1 }", "1", "{ 1 }", "x = new int[] { 1 }");

            cfg = Build("var x = new int [1,2][3]{ 10, 20 };");
            VerifyMinimalCfg(cfg);
            VerifyInstructions(cfg.EntryBlock, 0,
                "1", "2", "3",
                "int [1,2][3]",
                "new int [1,2][3]{ 10, 20 }",
                "10",
                "20",
                "{ 10, 20 }");

            cfg = Build("var z = x->prop;");
            VerifyMinimalCfg(cfg);
            VerifyInstructions(cfg.EntryBlock, 0, "x", "x->prop");

            cfg = Build("var x = await this.Method(__arglist(10,11));");
            VerifyMinimalCfg(cfg);
            VerifyInstructions(cfg.EntryBlock, 2, "__arglist", "10", "11", "__arglist(10,11)",
                "this.Method(__arglist(10,11))", "await this.Method(__arglist(10,11))");

            cfg = Build("var x = 1; var y = __refvalue(__makeref(x), int); var t = __reftype(__makeref(x));");
            VerifyMinimalCfg(cfg);
            VerifyInstructions(cfg.EntryBlock, 2, "x", "__makeref(x)", "__refvalue(__makeref(x), int)");
            VerifyInstructions(cfg.EntryBlock, 6, "x", "__makeref(x)", "__reftype(__makeref(x))");

            cfg = Build("var x = new Action(()=>{}); var y = new Action(i=>{}); var z = new Action(delegate(){});");
            VerifyMinimalCfg(cfg);
            VerifyInstructions(cfg.EntryBlock, 0, "()=>{}", "new Action(()=>{})");

            cfg = Build("var x = from t in ts where t > 42;");
            VerifyMinimalCfg(cfg);
            VerifyInstructions(cfg.EntryBlock, 0, "from t in ts where t > 42", "x = from t in ts where t > 42");

            cfg = Build("string.Format(\"\")");
            VerifyMinimalCfg(cfg);
            VerifyAllInstructions(cfg.EntryBlock, "string", "string.Format", "\"\"", "string.Format(\"\")");
        }

        [TestMethod]
        public void Cfg_Stackalloc()
        {
            var cfg = Build("var x = stackalloc int[10];");
            VerifyMinimalCfg(cfg);
            VerifyInstructions(cfg.EntryBlock, 0, "10", "int[10]", "stackalloc int[10]");
        }

        [TestMethod]
        public void Cfg_Stackalloc_Initializer()
        {
            var cfg = Build("var x = stackalloc int[2] { 10, 20 };");
            VerifyMinimalCfg(cfg);
            VerifyInstructions(cfg.EntryBlock, 0, "2", "int[2]", "10", "20", "{ 10, 20 }", "stackalloc int[2] { 10, 20 }");
        }

        [TestMethod]
        public void Cfg_Stackalloc_Implicit()
        {
            var cfg = Build("var x = stackalloc [] {100, 200, 300};");
            VerifyMinimalCfg(cfg);
            VerifyInstructions(cfg.EntryBlock, 0, "100", "200", "300", "{100, 200, 300}", "stackalloc [] {100, 200, 300}");
        }

        [TestMethod]
        public void Cfg_NonRemovedCalls()
        {
            var cfg = Build(@"System.Diagnostics.Debug.Fail("""");");
            VerifyCfg(cfg, 2);
            cfg.EntryBlock.Instructions.Should().NotBeEmpty();

            cfg = Build(@"System.Diagnostics.Debug.Assert(false);");
            VerifyCfg(cfg, 2);
            cfg.EntryBlock.Instructions.Should().NotBeEmpty();
        }

        #endregion

        #region Method invocation

        [TestMethod]
        public void Cfg_Ref_Arg_Should_Be_Last_Instruction()
        {
            var cfg = Build(@"Bye(ref x0, x1, x2, x3, ref x4);}");
            VerifyCfg(cfg, 2);
            var instructionsBlock = (SimpleBlock)cfg.Blocks.ElementAt(0);
            VerifyAllInstructions(instructionsBlock, "Bye", "x1", "x2", "x3", "x0", "x4", "Bye(ref x0, x1, x2, x3, ref x4)");
        }

        [TestMethod]
        public void Cfg_Ref_Arg_Should_Be_Last_Instruction_WithMethodCallOnObject()
        {
            var cfg = Build(@"Bye.Hi(ref x0, x1, x2, x3, ref x4);}");
            VerifyCfg(cfg, 2);
            var instructionsBlock = (SimpleBlock)cfg.Blocks.ElementAt(0);
            VerifyAllInstructions(instructionsBlock, "Bye", "Bye.Hi", "x1", "x2", "x3", "x0", "x4", "Bye.Hi(ref x0, x1, x2, x3, ref x4)");
        }

        #endregion

        #region Property Pattern Clause

        [TestMethod]
        public void Cfg_PropertyPatternClause_Simple()
        {
            var cfg = Build(@"var x = address is Address { State: ""WA"" };");

            VerifyCfg(cfg, 2);

            var entryBlock = (SimpleBlock)cfg.EntryBlock;
            var exitBlock = cfg.ExitBlock;

            entryBlock.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(entryBlock,
                "address",
                "Address { State: \"WA\" }", // RecursivePattern
                "address is Address { State: \"WA\" }", // IsPatternExpression
                "x = address is Address { State: \"WA\" }"); // VariableDeclaration
        }

        [TestMethod]
        public void Cfg_PropertyPatternClause_MultipleProperties()
        {
            var cfg = Build(@"var x = address is { State: ""WA"", Street: ""Rue"" };");

            VerifyCfg(cfg, 2);

            var entryBlock = (SimpleBlock)cfg.EntryBlock;
            var exitBlock = cfg.ExitBlock;

            entryBlock.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(entryBlock,
                "address",
                "{ State: \"WA\", Street: \"Rue\" }", // Recursive Pattern
                "address is { State: \"WA\", Street: \"Rue\" }", // IsPatternExpression
                "x = address is { State: \"WA\", Street: \"Rue\" }"); // VariableDeclaration
        }

        [TestMethod]
        public void Cfg_PropertyPatternClause_WithSingleVariableDesignation()
        {
            var cfg = Build(@"var x = address is Address { State: ""WA"" } addr;");

            VerifyCfg(cfg, 2);

            var entryBlock = (SimpleBlock)cfg.EntryBlock;
            var exitBlock = cfg.ExitBlock;

            entryBlock.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(entryBlock,
                "address",
                "Address { State: \"WA\" } addr", // Recursive Pattern
                "address is Address { State: \"WA\" } addr", // IsPatternExpression
                "x = address is Address { State: \"WA\" } addr"); // VariableDeclaration
        }

        [TestMethod]
        public void Cfg_PropertyPatternClause_InsideIf()
        {
            var cfg = Build(@"if (address is Address { State: ""WA"" }) { return true; }");

            VerifyCfg(cfg, 3);

            var entryBlock = (BinaryBranchBlock)cfg.EntryBlock;
            var trueBlock = (JumpBlock)cfg.Blocks.ElementAt(1);
            var exitBlock = cfg.ExitBlock;

            entryBlock.TrueSuccessorBlock.Should().Be(trueBlock);
            entryBlock.FalseSuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(entryBlock,
                "address",
                "Address { State: \"WA\" }", // Recursive Pattern
                "address is Address { State: \"WA\" }"); // IsPatternExpression

            trueBlock.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(trueBlock, "true");

            VerifyNoInstruction(exitBlock);
        }

        [TestMethod]
        public void Cfg_PropertyPatternClause_InsideIf_WithSingleVariableDesignation()
        {
            var cfg = Build(@"if (address is Address { State: ""WA"" } addr) { return true; }");

            VerifyCfg(cfg, 3);

            var entryBlock = (BinaryBranchBlock)cfg.EntryBlock;
            var trueBlock = (JumpBlock)cfg.Blocks.ElementAt(1);
            var exitBlock = cfg.ExitBlock;

            entryBlock.TrueSuccessorBlock.Should().Be(trueBlock);
            entryBlock.FalseSuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(entryBlock,
                "address",
                "Address { State: \"WA\" } addr", // Recursive Pattern
                "address is Address { State: \"WA\" } addr"); // IsPatternExpression

            trueBlock.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(trueBlock, "true");

            VerifyNoInstruction(exitBlock);
        }

        [TestMethod]
        public void Cfg_PropertyPatternClause_InsideSwitch()
        {
            var cfg = Build(@"location switch { { State: ""WA"" } adr => salePrice * 0.06M, { State: ""MN"" } => salePrice * 0.75M, _ => 0M };");

            VerifyCfg(cfg, 6);

            var waArmBranch = (BinaryBranchBlock)cfg.EntryBlock;
            var waArmTrueBranch = (SimpleBlock)cfg.Blocks.ElementAt(1);
            var mnArmBranch = (BinaryBranchBlock)cfg.Blocks.ElementAt(2);
            var mnArmTrueBranch = (SimpleBlock)cfg.Blocks.ElementAt(3);
            var discardArm = (SimpleBlock)cfg.Blocks.ElementAt(4);
            var exitBlock = cfg.ExitBlock;

            waArmBranch.TrueSuccessorBlock.Should().Be(waArmTrueBranch);
            waArmBranch.FalseSuccessorBlock.Should().Be(mnArmBranch);
            VerifyAllInstructions(waArmBranch, "location", "{ State: \"WA\" } adr" /* RecursivePattern */);

            waArmTrueBranch.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(waArmTrueBranch, "salePrice", "0.06M", "salePrice * 0.06M");

            mnArmBranch.TrueSuccessorBlock.Should().Be(mnArmTrueBranch);
            mnArmBranch.FalseSuccessorBlock.Should().Be(discardArm);
            VerifyAllInstructions(mnArmBranch, "location", "{ State: \"MN\" }" /* RecursivePattern */);

            mnArmTrueBranch.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(mnArmTrueBranch, "salePrice", "0.75M", "salePrice * 0.75M");

            discardArm.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(discardArm, "0M");

            VerifyNoInstruction(exitBlock);
        }

        [TestMethod]
        public void Cfg_PropertyPatternClause_Nested()
        {
            var cfg = Build(@"var result = o is Person { Name: ""John Doe"", Address: { State: ""WA"" } };");

            VerifyCfg(cfg, 2);

            var entryBlock = (SimpleBlock)cfg.EntryBlock;
            var exitBlock = cfg.ExitBlock;

            entryBlock.SuccessorBlock.Should().Be(exitBlock);
            VerifyAllInstructions(entryBlock,
                "o",
                "Person { Name: \"John Doe\", Address: { State: \"WA\" } }", // RecursivePattern
                "o is Person { Name: \"John Doe\", Address: { State: \"WA\" } }", // IsPatternExpression
                "result = o is Person { Name: \"John Doe\", Address: { State: \"WA\" } }"); // VariableDeclaration
        }

        #endregion

        #region Instance creation

        [TestMethod]
        public void Cfg_New()
        {
            var cfg = Build(@"var x = new Object()");

            VerifyMinimalCfg(cfg);
            VerifyAllInstructions(cfg.EntryBlock, "new Object()", "x = new Object()");
        }

        [TestMethod]
        public void Cfg_New_TargetTyped()
        {
            Action a = () => Build(@"Object x = new()");
            a.Should().Throw<NotSupportedException>(); // C# 9 ImplicitObjectCreationExpressionSyntax is not supported yet
        }

        #endregion

        #region "Tuples"

        [TestMethod]
        public void Cfg_Tuple_Create()
        {
            var cfg = Build(@"var x = (true, 42);");

            VerifyMinimalCfg(cfg);
            VerifyAllInstructions(cfg.EntryBlock, "true", "42", "(true, 42)", "x = (true, 42)");
        }

        [TestMethod]
        public void Cfg_Tuple_ComplexExpression()
        {
            const string code = @"

var x = (LocalBool(), LocalInt() + 2);

bool LocalBool() => true;
int LocalInt() => 40;
";
            var cfg = Build(code);

            VerifyMinimalCfg(cfg);
            VerifyAllInstructions(cfg.EntryBlock, "LocalBool", "LocalBool()", "LocalInt", "LocalInt()", "2", "LocalInt() + 2", "(LocalBool(), LocalInt() + 2)", "x = (LocalBool(), LocalInt() + 2)");
        }

        [TestMethod]
        public void Cfg_Tuple_InDeclaration()
        {
            var cfg = Build(@"var (a, b) = (true, 42);");

            VerifyMinimalCfg(cfg);
            VerifyAllInstructions(cfg.EntryBlock, "var (a, b)", "true", "42", "(true, 42)", "var (a, b) = (true, 42)");
        }

        [TestMethod]
        public void Cfg_Tuple_AssignmentTarget()
        {
            var cfg = Build(@"bool a; int b; (a, b) = (true, 42);");

            VerifyMinimalCfg(cfg);
            VerifyAllInstructions(cfg.EntryBlock, "a", "b", "a", "b", "(a, b)", "true", "42", "(true, 42)", "(a, b) = (true, 42)");
        }

        #endregion

        #region Helpers to build the CFG for the tests

        internal const string TestInput = @"
using System;
namespace NS
{{
  public class Foo
  {{
    public void Bar()
    {{
      {0}
    }}
  }}
}}";

        internal static (MethodDeclarationSyntax Method, SemanticModel Model) CompileWithMethodBody(string input)
        {
            var (tree, semanticModel) = TestHelper.CompileIgnoreErrorsCS(input);
            return (tree.First<MethodDeclarationSyntax>(), semanticModel);
        }

        internal static string ExtremelyNestedExpression()
        {
            const int count = 2000;
            const string dna = "ACGT";
            return Enumerable.Repeat($@"""{dna}""", count).JoinStr(" +\n");
        }

        private static IControlFlowGraph Build(string methodBody)
        {
            var (method, model) = CompileWithMethodBody(string.Format(TestInput, methodBody));
            var cfg = CSharpControlFlowGraph.Create(method.Body, model);

            // when debugging the CFG, it is useful to visualize the CFG
            var dot = CfgSerializer.Serialize(cfg, "CFG diagnostics");
            System.Diagnostics.Debug.WriteLine(dot);

            return cfg;
        }

        private static SyntaxNode FirstConstructorBody(SyntaxTree tree) =>
            tree.First<ConstructorDeclarationSyntax>().Body;

        private static MethodDeclarationSyntax FirstMethod(SyntaxTree tree) =>
            tree.First<MethodDeclarationSyntax>();

        #endregion

        #region Verify helpers

        private static void VerifyForStatement(IControlFlowGraph cfg)
        {
            VerifyCfg(cfg, 5);
            var initBlock = cfg.EntryBlock;
            var blocks = cfg.Blocks.ToList();
            var branchBlock = blocks[1] as BinaryBranchBlock;
            var incrementorBlock = blocks[3];
            var loopBodyBlock = blocks[2];
            var exitBlock = cfg.ExitBlock;

            initBlock.SuccessorBlocks.Should().Equal(branchBlock);

            branchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { loopBodyBlock, exitBlock });
            branchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.ForStatement);

            loopBodyBlock.SuccessorBlocks.Should().Equal(incrementorBlock);
            incrementorBlock.SuccessorBlocks.Should().Equal(branchBlock);
            branchBlock.PredecessorBlocks.Should().BeEquivalentTo(new[] { initBlock, incrementorBlock });
            exitBlock.PredecessorBlocks.Should().Equal(branchBlock);
        }

        private static void VerifyInstructions(Block block, int fromIndex, params string[] instructions)
        {
            block.Instructions.Count.Should().BeGreaterOrEqualTo(fromIndex + instructions.Length);
            for (var i = 0; i < instructions.Length; i++)
            {
                block.Instructions[fromIndex + i].ToString().Should().Be(instructions[i]);
            }
        }

        private static void VerifyAllInstructions(Block block, params string[] instructions)
        {
            block.Instructions.Should().HaveSameCount(instructions);
            VerifyInstructions(block, 0, instructions);
        }

        private static void VerifyNoInstruction(Block block) =>
            VerifyAllInstructions(block, Array.Empty<string>());

        private static void VerifyCfg(IControlFlowGraph cfg, int numberOfBlocks)
        {
            VerifyBasicCfgProperties(cfg);

            cfg.Blocks.Should().HaveCount(numberOfBlocks);

            if (numberOfBlocks > 1)
            {
                cfg.EntryBlock.Should().NotBeSameAs(cfg.ExitBlock);
                cfg.Blocks.Should().ContainInOrder(new[] { cfg.EntryBlock, cfg.ExitBlock });
            }
            else
            {
                cfg.EntryBlock.Should().BeSameAs(cfg.ExitBlock);
                cfg.Blocks.Should().Contain(cfg.EntryBlock);
            }
        }

        private static void VerifyMinimalCfg(IControlFlowGraph cfg)
        {
            VerifyCfg(cfg, 2);

            cfg.EntryBlock.SuccessorBlocks.Should().Equal(cfg.ExitBlock);
        }

        private static void VerifyEmptyCfg(IControlFlowGraph cfg) =>
            VerifyCfg(cfg, 1);

        private static void VerifyBasicCfgProperties(IControlFlowGraph cfg)
        {
            cfg.Should().NotBeNull();
            cfg.EntryBlock.Should().NotBeNull();
            cfg.ExitBlock.Should().NotBeNull();

            cfg.ExitBlock.SuccessorBlocks.Should().BeEmpty();
            cfg.ExitBlock.Instructions.Should().BeEmpty();
        }

        private static void VerifyForStatementNoInitializer(IControlFlowGraph cfg)
        {
            VerifyCfg(cfg, 5);
            var blocks = cfg.Blocks.ToList();
            var initializerBlock = cfg.EntryBlock as ForInitializerBlock;
            var branchBlock = blocks[1] as BinaryBranchBlock;
            var incrementorBlock = blocks
                .First(b => b.Instructions.Any(n => n.ToString() == "i++"));
            var loopBodyBlock = blocks
                .First(b => b.Instructions.Any(n => n.ToString() == "x = 10"));
            var exitBlock = cfg.ExitBlock;

            initializerBlock.SuccessorBlock.Should().Be(branchBlock);

            branchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { loopBodyBlock, exitBlock });
            branchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.ForStatement);

            loopBodyBlock.SuccessorBlocks.Should().Equal(incrementorBlock);
            incrementorBlock.SuccessorBlocks.Should().Equal(branchBlock);
            branchBlock.PredecessorBlocks.Should().BeEquivalentTo(new[] { incrementorBlock, initializerBlock });
            exitBlock.PredecessorBlocks.Should().Equal(branchBlock);
        }

        private static void VerifyForStatementNoIncrementor(IControlFlowGraph cfg)
        {
            VerifyCfg(cfg, 4);
            var initBlock = cfg.EntryBlock;
            var blocks = cfg.Blocks.ToList();
            var branchBlock = blocks[1] as BinaryBranchBlock;
            var loopBodyBlock = blocks
                .First(b => b.Instructions.Any(n => n.ToString() == "x = 10"));
            var exitBlock = cfg.ExitBlock;

            initBlock.SuccessorBlocks.Should().Equal(branchBlock);

            branchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { loopBodyBlock, exitBlock });
            branchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.ForStatement);

            loopBodyBlock.SuccessorBlocks.Should().Equal(branchBlock);

            branchBlock.PredecessorBlocks.Should().BeEquivalentTo(new[] { initBlock, loopBodyBlock });

            exitBlock.PredecessorBlocks.Should().Equal(branchBlock);
        }

        private static void VerifyForStatementEmpty(IControlFlowGraph cfg)
        {
            VerifyCfg(cfg, 4);
            var initializerBlock = cfg.EntryBlock as ForInitializerBlock;
            var blocks = cfg.Blocks.ToList();
            var branchBlock = blocks[1] as BinaryBranchBlock;
            var loopBodyBlock = cfg.Blocks
                .First(b => b.Instructions.Any(n => n.ToString() == "x = 10"));
            var exitBlock = cfg.ExitBlock;

            initializerBlock.SuccessorBlock.Should().Be(branchBlock);

            branchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { loopBodyBlock, exitBlock });
            branchBlock.BranchingNode.Kind().Should().Be(SyntaxKind.ForStatement);

            loopBodyBlock.SuccessorBlocks.Should().Equal(branchBlock);
            branchBlock.PredecessorBlocks.Should().BeEquivalentTo(new[] { loopBodyBlock, initializerBlock });
            exitBlock.PredecessorBlocks.Should().Equal(branchBlock);
        }

        private static void VerifySimpleJumpBlock(IControlFlowGraph cfg, SyntaxKind kind)
        {
            VerifyCfg(cfg, 3);
            var jumpBlock = cfg.EntryBlock as JumpBlock;
            var bodyBlock = cfg.Blocks.ToList()[1];
            var exitBlock = cfg.ExitBlock;

            jumpBlock.SuccessorBlocks.Should().Equal(bodyBlock);
            bodyBlock.SuccessorBlocks.Should().Equal(exitBlock);

            jumpBlock.JumpNode.Kind().Should().Be(kind);
        }

        private static void VerifyJumpWithNoExpression(IControlFlowGraph cfg, SyntaxKind kind)
        {
            VerifyCfg(cfg, 4);
            var branchBlock = cfg.EntryBlock as BinaryBranchBlock;
            var blocks = cfg.Blocks.ToList();
            var trueBlock = blocks[1] as JumpBlock;
            var falseBlock = blocks[2];
            var exitBlock = cfg.ExitBlock;

            branchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { trueBlock, falseBlock });
            trueBlock.SuccessorBlocks.Should().Equal(exitBlock);
            trueBlock.JumpNode.Kind().Should().Be(kind);
            falseBlock.SuccessorBlocks.Should().Equal(exitBlock);
            exitBlock.PredecessorBlocks.Should().BeEquivalentTo(new[] { trueBlock, falseBlock });
        }

        private static void VerifyJumpWithExpression(IControlFlowGraph cfg, SyntaxKind kind)
        {
            VerifyCfg(cfg, 4);
            var branchBlock = cfg.EntryBlock as BinaryBranchBlock;
            var blocks = cfg.Blocks.ToList();
            var trueBlock = blocks[1] as JumpBlock;
            var falseBlock = blocks[2];
            var exitBlock = cfg.ExitBlock;

            branchBlock.SuccessorBlocks.Should().BeEquivalentTo(new[] { trueBlock, falseBlock });
            trueBlock.SuccessorBlocks.Should().Equal(exitBlock);
            trueBlock.JumpNode.Kind().Should().Be(kind);

            trueBlock.Instructions.Should().Contain(n => n.IsKind(SyntaxKind.IdentifierName) && n.ToString() == "ii");

            exitBlock.PredecessorBlocks.Should().BeEquivalentTo(new[] { trueBlock, falseBlock });
        }

        #endregion
    }
}
