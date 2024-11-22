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

using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.Test.TestFramework.Tests.Common;

[TestClass]
public class TestHelperTest
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
        ((Func<ControlFlowGraph>)(() => TestHelper.CompileCfg(CompileCfgLambda, AnalyzerLanguage.CSharp, false, "LocalFunctionName", "AnonymousFunctionFragment")))
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Specify localFunctionName or anonymousFunctionFragment.");

    [TestMethod]
    public void CompileCfg_InvalidAnonymousFunctionFragment_Throws() =>
        ((Func<ControlFlowGraph>)(() => TestHelper.CompileCfg(CompileCfgLambda, AnalyzerLanguage.CSharp, false, null, "InvalidFragment")))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("Anonymous function with 'InvalidFragment' fragment was not found.");

    [TestMethod]
    public void Serialize_Default_Throws() =>
        ((Func<string>)(() => TestHelper.Serialize(default))).Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("operation");

    [TestMethod]
    public void TestPath_ThisTestHasArbitraryLongNameSoWeTestTheBranchThatDependsOnVeryLongTestNames_ThisIsAFunctionalName_DoNotGetInspiredExlamationMark_ThisIsNotCleanAtAllExclamationMark()
    {
        TestHelper.TestPath(TestContext, "This is also pretty long file name that will make sure we exceed the maximum reasonable file length 1.txt").Should().Contain("TooLongTestName.0");
        TestHelper.TestPath(TestContext, "This is also pretty long file name that will make sure we exceed the maximum reasonable file length 2.txt").Should().Contain("TooLongTestName.1");
    }

    [DataTestMethod]
    [DataRow(@"C:\", @"C:\", @"\")]
    [DataRow(@"C:\a", @"C:\a\", @"\")]
    [DataRow(@"C:\A", @"C:\a\", @"\")]
    [DataRow(@"C:\a\", @"C:\a", @"")]
    [DataRow(@"C:\", @"C:\b", @"b")]
    [DataRow(@"C:\a", @"C:\b", @"..\b")]
    [DataRow(@"C:\a", @"C:\b\", @"..\b\")]
    [DataRow(@"C:\a\b", @"C:\a", @"..")]
    [DataRow(@"C:\a\b", @"C:\a\", @"..")]
    [DataRow(@"C:\a\b\", @"C:\a", @"..")]
    [DataRow(@"C:\a\b\", @"C:\a\", @"..")]
    [DataRow(@"C:\a\b\c", @"C:\a\b", @"..")]
    [DataRow(@"C:\a\b\c", @"C:\a\b\", @"..")]
    [DataRow(@"C:\a\b\c", @"C:\a", @"..\..")]
    [DataRow(@"C:\a\b\c", @"C:\a\", @"..\..")]
    [DataRow(@"C:\a\b\c\", @"C:\a\b", @"..")]
    [DataRow(@"C:\a\b\c\", @"C:\a\b\", @"..")]
    [DataRow(@"C:\a\b\c\", @"C:\a", @"..\..")]
    [DataRow(@"C:\a\b\c\", @"C:\a\", @"..\..")]
    [DataRow(@"C:\a\", @"C:\b", @"..\b")]
    [DataRow(@"C:\a", @"C:\a\b", @"b")]
    [DataRow(@"C:\a", @"C:\A\b", @"b")]
    [DataRow(@"C:\a", @"C:\b\c", @"..\b\c")]
    [DataRow(@"C:\a\", @"C:\a\b", @"b")]
    [DataRow(@"C:\", @"D:\", @"D:\")]
    [DataRow(@"C:\", @"D:\b", @"D:\b")]
    [DataRow(@"C:\", @"D:\b\", @"D:\b\")]
    [DataRow(@"C:\a", @"D:\b", @"D:\b")]
    [DataRow(@"C:\a\", @"D:\b", @"D:\b")]
    [DataRow(@"C:\ab", @"C:\a", @"..\a")]
    [DataRow(@"C:\a", @"C:\ab", @"..\ab")]
    [DataRow(@"C:\", @"\\LOCALHOST\Share\b", @"\\LOCALHOST\Share\b")]
    [DataRow(@"\\LOCALHOST\Share\a", @"\\LOCALHOST\Share\b", @"..\b")]
    public void GetRelativePath_ValidPaths_ReturnsRelativePath(string relativeTo, string path, string expected) =>
        TestHelper.GetRelativePath(relativeTo, path).Should().Be(expected);
}
