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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class AvoidExcessiveInheritanceTest
    {
        private readonly VerifierBuilder defaultBuilder = new VerifierBuilder<AvoidExcessiveInheritance>();

        [TestMethod]
        public void AvoidExcessiveInheritance_DefaultValues() =>
            defaultBuilder.AddPaths(
                "AvoidExcessiveInheritance_DefaultValues.cs",
                "AvoidExcessiveInheritance_DefaultValues2.cs")
                .WithAutogenerateConcurrentFiles(false)
                .Verify();

#if NET

        [TestMethod]
        public void AvoidExcessiveInheritance_DefaultValues_Records() =>
            defaultBuilder.AddPaths(
                "AvoidExcessiveInheritance_DefaultValues.Records.cs",
                "AvoidExcessiveInheritance_DefaultValues.Records2.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .WithAutogenerateConcurrentFiles(false)
                .Verify();

#endif

        [TestMethod]
        public void AvoidExcessiveInheritance_CustomValuesFullyNamedFilteredClass() =>
            new VerifierBuilder()
                .AddAnalyzer(() => CreateAnalyzerWithFilter("Tests.Diagnostics.SecondSubClass"))
                .AddPaths("AvoidExcessiveInheritance_CustomValues.cs")
                .WithAutogenerateConcurrentFiles(false)
                .Verify();

        [TestMethod]
        public void AvoidExcessiveInheritance_CustomValuesWilcardFilteredClass() =>
            new VerifierBuilder()
                .AddAnalyzer(() => CreateAnalyzerWithFilter("Tests.Diagnostics.*SubClass"))
                .AddPaths("AvoidExcessiveInheritance_CustomValues.cs")
                .WithAutogenerateConcurrentFiles(false)
                .Verify();

#if NET

        [TestMethod]
        public void AvoidExcessiveInheritance_CustomValuesWilcardFilteredRecord() =>
            new VerifierBuilder()
                .AddAnalyzer(() => CreateAnalyzerWithFilter("Tests.Diagnostics.*SubRecord"))
                .AddPaths("AvoidExcessiveInheritance_CustomValues.Records.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .WithAutogenerateConcurrentFiles(false)
                .Verify();

#endif

        [TestMethod]
        public void FilteredClasses_ByDefault_ShouldBeEmpty() =>
            new AvoidExcessiveInheritance().FilteredClasses.Should().BeEmpty();

        private static AvoidExcessiveInheritance CreateAnalyzerWithFilter(string filter) =>
            new AvoidExcessiveInheritance { MaximumDepth = 2, FilteredClasses = filter };
    }
}
