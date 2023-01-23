/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class StringFormatValidatorTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<StringFormatValidator>();

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void StringFormatRuntimeExceptionFreeValidator(ProjectType projectType) =>
            builder.AddPaths("StringFormatRuntimeExceptionFreeValidator.cs")
                .AddReferences(TestHelper.ProjectTypeReference(projectType))
                .Verify();

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void StringFormatTypoFreeValidator(ProjectType projectType) =>
            builder.AddPaths("StringFormatTypoFreeValidator.cs")
                .AddReferences(TestHelper.ProjectTypeReference(projectType))
                .Verify();

        [TestMethod]
        public void StringFormatEdgeCasesValidator() =>
            builder.AddPaths("StringFormatEdgeCasesValidator.cs").Verify();

#if NET

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void StringFormatRuntimeExceptionFreeValidator_CSharp11(ProjectType projectType) =>
            builder.AddPaths("StringFormatRuntimeExceptionFreeValidator.CSharp11.cs")
                .AddReferences(TestHelper.ProjectTypeReference(projectType))
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void StringFormatTypoFreeValidator_CSharp11(ProjectType projectType) =>
            builder.AddPaths("StringFormatTypoFreeValidator.CSharp11.cs")
                .AddReferences(TestHelper.ProjectTypeReference(projectType))
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

#endif

    }
}
