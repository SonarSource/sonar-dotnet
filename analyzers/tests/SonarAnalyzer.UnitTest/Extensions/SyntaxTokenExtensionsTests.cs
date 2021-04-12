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

extern alias csharp;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static csharp::SonarAnalyzer.Extensions.SyntaxTokenExtensions;

namespace SonarAnalyzer.UnitTest.Extensions
{
    [TestClass]
    public class SyntaxTokenExtensionsTests
    {
        [TestMethod]
        public void GetBindableParent_ForEmptyToken_ReturnsNull()
        {
            SyntaxToken empty = default;

            empty.GetBindableParent().Should().BeNull();
        }

        [TestMethod]
        public void GetBindableParent_ForInterpolatedStringTextToken_ReturnsInterpolatedStringExpression()
        {
            const string code = @"
using System;

namespace TestCases
{
    class Foo
    {
        string x = $""a"";
    }
}";
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var aToken = syntaxTree.GetRoot().DescendantTokens().First(token => token.IsKind(SyntaxKind.InterpolatedStringTextToken));

            var parent = aToken.GetBindableParent();
            parent.Kind().Should().Be(SyntaxKind.InterpolatedStringExpression);
        }

        [TestMethod]
        public void GetBindableParent_ForMemberAccessExpressionSyntax_ReturnsTheExpression()
        {
            const string code = @"
namespace TestCases
{
    class Foo
    {
        public int Value {get; set;}

        void M()
        {
            _ = this.Value;
        }
    }
}";
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var thisToken = syntaxTree.GetRoot().DescendantTokens().First(token => token.IsKind(SyntaxKind.ThisKeyword));

            var parent = thisToken.GetBindableParent();
            parent.Kind().Should().Be(SyntaxKind.ThisExpression);
        }
    }
}
