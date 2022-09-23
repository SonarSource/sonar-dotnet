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

using Moq;
using Moq.Protected;
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.UnitTest.Extensions
{
    [TestClass]
    public class AttributeDataExtensionsTest
    {
        [DataTestMethod]
        [DataRow("Test", "Test", true)]
        [DataRow("Test", "test", true)]
        [DataRow("Test", "TEST", true)]
        [DataRow("TestAttribute", "Test", true)]
        [DataRow("TestAttribute", "test", true)]
        [DataRow("TestAttribute", "TestTest", false)]
        [DataRow("Test", "TestAttribute", false)]
        [DataRow("Test", "PrefixTest", false)]
        [DataRow("Test", "TestSuffix", false)]
        public void MyTestMethod(string attributeClassName, string testName, bool expected)
        {
            var namedTypeMock = new Mock<INamedTypeSymbol>();
            namedTypeMock.Setup(x => x.Name).Returns(attributeClassName);
            var attributeDataMock = new Mock<AttributeData>();
            attributeDataMock.Protected().Setup<INamedTypeSymbol>("CommonAttributeClass").Returns(namedTypeMock.Object);
            attributeDataMock.Object.HasName(testName).Should().Be(expected);
        }
    }
}
