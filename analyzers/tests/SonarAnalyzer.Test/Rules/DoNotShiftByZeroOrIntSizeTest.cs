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

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class DoNotShiftByZeroOrIntSizeTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<DoNotShiftByZeroOrIntSize>();

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void DoNotShiftByZeroOrIntSize() =>
        builder.AddPaths("DoNotShiftByZeroOrIntSize.cs").Verify();

#if NET

    [TestMethod]
    public void DoNotShiftByZeroOrIntSize_CSharp9() =>
        builder.AddPaths("DoNotShiftByZeroOrIntSize.CSharp9.cs")
            .WithTopLevelStatements()
            .VerifyNoIssues();  // FN, native ints are not supported

    [TestMethod]
    public void DoNotShiftByZeroOrIntSize_CSharp10() =>
        builder.AddPaths("DoNotShiftByZeroOrIntSize.CSharp10.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp10)
            .Verify();

    [TestMethod]
    public void DoNotShiftByZeroOrIntSize_CSharp11() =>
        builder.AddPaths("DoNotShiftByZeroOrIntSize.CSharp11.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp11)
            .Verify();

    [TestMethod]
    public void DoNotShiftByZeroOrIntSize_RazorFile_CorrectMessage() =>
        builder.AddSnippet(
            """
            @code
            {
                public void Method()
                {
                    byte b = 1;
                    b = (byte)(b << 10);
                    b = (byte)(b << 10);
                    b = 1 << 0;

                    sbyte sb = 1;
                    sb = (sbyte)(sb << 10);

                    int i = 1 << 10;
                    i = i << 32;  // Noncompliant
                }
            }
            """,
            "SomeRazorFile.razor")
        .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
        .Verify();

#endif

}
