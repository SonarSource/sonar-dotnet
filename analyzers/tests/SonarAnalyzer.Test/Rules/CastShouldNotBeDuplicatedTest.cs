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

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class CastShouldNotBeDuplicatedTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<CastShouldNotBeDuplicated>();

        [TestMethod]
        public void CastShouldNotBeDuplicated() =>
            builder.AddPaths("CastShouldNotBeDuplicated.cs").Verify();

#if NET

        [TestMethod]
        public void CastShouldNotBeDuplicated_CSharp9() =>
            builder.AddPaths("CastShouldNotBeDuplicated.CSharp9.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();

        [TestMethod]
        public void CastShouldNotBeDuplicated_CSharp10() =>
            builder.AddPaths("CastShouldNotBeDuplicated.CSharp10.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .Verify();

        [TestMethod]
        public void CastShouldNotBeDuplicated_CSharp11() =>
            builder.AddPaths("CastShouldNotBeDuplicated.CSharp11.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

        [TestMethod]
        public void CastShouldNotBeDuplicated_CSharp12() =>
            builder.AddPaths("CastShouldNotBeDuplicated.CSharp12.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp12)
                .Verify();

#endif

    }
}
