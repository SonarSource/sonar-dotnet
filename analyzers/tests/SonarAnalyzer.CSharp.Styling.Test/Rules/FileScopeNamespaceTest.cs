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

using SonarAnalyzer.TestFramework.Common;

namespace SonarAnalyzer.CSharp.Styling.Test.Rules;

[TestClass]
public class FileScopeNamespaceTest
{
    private readonly VerifierBuilder builder = StylingVerifierBuilder.Create<FileScopeNamespace>().WithConcurrentAnalysis(false);

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void FileScopeNamespace() =>
        builder.AddPaths("FileScopeNamespace.cs").Verify();

    [TestMethod]
    public void FileScopeNamespace_TestCode() =>
        builder.AddPaths("FileScopeNamespace.cs")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, Helpers.ProjectType.Test))
            .Verify();

    [TestMethod]
    public void FileScopeNamespace_Compliant() =>
        builder.AddPaths("FileScopeNamespace.Compliant.cs").VerifyNoIssueReported();
}
