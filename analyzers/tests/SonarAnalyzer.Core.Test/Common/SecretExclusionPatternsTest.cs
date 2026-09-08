/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Common.Test;

[TestClass]
public class SecretExclusionPatternsTest
{
    [TestMethod]
    public void IsKnownNonSecret_Null_ReturnsFalse() =>
        SecretExclusionPatterns.IsKnownNonSecret(null).Should().BeFalse();

    [TestMethod]
    public void GeneratedCatalog_ContainsData()
    {
        SecretExclusionPatterns.Regexes.Should().NotBeEmpty();
        SecretExclusionPatterns.ExactMatches.Should().NotBeEmpty();
        SecretExclusionCorpus.KnownNonSecrets.Should().NotBeEmpty();
        SecretExclusionCorpus.SecretCandidates.Should().NotBeEmpty();
    }

    [TestMethod]
    public void KnownNonSecrets_AreAllExcluded() =>
        SecretExclusionCorpus.KnownNonSecrets.Should().OnlyContain(
            x => SecretExclusionPatterns.IsKnownNonSecret(x),
            "every corpus value classified as a known non-secret upstream must be excluded by the .NET Regex engine as well");

    [TestMethod]
    public void SecretCandidates_AreNotExcluded() =>
        SecretExclusionCorpus.SecretCandidates.Should().NotContain(
            x => SecretExclusionPatterns.IsKnownNonSecret(x),
            "a pattern that is too broad in .NET hides real hardcoded secrets instead of merely adding noise");

    [TestMethod]
    // "token" is an exact-match value, and ExactMatches is matched in full: a value that merely contains one stays a secret candidate.
    [DataRow("mytoken123")]
    [DataRow("this_should_remain_unknown")]
    public void IsKnownNonSecret_ValueContainingExactMatch_ReturnsFalse(string value) =>
        SecretExclusionPatterns.IsKnownNonSecret(value).Should().BeFalse();

    [TestMethod]
    public void Regexes_EveryPatternIsExercisedByTheCorpus() =>
        SecretExclusionPatterns.Regexes.Should().OnlyContain(
            x => SecretExclusionCorpus.KnownNonSecrets.Any(x.SafeIsMatch),
            "upstream guarantees every pattern is covered by at least one known non-secret, so an unmatched one means .NET reads it differently");

    [TestMethod]
    public void ExactMatches_EveryValueIsExercisedByTheCorpus() =>
        // ExactMatches is a case-insensitive HashSet in production, so casing must not decide whether a value counts as covered.
        SecretExclusionPatterns.ExactMatches.Should().OnlyContain(
            x => SecretExclusionCorpus.KnownNonSecrets.Contains(x, StringComparer.OrdinalIgnoreCase),
            "upstream guarantees every exact-match value is covered by at least one known non-secret");
}
