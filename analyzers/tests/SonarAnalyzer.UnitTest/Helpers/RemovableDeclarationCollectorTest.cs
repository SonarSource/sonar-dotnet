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
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class RemovableDeclarationCollectorTest
    {
        [TestMethod]
        public void GetRemovableFieldLikeDeclarations_SearchesInNestedTypes_VB()
        {
            const string code = @"
Public Class Sample

    Public CompliantA, CompliantB As Integer
    Public CompliantC As Integer

    Private Class NestedClass

        Public FieldInNestedClass As Integer

    End Class

    Private Structure NestedStruct

        Public FieldInNestedStruct As Integer

    End Structure

End Class";
            var sut = CreateCollector(code);
            var ret = sut.GetRemovableFieldLikeDeclarations(new[] { SyntaxKind.FieldDeclaration }.ToHashSet(), Accessibility.Public);
            ret.Should().HaveCount(5);
            ret.Select(x => x.Symbol.Name).Should().BeEquivalentTo("CompliantA", "CompliantB", "CompliantC", "FieldInNestedClass", "FieldInNestedStruct");
        }

        private VisualBasicRemovableDeclarationCollector CreateCollector(string code)
        {
            var (tree, semanticModel) = TestHelper.Compile(code, false);
            var type = tree.GetRoot().DescendantNodes().OfType<ClassBlockSyntax>().Single(x => x.ClassStatement.Identifier.ValueText == "Sample");
            return new VisualBasicRemovableDeclarationCollector(semanticModel.GetDeclaredSymbol(type), semanticModel.Compilation);
        }
    }
}
