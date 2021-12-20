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
using SonarAnalyzer.UnitTest.MetadataReferences;

namespace SonarAnalyzer.UnitTest.TestFramework.Tests
{
    [TestClass]
    public partial class VerifierBuilderTest
    {
        private static readonly VerifierBuilder<DummyAnalyzer> Empty = new();

        [TestMethod]
        public void AddPaths_Concatenates_IsImmutable()
        {
            var single = Empty.AddPaths("First");
            var three = single.AddPaths("Second", "Third");
            Empty.Paths.Should().BeEmpty();
            single.Paths.Should().ContainSingle().Which.Should().Be("First");
            three.Paths.Should().BeEquivalentTo("First", "Second", "Third");
        }

        [TestMethod]
        public void AddReferences_Concatenates_IsImmutable()
        {
            var single = Empty.AddReferences(MetadataReferenceFacade.MsCorLib);
            var two = single.AddReferences(MetadataReferenceFacade.SystemData);
            Empty.References.Should().BeEmpty();
            single.References.Should().BeEquivalentTo(MetadataReferenceFacade.MsCorLib).And.HaveCount(1);
            two.References.Should().BeEquivalentTo(MetadataReferenceFacade.MsCorLib.Concat(MetadataReferenceFacade.SystemData)).And.HaveCount(2);
        }

        [TestMethod]
        public void WithOptions_Overrides_IsImmutable()
        {
            var first = Empty.WithOptions(ParseOptionsHelper.OnlyCSharp7);
            var second = first.WithOptions(ParseOptionsHelper.FromCSharp8);
            Empty.ParseOptions.Should().BeEmpty();
            first.ParseOptions.Should().BeEquivalentTo(ParseOptionsHelper.OnlyCSharp7);
            second.ParseOptions.Should().BeEquivalentTo(ParseOptionsHelper.FromCSharp8);
        }

        [TestMethod]
        public void Build_ReturnsVerifier() =>
            new VerifierBuilder<DummyAnalyzer>().Build().Should().NotBeNull();
    }
}
