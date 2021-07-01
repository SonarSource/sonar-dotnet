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

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.Common;
using CS = Microsoft.CodeAnalysis.CSharp.Syntax;
using VB = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class EquivalenceCheckerTest
    {
        private const string CsSource = @"
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

        private const string VbSource = @"
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

        private CSharpMethods csMethods;
        private VisualBasicMethods vbMethods;

        [TestInitialize]
        public void TestSetup()
        {
            csMethods = new CSharpMethods(CsSource);
            vbMethods = new VisualBasicMethods(VbSource);
        }

        [TestMethod]
        public void AreEquivalent_Node_CS()
        {
            var result = CSharpEquivalenceChecker.AreEquivalent(csMethods.Method1, csMethods.Method2);
            result.Should().BeTrue();

            result = CSharpEquivalenceChecker.AreEquivalent(csMethods.Method1, csMethods.Method3);
            result.Should().BeFalse();
        }

        [TestMethod]
        public void AreEquivalent_List_CS()
        {
            var result = CSharpEquivalenceChecker.AreEquivalent(csMethods.Method1.Statements, csMethods.Method2.Statements);
            result.Should().BeTrue();

            result = CSharpEquivalenceChecker.AreEquivalent(csMethods.Method1.Statements, csMethods.Method3.Statements);
            result.Should().BeFalse();
        }

        [TestMethod]
        public void EqualityComparer_Node_CS()
        {
            var comparer = new CSharpSyntaxNodeEqualityComparer<CS.BlockSyntax>();

            var result = comparer.Equals(csMethods.Method1, csMethods.Method2);
            result.Should().BeTrue();

            result = comparer.Equals(csMethods.Method1, csMethods.Method3);
            result.Should().BeFalse();

            var hashSet = new HashSet<CS.BlockSyntax>(new[] { csMethods.Method1, csMethods.Method2, csMethods.Method3 }, comparer);
            hashSet.Should().HaveCount(2);
            hashSet.Should().Contain(csMethods.Method1);
            hashSet.Should().NotContain(csMethods.Method4);
        }

        [TestMethod]
        public void EqualityComparer_List_CS()
        {
            var comparer = new CSharpSyntaxNodeEqualityComparer<CS.StatementSyntax>();

            var result = comparer.Equals(csMethods.Method1.Statements, csMethods.Method2.Statements);
            result.Should().BeTrue();

            result = comparer.Equals(csMethods.Method1.Statements, csMethods.Method3.Statements);
            result.Should().BeFalse();

            var hashSet = new HashSet<SyntaxList<CS.StatementSyntax>>(new[] { csMethods.Method1.Statements, csMethods.Method2.Statements, csMethods.Method3.Statements }, comparer);
            hashSet.Should().HaveCount(2);
            hashSet.Should().Contain(csMethods.Method1.Statements);
            hashSet.Should().NotContain(csMethods.Method4.Statements);
        }

        [TestMethod]
        public void AreEquivalent_Node_VB()
        {
            var result = VisualBasicEquivalenceChecker.AreEquivalent(vbMethods.Method1.First(), vbMethods.Method2.First());
            result.Should().BeTrue();

            result = VisualBasicEquivalenceChecker.AreEquivalent(vbMethods.Method1.First(), vbMethods.Method3.First());
            result.Should().BeFalse();
        }

        [TestMethod]
        public void AreEquivalent_List_VB()
        {
            var result = VisualBasicEquivalenceChecker.AreEquivalent(vbMethods.Method1, vbMethods.Method2);
            result.Should().BeTrue();

            result = VisualBasicEquivalenceChecker.AreEquivalent(vbMethods.Method1, vbMethods.Method3);
            result.Should().BeFalse();
        }

        [TestMethod]
        public void EqualityComparer_Node_VB()
        {
            var comparer = new VisualBasicSyntaxNodeEqualityComparer<VB.StatementSyntax>();

            var result = comparer.Equals(vbMethods.Method1.First(), vbMethods.Method2.First());
            result.Should().BeTrue();

            result = comparer.Equals(vbMethods.Method1.First(), vbMethods.Method3.First());
            result.Should().BeFalse();

            var hashSet = new HashSet<VB.StatementSyntax>(new[] { vbMethods.Method1.First(), vbMethods.Method2.First(), vbMethods.Method3.First() }, comparer);
            hashSet.Should().HaveCount(2);
            hashSet.Should().Contain(vbMethods.Method1.First());
            hashSet.Should().NotContain(vbMethods.Method4.First());
        }

        [TestMethod]
        public void EqualityComparer_List_VB()
        {
            var comparer = new VisualBasicSyntaxNodeEqualityComparer<VB.StatementSyntax>();

            var result = comparer.Equals(vbMethods.Method1, vbMethods.Method2);
            result.Should().BeTrue();

            result = comparer.Equals(vbMethods.Method1, vbMethods.Method3);
            result.Should().BeFalse();

            var hashSet = new HashSet<SyntaxList<VB.StatementSyntax>>(new[] { vbMethods.Method1, vbMethods.Method2, vbMethods.Method3 }, comparer);
            hashSet.Should().HaveCount(2);
            hashSet.Should().Contain(vbMethods.Method1);
            hashSet.Should().NotContain(vbMethods.Method4);
        }

        [TestMethod]
        public void EqualityComparer_Node_CrossLanguage() =>
            EquivalenceChecker.AreEquivalent(vbMethods.Method1.First(), csMethods.Method1, null).Should().BeFalse();

        private class CSharpMethods
        {
            public readonly CS.BlockSyntax Method1;
            public readonly CS.BlockSyntax Method2;
            public readonly CS.BlockSyntax Method3;
            public readonly CS.BlockSyntax Method4;

            public CSharpMethods(string source)
            {
                var methods = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().OfType<CS.MethodDeclarationSyntax>().ToArray();
                Method1 = methods.Single(m => m.Identifier.ValueText == "Method1").Body;
                Method2 = methods.Single(m => m.Identifier.ValueText == "Method2").Body;
                Method3 = methods.Single(m => m.Identifier.ValueText == "Method3").Body;
                Method4 = methods.Single(m => m.Identifier.ValueText == "Method4").Body;
            }
        }

        private class VisualBasicMethods
        {
            public readonly SyntaxList<VB.StatementSyntax> Method1;
            public readonly SyntaxList<VB.StatementSyntax> Method2;
            public readonly SyntaxList<VB.StatementSyntax> Method3;
            public readonly SyntaxList<VB.StatementSyntax> Method4;

            public VisualBasicMethods(string source)
            {
                var methods = Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(source).GetRoot().DescendantNodes().OfType<VB.MethodBlockSyntax>().ToArray();
                Method1 = methods.Single(m => m.GetIdentifierText() == "Method1").Statements;
                Method2 = methods.Single(m => m.GetIdentifierText() == "Method2").Statements;
                Method3 = methods.Single(m => m.GetIdentifierText() == "Method3").Statements;
                Method4 = methods.Single(m => m.GetIdentifierText() == "Method4").Statements;
            }
        }
    }
}
