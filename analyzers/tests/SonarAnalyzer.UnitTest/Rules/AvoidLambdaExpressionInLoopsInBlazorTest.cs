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

#if NET
using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules;

[TestClass]
public class AvoidLambdaExpressionInLoopsInBlazorTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<AvoidLambdaExpressionInLoopsInBlazor>();

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void AvoidLambdaExpressionInLoopsInBlazor_Blazor() =>
        builder.AddPaths("AvoidLambdaExpressionInLoopsInBlazor.razor")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
            .WithConcurrentAnalysis(false)
            .Verify();

    [TestMethod]
    public void AvoidLambdaExpressionInLoopsInBlazor_DoesNotReportInCs() =>
        builder.AddPaths("AvoidLambdaExpressionInLoopsInBlazor.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp10)
            .VerifyNoIssueReported();
}
#endif
