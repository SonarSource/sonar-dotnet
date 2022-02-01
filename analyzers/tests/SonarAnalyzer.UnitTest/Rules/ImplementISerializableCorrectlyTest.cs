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
    public class ImplementISerializableCorrectlyTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<ImplementISerializableCorrectly>();

        [TestMethod]
        public void ImplementISerializableCorrectly() =>
            builder.AddPaths("ImplementISerializableCorrectly.cs").Verify();

#if NET

        [TestMethod]
        public void ImplementISerializableCorrectly_FromCSharp9() =>
            builder.AddPaths("ImplementISerializableCorrectly.CSharp9.Part1.cs", "ImplementISerializableCorrectly.CSharp9.Part2.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void ImplementISerializableCorrectly_FromCSharp10() =>
            builder.AddPaths("ImplementISerializableCorrectly.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

#endif

    }
}
