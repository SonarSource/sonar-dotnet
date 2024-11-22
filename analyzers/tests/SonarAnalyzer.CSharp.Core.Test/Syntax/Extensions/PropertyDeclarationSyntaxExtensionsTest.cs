/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.CSharp.Core.Test.Syntax.Extensions
{
    [TestClass]
    public class PropertyDeclarationSyntaxExtensionsTest
    {
        [TestMethod]
        public void IsAutoProperty_AccessorWithBody_ReturnsFalse()
        {
            const string code = @"class TestCases
{
    public int Property { get { return 0; } }
}
";
            GetPropertyDeclaration(code).IsAutoProperty().Should().BeFalse();
        }

        [TestMethod]
        public void IsAutoProperty_AccessorWithExpressionBody_ReturnsFalse()
        {
            const string code = @"record TestCases
{
    public int Property
    {
        get => 0;
    }
}
";
            GetPropertyDeclaration(code).IsAutoProperty().Should().BeFalse();
        }

        [TestMethod]
        public void IsAutoProperty_ExpressionBody_ReturnsFalse()
        {
            const string code = @"record TestCases
{
    public int Property => 0;
}
";
            GetPropertyDeclaration(code).IsAutoProperty().Should().BeFalse();
        }

        [TestMethod]
        public void IsAutoProperty_AccessorsWithoutBody_ReturnsTrue()
        {
            const string code = @"class TestCases
{
    public int Property { get; set; }
}
";
            GetPropertyDeclaration(code).IsAutoProperty().Should().BeTrue();
        }

        private static PropertyDeclarationSyntax GetPropertyDeclaration(string code) =>
            SyntaxFactory.ParseSyntaxTree(code)
                         .GetRoot()
                         .DescendantNodes()
                         .OfType<PropertyDeclarationSyntax>()
                         .First();
    }
}
