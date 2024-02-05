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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class FindInsteadOfFirstOrDefaultTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.FindInsteadOfFirstOrDefault>().WithConcurrentAnalysis(false);
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.FindInsteadOfFirstOrDefault>();

    [TestMethod]
    public void FindInsteadOfFirstOrDefault_CS() =>
        builderCS.AddPaths("FindInsteadOfFirstOrDefault.cs").AddReferences(GetReferencesEntityFrameworkNetCore("7.0.5")).Verify();

    internal static IEnumerable<MetadataReference> GetReferencesEntityFrameworkNetCore(string entityFrameworkVersion) =>
        Enumerable.Empty<MetadataReference>()
            .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCore(entityFrameworkVersion))
            .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCoreRelational(entityFrameworkVersion));

#if NET

    [TestMethod]
    public void FindInsteadOfFirstOrDefault_Immutable_CS() =>
        builderCS.AddPaths("FindInsteadOfFirstOrDefault.Immutable.cs").AddReferences(MetadataReferenceFacade.SystemCollections).Verify();

    [TestMethod]
    public void FindInsteadOfFirstOrDefault_Net_CS() =>
        builderCS.AddPaths("FindInsteadOfFirstOrDefault.Net.cs").Verify();

    [TestMethod]
    public void FindInsteadOfFirstOrDefault_Net_VB() =>
        builderVB.AddPaths("FindInsteadOfFirstOrDefault.Net.vb").Verify();

#endif

    [TestMethod]
    public void FindInsteadOfFirstOrDefault_Array_CS() =>
        builderCS.AddPaths("FindInsteadOfFirstOrDefault.Array.cs").Verify();

    [TestMethod]
    public void FindInsteadOfFirstOrDefault_VB() =>
        builderVB.AddPaths("FindInsteadOfFirstOrDefault.vb").WithOptions(ParseOptionsHelper.FromVisualBasic14).Verify();
}
