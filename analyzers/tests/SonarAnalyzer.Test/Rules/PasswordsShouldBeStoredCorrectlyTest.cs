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

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class PasswordsShouldBeStoredCorrectlyTest
{
    private static readonly VerifierBuilder Builder = new VerifierBuilder<PasswordsShouldBeStoredCorrectly>();

#if NET
    [TestMethod]
    public void PasswordsShouldBeStoredCorrectly_CS_Core() =>
        Builder
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .AddPaths("PasswordsShouldBeStoredCorrectly.Core.cs")
            .AddReferences([
                AspNetCoreMetadataReference.MicrosoftExtensionsIdentityCore,
                AspNetCoreMetadataReference.MicrosoftAspNetCoreCryptographyKeyDerivation,
                ..MetadataReferenceFacade.SystemSecurityCryptography,
            ])
            .Verify();
#endif

    [TestMethod]
    public void PasswordsShouldBeStoredCorrectly_CS_PasswordHasherOptions() =>
        Builder
            .WithOptions(ParseOptionsHelper.FromCSharp9)
            .AddReferences(NuGetMetadataReference.MicrosoftAspNetIdentity())
            .AddSnippet("""
                using Microsoft.AspNet.Identity;

                class Testcases
                {
                   void Method()
                   {
                        var _ = new PasswordHasherOptions();                    // Noncompliant {{PasswordHasher does not support state-of-the-art parameters. Use Rfc2898DeriveBytes instead.}}
                        //      ^^^^^^^^^^^^^^^^^^^^^^^^^^^
                        PasswordHasherOptions x = new();                        // Noncompliant
                        //                        ^^^^^
                        _ = new PasswordHasherOptions() {};                     // Noncompliant
                        //  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                        _ = new PasswordHasherOptions { IterationCount = 42 };  // Noncompliant
                        //  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                        _ = new Derived();                                      // Noncompliant
                        //  ^^^^^^^^^^^^^
                   }
                }

                public class Derived: PasswordHasherOptions { }
                """)
            .Verify();

    [TestMethod]
    public void PasswordsShouldBeStoredCorrectly_CS_Rfc2898DeriveBytes() =>
        Builder
            .AddReferences(MetadataReferenceFacade.SystemSecurityCryptography)
            .AddSnippet("""
                using System.Security.Cryptography;
                class Testcases
                {
                    const int ITERATIONS = 100_000;

                    void Method(int iterations, byte[] bs, HashAlgorithmName han)
                    {
                        new Rfc2898DeriveBytes("password", bs);                             // Noncompliant
                        new Rfc2898DeriveBytes("password", 42);                             // Noncompliant
                        new Rfc2898DeriveBytes(bs, bs, 42);                                 // Noncompliant
                    //  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                        new Rfc2898DeriveBytes(iterations: 42, salt: bs, password: bs);     // Noncompliant {{Use at least 100,000 iterations and a state-of-the-art digest algorithm here.}}
                    //  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                        new Rfc2898DeriveBytes("password", bs, 42);                         // Noncompliant
                        new Rfc2898DeriveBytes("password", 42, 42);                         // Noncompliant
                        new Rfc2898DeriveBytes(bs, bs, 42, han);                            // Noncompliant {{Use at least 100,000 iterations here.}}
                    //                                 ^^
                        new Rfc2898DeriveBytes("password", bs, 42, han);                    // Noncompliant
                        new Rfc2898DeriveBytes("password", 42, 42, han);                    // Noncompliant
                        new Rfc2898DeriveBytes(bs, bs, ITERATIONS);                         // Noncompliant
                        new Rfc2898DeriveBytes("", bs, ITERATIONS);                         // Noncompliant
                        new Rfc2898DeriveBytes("", 42, ITERATIONS);                         // Noncompliant
                    //  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

                        new Rfc2898DeriveBytes(bs, bs, iterations, han);                    // Compliant
                        new Rfc2898DeriveBytes(bs, bs, ITERATIONS, han);                    // Compliant
                        new Rfc2898DeriveBytes("", bs, ITERATIONS, han);                    // Compliant
                        new Rfc2898DeriveBytes("", 42, ITERATIONS, han);                    // Compliant

                        var x = new Rfc2898DeriveBytes(bs, bs, ITERATIONS, han);
                        x.IterationCount = 1;                                               // Noncompliant {{Use at least 100,000 iterations here.}}
                    //  ^^^^^^^^^^^^^^^^

                        x.IterationCount = 100_042;                                         // Compliant

                        new Rfc2898DeriveBytes(bs, bs, ITERATIONS, han)
                        {
                            IterationCount = 42                                             // Noncompliant {{Use at least 100,000 iterations here.}}
                    //      ^^^^^^^^^^^^^^
                        };
                    }

                    void MakeItUnsafe(Rfc2898DeriveBytes password)
                    {
                        password.IterationCount = 1;                                        // Noncompliant {{Use at least 100,000 iterations here.}}
                    //  ^^^^^^^^^^^^^^^^^^^^^^^
                    }
                }
                """)
            .Verify();

    [TestMethod]
    public void PasswordsShouldBeStoredCorrectly_CS_BouncyCastle_Generate() =>
        Builder
            .AddReferences(NuGetMetadataReference.BouncyCastle())
            .AddSnippet("""
            using Org.BouncyCastle.Crypto.Generators;

            class Testcases
            {
                const int COST = 12;

                void Method(char[] cs, byte[] bs)
                {
                    OpenBsdBCrypt.Generate(cs, bs, 4);                          // Noncompliant {{Use a cost factor of at least 12 here.}}
                    OpenBsdBCrypt.Generate(cost: 4, password: cs, salt: bs);    // Noncompliant
                    OpenBsdBCrypt.Generate("", cs, bs, 4);                      // Noncompliant
                    //                                 ^
                    OpenBsdBCrypt.Generate(
                        cost: 4,                                                // Noncompliant
                    //  ^^^^^^^
                        version: "",
                        password: cs,
                        salt: bs);

                    OpenBsdBCrypt.Generate(cs, bs, COST);                       // Compliant
                    OpenBsdBCrypt.Generate(cs, bs, 42);                         // Compliant
                    OpenBsdBCrypt.Generate("", cs, bs, COST);                   // Compliant
                    OpenBsdBCrypt.Generate("", cs, bs, 42);                     // Compliant
                    OpenBsdBCrypt.Generate(
                        cost: 42,                                                // Compliant
                        version: "",
                        password: cs,
                        salt: bs);

                    BCrypt.Generate(bs, bs, 4);                                  // Noncompliant
                    //                      ^
                    BCrypt.Generate(bs, bs, COST);                               // Compliant
                }
            }
            """)
            .Verify();

    [TestMethod]
    public void PasswordsShouldBeStoredCorrectly_CS_BouncyCastle_Init() =>
        Builder
            .AddReferences(NuGetMetadataReference.BouncyCastle())
            .AddSnippet("""
            using Org.BouncyCastle.Crypto;
            using Org.BouncyCastle.Crypto.Generators;

            class Testcases
            {
                const int ITERATIONS = 100_000;

                void Method(byte[] bs, PbeParametersGenerator baseGen, Pkcs5S2ParametersGenerator gen, int iterations)
                {
                    baseGen.Init(bs, bs, 42);                                       // Noncompliant {{Use at least 100,000 iterations here.}}
                //                       ^^
                    gen.Init(iterationCount: 42, password: bs, salt: bs);           // Noncompliant
                //           ^^^^^^^^^^^^^^^^^^

                    baseGen.Init(bs, bs, ITERATIONS);                               // Compliant
                    gen.Init(iterationCount: iterations, password: bs, salt: bs);   // Compliant
                }
            }
            """)
            .Verify();

    [TestMethod]
    public void PasswordsShouldBeStoredCorrectly_CS_BouncyCastle_Generate_SCrypt() =>
        Builder
            .AddReferences(NuGetMetadataReference.BouncyCastle())
            .AddSnippet("""
            using Org.BouncyCastle.Crypto.Generators;

            class Testcases
            {
                void Method(byte[] bs, int p)
                {
                    SCrypt.Generate(bs, bs, 1 << 12, 8, p, 32);         // Compliant

                    SCrypt.Generate(bs, bs, 1 << 11, 42, p, 42);        // Noncompliant {{Use a cost factor of at least 2 ^ 12 for N here.}}
                    //                      ^^^^^^^
                    SCrypt.Generate(bs, bs, 1 << 12, 7, p, 42);         // Noncompliant {{Use a memory factor of at least 8 for r here.}}
                    //                               ^
                    SCrypt.Generate(bs, bs, 1 << 12, 42, p, 31);        // Noncompliant {{Use an output length of at least 32 for dkLen here.}}
                    //                                      ^^

                    SCrypt.Generate(bs, bs, 1 << 11, 7, p, 31);
                    //                      ^^^^^^^ {{Use a cost factor of at least 2 ^ 12 for N here.}}
                    //                               ^ @-1 {{Use a memory factor of at least 8 for r here.}}
                    //                                     ^^ @-2 {{Use an output length of at least 32 for dkLen here.}}
                }
            }
            """)
            .Verify();
}
