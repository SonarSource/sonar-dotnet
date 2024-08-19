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
    public class CallToAsyncMethodShouldNotBeBlockingTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<CallToAsyncMethodShouldNotBeBlocking>();

        [TestMethod]
        public void CallToAsyncMethodShouldNotBeBlocking() =>
            builder.AddPaths("CallToAsyncMethodShouldNotBeBlocking.cs")
                .AddReferences(NuGetMetadataReference.MicrosoftNetSdkFunctions())
                .Verify();

#if NET

        [TestMethod]
        public void CallToAsyncMethodShouldNotBeBlocking_CSharp9() =>
            builder.AddPaths("CallToAsyncMethodShouldNotBeBlocking.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void CallToAsyncMethodShouldNotBeBlocking_CSharp11() =>
            builder.AddPaths("CallToAsyncMethodShouldNotBeBlocking.CSharp11.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

#endif

    }
}
