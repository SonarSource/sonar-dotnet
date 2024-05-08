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
    public class FinalizerShouldNotBeEmptyTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<FinalizerShouldNotBeEmpty>();

        [TestMethod]
        public void FinalizerShouldNotBeEmpty() =>
            builder.AddPaths("FinalizerShouldNotBeEmpty.cs").Verify();

#if NET

        [TestMethod]
        public void FinalizerShouldNotBeEmpty_CSharp9() =>
            builder.AddPaths("FinalizerShouldNotBeEmpty.CSharp9.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();

#endif

        [TestMethod]
        public void FinalizerShouldNotBeEmpty_InvalidCode() =>
            builder.AddSnippet("""
                class Program4
                {
                    ~Program4() =>  // Error [CS1002] ; expected
                                    // Error@-1 [CS1525] Invalid expression term
                }
                """)
                .Verify();
    }
}
