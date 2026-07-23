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
    // Realistic high-entropy tokens must stay candidates.
    [DataRow("Xk9Lm2Qp7Rs4Tv1Wz0")]
    [DataRow("9f8e7d6c5b4a392817")]
    [DataRow("Tr0ub4dor&3xpl0!t")]
    // Credential words are exact-match only, so a value that merely contains one is not excluded.
    [DataRow("mytoken123")]
    [DataRow("this_should_remain_unknown")]
    public void IsKnownNonSecret_RealSecretsAndCredentialWordSubstrings_ReturnsFalse(string value) =>
        SecretExclusionPatterns.IsKnownNonSecret(value).Should().BeFalse();

    [TestMethod]
    public void IsKnownNonSecret_Null_ReturnsFalse() =>
        SecretExclusionPatterns.IsKnownNonSecret(null).Should().BeFalse();

    [TestMethod]
    public void GeneratedCatalog_ContainsData()
    {
        SecretExclusionPatterns.Regexes.Should().NotBeEmpty();
        SecretExclusionPatterns.ExactMatches.Should().NotBeEmpty();
    }

    // ToDo: Use secret-exclusion-corpus.json instead, once SonarAnalyzer.CommonsConfigurations publishes it.
    [TestMethod]
    public void Regexes_AreValid()
    {
        // Subset of SecretClassifierTest.KNOWN_NON_SECRETS from sonar-analyzer-commons
        var knownNonSecrets = new[]
        {
            // SECRET (exact match, case-insensitive)
            "hunter2", "letmein", "abc123",
            "changeme", "changeit", "unknown", "optional", "enabled", "disabled", "string", "random", "token",
            // PLACEHOLDER
            "${secret}", "value-${pwd}", "#{{secret}}", "((db-password))",
            "$(echo $PASSWORD)", "`echo $PASSWORD`", "$foo_bar",
            "{secret}", "%{secret}", "{{secret}}",
            "System.getenv(\"secret\")", "process.env.MY_SECRET", "%GITHUB_TOKEN%", "config['secret']", "Read-Host",
            "<password>", "(password)", "[password]", "%(password)s", "@variables('name')", "__secret__",
            // ENCRYPTED
            "encrypted:YWJjZGVm", "{cipher}1e3faa2cdab2deae117dca102e52922a", "enc[QUJDRA==]", "ENC{abcdef}", "%enc{QUJDRA==}", "ENC(abcdef)",
            // REFERENCE
            "arn:aws:secretsmanager:us-east-1:123456789012:secret:db-pass", "op://vault/item/password", "VAULT[path/to/secret access_token]",
            // STRUCTURED_FORMAT
            "/var/keys/gsa-key.json", "v1.2.3", ">=1.0.0", "~1.4.5-alpha", "4.0.9(@types/node@22.13.4)"
        };
        foreach (var value in knownNonSecrets)
        {
            SecretExclusionPatterns.IsKnownNonSecret(value).Should().BeTrue($"value '{value}' should be known non-secret");
        }
    }
}
