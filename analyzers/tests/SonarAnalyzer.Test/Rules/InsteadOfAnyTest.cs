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
public class InsteadOfAnyTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.InsteadOfAny>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.InsteadOfAny>();

    [TestMethod]
    public void InsteadOfAny_CS() =>
        builderCS.AddPaths("InsteadOfAny.cs").Verify();

#if NET

    [TestMethod]
    public void InsteadOfAny_TopLevelStatements() =>
        builderCS.AddPaths("InsteadOfAny.CSharp9.cs")
                 .WithTopLevelStatements()
                 .AddReferences(MetadataReferenceFacade.SystemCollections)
                 .Verify();
#endif

    [TestMethod]
    public void InsteadOfAny_EntityFramework() =>
        builderCS.AddPaths("InsteadOfAny.EntityFramework.Core.cs")
                 .WithOptions(ParseOptionsHelper.FromCSharp8)
                 .AddReferences(GetReferencesEntityFrameworkNetCore())
                 .Verify();

#if NETFRAMEWORK

    [TestMethod]
    public void InsteadOfAny_EntityFramework_Framework() =>
        builderCS.AddPaths("InsteadOfAny.EntityFramework.Framework.cs")
                 .AddReferences(GetReferencesEntityFrameworkNetFramework())
                 .VerifyNoIssues();

#endif

    [TestMethod]
    public void ExistsInsteadOfAny_VB() =>
        builderVB.AddPaths("InsteadOfAny.vb").WithOptions(ParseOptionsHelper.FromVisualBasic14).Verify();

    [TestMethod]
    public void InsteadOfAny_EntityFramework_VB() =>
        builderVB.AddPaths("InsteadOfAny.EntityFramework.Core.vb")
            .AddReferences(GetReferencesEntityFrameworkNetCore())
            .Verify();

    private static IEnumerable<MetadataReference> GetReferencesEntityFrameworkNetCore() =>
        Enumerable.Empty<MetadataReference>()
        .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCore("2.2.6"))
        .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCoreRelational("2.2.6"))
        .Concat(NuGetMetadataReference.SystemComponentModelTypeConverter());

    private static IEnumerable<MetadataReference> GetReferencesEntityFrameworkNetFramework() =>
        Enumerable.Empty<MetadataReference>()
        .Concat(NuGetMetadataReference.EntityFramework());

}
