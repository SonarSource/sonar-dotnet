/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using NSubstitute;
using SonarAnalyzer.Core.Syntax.Extensions;

namespace SonarAnalyzer.CSharp.Core.Test.Syntax.Extensions;

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

    [TestMethod]
    public void TryGetAttributeValue_Arguments()
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
        var named = AttributeDataWithArguments(namedArguments: arguments);
        var constructor = AttributeDataWithArguments(constructorArguments: arguments);
        AssertTryGetAttributeValue<bool>("SomeBool", true, true);
        AssertTryGetAttributeValue<bool>("someBool", true, true);
        AssertTryGetAttributeValue<bool>("somebool", true, true);
        AssertTryGetAttributeValue<bool>("SOMEBOOL", true, true);
        AssertTryGetAttributeValue<int>("SomeInt", true, 1_234_567);
        AssertTryGetAttributeValue<byte>("SomeInt", false, 0); // SomeInt is too big
        AssertTryGetAttributeValue<byte>("SomeByte", true, 24);
        AssertTryGetAttributeValue<int>("SomeByte", true, 24);
        AssertTryGetAttributeValue<string>("SomeString", true, "Text");
        AssertTryGetAttributeValue<object>("SomeNull", true, null);
        AssertTryGetAttributeValue<string>("SomeNull", true, null);
        AssertTryGetAttributeValue<int>("SomeNull", true, 0);
        AssertTryGetAttributeValue<object>("Missing", false, null);
        AssertTryGetAttributeValue<string>("Missing", false, null);
        AssertTryGetAttributeValue<int>("Missing", false, 0);
        AssertTryGetAttributeValue<int>("SomeString", false, 0);
        AssertTryGetAttributeValue<int>("SomeNumberString", true, 42);

        void AssertTryGetAttributeValue<T>(string valueName, bool expectedSuccess, T expectedResult)
        {
            var success = named.TryGetAttributeValue<T>(valueName, out T result);
            success.Should().Be(expectedSuccess);
            result.Should().Be(expectedResult);

            success = constructor.TryGetAttributeValue<T>(valueName, out result);
            success.Should().Be(expectedSuccess);
            result.Should().Be(expectedResult);
        }
    }

    [TestMethod]
    public void TryGetAttributeValue_ConstructorArgumentAndNamedArgumentNamedTheSame()
    {
        var attributeData = AttributeDataWithArguments(namedArguments: new() { { "Result", true } }, constructorArguments: new() { { "Result", false } });
        var actualSuccess = attributeData.TryGetAttributeValue("Result", out bool actualValue);
        actualSuccess.Should().BeTrue();
        actualValue.Should().BeTrue(); // Named argument takes precedence
    }

    [TestMethod]
    public void TryGetAttributeValue_DateTimeConversion()
    {
        var attributeData = AttributeDataWithArguments(namedArguments: new() { { "Result", "2022-12-24" } });
        var actualSuccess = attributeData.TryGetAttributeValue("Result", out DateTime actualValue);
        actualSuccess.Should().BeTrue();
        actualValue.Should().Be(new DateTime(2022, 12, 24));
    }

    [DataTestMethod]
    [DataRow("SomeText", typeof(string))]
    [DataRow(42, typeof(int))]
    [DataRow(null, null)]
    public void TryGetAttributeValue_ObjectConversion(object value, Type expectedType)
    {
        var attributeData = AttributeDataWithArguments(namedArguments: new() { { "Result", value } });
        var actualSuccess = attributeData.TryGetAttributeValue("Result", out object actualValue);
        actualSuccess.Should().BeTrue();
        if (expectedType != null)
        {
            actualValue.Should().BeOfType(expectedType);
        }
        actualValue.Should().Be(value);
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void HasAttributeUsageInherited_InheritedSpecified(bool inherited)
    {
        var code = $$"""
            using System;

            [AttributeUsage(AttributeTargets.All, Inherited = {{inherited.ToString().ToLower()}})]
            public class MyAttribute: Attribute { }

            [My]
            public class Program { }
            """;
        CompileAttribute(code).HasAttributeUsageInherited().Should().Be(inherited);
    }

    [TestMethod]
    public void HasAttributeUsageInherited_InheritedUnSpecified()
    {
        const string code = """
            using System;

            [AttributeUsage(AttributeTargets.All)]
            public class MyAttribute: Attribute { }

            [My]
            public class Program { }
            """;
        CompileAttribute(code).HasAttributeUsageInherited().Should().Be(true); // The default for Inherited = true
    }

    [TestMethod]
    public void HasAttributeUsageInherited_NoUsageAttribute()
    {
        const string code = """
            using System;

            public class MyAttribute: Attribute { }

            [My]
            public class Program { }
            """;
        CompileAttribute(code).HasAttributeUsageInherited().Should().Be(true); // The default for Inherited = true
    }

    [DataTestMethod]
    [DataRow(true, true)]
    [DataRow(false, true)] // The "Inherited" flag is not inherited for the AttributeUsage attribute itself. See also the SymbolHelperTest.GetAttributesWithInherited... tests,
                           // where the reflection behavior of MemberInfo.GetCustomAttributes is also tested.
    public void HasAttributeUsageInherited_UsageInherited(bool inherited, bool expected)
    {
        var code = $$"""
            using System;

            [AttributeUsage(AttributeTargets.All, Inherited = {{inherited.ToString().ToLower()}})]
            public class BaseAttribute: Attribute { }

            public class MyAttribute: BaseAttribute { }

            [My]
            public class Program { }
            """;
        CompileAttribute(code).HasAttributeUsageInherited().Should().Be(expected);
    }

    [TestMethod]
    public void HasAttributeUsageInherited_DuplicateAttributeUsage()
    {
        const string code = """
            using System;

            [AttributeUsage(AttributeTargets.All, Inherited = true)]
            [AttributeUsage(AttributeTargets.All, Inherited = false)] // Compiler error
            public class MyAttribute: Attribute { }

            [My]
            public class Program { }
            """;
        CompileAttribute(code, ignoreErrors: true).HasAttributeUsageInherited().Should().BeTrue();
    }

    private static AttributeData CompileAttribute(string code, bool ignoreErrors = false) =>
        new SnippetCompiler(code, ignoreErrors, AnalyzerLanguage.CSharp).GetTypeSymbol("Program").GetAttributes().Single(x => x.HasName("MyAttribute"));

    private static AttributeDataMock AttributeDataWithName(string attributeClassName)
    {
        var namedType = Substitute.For<INamedTypeSymbol>();
        namedType.Name.Returns(attributeClassName);
        return new AttributeDataMock(namedType);
    }

    private static AttributeData AttributeDataWithArguments(Dictionary<string, object> namedArguments = null, Dictionary<string, object> constructorArguments = null)
    {
        namedArguments ??= new();
        constructorArguments ??= new();
        var separator = constructorArguments.Any() && namedArguments.Any() ? ", " : string.Empty;
        var code = $$"""
            using System;

            public class MyAttribute: Attribute
            {
                public MyAttribute({{constructorArguments.Select(x => $"{TypeName(x.Value)} {x.Key}").JoinStr(", ")}})
                {
                }

                {{namedArguments.Select(x => $@"public {TypeName(x.Value)} {x.Key} {{ get; set; }}").JoinStr("\r\n")}}
            }

            [My({{constructorArguments.Select(x => Quote(x.Value)).JoinStr(", ")}}{{separator}}{{namedArguments.Select(x => $"{x.Key}={Quote(x.Value)}").JoinStr(", ")}})]
            public class Dummy { }
            """;
        var snippet = new SnippetCompiler(code);
        var classDeclaration = snippet.SyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Last();
        var symbol = snippet.SemanticModel.GetDeclaredSymbol(classDeclaration);
        return symbol.GetAttributes().First();

        static string TypeName(object value) =>
            value == null ? "object" : value.GetType().FullName;

        static string Quote(object value) =>
            value switch
            {
                string s => @$"""{s}""",
                bool b => b ? "true" : "false",
                null => "null",
                var v => v.ToString(),
            };
    }

    private class AttributeDataMock : AttributeData
    {
        protected override INamedTypeSymbol CommonAttributeClass { get; }
        protected override IMethodSymbol CommonAttributeConstructor => throw new NotSupportedException();
        protected override SyntaxReference CommonApplicationSyntaxReference => throw new NotSupportedException();
        protected override ImmutableArray<TypedConstant> CommonConstructorArguments => throw new NotSupportedException();
        protected override ImmutableArray<KeyValuePair<string, TypedConstant>> CommonNamedArguments => throw new NotSupportedException();

        public AttributeDataMock(INamedTypeSymbol commonAttributeClass) =>
            CommonAttributeClass = commonAttributeClass;
    }
}
