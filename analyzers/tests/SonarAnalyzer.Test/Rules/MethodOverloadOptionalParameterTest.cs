/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class MethodOverloadOptionalParameterTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<MethodOverloadOptionalParameter>();

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void MethodOverloadOptionalParameter() =>
        builder.AddPaths("MethodOverloadOptionalParameter.cs").AddReferences(MetadataReferenceFacade.NetStandard21).WithOptions(LanguageOptions.FromCSharp8).Verify();

    [TestMethod]
    public void MethodOverloadOptionalParameter_CS_Latest() =>
        builder.AddPaths("MethodOverloadOptionalParameter.Latest.cs", "MethodOverloadOptionalParameter.Latest.Partial.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();

    [TestMethod]
    public void MethodOverloadOptionalParameter_Razor() =>
        builder.AddSnippet(
            """
            @code
            {
                void Print2(string[] messages) { }
                void Print2(string[] messages, string delimiter = "\n") { } // Noncompliant {{This method signature overlaps the one defined on line 3, the default parameter value can't be used.}};
                //                             ^^^^^^^^^^^^^^^^^^^^^^^
            }
            """,
            "SomeRazorFile.razor")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .Verify();
}
