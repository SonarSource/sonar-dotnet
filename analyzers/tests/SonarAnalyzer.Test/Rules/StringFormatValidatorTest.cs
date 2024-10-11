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
    public class StringFormatValidatorTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<StringFormatValidator>();

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void StringFormatValidator_RuntimeExceptionFree(ProjectType projectType) =>
            builder.AddPaths("StringFormatValidator.RuntimeExceptionFree.cs")
                .AddReferences(TestHelper.ProjectTypeReference(projectType))
                .Verify();

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void StringFormatValidator_TypoFree(ProjectType projectType) =>
            builder.AddPaths("StringFormatValidator.TypoFree.cs")
                .AddReferences(TestHelper.ProjectTypeReference(projectType))
                .Verify();

        [TestMethod]
        public void StringFormatValidator_EdgeCases() =>
            builder.AddPaths("StringFormatValidator.EdgeCases.cs").Verify();

#if NET

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void StringFormatValidator_RuntimeExceptionFree_CSharp11(ProjectType projectType) =>
            builder.AddPaths("StringFormatValidator.RuntimeExceptionFree.CSharp11.cs")
                .AddReferences(TestHelper.ProjectTypeReference(projectType))
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void StringFormatValidator_TypoFree_CSharp11(ProjectType projectType) =>
            builder.AddPaths("StringFormatValidator.TypoFree.CSharp11.cs")
                .AddReferences(TestHelper.ProjectTypeReference(projectType))
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

        [TestMethod]
        public void StringFormatValidator_Latest() =>
            builder.AddPaths("StringFormatValidator.Latest.cs")
                .AddReferences(TestHelper.ProjectTypeReference(ProjectType.Product))
                .WithOptions(ParseOptionsHelper.CSharpLatest)
                .Verify();

#endif

    }
}
