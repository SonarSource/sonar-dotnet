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

using SonarAnalyzer.Core.Syntax.Extensions;
using SonarAnalyzer.Core.Syntax.Utilities;

namespace SonarAnalyzer.CSharp.Core.Test.Syntax.Extensions;

[TestClass]
public class SyntaxTreeExtensionsTest
{
    [TestMethod]
    public void IsGenerated_On_GeneratedTree()
    {
        const string source =
@"namespace Generated
{
    class MyClass
    {
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        void M()
        {
            ;;;;
        }
    }
}";

        var result = IsGenerated(source, CSharpGeneratedCodeRecognizer.Instance);
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsGenerated_On_GeneratedLocalFunctionTree()
    {
        const string source =
@"namespace Generated
{
    class MyClass
    {
        void M()
        {
            ;;;;
            [System.Diagnostics.DebuggerNonUserCodeAttribute()]
            void LocalFunction()
            {
                ;;;
            }
        }
    }
}";
        var result = IsGenerated(source, CSharpGeneratedCodeRecognizer.Instance);
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsGenerated_On_NonGeneratedTree()
    {
        const string source =
@"namespace NonGenerated
{
    class MyClass
    {
    }
}";

        var result = IsGenerated(source, CSharpGeneratedCodeRecognizer.Instance);
        result.Should().BeFalse();
    }

    private static bool IsGenerated(string content, GeneratedCodeRecognizer generatedCodeRecognizer)
    {
        var compilation = SolutionBuilder
           .Create()
           .AddProject(AnalyzerLanguage.CSharp)
           .AddSnippet(content)
           .GetCompilation();
        return compilation.SyntaxTrees.First().IsGenerated(generatedCodeRecognizer);
    }
}
