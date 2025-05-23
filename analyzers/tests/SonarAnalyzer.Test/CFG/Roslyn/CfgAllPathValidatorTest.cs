﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.CFG.Roslyn.Test;

[TestClass]
public class CfgAllPathValidatorTest
{
    [TestMethod]
    public void ValidateCfgPaths()
    {
        const string code = @"
public class Sample
{
    public int Method2(bool condition)
    {
        if (condition)
            return 42;
        else
            return 1;
    }
}";
        var cfg = TestCompiler.CompileCfgCS(code);
        /*
         *           Entry 0
         *             |
         *             |
         *           Block 1
         *           /   \
         *     Else /     \ WhenFalse
         *         /       \
         *     Block 2    Block 3
         *        \        /
         *         \      /
         *          \    /
         *           Exit 4
         */
        var validator = new TestCfgValidator(cfg, 0, 1, 2);
        validator.CheckAllPaths().Should().BeTrue();

        // only entry block is valid
        validator = new TestCfgValidator(cfg, 0);
        validator.CheckAllPaths().Should().BeTrue();

        // only exit block is valid
        validator = new TestCfgValidator(cfg, 4);
        validator.CheckAllPaths().Should().BeFalse();

        var nonEntryBlockValid = new TestNonEntryBlockValidator(cfg);
        nonEntryBlockValid.CheckAllPaths().Should().BeTrue();
    }

    // This test fails on the Sonar version of CfgAllPathValidator
    [TestMethod]
    public void ValidAfterBranching()
    {
        const string code = @"
public class Sample
{
    internal string Prop;
    public void Method(string input)
    {
        var x = true && true;
        Prop = input;
    }
}
";
        var cfg = TestCompiler.CompileCfgCS(code);
        /*
         *           Entry 0
         *             |
         *             |
         *           Block 1
         *           /   \
         *     Else /     \ WhenFalse
         *         /       \
         *     Block 2    Block 3
         *        \        /
         *         \      /
         *          \    /
         *          Block 4
         *             |
         *             |
         *          Block 5
         *             |
         *             |
         *          Exit 6
         */
        var validator = new OnlyOneBlockIsValid(cfg, 5);
        validator.CheckAllPaths().Should().BeTrue();
    }

    [TestMethod]
    public void LoopInCfg()
    {
        const string code = @"
public class Sample
{
    public void Method(string input)
    {
        var a = input;
    A:
        if (input != """")
            goto C;
        else
            goto B;
    C:
        input = System.String.Empty;
        goto A;
    B:
        input = input;
    }
}
";
        var cfg = TestCompiler.CompileCfgCS(code);
        /*
         *           Entry 0
         *             |
         *           Block 1
         *             |
         *           Block 2 <----> Block 3
         *             |
         *          Block 4
         *             |
         *           Exit 5
         */
        var validator = new OnlyOneBlockIsValid(cfg, 4);
        validator.CheckAllPaths().Should().BeTrue();
    }

    private class TestNonEntryBlockValidator : CfgAllPathValidator
    {
        public TestNonEntryBlockValidator(ControlFlowGraph cfg) : base(cfg) { }

        protected override bool IsValid(BasicBlock block) => block.Ordinal > 0;

        protected override bool IsInvalid(BasicBlock block) => false;
    }

    private class TestCfgValidator : CfgAllPathValidator
    {
        private readonly int[] validBlocks;

        public TestCfgValidator(ControlFlowGraph cfg, params int[] validBlocks) : base(cfg) =>
            this.validBlocks = validBlocks;

        protected override bool IsValid(BasicBlock block) =>
            validBlocks.Contains(block.Ordinal);

        protected override bool IsInvalid(BasicBlock block) =>
            !validBlocks.Contains(block.Ordinal);
    }

    private class OnlyOneBlockIsValid : CfgAllPathValidator
    {
        private readonly int validBlock;

        public OnlyOneBlockIsValid(ControlFlowGraph cfg, int validBlock) : base(cfg) =>
            this.validBlock = validBlock;

        protected override bool IsValid(BasicBlock block) =>
            validBlock == block.Ordinal;

        protected override bool IsInvalid(BasicBlock block) => false;
    }
}
