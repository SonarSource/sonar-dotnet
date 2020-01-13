/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class TypeDeclarationSyntaxExtensionsTest
    {
        [TestMethod]
        public void GetMethodDeclarations_EmptyClass_ReturnsEmpty()
        {
            var typeDeclaration = SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier("TestClass"));

            typeDeclaration.GetMethodDeclarations().Should().BeEmpty();
        }

        [TestMethod]
        public void GetMethodDeclarations_SingleMethod_ReturnsMethod()
        {
            const string code = @"
namespace Test
{
    class TestClass
    {
        private void WriteLine() {}
    }
}
";
            var snippet = new SnippetCompiler(code);

            var typeDeclaration = snippet.SyntaxTree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>().Single();

            typeDeclaration.GetMethodDeclarations().Single().Identifier.Text.Should().Be("WriteLine");
        }

        [TestMethod]
        public void GetMethodDeclarations_MultipleMethodsWithLocalFunctions_ReturnsMethodsAndFunctions()
        {
            const string code = @"
namespace Test
{
    class TestClass
    {
        private void Method1()
        {
            Function1();
            Function2();
            void Function1() {}
            void Function2() {}
        }

        private void Method2()
        {
            Function3();
            void Function3() {}
        }
    }
}
";
            var snippet = new SnippetCompiler(code, NuGetMetadataReference.NETStandardV2_1_0);

            var typeDeclaration = snippet.SyntaxTree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>().Single();

            typeDeclaration
                .GetMethodDeclarations()
                .Select(methodDeclaration => methodDeclaration.Identifier.Text)
                .Should()
                .BeEquivalentTo(new List<string>
                {
                    "Method1",
                    "Method2",
                    "Function1",
                    "Function2",
                    "Function3"
                });
        }
    }
}
