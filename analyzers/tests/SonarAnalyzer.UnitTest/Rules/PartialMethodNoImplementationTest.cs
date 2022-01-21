/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
    public class PartialMethodNoImplementationTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<PartialMethodNoImplementation>();

        [TestMethod]
        public void PartialMethodNoImplementation() =>
            builder.AddPaths("PartialMethodNoImplementation.cs").Verify();

#if NET

        [TestMethod]
        public void PartialMethodNoImplementation_CSharp9() =>
            builder.AddPaths("PartialMethodNoImplementation.CSharp9.Part1.cs", "PartialMethodNoImplementation.CSharp9.Part2.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void PartialMethodNoImplementation_CSharp10() =>
            builder.AddPaths("PartialMethodNoImplementation.CSharp10.Part1.cs", "PartialMethodNoImplementation.CSharp10.Part2.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

#endif

    }
}
