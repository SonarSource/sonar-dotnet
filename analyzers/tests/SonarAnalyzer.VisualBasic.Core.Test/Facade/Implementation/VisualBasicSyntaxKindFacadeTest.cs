/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.Core.Facade;

namespace SonarAnalyzer.VisualBasic.Core.Facade.Implementation.Test;

[TestClass]
public class VisualBasicSyntaxKindFacadeTest
{
    private readonly VisualBasicSyntaxKindFacade vb = new();

    [TestMethod]
    public void TypeDeclarations_None_Throws() =>
        FluentActions.Invoking(() => vb.TypeDeclarations(TypeDeclarationKinds.None)).Should().Throw<ArgumentException>();

    [TestMethod]
    public void TypeDeclarations_UndefinedKinds_Throws()
    {
        FluentActions.Invoking(() => vb.TypeDeclarations((TypeDeclarationKinds)(1 << 20))).Should().Throw<ArgumentException>();
        FluentActions.Invoking(() => vb.TypeDeclarations(TypeDeclarationKinds.Class | (TypeDeclarationKinds)(1 << 20))).Should().Throw<ArgumentException>();
    }

    [TestMethod]
    [DataRow(TypeDeclarationKinds.Class, SyntaxKind.ClassBlock)]
    [DataRow(TypeDeclarationKinds.Module, SyntaxKind.ModuleBlock)]
    [DataRow(TypeDeclarationKinds.Struct, SyntaxKind.StructureBlock)]
    [DataRow(TypeDeclarationKinds.Interface, SyntaxKind.InterfaceBlock)]
    [DataRow(TypeDeclarationKinds.Enum, SyntaxKind.EnumBlock)]
    [DataRow(TypeDeclarationKinds.Delegate, SyntaxKind.DelegateSubStatement, SyntaxKind.DelegateFunctionStatement)]
    [DataRow(TypeDeclarationKinds.Record)]
    [DataRow(TypeDeclarationKinds.RecordStruct)]
    [DataRow(TypeDeclarationKinds.Union)]
    [DataRow(TypeDeclarationKinds.Extension)]
    [DataRow(TypeDeclarationKinds.ClassOrStructLike, SyntaxKind.ClassBlock, SyntaxKind.StructureBlock)]
    [DataRow(TypeDeclarationKinds.Class | TypeDeclarationKinds.Module, SyntaxKind.ClassBlock, SyntaxKind.ModuleBlock)]
    public void TypeDeclarations(TypeDeclarationKinds kinds, params SyntaxKind[] expected) =>
        vb.TypeDeclarations(kinds).Should().BeEquivalentTo(expected);

    [TestMethod]
    public void SharedKindSets()
    {
        vb.ClassAndRecordDeclarations.Should().BeEquivalentTo([SyntaxKind.ClassBlock]);
        vb.ClassAndModuleDeclarations.Should().BeEquivalentTo([SyntaxKind.ClassBlock, SyntaxKind.ModuleBlock]);
        vb.TypeDeclaration.Should().BeEquivalentTo([SyntaxKind.ClassBlock, SyntaxKind.StructureBlock, SyntaxKind.InterfaceBlock, SyntaxKind.EnumBlock]);
    }

    [TestMethod]
    public void IsTypeDeclaration()
    {
        var root = SyntaxFactory.ParseCompilationUnit("Module M\nEnd Module\nClass C\nEnd Class");
        var module = root.Members[0];
        var cls = root.Members[1];

        vb.IsTypeDeclaration(module, TypeDeclarationKinds.Module).Should().BeTrue();
        vb.IsTypeDeclaration(module, TypeDeclarationKinds.Class).Should().BeFalse();
        vb.IsTypeDeclaration(cls, TypeDeclarationKinds.ClassOrStructLike).Should().BeTrue();
        vb.IsTypeDeclaration(null, TypeDeclarationKinds.All).Should().BeFalse();
    }
}
