/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

extern alias csharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using csharp::SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class RedundantArgumentTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void RedundantArgument() =>
            Verifier.VerifyAnalyzer(@"TestCases\RedundantArgument.cs",
                                    new RedundantArgument(),
                                    ParseOptionsHelper.FromCSharp8,
                                    additionalReferences: NuGetMetadataReference.NETStandardV2_1_0);

// On .Net Core 3.1 the generated code cannot be compiled.
// See: https://github.com/SonarSource/sonar-dotnet/issues/3430
#if NETFRAMEWORK
        [TestMethod]
        [TestCategory("CodeFix")]
        public void RedundantArgument_CodeFix_No_Named_Arguments() =>
            Verifier.VerifyCodeFix(@"TestCases\RedundantArgument.cs",
                                   @"TestCases\RedundantArgument.NoNamed.Fixed.cs",
                                   new RedundantArgument(),
                                   new RedundantArgumentCodeFixProvider(),
                                   RedundantArgumentCodeFixProvider.TitleRemove,
                                   ParseOptionsHelper.FromCSharp8,
                                   NuGetMetadataReference.NETStandardV2_1_0);
#endif

        [TestMethod]
        [TestCategory("CodeFix")]
        public void RedundantArgument_CodeFix_Named_Arguments() =>
            Verifier.VerifyCodeFix(@"TestCases\RedundantArgument.cs",
                                   @"TestCases\RedundantArgument.Named.Fixed.cs",
                                   new RedundantArgument(),
                                   new RedundantArgumentCodeFixProvider(),
                                   RedundantArgumentCodeFixProvider.TitleRemoveWithNameAdditions,
                                   ParseOptionsHelper.FromCSharp8,
                                   NuGetMetadataReference.NETStandardV2_1_0);
    }
}
