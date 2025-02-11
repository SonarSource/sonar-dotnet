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

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class MethodOverloadOptionalParameterTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<MethodOverloadOptionalParameter>();

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void MethodOverloadOptionalParameter() =>
            builder.AddPaths("MethodOverloadOptionalParameter.cs").AddReferences(MetadataReferenceFacade.NetStandard21).WithOptions(LanguageOptions.FromCSharp8).Verify();

#if NET

        [TestMethod]
        public void MethodOverloadOptionalParameter_CSharp9() =>
            builder.AddPaths("MethodOverloadOptionalParameter.CSharp9.cs").WithOptions(LanguageOptions.FromCSharp9).Verify();

        [TestMethod]
        public void MethodOverloadOptionalParameter_CSharp10() =>
            builder.AddPaths("MethodOverloadOptionalParameter.CSharp10.cs").WithOptions(LanguageOptions.FromCSharp10).Verify();

        [TestMethod]
        public void MethodOverloadOptionalParameter_CSharp11() =>
            builder.AddPaths("MethodOverloadOptionalParameter.CSharp11.cs").WithOptions(LanguageOptions.FromCSharp11).Verify();

        [TestMethod]
        public void MethodOverloadOptionalParameter_CSharp12() =>
            builder.AddPaths("MethodOverloadOptionalParameter.CSharp12.cs").WithOptions(LanguageOptions.FromCSharp12).Verify();

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

#endif

    }
}
