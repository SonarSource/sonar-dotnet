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

using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.VisualBasic.Core.Syntax.Utilities;
using CodeAnalysisAccessibility = Microsoft.CodeAnalysis.Accessibility; // This is needed because there is an Accessibility namespace in the windows forms binaries.

namespace SonarAnalyzer.Test.Helpers;

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
        var ret = sut.GetRemovableFieldLikeDeclarations(new[] { SyntaxKind.FieldDeclaration }.ToHashSet(), CodeAnalysisAccessibility.Public);
        ret.Should().HaveCount(5);
        ret.Select(x => x.Symbol.Name).Should().BeEquivalentTo("CompliantA", "CompliantB", "CompliantC", "FieldInNestedClass", "FieldInNestedStruct");
    }

    [TestMethod]
    public void GetRemovableDeclarations_VB()
    {
        const string code = @"
Public Class Base

    Public Overridable Sub OverridableMethod_NotRemovable()
    End Sub

End Class

Public Interface IBase

    Sub InterfaceMethod_NotRemovable()

End Interface

Public Class Sample
    Inherits Base
    Implements IBase

    Public Sub RemovableMethod()
    End Sub

    Public Overrides Sub OverridableMethod_NotRemovable()
    End Sub

    Public Sub InterfaceMethod_NotRemovable() Implements IBase.InterfaceMethod_NotRemovable
    End Sub

    <System.ComponentModel.Browsable(False)>
    Public Sub WithAttributes_NotRemovable()
    End Sub

    Public Overridable Sub Overridable_NotRemovable()
    End Sub

    Public Interface INestedInterface

        Sub NestedInterfaceMethod_NotRemovable()

    End Interface

    Public MustInherit Class AbstractType

        Public MustOverride Sub Abstract_NotRemovable()

    End Class

End Class";
        var sut = CreateCollector(code);
        var ret = sut.GetRemovableDeclarations(new[] { SyntaxKind.SubBlock, SyntaxKind.SubStatement }.ToHashSet(), CodeAnalysisAccessibility.Public);
        ret.Should().ContainSingle();
        ret.Single().Symbol.Name.Should().Be("RemovableMethod");
    }

    [TestMethod]
    public void IsRemovable_Null_ReturnsFalse() =>
        VisualBasicRemovableDeclarationCollector.IsRemovable(null, CodeAnalysisAccessibility.Public).Should().BeFalse();

    private static VisualBasicRemovableDeclarationCollector CreateCollector(string code)
    {
        var (tree, semanticModel) = TestCompiler.CompileVB(code, MetadataReferenceFacade.SystemComponentModelPrimitives.ToArray());
        var type = tree.GetRoot().DescendantNodes().OfType<ClassBlockSyntax>().Single(x => x.ClassStatement.Identifier.ValueText == "Sample");
        return new VisualBasicRemovableDeclarationCollector(semanticModel.GetDeclaredSymbol(type), semanticModel.Compilation);
    }
}
