/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class EquivalenceCheckerTest
    {
        private const string Source = @"
namespace Test
{
    class TestClass
    {
        int Property {get;set;}
        public void Method1()
        {
            var x = Property;
            Console.WriteLine(x);
        }

        public void Method2()
        {
            var x = Property;
            Console.WriteLine(x);
        }

        public void Method3()
        {
            var x = Property+2;
            Console.Write(x);
        }
    }
}";

        private List<MethodDeclarationSyntax> methods;

        [TestInitialize]
        public void TestSetup()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(Source);

            this.methods = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
        }

        [TestMethod]
        public void AreEquivalent_Node()
        {
            var result = CSharpEquivalenceChecker.AreEquivalent(
                this.methods.First(m => m.Identifier.ValueText == "Method1").Body,
                this.methods.First(m => m.Identifier.ValueText == "Method2").Body);
            result.Should().BeTrue();

            result = CSharpEquivalenceChecker.AreEquivalent(
                this.methods.First(m => m.Identifier.ValueText == "Method1").Body,
                this.methods.First(m => m.Identifier.ValueText == "Method3").Body);
            result.Should().BeFalse();
        }

        [TestMethod]
        public void AreEquivalent_List()
        {
            var result = CSharpEquivalenceChecker.AreEquivalent(
                this.methods.First(m => m.Identifier.ValueText == "Method1").Body.Statements,
                this.methods.First(m => m.Identifier.ValueText == "Method2").Body.Statements);
            result.Should().BeTrue();

            result = CSharpEquivalenceChecker.AreEquivalent(
                this.methods.First(m => m.Identifier.ValueText == "Method1").Body.Statements,
                this.methods.First(m => m.Identifier.ValueText == "Method3").Body.Statements);
            result.Should().BeFalse();
        }
    }
}
