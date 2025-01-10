/*
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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class DoNotWriteToStandardOutputTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<DoNotWriteToStandardOutput>();

        [TestMethod]
        public void DoNotWriteToStandardOutput() =>
            builder.AddPaths("DoNotWriteToStandardOutput.cs").Verify();

        [TestMethod]
        public void DoNotWriteToStandardOutput_ConditionalDirectives1() =>
            builder.AddPaths("DoNotWriteToStandardOutput_Conditionals1.cs")
                .WithConcurrentAnalysis(false)
                .Verify();

        [TestMethod]
        public void DoNotWriteToStandardOutput_ConditionalDirectives2() =>
            builder.AddPaths("DoNotWriteToStandardOutput_Conditionals2.cs")
                .WithConcurrentAnalysis(false)
                .Verify();
    }
}
