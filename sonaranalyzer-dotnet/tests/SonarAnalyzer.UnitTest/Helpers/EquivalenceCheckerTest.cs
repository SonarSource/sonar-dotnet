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

        private CSharpMethods methods_CS;
        private VisualBasicMethods methods_VB;

        [TestInitialize]
        public void TestSetup()
        {
            methods_CS = new CSharpMethods(Source_CS);
            methods_VB = new VisualBasicMethods(Source_VB);
        }

        [TestMethod]
        public void AreEquivalent_Node_CS()
        {
            var result = CSharpEquivalenceChecker.AreEquivalent(methods_CS.Method1, methods_CS.Method2);
            result.Should().BeTrue();

            result = CSharpEquivalenceChecker.AreEquivalent(methods_CS.Method1, methods_CS.Method3);
            result.Should().BeFalse();
        }

        [TestMethod]
        public void AreEquivalent_List_CS()
        {
            var result = CSharpEquivalenceChecker.AreEquivalent(methods_CS.Method1.Statements, methods_CS.Method2.Statements);
            result.Should().BeTrue();

            result = CSharpEquivalenceChecker.AreEquivalent(methods_CS.Method1.Statements, methods_CS.Method3.Statements);
            result.Should().BeFalse();
        }

        [TestMethod]
        public void EqualityComparer_Node_CS()
        {
            var comparer = new CSharpSyntaxNodeEqualityComparer<CSharp.BlockSyntax>();

            var result = comparer.Equals(methods_CS.Method1, methods_CS.Method2);
            result.Should().BeTrue();

            result = comparer.Equals(methods_CS.Method1, methods_CS.Method3);
            result.Should().BeFalse();

            var hashSet = new HashSet<CSharp.BlockSyntax>(new[] { methods_CS.Method1, methods_CS.Method2, methods_CS.Method3 }, comparer);
            hashSet.Count.Should().Be(2);
            hashSet.Contains(methods_CS.Method1).Should().BeTrue();
            hashSet.Contains(methods_CS.Method4).Should().BeFalse();
        }

        [TestMethod]
        public void EqualityComparer_List_CS()
        {
            var comparer = new CSharpSyntaxNodeEqualityComparer<CSharp.StatementSyntax>();

            var result = comparer.Equals(methods_CS.Method1.Statements, methods_CS.Method2.Statements);
            result.Should().BeTrue();

            result = comparer.Equals(methods_CS.Method1.Statements, methods_CS.Method3.Statements);
            result.Should().BeFalse();

            var hashSet = new HashSet<SyntaxList<CSharp.StatementSyntax>>(new[] { methods_CS.Method1.Statements, methods_CS.Method2.Statements, methods_CS.Method3.Statements }, comparer);
            hashSet.Count.Should().Be(2);
            hashSet.Contains(methods_CS.Method1.Statements).Should().BeTrue();
            hashSet.Contains(methods_CS.Method4.Statements).Should().BeFalse();
        }

        [TestMethod]
        public void AreEquivalent_Node_VB()
        {
            var result = VisualBasicEquivalenceChecker.AreEquivalent(methods_VB.Method1.First(), methods_VB.Method2.First());
            result.Should().BeTrue();

            result = VisualBasicEquivalenceChecker.AreEquivalent(methods_VB.Method1.First(), methods_VB.Method3.First());
            result.Should().BeFalse();
        }

        [TestMethod]
        public void AreEquivalent_List_VB()
        {
            var result = VisualBasicEquivalenceChecker.AreEquivalent(methods_VB.Method1, methods_VB.Method2);
            result.Should().BeTrue();

            result = VisualBasicEquivalenceChecker.AreEquivalent(methods_VB.Method1, methods_VB.Method3);
            result.Should().BeFalse();
        }

        [TestMethod]
        public void EqualityComparer_Node_VB()
        {
            var comparer = new VisualBasicSyntaxNodeEqualityComparer<VisualBasic.StatementSyntax>();

            var result = comparer.Equals(methods_VB.Method1.First(), methods_VB.Method2.First());
            result.Should().BeTrue();

            result = comparer.Equals(methods_VB.Method1.First(), methods_VB.Method3.First());
            result.Should().BeFalse();

            var hashSet = new HashSet<VisualBasic.StatementSyntax>(new[] { methods_VB.Method1.First(), methods_VB.Method2.First(), methods_VB.Method3.First() }, comparer);
            hashSet.Count.Should().Be(2);
            hashSet.Contains(methods_VB.Method1.First()).Should().BeTrue();
            hashSet.Contains(methods_VB.Method4.First()).Should().BeFalse();
        }

        [TestMethod]
        public void EqualityComparer_List_VB()
        {
            var comparer = new VisualBasicSyntaxNodeEqualityComparer<VisualBasic.StatementSyntax>();
            
            var result = comparer.Equals(methods_VB.Method1, methods_VB.Method2);
            result.Should().BeTrue();

            result = comparer.Equals(methods_VB.Method1, methods_VB.Method3);
            result.Should().BeFalse();

            var hashSet = new HashSet<SyntaxList<VisualBasic.StatementSyntax>>(new[] { methods_VB.Method1, methods_VB.Method2, methods_VB.Method3 }, comparer);
            hashSet.Count.Should().Be(2);
            hashSet.Contains(methods_VB.Method1).Should().BeTrue();
            hashSet.Contains(methods_VB.Method4).Should().BeFalse();
        }

        private class CSharpMethods
        {
            public readonly CSharp.BlockSyntax Method1, Method2, Method3, Method4;

            public CSharpMethods(string source)
            {
                var methods = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().OfType<CSharp.MethodDeclarationSyntax>().ToArray();
                Method1 = methods.Single(m => m.Identifier.ValueText == "Method1").Body;
                Method2 = methods.Single(m => m.Identifier.ValueText == "Method2").Body;
                Method3 = methods.Single(m => m.Identifier.ValueText == "Method3").Body;
                Method4 = methods.Single(m => m.Identifier.ValueText == "Method4").Body;
            }
        }

        private class VisualBasicMethods
        {
            public readonly SyntaxList<VisualBasic.StatementSyntax> Method1, Method2, Method3, Method4;

            public VisualBasicMethods(string source)
            {
                var methods = Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(source).GetRoot().DescendantNodes().OfType<VisualBasic.MethodBlockSyntax>().ToArray();
                Method1 = methods.Single(m => m.GetIdentifierText() == "Method1").Statements;
                Method2 = methods.Single(m => m.GetIdentifierText() == "Method2").Statements;
                Method3 = methods.Single(m => m.GetIdentifierText() == "Method3").Statements;
                Method4 = methods.Single(m => m.GetIdentifierText() == "Method4").Statements;
            }
        }

    }
}
