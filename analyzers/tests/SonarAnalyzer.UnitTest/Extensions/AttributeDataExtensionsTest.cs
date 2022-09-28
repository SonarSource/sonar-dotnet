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

using System.Reflection;
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
        [DataRow(true, "TestAttribute", "TestAttribute")]
        [DataRow(false, "TestAttribute", null)]
        [DataRow(false, "Test", "test")]
        [DataRow(false, "Test", "TEST")]
        [DataRow(false, "TestAttribute", "Test")]
        [DataRow(false, "TestAttribute", "testAttribute")]
        [DataRow(false, "TestAttribute", "test")]
        [DataRow(false, "TestAttribute", "testAttr")]
        [DataRow(false, "TestAttribute", "TestAttr")]
        [DataRow(false, "TestAttribute", "TestAttributes")]
        [DataRow(false, "TestAttribute", "TestTest")]
        [DataRow(false, "Test", "PrefixTest")]
        [DataRow(false, "Test", "TestSuffix")]
        public void HasName(bool expected, string attributeClassName, string testName) =>
            AttributeDataWithName(attributeClassName).HasName(testName).Should().Be(expected);

        [DataTestMethod]
        [DataRow(true, "Test", "Test", "Test")]
        [DataRow(true, "TestAttribute", "TestAttribute", "TestAttribute")]
        [DataRow(false, "Test", "test", "other")]
        [DataRow(false, "Test", "other", "test")]
        [DataRow(false, "TestAttribute", "test", "other")]
        [DataRow(false, "TestAttribute", "other", "other")]
        [DataRow(false, "TestAttribute", "other1", "other2", "Test")]
        [DataRow(true, "TestAttribute", "other1", "other2", "other3", "TestAttribute")]
        [DataRow(false, "TestAttribute", "Test", "test", "SomeAttribute")]
        public void HasAnyName(bool expected, string attributeClassName, params string[] testNames) =>
            AttributeDataWithName(attributeClassName).HasAnyName(testNames).Should().Be(expected);

        [TestMethod]
        public void HasAnyNameThrowsForNull() =>
            new Action(() => AttributeDataWithName("TestAttribute").HasAnyName(null)).Should().Throw<Exception>();

        [DataTestMethod]
        [DataRow("SomeBool", typeof(bool), true, true)]
        [DataRow("someBool", typeof(bool), true, true)]
        [DataRow("somebool", typeof(bool), true, true)]
        [DataRow("SOMEBOOL", typeof(bool), true, true)]
        [DataRow("SomeInt", typeof(int), true, 1_234_567)]
        [DataRow("SomeInt", typeof(byte), false, (byte)0)]
        [DataRow("SomeByte", typeof(byte), true, (byte)24)]
        [DataRow("SomeByte", typeof(int), true, 24)]
        [DataRow("SomeString", typeof(string), true, "Text")]
        [DataRow("SomeNull", typeof(string), true, null)]
        [DataRow("Missing", typeof(string), false, null)]
        [DataRow("SomeString", typeof(int), false, 0)]
        [DataRow("SomeNumberString", typeof(int), true, 42)]
        public void TryGetAttributeValue_Arguments(string valueName, Type valueType, bool expectedSucess, object expectedResult)
        {
            var arguments = new Dictionary<string, object>
            {
                { "SomeBool", true },
                { "SomeInt", 1_234_567 },
                { "SomeByte", (byte)24 },
                { "SomeString", "Text" },
                { "SomeNumberString", "42" },
                { "SomeNull", null },
            };
            TryGetAttributeValue_Arguments(valueName, valueType, expectedSucess, expectedResult, namedArguments: arguments);       // [Attr(SomeBool = true)]
            TryGetAttributeValue_Arguments(valueName, valueType, expectedSucess, expectedResult, constructorArguments: arguments); // [Attr(SomeBool: true)]

            static void TryGetAttributeValue_Arguments(string valueName,
                                                               Type valueType,
                                                               bool expectedSucess,
                                                               object expectedResult,
                                                               IDictionary<string, object> namedArguments = null,
                                                               IDictionary<string, object> constructorArguments = null)
            {
                var attributeData = AttributeDataWithArguments(namedArguments, constructorArguments);
                var tryGetAttributeValue = typeof(AttributeDataExtensions).GetMethod(nameof(AttributeDataExtensions.TryGetAttributeValue)).MakeGenericMethod(valueType);
                var arguments = new object[] { attributeData, valueName, null };
                var actualSuccess = tryGetAttributeValue.Invoke(null, arguments); // actualSuccess = attributeData.TryGetAttributeValue<valueType>(valueName, out valueType actualResult)
                var actualResult = arguments[2]; // the out parameter value
                actualSuccess.Should().Be(expectedSucess);
                if (expectedResult == null)
                {
                    actualResult.Should().BeNull();
                }
                else
                {
                    actualResult.Should().BeOfType(expectedResult.GetType());
                    actualResult.Should().Be(expectedResult);
                }
            }
        }

        [TestMethod]
        public void TryGetAttributeValue_ConstructorArgumentAndNamedArgumentNamedTheSame()
        {
            var attributeData = AttributeDataWithArguments(namedArguments: new Dictionary<string, object>
            {
                { "Result", true },
            }, constructorArguments: new Dictionary<string, object>
            {
                { "Result", false },
            });
            var actualSuccess = attributeData.TryGetAttributeValue("Result", out bool actualValue);
            actualSuccess.Should().BeTrue();
            actualValue.Should().BeTrue(); // Named argument takes precedence
        }

        [TestMethod]
        public void TryGetAttributeValue_DateTimeConversion()
        {
            var attributeData = AttributeDataWithArguments(namedArguments: new Dictionary<string, object> { { "Result", "2022-12-24" } });
            var actualSuccess = attributeData.TryGetAttributeValue("Result", out DateTime actualValue);
            actualSuccess.Should().BeTrue();
            actualValue.Should().Be(new DateTime(2022, 12, 24));
        }

        private static AttributeData AttributeDataWithName(string attributeClassName)
        {
            var namedType = new Mock<INamedTypeSymbol>();
            namedType.Setup(x => x.Name).Returns(attributeClassName);
            var attributeData = new Mock<AttributeData>();
            attributeData.Protected().Setup<INamedTypeSymbol>("CommonAttributeClass").Returns(namedType.Object);
            return attributeData.Object;
        }

        private static AttributeData AttributeDataWithArguments(IDictionary<string, object> namedArguments = null, IDictionary<string, object> constructorArguments = null)
        {
            var typedConstConstructor = typeof(TypedConstant).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Single(x => x.GetParameters().Length == 3);
            var namedArgumentsFake = namedArguments?.Select(x => new KeyValuePair<string, TypedConstant>(x.Key, CreateTypedConstant(x.Value))).ToImmutableArray()
                ?? ImmutableArray.Create<KeyValuePair<string, TypedConstant>>();
            var constructorArgumentsFake = constructorArguments?.Select(x => CreateTypedConstant(x.Value)).ToImmutableArray() ?? ImmutableArray.Create<TypedConstant>();
            var constructorParamatersFake = constructorArguments?.Select(x =>
            {
                var parameterMock = new Mock<IParameterSymbol>();
                parameterMock.Setup(x => x.Name).Returns(x.Key);
                return parameterMock.Object;
            }).ToImmutableArray() ?? ImmutableArray.Create<IParameterSymbol>();
            var constructorMock = new Mock<IMethodSymbol>();
            constructorMock.Setup(x => x.Parameters).Returns(constructorParamatersFake);
            var attributeDataMock = new Mock<AttributeData>();
            attributeDataMock.Protected().Setup<ImmutableArray<KeyValuePair<string, TypedConstant>>>("CommonNamedArguments").Returns(namedArgumentsFake);
            attributeDataMock.Protected().Setup<ImmutableArray<TypedConstant>>("CommonConstructorArguments").Returns(constructorArgumentsFake);
            attributeDataMock.Protected().Setup<IMethodSymbol>("CommonAttributeConstructor").Returns(constructorMock.Object);

            return attributeDataMock.Object;

            TypedConstant CreateTypedConstant(object value) =>
                (TypedConstant)typedConstConstructor.Invoke(new object[] { null, TypedConstantKind.Primitive, value });
        }
    }
}
