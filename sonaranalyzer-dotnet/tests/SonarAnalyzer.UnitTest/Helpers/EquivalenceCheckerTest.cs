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
using Microsoft.CodeAnalysis;
using CSharp = Microsoft.CodeAnalysis.CSharp.Syntax;
using VisualBasic = Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers.CSharp;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class EquivalenceCheckerTest
    {
        private const string Source_CS = @"
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

        public void Method4()
        {
            var x = Property-2;
            Console.Write(x);
        }
    }
}";

        private const string Source_VB = @"
Namespace Test

    Class TestClass

        Public Property Value As Integer

        Public Sub Method1()
            If True Then
                Dim x As Integer = Value
                Console.WriteLine(x)
            End If
        End Sub

        Public Sub Method2()
            If True Then
                Dim x As Integer = Value
                Console.WriteLine(x)
            End If
        End Sub

        Public Sub Method3()
            If True Then
                Dim x As Integer = Value + 2
                Console.Write(x)
            End If
        End Sub

        Public Sub Method4()
            If True Then
                Dim x As Integer = Value - 2
                Console.Write(x)
            End If
        End Sub

    End Class

End Namespace";

        private List<CSharp.MethodDeclarationSyntax> methods_CS;
        private List<VisualBasic.MethodBlockSyntax> methods_VB;

        [TestInitialize]
        public void TestSetup()
        {
            var syntaxTree_CS = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(Source_CS);
            this.methods_CS = syntaxTree_CS.GetRoot().DescendantNodes().OfType<CSharp.MethodDeclarationSyntax>().ToList();

            var syntaxTree_VB = Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(Source_VB);
            this.methods_VB = syntaxTree_VB.GetRoot().DescendantNodes().OfType<VisualBasic.MethodBlockSyntax>().ToList();
        }

        [TestMethod]
        public void AreEquivalent_Node_CS()
        {
            var result = CSharpEquivalenceChecker.AreEquivalent(
                this.methods_CS.First(m => m.Identifier.ValueText == "Method1").Body,
                this.methods_CS.First(m => m.Identifier.ValueText == "Method2").Body);
            result.Should().BeTrue();

            result = CSharpEquivalenceChecker.AreEquivalent(
                this.methods_CS.First(m => m.Identifier.ValueText == "Method1").Body,
                this.methods_CS.First(m => m.Identifier.ValueText == "Method3").Body);
            result.Should().BeFalse();
        }

        [TestMethod]
        public void AreEquivalent_List_CS()
        {
            var result = CSharpEquivalenceChecker.AreEquivalent(
                this.methods_CS.First(m => m.Identifier.ValueText == "Method1").Body.Statements,
                this.methods_CS.First(m => m.Identifier.ValueText == "Method2").Body.Statements);
            result.Should().BeTrue();

            result = CSharpEquivalenceChecker.AreEquivalent(
                this.methods_CS.First(m => m.Identifier.ValueText == "Method1").Body.Statements,
                this.methods_CS.First(m => m.Identifier.ValueText == "Method3").Body.Statements);
            result.Should().BeFalse();
        }
        
        [TestMethod]
        public void EqualityComparer_Node_CS()
        {
            var comparer = new CSharpSyntaxNodeEqualityComparer<CSharp.BlockSyntax>();
            var method1 = this.methods_CS.First(m => m.Identifier.ValueText == "Method1").Body;
            var method2 = this.methods_CS.First(m => m.Identifier.ValueText == "Method2").Body;
            var method3 = this.methods_CS.First(m => m.Identifier.ValueText == "Method3").Body;
            var method4 = this.methods_CS.First(m => m.Identifier.ValueText == "Method4").Body;

            var result = comparer.Equals(method1, method2);
            result.Should().BeTrue();

            result = comparer.Equals(method1, method3);
            result.Should().BeFalse();

            var hashSet = new HashSet<CSharp.BlockSyntax>(new[] { method1, method2, method3 }, comparer);
            hashSet.Count.Should().Be(2);
            hashSet.Contains(method1).Should().BeTrue();
            hashSet.Contains(method4).Should().BeFalse();
        }

        [TestMethod]
        public void EqualityComparer_List_CS()
        {
            var comparer = new CSharpSyntaxNodeEqualityComparer<CSharp.StatementSyntax>();
            var method1 = this.methods_CS.First(m => m.Identifier.ValueText == "Method1").Body.Statements;
            var method2 = this.methods_CS.First(m => m.Identifier.ValueText == "Method2").Body.Statements;
            var method3 = this.methods_CS.First(m => m.Identifier.ValueText == "Method3").Body.Statements;
            var method4 = this.methods_CS.First(m => m.Identifier.ValueText == "Method4").Body.Statements;

            var result = comparer.Equals(method1, method2);
            result.Should().BeTrue();

            result = comparer.Equals(method1, method3);
            result.Should().BeFalse();

            var hashSet = new HashSet<SyntaxList<CSharp.StatementSyntax>>(new[] { method1, method2, method3 }, comparer);
            hashSet.Count.Should().Be(2);
            hashSet.Contains(method1).Should().BeTrue();
            hashSet.Contains(method4).Should().BeFalse();
        }

        [TestMethod]
        public void AreEquivalent_Node_VB()
        {
            var result = VisualBasicEquivalenceChecker.AreEquivalent(
                this.methods_VB.First(m => m.GetIdentifierText() == "Method1").Statements.First(),
                this.methods_VB.First(m => m.GetIdentifierText() == "Method2").Statements.First());
            result.Should().BeTrue();

            result = VisualBasicEquivalenceChecker.AreEquivalent(
                this.methods_VB.First(m => m.GetIdentifierText() == "Method1").Statements.First(),
                this.methods_VB.First(m => m.GetIdentifierText() == "Method3").Statements.First());
            result.Should().BeFalse();
        }

        [TestMethod]
        public void AreEquivalent_List_VB()
        {
            var result = VisualBasicEquivalenceChecker.AreEquivalent(
                this.methods_VB.First(m => m.GetIdentifierText() == "Method1").Statements,
                this.methods_VB.First(m => m.GetIdentifierText() == "Method2").Statements);
            result.Should().BeTrue();

            result = VisualBasicEquivalenceChecker.AreEquivalent(
                this.methods_VB.First(m => m.GetIdentifierText() == "Method1").Statements,
                this.methods_VB.First(m => m.GetIdentifierText() == "Method3").Statements);
            result.Should().BeFalse();
        }

        [TestMethod]
        public void EqualityComparer_Node_VB()
        {
            var comparer = new VisualBasicSyntaxNodeEqualityComparer<VisualBasic.StatementSyntax>();
            var method1 = this.methods_VB.First(m => m.GetIdentifierText() == "Method1").Statements.First();
            var method2 = this.methods_VB.First(m => m.GetIdentifierText() == "Method2").Statements.First();
            var method3 = this.methods_VB.First(m => m.GetIdentifierText() == "Method3").Statements.First();
            var method4 = this.methods_VB.First(m => m.GetIdentifierText() == "Method4").Statements.First();

            var result = comparer.Equals(method1, method2);
            result.Should().BeTrue();

            result = comparer.Equals(method1, method3);
            result.Should().BeFalse();

            var hashSet = new HashSet<VisualBasic.StatementSyntax>(new[] { method1, method2, method3 }, comparer);
            hashSet.Count.Should().Be(2);
            hashSet.Contains(method1).Should().BeTrue();
            hashSet.Contains(method4).Should().BeFalse();
        }

        [TestMethod]
        public void EqualityComparer_List_VB()
        {
            var comparer = new VisualBasicSyntaxNodeEqualityComparer<VisualBasic.StatementSyntax>();
            var method1 = this.methods_VB.First(m => m.GetIdentifierText() == "Method1").Statements;
            var method2 = this.methods_VB.First(m => m.GetIdentifierText() == "Method2").Statements;
            var method3 = this.methods_VB.First(m => m.GetIdentifierText() == "Method3").Statements;
            var method4 = this.methods_VB.First(m => m.GetIdentifierText() == "Method4").Statements;

            var result = comparer.Equals(method1, method2);
            result.Should().BeTrue();

            result = comparer.Equals(method1, method3);
            result.Should().BeFalse();
            
            var hashSet = new HashSet<SyntaxList<VisualBasic.StatementSyntax>>(new[] { method1, method2, method3 }, comparer);
            hashSet.Count.Should().Be(2);
            hashSet.Contains(method1).Should().BeTrue();
            hashSet.Contains(method4).Should().BeFalse();
        }

    }
}
