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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class MethodOverloadOptionalParameterTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<MethodOverloadOptionalParameter>();

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void MethodOverloadOptionalParameter() =>
            builder.AddPaths("MethodOverloadOptionalParameter.cs").AddReferences(MetadataReferenceFacade.NetStandard21).WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

#if NET

        [TestMethod]
        public void MethodOverloadOptionalParameter_CSharp9() =>
            builder.AddPaths("MethodOverloadOptionalParameter.CSharp9.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void MethodOverloadOptionalParameter_CSharp10() =>
            builder.AddPaths("MethodOverloadOptionalParameter.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [TestMethod]
        public void MethodOverloadOptionalParameter_CSharp11() =>
            builder.AddPaths("MethodOverloadOptionalParameter.CSharp11.cs").WithOptions(ParseOptionsHelper.FromCSharp11).Verify();

        [TestMethod]
        public void MethodOverloadOptionalParameter_CSharp12() =>
            builder.AddPaths("MethodOverloadOptionalParameter.CSharp12.cs").WithOptions(ParseOptionsHelper.FromCSharp12).Verify();

        [TestMethod]
        public void MethodOverloadOptionalParameter_Razor() =>
            builder
                .AddSnippet(
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
