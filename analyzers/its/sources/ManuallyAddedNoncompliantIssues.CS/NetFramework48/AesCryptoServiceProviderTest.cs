/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

using System.Security.Cryptography;

namespace NetFramework48
{
    public class AesCryptoServiceProviderTest
    {
        public static void TestCases()
        {
            using (var sa = new AesCryptoServiceProvider())
            {
                sa.CreateEncryptor();
                sa.CreateEncryptor(sa.Key, new byte[16]); // Noncompliant (S3329) {{Use a dynamically-generated, random IV.}}
            }
        }
    }
}
