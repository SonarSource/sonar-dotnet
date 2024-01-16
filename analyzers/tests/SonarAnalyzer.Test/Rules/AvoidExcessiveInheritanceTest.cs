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

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class AvoidExcessiveInheritanceTest
    {
        private readonly VerifierBuilder defaultBuilder = new VerifierBuilder<AvoidExcessiveInheritance>();

        [TestMethod]
        public void AvoidExcessiveInheritance_DefaultValues() =>
            defaultBuilder.AddPaths(
                "AvoidExcessiveInheritance_DefaultValues.cs",
                // The test cases are duplicated to make sure the rules can execute in a concurrent manner.
                "AvoidExcessiveInheritance_DefaultValues.Concurrent.cs")
                .WithAutogenerateConcurrentFiles(false)
                .Verify();

#if NET

        [TestMethod]
        public void AvoidExcessiveInheritance_DefaultValues_Records() =>
            defaultBuilder.AddPaths(
                "AvoidExcessiveInheritance_DefaultValues.Records.cs",
                // The test cases are duplicated to make sure the rules can execute in a concurrent manner.
                "AvoidExcessiveInheritance_DefaultValues.Records.Concurrent.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .WithAutogenerateConcurrentFiles(false)
                .Verify();

        [TestMethod]
        public void AvoidExcessiveInheritance_DefaultValues_FileScopedTypes() =>
            defaultBuilder.AddPaths("AvoidExcessiveInheritance_DefaultValues.FileScopedTypes.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
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
        public void AvoidExcessiveInheritance_CustomValuesWildcardFilteredClass() =>
            new VerifierBuilder()
                .AddAnalyzer(() => CreateAnalyzerWithFilter("Tests.Diagnostics.*SubClass"))
                .AddPaths("AvoidExcessiveInheritance_CustomValues.cs")
                .WithAutogenerateConcurrentFiles(false)
                .Verify();

#if NET

        [TestMethod]
        public void AvoidExcessiveInheritance_CustomValuesWildcardFilteredRecord() =>
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
            new() { MaximumDepth = 2, FilteredClasses = filter };
    }
}
