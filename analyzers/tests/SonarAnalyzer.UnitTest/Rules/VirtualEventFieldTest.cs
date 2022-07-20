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
    public class VirtualEventFieldTest
    {
        [Ignore][TestMethod]
        public void VirtualEventField() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\VirtualEventField.cs", new VirtualEventField());

#if NET
        [Ignore][TestMethod]
        public void VirtualEventField_CSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Library(@"TestCases\VirtualEventField.CSharp9.cs", new VirtualEventField());

        [Ignore][TestMethod]
        public void VirtualEventField_CSharp9_CodeFix() =>
            OldVerifier.VerifyCodeFix<VirtualEventFieldCodeFix>(
                @"TestCases\VirtualEventField.CSharp9.cs",
                @"TestCases\VirtualEventField.CSharp9.Fixed.cs",
                new VirtualEventField(),
                ParseOptionsHelper.FromCSharp9);
#endif

        [Ignore][TestMethod]
        public void VirtualEventField_CodeFix() =>
            OldVerifier.VerifyCodeFix<VirtualEventFieldCodeFix>(
                @"TestCases\VirtualEventField.cs",
                @"TestCases\VirtualEventField.Fixed.cs",
                new VirtualEventField());
    }
}
