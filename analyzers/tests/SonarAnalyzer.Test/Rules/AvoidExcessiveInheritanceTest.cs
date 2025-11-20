/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CSharp.Rules;

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

        [TestMethod]
        public void AvoidExcessiveInheritance_DefaultValues_Records() =>
            defaultBuilder.AddPaths(
                "AvoidExcessiveInheritance_DefaultValues.Records.cs",
                // The test cases are duplicated to make sure the rules can execute in a concurrent manner.
                "AvoidExcessiveInheritance_DefaultValues.Records.Concurrent.cs")
                .WithOptions(LanguageOptions.FromCSharp9)
                .WithAutogenerateConcurrentFiles(false)
                .Verify();

        [TestMethod]
        public void AvoidExcessiveInheritance_DefaultValues_FileScopedTypes() =>
            defaultBuilder.AddPaths("AvoidExcessiveInheritance_DefaultValues.FileScopedTypes.cs")
                .WithOptions(LanguageOptions.FromCSharp11)
                .Verify();

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

        [TestMethod]
        public void AvoidExcessiveInheritance_CustomValuesWildcardFilteredRecord() =>
            new VerifierBuilder()
                .AddAnalyzer(() => CreateAnalyzerWithFilter("Tests.Diagnostics.*SubRecord"))
                .AddPaths("AvoidExcessiveInheritance_CustomValues.Records.cs")
                .WithOptions(LanguageOptions.FromCSharp9)
                .WithAutogenerateConcurrentFiles(false)
                .Verify();

        [TestMethod]
        public void FilteredClasses_ByDefault_ShouldBeEmpty() =>
            new AvoidExcessiveInheritance().FilteredClasses.Should().BeEmpty();

        private static AvoidExcessiveInheritance CreateAnalyzerWithFilter(string filter) =>
            new() { MaximumDepth = 2, FilteredClasses = filter };
    }
}
