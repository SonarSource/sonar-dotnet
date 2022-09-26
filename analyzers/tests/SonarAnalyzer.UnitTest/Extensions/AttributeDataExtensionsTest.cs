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
        [DataRow(true, "Test", "Test")]
        [DataRow(true, "Test", "test")]
        [DataRow(true, "Test", "TEST")]
        [DataRow(true, "TestAttribute", "Test")]
        [DataRow(true, "TestAttribute", "test")]
        [DataRow(false, "TestAttribute", "testAttr")]
        [DataRow(false, "TestAttribute", "TestAttr")]
        [DataRow(false, "TestAttribute", "TestAttributes")]
        [DataRow(false, "TestAttribute", "TestTest")]
        [DataRow(false, "Test", "PrefixTest")]
        [DataRow(false, "Test", "TestSuffix")]
        public void HasName(bool expected, string attributeClassName, string testName) =>
            AttributeDataWithName(attributeClassName).HasName(testName).Should().Be(expected);

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("Attribute")]
        [DataRow("   Attribute")]
        [DataRow("SomeAttribute")]
        [DataRow("TestAttribute")]
        [DataRow("TestAttributeAttribute")]
        public void HasNameThrows(string testName) =>
            new Action(() => AttributeDataWithName("Test").HasName(testName)).Should().Throw<Exception>();

        [DataTestMethod]
        [DataRow(true, "Test", "Test", "Test")]
        [DataRow(true, "Test", "test", "other")]
        [DataRow(true, "Test", "other", "test")]
        [DataRow(true, "TestAttribute", "test", "other")]
        [DataRow(false, "TestAttribute", "other", "other")]
        [DataRow(false, "TestAttribute", "other1", "other2", "other3")]
        [DataRow(true, "TestAttribute", "other1", "other2", "other3", "Test")]
        [DataRow(true, "TestAttribute", "Test", "test", "SomeAttribute")]
        public void HasAnyName(bool expected, string attributeClassName, params string[] testNames) =>
            AttributeDataWithName(attributeClassName).HasAnyName(testNames).Should().Be(expected);

        [DataTestMethod]
        [DataRow(null)]
        [DataRow(new[] { (string)null })]
        [DataRow(new[] { "Attribute" })]
        [DataRow(new[] { "   Attribute" })]
        [DataRow(new[] { "FailAttribute" })]
        [DataRow(new[] { "Some", "Other", "FailAttribute" })]
        public void HasAnyNameThrows(string[] testNames) =>
            new Action(() => AttributeDataWithName("Test").HasAnyName(testNames)).Should().Throw<Exception>();

        private static AttributeData AttributeDataWithName(string attributeClassName)
        {
            var namedType = new Mock<INamedTypeSymbol>();
            namedType.Setup(x => x.Name).Returns(attributeClassName);
            var attributeData = new Mock<AttributeData>();
            attributeData.Protected().Setup<INamedTypeSymbol>("CommonAttributeClass").Returns(namedType.Object);
            return attributeData.Object;
        }
    }
}
