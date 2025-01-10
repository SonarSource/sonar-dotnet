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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.Test.TestFramework.Tests.Analyzers;

[TestClass]
public class TestGeneratedCodeRecognizerTest
{
    [TestMethod]
    public void IsGenerated_FromAttribute_CS()
    {
        var tree = CSharpSyntaxTree.ParseText("[GeneratedCode] public class Sample { }", null, "File.cs");
        TestGeneratedCodeRecognizer.Instance.IsGenerated(tree).Should().BeTrue();
    }

    [TestMethod]
    public void IsGenerated_FromAttribute_VB()
    {
        var tree = VisualBasicSyntaxTree.ParseText("<GeneratedCode>Public Class Sample : End Class", null, "File.vb");
        TestGeneratedCodeRecognizer.Instance.IsGenerated(tree).Should().BeTrue();
    }
}
