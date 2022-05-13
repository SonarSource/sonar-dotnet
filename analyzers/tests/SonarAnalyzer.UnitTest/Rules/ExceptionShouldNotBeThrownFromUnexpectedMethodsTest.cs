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
    public class ExceptionShouldNotBeThrownFromUnexpectedMethodsTest
    {
        [TestMethod]
        public void ExceptionShouldNotBeThrownFromUnexpectedMethods() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\ExceptionShouldNotBeThrownFromUnexpectedMethods.cs",
                                    new ExceptionShouldNotBeThrownFromUnexpectedMethods(),
                                    ParseOptionsHelper.FromCSharp8);
 #if NET
        [TestMethod]
        public void ExceptionShouldNotBeThrownFromUnexpectedMethods_CSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Library(@"TestCases\ExceptionShouldNotBeThrownFromUnexpectedMethods.CSharp9.cs",
                                                      new ExceptionShouldNotBeThrownFromUnexpectedMethods());
#endif

    }
}
