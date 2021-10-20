/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.Helpers;

using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    [TestClass]
    public class ParseOptionsHelperTest
    {
        [TestMethod]
        public void ExpectedLanguageVersion()
        {
            var vbVersions = ParseOptionsHelper.FromVisualBasic12.Cast<VB.VisualBasicParseOptions>().Select(x => x.LanguageVersion);
            var csVersions = ParseOptionsHelper.FromCSharp6.Cast<CS.CSharpParseOptions>().Select(x => x.LanguageVersion);
            if (!TestContextHelper.IsAzureDevOpsContext || TestContextHelper.IsPullRequestBuild)
            {
                csVersions.Should().BeEquivalentTo(CS.LanguageVersion.CSharp6);
                vbVersions.Should().BeEquivalentTo(VB.LanguageVersion.VisualBasic12);
            }
            else
            {
                // This should fail when we add new language version
                csVersions.Should().BeEquivalentTo(
                    CS.LanguageVersion.CSharp6,
                    CS.LanguageVersion.CSharp7,
                    CS.LanguageVersion.CSharp7_1,
                    CS.LanguageVersion.CSharp7_2,
                    CS.LanguageVersion.CSharp7_3,
                    CS.LanguageVersion.CSharp8,
                    CS.LanguageVersion.CSharp9,
                    CS.LanguageVersion.Preview);

                vbVersions.Should().BeEquivalentTo(
                    VB.LanguageVersion.VisualBasic12,
                    VB.LanguageVersion.VisualBasic14,
                    VB.LanguageVersion.VisualBasic15,
                    VB.LanguageVersion.VisualBasic15_3,
                    VB.LanguageVersion.VisualBasic15_5,
                    VB.LanguageVersion.VisualBasic16);
            }
        }
    }
}
