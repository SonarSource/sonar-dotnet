/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.Test.TestFramework.Tests.Common;

[TestClass]
public class TestCompilerTest
{
    private const string CompileCfgLambda = """
        public class Sample
        {
            public void Method()
            {
                System.Func<int> f = () => 42;
            }
        }
        """;

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void CompileCfg_LocalfunctionNameAndAnonymousFunctionFragment_Throws() =>
        ((Func<ControlFlowGraph>)(() => TestCompiler.CompileCfg(CompileCfgLambda, AnalyzerLanguage.CSharp, false, "LocalFunctionName", "AnonymousFunctionFragment")))
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Specify localFunctionName or anonymousFunctionFragment.");

    [TestMethod]
    public void CompileCfg_InvalidAnonymousFunctionFragment_Throws() =>
        ((Func<ControlFlowGraph>)(() => TestCompiler.CompileCfg(CompileCfgLambda, AnalyzerLanguage.CSharp, false, null, "InvalidFragment")))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("Anonymous function with 'InvalidFragment' fragment was not found.");

    [TestMethod]
    public void Serialize_Default_Throws() =>
        ((Func<string>)(() => TestCompiler.Serialize(default))).Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("operation");
}
