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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class ImplementIDisposableCorrectlyTest
    {
        [TestMethod]
        public void ImplementIDisposableCorrectly() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\ImplementIDisposableCorrectly.cs", new ImplementIDisposableCorrectly(), ParseOptionsHelper.FromCSharp8);

#if NET
        [TestMethod]
        public void ImplementIDisposableCorrectly_FromCSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Library(@"TestCases\ImplementIDisposableCorrectly.CSharp9.cs", new ImplementIDisposableCorrectly());
#endif

        [TestMethod]
        public void ImplementIDisposableCorrectly_AbstractClass() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\ImplementIDisposableCorrectly.AbstractClass.cs", new ImplementIDisposableCorrectly());

        [TestMethod]
        public void ImplementIDisposableCorrectly_PartialClassesInDifferentFiles() =>
            OldVerifier.VerifyAnalyzer(
                new[]
                {
                    @"TestCases\ImplementIDisposableCorrectlyPartial1.cs",
                    @"TestCases\ImplementIDisposableCorrectlyPartial2.cs"
                }, new ImplementIDisposableCorrectly());
    }
}
