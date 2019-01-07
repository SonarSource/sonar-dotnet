/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

extern alias csharp;
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class MethodsShouldUseBaseTypesTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void MethodsShouldUseBaseTypes_Internals()
        {
            var solution = SolutionBuilder.Create()
                .AddProject(AnalyzerLanguage.CSharp)
                .AddSnippet(@"
internal interface IFoo
{
    bool IsFoo { get; }
}

public class Foo : IFoo
{
    public bool IsFoo { get; set; }
}
")              .GetSolution()
                .AddProject(AnalyzerLanguage.CSharp)
                .AddProjectReference(sln => sln.ProjectIds[0])
                .AddSnippet(@"
internal class Bar
{
    public void MethodOne(Foo foo)
    {
        var x = foo.IsFoo;
    }
}
")              .GetSolution();

            foreach (var compilation in solution.Compile())
            {
                DiagnosticVerifier.Verify(compilation, new MethodsShouldUseBaseTypes(), CompilationErrorBehavior.FailTest);
            }
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void MethodsShouldUseBaseTypes()
        {
            Verifier.VerifyAnalyzer(@"TestCases\MethodsShouldUseBaseTypes.cs",
                new MethodsShouldUseBaseTypes());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void MethodsShouldUseBaseTypes_InvalidCode()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Foo
{
    private void FooBar(IList<int> , IList<string>)
    {
        a.ToList();
    }

    // New test case - code doesn't compile but was making analyzer crash
    private void Foo(IList<int> a, IList<string> a)
    {
        a.ToList();
    }
}", new MethodsShouldUseBaseTypes(), checkMode: CompilationErrorBehavior.Ignore);
        }
    }
}
