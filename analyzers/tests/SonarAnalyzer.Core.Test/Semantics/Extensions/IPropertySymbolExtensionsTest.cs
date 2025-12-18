/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.Core.Test.Semantics.Extensions;

[TestClass]
public class IPropertySymbolExtensionsTest
{
    [TestMethod]
    public void IsExtension_ExtensionProperty_ReturnsTrue() =>
        new SnippetCompiler("""
            public static class Extensions
            {
                extension(string s)
                {
                    public int Property { get => 42; set { } }
                    public int GetterOnly => 42;
                    public int SetterOnly { set { } }
                }
                extension(string)
                {
                    public static int StaticProperty { get => 42; set { } }
                    public static int StaticGetterOnly => 42;
                    public static int StaticSetterOnly { set { } }
                }
            }
            """).DeclaredSymbols<IPropertySymbol>().Should().AllSatisfy(x => x.IsExtension.Should().BeTrue());

    [TestMethod]
    public void IsExtension_RegularProperty_ReturnsFalse() =>
        new SnippetCompiler("""
            public class Sample
            {
                public int Property { get; set; }
                public int GetterOnly => 42;
                public int SetterOnly { set { } }
                public static int StaticProperty { get; set; }
                public static int StaticGetterOnly => 42;
                public static int StaticSetterOnly { set { } }
            }
            """).DeclaredSymbols<IPropertySymbol>().Should().AllSatisfy(x => x.IsExtension.Should().BeFalse());

    [TestMethod]
    public void IsAnyAttributeInOverridingChain_WhenPropertySymbolIsNull_ReturnsFalse() =>
        IPropertySymbolExtensions.IsAnyAttributeInOverridingChain(null).Should().BeFalse();
}
