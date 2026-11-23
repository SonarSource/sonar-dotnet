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

using System.Numerics;

using SonarAnalyzer.Core.Facade;

namespace SonarAnalyzer.CSharp.Core.Facade.Implementation.Test;

[TestClass]
public class CSharpSyntaxKindFacadeTest
{
    private const string UnionCode = "public union SymbolMember(int, string);";

    [TestMethod]
    public void TypeDeclarations_None_Throws() =>
        FluentActions.Invoking(() => CSharpSyntaxKindFacade.Instance.TypeDeclarations(TypeDeclarationKinds.None)).Should().Throw<ArgumentException>();

    [TestMethod]
    public void TypeDeclarations_UndefinedKinds_Throws()
    {
        FluentActions.Invoking(() => CSharpSyntaxKindFacade.Instance.TypeDeclarations((TypeDeclarationKinds)(1 << 20))).Should().Throw<ArgumentException>();
        FluentActions.Invoking(() => CSharpSyntaxKindFacade.Instance.TypeDeclarations(TypeDeclarationKinds.Class | (TypeDeclarationKinds)(1 << 20))).Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void TypeDeclarations_EveryFlagMapsToDistinctKind()
    {
        var single = Enum.GetValues<TypeDeclarationKinds>().Where(x => x != TypeDeclarationKinds.None && BitOperations.PopCount((uint)x) == 1).ToArray();

        single.Should().HaveCount(10);
        single.SelectMany(CSharpSyntaxKindFacade.Instance.TypeDeclarations).Should().OnlyHaveUniqueItems().And.HaveCount(single.Length - 1);    // Module has no C# kind
        CSharpSyntaxKindFacade.Instance.TypeDeclarations(TypeDeclarationKinds.All).Should().BeEquivalentTo(
            [
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKindEx.RecordDeclaration,
                SyntaxKindEx.RecordStructDeclaration,
                SyntaxKindEx.UnionDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKind.EnumDeclaration,
                SyntaxKind.DelegateDeclaration,
                SyntaxKindEx.ExtensionBlockDeclaration,
            ]);
    }

    [TestMethod]
    [DataRow(TypeDeclarationKinds.ClassLike, SyntaxKind.ClassDeclaration, SyntaxKindEx.RecordDeclaration)]
    [DataRow(TypeDeclarationKinds.StructLike, SyntaxKind.StructDeclaration, SyntaxKindEx.RecordStructDeclaration, SyntaxKindEx.UnionDeclaration)]
    [DataRow(TypeDeclarationKinds.ClassOrStructLike, SyntaxKind.ClassDeclaration, SyntaxKindEx.RecordDeclaration, SyntaxKind.StructDeclaration, SyntaxKindEx.RecordStructDeclaration, SyntaxKindEx.UnionDeclaration)]
    [DataRow(TypeDeclarationKinds.Class | TypeDeclarationKinds.Interface, SyntaxKind.ClassDeclaration, SyntaxKind.InterfaceDeclaration)]
    public void TypeDeclarations_Composites(TypeDeclarationKinds kinds, params SyntaxKind[] expected) =>
        CSharpSyntaxKindFacade.Instance.TypeDeclarations(kinds).Should().BeEquivalentTo(expected);

    [TestMethod]
    public void TypeDeclarations_ReturnsSameInstance() =>
        CSharpSyntaxKindFacade.Instance.TypeDeclarations(TypeDeclarationKinds.ClassLike).Should().BeSameAs(CSharpSyntaxKindFacade.Instance.TypeDeclarations(TypeDeclarationKinds.ClassLike));

    [TestMethod]
    public void TypeDeclaration_IncludesUnions() =>
        CSharpSyntaxKindFacade.Instance.TypeDeclaration.Should().Contain(SyntaxKindEx.UnionDeclaration);

    [TestMethod]
    public void UnionSymbol_IsStruct()
    {
        var symbol = new SnippetCompiler(UnionCode, ignoreErrors: true, AnalyzerLanguage.CSharp, parseOptions: new CSharpParseOptions(LanguageVersionEx.Preview))
            .DeclaredSymbol<INamedTypeSymbol>("SymbolMember");

        symbol.TypeKind.Should().Be(TypeKind.Struct);
    }
}
