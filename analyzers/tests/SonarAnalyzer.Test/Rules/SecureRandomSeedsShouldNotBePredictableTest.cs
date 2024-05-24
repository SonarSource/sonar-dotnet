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

using ChecksCS = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class SecureRandomSeedsShouldNotBePredictableTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder()
        .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))
        .WithOnlyDiagnostics(ChecksCS.SecureRandomSeedsShouldNotBePredictable.S4347)
        .AddReferences(NuGetMetadataReference.BouncyCastle());

    [TestMethod]
    public void SecureRandomSeedsShouldNotBePredictable_CS() =>
        builder
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .AddPaths("SecureRandomSeedsShouldNotBePredictable.cs")
            .Verify();

    [DataRow("sr.Next()")]
    [DataRow("sr.Next(42)")]
    [DataRow("sr.Next(0, 42)")]
    [DataRow("sr.NextBytes(new byte[8])")]
    [DataRow("sr.NextInt()")]
    [DataRow("sr.NextLong()")]
    [DataRow("sr.NextDouble()")]
    [DataTestMethod]
    public void SecureRandomSeedsShouldNotBePredictable_CS_Next_NoSetSeed(string expression) =>
        builder
            .AddSnippet($$$"""
                using Org.BouncyCastle.Security;
                class Testcases
                {
                    void Method()
                    {
                        var sr = SecureRandom.GetInstance("SHA256PRNG", false);
                        {{{expression}}};                  // Noncompliant {{Set an unpredictable seed before generating random values.}}

                        sr = SecureRandom.GetInstance("SHA256PRNG", true);
                        {{{expression}}};                  // Compliant, autoSeed is set to true

                        sr = SecureRandom.GetInstance("SHA256PRNG");
                        {{{expression}}};                  // Compliant, autoSeed is implicitly set to true
                    }
                }
                """)
            .Verify();

    [DataRow("new byte[42]")]
    [DataRow("new byte[] { 1, 2, 3 }")]
    [DataRow("new byte[] { (byte)'a', (byte)'b', (byte)'c' }")]
    [DataRow("""Encoding.UTF8.GetBytes("exploding whale")""")]
    [DataRow("""Encoding.UTF8.GetBytes(SOME_STRING)""")]
#if NET
    [DataRow("""Encoding.UTF8.GetBytes("abc", 0, 1)""")]
#endif
    [DataRow("""Encoding.UTF8.GetBytes(new char[42])""")]
    [DataRow("""Encoding.UTF8.GetBytes(new char[] {'1', '2'})""")]
    [DataRow("""Encoding.UTF8.GetBytes(new char[] {'1', '2'}, 0, 1)""")]
    [DataRow("""Encoding.UTF8.GetBytes(new char[] {'1', '2'}, 0, 1, null, 42)""")]
    [DataRow("""Convert.FromBase64String("exploding whale")""")]
    [DataRow("""Convert.FromBase64String(SOME_STRING)""")]
    [DataRow("""Convert.FromBase64CharArray(new char[] {'1', '2'}, 0, 42)""")]
    [DataTestMethod]
    public void SecureRandomSeedsShouldNotBePredictable_CS_Next_WithSetSeed(string expression) =>
        builder
            .AddSnippet($$$"""
                using System;
                using System.Text;
                using Org.BouncyCastle.Security;
                class Testcases
                {
                    const string SOME_STRING = "exploding whale";

                    void StartWithUnpredictable(byte[] whoKnows)
                    {
                        var sr = SecureRandom.GetInstance("SHA256PRNG", false);

                        sr.SetSeed(whoKnows);
                        sr.Next();                  // Compliant, seed is updated with unknown data

                        sr.SetSeed({{{expression}}});
                        sr.Next();                  // Compliant, seed is updated with non-random data, but it (might) have already been random
                    }

                    void StartWithPredictable()
                    {
                        var sr = SecureRandom.GetInstance("SHA256PRNG", false);

                        var seed = {{{expression}}};
                        sr.SetSeed(seed);
                        sr.Next();                  // Noncompliant {{Set an unpredictable seed before generating random values.}}
                    //  ^^^^^^^^^

                        sr.SetSeed(Guid.NewGuid().ToByteArray());
                        sr.Next();                  // Compliant, seed is updated with random data

                        sr.SetSeed({{{expression}}});
                        sr.Next();                  // Compliant, seed is updated with non-random data, but it is already random
                    }
                }
                """)
            .Verify();

    [DataRow("bs.SetValue(42, b)")]
    [DataRow("bs[42] = b")]
    [DataRow("bs[42] += b")]
    [DataRow("bs[42] *= b")]
    [DataTestMethod]
    public void SecureRandomSeedsShouldNotBePredictable_CS_ArrayEdited(string messWithArray) =>
    builder
        .AddSnippet($$$"""
            using System;
            using System.Text;
            using Org.BouncyCastle.Security;
            class Testcases
            {
                void Method(byte b)
                {
                    var bs = new byte[42];

                    var sr = SecureRandom.GetInstance("SHA256PRNG", false);
                    sr.SetSeed(bs);
                    sr.Next();                  // Noncompliant

                    {{{messWithArray}}};
                    sr.SetSeed(bs);
                    sr.Next();                  // Compliant
                }
            }
            """)
        .Verify();
}
