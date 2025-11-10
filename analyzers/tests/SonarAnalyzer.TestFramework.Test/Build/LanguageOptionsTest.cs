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

using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.TestFramework.Test.Build;

[TestClass]
public class LanguageOptionsTest
{
    [TestMethod]
    public void ExpectedLanguageVersion()
    {
        var vbOptions = LanguageOptions.FromVisualBasic12.Cast<VB.VisualBasicParseOptions>().Select(x => x.LanguageVersion);
        var csOptions = LanguageOptions.FromCSharp6.Cast<CS.CSharpParseOptions>().Select(x => x.LanguageVersion);
        if (!TestEnvironment.IsAzureDevOpsContext || TestEnvironment.IsPullRequestBuild)
        {
            csOptions.Should().BeEquivalentTo([CS.LanguageVersion.CSharp6]);
            vbOptions.Should().BeEquivalentTo([VB.LanguageVersion.VisualBasic12]);
        }
        else
        {
            // This should fail when we add new language version
            csOptions.Should().BeEquivalentTo(
            [
                CS.LanguageVersion.CSharp6,
                CS.LanguageVersion.CSharp7,
                CS.LanguageVersion.CSharp7_1,
                CS.LanguageVersion.CSharp7_2,
                CS.LanguageVersion.CSharp7_3,
                CS.LanguageVersion.CSharp8,
                CS.LanguageVersion.CSharp9,
                CS.LanguageVersion.CSharp10,
                CS.LanguageVersion.CSharp11,
                CS.LanguageVersion.CSharp12,
                CS.LanguageVersion.CSharp13
            ]);

            vbOptions.Should().BeEquivalentTo(
            [
                VB.LanguageVersion.VisualBasic12,
                VB.LanguageVersion.VisualBasic14,
                VB.LanguageVersion.VisualBasic15,
                VB.LanguageVersion.VisualBasic15_3,
                VB.LanguageVersion.VisualBasic15_5,
                VB.LanguageVersion.VisualBasic16
            ]);
        }
    }
}
