/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.UnitTest.Extensions
{
    [TestClass]
    public class PropertyDeclarationSyntaxExtensionsTests
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
