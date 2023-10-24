/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class UnusedPrivateMemberTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<UnusedPrivateMember>();

#if NET
        // The exception should disappear once the fix for https://github.com/dotnet/roslyn/issues/70041 gets released.
        // If this does not happen before the official release of .NET8 and C#12, the code should be refactored to handle potential null values.
        // This should succeed with latest .NET 8
        [TestMethod]
        public void UnusedPrivateMember_FromCSharp12() =>
            builder.AddPaths("UnusedPrivateMember.CSharp12.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp12)
                .Verify();
#endif
    }
}
