/*
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

using SonarAnalyzer.CSharp.Core.Syntax.Utilities;
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.Test.Extensions;

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
