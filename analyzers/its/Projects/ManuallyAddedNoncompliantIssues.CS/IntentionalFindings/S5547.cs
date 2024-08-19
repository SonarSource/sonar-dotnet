/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */
using System.Security.Cryptography;

namespace IntentionalFindings
{
    public static class S5547
    {
        public static void TestMethod()
        {
            var tripleDES1 = new TripleDESCryptoServiceProvider(); // Noncompliant (S5547): Triple DES is vulnerable to meet-in-the-middle attack

            var simpleDES = new DESCryptoServiceProvider(); // Noncompliant: DES works with 56-bit keys allow attacks via exhaustive search

            var RC2 = new RC2CryptoServiceProvider(); // Noncompliant: RC2 is vulnerable to a related-key attack

            var AES = new AesCryptoServiceProvider(); // Compliant
        }
    }
}
