/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class MemberShouldBeStaticTest
    {
        [DataTestMethod]
        [DataRow("1.0.0", "3.0.20105.1")]
        [DataRow(Constants.NuGetLatestVersion, Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void MemberShouldBeStatic(string aspnetCoreVersion, string aspnetVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\MemberShouldBeStatic.cs",
                new MemberShouldBeStatic(),
                additionalReferences: NuGetMetadataReference.MicrosoftAspNetCoreMvcWebApiCompatShim(aspnetCoreVersion)
                    .Concat(NuGetMetadataReference.MicrosoftAspNetMvc(aspnetVersion))
                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(aspnetCoreVersion))
                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcViewFeatures(aspnetCoreVersion))
                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreRoutingAbstractions(aspnetCoreVersion))
                    .Concat(FrameworkMetadataReference.SystemWeb)
                    .ToImmutableArray());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void MemberShouldBeStatic_InvalidCode()
        {
            // Handle invalid code causing NullReferenceException: https://github.com/SonarSource/sonar-csharp/issues/819
            Verifier.VerifyCSharpAnalyzer(@"
public class Class7
{
    public async Task<Result<T> Function<T>(Func<Task<Result<T>>> f)
    {
        Result<T> result;
        result = await f();
        return result;
    }
}", new MemberShouldBeStatic(), checkMode: CompilationErrorBehavior.Ignore);
        }
    }
}
