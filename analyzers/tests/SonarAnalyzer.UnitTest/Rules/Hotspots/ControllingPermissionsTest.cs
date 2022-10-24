/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

#if NETFRAMEWORK // IdentityModel is not available on .Net Core

using SonarAnalyzer.Common;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class ControllingPermissionsTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new CS.ControllingPermissions(AnalyzerConfiguration.AlwaysEnabled));
        private readonly VerifierBuilder builderVB = new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new VB.ControllingPermissions(AnalyzerConfiguration.AlwaysEnabled));

        [TestMethod]
        public void ControllingPermissions_CS() =>
            builderCS.AddPaths("ControllingPermissions.cs")
                .AddReferences(AdditionalReferences)
                .Verify();

        [TestMethod]
        public void ControllingPermissions_VB() =>
            builderVB.AddPaths("ControllingPermissions.vb")
                .AddReferences(AdditionalReferences)
                .Verify();

        internal static readonly IEnumerable<MetadataReference> AdditionalReferences =
            Enumerable.Empty<MetadataReference>()
                .Concat(FrameworkMetadataReference.SystemIdentityModel)
                .Concat(FrameworkMetadataReference.SystemWeb);
    }
}

#endif
