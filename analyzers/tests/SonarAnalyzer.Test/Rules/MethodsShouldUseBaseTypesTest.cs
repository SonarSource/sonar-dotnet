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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class MethodsShouldUseBaseTypesTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<MethodsShouldUseBaseTypes>();

        [TestMethod]
        public void MethodsShouldUseBaseTypes_Internals()
        {
            const string code1 = @"
internal interface IFoo
{
    bool IsFoo { get; }
}

public class Foo : IFoo
{
    public bool IsFoo { get; set; }
}";
            const string code2 = @"
internal class Bar
{
    public void MethodOne(Foo foo)
    {
        var x = foo.IsFoo;
    }
}";
            var solution = SolutionBuilder.Create()
                .AddProject(AnalyzerLanguage.CSharp)
                .AddSnippet(code1)
                .Solution
                .AddProject(AnalyzerLanguage.CSharp)
                .AddProjectReference(sln => sln.ProjectIds[0])
                .AddSnippet(code2)
                .Solution;
            foreach (var compilation in solution.Compile())
            {
                DiagnosticVerifier.Verify(compilation, new MethodsShouldUseBaseTypes());
            }
        }

        [TestMethod]
        public void MethodsShouldUseBaseTypes() =>
            // There are two files provided (identical) in order to be able to test the rule behavior in concurrent environment.
            // The rule is executed concurrently if there are at least 2 syntax trees.
            builder.AddPaths("MethodsShouldUseBaseTypes.cs", "MethodsShouldUseBaseTypes.Concurrent.cs").WithAutogenerateConcurrentFiles(false).Verify();

        [TestMethod]
        public void MethodsShouldUseBaseTypes_CSharp8() =>
            builder.AddPaths("MethodsShouldUseBaseTypes.CSharp8.cs").WithAutogenerateConcurrentFiles(false).WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

        [TestMethod]
        public void MethodsShouldUseBaseTypes_Controllers() =>
            builder.AddPaths("MethodsShouldUseBaseTypes.AspControllers.cs")
                .AddReferences(NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(Constants.NuGetLatestVersion))
                .Verify();
#if NET
        [TestMethod]
        public void MethodsShouldUseBaseTypes_CSharp9() =>
            builder.AddPaths("MethodsShouldUseBaseTypes.CSharp9.cs").WithTopLevelStatements().Verify();
#endif

        [TestMethod]
        public void MethodsShouldUseBaseTypes_InvalidCode() =>
            builder.AddSnippet("""
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
                }
                """).VerifyNoIssuesIgnoreErrors();
    }
}
